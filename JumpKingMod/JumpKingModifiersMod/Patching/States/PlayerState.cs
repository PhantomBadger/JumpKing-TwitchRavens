using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching
{
    /// <summary>
    /// An aggregate class containing information about the player
    /// </summary>
    public class PlayerState
    {
        /// <summary>
        /// Whether the player is currently on the ground or not
        /// </summary>
        public bool IsOnGround { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="PlayerState"/>
        /// </summary>
        public PlayerState(bool isOnGround)
        {
            IsOnGround = isOnGround;
        }
    }
}
