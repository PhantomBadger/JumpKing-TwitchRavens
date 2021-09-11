using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JumpKingMod.Install.UI
{
    public class UserSettings
    {
        private const string ExpectedFileName = "JumpKingModInstall.settings";
        private Dictionary<string, string> currentSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "InstallDir", "" },
            { "ModDir", "" }
        };

        public UserSettings()
        {
            if (File.Exists(ExpectedFileName))
            {
                // Load the current settings
                LoadSettings();
            }
            else
            {
                // Save the default settings
                currentSettings = DefaultSettings;
                SaveSettings();
            }
        }

        public string GetSetting(string key, string defaultValue)
        {
            if (currentSettings.ContainsKey(key))
            {
                return currentSettings[key];
            }
            else
            {
                SetSetting(key, defaultValue);
                return defaultValue;
            }
        }

        public void SetSetting(string key, string value)
        {
            if (currentSettings.ContainsKey(key))
            {
                currentSettings[key] = value;
            }
            else
            {
                currentSettings.Add(key, value);
            }
            SaveSettings();
        }

        private void SaveSettings()
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(currentSettings);
                File.WriteAllText(ExpectedFileName, jsonString);
            }
            catch (Exception e)
            {
                MessageBox.Show($"Encountered Exception when saving settings: {e.ToString()}");
            }
        }

        private void LoadSettings()
        {
            try
            {
                string jsonString = File.ReadAllText(ExpectedFileName);
                currentSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            }
            catch (Exception e)
            {
                // oh well
                currentSettings = DefaultSettings;
            }
        }
    }
}
