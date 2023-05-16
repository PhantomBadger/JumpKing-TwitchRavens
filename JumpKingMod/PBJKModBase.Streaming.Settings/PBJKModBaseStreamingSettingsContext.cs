using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Streaming.Settings
{
    public abstract class PBJKModBaseStreamingSettingsContext
    {
        public const string SettingsFileName = "PBJKModBase.Streaming.settings";

        public const string SelectedStreamingPlatformKey = "SelectedStreamingPlatform";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch.ToString() }
            };
        }
    }
}
