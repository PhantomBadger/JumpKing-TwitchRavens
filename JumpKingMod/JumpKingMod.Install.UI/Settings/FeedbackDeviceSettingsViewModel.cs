using JumpKingMod.Install.UI.API;
using JumpKingRavensMod.Install.UI;
using JumpKingRavensMod.Settings;
using Logging.API;
using PBJKModBase.FeedbackDevice.Settings;
using Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI.Settings
{
    /// <summary>
    /// An implementation of <see cref="IInstallerSettingsViewModel"/> to handle device agnostic feedback settings
    /// </summary>
    public class FeedbackDeviceSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

        /// <summary>
        /// What feedback device is selected for use with the Mod
        /// </summary>
        public AvailableFeedbackDevices SelectedFeedbackDevice
        {
            get
            {
                return selectedFeedbackDevice;
            }
            set
            {
                if (selectedFeedbackDevice != value)
                {
                    selectedFeedbackDevice = value;
                    RaisePropertyChanged(nameof(SelectedFeedbackDevice));
                    RaisePropertyChanged(nameof(IsUsingPiShock));
                    RaisePropertyChanged(nameof(IsUsingNoDevice));
                    RaisePropertyChanged(nameof(IsUsingAnyDevice));
                }
            }
        }
        private AvailableFeedbackDevices selectedFeedbackDevice;

        /// <summary>
        /// Converts the <see cref="SelectedFeedbackDevice"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsUsingPiShock
        {
            get
            {
                return selectedFeedbackDevice == AvailableFeedbackDevices.PiShock;
            }
            set
            {
                SelectedFeedbackDevice = value ? AvailableFeedbackDevices.PiShock : AvailableFeedbackDevices.None;
            }
        }

        /// <summary>
        /// Converts the <see cref="SelectedFeedbackDevice"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsUsingNoDevice
        {
            get
            {
                return selectedFeedbackDevice == AvailableFeedbackDevices.None;
            }
            set
            {
                SelectedFeedbackDevice = value ? AvailableFeedbackDevices.None : AvailableFeedbackDevices.PiShock;
            }
        }

        /// <summary>
        /// Converts the <see cref="SelectedFeedbackDevice"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsUsingAnyDevice
        {
            get
            {
                return selectedFeedbackDevice != AvailableFeedbackDevices.None;
            }
        }

        /// <summary>
        /// The <see cref="UserSettings"/> object for the feedback device settings
        /// </summary>
        public UserSettings FeedbackDeviceSettings
        {
            get
            {
                return feedbackDeviceSettings;
            }
            private set
            {
                feedbackDeviceSettings = value;
                RaisePropertyChanged(nameof(FeedbackDeviceSettings));
                RaisePropertyChanged(nameof(AreFeedbackDeviceSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings feedbackDeviceSettings;

        /// <summary>
        /// Returns whether the feedback device settings are currently populated
        /// </summary>
        public bool AreFeedbackDeviceSettingsLoaded
        {
            get
            {
                return feedbackDeviceSettings != null;
            }
        }

        /// <summary>
        /// Constructor for creating a <see cref="FeedbackDeviceSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FeedbackDeviceSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to load Feedback Device settings as provided Game Directory was empty!");
                return false;
            }

            string expectedSettingsFilePath = Path.Combine(gameDirectory, PBJKModBaseFeedbackDeviceSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (FeedbackDeviceSettings == null)
                {
                    FeedbackDeviceSettings = new UserSettings(expectedSettingsFilePath, PBJKModBaseFeedbackDeviceSettingsContext.GetDefaultSettings(), logger);
                }

                // Load the initial data
                SelectedFeedbackDevice = FeedbackDeviceSettings.GetSettingOrDefault(PBJKModBaseFeedbackDeviceSettingsContext.SelectedFeedbackDeviceKey, AvailableFeedbackDevices.None);
            }
            else
            {
                logger.Error($"Failed to load Feedback Device settings as the settings file couldn't be found at '{expectedSettingsFilePath}'");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool SaveSettings(string gameDirectory, string modFolder)
        {
            if (FeedbackDeviceSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save feedback device settings, either internal settings object ({FeedbackDeviceSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            FeedbackDeviceSettings.SetOrCreateSetting(PBJKModBaseFeedbackDeviceSettingsContext.SelectedFeedbackDeviceKey, SelectedFeedbackDevice.ToString());
            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return AreFeedbackDeviceSettingsLoaded;
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
