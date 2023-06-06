using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurableModifierAttribute : Attribute
    {
        public string ConfigurableModifierName { get; private set; }

        public Keys DefaultToggleKey { get; private set; }

        public ConfigurableModifierAttribute(string modifierName, Keys defaultToggleKey)
        {
            if (string.IsNullOrWhiteSpace(modifierName))
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            ConfigurableModifierName = modifierName;
            DefaultToggleKey = defaultToggleKey;
        }

        public ConfigurableModifierAttribute(string modifierName) : this(modifierName, Keys.NumPad1)
        {
        }
    }
}
