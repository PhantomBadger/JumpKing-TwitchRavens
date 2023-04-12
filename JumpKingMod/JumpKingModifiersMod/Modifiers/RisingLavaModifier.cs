using JumpKing;
using JumpKing.SaveThread;
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
    /// An implementation of <see cref="IModifier"/> and <see cref="IDisposable"/> which has lava slowly rise from the bottom of the map. Touching the lava will mean death and a reset!
    /// </summary>
    /// TODO: Figure out how to handle teleports/horizontal screen transitions
    [Obsolete("Not yet complete!")]
    public class RisingLavaModifier : IModifier, IDisposable
    {
        public string DisplayName => "Rising Lava";

        private enum RisingLavaModifierState
        {
            Rising,                         // The Lava is rising as normal, we check for player contact
            PlayerDeathAnim,                // The player is forced into a Mario-esque death bounce
            ShrinkCutout,                   // A cutout fades across the screen like a Bowser cutout would in Mario
            ExpandCutout,                   // A cutout fades back revealing the player in the restarted position 
        }

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly ILogger logger;

        private WorldspaceImageEntity lavaEntity;
        private UIImageEntity deathPlayerEntity;
        private RisingLavaModifierState lavaModifierState;
        private Vector2 deathAnimVelocity;
        private float shrinkCutoutCounter;
        private float expandCutoutCounter;

        private const float SpawnYBuffer = 0;
        private const float ScreenHeight = 360;
        private const float LavaRisingSpeed = 2f;
        private const float DeathAnimUpVelocity = 3.5f;
        private const float Gravity = 9.8f;
        private const float ShrinkCutoutTimeInSeconds = 0.35f;
        private const float ExpandCutoutTimeInSeconds = 0.35f;

        /// <summary>
        /// Ctor for creating a <see cref="RisingLavaModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">A <see cref="ModifierUpdatingEntity"/> to register to for updates</param>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> to register created entities with</param>
        /// <param name="playerStateObserver">An implementation of <see cref="IPlayerStateObserver"/> for interacting with the player</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public RisingLavaModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager,
            IPlayerStateObserver playerStateObserver, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            lavaModifierState = RisingLavaModifierState.Rising;
            lavaEntity = null;

            modifierUpdatingEntity.RegisterModifier(this);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            modifierUpdatingEntity.UnregisterModifier(this);
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return lavaEntity != null;
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (lavaEntity == null)
            {
                logger.Error($"Failed to Disable 'Rising Lava' Modifier, it is already disabled!");
                return false;
            }

            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: false, isXVelocityDisabled: false);
            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: false);
            playerStateObserver.DisablePlayerBodyComp(isBodyCompDisabled: false);

            lavaEntity?.Dispose();
            lavaEntity = null;
            deathPlayerEntity?.Dispose();
            deathPlayerEntity = null;
            logger.Information($"Disabled 'Rising Lava' Modifier successfully!");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (lavaEntity != null)
            {
                logger.Error($"Failed to Enable 'Rising Lava' Modifier, it is already enabled!");
                return false;
            }

            // Spawn the lava at the bottom of the current screen
            float curScreenBottom = GetCurrentScreenBottomY();
            logger.Information($"current screen bottom: {curScreenBottom}");
            Vector2 spawnPos = new Vector2(240, curScreenBottom + (ModifiersModContentManager.LavaTexture.Height / 2) + SpawnYBuffer);
            lavaEntity = new WorldspaceImageEntity(modEntityManager,
                spawnPos,
                Sprite.CreateCenteredSprite(ModifiersModContentManager.LavaTexture, ModifiersModContentManager.LavaTexture.Bounds),
                zOrder: 2);
            logger.Information($"Enabled 'Rising Lava' Modifier successfully!");
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            PlayerState playerState = playerStateObserver.GetPlayerState();

            switch (lavaModifierState)
            {
                case RisingLavaModifierState.Rising:
                    {
                        // Make the Lava move up
                        lavaEntity.WorldSpacePosition -= new Vector2(0, p_delta * LavaRisingSpeed);

                        // Check to see if the player is intersecting with it
                        Rectangle playerHitbox = playerStateObserver.GetPlayerHitbox();
                        Vector2 playerBottomPosition = playerState.Position + new Vector2(0, playerHitbox.Height);
                        Vector2 lavaTopPos = lavaEntity.WorldSpacePosition - new Vector2(0, ModifiersModContentManager.LavaTexture.Height / 2);

                        if (lavaTopPos.Y < playerBottomPosition.Y)
                        {
                            // Disable the player
                            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: true, isXVelocityDisabled: true);
                            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: true);
                            playerStateObserver.DisablePlayerBodyComp(isBodyCompDisabled: true);

                            // Record the current information
                            deathAnimVelocity = new Vector2(0, -DeathAnimUpVelocity);

                            // Spawn a fake 'Death Player' sprite - TODO Replace Sprite lol
                            deathPlayerEntity = new UIImageEntity(
                                modEntityManager, 
                                Camera.TransformVector2(playerState.Position), 
                                Sprite.CreateSprite(ModifiersModContentManager.BloodSplatterRawTexture), 
                                zOrder: 3);

                            lavaModifierState = RisingLavaModifierState.PlayerDeathAnim;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.PlayerDeathAnim:
                    {
                        // Move the player up then down 'Mario Style'
                        deathPlayerEntity.ScreenSpacePosition += deathAnimVelocity;
                        deathAnimVelocity.Y += (Gravity * p_delta);
                        
                        // If we're off the bottom of the screen then go to the next stage
                        if (deathPlayerEntity.ScreenSpacePosition.Y > ScreenHeight)
                        {
                            deathPlayerEntity?.Dispose();
                            deathPlayerEntity = null;

                            shrinkCutoutCounter = 0;

                            // TODO: Spawn Fade

                            lavaModifierState = RisingLavaModifierState.ShrinkCutout;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.ShrinkCutout:
                    {
                        if ((shrinkCutoutCounter += p_delta) > ShrinkCutoutTimeInSeconds)
                        {
                            expandCutoutCounter = 0;

                            lavaModifierState = RisingLavaModifierState.ExpandCutout;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.ExpandCutout:
                    {
                        if ((expandCutoutCounter += p_delta) > ExpandCutoutTimeInSeconds)
                        {
                            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: false, isXVelocityDisabled: false);
                            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: false);
                            playerStateObserver.DisablePlayerBodyComp(isBodyCompDisabled: false);
                            playerStateObserver.RestartPlayerPosition();

                            lavaModifierState = RisingLavaModifierState.Rising;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Get the bottom Y position of the current screen
        /// </summary>
        private float GetCurrentScreenBottomY()
        {
            return (-Camera.Offset.Y) - ((Camera.CurrentScreen - 1) * ScreenHeight);
        }
    }
}
