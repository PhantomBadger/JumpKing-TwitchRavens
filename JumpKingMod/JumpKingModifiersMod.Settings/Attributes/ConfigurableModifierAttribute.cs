using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    /// <summary>
    /// An attribute representing a Modifier that can be configured by users
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ConfigurableModifierAttribute : Attribute
    {
        /// <summary>
        /// The name of the modifier to use in the UI
        /// </summary>
        public string ConfigurableModifierName { get; private set; }

        /// <summary>
        /// The default key to toggle this modifier when used in a Debug Trigger
        /// </summary>
        public Keys DefaultToggleKey { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierName"/>
        /// </summary>
        /// <param name="modifierName">The name of the modifier to use in the UI</param>
        /// <param name="defaultToggleKey">The default key to toggle this modifier when used in a Debug Trigger</param>
        public ConfigurableModifierAttribute(string modifierName, Keys defaultToggleKey)
        {
            if (string.IsNullOrWhiteSpace(modifierName))
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            ConfigurableModifierName = modifierName;
            DefaultToggleKey = defaultToggleKey;
        }

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierName"/>
        /// </summary>
        /// <param name="modifierName">The name of the modifier to use in the UI</param>
        public ConfigurableModifierAttribute(string modifierName) : this(modifierName, Keys.NumPad1)
        {
        }
    }
}
