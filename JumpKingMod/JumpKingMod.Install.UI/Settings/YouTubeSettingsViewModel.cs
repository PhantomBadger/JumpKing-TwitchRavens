using JumpKingMod.Install.UI.API;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.Twitch.Settings;
using PBJKModBase.YouTube.Settings;
using Settings;
using System;
using System.ComponentModel;
using System.IO;

namespace JumpKingRavensMod.Install.UI
{
    /// <summary>
    /// An aggregatce alss of YouTube Settings
    /// </summary>
    public class YouTubeSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

        /// <summary>
        /// The name of the youtube account to use
        /// </summary>
        public string YouTubeAccountName
        {
            get
            {
                return youTubeAccountName;
            }
            set
            {
                if (youTubeAccountName != value)
                {
                    youTubeAccountName = value;
                    RaisePropertyChanged(nameof(YouTubeAccountName));
                }
            }
        }
        private string youTubeAccountName;

        /// <summary>
        /// The API key to use for YouTube
        /// </summary>
        public string YouTubeAPIKey
        {
            get
            {
                return youTubeAPIKey;
            }
            set
            {
                if (youTubeAPIKey != value)
                {
                    youTubeAPIKey = value;
                    RaisePropertyChanged(nameof(YouTubeAPIKey));
                }
            }
        }
        private string youTubeAPIKey;

        /// <summary>
        /// The key to press to connect/disconnect from youtube
        /// </summary>
        public Keys ConnectKey
        {
            get
            {
                return connectKey;
            }
            set
            {
                if (connectKey != value)
                {
                    connectKey = value;
                    RaisePropertyChanged(nameof(ConnectKey));
                }
            }
        }
        private Keys connectKey;

        /// <summary>
        /// The <see cref="UserSettings"/> object containing the youtube settings
        /// </summary>
        public UserSettings YouTubeBaseSettings
        {
            get
            {
                return youTubeBaseSettings;
            }
            private set
            {
                youTubeBaseSettings = value;
                RaisePropertyChanged(nameof(YouTubeBaseSettings));
                RaisePropertyChanged(nameof(AreYouTubeSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings youTubeBaseSettings;

        /// <summary>
        /// Returns whether the youtube settings are currently populated
        /// </summary>
        public bool AreYouTubeSettingsLoaded
        {
            get
            {
                return YouTubeBaseSettings != null;
            }
        }

        /// <summary>
        /// Ctor for creating a <see cref="YouTubeSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <exception cref="ArgumentNullException"></exception>
        public YouTubeSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads the youtube settings from disk, optionally creating the file if it doesnt exist
        /// </summary>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load YouTube settings as provided Game Directory was empty!");
                return false;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBaseYouTubeSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (YouTubeBaseSettings == null)
                {
                    YouTubeBaseSettings = new UserSettings(expectedSettingsFilePath, PBJKModBaseYouTubeSettingsContext.GetDefaultSettings(), logger);
                }

                YouTubeAccountName = YouTubeBaseSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, string.Empty);
                YouTubeAPIKey = YouTubeBaseSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, string.Empty);
                ConnectKey = YouTubeBaseSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeConnectKeyKey, Keys.F9);
                return true;
            }
            else
            {
                logger.Error($"Failed to load YouTube settings as the settings file couldnt be found at '{expectedSettingsFilePath}'");
                return false;
            }
        }

        /// <summary>
        /// Saves the youtube settings back to disk
        /// </summary>
        /// <param name="gameDirectory"></param>
        public bool SaveSettings(string gameDirectory, string modFolder)
        {
            if (YouTubeBaseSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save raven settings, either internal settings object ({YouTubeBaseSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            YouTubeBaseSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, YouTubeAccountName);
            YouTubeBaseSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, YouTubeAPIKey);
            YouTubeBaseSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeConnectKeyKey, ConnectKey.ToString());
            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return AreYouTubeSettingsLoaded;
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
