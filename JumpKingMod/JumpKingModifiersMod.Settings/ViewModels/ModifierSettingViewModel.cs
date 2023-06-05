using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    public class ModifierSettingViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The key for the setting 
        /// </summary>
        public string SettingKey { get; private set; }

        public ModifierSettingType SettingType { get; private set; }
        public Type EnumType { get; private set; }
        public object DefaultSettingValue { get; private set; }

        /// <summary>
        /// The value of the setting as a bool
        /// </summary>
        public bool BoolSettingValue
        {
            get
            {
                return boolSettingValue;
            }
            set
            {
                if (boolSettingValue != value)
                {
                    boolSettingValue = value;
                    RaisePropertyChanged(nameof(BoolSettingValue));
                }
            }
        }
        private bool boolSettingValue;

        /// <summary>
        /// The value of the setting as a string
        /// </summary>
        public string StringSettingValue
        {
            get
            {
                return stringSettingValue;
            }
            set
            {
                if (stringSettingValue != value)
                {
                    stringSettingValue = value;
                    RaisePropertyChanged(nameof(StringSettingValue));
                }
            }
        }
        private string stringSettingValue;

        /// <summary>
        /// The value of the setting as an int
        /// </summary>
        public string IntSettingValue
        {
            get
            {
                return intSettingValue.ToString();
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    int.TryParse(value, out int intVal) &&
                    intVal != intSettingValue)
                {
                    intSettingValue = intVal;
                    RaisePropertyChanged(nameof(IntSettingValue));
                }
            }
        }
        private int intSettingValue;

        /// <summary>
        /// The value of the setting as a float
        /// </summary>
        public string FloatSettingValue
        {
            get
            {
                return floatSettingValue.ToString();
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture,
                        out float floatVal) &&
                    floatSettingValue != floatVal)
                {
                    floatSettingValue = floatVal;
                    RaisePropertyChanged(nameof(FloatSettingValue));
                }
            }
        }
        private float floatSettingValue;

        /// <summary>
        /// The value of the setting as an enum
        /// </summary>
        public string EnumSettingValue
        {
            get
            {
                return enumSettingValue?.ToString() ?? "";
            }
            set
            {
                try
                {
                    Enum enumValue = (Enum)Enum.Parse(EnumType, value);
                    if (enumSettingValue != enumValue)
                    {
                        enumSettingValue = enumValue;
                        RaisePropertyChanged(nameof(EnumSettingValue));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Sadge");
                    // it wasnt valid so we dont set it
                }
            }
        }
        private Enum enumSettingValue;

        public ModifierSettingViewModel(string settingKey, object defaultSettingValue, ModifierSettingType settingType)
        {
            SettingKey = settingKey;
            SettingType = settingType;
            DefaultSettingValue = defaultSettingValue;
        }
        public ModifierSettingViewModel(string settingKey, object defaultSettingValue, ModifierSettingType settingType, Type enumType) : this(settingKey, defaultSettingValue, settingType)
        {
            EnumType = enumType;
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
