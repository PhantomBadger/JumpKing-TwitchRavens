using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface wrapping the logic for accessing the jump time modifier
    /// </summary>
    public interface IJumpTimeModifier
    {
        /// <summary>
        /// Sets the jump time modifier to the new value
        /// </summary>
        void SetJumpTimeModifer(float newModifier);

        /// <summary>
        /// Gets the jump time modifier
        /// </summary>
        float GetJumpTimeModifier();
    }
}
