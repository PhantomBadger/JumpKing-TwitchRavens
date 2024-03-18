using Microsoft.Xna.Framework.Input;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace JumpKingRavensMod.Settings
{
    public abstract class JumpKingRavensModSettingsContext
    {
        public const string SettingsFileName = "PBJK/ChatRavens.settings";
        public const string ExcludedTermFilePath = "PBJK/ExcludedTermList.txt";
        public const string RavenInsultsFilePath = "PBJK/RavenInsultsList.txt";
        public const char CommentCharacter = '#';

        // YouTube
        public const string YouTubeRavenTriggerTypeKey = "YouTubeRavenTriggerType";

        // Twitch
        public const string RavenTriggerTypeKey = "RavenTriggerType";

        // Twitch Relay
        public const string TwitchRelayEnabledKey = "TwitchRelayEnabled";

        // Ravens
        public const string RavensEnabledKey = "RavensEnabled";
        public const string RavensClearDebugKeyKey = "RavensClearDebugKey";
        public const string RavensToggleDebugKeyKey = "RavensToggleDebugKey";
        public const string RavensSubModeToggleKeyKey = "RavensSubModeToggleKey";
        public const string RavensMaxCountKey = "RavensMaxCount";
        public const string RavensDisplayTimeInSecondsKey = "RavensDisplayTimeInSeconds";
        public const string RavenChannelPointRewardIDKey = "RavenChannelPointRewardID";
        public const string RavenInsultSpawnCountKey = "RavenInsultSpawnCount";
        public const string RavenEasterEggEnabledKey = "RavenEasterEggEnabled";

        // Free Fly
        public const string FreeFlyEnabledKey = "FreeFlyEnabled";
        public const string FreeFlyToggleKeyKey = "FreeFlyToggleKey";

        // Gun
        public const string GunEnabledKey = "GunEnabled";
        public const string GunToggleKeyKey = "GunToggleKey";

        public static ConcurrentDictionary<string, string> GetDefaultSettings()
        {
;           var dict = new ConcurrentDictionary<string, string>();
            // YouTube
            dict[YouTubeRavenTriggerTypeKey] = YouTubeRavenTriggerTypes.ChatMessage.ToString();

            // Twitch Chat
            dict[RavenTriggerTypeKey] = TwitchRavenTriggerTypes.ChatMessage.ToString();

            // Chat Display
            dict[TwitchRelayEnabledKey] = false.ToString();

            // Ravens
            // General
            dict[RavensEnabledKey] = true.ToString();
            dict[RavensClearDebugKeyKey] = Keys.F2.ToString();
            dict[RavensToggleDebugKeyKey] = Keys.F3.ToString();
            dict[RavensSubModeToggleKeyKey] = Keys.F4.ToString();
            dict[RavensMaxCountKey] = 5.ToString();

            // Message
            // Channel Point
            dict[RavenChannelPointRewardIDKey] = "";
            // Insult
            dict[RavenInsultSpawnCountKey] = 3.ToString();
            // Easter Egg
            dict[RavenEasterEggEnabledKey] = true.ToString();
            // Free Fly
            dict[FreeFlyEnabledKey] = false.ToString();
            dict[FreeFlyToggleKeyKey] = Keys.F1.ToString();
            // Gun
            dict[GunEnabledKey] = false.ToString();
            dict[GunToggleKeyKey] = Keys.F8.ToString();

            return dict;
        }

        public static string[] GetDefaultInsults()
        {
            return new string[]
            {
                "lmao",
                "OMEGADOWN",
                "LOL",
                "Fall King",
                "idiot",
                "lmfao",
                "back to the old man",
                "LETS GOOOO",
                "stop playing",
                "you suck",
                "KEKW",
                ":(",
                "YEP FALL",
                "Deez Nuts",
                "Get got",
                "kek wow",
                "THIS DUDE",
                "ravenJAM",
                "Hi YouTube",
                "L",
                "It's like you don't even try",
            };
        }
    }
}
