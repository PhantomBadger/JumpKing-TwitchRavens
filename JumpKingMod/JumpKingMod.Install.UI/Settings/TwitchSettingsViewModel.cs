﻿using JumpKingMod.Install.UI.API;
using Logging.API;
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
    public class TwitchSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

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
        public TwitchSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Loads the twitch settings from disk, optionally creating the file if it doesnt exist
        /// </summary>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load Twitch settings as provided Game Directory was empty!");
                return false;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBaseTwitchSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (TwitchBaseSettings == null)
                {
                    TwitchBaseSettings = new UserSettings(expectedSettingsFilePath, PBJKModBaseTwitchSettingsContext.GetDefaultSettings(), logger);
                }

                TwitchAccountName = TwitchBaseSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                TwitchOAuth = TwitchBaseSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.OAuthKey, string.Empty);

                return true;
            }
            else
            {
                logger.Error($"Failed to load Twitch settings as the settings file couldnt be found at '{expectedSettingsFilePath}'");
                return false;
            }
        }

        /// <summary>
        /// Saves the twitch settings back to disk
        /// </summary>
        /// <param name="gameDirectory"></param>
        public bool SaveSettings(string gameDirectory, string modFolder)
        {
            if (TwitchBaseSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save twitch settings, either internal settings object ({TwitchBaseSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            TwitchBaseSettings.SetOrCreateSetting(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, TwitchAccountName);
            TwitchBaseSettings.SetOrCreateSetting(PBJKModBaseTwitchSettingsContext.OAuthKey, TwitchOAuth);
            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return AreTwitchSettingsLoaded;
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
