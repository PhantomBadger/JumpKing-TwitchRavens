using JumpKingRavensMod.Settings;
using JumpKingModifiersMod.Settings;
using Logging;
using Logging.API;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.Xna.Framework.Input;
using Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MessageBox = System.Windows.MessageBox;
using System.Globalization;
using PBJKModBase.Twitch.Settings;
using PBJKModBase.YouTube.Settings;
using JumpKingMod.Install.UI.Settings;
using JumpKingMod.Install.UI.API;
using System.Windows.Controls;

namespace JumpKingRavensMod.Install.UI
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
        /// An aggregate class of Modifiers Settings
        /// </summary>
        public ModifiersSettingsViewModel ModifiersSettings { get; set; }

        /// <summary>
        /// An aggregate class of Streaming Settings
        /// </summary>
        public StreamingSettingsViewModel StreamingSettings { get; set; }

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
        /// The error message to display when an install is not valid
        /// </summary>
        public string InstallErrorMessage
        {
            get
            {
                return installErrorMessage;
            }
            set
            {
                if (installErrorMessage != value)
                {
                    installErrorMessage = value;
                    RaisePropertyChanged(nameof(InstallErrorMessage));
                }
            }
        }
        private string installErrorMessage;

        /// <summary>
        /// Whether to show the install error message
        /// </summary>
        public bool ShowInstallErrorMessage
        {
            get
            {
                return showInstallErrorMessage;
            }
            set
            {
                if (showInstallErrorMessage != value)
                {
                    showInstallErrorMessage = value;
                    RaisePropertyChanged(nameof(ShowInstallErrorMessage));
                }
            }
        }
        private bool showInstallErrorMessage;

        #region Commands
        /// <summary>
        /// Opens a Folder Browser to browse for the Game Directory
        /// </summary>
        public ICommand BrowseGameDirectoryCommand { get; private set; }

        /// <summary>
        /// Opens a Folder Browser to browse for the Mod Directory
        /// </summary>
        public ICommand BrowseModDirectoryCommand { get; private set; }

        /// <summary>
        /// Attempts to Install the Mods
        /// </summary>
        public DelegateCommand InstallCommand { get; private set; }

        /// <summary>
        /// Updates the settings files
        /// </summary>
        public DelegateCommand UpdateSettingsCommand { get; private set; }

        /// <summary>
        /// Load the settings from disk
        /// </summary>
        public DelegateCommand LoadSettingsCommand { get; private set; }
        #endregion

        private readonly ILogger logger;
        private readonly UserSettings installerSettings;
        private readonly List<IInstallerSettingsViewModel> registeredSettings;

        private const string ExpectedFrameworkDllName = "MonoGame.Framework.dll";
        private const string ExpectedModDllName = "JumpKingModLoader.dll";
        private const string RemoteModFolderSuffix = @"Content\Mods";

        /// <summary>
        /// Ctor for creating a <see cref="InstallerViewModel"/>
        /// </summary>
        public InstallerViewModel(StackPanel modifiersStackPanel)
        {
            logger = new ConsoleLogger();
            installerSettings = new UserSettings(JumpKingModInstallerSettingsContext.SettingsFileName, JumpKingModInstallerSettingsContext.GetDefaultSettings(), logger);
            registeredSettings = new List<IInstallerSettingsViewModel>();
            
            InitialiseCommands();

            RavensSettings = new RavensSettingsViewModel(UpdateSettingsCommand, LoadSettingsCommand, logger);
            ModifiersSettings = new ModifiersSettingsViewModel(UpdateSettingsCommand, LoadSettingsCommand, logger, modifiersStackPanel);
            TwitchSettings = new TwitchSettingsViewModel(UpdateSettingsCommand, LoadSettingsCommand, logger);
            YouTubeSettings = new YouTubeSettingsViewModel(UpdateSettingsCommand, LoadSettingsCommand, logger);
            StreamingSettings = new StreamingSettingsViewModel(UpdateSettingsCommand, LoadSettingsCommand, logger);

            registeredSettings.Add(RavensSettings);
            registeredSettings.Add(ModifiersSettings);
            registeredSettings.Add(TwitchSettings);
            registeredSettings.Add(YouTubeSettings);
            registeredSettings.Add(StreamingSettings);

            GameDirectory = installerSettings.GetSettingOrDefault(JumpKingModInstallerSettingsContext.GameDirectoryKey, string.Empty);
            ModDirectory = installerSettings.GetSettingOrDefault(JumpKingModInstallerSettingsContext.ModDirectoryKey, string.Empty);

            if (string.IsNullOrWhiteSpace(ModDirectory))
            {
                string attemptedDir = Path.Combine(@"..\Mod\");
                var directoryInfo = new DirectoryInfo(attemptedDir);
                if (directoryInfo.Exists)
                {
                    ModDirectory = directoryInfo.FullName;
                }
            }

            RavensSettings.InitialiseCommands();
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
            UpdateSettingsCommand = new DelegateCommand(_ => 
            { 
                UpdateModSettings(); 
            }, 
            _ => 
            { 
                return registeredSettings.All(settings => settings.AreSettingsLoaded()); 
            });
            LoadSettingsCommand = new DelegateCommand(_ => 
            {
                LoadModSettings();
            });
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

                        // Get all files and all files in the subfolders
                        List<string> localFiles = new List<string>();
                        localFiles.AddRange(Directory.GetFiles(ModDirectory));
                        string[] folders = Directory.GetDirectories(ModDirectory);
                        for (int i = 0; i < folders.Length; i++)
                        {
                            localFiles.AddRange(Directory.GetFiles(folders[i]));
                        }

                        if (localFiles.Count > 0)
                        {
                            FileUnblocker fileUnblocker = new FileUnblocker();

                            // Go through each file, and move it over into the destination
                            for (int i = 0; i < localFiles.Count; i++)
                            {
                                string relativePath = localFiles[i].Replace(ModDirectory, "").Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                string dstFilePath = Path.Combine(expectedRemoteModFolder, relativePath);
                                Directory.CreateDirectory(Path.GetDirectoryName(dstFilePath));
                                File.Copy(localFiles[i], dstFilePath, true);

                                // Attempt to unblock the file
                                if (!fileUnblocker.TryUnblockFile(dstFilePath, out string error, out int errorCode) && errorCode != FileUnblocker.ERROR_FILE_NOT_FOUND)
                                {
                                    logger.Warning($"Failed to unblock '{dstFilePath}' with error '{error}', file will need to be manually unblocked. Refer to README on Repo");
                                }
                                else
                                {
                                    logger.Information($"Successfully Copied & Unblocked File '{dstFilePath}'");
                                }
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
                string expectedModDllPath = Path.Combine(expectedRemoteModFolder, expectedRemoteModFolder, ExpectedModDllName);
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
                for (int i = 0; i < registeredSettings.Count; i++)
                {
                    registeredSettings[i].LoadSettings(gameDirectory, expectedRemoteModFolder, createIfDoesntExist: true);
                }
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
            StringBuilder installMessageBuilder = new StringBuilder();

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

            // Update the messaging
            ShowInstallErrorMessage = false;
            if (!validGameDir)
            {
                installMessageBuilder.Append("The Install Directory specified does not contain MonoGame.Framework.dll or JumpKing.exe\n");
                ShowInstallErrorMessage = true;
            }
            if (!validModDir)
            {
                installMessageBuilder.Append("The Mod Directory specified does not contain JumpKingModLoader.dll");
                ShowInstallErrorMessage = true;
            }
            InstallErrorMessage = installMessageBuilder.ToString();

            return validGameDir && validModDir;
        }

        /// <summary>
        /// Called by <see cref="UpdateSettingsCommand"/> to determine whether the command can execute
        /// </summary>
        private bool CanUpdateModSettings()
        {
            if (string.IsNullOrWhiteSpace(GameDirectory))
            {
                return false;
            }

            string expectedSettingsFilePath = Path.Combine(GameDirectory, JumpKingRavensModSettingsContext.SettingsFileName);
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
        /// Loads all the settings using the registered ViewModels
        /// </summary>
        private void LoadModSettings()
        {
            if (string.IsNullOrWhiteSpace(GameDirectory))
            {
                return;
            }

            bool success = true;
            for (int i = 0; i < registeredSettings.Count; i++)
            {
                success &= registeredSettings[i].LoadSettings(gameDirectory, ExpectedRemoteModDirectory, createIfDoesntExist: true);
            }

            if (!success)
            {
                MessageBox.Show($"Failed to load settings! Check the console logs for more info!");
            }
            else
            {
                MessageBox.Show($"Settings loaded successfully!");
            }
        }

        /// <summary>
        /// Updates the settings that the mod will use based on the current values in the ViewModel
        /// </summary>
        private void UpdateModSettings()
        {
            if (string.IsNullOrWhiteSpace(GameDirectory))
            {
                return;
            }

            bool success = true;
            for (int i = 0; i < registeredSettings.Count; i++)
            {
                success &= registeredSettings[i].SaveSettings(gameDirectory, ExpectedRemoteModDirectory);
            }

            if (!success)
            {
                MessageBox.Show($"Failed to save settings! Check the console logs for more info!");
            }
            else
            {
                MessageBox.Show($"Settings updated successfully!");
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
