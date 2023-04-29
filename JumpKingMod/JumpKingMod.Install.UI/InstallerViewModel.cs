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
        /// Converts the <see cref="SelectedStreamingPlatform"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsStreamingOnTwitch
        {
            get
            {
                return selectedStreamingPlatform == AvailableStreamingPlatforms.Twitch;
            }
            set
            {
                SelectedStreamingPlatform = value ? AvailableStreamingPlatforms.Twitch : AvailableStreamingPlatforms.YouTube;
            }
        }

        /// <summary>
        /// Converts the <see cref="SelectedStreamingPlatform"/> property into a bool which can be bound to the UI or filtered against
        /// </summary>
        public bool IsStreamingOnYouTube
        {
            get
            {
                return selectedStreamingPlatform == AvailableStreamingPlatforms.YouTube;
            }
            set
            {
                SelectedStreamingPlatform = value ? AvailableStreamingPlatforms.YouTube : AvailableStreamingPlatforms.Twitch;
            }
        }

        /// <summary>
        /// What streaming platform is selected for use with the Mod
        /// </summary>
        public AvailableStreamingPlatforms SelectedStreamingPlatform
        {
            get
            {
                return selectedStreamingPlatform;
            }
            set
            {
                if (selectedStreamingPlatform != value)
                {
                    selectedStreamingPlatform = value;
                    RaisePropertyChanged(nameof(SelectedStreamingPlatform));
                    RaisePropertyChanged(nameof(IsStreamingOnTwitch));
                    RaisePropertyChanged(nameof(IsStreamingOnYouTube));
                }
            }
        }
        private AvailableStreamingPlatforms selectedStreamingPlatform;

        /// <summary>
        /// An aggregate class of Twitch Settings
        /// </summary>
        public TwitchSettings TwitchSettings { get; set; }

        /// <summary>
        /// An aggregate class of YouTube Settings
        /// </summary>
        public YouTubeSettings YouTubeSettings { get; set; }

        /// <summary>
        /// An aggregate class of Raven Settings
        /// </summary>
        public RavensSettings Ravens { get; set; }

        /// <summary>
        /// Whether the chat display is enabled or not
        /// </summary>
        public bool ChatDisplayEnabled
        {
            get
            {
                return chatDisplayEnabled;
            }
            set
            {
                if (chatDisplayEnabled != value)
                {
                    chatDisplayEnabled = value;
                    RaisePropertyChanged(nameof(ChatDisplayEnabled));
                }
            }
        }
        private bool chatDisplayEnabled;

        /// <summary>
        /// Whether the Free Flying Mod is enabled or not
        /// </summary>
        public bool FreeFlyingEnabled
        {
            get
            {
                return freeFlyingEnabled;
            }
            set
            {
                if (freeFlyingEnabled != value)
                {
                    freeFlyingEnabled = value;
                    RaisePropertyChanged(nameof(FreeFlyingEnabled));
                }
            }
        }
        private bool freeFlyingEnabled;

        /// <summary>
        /// The key to use to toggle free flying
        /// </summary>
        public Keys FreeFlyToggleKey
        {
            get
            {
                return freeFlyToggleKey;
            }
            set
            {
                if (freeFlyToggleKey != value)
                {
                    freeFlyToggleKey = value;
                    RaisePropertyChanged(nameof(FreeFlyToggleKey));
                }
            }
        }
        private Keys freeFlyToggleKey;

        /// <summary>
        /// Returns whether the Fall Damage Mod should be enabled
        /// </summary>
        public bool FallDamageEnabled
        {
            get
            {
                return fallDamageEnabled;
            }
            set
            {
                if (fallDamageEnabled != value)
                {
                    fallDamageEnabled = value;
                    RaisePropertyChanged(nameof(FallDamageEnabled));
                    RaisePropertyChanged(nameof(FallDamageBloodSplatVisibility));
                }
            }
        }
        private bool fallDamageEnabled;

        /// <summary>
        /// Returns the key which toggles the fall damage modifier
        /// </summary>
        public Keys FallDamageToggleKey
        {
            get
            {
                return fallDamageToggleKey;
            }
            set
            {
                if (fallDamageToggleKey != value)
                {
                    fallDamageToggleKey = value;
                    RaisePropertyChanged(nameof(FallDamageToggleKey));
                }
            }
        }
        private Keys fallDamageToggleKey;

        /// <summary>
        /// The modifier to apply to the fall distance to produce damage
        /// </summary>
        public string FallDamageModifier
        {
            get
            {
                return fallDamageModifier.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    fallDamageModifier = JumpKingModifiersModSettingsContext.DefaultFallDamageModifier;
                }
                else
                {
                    if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 1000)
                        {
                            newVal = 1000;
                        }
                        fallDamageModifier = newVal;
                    }
                }
                RaisePropertyChanged(nameof(FallDamageModifier));
            }
        }
        private float fallDamageModifier;

        /// <summary>
        /// Returns whether the Blood Splat on Fall should be enabled
        /// </summary>
        public bool FallDamageBloodSplatEnabled
        {
            get
            {
                return fallDamageBloodSplatEnabled;
            }
            set
            {
                if (fallDamageBloodSplatEnabled != value)
                {
                    fallDamageBloodSplatEnabled = value;
                    RaisePropertyChanged(nameof(FallDamageBloodSplatEnabled));
                    RaisePropertyChanged(nameof(FallDamageBloodSplatVisibility));
                }
            }
        }
        private bool fallDamageBloodSplatEnabled;

        /// <summary>
        /// Aggregate field for visibility control
        /// </summary>
        public bool FallDamageBloodSplatVisibility
        {
            get
            {
                return FallDamageBloodSplatEnabled && FallDamageEnabled;
            }
        }

        /// <summary>
        /// The key to press to clear Blood Splats
        /// </summary>
        public Keys FallDamageClearBloodKey
        {
            get
            {
                return fallDamageClearBloodKey;
            }
            set
            {
                if (fallDamageClearBloodKey != value)
                {
                    fallDamageClearBloodKey = value;
                    RaisePropertyChanged(nameof(FallDamageClearBloodKey));
                }
            }
        }
        private Keys fallDamageClearBloodKey;

        /// <summary>
        /// Whether we will use nice spawns for DLC maps for Fall Damage
        /// </summary>
        public bool FallDamageNiceSpawns
        {
            get
            {
                return fallDamageNiceSpawns;
            }
            set
            {
                if (fallDamageNiceSpawns != value)
                {
                    fallDamageNiceSpawns = value;
                    RaisePropertyChanged(nameof(FallDamageNiceSpawns));
                }
            }
        }
        private bool fallDamageNiceSpawns;

        /// <summary>
        /// Whether the Resizing mod will be enabled or not
        /// </summary>
        public bool ManualResizingEnabled
        {
            get
            {
                return resizingEnabled;
            }
            set
            {
                if (resizingEnabled != value)
                {
                    resizingEnabled = value;
                    RaisePropertyChanged(nameof(ManualResizingEnabled));
                }
            }
        }
        private bool resizingEnabled;

        /// <summary>
        /// The key to press to toggle the Resize Mod
        /// </summary>
        public Keys ManualResizingToggleKey
        {
            get
            {
                return manualResizingToggleKey;
            }
            set
            {
                if (manualResizingToggleKey != value)
                {
                    manualResizingToggleKey = value;
                    RaisePropertyChanged(nameof(ManualResizingToggleKey));
                }
            }
        }
        private Keys manualResizingToggleKey;

        /// <summary>
        /// The key to press to grow the screen in the Resize Mod
        /// </summary>
        public Keys ManualResizingGrowKey
        {
            get
            {
                return manualResizingGrowKey;
            }
            set
            {
                if (manualResizingGrowKey != value)
                {
                    manualResizingGrowKey = value;
                    RaisePropertyChanged(nameof(ManualResizingGrowKey));
                }
            }
        }
        private Keys manualResizingGrowKey;

        /// <summary>
        /// The key to press to shrink the screen in the Resize Mod
        /// </summary>
        public Keys ManualResizingShrinkKey
        {
            get
            {
                return manualResizingShrinkKey;
            }
            set
            {
                if (manualResizingShrinkKey != value)
                {
                    manualResizingShrinkKey = value;
                    RaisePropertyChanged(nameof(ManualResizingShrinkKey));
                }
            }
        }
        private Keys manualResizingShrinkKey;

        /// <summary>
        /// Whether the Rising Lava mod is enabled
        /// </summary>
        public bool RisingLavaEnabled
        {
            get
            {
                return risingLavaEnabled;
            }
            set
            {
                if (risingLavaEnabled != value)
                {
                    risingLavaEnabled = value;
                    RaisePropertyChanged(nameof(RisingLavaEnabled));
                }
            }
        }
        private bool risingLavaEnabled;

        /// <summary>
        /// The button to press to toggle rising lava in game
        /// </summary>
        public Keys RisingLavaToggleKey
        {
            get
            {
                return risingLavaToggleKey;
            }
            set
            {
                if (risingLavaToggleKey != value)
                {
                    risingLavaToggleKey = value;
                    RaisePropertyChanged(nameof(RisingLavaToggleKey));
                }
            }
        }
        private Keys risingLavaToggleKey;

        /// <summary>
        /// The speed the lava will raise
        /// </summary>
        public string RisingLavaSpeed
        {
            get
            {
                return risingLavaSpeed.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    risingLavaSpeed = JumpKingModifiersModSettingsContext.DefaultRisingLavaSpeed;
                }
                else
                {
                    if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 1000)
                        {
                            newVal = 1000;
                        }
                        risingLavaSpeed = newVal;
                    }
                }
                RaisePropertyChanged(nameof(RisingLavaSpeed));
            }
        }
        private float risingLavaSpeed;

        /// <summary>
        /// Whether or not we use nice spawns for DLC maps for Rising Lava
        /// </summary>
        public bool RisingLavaNiceSpawns
        {
            get
            {
                return risingLavaNiceSpawns;
            }
            set
            {
                if (risingLavaNiceSpawns != value)
                {
                    risingLavaNiceSpawns = value;
                    RaisePropertyChanged(nameof(RisingLavaNiceSpawns));
                }
            }
        }
        private bool risingLavaNiceSpawns;

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
        /// Returns whether the ravenmod settings are currently populated
        /// </summary>
        public bool AreRavenModSettingsLoaded
        {
            get
            {
                return RavenModSettings != null;
            }
        }

        /// <summary>
        /// Returns whether the fall damage mod settings are currently populated
        /// </summary>
        public bool AreFallDamageModSettingsLoaded
        {
            get
            {
                return FallDamageModSettings != null;
            }
        }

        public UserSettings RavenModSettings
        {
            get
            {
                return ravenModSettings;
            }
            private set
            {
                ravenModSettings = value;
                RaisePropertyChanged(nameof(RavenModSettings));
                RaisePropertyChanged(nameof(AreRavenModSettingsLoaded));
                UpdateSettingsCommand.RaiseCanExecuteChanged();
                LoadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings ravenModSettings;

        public UserSettings FallDamageModSettings
        {
            get
            {
                return fallDamageModSettings;
            }
            private set
            {
                fallDamageModSettings = value;
                RaisePropertyChanged(nameof(FallDamageModSettings));
                RaisePropertyChanged(nameof(AreFallDamageModSettingsLoaded));
                UpdateSettingsCommand.RaiseCanExecuteChanged();
                LoadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings fallDamageModSettings;

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

        public ICommand BrowseGameDirectoryCommand { get; private set; }
        public ICommand BrowseModDirectoryCommand { get; private set; }
        public DelegateCommand InstallCommand { get; private set; }
        public DelegateCommand UpdateSettingsCommand { get; private set; }
        public DelegateCommand LoadSettingsCommand { get; private set; }
        public ICommand AddExcludedTermCommand { get; private set; }
        public ICommand RemoveExcludedTermCommand { get; private set; }
        public ICommand AddRavenInsultCommand { get; private set; }
        public ICommand RemoveRavenInsultCommand { get; private set; }

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

            Ravens = new RavensSettings();
            TwitchSettings = new TwitchSettings();
            YouTubeSettings = new YouTubeSettings();

            InitialiseCommands();

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
            UpdateSettingsCommand = new DelegateCommand(_ => { UpdateModSettings(); }, _ => { return AreRavenModSettingsLoaded && CanUpdateModSettings(); });
            LoadSettingsCommand = new DelegateCommand(_ => 
            { 
                LoadRavenModSettings(createIfDoesntExist: true);
                LoadFallDamageModSettings(createIfDoesntExist: true);
            }, _ => { return CanUpdateModSettings(); });
            AddExcludedTermCommand = new DelegateCommand(_ => { AddToCollection(Ravens.ExcludedTerms, Ravens.CandidateExcludedItem); });
            RemoveExcludedTermCommand = new DelegateCommand(_ => 
            { 
                RemoveFromCollection(Ravens.ExcludedTerms, Ravens.SelectedExcludedItemIndex);
                Ravens.SelectedExcludedItemIndex = 0;
            });
            AddRavenInsultCommand = new DelegateCommand(_ => { AddToCollection(Ravens.RavenInsults, Ravens.CandidateRavenInsult); });
            RemoveRavenInsultCommand = new DelegateCommand(_ => 
            { 
                RemoveFromCollection(Ravens.RavenInsults, Ravens.SelectedRavenInsultIndex);
                Ravens.SelectedRavenInsultIndex = 0;
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
                LoadRavenModSettings(createIfDoesntExist: true);
                LoadFallDamageModSettings(createIfDoesntExist: true);
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
        /// Adds the <paramref name="candidate"/> to the <paramref name="collection"/> list
        /// </summary>
        private void AddToCollection(ObservableCollection<string> collection, string candidate)
        {
            collection?.Add(candidate);
        }

        /// <summary>
        /// Removes the <paramref name="indexToRemove"/> from the <paramref name="collection"/>  list
        /// </summary>
        private void RemoveFromCollection(ObservableCollection<string> collection, int indexToRemove)
        {
            if (collection != null && collection.Count > indexToRemove && indexToRemove >= 0)
            {
                collection?.RemoveAt(indexToRemove);
            }
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
            if (RavenModSettings == null || string.IsNullOrWhiteSpace(GameDirectory))
            {
                return;
            }

            if (SelectedStreamingPlatform == AvailableStreamingPlatforms.Twitch &&
                Ravens.RavenEnabled && 
                (Ravens.RavenTriggerType == TwitchRavenTriggerTypes.ChannelPointReward || Ravens.RavenTriggerType == TwitchRavenTriggerTypes.ChatMessage) && 
                ChatDisplayEnabled)
            {
                MessageBoxResult result = MessageBox.Show($"If Chat Display is active, the Chat-based Raven Triggers will not function. Are you sure you want to proceed?", "Setting Conflict!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel || result == MessageBoxResult.No || result == MessageBoxResult.None)
                {
                    return;
                }
            }

            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.SelectedStreamingPlatformKey, SelectedStreamingPlatform.ToString());

            // YouTube
            RavenModSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, YouTubeSettings.YouTubeAccountName);
            RavenModSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, YouTubeSettings.YouTubeAPIKey);
            RavenModSettings.SetOrCreateSetting(PBJKModBaseYouTubeSettingsContext.YouTubeConnectKeyKey, YouTubeSettings.ConnectKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.YouTubeRavenTriggerTypeKey, Ravens.YouTubeRavenTriggerType.ToString());

            // Twitch
            RavenModSettings.SetOrCreateSetting(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, TwitchSettings.TwitchAccountName);
            RavenModSettings.SetOrCreateSetting(PBJKModBaseTwitchSettingsContext.OAuthKey, TwitchSettings.TwitchOAuth);
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavenTriggerTypeKey, Ravens.RavenTriggerType.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavenChannelPointRewardIDKey, Ravens.RavensChannelPointID);

            // Ravens
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensEnabledKey, Ravens.RavenEnabled.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensToggleDebugKeyKey, Ravens.RavenToggleDebugKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensClearDebugKeyKey, Ravens.RavenClearDebugKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensSubModeToggleKeyKey, Ravens.RavenSubModeToggleKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensMaxCountKey, Ravens.MaxRavensCount);
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensDisplayTimeInSecondsKey, Ravens.MessageDurationInSeconds);

            // Chat Display
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.TwitchRelayEnabledKey, ChatDisplayEnabled.ToString());

            // Free Fly
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.FreeFlyEnabledKey, FreeFlyingEnabled.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.FreeFlyToggleKeyKey, FreeFlyToggleKey.ToString());

            // Exclusion List
            string expectedExclusionPath = Path.Combine(GameDirectory, PBJKModBaseTwitchSettingsContext.ExcludedTermFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expectedExclusionPath));
            File.WriteAllLines(expectedExclusionPath, Ravens.ExcludedTerms);

            // Raven Insults
            string expectedRavenInsultsPath = Path.Combine(GameDirectory, JumpKingRavensModSettingsContext.RavenInsultsFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expectedRavenInsultsPath));
            File.WriteAllLines(expectedRavenInsultsPath, Ravens.RavenInsults);

            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavenInsultSpawnCountKey, Ravens.InsultRavenSpawnCount.ToString());

            // Gun Mode
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.GunEnabledKey, Ravens.GunEnabled.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.GunToggleKeyKey, Ravens.GunToggleKey.ToString());

            // Fall Damage
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageEnabledKey, FallDamageEnabled.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.DebugTriggerFallDamageToggleKeyKey, FallDamageToggleKey.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageModifierKey, FallDamageModifier);
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageBloodEnabledKey, FallDamageBloodSplatEnabled.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageClearBloodKey, FallDamageClearBloodKey.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageNiceSpawnsKey, FallDamageNiceSpawns.ToString());

            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.ManualResizeEnabledKey, ManualResizingEnabled.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.DebugTriggerManualResizeToggleKey, ManualResizingToggleKey.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.ManualResizeGrowKeyKey, ManualResizingGrowKey.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.ManualResizeShrinkKeyKey, ManualResizingShrinkKey.ToString());

            // Rising Lava
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.RisingLavaEnabledKey, RisingLavaEnabled.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.DebugTriggerLavaRisingToggleKeyKey, RisingLavaToggleKey.ToString());
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.RisingLavaSpeedKey, RisingLavaSpeed);
            FallDamageModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.RisingLavaNiceSpawnsKey, RisingLavaNiceSpawns.ToString());

            MessageBox.Show($"Settings updated successfully!");
        }

        /// <summary>
        /// Creates or loads the mod settings from the given install directory
        /// </summary>
        private void LoadRavenModSettings(bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(GameDirectory))
            {
                return;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(GameDirectory, JumpKingRavensModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                RavenModSettings = new UserSettings(expectedSettingsFilePath, JumpKingRavensModSettingsContext.GetDefaultSettings(), logger);

                // Load the initial data
                SelectedStreamingPlatform = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch);

                // YouTube Info
                YouTubeSettings.YouTubeAccountName = RavenModSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, string.Empty);
                YouTubeSettings.YouTubeAPIKey = RavenModSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, string.Empty);
                YouTubeSettings.ConnectKey = RavenModSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeConnectKeyKey, Keys.F9);
                Ravens.YouTubeRavenTriggerType = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.YouTubeRavenTriggerTypeKey, YouTubeRavenTriggerTypes.ChatMessage);

                // Twitch Info
                TwitchSettings.TwitchAccountName = RavenModSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                TwitchSettings.TwitchOAuth = RavenModSettings.GetSettingOrDefault(PBJKModBaseTwitchSettingsContext.OAuthKey, string.Empty);
                Ravens.RavenTriggerType = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenTriggerTypeKey, TwitchRavenTriggerTypes.ChatMessage);
                Ravens.RavensChannelPointID = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenChannelPointRewardIDKey, string.Empty);

                // Raven Info
                Ravens.RavenEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensEnabledKey, true);
                Ravens.RavenClearDebugKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensClearDebugKeyKey, Keys.F2);
                Ravens.RavenToggleDebugKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensToggleDebugKeyKey, Keys.F3);
                Ravens.RavenSubModeToggleKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensSubModeToggleKeyKey, Keys.F4);
                Ravens.MaxRavensCount = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensMaxCountKey, 5.ToString());
                Ravens.MessageDurationInSeconds = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensDisplayTimeInSecondsKey, 3.0f.ToString());
                Ravens.InsultRavenSpawnCount = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenInsultSpawnCountKey, 3.ToString());

                // Chat Display Info
                ChatDisplayEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.TwitchRelayEnabledKey, false);

                // Free Fly
                FreeFlyingEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.FreeFlyEnabledKey, false);
                FreeFlyToggleKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.FreeFlyToggleKeyKey, Keys.F1);

                // Gun
                Ravens.GunEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.GunEnabledKey, false);
                Ravens.GunToggleKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.GunToggleKeyKey, Keys.F8);
            }

            // Load in Exclusion List
            List<string> excludedTerms = new List<string>();
            string expectedExclusionPath = Path.Combine(GameDirectory, PBJKModBaseTwitchSettingsContext.ExcludedTermFilePath);
            try
            {
                if (File.Exists(expectedExclusionPath))
                {
                    string[] fileContent = File.ReadAllLines(expectedExclusionPath);
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        string line = fileContent[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingRavensModSettingsContext.CommentCharacter)
                        {
                            continue;
                        }

                        excludedTerms.Add(line);
                    }
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(expectedExclusionPath));
                    File.Create(expectedExclusionPath);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Encountered error when parsing Exclusion List {e.ToString()}");
            }
            Ravens.ExcludedTerms = new ObservableCollection<string>(excludedTerms);

            // Load in Raven Insults
            List<string> ravenInsultsFileContent = new List<string>();
            string expectedRavenInsultsPath = Path.Combine(GameDirectory, JumpKingRavensModSettingsContext.RavenInsultsFilePath);
            try
            {
                if (File.Exists(expectedRavenInsultsPath))
                {
                    string[] fileContent = File.ReadAllLines(expectedRavenInsultsPath);
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        string line = fileContent[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingRavensModSettingsContext.CommentCharacter)
                        {
                            continue;
                        }

                        ravenInsultsFileContent.Add(line);
                    }
                }
                else
                {
                    ravenInsultsFileContent.AddRange(JumpKingRavensModSettingsContext.GetDefaultInsults());
                    Directory.CreateDirectory(Path.GetDirectoryName(expectedRavenInsultsPath));
                    File.WriteAllLines(expectedRavenInsultsPath, ravenInsultsFileContent);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Encountered error when parsing Raven Insults {e.ToString()}");
            }
            Ravens.RavenInsults = new ObservableCollection<string>(ravenInsultsFileContent);
        }

        private void LoadFallDamageModSettings(bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(GameDirectory))
            {
                return;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(GameDirectory, JumpKingModifiersModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                FallDamageModSettings = new UserSettings(expectedSettingsFilePath, JumpKingModifiersModSettingsContext.GetDefaultSettings(), logger);

                // Load the initial data
                FallDamageEnabled = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageEnabledKey, false);
                FallDamageToggleKey = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerFallDamageToggleKeyKey, Keys.F11);
                FallDamageModifier = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageModifierKey, JumpKingModifiersModSettingsContext.DefaultFallDamageModifier).ToString();
                FallDamageBloodSplatEnabled = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageBloodEnabledKey, true);
                FallDamageClearBloodKey = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageClearBloodKey, Keys.F10);
                FallDamageNiceSpawns = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageNiceSpawnsKey, true);

                ManualResizingEnabled = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeEnabledKey, false);
                ManualResizingToggleKey = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerManualResizeToggleKey, Keys.F9);
                ManualResizingGrowKey = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeGrowKeyKey, Keys.Up);
                ManualResizingShrinkKey = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeShrinkKeyKey, Keys.Down);

                RisingLavaEnabled = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaEnabledKey, false);
                RisingLavaToggleKey = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerLavaRisingToggleKeyKey, Keys.F7);
                RisingLavaSpeed = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaSpeedKey, JumpKingModifiersModSettingsContext.DefaultRisingLavaSpeed.ToString());
                RisingLavaNiceSpawns = FallDamageModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaNiceSpawnsKey, true);
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
