using JumpKingMod.Install.UI.API;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Settings.ViewModels;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

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
        private readonly List<ConfigurableModifierViewModel> modifierViewModels;
        private readonly StackPanel modifiersStackPanel;

        #region Properties

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

        #endregion

        /// <summary>
        /// Ctor for creating a <see cref="ModifiersSettingsViewModel"/>
        /// </summary>
        /// <param name="updateSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the updating of settings</param>
        /// <param name="loadSettingsCommand">A <see cref="DelegateCommand"/> in the UI for handling the loading of settings</param>
        /// <exception cref="ArgumentNullException"></exception>
        public ModifiersSettingsViewModel(DelegateCommand updateSettingsCommand, DelegateCommand loadSettingsCommand, ILogger logger,
            StackPanel modifiersStackPanel)
        {
            this.updateSettingsCommand = updateSettingsCommand ?? throw new ArgumentNullException(nameof(updateSettingsCommand));
            this.loadSettingsCommand = loadSettingsCommand ?? throw new ArgumentNullException(nameof(loadSettingsCommand));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.modifierViewModels = new List<ConfigurableModifierViewModel>();
            this.modifiersStackPanel = modifiersStackPanel ?? throw new ArgumentNullException(nameof(modifiersStackPanel));
        }

        /// <inheritdoc/>
        public bool LoadSettings(string gameDirectory, string modFolder, bool createIfDoesntExist)
        {
            if (string.IsNullOrWhiteSpace(gameDirectory))
            {
                logger.Error("Failed to load Modifier settings as provided Game Directory was empty!");
                return false;
            }

            // Create dynamic controls for modifiers
            string modifiersDLLPath = Path.Combine(modFolder, "JumpKingModifiersMod.dll");
            CreateModifiersUIControls(modifiersStackPanel, modifiersDLLPath);

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

                string rawEnabledModifiers = ModifiersModSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.EnabledModifiersKey, "");
                HashSet<string> enabledModifiers = new HashSet<string>(rawEnabledModifiers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
                for (int i = 0; i < modifierViewModels.Count; i++)
                {
                    ConfigurableModifierViewModel configurableModifier = modifierViewModels[i];
                    configurableModifier.ModifierEnabled = enabledModifiers.Contains(configurableModifier.ModifierType.ToString());

                    for (int j = 0; j < configurableModifier.ModifierSettings.Count; j++)
                    {
                        ModifierSettingViewModel modifierSetting = configurableModifier.ModifierSettings[j];
                        switch (modifierSetting.SettingType)
                        {
                            case ModifierSettingType.Bool:
                                modifierSetting.BoolSettingValue = ModifiersModSettings.GetSettingOrDefault(modifierSetting.SettingKey, (bool)modifierSetting.DefaultSettingValue);
                                break;
                            case ModifierSettingType.String:
                                modifierSetting.StringSettingValue = ModifiersModSettings.GetSettingOrDefault(modifierSetting.SettingKey, (string)modifierSetting.DefaultSettingValue);
                                break;
                            case ModifierSettingType.Float:
                                modifierSetting.FloatSettingValue = ModifiersModSettings.GetSettingOrDefault(modifierSetting.SettingKey, ((float)modifierSetting.DefaultSettingValue).ToString());
                                break;
                            case ModifierSettingType.Int:
                                modifierSetting.IntSettingValue = ModifiersModSettings.GetSettingOrDefault(modifierSetting.SettingKey, ((int)modifierSetting.DefaultSettingValue).ToString());
                                break;
                            case ModifierSettingType.Enum:
                                modifierSetting.EnumSettingValue = ModifiersModSettings.GetSettingOrDefault(modifierSetting.SettingKey, (Enum)modifierSetting.DefaultSettingValue, modifierSetting.EnumType).ToString();
                                break;
                        }
                    }
                }

                return true;
            }
            else
            {
                logger.Error($"Failed to load Modifier settings as the settings file couldnt be found at '{expectedSettingsFilePath}'");
                return false;
            }
        }

        /// <inheritdoc/>
        public bool SaveSettings(string gameDirectory, string modFolder)
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

            // Save all the modifier settings
            StringBuilder enabledStringBuilder = new StringBuilder();
            for (int i = 0; i < modifierViewModels.Count; i++)
            {
                ConfigurableModifierViewModel configurableModifier = modifierViewModels[i];
                if (configurableModifier.ModifierEnabled)
                {
                    enabledStringBuilder.Append($"{configurableModifier.ModifierType.ToString()}");
                    if (i < (modifierViewModels.Count - 1))
                    {
                        enabledStringBuilder.Append(",");
                    }
                }

                for (int j = 0; j < configurableModifier.ModifierSettings.Count; j++)
                {
                    ModifierSettingViewModel modifierSetting = configurableModifier.ModifierSettings[j];
                    switch (modifierSetting.SettingType)
                    {
                        case ModifierSettingType.Bool:
                            ModifiersModSettings.SetOrCreateSetting(modifierSetting.SettingKey, modifierSetting.BoolSettingValue.ToString());
                            break;
                        case ModifierSettingType.String:
                            ModifiersModSettings.SetOrCreateSetting(modifierSetting.SettingKey, modifierSetting.StringSettingValue.ToString());
                            break;
                        case ModifierSettingType.Float:
                            ModifiersModSettings.SetOrCreateSetting(modifierSetting.SettingKey, modifierSetting.FloatSettingValue);
                            break;
                        case ModifierSettingType.Int:
                            ModifiersModSettings.SetOrCreateSetting(modifierSetting.SettingKey, modifierSetting.IntSettingValue);
                            break;
                        case ModifierSettingType.Enum:
                            ModifiersModSettings.SetOrCreateSetting(modifierSetting.SettingKey, modifierSetting.EnumSettingValue);
                            break;
                    }
                }
            }
            ModifiersModSettings.SetOrCreateSetting(JumpKingModifiersModSettingsContext.EnabledModifiersKey, enabledStringBuilder.ToString());

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

        /// <summary>
        /// Loads all possible modifiers from the modifier DLL and creates UI controls for each,
        /// which are then registered with the provided stack
        /// </summary>
        /// <remarks>This should really be user controls and data templates but I hate myself</remarks>
        private void CreateModifiersUIControls(StackPanel modifiersStack, string modifierDLLPath)
        {
            modifiersStack.Children.Clear();

            Assembly assembly = Assembly.LoadFrom(modifierDLLPath);
            List<Tuple<Type, ConfigurableModifierAttribute>> modifiers = assembly.GetTypes().Select((Type t) =>
            {
                ConfigurableModifierAttribute attribute = t.GetCustomAttribute<ConfigurableModifierAttribute>();
                if (attribute != null)
                {
                    return new Tuple<Type, ConfigurableModifierAttribute>(t, attribute);
                }
                return null;
            })
            .Where((Tuple<Type, ConfigurableModifierAttribute> tuple) => tuple != null)
            .OrderBy((Tuple<Type, ConfigurableModifierAttribute> tuple) => tuple.Item2.ConfigurableModifierName)
            .ToList();

            // Go through all Modifiers
            modifierViewModels.Clear();
            for (int i = 0; i < modifiers.Count; i++)
            {
                // Collect any other settings for this modifier
                List<ModifierSettingAttribute> modifierSettings = modifiers[i].Item1.GetFields()
                    .Select((FieldInfo f) => f.GetCustomAttribute<ModifierSettingAttribute>()).Where((ModifierSettingAttribute a) => a != null).ToList();

                // Make the grid to contain the modifier name & enabled toggle
                Grid modifierActiveGrid = new Grid();
                modifierActiveGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                modifierActiveGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                modifierActiveGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                modifierActiveGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                // Make the modifier name
                Label modifierName = new Label();
                modifierName.Content = modifiers[i].Item2.ConfigurableModifierName;
                modifierName.FontWeight = FontWeights.Bold;
                modifierName.SetValue(Grid.ColumnProperty, 0);
                modifierName.SetValue(Grid.RowProperty, 0);
                modifierName.VerticalAlignment = VerticalAlignment.Center;

                // Make the enabled toggle
                CheckBox enabledBox = new CheckBox();
                enabledBox.SetValue(Grid.ColumnProperty, 2);
                enabledBox.SetValue(Grid.RowProperty, 0);
                enabledBox.VerticalAlignment = VerticalAlignment.Center;
                enabledBox.HorizontalAlignment = HorizontalAlignment.Right;

                // Add to the grid & parent stack
                modifierActiveGrid.Children.Add(modifierName);
                modifierActiveGrid.Children.Add(enabledBox);
                Expander modifierExpander = null;
                StackPanel modifierExpanderStack = null;
                if (modifierSettings.Count > 0)
                {
                    modifierExpander = new Expander();
                    modifierExpanderStack = new StackPanel();
                    modifierExpander.Header = modifierActiveGrid;
                    modifierExpander.Content = modifierExpanderStack;
                    modifiersStack.Children.Add(modifierExpander);
                }
                else
                {
                    modifiersStack.Children.Add(modifierActiveGrid);
                }

                ConfigurableModifierViewModel configurableModifierViewModel =
                    new ConfigurableModifierViewModel(modifiers[i].Item1, modifiers[i].Item2.ConfigurableModifierName);

                // Make the binding and assign the default value
                Binding enabledBinding = new Binding(nameof(configurableModifierViewModel.ModifierEnabled));
                enabledBinding.Source = configurableModifierViewModel;
                enabledBox.SetBinding(CheckBox.IsCheckedProperty, enabledBinding);
                enabledBox.IsChecked = true;

                modifierViewModels.Add(configurableModifierViewModel);

                // Make options for every defined setting
                for (int j = 0; j < modifierSettings.Count; j++)
                {
                    // Make the grid to contain this setting name and it's value
                    Grid settingGrid = new Grid();
                    settingGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                    settingGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                    settingGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                    // Make the setting name
                    Label settingName = new Label();
                    settingName.Content = modifierSettings[j].DisplayName;
                    settingName.SetValue(Grid.ColumnProperty, 0);
                    settingName.SetValue(Grid.RowProperty, 0);
                    settingName.VerticalAlignment = VerticalAlignment.Center;
                    settingGrid.Children.Add(settingName);

                    // Make the value editor
                    switch (modifierSettings[j].SettingType)
                    {
                        case ModifierSettingType.Bool:
                            {
                                CheckBox settingBox = new CheckBox();
                                settingBox.SetValue(Grid.ColumnProperty, 1);
                                settingBox.SetValue(Grid.RowProperty, 0);
                                settingBox.VerticalAlignment = VerticalAlignment.Center;

                                ModifierSettingViewModel modifierSettingViewModel =
                                    new ModifierSettingViewModel(modifierSettings[j].SettingKey, modifierSettings[j].DefaultSetting, modifierSettings[j].SettingType);

                                // Make the binding and assign the default value
                                Binding boolBinding = new Binding(nameof(modifierSettingViewModel.BoolSettingValue));
                                boolBinding.Source = modifierSettingViewModel;
                                settingBox.SetBinding(CheckBox.IsCheckedProperty, boolBinding);
                                settingBox.IsChecked = (bool)modifierSettings[j].DefaultSetting;

                                // Set the enabled binding
                                Binding settingEnabledBinding = new Binding(nameof(configurableModifierViewModel.ModifierEnabled));
                                settingEnabledBinding.Source = configurableModifierViewModel;
                                settingBox.SetBinding(CheckBox.IsEnabledProperty, settingEnabledBinding);

                                configurableModifierViewModel.ModifierSettings.Add(modifierSettingViewModel);

                                settingGrid.Children.Add(settingBox);
                                break;
                            }
                        case ModifierSettingType.String:
                            {
                                TextBox textBox = new TextBox();
                                textBox.SetValue(Grid.ColumnProperty, 1);
                                textBox.SetValue(Grid.RowProperty, 0);
                                textBox.MinWidth = 100;
                                textBox.VerticalAlignment = VerticalAlignment.Center;

                                ModifierSettingViewModel modifierSettingViewModel =
                                    new ModifierSettingViewModel(modifierSettings[j].SettingKey, modifierSettings[j].DefaultSetting, modifierSettings[j].SettingType);

                                // Make the binding and assign the default value
                                Binding stringBinding = new Binding(nameof(modifierSettingViewModel.StringSettingValue));
                                stringBinding.Source = modifierSettingViewModel;
                                textBox.SetBinding(TextBox.TextProperty, stringBinding);
                                textBox.Text = (string)modifierSettings[j].DefaultSetting;

                                // Set the enabled binding
                                Binding settingEnabledBinding = new Binding(nameof(configurableModifierViewModel.ModifierEnabled));
                                settingEnabledBinding.Source = configurableModifierViewModel;
                                textBox.SetBinding(TextBox.IsEnabledProperty, settingEnabledBinding);

                                configurableModifierViewModel.ModifierSettings.Add(modifierSettingViewModel);

                                settingGrid.Children.Add(textBox);
                                break;
                            }
                        case ModifierSettingType.Float:
                            {
                                TextBox textBox = new TextBox();
                                textBox.SetValue(Grid.ColumnProperty, 1);
                                textBox.SetValue(Grid.RowProperty, 0);
                                textBox.MinWidth = 100;
                                textBox.VerticalAlignment = VerticalAlignment.Center;

                                ModifierSettingViewModel modifierSettingViewModel =
                                    new ModifierSettingViewModel(modifierSettings[j].SettingKey, modifierSettings[j].DefaultSetting, modifierSettings[j].SettingType);

                                // Make the binding and assign the default value
                                Binding floatBinding = new Binding(nameof(modifierSettingViewModel.FloatSettingValue));
                                floatBinding.Source = modifierSettingViewModel;
                                floatBinding.UpdateSourceTrigger = UpdateSourceTrigger.Default;
                                textBox.SetBinding(TextBox.TextProperty, floatBinding);
                                textBox.Text = ((float)modifierSettings[j].DefaultSetting).ToString();

                                // Set the enabled binding
                                Binding settingEnabledBinding = new Binding(nameof(configurableModifierViewModel.ModifierEnabled));
                                settingEnabledBinding.Source = configurableModifierViewModel;
                                textBox.SetBinding(TextBox.IsEnabledProperty, settingEnabledBinding);

                                configurableModifierViewModel.ModifierSettings.Add(modifierSettingViewModel);

                                settingGrid.Children.Add(textBox);
                                break;
                            }
                        case ModifierSettingType.Int:
                            {
                                TextBox textBox = new TextBox();
                                textBox.SetValue(Grid.ColumnProperty, 1);
                                textBox.SetValue(Grid.RowProperty, 0);
                                textBox.MinWidth = 100;
                                textBox.VerticalAlignment = VerticalAlignment.Center;

                                ModifierSettingViewModel modifierSettingViewModel =
                                    new ModifierSettingViewModel(modifierSettings[j].SettingKey, modifierSettings[j].DefaultSetting, modifierSettings[j].SettingType);

                                // Make the binding and assign the default value
                                Binding intBinding = new Binding(nameof(modifierSettingViewModel.IntSettingValue));
                                intBinding.Source = modifierSettingViewModel;
                                intBinding.UpdateSourceTrigger = UpdateSourceTrigger.Default;
                                textBox.SetBinding(TextBox.TextProperty, intBinding);
                                textBox.Text = ((int)modifierSettings[j].DefaultSetting).ToString();

                                // Set the enabled binding
                                Binding settingEnabledBinding = new Binding(nameof(configurableModifierViewModel.ModifierEnabled));
                                settingEnabledBinding.Source = configurableModifierViewModel;
                                textBox.SetBinding(TextBox.IsEnabledProperty, settingEnabledBinding);

                                configurableModifierViewModel.ModifierSettings.Add(modifierSettingViewModel);

                                settingGrid.Children.Add(textBox);
                                break;
                            }
                        case ModifierSettingType.Enum:
                            {
                                ComboBox comboBox = new ComboBox();
                                comboBox.SetValue(Grid.ColumnProperty, 1);
                                comboBox.SetValue(Grid.RowProperty, 0);
                                comboBox.ItemsSource = Enum.GetNames(modifierSettings[j].EnumType);
                                comboBox.VerticalAlignment = VerticalAlignment.Center;

                                ModifierSettingViewModel modifierSettingViewModel =
                                    new ModifierSettingViewModel(modifierSettings[j].SettingKey, modifierSettings[j].DefaultSetting, modifierSettings[j].SettingType, modifierSettings[j].EnumType);

                                // Make the binding and assign the default value
                                Binding enumBinding = new Binding(nameof(modifierSettingViewModel.EnumSettingValue));
                                enumBinding.Source = modifierSettingViewModel;
                                comboBox.SetBinding(ComboBox.SelectedValueProperty, enumBinding);
                                comboBox.SelectedValue = modifierSettingViewModel.DefaultSettingValue.ToString();

                                // Set the enabled binding
                                Binding settingEnabledBinding = new Binding(nameof(configurableModifierViewModel.ModifierEnabled));
                                settingEnabledBinding.Source = configurableModifierViewModel;
                                comboBox.SetBinding(ComboBox.IsEnabledProperty, settingEnabledBinding);

                                configurableModifierViewModel.ModifierSettings.Add(modifierSettingViewModel);

                                settingGrid.Children.Add(comboBox);
                                break;
                            }
                    }

                    if (modifierExpanderStack != null)
                    {
                        modifierExpanderStack.Children.Add(settingGrid);
                    }
                    else
                    {
                        modifiersStack.Children.Add(settingGrid);
                    }
                }
            }
        }

    }
}
