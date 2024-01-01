using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.PiShock.Settings
{
    public abstract class PBJKModBasePiShockSettingsContext
    {
        public const string SettingsFileName = "PBJKModBase.PiShock.settings";

        // PiShock
        public const string UsernameKey = "APIUsername";
        public const string APIKeyKey = "APIKey";
        public const string ShareCodeKey = "APIShareCode";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { UsernameKey, string.Empty },
                { APIKeyKey, string.Empty },
                { ShareCodeKey, string.Empty },
            };
        }
    }
}
