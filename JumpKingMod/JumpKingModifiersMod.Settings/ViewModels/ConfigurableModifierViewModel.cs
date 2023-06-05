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

        public ConfigurableModifierViewModel(Type modifierType, string modifierName)
        {
            ModifierType = modifierType;
            ModifierName = modifierName;
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
