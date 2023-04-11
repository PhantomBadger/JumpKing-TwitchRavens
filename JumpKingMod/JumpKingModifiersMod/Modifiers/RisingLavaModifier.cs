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
            FadingCutout,                   // A cutout fades across the screen like a Bowser cutout would in Mario
            DisplayingYouDied,              // Displaying the primary "You Died" text
            DisplayingYouDiedSubtext,       // Displaying the secondary "You Died" subtext
            WaitingForInput                 // Waiting for user input to reset
        }

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly ILogger logger;

        private WorldspaceImageEntity lavaEntity;
        private WorldspaceImageEntity deathPlayerEntity;
        private RisingLavaModifierState lavaModifierState;
        private Vector2 deathAnimVelocity;
        private float deathScreenBottom;

        private const float SpawnYBuffer = 0;
        private const float ScreenHeight = 360;
        private const float LavaRisingSpeed = 2f;
        private const float DeathAnimUpVelocity = 25f;
        private const float Gravity = 9.8f;

        public RisingLavaModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager,
            IPlayerStateObserver playerStateObserver, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentException(nameof(playerStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            lavaModifierState = RisingLavaModifierState.Rising;
            lavaEntity = null;

            modifierUpdatingEntity.RegisterModifier(this);
        }

        public void Dispose()
        {
            modifierUpdatingEntity.UnregisterModifier(this);
        }

        public bool IsModifierEnabled()
        {
            return lavaEntity != null;
        }

        public bool DisableModifier()
        {
            if (lavaEntity == null)
            {
                logger.Error($"Failed to Disable 'Rising Lava' Modifier, it is already disabled!");
                return false;
            }

            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: false, isXVelocityDisabled: false);
            playerStateObserver.SetKnockedStateOverride(isActive: false, newState: false);
            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: false);

            lavaEntity.Dispose();
            lavaEntity = null;
            logger.Information($"Disabled 'Rising Lava' Modifier successfully!");
            return true;
        }

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
                        logger.Information($"Current lava top pos: {lavaTopPos}");
                        if (lavaTopPos.Y < playerBottomPosition.Y)
                        {
                            // Disable the player
                            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: true, isXVelocityDisabled: true);
                            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: true);

                            // Record the current information
                            deathAnimVelocity = new Vector2(0, -DeathAnimUpVelocity);
                            deathScreenBottom = GetCurrentScreenBottomY();

                            // Spawn a fake 'Death Player' sprite - TODO
                            //deathPlayerEntity = new WorldspaceImageEntity(modEntityManager, playerState.Position, )

                            lavaModifierState = RisingLavaModifierState.PlayerDeathAnim;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.PlayerDeathAnim:
                    {
                        // Move the player up then down 'Mario Style'
                        
                        //playerStateObserver.SetPosition(playerState.Position + deathAnimVelocity);
                        //deathAnimVelocity.Y += (Gravity * p_delta);
                        //if (playerState.Position.Y > deathScreenBottom)
                        //{
                        //    logger.Information($"FELL OFF BOTTOM");
                        //}
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
