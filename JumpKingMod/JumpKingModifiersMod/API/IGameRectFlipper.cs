using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing an object that can flip the game rect
    /// </summary>
    public interface IGameRectFlipper
    {
        /// <summary>
        /// Sets whether we should be flipping the game rect or not
        /// </summary>
        void SetShouldFlipGameRect(bool shouldFlip);

        /// <summary>
        /// Returns whether the game rect is currently flipped or not
        /// </summary>
        bool IsGameRectFlipped();
    }
}
