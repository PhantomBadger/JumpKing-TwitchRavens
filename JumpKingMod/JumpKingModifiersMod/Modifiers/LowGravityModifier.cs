using JumpKingModifiersMod.API;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> which turns on low gravity when enabled
    /// </summary>
    public class LowGravityModifier : IModifier
    {
        public string DisplayName => "Low Gravity";

        private readonly ILogger logger;
        private readonly ILowGravityObserver lowGravityObserver;

        /// <summary>
        /// Ctor for creating a <see cref="LowGravityModifier"/>
        /// </summary>
        /// <param name="lowGravityObserver">An implementation of <see cref="ILowGravityObserver"/></param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public LowGravityModifier(ILowGravityObserver lowGravityObserver, ILogger logger)
        {
            this.lowGravityObserver = lowGravityObserver ?? throw new ArgumentNullException(nameof(lowGravityObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!lowGravityObserver.GetLowGravityOverrideState())
            {
                logger.Error($"Failed to disable '{DisplayName}' Modifier as it is already disabled!");
                return false;
            }

            lowGravityObserver.SetLowGravityOverrideState(shouldOverrideLowGravity: false);
            logger.Information($"Disabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (lowGravityObserver.GetLowGravityOverrideState())
            {
                logger.Error($"Failed to enable '{DisplayName}' Modifier as it is already enabled!");
                return false;
            }

            lowGravityObserver.SetLowGravityOverrideState(shouldOverrideLowGravity: true);
            logger.Information($"Enabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return lowGravityObserver.GetLowGravityOverrideState();
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
