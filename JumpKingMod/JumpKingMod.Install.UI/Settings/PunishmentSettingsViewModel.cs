using JumpKingMod.Install.UI.API;
using JumpKingRavensMod.Install.UI;
using JumpKingPunishmentMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using Settings;
using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace JumpKingMod.Install.UI.Settings
{
    /// <summary>
    /// An aggregate class of Punishment Mod Settings
    /// </summary>
    public class PunishmentSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

        /// <summary>
        /// Whether the Punishment mod is enabled or not
        /// </summary>
        public bool PunishmentModEnabled
        {
            get
            {
                return punishmentModEnabled;
            }
            set
            {
                if (punishmentModEnabled != value)
                {
                    punishmentModEnabled = value;
                    RaisePropertyChanged(nameof(PunishmentModEnabled));
                    RaisePropertyChanged(nameof(PunishmentSettingsVisible));
                    RaisePropertyChanged(nameof(RewardSettingsVisible));

                }
            }
        }
        private bool punishmentModEnabled;

        /// <summary>
        /// The key to use to toggle Punishment functionality during gameplay
        /// </summary>
        public Keys PunishmentToggleDebugKey
        {
            get
            {
                return punishmentToggleDebugKey;
            }
            set
            {
                if (punishmentToggleDebugKey != value)
                {
                    punishmentToggleDebugKey = value;
                    RaisePropertyChanged(nameof(PunishmentToggleDebugKey));
                }
            }
        }
        private Keys punishmentToggleDebugKey;

        /// <summary>
        /// The key to use to send a test feedback event to your feedback device
        /// </summary>
        public Keys PunishmentTestFeedbackDebugKey
        {
            get
            {
                return punishmentTestFeedbackDebugKey;
            }
            set
            {
                if (punishmentTestFeedbackDebugKey != value)
                {
                    punishmentTestFeedbackDebugKey = value;
                    RaisePropertyChanged(nameof(PunishmentTestFeedbackDebugKey));
                }
            }
        }
        private Keys punishmentTestFeedbackDebugKey;

        /// <summary>
        /// The on screen display behavior we want to use
        /// </summary>
        public PunishmentOnScreenDisplayBehavior OnScreenDisplayBehavior
        {
            get
            {
                return onScreenDisplayBehavior;
            }
            set
            {
                if (onScreenDisplayBehavior != value)
                {
                    onScreenDisplayBehavior = value;
                    RaisePropertyChanged(nameof(OnScreenDisplayBehavior));
                }
            }
        }
        private PunishmentOnScreenDisplayBehavior onScreenDisplayBehavior;

        /// <summary>
        /// Whether calculated durations should round to the nearest second
        /// </summary>
        public bool RoundDurations
        {
            get
            {
                return roundDurations;
            }
            set
            {
                if (roundDurations != value)
                {
                    roundDurations = value;
                    RaisePropertyChanged(nameof(RoundDurations));
                }
            }
        }
        private bool roundDurations;

        /// <summary>
        /// Whether punishments are enabled or not
        /// </summary>
        public bool EnabledPunishment
        {
            get
            {
                return enabledPunishment;
            }
            set
            {
                if (enabledPunishment != value)
                {
                    enabledPunishment = value;
                    RaisePropertyChanged(nameof(EnabledPunishment));
                    RaisePropertyChanged(nameof(PunishmentSettingsVisible));
                }
            }
        }
        private bool enabledPunishment;

        public bool PunishmentSettingsVisible
        {
            get
            {
                return enabledPunishment && punishmentModEnabled;
            }
        }

        /// <summary>
        /// The minimum punishment duration
        /// </summary>
        public string MinPunishmentDuration
        {
            get
            {
                return minPunishmentDuration.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    minPunishmentDuration = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        minPunishmentDuration = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MinPunishmentDuration));
            }
        }
        private float minPunishmentDuration;

        /// <summary>
        /// The minimum punishment intensity
        /// </summary>
        public string MinPunishmentIntensity
        {
            get
            {
                return minPunishmentIntensity.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    minPunishmentIntensity = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        minPunishmentIntensity = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MinPunishmentIntensity));
            }
        }
        private float minPunishmentIntensity;

        /// <summary>
        /// The maximum punishment duration
        /// </summary>
        public string MaxPunishmentDuration
        {
            get
            {
                return maxPunishmentDuration.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxPunishmentDuration = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        maxPunishmentDuration = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxPunishmentDuration));
            }
        }
        private float maxPunishmentDuration;

        /// <summary>
        /// The maximum punishment intensity
        /// </summary>
        public string MaxPunishmentIntensity
        {
            get
            {
                return maxPunishmentIntensity.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxPunishmentIntensity = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        maxPunishmentIntensity = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxPunishmentIntensity));
            }
        }
        private float maxPunishmentIntensity;

        /// <summary>
        /// The minimum fall distance to receive a punishment
        /// </summary>
        public string MinFallDistance
        {
            get
            {
                return minFallDistance.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    minFallDistance = 0.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        minFallDistance = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MinFallDistance));
            }
        }
        private float minFallDistance;

        /// <summary>
        /// The fall distance at which you will receive the maximum punishment
        /// </summary>
        public string MaxFallDistance
        {
            get
            {
                return maxFallDistance.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxFallDistance = 1000.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        maxFallDistance = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxFallDistance));
            }
        }
        private float maxFallDistance;

        /// <summary>
        /// Whether easy mode punishments are enabled (vibrate instead of shock)
        /// </summary>
        public bool PunishmentEasyMode
        {
            get
            {
                return punishmentEasyMode;
            }
            set
            {
                if (punishmentEasyMode != value)
                {
                    punishmentEasyMode = value;
                    RaisePropertyChanged(nameof(PunishmentEasyMode));
                }
            }
        }
        private bool punishmentEasyMode;

        /// <summary>
        /// Whether rewards are enabled or not
        /// </summary>
        public bool EnabledRewards
        {
            get
            {
                return enabledRewards;
            }
            set
            {
                if (enabledRewards != value)
                {
                    enabledRewards = value;
                    RaisePropertyChanged(nameof(EnabledRewards));
                    RaisePropertyChanged(nameof(RewardSettingsVisible));
                }
            }
        }
        private bool enabledRewards;

        public bool RewardSettingsVisible
        {
            get
            {
                return enabledRewards && punishmentModEnabled;
            }
        }

        /// <summary>
        /// The minimum shock duration when receiving a reward
        /// </summary>
        public string MinRewardDuration
        {
            get
            {
                return minRewardDuration.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    minRewardDuration = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        minRewardDuration = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MinRewardDuration));
            }
        }
        private float minRewardDuration;

        /// <summary>
        /// The minimum shock intensity when receiving a reward
        /// </summary>
        public string MinRewardIntensity
        {
            get
            {
                return minRewardIntensity.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    minRewardIntensity = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        minRewardIntensity = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MinRewardIntensity));
            }
        }
        private float minRewardIntensity;

        /// <summary>
        /// The maximum shock duration when receiving a reward
        /// </summary>
        public string MaxRewardDuration
        {
            get
            {
                return maxRewardDuration.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxRewardDuration = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        maxRewardDuration = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxRewardDuration));
            }
        }
        private float maxRewardDuration;

        /// <summary>
        /// The maximum shock intensity when receiving a reward
        /// </summary>
        public string MaxRewardIntensity
        {
            get
            {
                return maxRewardIntensity.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxRewardIntensity = 1.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        maxRewardIntensity = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxRewardIntensity));
            }
        }
        private float maxRewardIntensity;

        /// <summary>
        /// The minimum progress amount to receive a reward
        /// </summary>
        public string MinRewardDistance
        {
            get
            {
                return minRewardDistance.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    minRewardDistance = 0.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        minRewardDistance = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MinRewardDistance));
            }
        }
        private float minRewardDistance;

        /// <summary>
        /// The progress amount at which you will receive the maximum reward
        /// </summary>
        public string MaxRewardDistance
        {
            get
            {
                return maxRewardDistance.ToString();
            }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    maxRewardDistance = 1000.0f;
                }
                else
                {
                    if (float.TryParse(value, out float newVal))
                    {
                        maxRewardDistance = newVal;
                    }
                }
                RaisePropertyChanged(nameof(MaxRewardDistance));
            }
        }
        private float maxRewardDistance;

        /// <summary>
        /// Whether progress only mode is enabled for rewards (only rewarded when getting new progress)
        /// </summary>
        public bool RewardProgressOnlyMode
        {
            get
            {
                return rewardProgressOnlyMode;
            }
            set
            {
                if (rewardProgressOnlyMode != value)
                {
                    rewardProgressOnlyMode = value;
                    RaisePropertyChanged(nameof(RewardProgressOnlyMode));
                }
            }
        }
        private bool rewardProgressOnlyMode;

        /// <summary>
        /// The <see cref="UserSettings"/> object for the Punishment Mod settings
        /// </summary>
        public UserSettings PunishmentSettings
        {
            get
            {
                return punishmentSettings;
            }
            private set
            {
                punishmentSettings = value;
                RaisePropertyChanged(nameof(PunishmentSettings));
                RaisePropertyChanged(nameof(ArePunishmentSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings punishmentSettings;

        /// <summary>
        /// Returns whether the PiShock settings are currently populated
        /// </summary>
        public bool ArePunishmentSettingsLoaded
        {
            get
            {
                return PunishmentSettings != null;
            }
        }

        /// <summary>
        /// Constructor for creating a <see cref="PunishmentSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        /// <exception cref="ArgumentNullException"></exception>
        public PunishmentSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Creates or loads the Punishment mod settings from the given install directory
        /// </summary>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load Punishment mod settings as provided Game Directory was empty!");
                return false;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, JumpKingPunishmentModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (PunishmentSettings == null)
                {
                    PunishmentSettings = new UserSettings(expectedSettingsFilePath, JumpKingPunishmentModSettingsContext.GetDefaultSettings(), logger);
                }

                // Enable
                PunishmentModEnabled = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentModEnabledKey, true);

                // General
                PunishmentToggleDebugKey = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentModToggleKeyKey, Keys.F8);
                PunishmentTestFeedbackDebugKey = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentFeedbackTestKeyKey, Keys.F9);
                OnScreenDisplayBehavior = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.OnScreenDisplayBehaviorKey, PunishmentOnScreenDisplayBehavior.FeedbackIntensityAndDuration);
                RoundDurations = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.RoundDurationsKey, false);

                // Punishment
                EnabledPunishment = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.EnablePunishmentKey, true);
                MinPunishmentDuration = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture));
                MinPunishmentIntensity = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentIntensityKey, 1.0f.ToString(CultureInfo.InvariantCulture));
                MaxPunishmentDuration = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture));
                MaxPunishmentIntensity = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentIntensityKey, 15.0f.ToString(CultureInfo.InvariantCulture));
                MinFallDistance = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentFallDistanceKey, 150.0f.ToString(CultureInfo.InvariantCulture));
                MaxFallDistance = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentfallDistanceKey, 1000.0f.ToString(CultureInfo.InvariantCulture));
                PunishmentEasyMode = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentEasyModeKey, false);

                // Rewards
                EnabledRewards = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.EnableRewardsKey, false);
                MinRewardDuration = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture));
                MinRewardIntensity = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardIntensityKey, 10.0f.ToString(CultureInfo.InvariantCulture));
                MaxRewardDuration = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture));
                MaxRewardIntensity = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardIntenityKey, 100.0f.ToString(CultureInfo.InvariantCulture));
                MinRewardDistance = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardProgressDistanceKey, 0.0f.ToString(CultureInfo.InvariantCulture));
                MaxRewardDistance = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardProgressDistanceKey, 150.0f.ToString(CultureInfo.InvariantCulture));
                RewardProgressOnlyMode = PunishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.RewardProgressOnlyKey, true);

                ValidateSettings();
            }
            else
            {
                logger.Error($"Failed to load Punishment mod settings as the settings file couldn't be found at '{expectedSettingsFilePath}'");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Saves the Punishment Mod settings back to disk
        /// </summary>
        /// <param name="gameDirectory"></param>
        public bool SaveSettings(string gameDirectory, string modFolder)
        {
            if (PunishmentSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save Punishment mod settings, either internal settings object ({PunishmentSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            ValidateSettings();

            // Enable
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.PunishmentModEnabledKey, PunishmentModEnabled.ToString());

            //General
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.PunishmentModToggleKeyKey, PunishmentToggleDebugKey.ToString());
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.PunishmentFeedbackTestKeyKey, PunishmentTestFeedbackDebugKey.ToString());
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.OnScreenDisplayBehaviorKey, OnScreenDisplayBehavior.ToString());
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.RoundDurationsKey, RoundDurations.ToString());

            // Punishment
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.EnablePunishmentKey, EnabledPunishment.ToString());
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MinPunishmentDurationKey, MinPunishmentDuration);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MinPunishmentIntensityKey, MinPunishmentIntensity);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MaxPunishmentDurationKey, MaxPunishmentDuration);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MaxPunishmentIntensityKey, MaxPunishmentIntensity);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MinPunishmentFallDistanceKey, MinFallDistance);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MaxPunishmentfallDistanceKey, MaxFallDistance);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.PunishmentEasyModeKey, PunishmentEasyMode.ToString());

            // Rewards
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.EnableRewardsKey, EnabledRewards.ToString());
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MinRewardDurationKey, MinRewardDuration);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MinRewardIntensityKey, MinRewardIntensity);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MaxRewardDurationKey, MaxRewardDuration);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MaxRewardIntenityKey, MaxRewardIntensity);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MinRewardProgressDistanceKey, MinRewardDistance);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.MaxRewardProgressDistanceKey, MaxRewardDistance);
            PunishmentSettings.SetOrCreateSetting(JumpKingPunishmentModSettingsContext.RewardProgressOnlyKey, RewardProgressOnlyMode.ToString());

            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return ArePunishmentSettingsLoaded;
        }

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Clamps and validates settings (updating them if they are invalid)
        /// </summary>
        private void ValidateSettings()
        {
            // Clamp min/maxes
            if (minFallDistance > maxFallDistance)
            {
                MaxFallDistance = MinFallDistance;
            }
            if (minPunishmentDuration > maxPunishmentDuration)
            {
                MaxPunishmentDuration = MinPunishmentDuration;
            }
            if (minPunishmentIntensity > maxPunishmentIntensity)
            {
                MaxPunishmentIntensity = MinPunishmentIntensity;
            }
            if (minRewardDistance > maxRewardDistance)
            {
                MaxRewardDistance = MinRewardDistance;
            }
            if (minRewardDuration > maxRewardDuration)
            {
                MaxRewardDuration = MinRewardDuration;
            }
            if (minRewardIntensity > maxRewardIntensity)
            {
                MaxRewardIntensity = MinRewardIntensity;
            }

            // Clamp to valid ranges
            float clampedMinPunishmentDuration = Math.Max(minPunishmentDuration, 0.0f);
            clampedMinPunishmentDuration = Math.Min(clampedMinPunishmentDuration, 10.0f);
            if (minPunishmentDuration != clampedMinPunishmentDuration)
            {
                MinPunishmentDuration = clampedMinPunishmentDuration.ToString();
            }
            float clampedMinPunishmentIntensity = Math.Max(minPunishmentIntensity, 0.0f);
            clampedMinPunishmentIntensity = Math.Min(clampedMinPunishmentIntensity, 100.0f);
            if (minPunishmentIntensity != clampedMinPunishmentIntensity)
            {
                MinPunishmentIntensity = clampedMinPunishmentIntensity.ToString();
            }
            float clampedMaxPunishmentDuration = Math.Max(maxPunishmentDuration, 0.0f);
            clampedMaxPunishmentDuration = Math.Min(clampedMaxPunishmentDuration, 10.0f);
            if (maxPunishmentDuration != clampedMaxPunishmentDuration)
            {
                MaxPunishmentDuration = clampedMaxPunishmentDuration.ToString();
            }
            float clampedMaxPunishmentIntensity = Math.Max(maxPunishmentIntensity, 0.0f);
            clampedMaxPunishmentIntensity = Math.Min(clampedMaxPunishmentIntensity, 100.0f);
            if (maxPunishmentIntensity != clampedMaxPunishmentIntensity)
            {
                MaxPunishmentIntensity = clampedMaxPunishmentIntensity.ToString();
            }

            float clampedMinRewardDuration = Math.Max(minRewardDuration, 0.0f);
            clampedMinRewardDuration = Math.Min(clampedMinRewardDuration, 10.0f);
            if (minRewardDuration != clampedMinRewardDuration)
            {
                MinRewardDuration = clampedMinRewardDuration.ToString();
            }
            float clampedMinRewardIntensity = Math.Max(minRewardIntensity, 0.0f);
            clampedMinRewardIntensity = Math.Min(clampedMinRewardIntensity, 100.0f);
            if (minRewardIntensity != clampedMinRewardIntensity)
            {
                MinRewardIntensity = clampedMinRewardIntensity.ToString();
            }
            float clampedMaxRewardDuration = Math.Max(maxRewardDuration, 0.0f);
            clampedMaxRewardDuration = Math.Min(clampedMaxRewardDuration, 10.0f);
            if (maxRewardDuration != clampedMaxRewardDuration)
            {
                MaxRewardDuration = clampedMaxRewardDuration.ToString();
            }
            float clampedMaxRewardIntensity = Math.Max(maxRewardIntensity, 0.0f);
            clampedMaxRewardIntensity = Math.Min(clampedMaxRewardIntensity, 100.0f);
            if (maxRewardIntensity != clampedMaxRewardIntensity)
            {
                MaxRewardIntensity = clampedMaxRewardIntensity.ToString();
            }
        }
    }
}
