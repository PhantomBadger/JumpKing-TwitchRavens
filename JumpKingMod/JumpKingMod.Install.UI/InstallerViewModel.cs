using JumpKingMod.Settings;
using Logging;
using Logging.API;
using Microsoft.WindowsAPICodePack.Dialogs;
using Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JumpKingMod.Install.UI
{
    public class InstallerViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The directory to install our mod to
        /// </summary>
        public string GameDirectory
        {
            get
            {
                return gameDirectory;
            }
            set
            {
                if (gameDirectory != value)
                {
                    gameDirectory = value;
                    RaisePropertyChanged(nameof(GameDirectory));
                    InstallCommand.RaiseCanExecuteChanged();
                    UpdateSettingsCommand.RaiseCanExecuteChanged();
                    LoadSettingsCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string gameDirectory;

        /// <summary>
        /// The directory of our install media
        /// </summary>
        public string ModDirectory
        {
            get
            {
                return modDirectory;
            }
            set
            {
                if (modDirectory != value)
                {
                    modDirectory = value;
                    RaisePropertyChanged(nameof(ModDirectory));
                    InstallCommand.RaiseCanExecuteChanged();
                }
            }
        }
        private string modDirectory;

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
        /// Combines <see cref="GameDirectory"/> with the <see cref="RemoteModFolderSuffix"/> to get the expected Mod Directory
        /// </summary>
        public string ExpectedRemoteModDirectory
        {
            get
            {
                return Path.Combine(GameDirectory, RemoteModFolderSuffix);
            }
        }

        /// <summary>
        /// Returns whether the mod settings are currently populated
        /// </summary>
        public bool AreModSettingsLoaded
        {
            get
            {
                return ModSettings != null;
            }
        }

        public UserSettings ModSettings
        {
            get
            {
                return modSettings;
            }
            private set
            {
                modSettings = value;
                RaisePropertyChanged(nameof(ModSettings));
                RaisePropertyChanged(nameof(AreModSettingsLoaded));
                UpdateSettingsCommand.RaiseCanExecuteChanged();
                LoadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings modSettings;

        public ICommand BrowseGameDirectoryCommand { get; private set; }
        public ICommand BrowseModDirectoryCommand { get; private set; }
        public DelegateCommand InstallCommand { get; private set; }
        public DelegateCommand UpdateSettingsCommand { get; private set; }
        public DelegateCommand LoadSettingsCommand { get; private set; }

        private readonly ILogger logger;
        private readonly UserSettings installerSettings;

        private const string ExpectedFrameworkDllName = "MonoGame.Framework.dll";
        private const string ExpectedModDllName = "JumpKingModLoader.dll";
        private const string RemoteModFolderSuffix = @"Content\Mods";

        /// <summary>
        /// Ctor for creating a <see cref="InstallerViewModel"/>
        /// </summary>
        public InstallerViewModel()
        {
            logger = new ConsoleLogger();
            installerSettings = new UserSettings(JumpKingModInstallerSettingsContext.SettingsFileName, JumpKingModInstallerSettingsContext.GetDefaultSettings(), logger);

            InitialiseCommands();

            GameDirectory = installerSettings.GetSettingOrDefault(JumpKingModInstallerSettingsContext.GameDirectoryKey, string.Empty);
            ModDirectory = installerSettings.GetSettingOrDefault(JumpKingModInstallerSettingsContext.ModDirectoryKey, string.Empty);
            
        }

        /// <summary>
        /// Called when the window is closing
        /// Updates the settings
        /// </summary>
        public void OnWindowClosing(object sender, CancelEventArgs e)
        {
            UpdateInstallerSettings();
        }

        /// <summary>
        /// Initialises the commands used for this window
        /// </summary>
        private void InitialiseCommands()
        {
            BrowseGameDirectoryCommand = new DelegateCommand(_ => { BrowseForGameDirectory(); });
            BrowseModDirectoryCommand = new DelegateCommand(_ => { BrowseForModDirectory(); });
            InstallCommand = new DelegateCommand(_ => { InstallMod(); }, _ => { return CanInstallMod(); });
            UpdateSettingsCommand = new DelegateCommand(_ => { UpdateModSettings(); }, _ => { return AreModSettingsLoaded && CanUpdateModSettings(); });
            LoadSettingsCommand = new DelegateCommand(_ => { LoadModSettings(createIfDoesntExist: true); }, _ => { return CanUpdateModSettings(); });
        }

        /// <summary>
        /// Asks the user to select the game's install directory
        /// </summary>
        private void BrowseForGameDirectory()
        {
            string userPickedVal = AskUserForFolder(GameDirectory);
            if (!string.IsNullOrWhiteSpace(userPickedVal))
            {
                GameDirectory = userPickedVal;
            }
        }

        /// <summary>
        /// Asks the user to select the Mod's install directory
        /// </summary>
        private void BrowseForModDirectory()
        {
            string userPickedVal = AskUserForFolder(ModDirectory);
            if (!string.IsNullOrWhiteSpace(userPickedVal))
            {
                ModDirectory = userPickedVal;
            }
        }

        /// <summary>
        /// Attempts to copy the local mod folder to the remote location, then attempts to install the mod
        /// </summary>
        private void InstallMod()
        {
            // Copy over local mods to destination mods
            string expectedRemoteModFolder = ExpectedRemoteModDirectory;
            bool success = true;
            string errorText = string.Empty;
            if (Directory.Exists(ModDirectory))
            {
                try
                {
                    // If the local and destination directories match, then we're golden!
                    if (!expectedRemoteModFolder.Equals(ModDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.CreateDirectory(expectedRemoteModFolder);
                        string[] dllFiles = Directory.GetFiles(ModDirectory);
                        if (dllFiles.Length > 0)
                        {
                            for (int i = 0; i < dllFiles.Length; i++)
                            {
                                string dstFilePath = Path.Combine(expectedRemoteModFolder, Path.GetFileName(dllFiles[i]));
                                File.Copy(dllFiles[i], dstFilePath, true);
                            }
                        }
                        else
                        {
                            errorText = $"Failed to identify any files in our Install Directory at '{ModDirectory}'";
                            success = false;
                            logger.Error(errorText);
                        }
                    }
                }
                catch (Exception e)
                {
                    errorText = $"Failed to Install Mod due to Exception: {e.ToString()}";
                    success = false;
                    logger.Error(errorText);
                }
            }
            else
            {
                success = false;
                errorText = $"Failed to Install Mod as we couldn't find our local install media in '{ModDirectory}'!";
            }

            // Do the Install
            if (success)
            {
                Installer installer = new Installer();

                string frameworkDllPath = Path.Combine(GameDirectory, ExpectedFrameworkDllName);
                string expectedModDllPath = Path.Combine(expectedRemoteModFolder, ExpectedModDllName);
                ModEntrySettings modEntrySettings = new ModEntrySettings()
                {
                    EntryClassTypeName = "JumpKingModLoader.Loader",
                    EntryMethodName = "Init"
                };
                success = installer.InstallMod(frameworkDllPath, expectedModDllPath, modEntrySettings, out string error);
                errorText = error;
            }

            // Load in any settings at the mod location
            if (success)
            {
                LoadModSettings(createIfDoesntExist: true);
            }

            // Report the result
            if (success)
            {
                MessageBox.Show("JumpKingMod Installed Correctly!");
            }
            else
            {
                MessageBox.Show($"JumpKingMod Failed to Install! Error: {errorText}", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Called by <see cref="InstallCommand"/> to determine whether the command can execute
        /// </summary>
        private bool CanInstallMod()
        {
            bool validGameDir = false;
            if (Directory.Exists(GameDirectory))
            {
                string dllPath = Path.Combine(GameDirectory, ExpectedFrameworkDllName);
                validGameDir = File.Exists(dllPath);
            }

            bool validModDir = false;
            if (Directory.Exists(ModDirectory))
            {
                string dllPath = Path.Combine(ModDirectory, ExpectedModDllName);
                validModDir = File.Exists(dllPath);
            }

            return validGameDir && validModDir;
        }

        /// <summary>
        /// Called by <see cref="UpdateSettingsCommand"/> to determine whether the command can execute
        /// </summary>
        private bool CanUpdateModSettings()
        {
            string expectedSettingsFilePath = Path.Combine(ExpectedRemoteModDirectory, JumpKingModSettingsContext.SettingsFileName);
            bool fileExists = File.Exists(expectedSettingsFilePath);
            return fileExists;
        }

        /// <summary>
        /// Utility function which wraps a CommonOpenFileDialog which is used to ask the user for a folder
        /// </summary>
        private string AskUserForFolder(string initialDirectory)
        {
            CommonOpenFileDialog openDialog = new CommonOpenFileDialog();
            openDialog.IsFolderPicker = true;
            if (!string.IsNullOrWhiteSpace(initialDirectory) && Directory.Exists(initialDirectory))
            {
                openDialog.InitialDirectory = initialDirectory;
            }

            if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return openDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// Updates the settings from the current values in the ViewModel
        /// </summary>
        private void UpdateInstallerSettings()
        {
            installerSettings.SetOrCreateSetting(JumpKingModInstallerSettingsContext.GameDirectoryKey, gameDirectory);
            installerSettings.SetOrCreateSetting(JumpKingModInstallerSettingsContext.ModDirectoryKey, modDirectory);
            
        }

        /// <summary>
        /// Updates the settings that the mod will use based on the current values in the ViewModel
        /// </summary>
        private void UpdateModSettings()
        {
            ModSettings?.SetOrCreateSetting(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, TwitchAccountName);
        }

        /// <summary>
        /// Creates or loads the mod settings from the given install directory
        /// </summary>
        private void LoadModSettings(bool createIfDoesntExist)
        {
            // Load in the settings
            string expectedRemoteModFolder = ExpectedRemoteModDirectory;
            string expectedSettingsFilePath = Path.Combine(expectedRemoteModFolder, JumpKingModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                ModSettings = new UserSettings(expectedSettingsFilePath, JumpKingModSettingsContext.GetDefaultSettings(), logger);

                // Load the initial data
                TwitchAccountName = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
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
