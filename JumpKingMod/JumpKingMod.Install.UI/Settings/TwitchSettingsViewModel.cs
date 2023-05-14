using PBJKModBase.Twitch.Settings;
using Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Install.UI
{
    /// <summary>
    /// An aggregate class of Twitch Settings
    /// </summary>
    public class TwitchSettingsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;

        /// <summary>
        /// The name of the twitch account to use
        /// </summary>
        public string TwitchAccountName
        {
            get
            {
                return twitchAccountName;
            }
            set
            {
                if (twitchAccountName != value)
                {
                    twitchAccountName = value;
                    RaisePropertyChanged(nameof(TwitchAccountName));
                }
            }
        }
        private string twitchAccountName;

        /// <summary>
        /// The OAuth token to use for Twitch Chat
        /// </summary>
        public string TwitchOAuth
        {
            get
            {
                return twitchOAuth;
            }
            set
            {
                if (twitchOAuth != value)
                {
                    twitchOAuth = value;
                    RaisePropertyChanged(nameof(TwitchOAuth));
                }
            }
        }
        private string twitchOAuth;

        /// <summary>
        /// The <see cref="UserSettings"/> object containing the twitch settings
        /// </summary>
        public UserSettings TwitchBaseSettings
        {
            get
            {
                return twitchBaseSettings;
            }
            private set
            {
                twitchBaseSettings = value;
                RaisePropertyChanged(nameof(TwitchBaseSettings));
                RaisePropertyChanged(nameof(AreTwitchSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings twitchBaseSettings;

        /// <summary>
        /// Returns whether the twitch settings are currently populated
        /// </summary>
        public bool AreTwitchSettingsLoaded
        {
            get
            {
                return TwitchBaseSettings != null;
            }
        }

        /// <summary>
        /// Ctor for creating a <see cref="TwitchSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <exception cref="ArgumentNullException"></exception>
        public TwitchSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
        }

        /// <summary>
        /// Loads the twitch settings from disk, optionally creating the file if it doesnt exist
        /// </summary>
        public void LoadTwitchSettings(string gameDirectory, bool createIfDoesntExist)
        {
            if (TwitchBaseSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                return;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBaseTwitchSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                TwitchAccountName = TwitchBaseSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                TwitchOAuth = TwitchBaseSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.OAuthKey, string.Empty);
            }
        }

        /// <summary>
        /// Saves the twitch settings back to disk
        /// </summary>
        /// <param name="gameDirectory"></param>
        public void SaveTwitchSettings(string gameDirectory)
        {
            if (TwitchBaseSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                return;
            }

            TwitchBaseSettings.SetOrCreateSetting(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, TwitchAccountName);
            TwitchBaseSettings.SetOrCreateSetting(PBJKModBaseTwitchSettingsContext.OAuthKey, TwitchOAuth);
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
