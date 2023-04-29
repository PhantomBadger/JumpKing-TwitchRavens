using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.YouTube.Settings
{
    public abstract class PBJKModBaseYouTubeSettingsContext
    {
        public const string SettingsFileName = "PBJKModBase.YouTube.settings";

        // YouTube
        public const string YouTubeApiKeyKey = "YouTubeAPIKey";
        public const string YouTubeChannelNameKey = "YouTubeChannelName";
        public const string YouTubeConnectKeyKey = "YouTubeConnectKey";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { YouTubeApiKeyKey, "" },
                { YouTubeChannelNameKey, "" },
                { YouTubeConnectKeyKey, Keys.F9.ToString() },
            };
        }
    }
}
