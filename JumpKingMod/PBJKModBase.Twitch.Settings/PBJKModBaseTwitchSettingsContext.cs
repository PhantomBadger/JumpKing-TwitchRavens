using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Twitch.Settings
{
    public abstract class PBJKModBaseTwitchSettingsContext
    {
        public const string SettingsFileName = "PBJK/PBJKModBase.Twitch.settings";

        // Twitch
        public const string ChatListenerTwitchAccountNameKey = "ChatListenerTwitchAccountName";
        public const string OAuthKey = "OAuth";

        public static ConcurrentDictionary<string, string> GetDefaultSettings()
        {
            var dict = new ConcurrentDictionary<string, string>();
            dict[ChatListenerTwitchAccountNameKey] = "";
            dict[OAuthKey] = "";
            return dict;
        }
    }
}
