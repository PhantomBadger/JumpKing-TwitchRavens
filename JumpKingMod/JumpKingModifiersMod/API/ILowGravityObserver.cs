using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface wrapping the logic for accessing and modifying the Low Gravity of the Level
    /// </summary>
    public interface ILowGravityObserver
    {
        /// <summary>
        /// Sets the 'Low Gravity' override state of the Level Manager
        /// </summary>
        void SetLowGravityOverrideState(bool shouldOverrideLowGravity);

        /// <summary>
        /// Gets the 'Low Gravity' override state of the Level Manager
        /// </summary>
        bool GetLowGravityOverrideState();
    }
}
