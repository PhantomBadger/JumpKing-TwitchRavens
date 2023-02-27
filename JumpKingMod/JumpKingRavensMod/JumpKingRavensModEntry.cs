using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using JumpKingRavensMod.API;
using JumpKingRavensMod.Entities;
using JumpKingRavensMod.Entities.Raven;
using JumpKingRavensMod.Entities.Raven.Triggers;
using JumpKingRavensMod.Patching;
using JumpKingRavensMod.Settings;
using JumpKingRavensMod.Twitch;
using JumpKingRavensMod.YouTube;
using Logging;
using Logging.API;
using Settings;

namespace JumpKingRavensMod
{
    public class JumpKingRavensModEntry
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
                var harmony = new Harmony("com.phantombadger.jumpkingmod");
                harmony.PatchAll();

                Logger.Information($"====================================");
                Logger.Information($"Thanks for using PhantomBadger's Jump King Chat Raven Mod!");
                Logger.Information($"You don't need to credit me for use of the mod, but a shoutout");
                Logger.Information($"would be much appreciated!");
                Logger.Information($"====================================");
                Logger.Information($"Send me a tweet if you use my mod, I'd love to check out the VOD!");
                Logger.Information($"====================================");
                Logger.Information($"Twitter - @PhantomBadger_");
                Logger.Information($"Twitch - PhantomBadger");
                Logger.Information($"====================================");

                // Load Settings
                var userSettings = new UserSettings(JumpKingModSettingsContext.SettingsFileName, JumpKingModSettingsContext.GetDefaultSettings(), Logger);
                ModContentManager.LoadContent(Logger);

                // Set up observer
                var gameStateObserver = new GameStateObserverManualPatch(Logger);
                gameStateObserver.SetUpManualPatch(harmony);

                // Make our Mod Entity Manager and patch it
                var modEntityManager = new ModEntityManager();
                IManualPatch modEntityManagerPatch = new ModEntityManagerManualPatch(modEntityManager);
                modEntityManagerPatch.SetUpManualPatch(harmony);

                // Twitch Chat Client
                var twitchClientFactory = new TwitchClientFactory(userSettings, Logger);

                // YouTube Chat Client
                var youtubeChatClientFactory = new YouTubeChatClientFactory(userSettings, Logger);

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

