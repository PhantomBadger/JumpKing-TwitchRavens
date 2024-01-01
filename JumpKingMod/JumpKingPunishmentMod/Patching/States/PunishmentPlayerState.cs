using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingPunishmentMod.Patching.States
{
    /// <summary>
    /// An aggregate class containing information about the player for the punishment mod
    /// </summary>
    public class PunishmentPlayerState
    {
        /// <summary>
        /// Whether the player is currently on the ground or not
        /// </summary>
        public bool IsOnGround { get; private set; }

        /// <summary>
        /// Whether the player is currently on sand or not
        /// </summary>
        public bool IsOnSand { get; private set; }

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
        /// Ctor for creating a <see cref="PunishmentPlayerState"/>
        /// </summary>
        public PunishmentPlayerState(bool isOnGround, bool isOnSand, Vector2 velocity, Vector2 position, bool knocked)
        {
            IsOnGround = isOnGround;
            IsOnSand = isOnSand;
            Velocity = velocity;
            Position = position;
            Knocked = knocked;
        }
    }
}
