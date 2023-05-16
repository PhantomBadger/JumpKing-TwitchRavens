using JumpKingMod.Install.UI.API;
using JumpKingModifiersMod.Settings;
using JumpKingRavensMod.Install.UI;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Install.UI.Settings
{
    /// <summary>
    /// An aggregate of all settings for the Modifiers Mod
    /// </summary>
    public class ModifiersSettingsViewModel : INotifyPropertyChanged, IInstallerSettingsViewModel
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DelegateCommand updateSettingsCommand;
        private readonly DelegateCommand loadSettingsCommand;
        private readonly ILogger logger;

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
        /// Returns whether the fall damage mod settings are currently populated
        /// </summary>
        public bool AreFallDamageModSettingsLoaded
        {
            get
            {
                return ModifiersModSettings != null;
            }
        }

        /// <summary>
        /// The <see cref="UserSettings"/> object for the Fall Damage Mod settings
        /// </summary>
        public UserSettings ModifiersModSettings
        {
            get
            {
                return fallDamageModSettings;
            }
            private set
            {
                fallDamageModSettings = value;
                RaisePropertyChanged(nameof(ModifiersModSettings));
                RaisePropertyChanged(nameof(AreFallDamageModSettingsLoaded));
                updateSettingsCommand.RaiseCanExecuteChanged();
                loadSettingsCommand.RaiseCanExecuteChanged();
            }
        }
        private UserSettings fallDamageModSettings;

        /// <summary>
        /// Ctor for creating a <see cref="ModifiersSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ModifiersSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool LoadSettings(string gameDirectory, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load Modifier settings as provided Game Directory was empty!");
                return false;
            }

            // Load in the settings
            string expectedSettingsFilePath = Path.Combine(gameDirectory, JumpKingModifiersModSettingsContext.SettingsFileName);
            if (File.Exists(expectedSettingsFilePath) || createIfDoesntExist)
            {
                if (ModifiersModSettings == null)
                {
                    ModifiersModSettings = new UserSettings(expectedSettingsFilePath, JumpKingModifiersModSettingsContext.GetDefaultSettings(), logger);
                }

                // Load the initial data
                FallDamageEnabled = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageEnabledKey, false);
                FallDamageToggleKey = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerFallDamageToggleKeyKey, Keys.F11);
                FallDamageModifier = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageModifierKey, JumpKingModifiersModSettingsContext.DefaultFallDamageModifier).ToString(CultureInfo.InvariantCulture);
                FallDamageBloodSplatEnabled = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageBloodEnabledKey, true);
                FallDamageClearBloodKey = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageClearBloodKey, Keys.F10);
                FallDamageNiceSpawns = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageNiceSpawnsKey, true);

                ManualResizingEnabled = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeEnabledKey, false);
                ManualResizingToggleKey = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerManualResizeToggleKey, Keys.F9);
                ManualResizingGrowKey = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeGrowKeyKey, Keys.Up);
                ManualResizingShrinkKey = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeShrinkKeyKey, Keys.Down);

                RisingLavaEnabled = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaEnabledKey, false);
                RisingLavaToggleKey = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerLavaRisingToggleKeyKey, Keys.F7);
                RisingLavaSpeed = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaSpeedKey, JumpKingModifiersModSettingsContext.DefaultRisingLavaSpeed.ToString(CultureInfo.InvariantCulture));
                RisingLavaNiceSpawns = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaNiceSpawnsKey, true);
                return true;
            }
            else
            {
                logger.Error($"Failed to load Modifier settings as the settings file couldnt be found at '{expectedSettingsFilePath}'");
                return false;
            }
        }

        /// <inheritdoc/>
        public bool SaveSettings(string gameDirectory)
        {
            if (ModifiersModSettings == null || string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error($"Failed to save modifier settings, either internal settings object ({ModifiersModSettings}) is null, or no game directory was provided ({gameDirectory})");
                return false;
            }

            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageEnabledKey, FallDamageEnabled.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.DebugTriggerFallDamageToggleKeyKey, FallDamageToggleKey.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageModifierKey, FallDamageModifier);
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageBloodEnabledKey, FallDamageBloodSplatEnabled.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageClearBloodKey, FallDamageClearBloodKey.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.FallDamageNiceSpawnsKey, FallDamageNiceSpawns.ToString());

            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.ManualResizeEnabledKey, ManualResizingEnabled.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.DebugTriggerManualResizeToggleKey, ManualResizingToggleKey.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.ManualResizeGrowKeyKey, ManualResizingGrowKey.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.ManualResizeShrinkKeyKey, manualResizingShrinkKey.ToString());

            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.RisingLavaEnabledKey, RisingLavaEnabled.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.DebugTriggerLavaRisingToggleKeyKey, RisingLavaToggleKey.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.RisingLavaSpeedKey, RisingLavaSpeed.ToString());
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.RisingLavaNiceSpawnsKey, RisingLavaNiceSpawns.ToString());
            return true;
        }

        /// <inheritdoc/>
        public bool AreSettingsLoaded()
        {
            return AreFallDamageModSettingsLoaded;
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
