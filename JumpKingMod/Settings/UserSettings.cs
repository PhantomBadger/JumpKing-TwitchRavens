using Logging.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Settings
{
    /// <summary>
    /// A simple settings file implementation which can read and write JSON settings from a file
    /// </summary>
    public class UserSettings
    {
        private readonly string settingsFilePath;
        private readonly Dictionary<string, string> defaultSettings;
        private readonly ILogger logger;

        private Dictionary<string, string> currentSettings;

        /// <summary>
        /// Ctor for creating a <see cref="UserSettings"/>
        /// </summary>
        /// <param name="settingsFilePath">The file path to store and read the settings from. Can be a partial path or a full path</param>
        /// <param name="defaultSettings">The default settings to populate the file with in the event of a fatal event</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation to use for logging</param>
        public UserSettings(string settingsFilePath, Dictionary<string, string> defaultSettings, ILogger logger)
        {
            this.settingsFilePath = settingsFilePath ?? throw new ArgumentNullException(nameof(settingsFilePath));
            this.defaultSettings = defaultSettings ?? throw new ArgumentNullException(nameof(defaultSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            currentSettings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (File.Exists(settingsFilePath))
            {
                // Load the current settings
                LoadSettings();
            }
            else
            {
                // Save the default settings
                currentSettings = defaultSettings;
                SaveSettings();
            }
        }

        /// <summary>
        /// Get the value of the setting with the given key, or the default value if the key is not present
        /// </summary>
        /// <param name="key">The key for the setting, not case sensitive</param>
        /// <param name="defaultValue">The default value to return if the key isnt present</param>
        /// <remarks>Sets the setting if it is not present</remarks>
        public string GetSettingOrDefault(string key, string defaultValue)
        {
            if (currentSettings.ContainsKey(key))
            {
                return currentSettings[key];
            }
            else
            {
                SetOrCreateSetting(key, defaultValue);
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets an enum variation of the setting, or the default value if the key is not present
        /// or the value is invalid
        /// </summary>
        public TEnum GetSettingOrDefault<TEnum>(string key, TEnum defaultValue)
            where TEnum : struct
        {
            string rawValue = GetSettingOrDefault(key, defaultValue.ToString());
            if (Enum.TryParse<TEnum>(rawValue, out TEnum parsedValue))
            {
                return parsedValue;
            }
            else
            {
                logger.Warning($"Unable to parse found setting for '{key}' into Enum, found value was '{rawValue}', using default of {defaultValue.ToString()} instead");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets an enum variation of the setting, or the default value if the key is not present
        /// or the value is invalid
        /// </summary>
        public Enum GetSettingOrDefault(string key, Enum defaultValue, Type enumType)
        {
            if (currentSettings.ContainsKey(key))
            {
                try
                {
                    return (Enum)Enum.Parse(enumType, currentSettings[key]);
                }
                catch (Exception e)
                {
                    return defaultValue;
                }
            }
            else
            {
                SetOrCreateSetting(key, defaultValue.ToString());
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a bool variation of the setting, or the default value if the key is not present
        /// or the value is invalid
        /// </summary>
        public bool GetSettingOrDefault(string key, bool defaultValue)
        {
            string rawValue = GetSettingOrDefault(key, defaultValue.ToString());
            if (bool.TryParse(rawValue, out bool parsedValue))
            {
                return parsedValue;
            }
            else
            {
                logger.Warning($"Unable to parse found setting for '{key}' into bool, found value was '{rawValue}', using default of {defaultValue.ToString()} instead");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets an int variation of the setting, or the default value if the key is not present
        /// or the value is invalid
        /// </summary>
        public int GetSettingOrDefault(string key, int defaultValue)
        {
            string rawValue = GetSettingOrDefault(key, defaultValue.ToString());
            if (int.TryParse(rawValue, out int parsedValue))
            {
                return parsedValue;
            }
            else
            {
                logger.Warning($"Unable to parse found setting for '{key}' into int, found value was '{rawValue}', using default of {defaultValue.ToString()} instead");
                return defaultValue;
            }
        }

        /// <summary>
        /// Gets a float variation of the setting, or the default value if the key is not present
        /// or the value is invalid
        /// </summary>
        public float GetSettingOrDefault(string key, float defaultValue)
        {
            string rawValue = GetSettingOrDefault(key, defaultValue.ToString(CultureInfo.InvariantCulture));
            if (float.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedValue))
            {
                return parsedValue;
            }
            else
            {
                logger.Warning($"Unable to parse found setting for '{key}' into float, found value was '{rawValue}', using default of {defaultValue.ToString()} instead");
                return defaultValue;
            }
        }

        /// <summary>
        /// Sets the setting, creating it if it isn't already present
        /// </summary>
        /// <param name="key">The key for the setting, not case sensitive</param>
        /// <param name="value">The value to use for the setting</param>
        public void SetOrCreateSetting(string key, string value)
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

        /// <summary>
        /// Save the settings to the file
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                string jsonString = JsonConvert.SerializeObject(currentSettings);
                File.WriteAllText(settingsFilePath, jsonString);
            }
            catch (Exception e)
            {
                logger.Error($"Encountered Exception when saving settings: {e.ToString()}\nSettings have not been saved out!");
            }
        }

        /// <summary>
        /// Load the settings from the file
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                string jsonString = File.ReadAllText(settingsFilePath);
                currentSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
            }
            catch (Exception e)
            {
                logger.Error($"Encountered Exception when loading settings: {e.ToString()}\nSetting them to the defaults instead");
                currentSettings = defaultSettings;
            }
        }
    }
}
