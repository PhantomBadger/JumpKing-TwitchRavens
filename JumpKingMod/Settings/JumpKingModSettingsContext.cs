using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace Settings
{
    public abstract class JumpKingModSettingsContext
    {
        public const string SettingsFileName = "JumpKingMod.settings";

        // Twitch
        public const string ChatListenerTwitchAccountNameKey = "ChatListenerTwitchAccountName";
        public const string OAuthKey = "OAuth";

        // Twitch Relay
        public const string TwitchRelayEnabledKey = "TwitchRelayEnabled";

        // Ravens
        public const string RavensEnabledKey = "RavensEnabled";
        public const string RavenTriggerTypeKey = "RavenTriggerType";
        public const string RavenChannelPointRewardIDKey = "RavenChannelPointRewardID";

        // Free Fly
        public const string FreeFlyEnabledKey = "FreeFlyEnabled";
        public const string FreeFlyToggleKeyKey = "FreeFlyToggleKey";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { ChatListenerTwitchAccountNameKey, "" },
                { OAuthKey, "" },
                { TwitchRelayEnabledKey, false.ToString() },
                { RavensEnabledKey, true.ToString() },
                { RavenTriggerTypeKey, RavenTriggerTypes.ChatMessage.ToString() },
                { RavenChannelPointRewardIDKey, "" },
                { FreeFlyEnabledKey, false.ToString() },
                { FreeFlyToggleKeyKey, Keys.F1.ToString() }
            };
        }
    }
}
