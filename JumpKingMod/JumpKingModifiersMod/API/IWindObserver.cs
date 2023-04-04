using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing an object capable of interacting with the wind state
    /// </summary>
    public interface IWindObserver
    {
        /// <summary>
        /// Returns <c>true</c> if the Wind is currently overridden, <c>false</c> if not
        /// </summary>
        bool GetWindOverrideState();

        /// <summary>
        /// Sets the wind to be overridden to on or not
        /// </summary>
        void SetWindOverrideState(bool shouldOverrideWind);
    }
}
