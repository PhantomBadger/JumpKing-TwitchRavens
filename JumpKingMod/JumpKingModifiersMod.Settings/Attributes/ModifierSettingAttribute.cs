using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ModifierSettingAttribute : Attribute
    {

        public string SettingKey { get; private set; }
        public string DisplayName { get; private set; }
        public ModifierSettingType SettingType { get; private set; }
        public object DefaultSetting { get; private set; }
        public Type EnumType { get; private set; }


        private ModifierSettingAttribute(string settingKey, string displayName)
        {
            SettingKey = settingKey;
            DisplayName = displayName;
        }

        public ModifierSettingAttribute(string settingKey, string displayName, bool defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Bool;
            DefaultSetting = defaultValue;
        }

        public ModifierSettingAttribute(string settingKey, string displayName, string defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.String;
            DefaultSetting = defaultValue;
        }

        public ModifierSettingAttribute(string settingKey, string displayName, float defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Float;
            DefaultSetting = defaultValue;
        }

        public ModifierSettingAttribute(string settingKey, string displayName, int defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Int;
            DefaultSetting = defaultValue;
        }

        public ModifierSettingAttribute(string settingKey, string displayName, object defaultValue, Type enumType) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Enum;
            DefaultSetting = defaultValue as Enum;
            EnumType = enumType;
        }
    }
}
