using JumpKing;
using JumpKing.Player;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        private enum FallDamageModifierState
        {
            Playing,
            DisplayingYouDied,
            DisplayingYouDiedSubtext,
            WaitingForInput
        }

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly IGameStateObserver gameStateObserver;
        private readonly ILogger logger;

        // TODO - Swap to be a health bar
        private UITextEntity healthEntity;
        private UIImageEntity youDiedEntity;
        private UITextEntity youDiedSubtextEntity;
        private UITextEntity userPromptTextEntity;
        private int healthValue;
        private Vector2? lastOnGroundPosition;
        private bool processSplat;
        private bool reActivateModifier;
        private FallDamageModifierState fallModifierState;
        private float youDiedAlphaLerpCounter = 0;
        private float youDiedSubtextAlphaLerpCounter = 0;
        private float userPromptPulseCounter = 0;

        private const int MaxHealthValue = 100;
        private const float DistanceDamageModifier = 0.1f;
        private const float TimeTakenToShowYouDiedInSeconds = 2f;
        private const float TimeTakenToShowYouDiedSubtextInSeconds = 1f;
        private const float UserPromptPulseTimeInSeconds = 0.75f;
        private const Keys UserPromptKey = Keys.Space;

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

            fallModifierState = FallDamageModifierState.Playing;

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
            youDiedEntity?.Dispose();
            youDiedSubtextEntity?.Dispose();
            userPromptTextEntity?.Dispose();

            healthEntity = null;
            youDiedEntity = null;
            youDiedSubtextEntity = null;
            userPromptTextEntity = null;

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

            fallModifierState = FallDamageModifierState.Playing;

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
                UIEntityAnchor.Center,
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
            youDiedEntity?.Dispose();
            youDiedSubtextEntity?.Dispose();
            userPromptTextEntity?.Dispose();

            healthEntity = null;
            youDiedEntity = null;
            youDiedSubtextEntity = null;
            userPromptTextEntity = null;

            lastOnGroundPosition = null;
            processSplat = false;
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            switch (fallModifierState)
            {
                case FallDamageModifierState.Playing:
                    {
                        PlayingUpdate(p_delta);
                        break;
                    }
                case FallDamageModifierState.DisplayingYouDied:
                    {
                        DisplayingYouDiedUpdate(p_delta);
                        break;
                    }
                case FallDamageModifierState.DisplayingYouDiedSubtext:
                    {
                        DisplayingYouDiedSubtextUpdate(p_delta);
                        break;
                    }
                case FallDamageModifierState.WaitingForInput:
                    {
                        WaitingForInputUpdate(p_delta);
                        break;
                    }
            }
        }

        /// <summary>
        /// The update loop for the <see cref="FallDamageModifierState.Playing"/> state
        /// </summary>
        private void PlayingUpdate(float p_delta)
        {
            Vector2? position = GetScreenSpacePositionForHealthEntity(out PlayerState playerState);
            if (!position.HasValue)
            {
                return;
            }

            // Apply any damage thats pending
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

            // Update the text
            healthEntity.ScreenSpacePosition = position.Value;
            healthEntity.TextValue = healthValue.ToString();

            // We have died, enter the death state and restart
            if (healthValue <= 0)
            {
                Sprite youDiedSprite = Sprite.CreateSprite(ModifiersModContentManager.YouDiedTexture);
                youDiedEntity = new UIImageEntity(modEntityManager, new Vector2(0, 0), youDiedSprite, zOrder: 1);
                youDiedEntity.ImageValue.SetAlpha(0);
                youDiedAlphaLerpCounter = 0;

                fallModifierState = FallDamageModifierState.DisplayingYouDied;
                logger.Information($"Setting Modifier State to {fallModifierState.ToString()}!");
            }
        }

        /// <summary>
        /// The update loop for the <see cref="FallDamageModifierState.DisplayingYouDied"/> state
        /// </summary>
        private void DisplayingYouDiedUpdate(float p_delta)
        {
            // Lerp between 0 and 1 for the Alpha to fade in the texture
            youDiedAlphaLerpCounter += p_delta;
            float alphaValue = (youDiedAlphaLerpCounter / TimeTakenToShowYouDiedInSeconds);
            youDiedEntity.ImageValue.SetAlpha(alphaValue);

            // Once fully faded in we move to the Waiting for Input state
            if (alphaValue >= 1)
            {
                youDiedEntity.ImageValue.SetAlpha(1);

                youDiedSubtextEntity = new UITextEntity(modEntityManager,
                    new Vector2(240, 245),
                    "That's gotta be embarrassing...",
                    Color.White,
                    UIEntityAnchor.Center,
                    zOrder: 2);
                youDiedSubtextEntity.TextColor = new Color(Color.White, 0.0f);
                youDiedSubtextAlphaLerpCounter = 0;

                fallModifierState = FallDamageModifierState.DisplayingYouDiedSubtext;
                logger.Information($"Setting Modifier State to {fallModifierState.ToString()}!");
            }
        }

        /// <summary>
        /// The update loop for the <see cref="FallDamageModifierState.DisplayingYouDiedSubtext"/> state
        /// </summary>
        private void DisplayingYouDiedSubtextUpdate(float p_delta)
        {
            youDiedSubtextAlphaLerpCounter += p_delta;
            float alphaValue = (youDiedSubtextAlphaLerpCounter / TimeTakenToShowYouDiedSubtextInSeconds);
            youDiedSubtextEntity.TextColor = new Color(Color.White, alphaValue);

            if (alphaValue >= 1)
            {
                userPromptTextEntity = new UITextEntity(modEntityManager,
                    new Vector2(240, 260),
                    "Press Jump to restart!",
                    Color.White,
                    UIEntityAnchor.Center,
                    JKContentManager.Font.MenuFontSmall,
                    zOrder: 2);
                userPromptPulseCounter = 0;

                youDiedSubtextEntity.TextColor = Color.White;
                fallModifierState = FallDamageModifierState.WaitingForInput;
                logger.Information($"Setting Modifier State to {fallModifierState.ToString()}!");
            }
        }

        /// <summary>
        /// The update loop for the <see cref="FallDamageModifierState.WaitingForInput"/> state
        /// </summary>
        private void WaitingForInputUpdate(float p_delta)
        {
            // Pulse the user prompt
            userPromptPulseCounter += p_delta;
            if (userPromptPulseCounter > UserPromptPulseTimeInSeconds)
            {
                userPromptPulseCounter -= UserPromptPulseTimeInSeconds;
                if (userPromptTextEntity.TextColor.A > 0)
                {
                    userPromptTextEntity.TextColor = new Color(userPromptTextEntity.TextColor, 0f);
                }
                else
                {
                    userPromptTextEntity.TextColor = new Color(userPromptTextEntity.TextColor, 1f);
                }
            }

            // TODO: Swap to use actual jump key to account for keyboard users and such
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(UserPromptKey))
            {
                // Clear up the You Died & Text Entities
                youDiedEntity?.Dispose();
                youDiedEntity = null;

                youDiedSubtextEntity?.Dispose();
                youDiedSubtextEntity = null;

                userPromptTextEntity?.Dispose();
                userPromptTextEntity = null;

                // Restart the player position and reset the health
                playerStateObserver.RestartPlayerPosition();
                SetDefaultHealthState();
                fallModifierState = FallDamageModifierState.Playing;
                logger.Information($"Setting Modifier State to {fallModifierState.ToString()}!");
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
