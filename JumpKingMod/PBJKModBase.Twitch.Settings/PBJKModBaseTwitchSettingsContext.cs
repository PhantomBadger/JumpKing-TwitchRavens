using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Twitch.Settings
{
    public abstract class PBJKModBaseTwitchSettingsContext
    {
        public const string SettingsFileName = "PBJKModBase.Twitch.settings";
        public const string ExcludedTermFilePath = "Content/Mods/ExcludedTermList.txt";
        public const char CommentCharacter = '#';

        // Twitch
        public const string ChatListenerTwitchAccountNameKey = "ChatListenerTwitchAccountName";
        public const string OAuthKey = "OAuth";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { ChatListenerTwitchAccountNameKey, "" },
                { OAuthKey, "" },
            };
        }
    }
}
