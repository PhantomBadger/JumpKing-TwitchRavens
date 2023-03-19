using JumpKing;
using JumpKing.Player;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> and <see cref="IDisposable"/> which gives a health bar to the player
    /// and loses health whenever they fall
    /// </summary>
    public class FallDamageModifier : IModifier, IDisposable
    {
        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly IGameStateObserver gameStateObserver;
        private readonly ILogger logger;

        // TODO - Swap to be a health bar
        private UITextEntity healthEntity;
        private int healthValue;
        private Vector2? lastOnGroundPosition;
        private bool processSplat;
        private bool reActivateModifier;

        private const int MaxHealthValue = 100;
        private const float DistanceDamageModifier = 0.1f;

        /// <summary>
        /// Ctor for creating a <see cref="FallDamageModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">A <see cref="ModifierUpdatingEntity"/> to hook our update method into</param>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> to use for creating new entities</param>
        /// <param name="playerStateObserver">An implementation of <see cref="IPlayerStateObserver"/> for getting the current player state</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public FallDamageModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager,
            IPlayerStateObserver playerStateObserver, IGameStateObserver gameStateObserver, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.gameStateObserver = gameStateObserver ?? throw new ArgumentNullException(nameof(gameStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            gameStateObserver.OnGameLoopNotRunning += OnGameLoopNotRunning;
            gameStateObserver.OnGameLoopRunning += OnGameLoopRunning;
            PlayerEntity.OnSplatCall =
                (PlayerEntity.OnSplat)Delegate.Combine(PlayerEntity.OnSplatCall,
                new PlayerEntity.OnSplat(this.OnSplat));

            modifierUpdatingEntity.RegisterModifier(this);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> which cleans up the entities
        /// </summary>
        public void Dispose()
        {
            healthEntity?.Dispose();
            modifierUpdatingEntity.UnregisterModifier(this);
            gameStateObserver.OnGameLoopNotRunning -= OnGameLoopNotRunning;
        }

        /// <summary>
        /// If the game loop stops running we cancel the modifier
        /// </summary>
        private void OnGameLoopNotRunning()
        {
            if (IsModifierEnabled())
            {
                DisableModifier();
                reActivateModifier = true;
            }
        }

        /// <summary>
        /// If the game loop restarts after we auto disable we re-enable
        /// </summary>
        private void OnGameLoopRunning()
        {
            if (reActivateModifier && !IsModifierEnabled())
            {
                EnableModifier();
            }
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return healthEntity != null;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (healthEntity != null)
            {
                // Already active
                logger.Information($"Failed to Enable Fall Damage Modifier - Effect already active");
                return false;
            }

            Vector2? healthPosition = GetScreenSpacePositionForHealthEntity(out _);
            if (!healthPosition.HasValue)
            {
                logger.Information($"Failed to Enable Fall Damage Modifier - Unable to get a player position");
                return false;
            }

            logger.Information($"Enable Fall Damage Modifier");
            healthEntity = new UITextEntity(
                modEntityManager,
                healthPosition.Value,
                healthValue.ToString(),
                Color.Red,
                UITextEntityAnchor.Center,
                JKContentManager.Font.MenuFontSmall);

            SetDefaultHealthState();
            return true;
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (healthEntity == null)
            {
                // Already inactive
                logger.Information($"Failed to Disable Fall Damage Modifier");
                return false;
            }

            logger.Information($"Disable Fall Damage Modifier");
            healthEntity?.Dispose();
            healthEntity = null;
            lastOnGroundPosition = null;
            processSplat = false;
            return true;
        }

        /// <inheritdoc/>
        public void Update()
        {
            Vector2? position = GetScreenSpacePositionForHealthEntity(out PlayerState playerState);
            if (!position.HasValue)
            {
                return;
            }
            healthEntity.ScreenSpacePosition = position.Value;
            healthEntity.TextValue = healthValue.ToString();

            if (processSplat && lastOnGroundPosition.HasValue)
            {
                processSplat = false;
                Vector2 newSplatPosition = playerState.Position;

                // Get the distance fallen and turn that into a damage value
                float yDiff = Math.Abs(lastOnGroundPosition.Value.Y - newSplatPosition.Y);
                float rawDamage = (yDiff * DistanceDamageModifier);
                int damage = Math.Max(1, (int)rawDamage);

                // Apply the damage to the health
                logger.Information($"Dealing Damage of '{damage}' (Raw damage '{rawDamage}')");
                healthValue = Math.Max(0, healthValue - damage);
            }
            else if (playerState.IsOnGround)
            {
                lastOnGroundPosition = playerState.Position;
            }

            // We have died, enter the death state and restart
            if (healthValue <= 0)
            {
                logger.Information($"Dead!");
                playerStateObserver.RestartPlayerPosition();
                SetDefaultHealthState();
            }
        }

        /// <summary>
        /// Called by JumpKing when the player splats on the ground
        /// </summary>
        private void OnSplat()
        {
            logger.Information($"Received Splat notification");
            processSplat = true;
        }

        /// <summary>
        /// Gets the current position for the health entity based on the player's state
        /// </summary>
        private Vector2? GetScreenSpacePositionForHealthEntity(out PlayerState playerState)
        {
            playerState = playerStateObserver.GetPlayerState();
            if (playerState != null)
            {
                Vector2 healthPosition = playerState.Position + new Vector2(0, -20);
                return Camera.TransformVector2(healthPosition);
            }
            return null;
        }

        /// <summary>
        /// Sets the health value to max and applies it to the health entity
        /// </summary>
        private void SetDefaultHealthState()
        {
            if (healthEntity != null)
            {
                healthValue = MaxHealthValue;
                healthEntity.TextValue = healthValue.ToString();
            }
        }
    }
}
