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
        /// Whether the Raven system is enabled or not
        /// </summary>
        public bool RavenEnabled
        {
            get
            {
                return ravenEnabled;
            }
            set
            {
                if (ravenEnabled != value)
                {
                    ravenEnabled = value;
                    RaisePropertyChanged(nameof(RavenEnabled));
                }
            }
        }
        private bool ravenEnabled;

        /// <summary>
        /// The key to use to toggle the raven spawning
        /// </summary>
        public Keys RavenToggleDebugKey
        {
            get
            {
                return ravenToggleDebugKey;
            }
            set
            {
                if (ravenToggleDebugKey != value)
                {
                    ravenToggleDebugKey = value;
                    RaisePropertyChanged(nameof(RavenToggleDebugKey));
                }
            }
        }
        private Keys ravenToggleDebugKey;

        /// <summary>
        /// The key to use to clear the ravens
        /// </summary>
        public Keys RavenClearDebugKey
        {
            get
            {
                return ravenClearDebugKey;
            }
            set
            {
                if (ravenClearDebugKey != value)
                {
                    ravenClearDebugKey = value;
                    RaisePropertyChanged(nameof(RavenClearDebugKey));
                }
            }
        }
        private Keys ravenClearDebugKey;

        /// <summary>
        /// The key to use to toggle sub mode
        /// </summary>
        public Keys RavenSubModeToggleKey
        {
            get
            {
                return ravenSubModeToggleKey;
            }
            set
            {
                if (ravenSubModeToggleKey != value)
                {
                    ravenSubModeToggleKey = value;
                    RaisePropertyChanged(nameof(RavenSubModeToggleKey));
                }
            }
        }
        private Keys ravenSubModeToggleKey;

        /// <summary>
        /// The trigger type we want to use for the ravens
        /// </summary>
        public RavenTriggerTypes RavenTriggerType
        {
            get
            {
                return ravenTriggerType;
            }
            set
            {
                if (ravenTriggerType != value)
                {
                    ravenTriggerType = value;
                    RaisePropertyChanged(nameof(RavenTriggerType));
                }
            }
        }
        private RavenTriggerTypes ravenTriggerType;

        /// <summary>
        /// The maximum number of ravens visible on the screen at once
        /// </summary>
        public string MaxRavensCount
        {
            get
            {
                return maxRavensCount.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxRavensCount = 0;
                }
                else
                {
                    if (int.TryParse(value, out int newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 100)
                        {
                            newVal = 100;
                        }
                        maxRavensCount = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxRavensCount));
            }
        }
        private int maxRavensCount;

        /// <summary>
        /// The ID of the Channel Point Reward to use for the Raven Trigger
        /// </summary>
        public string RavensChannelPointID
        {
            get
            {
                return ravensChannelPointID;
            }
            set
            {
                if (ravensChannelPointID != value)
                {
                    ravensChannelPointID = value;
                    RaisePropertyChanged(nameof(RavensChannelPointID));
                }
            }
        }
        private string ravensChannelPointID;

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
        /// A collection of excluded terms
        /// </summary>
        public ObservableCollection<string> ExcludedTerms
        {
            get
            {
                return excludedTerms;
            }
            set
            {
                if (excludedTerms != value)
                {
                    excludedTerms = value;
                    RaisePropertyChanged(nameof(ExcludedTerms));
                }
            }
        }
        private ObservableCollection<string> excludedTerms;

        /// <summary>
        /// The index of the selected item in the excluded items list
        /// </summary>
        public int SelectedExcludedItemIndex
        {
            get
            {
                return selectedExcludedItemIndex;
            }
            set
            {
                if (selectedExcludedItemIndex != value)
                {
                    selectedExcludedItemIndex = value;
                    RaisePropertyChanged(nameof(SelectedExcludedItemIndex));
                }
            }
        }
        private int selectedExcludedItemIndex;

        /// <summary>
        /// The candidate item to add to the excluded items list
        /// </summary>
        public string CandidateExcludedItem
        {
            get
            {
                return candidateExcludedItem;
            }
            set
            {
                if (candidateExcludedItem != value)
                {
                    candidateExcludedItem = value;
                    RaisePropertyChanged(nameof(CandidateExcludedItem));
                }
            }
        }
        private string candidateExcludedItem;

        /// <summary>
        /// A collection of raven insults
        /// </summary>
        public ObservableCollection<string> RavenInsults
        {
            get
            {
                return ravenInsults;
            }
            set
            {
                if (ravenInsults != value)
                {
                    ravenInsults = value;
                    RaisePropertyChanged(nameof(RavenInsults));
                }
            }
        }
        private ObservableCollection<string> ravenInsults;

        /// <summary>
        /// The index of the selected item in the Raven Insults list
        /// </summary>
        public int SelectedRavenInsultIndex
        {
            get
            {
                return selectedRavenInsultIndex;
            }
            set
            {
                if (selectedRavenInsultIndex != value)
                {
                    selectedRavenInsultIndex = value;
                    RaisePropertyChanged(nameof(SelectedRavenInsultIndex));
                }
            }
        }
        private int selectedRavenInsultIndex;

        /// <summary>
        /// The candidate item to add to the Raven Insults list
        /// </summary>
        public string CandidateRavenInsult
        {
            get
            {
                return candidateRavenInsult;
            }
            set
            {
                if (candidateRavenInsult != value)
                {
                    candidateRavenInsult = value;
                    RaisePropertyChanged(nameof(CandidateRavenInsult));
                }
            }
        }
        private string candidateRavenInsult;

        /// <summary>
        /// The number of ravens to spawn when the insult trigger is hit
        /// </summary>
        public string InsultRavenSpawnCount
        {
            get
            {
                return insultRavenSpawnCount.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    insultRavenSpawnCount = 0;
                }
                else
                {
                    if (int.TryParse(value, out int newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 100)
                        {
                            newVal = 100;
                        }
                        insultRavenSpawnCount = newVal;
                    }
                }
                RaisePropertyChanged(nameof(insultRavenSpawnCount));
            }
        }
        private int insultRavenSpawnCount;

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

            InitialiseCommands();

            GameDirectory = installerSettings.GetSettingOrDefault(JumpKingModInstallerSettingsContext.GameDirectoryKey, string.Empty);
            ModDirectory = installerSettings.GetSettingOrDefault(JumpKingModInstallerSettingsContext.ModDirectoryKey, string.Empty);

            ExcludedTerms = new ObservableCollection<string>();
            RavenInsults = new ObservableCollection<string>();
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
            AddExcludedTermCommand = new DelegateCommand(_ => { AddToCollection(ExcludedTerms, CandidateExcludedItem); });
            RemoveExcludedTermCommand = new DelegateCommand(_ => 
            { 
                RemoveFromCollection(ExcludedTerms, SelectedExcludedItemIndex);
                SelectedExcludedItemIndex = 0;
            });
            AddRavenInsultCommand = new DelegateCommand(_ => { AddToCollection(RavenInsults, CandidateRavenInsult); });
            RemoveRavenInsultCommand = new DelegateCommand(_ => 
            { 
                RemoveFromCollection(RavenInsults, SelectedRavenInsultIndex);
                SelectedRavenInsultIndex = 0;
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
                            // Go through each file, and move it over into the destination
                            for (int i = 0; i < localFiles.Count; i++)
                            {
                                string relativePath = localFiles[i].Replace(ModDirectory, "").Trim(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                                string dstFilePath = Path.Combine(expectedRemoteModFolder, relativePath);
                                Directory.CreateDirectory(Path.GetDirectoryName(dstFilePath));
                                File.Copy(localFiles[i], dstFilePath, true);
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

            if (RavenEnabled && 
                (RavenTriggerType == RavenTriggerTypes.ChannelPointReward || RavenTriggerType == RavenTriggerTypes.ChatMessage) && 
                ChatDisplayEnabled)
            {
                MessageBoxResult result = MessageBox.Show($"If Chat Display is active, the Chat-based Raven Triggers will not function. Are you sure you want to proceed?", "Setting Conflict!", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel || result == MessageBoxResult.No || result == MessageBoxResult.None)
                {
                    return;
                }
            }

            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, TwitchAccountName);
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.OAuthKey, TwitchOAuth);

            // Ravens
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensEnabledKey, RavenEnabled.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensToggleDebugKeyKey, RavenToggleDebugKey.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensClearDebugKeyKey, RavenClearDebugKey.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensSubModeToggleKeyKey, RavenSubModeToggleKey.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavensMaxCountKey, MaxRavensCount);
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavenTriggerTypeKey, RavenTriggerType.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavenChannelPointRewardIDKey, RavensChannelPointID);

            // Chat Display
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.TwitchRelayEnabledKey, ChatDisplayEnabled.ToString());

            // Free Fly
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.FreeFlyEnabledKey, FreeFlyingEnabled.ToString());
            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.FreeFlyToggleKeyKey, FreeFlyToggleKey.ToString());

            // Exclusion List
            string expectedExclusionPath = Path.Combine(GameDirectory, JumpKingModSettingsContext.ExcludedTermFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expectedExclusionPath));
            File.WriteAllLines(expectedExclusionPath, ExcludedTerms);

            // Raven Insults
            string expectedRavenInsultsPath = Path.Combine(GameDirectory, JumpKingModSettingsContext.RavenInsultsFilePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expectedRavenInsultsPath));
            File.WriteAllLines(expectedRavenInsultsPath, RavenInsults);

            ModSettings.SetOrCreateSetting(JumpKingModSettingsContext.RavenInsultSpawnCountKey, InsultRavenSpawnCount.ToString());

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
                // Twitch Info
                TwitchAccountName = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.ChatListenerTwitchAccountNameKey, string.Empty);
                TwitchOAuth = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.OAuthKey, string.Empty);

                // Raven Info
                RavenEnabled = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensEnabledKey, true);
                RavenClearDebugKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensClearDebugKeyKey, Keys.F2);
                RavenToggleDebugKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensToggleDebugKeyKey, Keys.F3);
                RavenSubModeToggleKey = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensSubModeToggleKeyKey, Keys.F4);
                MaxRavensCount = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensMaxCountKey, 5.ToString());
                RavenTriggerType = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenTriggerTypeKey, RavenTriggerTypes.ChatMessage);
                RavensChannelPointID = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenChannelPointRewardIDKey, string.Empty);
                InsultRavenSpawnCount = ModSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenInsultSpawnCountKey, 3.ToString());

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
            ExcludedTerms = new ObservableCollection<string>(excludedTerms);

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
            RavenInsults = new ObservableCollection<string>(ravenInsultsFileContent);
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
