using Microsoft.Xna.Framework;
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
        /// The velocity of the player at this point
        /// </summary>
        public Vector2 Velocity { get; private set; }

        /// <summary>
        /// The position of the player at this point
        /// </summary>
        public Vector2 Position { get; private set; }

        /// <summary>
        /// Whether the player is in a knocked state
        /// </summary>
        public bool Knocked { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="PlayerState"/>
        /// </summary>
        public PlayerState(bool isOnGround, Vector2 velocity, 
            Vector2 position, bool knocked)
        {
            IsOnGround = isOnGround;
            Velocity = velocity;
            Position = position;
            Knocked = knocked;
        }
    }
}
