﻿using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> to modify the walk speed
    /// </summary>
    [ConfigurableModifier("Increased Walk Speed", "The player walks at a faster rate than normal")]
    public class WalkSpeedModifier : IModifier
    {
        public string DisplayName { get; } = "Increase Walk Speed";

        private readonly float speedModifier;
        private readonly ILogger logger;
        private readonly IWalkSpeedModifier walkSpeedModifierAccessor;

        private const float OriginalValue = 1.0f;
        public const float DefaultModifier = 2.0f;

        /// <summary>
        /// Ctor for creating a <see cref="WalkSpeedModifier"/>
        /// </summary>
        /// <param name="speedModifier">The modifier to apply to the walk speed</param>
        /// <param name="walkSpeedModifierAccessor">The <see cref="IWalkSpeedModifier"/> to use for applying the modifier to the game</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public WalkSpeedModifier(float speedModifier, IWalkSpeedModifier walkSpeedModifierAccessor, ILogger logger)
        {
            this.speedModifier = speedModifier;
            this.walkSpeedModifierAccessor = walkSpeedModifierAccessor ?? throw new ArgumentNullException(nameof(walkSpeedModifierAccessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns whether the <see cref="WalkSpeedModifier"/> is active or not
        /// </summary>
        public bool IsModifierEnabled()
        {
            return Math.Abs(walkSpeedModifierAccessor.GetWalkSpeedModifier() - OriginalValue) > float.Epsilon;
        }

        /// <summary>
        /// Enables the modifier, changing the player's walk speed
        /// </summary>
        /// <returns><c>true</c> if successfully applied, <c>false</c> if not</returns>
        public bool EnableModifier()
        {
            logger.Information($"Enable Walk Speed Modifier");
            walkSpeedModifierAccessor.SetWalkSpeedModifer(speedModifier);
            return true;
        }

        /// <summary>
        /// Disables the modifier, resetting the player's walk speed
        /// </summary>
        /// <returns><c>true</c> if successfully disabled, <c>false</c> if not</returns>
        public bool DisableModifier()
        {
            logger.Information($"Disable Walk Speed Modifier");
            walkSpeedModifierAccessor.SetWalkSpeedModifer(OriginalValue);
            return true;
        }

        /// <summary>
        /// Called each frame by the <see cref="ModifierUpdatingEntity"/>
        /// </summary>
        public void Update(float p_delta)
        {
            // do nothing
        }
    }
}
