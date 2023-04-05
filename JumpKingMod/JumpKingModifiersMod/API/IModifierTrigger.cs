using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    public delegate void ModifierEnabledDelegate(IModifier modifier);
    public delegate void ModifierDisabledDelegate(IModifier modifier);

    /// <summary>
    /// An interface representing a possible trigger for a modifier
    /// </summary>
    public interface IModifierTrigger
    {
        event ModifierEnabledDelegate OnModifierEnabled;
        event ModifierDisabledDelegate OnModifierDisabled;

        /// <summary>
        /// Enabled the trigger to start being able to activate a modifier
        /// </summary>
        /// <returns><c>true</c> if successfully enabled, <c>false</c> if not</returns>
        bool EnableTrigger();

        /// <summary>
        /// Disable the trigger to stop being able to activate a modifier
        /// </summary>
        /// <returns><c>true</c> if successfully disabled, <c>false</c> if not</returns>
        bool DisableTrigger();

        /// <summary>
        /// Returns whether the current trigger is enabled
        /// </summary>
        /// <returns><c>true</c> if enabled, <c>false</c> if disabled</returns>
        bool IsTriggerEnabled();
    }
}