                        // Get the Streaming Platform being used
                        AvailableStreamingPlatforms selectedStreamingPlatform = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch);
                        
                        // Twitch Chat Relay - DEPRECATED
                        //if (selectedStreamingPlatform == AvailableStreamingPlatforms.Twitch)
                        //{
                        //    bool relayEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.TwitchRelayEnabledKey, false);
                        //    if (relayEnabled)
                        //    {
                        //        Logger.Information($"Initialising Twitch Chat UI Display");
                        //        var relay = new TwitchChatUIDisplay(twitchClientFactory.GetTwitchClient(), modEntityManager, gameStateObserver, Logger);
                        //    }
                        //}

                        // Ravens
                        bool ravensEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensEnabledKey, false);
                        if (ravensEnabled)
                        {
                            bool easterEggEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenEasterEggEnabledKey, true);

                            var ravenTriggers = new List<IMessengerRavenTrigger>();

                            // Platform-Specific Raven Setups
                            if (selectedStreamingPlatform == AvailableStreamingPlatforms.Twitch)
                            {
                                // Read in the trigger type from the settings file, create the appropriate trigger, then create the spawning entity
                                // using that trigger
                                TwitchRavenTriggerTypes ravenTriggerType = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenTriggerTypeKey, TwitchRavenTriggerTypes.ChatMessage);

                                switch (ravenTriggerType)
                                {
                                    case TwitchRavenTriggerTypes.ChatMessage:
                                        {
                                            Logger.Information($"Loading Twitch Chat Message Raven Trigger");

                                            // Make the exluded word filter
                                            var filter = new ExcludedTermListFilter(Logger);

                                            // Easter Egg
                                            string twitchName = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                                            if (easterEggEnabled &&
                                                TryGetAndStartEasterEggTrigger(twitchName, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                            {
                                                ravenTriggers.Add(easterEggTrigger);
                                            }

                                            var chatTrigger = new TwitchChatMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, filter, Logger);
                                            ravenTriggers.Add(chatTrigger);
                                            break;
                                        }
                                    case TwitchRavenTriggerTypes.ChannelPointReward:
                                        {
                                            Logger.Information($"Loading Twitch Channel Point Raven Trigger");

                                            // Make the exluded word filter
                                            var filter = new ExcludedTermListFilter(Logger);

                                            // Easter Egg
                                            string twitchName = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                                            if (easterEggEnabled &&
                                                TryGetAndStartEasterEggTrigger(twitchName, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                            {
                                                ravenTriggers.Add(easterEggTrigger);
                                            }

                                            var channelPointTrigger = new TwitchChannelPointMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), userSettings, filter, Logger);
                                            ravenTriggers.Add(channelPointTrigger);
                                            break;
                                        }
                                    case TwitchRavenTriggerTypes.Insult:
                                        {
                                            Logger.Information($"Loading Twitch Insult Raven Trigger");

                                            // Make the Insult Getter
                                            var insultGetter = new RavenInsultFileInsultGetter(Logger);

                                            var fallTrigger = new PlayerFallMessengerRavenTrigger(userSettings, insultGetter, Logger);
                                            fallTrigger.SetUpManualPatch(harmony);
                                            ravenTriggers.Add(fallTrigger);
                                            break;
                                        }
                                    default:
                                        Logger.Error($"Unknown Twitch Raven Trigger Type {ravenTriggerType.ToString()}");
                                        break;
                                }
                            }
                            else if (selectedStreamingPlatform == AvailableStreamingPlatforms.YouTube)
                            {
                                // Read in the trigger type from the settings file, create the appropriate trigger, then create the spawning entity
                                // using that trigger
                                YouTubeRavenTriggerTypes ravenTriggerType = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.YouTubeRavenTriggerTypeKey, YouTubeRavenTriggerTypes.ChatMessage);

                                switch (ravenTriggerType)
                                {
                                    case YouTubeRavenTriggerTypes.ChatMessage:
                                        {
                                            Logger.Information("Loading YouTube Chat Message Raven Trigger");

                                            // Make the exluded word filter
                                            var filter = new ExcludedTermListFilter(Logger);

                                            // Easter Egg
                                            string youTubeChannelId = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.YouTubeChannelNameKey, string.Empty);
                                            if (easterEggEnabled &&
                                                TryGetAndStartEasterEggTrigger(youTubeChannelId, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                            {
                                                ravenTriggers.Add(easterEggTrigger);
                                            }

                                            // Create the YouTube Client and kick off the connection process
                                            YouTubeChatClient youtubeClient = youtubeChatClientFactory.GetYouTubeClient();
                                            IYouTubeClientConnector clientController = new ManualYouTubeClientConnector(youtubeClient, modEntityManager, userSettings, Logger);
                                            clientController.StartAttemptingConnection();

                                            // Create the Trigger
                                            var chatTrigger = new YouTubeChatMessengerRavenTrigger(youtubeClient, filter, Logger);
                                            ravenTriggers.Add(chatTrigger);
                                            break;
                                        }
                                    default:
                                        Logger.Error($"Unknown YouTube Raven Trigger Type {ravenTriggerType.ToString()}");
                                        break;
                                }
                            }

                            if (ravenTriggers != null && ravenTriggers.Count > 0)
                            {
                                Logger.Information($"Initialising Messenger Ravens");
                                MessengerRavenSpawningEntity spawningEntity = new MessengerRavenSpawningEntity(userSettings, modEntityManager, ravenTriggers, isGameLoopRunning: true, Logger);

                                // Initialise the Gun
                                bool gunEnabled = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.GunEnabledKey, false);
                                if (gunEnabled)
                                {
                                    Logger.Information($"Initialising Gun");
                                    GunEntity gunEntity = new GunEntity(spawningEntity, modEntityManager, userSettings, Logger);
                                }

                                // Bind to the events so we can start/stop ravens and invalidate caches
                                gameStateObserver.OnGameLoopRunning += spawningEntity.OnGameLoopStarted;
                                gameStateObserver.OnGameLoopNotRunning += spawningEntity.OnGameLoopStopped;
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

        private static bool TryGetAndStartEasterEggTrigger(string channelId, out FakeMessageEasterEggMessengerRavenTrigger trigger)
        {
            // Easter Egg
            var easterEggTrigger = new FakeMessageEasterEggMessengerRavenTrigger(Logger);
            if (easterEggTrigger.ShouldActivateEasterEggs(channelId))
            {
                easterEggTrigger.StartEasterEggTrigger(channelId);
                trigger = easterEggTrigger;
                return true;
            }
            trigger = null;
            return false;
        }
    }
}
