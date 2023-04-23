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
    /// An implementation of <see cref="IModifier"/> and <see cref="IDisposable"/> which flips
    /// the screen vertically and horizontally
    /// </summary>
    public class FlipScreenModifier : IModifier, IDisposable
    {
        public string DisplayName { get; } = "Flip Screen";

        private readonly IGameRectFlipper gameRectFlipper;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="FlipScreenModifier"/>
        /// </summary>
        /// <param name="gameRectFlipper">An implementation of <see cref="IGameRectFlipper"/> to flip the game rect</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        /// <exception cref="ArgumentNullException"></exception>
        public FlipScreenModifier(IGameRectFlipper gameRectFlipper, ILogger logger)
        {
            this.gameRectFlipper = gameRectFlipper ?? throw new ArgumentNullException(nameof(gameRectFlipper));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            DisableModifier();
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return gameRectFlipper.IsGameRectFlipped();
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!gameRectFlipper.IsGameRectFlipped()) 
            {
                logger.Error($"Failed to disable 'Flip Screen' Modifier as it is already disabled!");
            }

            gameRectFlipper.SetShouldFlipGameRect(shouldFlip: false);
            logger.Information($"Disabled 'Flip Screen' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (gameRectFlipper.IsGameRectFlipped())
            {
                logger.Error($"Failed to enable 'Flip Screen' Modifier as it is already enabled!");
            }

            gameRectFlipper.SetShouldFlipGameRect(shouldFlip: true);
            logger.Information($"Enabled 'Flip Screen' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // do nothing
        }
    }
}