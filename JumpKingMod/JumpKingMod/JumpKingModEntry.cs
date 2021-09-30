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

                // Set up observer
                GameStateObserverManualPatch gameStateObserver = new GameStateObserverManualPatch(Logger);
                gameStateObserver.SetUpManualPatch(harmony);

                // Make our Mod Entity Manager and patch it
                ModEntityManager modEntityManager = new ModEntityManager();
                IManualPatch modEntityManagerPatch = new ModEntityManagerManualPatch(modEntityManager);
                modEntityManagerPatch.SetUpManualPatch(harmony);

                // Free Fly Patch
                IManualPatch freeFlyPatch = new FreeFlyManualPatch(modEntityManager, Logger);
                freeFlyPatch.SetUpManualPatch(harmony);

                UserSettings userSettings = new UserSettings(JumpKingModSettingsContext.SettingsFileName, JumpKingModSettingsContext.GetDefaultSettings(), Logger);

                // Twitch Chat Client
                TwitchClientFactory twitchClientFactory = new TwitchClientFactory(userSettings, Logger);

                // Twitch Chat Relay
                string relayEnabledRaw = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.TwitchRelayEnabledKey, false.ToString());
                if (bool.TryParse(relayEnabledRaw, out bool relayEnabled) && relayEnabled)
                {
                    TwitchChatUIDisplay relay = new TwitchChatUIDisplay(modEntityManager, gameStateObserver, Logger);
                }
                else
                {
                    Logger.Error($"Failed to parse '{JumpKingModSettingsContext.TwitchRelayEnabledKey}' from the settings file");
                }

                Task.Run(() =>
                {
                    while (!gameStateObserver.IsGameLoopRunning())
                    {
                        Task.Delay(100).Wait();
                    }

                    // Ravens
                    string ravensEnabledRaw = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensEnabledKey, false.ToString());
                    if (bool.TryParse(ravensEnabledRaw, out bool ravensEnabled) && ravensEnabled)
                    {
                        // Read in the trigger type from the settings file, create the appropriate trigger, then create the spawning entity
                        // using that trigger
                        string ravenTriggerTypeRaw = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenTriggerTypeKey, RavenTriggerTypes.ChatMessage.ToString());
                        if (Enum.TryParse(ravenTriggerTypeRaw, out RavenTriggerTypes parsedTriggerType))
                        {
                            IMessengerRavenTrigger ravenTrigger = null;
                            switch (parsedTriggerType)
                            {
                                case RavenTriggerTypes.ChatMessage:
                                    ravenTrigger = new TwitchChatMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, Logger); ;
                                    break;
                                case RavenTriggerTypes.ChannelPointReward:
                                    ravenTrigger = new TwitchChannelPointMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, Logger);
                                    break;
                                case RavenTriggerTypes.Insult:
                                    PlayerFallMessengerRavenTrigger fallTrigger = new PlayerFallMessengerRavenTrigger(Logger);
                                    fallTrigger.SetUpManualPatch(harmony);
                                    ravenTrigger = fallTrigger;
                                    break;
                                default:
                                    Logger.Error($"Unknown Raven Trigger Type {parsedTriggerType.ToString()}");
                                    break;
                            }

                            if (ravenTrigger != null)
                            {
                                MessengerRavenSpawningEntity spawningEntity = new MessengerRavenSpawningEntity(modEntityManager, ravenTrigger, Logger);
                            }
                        }
                    }
                    else
                    {
                        Logger.Error($"Failed to parse '{JumpKingModSettingsContext.RavensEnabledKey}' from the settings file");
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
