using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching;
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
    public class WalkSpeedModifier : IModifier
    {
        private readonly float speedModifier;
        private readonly ILogger logger;
        private readonly PlayerValuesManualPatch playerValuesManualPatch;

        /// <summary>
        /// Ctor for creating a <see cref="WalkSpeedModifier"/>
        /// </summary>
        /// <param name="speedModifier">The modifier to apply to the walk speed</param>
        /// <param name="playerValuesManualPatch">The <see cref="PlayerValuesManualPatch"/> to use for applying the modifier to the game</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public WalkSpeedModifier(float speedModifier, PlayerValuesManualPatch playerValuesManualPatch, ILogger logger)
        {
            this.speedModifier = speedModifier;
            this.playerValuesManualPatch = playerValuesManualPatch ?? throw new ArgumentNullException(nameof(playerValuesManualPatch));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Returns whether the <see cref="WalkSpeedModifier"/> is active or not
        /// </summary>
        public bool IsModifierEnabled()
        {
            return Math.Abs(playerValuesManualPatch.GetWalkSpeedModifier() - 1f) > float.Epsilon;
        }

        /// <summary>
        /// Enables the modifier, changing the player's walk speed
        /// </summary>
        /// <returns><c>true</c> if successfully applied, <c>false</c> if not</returns>
        public bool EnableModifier()
        {
            logger.Information($"Enable Walk Speed Modifier");
            playerValuesManualPatch.SetWalkSpeedModifer(speedModifier);
            return true;
        }

        /// <summary>
        /// Disables the modifier, resetting the player's walk speed
        /// </summary>
        /// <returns><c>true</c> if successfully disabled, <c>false</c> if not</returns>
        public bool DisableModifier()
        {
            logger.Information($"Disable Walk Speed Modifier");
            playerValuesManualPatch.SetWalkSpeedModifer(1f);
            return true;
        }
    }
}
