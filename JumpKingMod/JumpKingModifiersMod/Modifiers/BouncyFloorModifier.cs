using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> that makes the floor bounce the player
    /// after each jump. The direction can be controlled by the player.
    /// </summary>
    public class BouncyFloorModifier : IModifier
    {
        private readonly IPlayerStateGetter playerStateGetter;
        private readonly IPlayerJumper playerJumper;
        private readonly ILogger logger;

        private bool modifierTaskActive;

        private const float BounceJumpModifier = 0.9f;
        private const float MinimumIntensity = 0.4f;

        public BouncyFloorModifier(IPlayerStateGetter playerStateGetter, IPlayerJumper playerJumper, ILogger logger)
        {
            this.playerStateGetter = playerStateGetter ?? throw new ArgumentNullException(nameof(playerStateGetter));
            this.playerJumper = playerJumper ?? throw new ArgumentNullException(nameof(playerJumper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return modifierTaskActive;
        }

        /// <summary>
        /// Disables the Bouncy Floor modifier
        /// </summary>
        /// <returns></returns>
        public bool DisableModifier()
        {
            if (!modifierTaskActive)
            {
                logger.Warning($"Failed to Disable Bouncy Floor Modifier");
                return false;
            }
            logger.Information($"Disable Bouncy Floor Modifier");
            modifierTaskActive = false;
            return true;
        }

        /// <summary>
        /// Enables the Bouncy Floor modifier
        /// </summary>
        public bool EnableModifier()
        {
            if (modifierTaskActive)
            {
                logger.Warning($"Failed to Enable Walk Speed Modifier");
                return false;
            }

            logger.Information($"Enable Bouncy Floor Modifier");
            modifierTaskActive = true;

            // Kick off a task to keep track of the player state and update as appropriate
            Task.Run(() =>
            {
                try
                {
                    bool isInAir = false;
                    while (modifierTaskActive)
                    {
                        bool justLanded = false;

                        // Check to see if the player just landed
                        PlayerState playerState = playerStateGetter.GetPlayerState();
                        if (playerState.IsOnGround)
                        {
                            if (isInAir)
                            {
                                isInAir = false;
                                justLanded = true;
                            }
                        }
                        else
                        {
                            isInAir = true;
                        }

                        // If we just landed, trigger a jump
                        if (justLanded)
                        {
                            JumpState previousJumpState = playerJumper.GetPreviousJumpState();
                            if (previousJumpState != null)
                            {
                                float newIntensity = previousJumpState.Intensity * BounceJumpModifier;
                                // TODO: Swap previous x value for previous velocity?? Confirm Bounce controls
                                if (newIntensity > MinimumIntensity)
                                {
                                    JumpState newJumpState = new JumpState(
                                        previousJumpState.Intensity * BounceJumpModifier, 
                                        previousJumpState.XValue);
                                    playerJumper.RequestJump(newJumpState);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Encountered exception during {this.GetType().Name} Modifier Task: {e.ToString()}");
                }
            });
            return true;
        }

    }
}
