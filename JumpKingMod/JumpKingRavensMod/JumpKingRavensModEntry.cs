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
using PBJKModBase.YouTube;
using Logging;
using Logging.API;
using PBJKModBase.API;
using PBJKModBase.Entities;
using PBJKModBase.Patching;
using PBJKModBase.Twitch;
using PBJKModBase.Twitch.Settings;
using Settings;
using TwitchLib.Client;
using JumpKing.Mods;
using PBJKModBase.Streaming.Settings;
using PBJKModBase.YouTube.Settings;
using PBJKModBase.YouTube.API;
using System.Diagnostics;

namespace JumpKingRavensMod
{
    [JumpKingMod("PhantomBadger.JumpKingRavens")]
    public static class JumpKingRavensModEntry
    {
        public static ILogger Logger;
        public static Harmony harmony;

        private static IGameStateObserver gameStateObserver;
        private static TwitchClientFactory twitchClientFactory;
        private static YouTubeChatClientFactory youtubeChatClientFactory;

        private static UserSettings ravenModSettings;
        private static UserSettings streamingSettings;
        private static UserSettings twitchSettings;
        private static UserSettings youtubeSettings;

        /// <summary>
        /// Called by Jump King before the level loads
        /// </summary>
        [BeforeLevelLoad]
        public static void BeforeLevelLoad()
        {
#if (DEBUG)
            Debugger.Launch();
#endif

            Logger = ConsoleLogger.Instance;

            try
            {
                PBJKModBase.PBJKModBaseEntry.Init();

                // Your code here
                harmony = new Harmony("com.phantombadger.jumpkingravens");
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

                ravenModSettings = new UserSettings(JumpKingRavensModSettingsContext.SettingsFileName, JumpKingRavensModSettingsContext.GetDefaultSettings(), Logger);
                streamingSettings = new UserSettings(PBJKModBaseStreamingSettingsContext.SettingsFileName, PBJKModBaseStreamingSettingsContext.GetDefaultSettings(), Logger);
                twitchSettings = new UserSettings(PBJKModBaseTwitchSettingsContext.SettingsFileName, PBJKModBaseTwitchSettingsContext.GetDefaultSettings(), Logger);
                youtubeSettings = new UserSettings(PBJKModBaseYouTubeSettingsContext.SettingsFileName, PBJKModBaseYouTubeSettingsContext.GetDefaultSettings(), Logger);
                RavensModContentManager.LoadContent(Logger);

                // Get the observer
                gameStateObserver = GameStateObserverManualPatch.Instance;

                // Twitch Chat Client
                twitchClientFactory = new TwitchClientFactory(twitchSettings, Logger);

                // YouTube Chat Client
                youtubeChatClientFactory = new YouTubeChatClientFactory(youtubeSettings, Logger);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Called by Jump King when the level unloads
        /// </summary>
        [OnLevelUnload]
        public static void OnLevelUnload()
        {
            // Your code here
        }

        /// <summary>
        /// Called by Jump King when the Level Starts
        /// </summary>
        [OnLevelStart]
        public static void OnLevelStart()
        {
            // Get the Streaming Platform being used
            AvailableStreamingPlatforms selectedStreamingPlatform = streamingSettings.GetSettingOrDefault(PBJKModBaseStreamingSettingsContext.SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch);

            // Ravens
            try
            {
                // Free Fly Patch
                bool freeFlyEnabled = ravenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.FreeFlyEnabledKey, false);
                if (freeFlyEnabled)
                {
                    Logger.Information($"Initialising Free Fly Mod");
                    IManualPatch freeFlyPatch = new FreeFlyManualPatch(ravenModSettings, ModEntityManager.Instance, Logger);
                    freeFlyPatch.SetUpManualPatch(harmony);

                    IManualPatch achievementDisablePatch = new AchievementRegisterDisableManualPatch();
                    achievementDisablePatch.SetUpManualPatch(harmony);
                }

                bool ravensEnabled = ravenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensEnabledKey, false);
                if (ravensEnabled)
                {
                    bool easterEggEnabled = ravenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenEasterEggEnabledKey, true);

                    var ravenTriggers = new List<IMessengerRavenTrigger>();

                    // Platform-Specific Raven Setups
                    if (selectedStreamingPlatform == AvailableStreamingPlatforms.Twitch)
                    {
                        // Read in the trigger type from the settings file, create the appropriate trigger, then create the spawning entity
                        // using that trigger
                        TwitchRavenTriggerTypes ravenTriggerType = ravenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenTriggerTypeKey, TwitchRavenTriggerTypes.ChatMessage);

                        switch (ravenTriggerType)
                        {
                            case TwitchRavenTriggerTypes.ChatMessage:
                                {
                                    Logger.Information($"Loading Twitch Chat Message Raven Trigger");

                                    // Make the exluded word filter
                                    var filter = new ExcludedTermListFilter(Logger);

                                    // Easter Egg
                                    string twitchName = ravenModSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                                    if (easterEggEnabled &&
                                        TryGetAndStartEasterEggTrigger(twitchName, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                    {
                                        ravenTriggers.Add(easterEggTrigger);
                                    }

                                    TwitchClient client = twitchClientFactory.GetTwitchClient();
                                    if (client != null)
                                    {
                                        var chatTrigger = new TwitchChatMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), ravenModSettings, filter, Logger);
                                        ravenTriggers.Add(chatTrigger);
                                    }
                                    else
                                    {
                                        Logger.Error($"Unable to create a Twitch Chat Raven Trigger as there is no valid Twitch Client created! Please check your settings!");
                                    }
                                    break;
                                }
                            case TwitchRavenTriggerTypes.ChannelPointReward:
                                {
                                    Logger.Information($"Loading Twitch Channel Point Raven Trigger");

                                    // Make the exluded word filter
                                    var filter = new ExcludedTermListFilter(Logger);

                                    // Easter Egg
                                    string twitchName = ravenModSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                                    if (easterEggEnabled &&
                                        TryGetAndStartEasterEggTrigger(twitchName, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                    {
                                        ravenTriggers.Add(easterEggTrigger);
                                    }

                                    var channelPointTrigger = new TwitchChannelPointMessengerRavenTrigger(twitchClientFactory.GetTwitchClient(), ravenModSettings, filter, Logger);
                                    ravenTriggers.Add(channelPointTrigger);
                                    break;
                                }
                            case TwitchRavenTriggerTypes.Insult:
                                {
                                    Logger.Information($"Loading Twitch Insult Raven Trigger");

                                    // Make the Insult Getter
                                    var insultGetter = new RavenInsultFileInsultGetter(Logger);

                                    var fallTrigger = new PlayerFallMessengerRavenTrigger(ravenModSettings, insultGetter, Logger);
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
                        YouTubeRavenTriggerTypes ravenTriggerType = ravenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.YouTubeRavenTriggerTypeKey, YouTubeRavenTriggerTypes.ChatMessage);

                        switch (ravenTriggerType)
                        {
                            case YouTubeRavenTriggerTypes.ChatMessage:
                                {
                                    Logger.Information("Loading YouTube Chat Message Raven Trigger");

                                    // Make the exluded word filter
                                    var filter = new ExcludedTermListFilter(Logger);

                                    // Easter Egg
                                    string youTubeChannelId = youtubeSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, string.Empty);
                                    if (easterEggEnabled &&
                                        TryGetAndStartEasterEggTrigger(youTubeChannelId, out FakeMessageEasterEggMessengerRavenTrigger easterEggTrigger))
                                    {
                                        ravenTriggers.Add(easterEggTrigger);
                                    }

                                    // Create the YouTube Client and kick off the connection process
                                    YouTubeChatClient youtubeClient = youtubeChatClientFactory.GetYouTubeClient();
                                    IYouTubeClientConnector clientController = new ManualYouTubeClientConnector(youtubeClient, ModEntityManager.Instance, youtubeSettings, Logger);
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
                        MessengerRavenSpawningEntity spawningEntity = new MessengerRavenSpawningEntity(ravenModSettings, ModEntityManager.Instance, ravenTriggers, isGameLoopRunning: true, Logger);

                        // Initialise the Gun
                        bool gunEnabled = ravenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.GunEnabledKey, false);
                        if (gunEnabled)
                        {
                            Logger.Information($"Initialising Gun");
                            GunEntity gunEntity = new GunEntity(spawningEntity, ModEntityManager.Instance, ravenModSettings, Logger);
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
        }

        /// <summary>
        /// Called by Jump King when the Level Ends
        /// </summary>
        [OnLevelEnd]
        public static void OnLevelEnd()
        {
            // Your code here
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
