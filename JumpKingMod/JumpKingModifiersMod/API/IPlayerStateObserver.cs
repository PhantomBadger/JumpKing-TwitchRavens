using JumpKingModifiersMod.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing an object capable of getting the current player state
    /// </summary>
    public interface IPlayerStateObserver
    {
        /// <summary>
        /// Returns the current <see cref="PlayerState"/>
        /// </summary>
        PlayerState GetPlayerState();

        /// <summary>
        /// Sets the player to be in a knocked state
        /// </summary>
        void SetKnockedStateOverride(bool isActive, bool newState);

        /// <summary>
        /// Sets the direction of the player
        /// </summary>
        void SetDirectionOverride(bool isActive, int newDirection);

        /// <summary>
        /// Restarts the player's position to the start of the map
        /// </summary>
        void RestartPlayerPosition();
    }
}
