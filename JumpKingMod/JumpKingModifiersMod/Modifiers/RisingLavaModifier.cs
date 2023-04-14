using JumpKing;
using JumpKing.SaveThread;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Patching.Teleporting;
using JumpKingModifiersMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using PBJKModBase.Components;
using PBJKModBase.Entities;
using Settings;
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
    public class RisingLavaModifier : IModifier, IDisposable
    {
        public string DisplayName => "Rising Lava";

        private enum RisingLavaModifierState
        {
            Rising,                         // The Lava is rising as normal, we check for player contact
            PlayerDeathAnim,                // The player is forced into a Mario-esque death bounce
            ShrinkCutout,                   // A cutout fades across the screen like a Bowser cutout would in Mario
            CutoutPause,                    // A pause before fading back out
            ExpandCutout,                   // A cutout fades back revealing the player in the restarted position 
        }

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly ILogger logger;
        private readonly float lavaRisingSpeed;
        private readonly bool niceSpawns;

        private WorldspaceImageEntity lavaEntity;
        private UIImageEntity deathPlayerEntity;
        private RisingLavaModifierState lavaModifierState;
        private Vector2 deathAnimVelocity;
        private AnimationComponent cutoutShrinkAnimationComponent;
        private AnimationComponent cutoutExpandAnimationComponent;
        private UIImageEntity cutoutEntity;
        private float cutoutPauseCounter;
        private float oscillationCounter;

        private const float SpawnYBuffer = 0;
        private const float ScreenHeight = 360;
        private const float DeathAnimUpVelocity = 3.5f;
        private const float Gravity = 9.8f;
        private const float ShrinkCutoutTimeInSeconds = 0.35f;
        private const float ExpandCutoutTimeInSeconds = 0.2f;
        private const float CutoutPauseTimeInSeconds = 0.2f;

        /// <summary>
        /// Ctor for creating a <see cref="RisingLavaModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">A <see cref="ModifierUpdatingEntity"/> to register to for updates</param>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> to register created entities with</param>
        /// <param name="playerStateObserver">An implementation of <see cref="IPlayerStateObserver"/> for interacting with the player</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public RisingLavaModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager,
            IPlayerStateObserver playerStateObserver, UserSettings userSettings, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            lavaModifierState = RisingLavaModifierState.Rising;
            lavaEntity = null;

            lavaRisingSpeed = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaSpeedKey, JumpKingModifiersModSettingsContext.DefaultRisingLavaSpeed);
            niceSpawns = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaNiceSpawnsKey, true);

            // Set up the animation components
            cutoutShrinkAnimationComponent = new AnimationComponent(
                ModifiersModContentManager.CutoutSprites, 
                ShrinkCutoutTimeInSeconds / ModifiersModContentManager.CutoutSprites.Length,
                looping: false);

            Sprite[] reverseCutout = ModifiersModContentManager.CutoutSprites.Reverse().ToArray();
            cutoutExpandAnimationComponent = new AnimationComponent(
                reverseCutout,
                ExpandCutoutTimeInSeconds / reverseCutout.Length,
                looping: false);

            cutoutPauseCounter = 0;
            oscillationCounter = 0;

            playerStateObserver.OnPlayerTeleported += OnPlayerTeleported;
            playerStateObserver.OnPlayerPositionRestarted += OnPlayerPositionRestarted;
            modifierUpdatingEntity.RegisterModifier(this);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            playerStateObserver.OnPlayerTeleported -= OnPlayerTeleported;
            playerStateObserver.OnPlayerPositionRestarted -= OnPlayerPositionRestarted;
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

            DisableInternal();

            logger.Information($"Disabled 'Rising Lava' Modifier successfully!");
            return true;
        }

        private void DisableInternal()
        {
            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: false, isXVelocityDisabled: false);
            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: false);
            playerStateObserver.DisablePlayerBodyComp(isBodyCompDisabled: false);

            cutoutShrinkAnimationComponent.ResetAnimation();
            cutoutExpandAnimationComponent.ResetAnimation();
            oscillationCounter = 0;

            cutoutEntity?.Dispose();
            cutoutEntity = null;
            lavaEntity?.Dispose();
            lavaEntity = null;
            deathPlayerEntity?.Dispose();
            deathPlayerEntity = null;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (lavaEntity != null)
            {
                logger.Error($"Failed to Enable 'Rising Lava' Modifier, it is already enabled!");
                return false;
            }

            PlayerState playerState = playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                logger.Error($"Failed to Enable 'Rising Lava' Modifier, the player is not yet initialised!");
                return false;
            }

            EnableInternal();

            logger.Information($"Enabled 'Rising Lava' Modifier successfully!");
            return true;
        }

        private void EnableInternal()
        {
            lavaModifierState = RisingLavaModifierState.Rising;
            oscillationCounter = 0;

            // Spawn the lava at the bottom of the current screen
            lavaEntity = new WorldspaceImageEntity(modEntityManager,
                GetLavaSpawnPos(),
                Sprite.CreateCenteredSprite(ModifiersModContentManager.LavaTexture, ModifiersModContentManager.LavaTexture.Bounds),
                zOrder: 2);
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            PlayerState playerState = playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                return;
            }

            switch (lavaModifierState)
            {
                case RisingLavaModifierState.Rising:
                    {
                        // Make the Lava move up & wiggle
                        float xOscillation = ((float)Math.Sin(oscillationCounter += p_delta) / 30f);
                        oscillationCounter %= (float)(2 * Math.PI);
                        lavaEntity.WorldSpacePosition -= new Vector2(xOscillation, p_delta * lavaRisingSpeed);

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
                                Sprite.CreateSprite(ModifiersModContentManager.KingDeathTexture), 
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

                            cutoutShrinkAnimationComponent.ResetAnimation();
                            cutoutEntity = new UIImageEntity(modEntityManager, Vector2.Zero, cutoutShrinkAnimationComponent.GetActiveSprite(), zOrder: 4);

                            lavaModifierState = RisingLavaModifierState.ShrinkCutout;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.ShrinkCutout:
                    {
                        cutoutShrinkAnimationComponent.Update(p_delta);
                        cutoutEntity.ImageValue = cutoutShrinkAnimationComponent.GetActiveSprite();

                        if (cutoutShrinkAnimationComponent.IsAtEndOfAnimation())
                        {
                            playerStateObserver.DisablePlayerWalking(isWalkingDisabled: false, isXVelocityDisabled: false);
                            playerStateObserver.DisablePlayerDrawing(isDrawDisabled: false);
                            playerStateObserver.DisablePlayerBodyComp(isBodyCompDisabled: false);
                            playerStateObserver.RestartPlayerPosition(niceSpawns);
                            
                            cutoutPauseCounter = 0;

                            lavaModifierState = RisingLavaModifierState.CutoutPause;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.CutoutPause:
                    {
                        if ((cutoutPauseCounter += p_delta) > CutoutPauseTimeInSeconds)
                        {
                            cutoutShrinkAnimationComponent.ResetAnimation();
                            cutoutExpandAnimationComponent.ResetAnimation();
                            cutoutEntity.ImageValue = cutoutExpandAnimationComponent.GetActiveSprite();

                            lavaEntity.WorldSpacePosition = GetLavaSpawnPos();
                            oscillationCounter = 0;

                            lavaModifierState = RisingLavaModifierState.ExpandCutout;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
                case RisingLavaModifierState.ExpandCutout:
                    {
                        cutoutExpandAnimationComponent.Update(p_delta);
                        cutoutEntity.ImageValue = cutoutExpandAnimationComponent.GetActiveSprite();

                        if (cutoutExpandAnimationComponent.IsAtEndOfAnimation())
                        {
                            cutoutShrinkAnimationComponent.ResetAnimation();
                            cutoutExpandAnimationComponent.ResetAnimation();
                            cutoutEntity?.Dispose();
                            cutoutEntity = null;

                            lavaModifierState = RisingLavaModifierState.Rising;
                            logger.Information($"Setting Modifier State to {lavaModifierState.ToString()}");
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// Called when the player is teleported
        /// </summary>
        /// <param name="e"></param>
        private void OnPlayerTeleported(OnTeleportedEventArgs e)
        {
            if (lavaModifierState == RisingLavaModifierState.Rising && lavaEntity != null)
            {
                logger.Information($"Player Teleporting from {e.PreviousScreenIndex} to {e.NewScreenIndex}");
                float prevScreenBottom = GetScreenBottomY(e.PreviousScreenIndex);
                float newScreenBottom = GetScreenBottomY(e.NewScreenIndex);
                lavaEntity.WorldSpacePosition += new Vector2(0, (newScreenBottom - prevScreenBottom));
            }
        }

        /// <summary>
        /// Called when the player's position is restarted
        /// </summary>
        /// <param name="newPosition"></param>
        private void OnPlayerPositionRestarted(Vector2 newPosition)
        {
            if (lavaEntity != null)
            {
                lavaEntity.WorldSpacePosition = GetLavaSpawnPos();
            }
        }

        /// <summary>
        /// Get the bottom Y position of the current screen
        /// </summary>
        private float GetCurrentScreenBottomY()
        {
            return GetScreenBottomY(Camera.CurrentScreen);
        }

        /// <summary>
        /// Get the bottom Y position of the provided screen
        /// </summary>
        private float GetScreenBottomY(int screenIndex)
        {
            return (-Camera.Offset.Y) - ((screenIndex - 1) * ScreenHeight);
        }

        /// <summary>
        /// Gets the spawn position for the Lava
        /// </summary>
        private Vector2 GetLavaSpawnPos()
        {
            float curScreenBottom = GetCurrentScreenBottomY();
            Vector2 spawnPos = new Vector2(ModifiersModContentManager.LavaTexture.Width/ 2, curScreenBottom + (ModifiersModContentManager.LavaTexture.Height / 2) + SpawnYBuffer);
            return spawnPos;
        }
    }
}
