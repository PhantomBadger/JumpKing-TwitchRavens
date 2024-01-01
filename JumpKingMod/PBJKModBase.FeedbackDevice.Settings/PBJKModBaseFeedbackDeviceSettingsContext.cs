using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.FeedbackDevice.Settings
{
    public abstract class PBJKModBaseFeedbackDeviceSettingsContext
    {
        public const string SettingsFileName = "PBJKModBase.FeedbackDevice.settings";

        public const string SelectedFeedbackDeviceKey = "SelectedFeedbackDevice";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { SelectedFeedbackDeviceKey, AvailableFeedbackDevices.None.ToString() }
            };
        }
    }
}
