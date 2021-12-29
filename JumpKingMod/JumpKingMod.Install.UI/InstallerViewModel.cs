using JumpKingMod.Settings;
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
            string expectedSettingsFilePath = Path.Combine(GameDirectory, JumpKingModSettingsContext.SettingsFileName);
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
            if (ModSettings == null)
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

            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.SelectedStreamingPlatformKey, SelectedStreamingPlatform.ToString());

            // YouTube
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.YouTubeChannelNameKey, YouTubeSettings.YouTubeAccountName);
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.YouTubeApiKeyKey, YouTubeSettings.YouTubeAPIKey);
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.YouTubeRavenTriggerTypeKey, Ravens.YouTubeRavenTriggerType.ToString());

            // Twitch
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, TwitchSettings.TwitchAccountName);
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.OAuthKey, TwitchSettings.TwitchOAuth);
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavenTriggerTypeKey, Ravens.RavenTriggerType.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavenChannelPointRewardIDKey, Ravens.RavensChannelPointID);

            // Ravens
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensEnabledKey, Ravens.RavenEnabled.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensToggleDebugKeyKey, Ravens.RavenToggleDebugKey.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensClearDebugKeyKey, Ravens.RavenClearDebugKey.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensSubModeToggleKeyKey, Ravens.RavenSubModeToggleKey.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensMaxCountKey, Ravens.MaxRavensCount);

            // Chat Display
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.TwitchRelayEnabledKey, ChatDisplayEnabled.ToString());

            // Free Fly
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.FreeFlyEnabledKey, FreeFlyingEnabled.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.FreeFlyToggleKeyKey, FreeFlyToggleKey.ToString());

            // Exclusion List
            string expectedExclusionPath = Path.Combine(GameDirectory, JumpKingModSettingsContext.ExcludedTermFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expectedExclusionPath));
            File.WriteAllLines(expectedExclusionPath, Ravens.ExcludedTerms);

            // Raven Insults
            string expectedRavenInsultsPath = Path.Combine(GameDirectory, JumpKingModSettingsContext.RavenInsultsFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expectedRavenInsultsPath));
            File.WriteAllLines(expectedRavenInsultsPath, Ravens.RavenInsults);

            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavenInsultSpawnCountKey, Ravens.InsultRavenSpawnCount.ToString());

            MessageBox.Show($"Settings updated successfully!");
        }

        /// <summary>
        /// Creates or loads the mod settings from the given install directory
        /// </summary>
        private void LoadModSettings(bool createIfDoesntExist)
        {
            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(GameDirectory, JumpKingModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                ModSettings = new UserSettings(expectedSettingsFilePath, JumpKingModSettingsContext.GetDefaultSettings(), logger);

                // Load the initial data
                SelectedStreamingPlatform = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch);

                // YouTube Info
                YouTubeSettings.YouTubeAccountName = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.YouTubeChannelNameKey, string.Empty);
                YouTubeSettings.YouTubeAPIKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.YouTubeApiKeyKey, string.Empty);
                Ravens.YouTubeRavenTriggerType = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.YouTubeRavenTriggerTypeKey, YouTubeRavenTriggerTypes.ChatMessage);

                // Twitch Info
                TwitchSettings.TwitchAccountName = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                TwitchSettings.TwitchOAuth = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.OAuthKey, string.Empty);
                Ravens.RavenTriggerType = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenTriggerTypeKey, TwitchRavenTriggerTypes.ChatMessage);
                Ravens.RavensChannelPointID = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenChannelPointRewardIDKey, string.Empty);

                // Raven Info
                Ravens.RavenEnabled = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensEnabledKey, true);
                Ravens.RavenClearDebugKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensClearDebugKeyKey, Keys.F2);
                Ravens.RavenToggleDebugKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensToggleDebugKeyKey, Keys.F3);
                Ravens.RavenSubModeToggleKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensSubModeToggleKeyKey, Keys.F4);
                Ravens.MaxRavensCount = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensMaxCountKey, 5.ToString());
                Ravens.InsultRavenSpawnCount = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenInsultSpawnCountKey, 3.ToString());

                // Chat Display Info
                ChatDisplayEnabled = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.TwitchRelayEnabledKey, false);

                // Free Fly
                FreeFlyingEnabled = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.FreeFlyEnabledKey, false);
                FreeFlyToggleKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.FreeFlyToggleKeyKey, Keys.F1);
            }

            // Load in Exclusion List
            List<string> excludedTerms = new List<string>();
            string expectedExclusionPath = Path.Combine(GameDirectory, JumpKingModSettingsContext.ExcludedTermFilePath);
            try
            {
                if (File.Exists(expectedExclusionPath))
                {
                    string[] fileContent = File.ReadAllLines(expectedExclusionPath);
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        string line = fileContent[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingModSettingsContext.CommentCharacter)
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
            string expectedRavenInsultsPath = Path.Combine(GameDirectory, JumpKingModSettingsContext.RavenInsultsFilePath);
            try
            {
                if (File.Exists(expectedRavenInsultsPath))
                {
                    string[] fileContent = File.ReadAllLines(expectedRavenInsultsPath);
                    for (int i = 0; i < fileContent.Length; i++)
                    {
                        string line = fileContent[i].Trim();
                        if (line.Length <= 0 || line[0] == JumpKingModSettingsContext.CommentCharacter)
                        {
                            continue;
                        }

                        ravenInsultsFileContent.Add(line);
                    }
                }
                else
                {
                    ravenInsultsFileContent.AddRange(JumpKingModSettingsContext.GetDefaultInsults());
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

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
