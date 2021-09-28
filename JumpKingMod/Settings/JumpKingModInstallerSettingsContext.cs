using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    public abstract class JumpKingModInstallerSettingsContext
    {
        public const string SettingsFileName = "JumpKingModInstaller.settings";
        public const string GameDirectoryKey = "GameDirectory";
        public const string ModDirectoryKey = "ModDirectory";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { GameDirectoryKey, "" },
                { ModDirectoryKey, "" }
            };
        }
    }
}
