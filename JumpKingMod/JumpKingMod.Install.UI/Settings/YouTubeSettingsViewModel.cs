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
    public class YouTubeSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;

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
        public YouTubeSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
        }

        /// <summary>
        /// Loads the youtube settings from disk, optionally creating the file if it doesnt exist
        /// </summary>
        public void LoadYouTubeSettings(string gameDirectory, bool createIfDoesntExist)
        {
            if (YouTubeBaseSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                return;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBaseYouTubeSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                YouTubeBaseSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, YouTubeAccountName);
                YouTubeBaseSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, YouTubeAPIKey);
                YouTubeBaseSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeConnectKeyKey, ConnectKey.ToString());
            }
        }

        /// <summary>
        /// Saves the youtube settings back to disk
        /// </summary>
        /// <param name="gameDirectory"></param>
        public void SaveYouTubeSettings(string gameDirectory)
        {
            if (YouTubeBaseSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                return;
            }

            YouTubeBaseSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, YouTubeAccountName);
            YouTubeBaseSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, YouTubeAPIKey);
            YouTubeBaseSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeConnectKeyKey, ConnectKey.ToString());
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
