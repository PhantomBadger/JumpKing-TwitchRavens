using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings.ViewModels
{
    /// <summary>
    /// A ViewModel to represent the setting of a single configurable Modifier
    /// </summary>
    public class ConfigurableModifierViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The type of the Modifier
        /// </summary>
        public Type ModifierType { get; private set; }

        /// <summary>
        /// The name of the Modifier
        /// </summary>
        public string ModifierName { get; private set; }

        /// <summary>
        /// A collection of ViewModels for the settings of this Modifier
        /// </summary>
        public List<ModifierSettingViewModel> ModifierSettings { get; private set; }

        /// <summary>
        /// The key to be used to toggle this modifier when used in a debug trigger
        /// </summary>
        public Keys ToggleKey
        {
            get
            {
                return toggleKey;
            }
            set
            {
                if (toggleKey != value)
                {
                    toggleKey = value;
                    RaisePropertyChanged(nameof(ToggleKey));
                }
            }
        }
        private Keys toggleKey;

        /// <summary>
        /// Whether this modifier is selected to be used or not
        /// </summary>
        public bool ModifierEnabled
        {
            get
            {
                return modifierEnabled;
            }
            set
            {
                if (modifierEnabled != value)
                {
                    modifierEnabled = value;
                    RaisePropertyChanged(nameof(ModifierEnabled));
                }
            }
        }
        private bool modifierEnabled;

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierViewModel"/>
        /// </summary>
        /// <param name="modifierType">The type of the modifier</param>
        /// <param name="modifierName">The name of the modifier</param>
        /// <param name="toggleKey">The key to be used to toggle this modifier when used in a debug trigger</param>
        public ConfigurableModifierViewModel(Type modifierType, string modifierName, Keys toggleKey)
        {
            ModifierType = modifierType;
            ModifierName = modifierName;
            ToggleKey = toggleKey;
            ModifierSettings = new List<ModifierSettingViewModel>();
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
