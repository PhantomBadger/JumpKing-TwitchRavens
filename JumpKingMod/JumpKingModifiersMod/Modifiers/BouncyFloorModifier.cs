using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JumpKing.Player;
using System.Collections.Concurrent;
using JumpKingModifiersMod.Settings;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> that makes the floor bounce the player
    /// after each jump. The direction can be controlled by the player.
    /// </summary>
    [ConfigurableModifier("Bouncy", "Makes the player do a smaller jump after each normal jump, this bounce can be steered using the arrow keys before you land")]
    public class BouncyFloorModifier : IModifier, IDisposable
    {
        public string DisplayName { get; } = "Bouncy";

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly IPlayerStateObserver playerStateAccessor;
        private readonly IPlayerJumper playerJumper;
        private readonly ILogger logger;

        private bool modifierActive;
        private bool playerSplatted;
        private Task currentlyActiveBounceTask;

        private int bounceCounter;
        private bool isInAir;
        private PlayerState previousPlayerState;
        private ConcurrentQueue<JumpState> jumpsToProcess;

        private const float BounceJumpModifier = 0.25f;
        private const int MaxNumberOfBounces = 1;

        /// <summary>
        /// Ctor for creating a <see cref="BouncyFloorModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">The <see cref="ModifierUpdatingEntity"/> to register to, which will call out Update method for us</param>
        /// <param name="playerStateAccessor">An implementation of <see cref="IPlayerStateObserver"/> for interacting with the player state</param>
        /// <param name="playerJumper">An implementation of <see cref="IPlayerJumper"/> for interacting with player jumps</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public BouncyFloorModifier(ModifierUpdatingEntity modifierUpdatingEntity, IPlayerStateObserver playerStateAccessor, IPlayerJumper playerJumper, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.playerStateAccessor = playerStateAccessor ?? throw new ArgumentNullException(nameof(playerStateAccessor));
            this.playerJumper = playerJumper ?? throw new ArgumentNullException(nameof(playerJumper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            playerJumper.OnPlayerJumped += OnPlayerJumped;
            PlayerEntity.OnSplatCall =
                (PlayerEntity.OnSplat)Delegate.Combine(PlayerEntity.OnSplatCall, 
                new PlayerEntity.OnSplat(this.OnSplat));

            bounceCounter = MaxNumberOfBounces;
            isInAir = false;
            previousPlayerState = null;
            jumpsToProcess = new ConcurrentQueue<JumpState>();

            modifierUpdatingEntity.RegisterModifier(this);
        }

        /// <summary>
        /// Implements <see cref="IDisposable.Dispose"/> to unhook any events
        /// </summary>
        public void Dispose()
        {
            playerJumper.OnPlayerJumped -= OnPlayerJumped;
            Delegate.Remove(PlayerEntity.OnSplatCall, new PlayerEntity.OnSplat(this.OnSplat));

            modifierUpdatingEntity.UnregisterModifier(this);
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
            return modifierActive;
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!modifierActive)
            {
                logger.Warning($"Failed to Disable Bouncy Floor Modifier");
                return false;
            }
            logger.Information($"Disable Bouncy Floor Modifier");
            modifierActive = false;

            // Ensure we're properly cleaning up
            currentlyActiveBounceTask = null;
            playerStateAccessor.SetKnockedStateOverride(isActive: false, newState: false);
            playerSplatted = false;

            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (modifierActive)
            {
                logger.Warning($"Failed to Enable Walk Speed Modifier");
                return false;
            }

            logger.Information($"Enable Bouncy Floor Modifier");
            modifierActive = true;
            return true;
        }

        /// <summary>
        /// Called when the player jumps
        /// </summary>
        private void OnPlayerJumped(JumpState jumpState)
        {
            if (!modifierActive)
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

            bounceCounter = MaxNumberOfBounces;
            isInAir = false;
            previousPlayerState = null;

            jumpsToProcess.Enqueue(jumpState);
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            try
            {
                if (playerSplatted)
                {
                    playerSplatted = false;

                    // No bounce needed to be processed, consume any lingering jumps
                    while (jumpsToProcess.TryDequeue(out _)) { }
                    return;
                }

                bool justLanded = false;

                // Check to see if the player just landed
                PlayerState playerState = playerStateAccessor.GetPlayerState();
                if (playerState == null)
                {
                    return;
                }

                if (playerState.IsOnGround)
                {
                    if (isInAir)
                    {
                        playerStateAccessor.SetKnockedStateOverride(isActive: false, newState: false);
                        isInAir = false;
                        justLanded = true;
                    }
                }
                else
                {
                    isInAir = true;
                }

                // If we just landed, trigger a jump
                if (justLanded && previousPlayerState != null &&
                    jumpsToProcess.TryDequeue(out JumpState previousJumpState))
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

                    // If we have bounces remaining, queue up another jump to respond to
                    if (--bounceCounter > 0)
                    {
                        jumpsToProcess.Enqueue(newJumpState);
                    }
                }

                previousPlayerState = playerState;
            }
            catch (Exception e)
            {
                logger.Error($"Encountered exception during {this.GetType().Name} Modifier Task: {e.ToString()}");
            }
        }
    }
}
