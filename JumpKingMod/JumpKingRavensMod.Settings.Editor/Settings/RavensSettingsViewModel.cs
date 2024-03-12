using JumpKingRavensMod.Settings;
using JumpKingRavensMod.Settings.Editor.API;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.YouTube.Settings;
using Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace JumpKingRavensMod.Settings.Editor
{
    /// <summary>
    /// An aggregate class of all settings for the Ravens Mod
    /// </summary>
    public class RavensSettingsViewModel : INotifyPropertyChanged, ISettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ILogger logger;

        #region Commands
        /// <summary>
        /// Add an excluded term to the ravens settings
        /// </summary>
        public ICommand AddExcludedTermCommand { get; private set; }

        /// <summary>
        /// Remove an excluded term from the ravens settings
        /// </summary>
        public ICommand RemoveExcludedTermCommand { get; private set; }

        /// <summary>
        /// Adds an insult to the ravens settings
        /// </summary>
        public ICommand AddRavenInsultCommand { get; private set; }

        /// <summary>
        /// Removes an insult from the ravens settings
        /// </summary>
        public ICommand RemoveRavenInsultCommand { get; private set; }
        #endregion

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
                    RaisePropertyChanged(nameof(GunSettingsVisible));
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
        /// The trigger type we want to use for the ravens on Twitch
        /// </summary>
        public TwitchRavenTriggerTypes RavenTriggerType
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
        private TwitchRavenTriggerTypes ravenTriggerType;

        /// <summary>
        /// The trigger type we want to use for the ravens on YouTube
        /// </summary>
        public YouTubeRavenTriggerTypes YouTubeRavenTriggerType
        {
            get
            {
                return youTubeRavenTriggerType;
            }
            set
            {
                if (youTubeRavenTriggerType != value)
                {
                    youTubeRavenTriggerType = value;
                    RaisePropertyChanged(nameof(YouTubeRavenTriggerType));
                }
            }
        }
        private YouTubeRavenTriggerTypes youTubeRavenTriggerType;

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
        /// The time a raven should spend displaying a message in seconds
        /// </summary>
        public string MessageDurationInSeconds
        {
            get
            {
                return messageDurationInSeconds.ToString(CultureInfo.InvariantCulture);
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    messageDurationInSeconds = DefaultMessageDurationInSeconds;
                }
                else
                {
                    if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out float newVal))
                    {
                        if (newVal < 0)
                        {
                            newVal = Math.Abs(newVal);
                        }
                        if (newVal > 100)
                        {
                            newVal = 100;
                        }
                        messageDurationInSeconds = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MessageDurationInSeconds));
            }
        }
        private float messageDurationInSeconds;
        private const float DefaultMessageDurationInSeconds = 3.0f;

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
                RaisePropertyChanged(nameof(InsultRavenSpawnCount));
            }
        }
        private int insultRavenSpawnCount;

        /// <summary>
        /// Whether the Gun mode can be enabled or not
        /// </summary>
        public bool GunEnabled
        {
            get
            {
                return gunEnabled;
            }
            set
            {
                if (gunEnabled != value)
                {
                    gunEnabled = value;
                    RaisePropertyChanged(nameof(GunEnabled));
                    RaisePropertyChanged(nameof(GunSettingsVisible));
                }
            }
        }
        private bool gunEnabled;

        public bool GunSettingsVisible
        {
            get
            {
                return gunEnabled && ravenEnabled;
            }
        }

        /// <summary>
        /// The key to press to toggle the gun mode
        /// </summary>
        public Keys GunToggleKey
        {
            get
            {
                return gunToggleKey;
            }
            set
            {
                if (gunToggleKey != value)
                {
                    gunToggleKey = value;
                    RaisePropertyChanged(nameof(GunToggleKey));
                }
            }
        }
        private Keys gunToggleKey;


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
        /// The <see cref="UserSettings"/> object for the Ravens Mod settings
        /// </summary>
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
            }
        }
        private UserSettings ravenModSettings;

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
        /// Constructor for creating a <see cref="RavensSettingsViewModel"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RavensSettingsViewModel(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ExcludedTerms = new ObservableCollection<string>();
            RavenInsults = new ObservableCollection<string>();
        }

        /// <summary>
        /// Initialise the Raven Settings related commands
        /// </summary>
        public void InitialiseCommands()
        {
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
        /// Creates or loads the Raven mod settings from the given install directory
        /// </summary>
        public bool LoadSettings(string gameDirectory, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load Raven settings as provided Game Directory was empty!");
                return false;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, JumpKingRavensModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (RavenModSettings == null)
                {
                    RavenModSettings = new UserSettings(expectedSettingsFilePath, JumpKingRavensModSettingsContext.GetDefaultSettings(), logger);
                }

                // YouTube Info
                YouTubeRavenTriggerType = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.YouTubeRavenTriggerTypeKey, YouTubeRavenTriggerTypes.ChatMessage);

                // Twitch Info
                RavenTriggerType = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenTriggerTypeKey, TwitchRavenTriggerTypes.ChatMessage);
                RavensChannelPointID = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenChannelPointRewardIDKey, string.Empty);

                // Raven Info
                RavenEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensEnabledKey, true);
                RavenClearDebugKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensClearDebugKeyKey, Keys.F2);
                RavenToggleDebugKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensToggleDebugKeyKey, Keys.F3);
                RavenSubModeToggleKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensSubModeToggleKeyKey, Keys.F4);
                MaxRavensCount = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensMaxCountKey, 5.ToString());
                MessageDurationInSeconds = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavensDisplayTimeInSecondsKey, 3.0f.ToString(CultureInfo.InvariantCulture));
                InsultRavenSpawnCount = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.RavenInsultSpawnCountKey, 3.ToString());

                // Chat Display Info
                ChatDisplayEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.TwitchRelayEnabledKey, false);

                // Free Fly
                FreeFlyingEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.FreeFlyEnabledKey, false);
                FreeFlyToggleKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.FreeFlyToggleKeyKey, Keys.F1);

                // Gun
                GunEnabled = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.GunEnabledKey, false);
                GunToggleKey = RavenModSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.GunToggleKeyKey, Keys.F8);
            }
            else
            {
                logger.Error($"Failed to load Raven settings as the settings file couldnt be found at '{expectedSettingsFilePath}'");
                return false;
            }

            // Load in Exclusion List
            List<string> excludedTerms = new List<string>();
            string expectedExclusionPath = Path.Combine(gameDirectory, JumpKingRavensModSettingsContext.ExcludedTermFilePath);
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
                return false;
            }
            ExcludedTerms = new ObservableCollection<string>(excludedTerms);

            // Load in Raven Insults
            List<string> ravenInsultsFileContent = new List<string>();
            string expectedRavenInsultsPath = Path.Combine(gameDirectory, JumpKingRavensModSettingsContext.RavenInsultsFilePath);
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
                return false;
            }
            RavenInsults = new ObservableCollection<string>(ravenInsultsFileContent);
            return true;
        }

        public bool SaveSettings(string gameDirectory)
        {
            if (RavenModSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save raven settings, either internal settings object ({RavenModSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            // YouTube
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.YouTubeRavenTriggerTypeKey, YouTubeRavenTriggerType.ToString());

            // Twitch
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavenTriggerTypeKey, RavenTriggerType.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavenChannelPointRewardIDKey, RavensChannelPointID);

            // Raven Info
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensEnabledKey, RavenEnabled.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensClearDebugKeyKey, RavenClearDebugKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensToggleDebugKeyKey, RavenToggleDebugKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensSubModeToggleKeyKey, RavenSubModeToggleKey.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensMaxCountKey, MaxRavensCount);
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavensDisplayTimeInSecondsKey, MessageDurationInSeconds);
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.RavenInsultSpawnCountKey, InsultRavenSpawnCount);

            // Chat display info
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.TwitchRelayEnabledKey, ChatDisplayEnabled.ToString());

            // Free fly
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.FreeFlyEnabledKey, FreeFlyingEnabled.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.FreeFlyToggleKeyKey, FreeFlyToggleKey.ToString());

            // Gun
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.GunEnabledKey, GunEnabled.ToString());
            RavenModSettings.SetOrCreateSetting(JumpKingRavensModSettingsContext.GunToggleKeyKey, GunToggleKey.ToString());

            try
            {
                // Exclusion List
                string expectedExclusionPath = Path.Combine(gameDirectory, JumpKingRavensModSettingsContext.ExcludedTermFilePath);
                Directory.CreateDirectory(Path.GetDirectoryName(expectedExclusionPath));
                File.WriteAllLines(expectedExclusionPath, ExcludedTerms);

                // Raven Insults
                string expectedRavenInsultsPath = Path.Combine(gameDirectory, JumpKingRavensModSettingsContext.RavenInsultsFilePath);
                Directory.CreateDirectory(Path.GetDirectoryName(expectedRavenInsultsPath));
                File.WriteAllLines(expectedRavenInsultsPath, RavenInsults);
            }
            catch (Exception e)
            {
                logger.Error($"Encountered exception when saving exclusion list or insult list: {e.ToString()}");
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return AreRavenModSettingsLoaded;
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
