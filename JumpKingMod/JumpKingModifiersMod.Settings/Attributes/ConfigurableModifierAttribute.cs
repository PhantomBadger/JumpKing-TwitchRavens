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

        public ConfigurableModifierAttribute(string modifierName)
        {
            if (string.IsNullOrWhiteSpace(modifierName))
            {
                throw new ArgumentNullException(nameof(modifierName));
            }
            ConfigurableModifierName = modifierName;
        }
    }
}
