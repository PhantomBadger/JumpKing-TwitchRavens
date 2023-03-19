using JumpKingModifiersMod.API;
using Logging.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> which maintains a countdown on the player
    /// once it hits zero a punishment is incurred, the countdown is staved by jumping
    /// </summary>
    public class BombCountdownModifier : IModifier
    {
        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly ModEntityManager modEntityManager;
        private readonly ILogger logger;
        private UITextEntity uiTextEntity;

        /// <summary>
        /// Ctor for creating a <see cref="BombCountdownModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">The <see cref="ModifierUpdatingEntity"/> to use to call update</param>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to add other entities to</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation used to log to</param>
        public BombCountdownModifier(ModifierUpdatingEntity modifierUpdatingEntity, ModEntityManager modEntityManager, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            uiTextEntity = null;
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
                return false;
            }
            uiTextEntity.Dispose();
            uiTextEntity = null;
            logger.Information("Disabling the Bomb Countdown Modifier!");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (uiTextEntity != null)
            {
                return false;
            }

            //uiTextEntity = new UITextEntity(modEntityManager, )
            logger.Information("Enabling the Bomb Countdown Modifier!");
            return true;
        }

        /// <inheritdoc/>
        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
