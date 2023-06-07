using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    /// <summary>
    /// An attribute representing a setting within a Modifier
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class ModifierSettingAttribute : Attribute
    {
        /// <summary>
        /// The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting
        /// </summary>
        public string SettingKey { get; private set; }

        /// <summary>
        /// The display name to use for the setting
        /// </summary>
        public string DisplayName { get; private set; }

        /// <summary>
        /// The type of the setting
        /// </summary>
        public ModifierSettingType SettingType { get; private set; }

        /// <summary>
        /// The default value of the setting, boxed into an object
        /// </summary>
        public object DefaultSetting { get; private set; }

        /// <summary>
        /// If the setting is an <see cref="ModifierSettingType.Enum"/> then this is the type of that enum
        /// </summary>
        public Type EnumType { get; private set; }


        /// <summary>
        /// Base ctor for creating a <see cref="ModifierSettingAttribute"/>
        /// </summary>
        /// <param name="settingKey">The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting</param>
        /// <param name="displayName">The display name to use for the setting</param>
        private ModifierSettingAttribute(string settingKey, string displayName)
        {
            SettingKey = settingKey;
            DisplayName = displayName;
        }

        /// <summary>
        /// Ctor for creating a <see cref="bool"/> <see cref="ModifierSettingAttribute"/>
        /// </summary>
        /// <param name="settingKey">The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting</param>
        /// <param name="displayName">The display name to use for the setting</param>
        /// <param name="defaultValue">The default <see cref="bool"/> value</param>
        public ModifierSettingAttribute(string settingKey, string displayName, bool defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Bool;
            DefaultSetting = defaultValue;
        }

        /// <summary>
        /// Ctor for creating a <see cref="string"/> <see cref="ModifierSettingAttribute"/>
        /// </summary>
        /// <param name="settingKey">The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting</param>
        /// <param name="displayName">The display name to use for the setting</param>
        /// <param name="defaultValue">The default <see cref="string"/> value</param>
        public ModifierSettingAttribute(string settingKey, string displayName, string defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.String;
            DefaultSetting = defaultValue;
        }

        /// <summary>
        /// Ctor for creating a <see cref="float"/> <see cref="ModifierSettingAttribute"/>
        /// </summary>
        /// <param name="settingKey">The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting</param>
        /// <param name="displayName">The display name to use for the setting</param>
        /// <param name="defaultValue">The default <see cref="float"/> value</param>
        public ModifierSettingAttribute(string settingKey, string displayName, float defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Float;
            DefaultSetting = defaultValue;
        }

        /// <summary>
        /// Ctor for creating a <see cref="int"/> <see cref="ModifierSettingAttribute"/>
        /// </summary>
        /// <param name="settingKey">The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting</param>
        /// <param name="displayName">The display name to use for the setting</param>
        /// <param name="defaultValue">The default <see cref="int"/> value</param>
        public ModifierSettingAttribute(string settingKey, string displayName, int defaultValue) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Int;
            DefaultSetting = defaultValue;
        }

        /// <summary>
        /// Ctor for creating a <see cref="Enum"/> <see cref="ModifierSettingAttribute"/>
        /// </summary>
        /// <param name="settingKey">The <see cref="JumpKingModifiersModSettingsContext"/> key to use for the setting</param>
        /// <param name="displayName">The display name to use for the setting</param>
        /// <param name="defaultValue">The default <see cref="Enum"/> value</param>
        /// <param name="enumType">The type of the enum being used</param>
        public ModifierSettingAttribute(string settingKey, string displayName, object defaultValue, Type enumType) : this(settingKey, displayName)
        {
            SettingType = ModifierSettingType.Enum;
            DefaultSetting = defaultValue as Enum;
            EnumType = enumType;
        }
    }
}
