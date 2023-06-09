using JumpKingModifiersMod.API;
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
    /// An implementation of <see cref="IModifier"/> which turns on the wind when enabled
    /// </summary>
    [ConfigurableModifier("Enable Wind", "Wind is enabled which will push the player left and right")]
    public class WindToggleModifier : IModifier
    {
        public string DisplayName { get; } = "Wind";

        private readonly IWindObserver windObserver;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="WindToggleModifier"/>
        /// </summary>
        /// <param name="windObserver">An implementation of <see cref="IWindObserver"/> to interface with the game's wind state</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public WindToggleModifier(IWindObserver windObserver, ILogger logger)
        {
            this.windObserver = windObserver ?? throw new ArgumentNullException(nameof(windObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!windObserver.GetWindOverrideState())
            {
                logger.Error("Failed to disable 'Wind Toggle' Modifier as it is already disabled!");
                return false;
            }

            windObserver.SetWindOverrideState(shouldOverrideWind: false);
            logger.Information("Disabled 'Wind Toggle' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (windObserver.GetWindOverrideState())
            {
                logger.Error("Failed to enable 'Wind Toggle' Modifier as it is already enabled!");
                return false;
            }

            windObserver.SetWindOverrideState(shouldOverrideWind: true);
            logger.Information("Enabled 'Wind Toggle' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return windObserver.GetWindOverrideState();
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
