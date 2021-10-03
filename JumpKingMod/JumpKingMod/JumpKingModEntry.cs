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
                        // Read in the trigger type from the settings file, create the appropriate trigger, then create the spawning entity
                        // using that trigger
                        RavenTriggerTypes ravenTriggerType = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenTriggerTypeKey, RavenTriggerTypes.ChatMessage);
                        IMessengerRavenTrigger ravenTrigger = null;
                        switch (ravenTriggerType)
                        {
                            case RavenTriggerTypes.ChatMessage:
                                Logger.Information($"Loading Chat Message Raven Trigger");
                                ravenTrigger = new TwitchChatMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, Logger); ;
                                break;
                            case RavenTriggerTypes.ChannelPointReward:
                                Logger.Information($"Loading Channel Point Raven Trigger");
                                ravenTrigger = new TwitchChannelPointMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, Logger);
                                break;
                            case RavenTriggerTypes.Insult:
                                Logger.Information($"Loading Insult Raven Trigger");
                                PlayerFallMessengerRavenTrigger fallTrigger = new PlayerFallMessengerRavenTrigger(Logger);
                                fallTrigger.SetUpManualPatch(harmony);
                                ravenTrigger = fallTrigger;
                                break;
                            default:
                                Logger.Error($"Unknown Raven Trigger Type {ravenTriggerType.ToString()}");
                                break;
                        }

                        if (ravenTrigger != null)
                        {
                            Logger.Information($"Initialising Messenger Ravens");
                            MessengerRavenSpawningEntity spawningEntity = new MessengerRavenSpawningEntity(userSettings, modEntityManager, ravenTrigger, Logger);
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Init {e.ToString()}");
            }

            Logger.Information("Init Called!");
        }
    }
}
