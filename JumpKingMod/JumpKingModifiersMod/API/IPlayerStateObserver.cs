using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Patching.States;
using Microsoft.Xna.Framework;
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
        /// Returns the last <see cref="InputState"/> polled by the game
        /// </summary>
        InputState GetInputState();

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

        /// <summary>
        /// Disables (or re-enables) player walking
        /// </summary>
        /// <param name="isWalkingDisabled">If <c>true</c> then the player will be unable to walk left/right</param>
        /// <param name="isXVelocityDisabled">
        /// If <c>true</c> then we will also set the X velocity to 0. This is useful as X velocity resetting is handled by the Walk
        /// component, which we disable when disabling walking, meaning if the player has X velocity when you disable walking
        /// they may slide unexpectedly.
        /// This is automatically set back to <c>false</c> internally if <paramref name="isWalkingDisabled"/> is <c>false</c>
        /// </param>
        void DisablePlayerWalking(bool isWalkingDisabled, bool isXVelocityDisabled = false);

        /// <summary>
        /// Allows the inverting of player left/right inputs
        /// </summary>
        /// <param name="invertPlayerInputs">if <c>true</c> then player inputs will be inverted, if <c>false</c> they wont</param>
        void SetInvertPlayerInputs(bool invertPlayerInputs);

        /// <summary>
        /// Gets whether the player's inputs are currently inverted
        /// </summary>
        /// <returns><c>true</c> if the inputs are inverted, <c>false</c> if not</returns>
        bool GetInvertPlayerInputs();

        /// <summary>
        /// Gets the player's hitbox
        /// </summary>
        Rectangle GetPlayerHitbox();

        /// <summary>
        /// Sets the player's position
        /// </summary>
        void SetPosition(Vector2 position);

        /// <summary>
        /// If set, the player will not be drawn
        /// </summary>
        void DisablePlayerDrawing(bool isDrawDisabled);

        /// <summary>
        /// If set, prevents the BodyComp component from updating
        /// </summary>
        void DisablePlayerBodyComp(bool isBodyCompDisabled);
    }
}
