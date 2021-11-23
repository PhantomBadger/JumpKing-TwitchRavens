using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HarmonyLib;
using JumpKingMod.API;
using JumpKingMod.Entities;
using JumpKingMod.Entities.Raven;
using JumpKingMod.Entities.Raven.Triggers;
using JumpKingMod.Patching;
using JumpKingMod.Settings;
using JumpKingMod.Twitch;
using Logging;
using Logging.API;
using Settings;

namespace JumpKingMod
{
    public class JumpKingModEntry
    {
        public static ILogger Logger;

        /// <summary>
        /// Entry point for our mod, initiates all of the patching
        /// </summary>
        public static void Init()
        {
            Logger = new ConsoleLogger();

            try
            {
                Harmony harmony = new Harmony("com.phantombadger.jumpkingmod");
                harmony.PatchAll();

                Logger.Information($"====================================");
                Logger.Information($"Thanks for using PhantomBadger's Jump King Twitch Raven Mod!");
                Logger.Information($"You don't need to credit me for use of the mod, but a shoutout");
                Logger.Information($"would be much appreciated!");
                Logger.Information($"====================================");
                Logger.Information($"Send me a tweet if you use my mod, I'd love to check out the VOD!");
                Logger.Information($"====================================");
                Logger.Information($"Twitter - @PhantomBadger_");
                Logger.Information($"Twitch - PhantomBadger");
                Logger.Information($"====================================");

                // Load Settings
                UserSettings userSettings = new UserSettings(JumpKingModSettingsContext.SettingsFileName, JumpKingModSettingsContext.GetDefaultSettings(), Logger);

                // Set up observer
                GameStateObserverManualPatch gameStateObserver = new GameStateObserverManualPatch(Logger);
                gameStateObserver.SetUpManualPatch(harmony);

                // Make our Mod Entity Manager and patch it
                ModEntityManager modEntityManager = new ModEntityManager();
                IManualPatch modEntityManagerPatch = new ModEntityManagerManualPatch(modEntityManager);
                modEntityManagerPatch.SetUpManualPatch(harmony);

                // Twitch Chat Client
                TwitchClientFactory twitchClientFactory = new TwitchClientFactory(userSettings, Logger);

                // Free Fly Patch
                bool freeFlyEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.FreeFlyEnabledKey, false);
                if (freeFlyEnabled)
                {
                    Logger.Information($"Initialising Free Fly Mod");
                    IManualPatch freeFlyPatch = new FreeFlyManualPatch(userSettings, modEntityManager, Logger);
                    freeFlyPatch.SetUpManualPatch(harmony);

                    IManualPatch achievementDisablePatch = new AchievementRegisterDisableManualPatch();
                    achievementDisablePatch.SetUpManualPatch(harmony);
                }

                // Run the rest after the game loop has started
                Task.Run(() =>
                {
                    try
                    {
                        while (!gameStateObserver.IsGameLoopRunning())
                        {
                            Task.Delay(100).Wait();
                        }

                        // Twitch Chat Relay
                        bool relayEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.TwitchRelayEnabledKey, false);
                        if (relayEnabled)
                        {
                            Logger.Information($"Initialising Twitch Chat UI Display");
                            TwitchChatUIDisplay relay = new TwitchChatUIDisplay(twitchClientFactory.GetTwitchClient(), modEntityManager, gameStateObserver, Logger);
                        }

                        // Ravens
                        bool ravensEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensEnabledKey, false);
                        if (ravensEnabled)
                        {
                            bool easterEggEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenEasterEggEnabledKey, true);

                            // Read in the trigger type from the settings file, create the appropriate trigger, then create the spawning entity
                            // using that trigger
                            RavenTriggerTypes ravenTriggerType = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenTriggerTypeKey, RavenTriggerTypes.ChatMessage);
                            List<IMessengerRavenTrigger> ravenTriggers = new List<IMessengerRavenTrigger>();
                            switch (ravenTriggerType)
                            {
                                case RavenTriggerTypes.ChatMessage:
                                    {
                                        Logger.Information($"Loading Chat Message Raven Trigger");

                                        // Make the exluded word filter
                                        ExcludedTermListFilter filter = new ExcludedTermListFilter(Logger);

                                        // Easter Egg
                                        if (easterEggEnabled && 
                                            TryGetAndStartEasterEggTrigger(userSettings, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                        {
                                            ravenTriggers.Add(easterEggTrigger);
                                        }

                                        ravenTriggers.Add(new TwitchChatMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, filter, Logger));
                                        break;
                                    }
                                case RavenTriggerTypes.ChannelPointReward:
                                    {
                                        Logger.Information($"Loading Channel Point Raven Trigger");

                                        // Make the exluded word filter
                                        ExcludedTermListFilter filter = new ExcludedTermListFilter(Logger);

                                        // Easter Egg
                                        if (easterEggEnabled && 
                                            TryGetAndStartEasterEggTrigger(userSettings, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                        {
                                            ravenTriggers.Add(easterEggTrigger);
                                        }

                                        ravenTriggers.Add(new TwitchChannelPointMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, filter, Logger));
                                        break;
                                    }
                                case RavenTriggerTypes.Insult:
                                    {
                                        Logger.Information($"Loading Insult Raven Trigger");

                                        // Make the Insult Getter
                                        RavenInsultFileInsultGetter insultGetter = new RavenInsultFileInsultGetter(Logger);

                                        PlayerFallMessengerRavenTrigger fallTrigger = new PlayerFallMessengerRavenTrigger(userSettings, insultGetter, Logger);
                                        fallTrigger.SetUpManualPatch(harmony);
                                        ravenTriggers.Add(fallTrigger);
                                        break;
                                    }
                                default:
                                    Logger.Error($"Unknown Raven Trigger Type {ravenTriggerType.ToString()}");
                                    break;
                            }

                            if (ravenTriggers != null && ravenTriggers.Count > 0)
                            {
                                Logger.Information($"Initialising Messenger Ravens");
                                MessengerRavenSpawningEntity spawningEntity = new MessengerRavenSpawningEntity(userSettings, modEntityManager, ravenTriggers, Logger);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"Error on Post-GameLoop Init: {e.ToString()}");
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Init {e.ToString()}");
            }

            Logger.Information("Init Called!");
        }

        private static bool TryGetAndStartEasterEggTrigger(UserSettings userSettings, out FakeMessageEasterEggMessengerRavenTrigger trigger)
        {
            // Easter Egg
            var easterEggTrigger = new FakeMessageEasterEggMessengerRavenTrigger(Logger);
            string twitchName = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
            if (easterEggTrigger.ShouldActivateEasterEggs(twitchName))
            {
                easterEggTrigger.StartEasterEggTrigger(twitchName);
                trigger = easterEggTrigger;
                return true;
            }
            trigger = null;
            return false;
        }
    }
}
