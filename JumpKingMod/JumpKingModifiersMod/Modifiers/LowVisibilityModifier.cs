using JumpKing;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
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
    /// An implementation of <see cref="IModifier"/> which displays a low visibility overlay over the screen
    /// </summary>
    [ConfigurableModifier("Low Visibility")]
    public class LowVisibilityModifier : IModifier, IDisposable
    {
        public string DisplayName => "Low Visibility";

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly ILogger logger;

        private UIImageEntity lowVisiblityOverlay;

        /// <summary>
        /// Ctor for creating a <see cref="LowVisibilityModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">A <see cref="ModifierUpdatingEntity"/> to register our update call to</param>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> to register ui entities to</param>
        /// <param name="playerStateObserver">An implementation of <see cref="IPlayerStateObserver"/> to get player info from</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public LowVisibilityModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager, IPlayerStateObserver playerStateObserver, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            return lowVisiblityOverlay != null;
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (lowVisiblityOverlay == null)
            {
                logger.Error($"Failed to Disable 'Low Visibility' Modifier as it's already disabled");
                return false;
            }

            lowVisiblityOverlay?.Dispose();
            lowVisiblityOverlay = null;
            logger.Information($"Disabled 'Low Visibility' Modifier!");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (lowVisiblityOverlay != null)
            {
                logger.Error($"Failed to Enable 'Low Visibility' Modifier as it's already enabled");
                return false;
            }

            PlayerState playerState = playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                logger.Error($"Failed to Enable 'Low Visibility' Modifier as we were unable to get a player state");
                return false;
            }

            // Make the overlay
            lowVisiblityOverlay = new UIImageEntity(modEntityManager, GetCenterScreenSpacePositionOfOverlay(playerState.Position),
                Sprite.CreateSprite(ModifiersModContentManager.LowVisibilityOverlayTexture), zOrder: 1);
            logger.Information($"Enabled 'Low Visibility' Modifier!");
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Update the overlay's position
            PlayerState playerState = playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                logger.Error($"Unable to get player state when trying to update 'Low Visibility' modifier");
                return;
            }

            lowVisiblityOverlay.ScreenSpacePosition = GetCenterScreenSpacePositionOfOverlay(playerState.Position);
        }

        /// <summary>
        /// Gets a centered screen space position for the low visibility overlay
        /// </summary>
        private Vector2 GetCenterScreenSpacePositionOfOverlay(Vector2 worldSpacePlayerPosition)
        {
            return Camera.TransformVector2(worldSpacePlayerPosition) -
                new Vector2(ModifiersModContentManager.LowVisibilityOverlayTexture.Width / 2,
                            ModifiersModContentManager.LowVisibilityOverlayTexture.Height / 2);
        }
    }
}
