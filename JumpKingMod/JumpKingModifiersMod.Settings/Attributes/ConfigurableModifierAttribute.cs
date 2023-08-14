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
        /// A description of the modifier
        /// </summary>
        public string ModifierDescription { get; private set; }

        /// <summary>
        /// The default key to toggle this modifier when used in a Debug Trigger
        /// </summary>
        public Keys DefaultToggleKey { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierAttribute"/>
        /// </summary>
        /// <param name="modifierName">The name of the modifier to use in the UI</param>
        /// <param name="description">A description for what the modifier does</param>
        /// <param name="defaultToggleKey">The default key to toggle this modifier when used in a Debug Trigger</param>
        public ConfigurableModifierAttribute(string modifierName, string description, Keys defaultToggleKey)
        {
            if (string.IsNullOrWhiteSpace(modifierName))
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            ConfigurableModifierName = modifierName;
            ModifierDescription = description;
            DefaultToggleKey = defaultToggleKey;
        }

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierAttribute"/> with no description
        /// </summary>
        /// <param name="modifierName">The name of the modifier to use in the UI</param>
        /// <param name="defaultToggleKey">The default key to toggle this modifier when used in a Debug Trigger</param>
        public ConfigurableModifierAttribute(string modifierName, Keys defaultToggleKey) : this(modifierName, "", defaultToggleKey)
        {
        }

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierName"/>
        /// </summary>
        /// <param name="modifierName">The name of the modifier to use in the UI</param>
        /// <param name="description">A description for what the modifier does</param>
        public ConfigurableModifierAttribute(string modifierName, string description) : this(modifierName, description, Keys.NumPad1)
        {
        }

        /// <summary>
        /// Ctor for creating a <see cref="ConfigurableModifierName"/>
        /// </summary>
        /// <param name="modifierName">The name of the modifier to use in the UI</param>
        public ConfigurableModifierAttribute(string modifierName) : this(modifierName, "", Keys.NumPad1)
        {
        }
    }
}
