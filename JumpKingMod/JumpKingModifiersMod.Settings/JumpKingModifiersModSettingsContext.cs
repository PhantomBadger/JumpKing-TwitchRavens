using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    /// <summary>
    /// An aggregate of settings keys for use in <see cref="JumpKingModifiersMod"/>
    /// </summary>
    public abstract class JumpKingModifiersModSettingsContext
    {
        public const string SettingsFileName = "JumpKingModifiersMod.settings";
        public const char CommentCharacter = '#';

        // Modifiers
        public const string EnabledModifiersKey = "EnabledModifiers";
        public const string TriggerTypeKey = "TriggerType";
        public const string ModifierToggleKeysKey = "ModifierToggleKeys";

        // Fall Damage
        public const string FallDamageBloodSplatterFilePath = "Content/Mods/BloodSplatters.txt";
        public const string FallDamageSubtextsFilePath = "Content/Mods/FallDamageSubtexts.txt";
        public const string FallDamageEnabledKey = "FallDamageEnabled";
        public const string FallDamageModifierKey = "FallDamageModifier";
        public const string FallDamageBloodEnabledKey = "FallDamageBloodEnabled";
        public const string FallDamageClearBloodKey = "FallDamageClearBlood";
        public const string FallDamagePreviousHealthKey = "FallDamagePreviousHealth";
        public const string FallDamageNiceSpawnsKey = "FallDamageNiceSpawns";
        public const float DefaultFallDamageModifier = 0.05f;

        // Shrinking
        public const string DebugTriggerManualResizeToggleKey = "DebugTriggerManualResizeToggle";
        public const string ManualResizeEnabledKey = "ManualResizeEnabled";
        public const string ManualResizeGrowKeyKey = "ManualResizeGrowKey";
        public const string ManualResizeShrinkKeyKey = "ManualResizeShrinkKey";

        // Lava
        public const string RisingLavaEnabledKey = "RisingLavaEnabled";
        public const string RisingLavaSpeedKey = "RisingLavaSpeed";
        public const string RisingLavaTestEnum = "RisingLavaTestEnum";
        public const string RisingLavaNiceSpawnsKey = "RisingLavaNiceSpawns";
        public const float DefaultRisingLavaSpeed = 5f;

        /// <summary>
        /// Gets the default state of the settings
        /// </summary>
        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                // Fall Damage
                { FallDamageEnabledKey, false.ToString() },
                { FallDamageBloodEnabledKey, true.ToString() },
                { FallDamageClearBloodKey, Keys.F10.ToString() },
                { FallDamageModifierKey, DefaultFallDamageModifier.ToString() },
                { FallDamagePreviousHealthKey, 100.ToString() },
                { FallDamageNiceSpawnsKey, true.ToString() },

                // Shrinking
                { DebugTriggerManualResizeToggleKey, Keys.F9.ToString() },
                { ManualResizeEnabledKey, false.ToString() },
                { ManualResizeGrowKeyKey, Keys.Up.ToString() },
                { ManualResizeShrinkKeyKey, Keys.Down.ToString() },

                // Rising Lava
                { RisingLavaEnabledKey, false.ToString() },
                { RisingLavaSpeedKey, DefaultRisingLavaSpeed.ToString() },
                { RisingLavaNiceSpawnsKey, true.ToString() },
            };
        }

        /// <summary>
        /// Gets the default values for the Fall Damage Subtexts
        /// </summary>
        public static string[] GetDefaultFallDamageSubtexts()
        {
            return new string[]
            {
                "That's gotta be embarrassing...",
                "You're meant to be going up, you know",
                "Thanks for playing Fall Guys!",
                "It's like you don't even try",
                "Stop milking this for content",
                "Follow PhantomBadger_ on Twitter!",
                "Do you regret your choices yet?",
                "You'll get it next time for sure",
            };
        }

        /// <summary>
        /// Utility function to parse the <see cref="ModifierToggleKeysKey"/> setting
        /// </summary>
        public static Dictionary<string, Keys> ParseToggleKeys(string rawModifierToggleKeysSetting)
        {
            string[] splitModifierToggleKeys = rawModifierToggleKeysSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, Keys> toggleKeys = new Dictionary<string, Keys>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < splitModifierToggleKeys.Length; i++)
            {
                string[] toggleSetting = splitModifierToggleKeys[i].Split(':');
                if (toggleSetting.Length == 2)
                {
                    toggleKeys.Add(toggleSetting[0], (Keys)Enum.Parse(typeof(Keys), toggleSetting[1]));
                }
            }
            return toggleKeys;
        }

        /// <summary>
        /// Utility function to parse the <see cref="EnabledModifiersKey"/> setting
        /// </summary>
        public static HashSet<string> ParseEnabledModifiers(string rawEnabledModifiersSetting)
        {
            return new HashSet<string>(rawEnabledModifiersSetting.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries), StringComparer.OrdinalIgnoreCase);
        }
    }
}
