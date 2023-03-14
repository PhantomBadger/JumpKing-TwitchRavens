using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JumpKing.Player;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> that makes the floor bounce the player
    /// after each jump. The direction can be controlled by the player.
    /// </summary>
    public class BouncyFloorModifier : IModifier, IDisposable
    {
        private readonly IPlayerStateAccessor playerStateAccessor;
        private readonly IPlayerJumper playerJumper;
        private readonly ILogger logger;

        private bool modifierTaskActive;
        private bool playerSplatted;
        private Task currentlyActiveBounceTask;

        private const float BounceJumpModifier = 0.25f;
        private const int MaxNumberOfBounces = 1;

        /// <summary>
        /// Ctor for creating a <see cref="BouncyFloorModifier"/>
        /// </summary>
        /// <param name="playerStateAccessor">An implementation of <see cref="IPlayerStateAccessor"/> for interacting with the player state</param>
        /// <param name="playerJumper">An implementation of <see cref="IPlayerJumper"/> for interacting with player jumps</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public BouncyFloorModifier(IPlayerStateAccessor playerStateAccessor, IPlayerJumper playerJumper, ILogger logger)
        {
            this.playerStateAccessor = playerStateAccessor ?? throw new ArgumentNullException(nameof(playerStateAccessor));
            this.playerJumper = playerJumper ?? throw new ArgumentNullException(nameof(playerJumper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            playerJumper.OnPlayerJumped += OnPlayerJumped;
            PlayerEntity.OnSplatCall =
                (PlayerEntity.OnSplat)Delegate.Combine(PlayerEntity.OnSplatCall, 
                new PlayerEntity.OnSplat(this.OnSplat));
        }

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/> to unhook any events
        /// </summary>
        public void Dispose()
        {
            playerJumper.OnPlayerJumped -= OnPlayerJumped;
        }

        /// <summary>
        /// Called when the player splats
        /// </summary>
        public void OnSplat()
        {
            playerSplatted = true;
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
            return true;
        }

        private void OnPlayerJumped(JumpState jumpState)
        {
            if (!modifierTaskActive)
            {
                // Modifier isnt yet active
                playerSplatted = false;
                return;
            }

            if (currentlyActiveBounceTask != null)
            {
                // Already processing a bounce
                return;
            }

            // Kick off a task to keep track of the player state and update as appropriate
            currentlyActiveBounceTask = Task.Run(() =>
            {
                try
                {
                    // Keep track of the 'global states'
                    int bounceCounter = MaxNumberOfBounces;
                    bool isInAir = false;
                    PlayerState previousPlayerState = null;

                    // Continue until we shouldnt be active anymore or something exits
                    while (modifierTaskActive)
                    {
                        if (playerSplatted)
                        {
                            playerSplatted = false;

                            // No longer bouncing
                            return;
                        }

                        bool justLanded = false;

                        // Check to see if the player just landed
                        PlayerState playerState = playerStateAccessor.GetPlayerState();
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
                        if (justLanded && previousPlayerState != null)
                        {
                            JumpState previousJumpState = playerJumper.GetPreviousJumpState();
                            if (previousJumpState != null)
                            {
                                // Update the intensity by our modifier
                                float newIntensity = previousJumpState.Intensity * BounceJumpModifier;

                                // Get a default direction based on the previous state's velocity
                                int previousVelocityX = 0;
                                if (previousPlayerState.Velocity.X > 0)
                                {
                                    previousVelocityX = 1;
                                }
                                else if (previousPlayerState.Velocity.X < 0)
                                {
                                    previousVelocityX = -1;
                                }

                                // If we still have some bounces left, request it
                                if (bounceCounter-- > 0)
                                {
                                    playerStateAccessor.SetKnockedStateOverride(isActive: true, newState: true);

                                    // 'Override X Value' will mean that if the user is holding a direction
                                    // that X will be used instead of the one we provide, this feels a bit better for a user
                                    // and gives them some control
                                    // 'Override Direction Opposite' will flip the player's sprite to be opposite to their
                                    // X value, this lets us better simulate a knocked state
                                    RequestedJumpState newJumpState = new RequestedJumpState(
                                        previousJumpState.Intensity * BounceJumpModifier,
                                        previousVelocityX,
                                        overrideXValue: true,
                                        overrideDirectionOpposite: true);
                                    playerJumper.RequestJump(newJumpState);
                                }
                                else
                                {
                                    // Done bouncing, exit the loop
                                    return;
                                }
                            }
                        }

                        previousPlayerState = playerState;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Encountered exception during {this.GetType().Name} Modifier Task: {e.ToString()}");
                }
                finally
                {
                    // Ensure we're properly cleaning up
                    currentlyActiveBounceTask = null;
                    playerStateAccessor.SetKnockedStateOverride(isActive: false, newState: false);
                    playerSplatted = false;
                }
            });
        }
    }
}
