using JumpKingRavensMod.Settings.Editor.API;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JumpKingRavensMod.Settings.Editor
{
    public class SettingsEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// An aggregate class of Twitch Settings
        /// </summary>
        public TwitchSettingsViewModel TwitchSettings { get; set; }

        /// <summary>
        /// An aggregate class of YouTube Settings
        /// </summary>
        public YouTubeSettingsViewModel YouTubeSettings { get; set; }

        /// <summary>
        /// An aggregate class of Raven Settings
        /// </summary>
        public RavensSettingsViewModel RavensSettings { get; set; }

        /// <summary>
        /// An aggregate class of Streaming Settings
        /// </summary>
        public StreamingSettingsViewModel StreamingSettings { get; set; }

        public ICommand UpdateSettingsCommand { get; private set; }

        private readonly ILogger logger;
        private readonly UserSettings installerSettings;
        private readonly List<ISettingsViewModel> registeredSettings;
        private readonly string gameDirectory;

        public SettingsEditorViewModel(ILogger logger, string gameDirectory)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.gameDirectory = gameDirectory ?? throw new ArgumentNullException(nameof(gameDirectory));
            installerSettings = new UserSettings(JumpKingModInstallerSettingsContext.SettingsFileName, JumpKingModInstallerSettingsContext.GetDefaultSettings(), logger);
            registeredSettings = new List<ISettingsViewModel>();

            UpdateSettingsCommand = new DelegateCommand((window) =>
            {
                UpdateModSettings();
                if (window is Window win)
                {
                    win.Close();
                }
            });

            RavensSettings = new RavensSettingsViewModel(logger);
            TwitchSettings = new TwitchSettingsViewModel(logger);
            YouTubeSettings = new YouTubeSettingsViewModel(logger);
            StreamingSettings = new StreamingSettingsViewModel(logger);

            registeredSettings.Add(RavensSettings);
            registeredSettings.Add(TwitchSettings);
            registeredSettings.Add(YouTubeSettings);
            registeredSettings.Add(StreamingSettings);

            // Load settings when opened
            LoadModSettings();
        }

        /// <summary>
        /// Loads all the settings using the registered ViewModels
        /// </summary>
        private void LoadModSettings()
        {
            bool success = true;
            for (int i = 0; i < registeredSettings.Count; i++)
            {
                success &= registeredSettings[i].LoadSettings(gameDirectory, createIfDoesntExist: true);
            }
        }

        /// <summary>
        /// Updates the settings that the mod will use based on the current values in the ViewModel
        /// </summary>
        private void UpdateModSettings()
        {
            bool success = true;
            for (int i = 0; i < registeredSettings.Count; i++)
            {
                success &= registeredSettings[i].SaveSettings(gameDirectory);
            }
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
