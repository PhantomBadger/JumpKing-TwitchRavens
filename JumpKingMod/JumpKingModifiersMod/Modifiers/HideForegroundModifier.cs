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
    /// An implementation of <see cref="IModifier"/> which hides the foreground platforms
    /// </summary>
    public class HideForegroundModifier : IModifier
    {
        public string DisplayName => "Hide Platforms";

        private readonly IDrawForegroundObserver drawForegroundObserver;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="HideForegroundModifier"/>
        /// </summary>
        /// <param name="drawForegroundObserver">An implementation of <see cref="IDrawForegroundObserver"/> for overriding the foreground drawing logic</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public HideForegroundModifier(IDrawForegroundObserver drawForegroundObserver, ILogger logger)
        {
            this.drawForegroundObserver = drawForegroundObserver ?? throw new ArgumentNullException(nameof(drawForegroundObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!drawForegroundObserver.GetDrawForegroundOverride())
            {
                logger.Error($"Failed to disable '{DisplayName}' Modifier as it is already disabled!");
                return false;
            }

            drawForegroundObserver.SetDrawForegroundOverride(drawForegroundOverride: false);
            logger.Information($"Disabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (drawForegroundObserver.GetDrawForegroundOverride())
            {
                logger.Error($"Failed to enable '{DisplayName}' Modifier as it is already enabled!");
                return false;
            }

            drawForegroundObserver.SetDrawForegroundOverride(drawForegroundOverride: true);
            logger.Information($"Enabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return drawForegroundObserver.GetDrawForegroundOverride();
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
