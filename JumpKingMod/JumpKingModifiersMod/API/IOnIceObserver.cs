using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface wrapping the logic for accessing and modifying the 'IsOnIce' of the BodyComp
    /// </summary>
    public interface IOnIceObserver
    {
        /// <summary>
        /// Sets the BodyComp to override for IsOnIce
        /// </summary>
        void SetIsOnIceOverrideState(bool shouldOverrideIsOnIce);

        /// <summary>
        /// Gets the BodyComp to override for IsOnIce
        /// </summary>
        bool GetIsOnIceOverrideState();
    }
}
