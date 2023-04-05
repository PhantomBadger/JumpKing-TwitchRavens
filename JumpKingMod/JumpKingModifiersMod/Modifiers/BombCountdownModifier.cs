using JumpKing;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/>  and <see cref="IDisposable"/> which maintains a countdown on the player
    /// once it hits zero a punishment is incurred, the countdown is staved by jumping
    /// </summary>
    public class BombCountdownModifier : IModifier, IDisposable
    {
        public string DisplayName { get; } = "Bomb Countdown";

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly IPlayerJumper playerJumper;
        private readonly ILogger logger;
        private readonly float countdownStartingValueInSeconds;

        private UITextEntity uiTextEntity;
        private float currentTimerCountdownInSeconds;


        /// <summary>
        /// Ctor for creating a <see cref="BombCountdownModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">The <see cref="ModifierUpdatingEntity"/> to use to call update</param>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to add other entities to</param>
        /// <param name="playerStateObserver">An implementation of <see cref="IPlayerStateObserver"/> for interacting with the player</param>
        /// <param name="playerJumper">An implementation of <see cref="IPlayerJumper"/> for dealing with player jumping</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation used to log to</param>
        public BombCountdownModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager, 
            IPlayerStateObserver playerStateObserver, IPlayerJumper playerJumper, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.playerJumper = playerJumper ?? throw new ArgumentNullException(nameof(playerJumper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            uiTextEntity = null;

            // TODO: Swap to a setting
            countdownStartingValueInSeconds = 5.0f;
            currentTimerCountdownInSeconds = countdownStartingValueInSeconds;

            modifierUpdatingEntity.RegisterModifier(this);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            DisableModifier();
            modifierUpdatingEntity.UnregisterModifier(this);
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return uiTextEntity != null;
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (uiTextEntity == null)
            {
                logger.Information($"Failed to disable 'Bomb Coutndown' modifier as it is already disabled!");
                return false;
            }
            uiTextEntity.Dispose();
            uiTextEntity = null;
            logger.Information("Disabling 'Bomb Countdown' modifier!");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (uiTextEntity != null)
            {
                logger.Information($"Failed to enable 'Bomb Countdown' modifier as it is already enabled!");
                return false;
            }

            PlayerState playerState = playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                logger.Information($"Failed to enable 'Bomb Countdown' modifier as the player state was null!");
                return false;
            }

            // Reset the counter
            currentTimerCountdownInSeconds = countdownStartingValueInSeconds;

            // TODO: Position properly
            uiTextEntity = new UITextEntity(modEntityManager, Camera.TransformVector2(playerState.Position),
                currentTimerCountdownInSeconds.ToString(), Color.White, UIEntityAnchor.BottomLeft);
            logger.Information("Enabling the 'Bomb Countdown' modifier!");
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            PlayerState playerState = playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                return;
            }

            // Update the countdown value
            currentTimerCountdownInSeconds -= p_delta;
            currentTimerCountdownInSeconds = (float)Math.Round(currentTimerCountdownInSeconds, 2);

            if (currentTimerCountdownInSeconds < 0)
            {
                currentTimerCountdownInSeconds = 0.0f;
                // TODO: bad thing
            }

            uiTextEntity.TextValue = currentTimerCountdownInSeconds.ToString();
            uiTextEntity.ScreenSpacePosition = Camera.TransformVector2(playerState.Position);
        }
    }
}
