using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing a possible modifier
    /// </summary>
    public interface IModifier
    {
        /// <summary>
        /// Applies the current modifier
        /// </summary>
        /// <returns><c>true</c> if successfully applied, <c>false</c> if not</returns>
        bool EnableModifier();

        /// <summary>
        /// Disables the current modifier
        /// </summary>
        /// <returns><c>true</c> if successfully disabled, <c>false</c> if not</returns>
        bool DisableModifier();

        /// <summary>
        /// Returns whether the current modifier is enabled
        /// </summary>
        /// <returns><c>true</c> if enabled, <c>false</c> if disabled</returns>
        bool IsModifierEnabled();

        /// <summary>
        /// Update the state of the modifier, only called if active.
        /// Called by the <see cref="ModifierUpdatingEntity"/> as long as it has been registered
        /// </summary>
        void Update(float p_delta);
    }
}
