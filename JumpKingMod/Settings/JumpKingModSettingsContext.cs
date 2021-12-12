using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Settings
{
    public abstract class JumpKingModSettingsContext
    {
        public const string SettingsFileName = "JumpKingMod.settings";
        public const string ExcludedTermFilePath = "Content/Mods/ExcludedTermList.txt";
        public const string RavenInsultsFilePath = "Content/Mods/RavenInsultsList.txt";
        public const char CommentCharacter = '#';

        // YouTube
        public const string YouTubeApiKeyKey = "YouTubeAPIKey";
        public const string YouTubeChannelNameKey = "YouTubeChannelName";

        // Twitch
        public const string ChatListenerTwitchAccountNameKey = "ChatListenerTwitchAccountName";
        public const string OAuthKey = "OAuth";

        // Twitch Relay
        public const string TwitchRelayEnabledKey = "TwitchRelayEnabled";

        // Ravens
        public const string RavensEnabledKey = "RavensEnabled";
        public const string RavensClearDebugKeyKey = "RavensClearDebugKey";
        public const string RavensToggleDebugKeyKey = "RavensToggleDebugKey";
        public const string RavensSubModeToggleKeyKey = "RavensSubModeToggleKey";
        public const string RavensMaxCountKey = "RavensMaxCount";
        public const string RavenTriggerTypeKey = "RavenTriggerType";
        public const string RavenChannelPointRewardIDKey = "RavenChannelPointRewardID";
        public const string RavenInsultSpawnCountKey = "RavenInsultSpawnCount";
        public const string RavenEasterEggEnabledKey = "RavenEasterEggEnabled";

        // Free Fly
        public const string FreeFlyEnabledKey = "FreeFlyEnabled";
        public const string FreeFlyToggleKeyKey = "FreeFlyToggleKey";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                // YouTube
                { YouTubeApiKeyKey, "" },
                { YouTubeChannelNameKey, "" },

                // Twitch Chat
                { ChatListenerTwitchAccountNameKey, "" },
                { OAuthKey, "" },

                // Chat Display
                { TwitchRelayEnabledKey, false.ToString() },

                // Ravens
                // General
                { RavensEnabledKey, true.ToString() },
                { RavensClearDebugKeyKey, Keys.F2.ToString() },
                { RavensToggleDebugKeyKey, Keys.F3.ToString() },
                { RavensSubModeToggleKeyKey, Keys.F4.ToString() },
                { RavensMaxCountKey, 5.ToString() },
                { RavenTriggerTypeKey, RavenTriggerTypes.ChatMessage.ToString() },
                // Message
                // Channel Point
                { RavenChannelPointRewardIDKey, "" },
                // Insult
                { RavenInsultSpawnCountKey, 3.ToString() },
                // Easter Egg
                { RavenEasterEggEnabledKey, true.ToString() },
                // Free Fly
                { FreeFlyEnabledKey, false.ToString() },
                { FreeFlyToggleKeyKey, Keys.F1.ToString() }
            };
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
