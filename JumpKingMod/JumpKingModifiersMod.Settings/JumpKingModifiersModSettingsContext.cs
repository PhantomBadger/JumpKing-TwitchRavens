using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    public abstract class JumpKingModifiersModSettingsContext
    {
        public const string SettingsFileName = "JumpKingModifiersMod.settings";
        public const char CommentCharacter = '#';

        // Debug Trigger
        public const string DebugTriggerToggleKey = "DebugTriggerToggleKey";

        // Fall Damage
        public const string FallDamageSubtextsFilePath = "Content/Mods/FallDamageSubtexts.txt";
        public const string FallDamageModifierKey = "FallDamageModifier";

        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                { FallDamageToggleKey, Keys.P.ToString() },
                { FallDamageModifierKey, 0.1f.ToString() }
            };
        }

        public static string[] GetDefaultFallDamageSubtextx()
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
    }
}
