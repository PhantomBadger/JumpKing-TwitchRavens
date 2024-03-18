using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Streaming.Settings
{
    public abstract class PBJKModBaseStreamingSettingsContext
    {
        public const string SettingsFileName = "PBJK/PBJKModBase.Streaming.settings";

        public const string SelectedStreamingPlatformKey = "SelectedStreamingPlatform";

        public static ConcurrentDictionary<string, string> GetDefaultSettings()
        {
            var dict = new ConcurrentDictionary<string, string>();
            dict[SelectedStreamingPlatformKey] = AvailableStreamingPlatforms.Twitch.ToString();
            return dict;
        }
    }
}
