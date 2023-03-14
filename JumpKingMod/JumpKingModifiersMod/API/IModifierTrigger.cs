using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing a possible trigger for a modifier
    /// </summary>
    public interface IModifierTrigger
    {
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
