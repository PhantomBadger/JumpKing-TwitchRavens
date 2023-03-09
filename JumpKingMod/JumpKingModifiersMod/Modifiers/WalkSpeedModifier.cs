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

        public WalkSpeedModifier(float speedModifier, PlayerValuesManualPatch playerValuesManualPatch, ILogger logger)
        {
            this.speedModifier = speedModifier;
            this.playerValuesManualPatch = playerValuesManualPatch ?? throw new ArgumentNullException(nameof(playerValuesManualPatch));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsModifierEnabled()
        {
            return Math.Abs(playerValuesManualPatch.GetWalkSpeedModifier() - 1f) > float.Epsilon;
        }

        public bool EnableModifier()
        {
            logger.Information($"Enable Walk Speed Modifier");
            playerValuesManualPatch.SetWalkSpeedModifer(speedModifier);
            return true;
        }

        public bool DisableModifier()
        {
            logger.Information($"Disable Walk Speed Modifier");
            playerValuesManualPatch.SetWalkSpeedModifer(1f);
            return true;
        }
    }
}
