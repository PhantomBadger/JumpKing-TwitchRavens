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
    /// An implementation of <see cref="IModifier"/> and <see cref="IDisposable"/> which flips the player's controls
    /// </summary>
    public class InvertControlsModifier : IModifier, IDisposable
    {
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating an <see cref="InvertControlsModifier"/>
        /// </summary>
        /// <param name="playerStateObserver">An implementation of <see cref="IPlayerStateObserver"/> for interfacing with player input</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public InvertControlsModifier(IPlayerStateObserver playerStateObserver, ILogger logger)
        {
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            DisableModifier();
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return playerStateObserver.GetInvertPlayerInputs();
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!playerStateObserver.GetInvertPlayerInputs())
            {
                logger.Error($"Failed to disable 'Invert Controls' Modifier as it's already disabled!");
                return false;
            }

            playerStateObserver.SetInvertPlayerInputs(false);
            logger.Information($"Disabled 'Invert Controls' Modifier!");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (playerStateObserver.GetInvertPlayerInputs())
            {
                logger.Error($"Failed to enable 'Invert Controls' Modifier as it's already enabled!");
                return false;
            }

            playerStateObserver.SetInvertPlayerInputs(true);
            logger.Information($"Enabled 'Invert Controls' Modifier!");
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }

    }
}
