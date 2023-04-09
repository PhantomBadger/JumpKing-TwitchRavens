using JumpKing;
using JumpKing.SaveThread;
using JumpKingModifiersMod.API;
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
        private RisingLavaModifierState lavaModifierState;

        private const float LavaRisingSpeed = 1f;

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

            // TODO: Set to be start of current screen?
            SaveState defaultSaveState = SaveState.GetDefault();
            Vector2 spawnPos = new Vector2(240, defaultSaveState.position.Y + (ModifiersModContentManager.LavaTexture.Height / 2) + 50);
            lavaEntity = new WorldspaceImageEntity(modEntityManager,
                spawnPos,
                Sprite.CreateCenteredSprite(ModifiersModContentManager.LavaTexture, ModifiersModContentManager.LavaTexture.Bounds),
                zOrder: 2);
            logger.Information($"Enabled 'Rising Lava' Modifier successfully!");
            return true;
        }

        public void Update(float p_delta)
        {
            switch (lavaModifierState)
            {
                case RisingLavaModifierState.Rising:

                    // Make the Lava move up
                    lavaEntity.WorldSpacePosition -= new Vector2(0, p_delta * LavaRisingSpeed);

                    // Check to see if the player is intersecting with it
                    // TODO

                    break;
            }
        }
    }
}
