using JumpKingRavensMod.Settings.Editor.API;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.Twitch.Settings;
using PBJKModBase.YouTube.Settings;
using Settings;
using System;
using System.ComponentModel;
using System.IO;

namespace JumpKingRavensMod.Settings.Editor
{
    /// <summary>
    /// An aggregatce alss of YouTube Settings
    /// </summary>
    public class YouTubeSettingsViewModel : INotifyPropertyChanged, ISettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

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

                    if (initialSettingsLoadComplete)
                    {
                        restartSettingChanged?.Invoke();
                    }
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

                    if (initialSettingsLoadComplete)
                    {
                        restartSettingChanged?.Invoke();
                    }
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

        private readonly Action restartSettingChanged;

        private bool initialSettingsLoadComplete = false;

        /// <summary>
        /// Ctor for creating a <see cref="YouTubeSettingsViewModel"/>
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public YouTubeSettingsViewModel(ILogger logger, Action restartSettingChanged)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.restartSettingChanged = restartSettingChanged ?? throw new ArgumentNullException(nameof(restartSettingChanged));
        }

        /// <summary>
        /// Loads the youtube settings from disk, optionally creating the file if it doesnt exist
        /// </summary>
        public bool LoadSettings(string gameDirectory, bool createIfDoesntExist)
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

                initialSettingsLoadComplete = true;
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
        public bool SaveSettings(string gameDirectory)
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
