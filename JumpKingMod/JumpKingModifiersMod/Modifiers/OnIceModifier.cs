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
    /// An implementation of <see cref="IModifier"/> for overriding the Ice state of the player
    /// </summary>
    public class OnIceModifier : IModifier
    {
        public string DisplayName => "On Ice";

        private readonly IOnIceObserver iceObserver;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="OnIceModifier"/>
        /// </summary>
        /// <param name="iceObserver">An implementation of <see cref="IOnIceObserver"/></param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public OnIceModifier(IOnIceObserver iceObserver, ILogger logger)
        {
            this.iceObserver = iceObserver ?? throw new ArgumentNullException(nameof(iceObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!iceObserver.GetIsOnIceOverrideState())
            {
                logger.Error($"Failed to disable '{DisplayName}' Modifier as it is already disabled!");
                return false;
            }

            iceObserver.SetIsOnIceOverrideState(shouldOverrideIsOnIce: false);
            logger.Information($"Disabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (iceObserver.GetIsOnIceOverrideState())
            {
                logger.Error($"Failed to enable '{DisplayName}' Modifier as it is already enabled!");
                return false;
            }

            iceObserver.SetIsOnIceOverrideState(shouldOverrideIsOnIce: true);
            logger.Information($"Enabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return iceObserver.GetIsOnIceOverrideState();
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            throw new NotImplementedException();
        }
    }
}
