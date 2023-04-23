using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface wrapping the logic for accessing the walk speed modifier
    /// </summary>
    public interface IWalkSpeedModifier
    {
        /// <summary>
        /// Sets the walk speed modifier to the new value
        /// </summary>
        void SetWalkSpeedModifer(float newModifier);

        /// <summary>
        /// Gets the walk speed modifier
        /// </summary>
        float GetWalkSpeedModifier();
    }
}
