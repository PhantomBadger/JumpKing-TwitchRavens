using JumpKingMod.Install.UI.API;
using Logging.API;
using PBJKModBase.PiShock.Settings;
using JumpKingRavensMod.Install.UI;
using Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI.Settings
{
    /// <summary>
    /// An aggregate class of PiShock Settings
    /// </summary>
    public class PiShockSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

        /// <summary>
        /// The username to use with the PiShock API
        /// </summary>
        public string Username
        {
            get
            {
                return username;
            }
            set
            {
                if (username != value)
                {
                    username = value;
                    RaisePropertyChanged(nameof(Username));
                }
            }
        }
        private string username;

        /// <summary>
        /// The API Key to use with the PiShock API
        /// </summary>
        public string APIKey
        {
            get
            {
                return apiKey;
            }
            set
            {
                if (apiKey != value)
                {
                    apiKey = value;
                    RaisePropertyChanged(nameof(APIKey));
                }
            }
        }
        private string apiKey;

        /// <summary>
        /// The share code to use with the PiShock API
        /// </summary>
        public string ShareCode
        {
            get
            {
                return shareCode;
            }
            set
            {
                if (shareCode != value)
                {
                    shareCode = value;
                    RaisePropertyChanged(nameof(ShareCode));
                }
            }
        }
        private string shareCode;

        /// <summary>
        /// The <see cref="UserSettings"/> object containing the twitch settings
        /// </summary>
        public UserSettings PiShockSettings
        {
            get
            {
                return piShockSettings;
            }
            private set
            {
                piShockSettings = value;
                RaisePropertyChanged(nameof(PiShockSettings));
                RaisePropertyChanged(nameof(ArePiShockSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings piShockSettings;

        /// <summary>
        /// Returns whether the twitch settings are currently populated
        /// </summary>
        public bool ArePiShockSettingsLoaded
        {
            get
            {
                return PiShockSettings != null;
            }
        }

        /// <summary>
        /// Ctor for creating a <see cref="PiShockSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PiShockSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads the PiShock settings from disk, optionally creating the file if it doesnt exist
        /// </summary>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load PiShock settings as provided Game Directory was empty!");
                return false;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBasePiShockSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (PiShockSettings == null)
                {
                    PiShockSettings = new UserSettings(expectedSettingsFilePath, PBJKModBasePiShockSettingsContext.GetDefaultSettings(), logger);
                }

                Username = PiShockSettings.GetSettingOrDefault(PBJKModBasePiShockSettingsContext.UsernameKey, string.Empty);
                APIKey = PiShockSettings.GetSettingOrDefault(PBJKModBasePiShockSettingsContext.APIKeyKey, string.Empty);
                ShareCode = PiShockSettings.GetSettingOrDefault(PBJKModBasePiShockSettingsContext.ShareCodeKey, string.Empty);

                return true;
            }
            else
            {
                logger.Error($"Failed to load PiShock settings as the settings file couldn't be found at '{expectedSettingsFilePath}'");
                return false;
            }
        }

        /// <summary>
        /// Saves the PiShock settings back to disk
        /// </summary>
        /// <param name="gameDirectory"></param>
        public bool SaveSettings(string gameDirectory, string modFolder)
        {
            if (PiShockSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save PiShock settings, either internal settings object ({PiShockSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            PiShockSettings.SetOrCreateSetting(PBJKModBasePiShockSettingsContext.UsernameKey, Username);
            PiShockSettings.SetOrCreateSetting(PBJKModBasePiShockSettingsContext.APIKeyKey, APIKey);
            PiShockSettings.SetOrCreateSetting(PBJKModBasePiShockSettingsContext.ShareCodeKey, ShareCode);
            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return ArePiShockSettingsLoaded;
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
