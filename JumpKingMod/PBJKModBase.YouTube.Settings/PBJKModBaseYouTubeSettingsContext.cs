using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.YouTube.Settings
{
    public abstract class PBJKModBaseYouTubeSettingsContext
    {
        public const string SettingsFileName = "PBJK/PBJKModBase.YouTube.settings";

        // YouTube
        public const string YouTubeApiKeyKey = "YouTubeAPIKey";
        public const string YouTubeChannelNameKey = "YouTubeChannelName";
        public const string YouTubeConnectKeyKey = "YouTubeConnectKey";

        public static ConcurrentDictionary<string, string> GetDefaultSettings()
        {
            var dict = new ConcurrentDictionary<string, string>();
            dict[YouTubeApiKeyKey] = "";
            dict[YouTubeChannelNameKey] = "";
            dict[YouTubeConnectKeyKey] = Keys.F9.ToString();
            return dict;
        }
    }
}
