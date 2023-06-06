using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings.ViewModels
{
    public class ConfigurableModifierViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public Type ModifierType { get; private set; }

        public string ModifierName { get; private set; }

        public List<ModifierSettingViewModel> ModifierSettings { get; private set; }

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
