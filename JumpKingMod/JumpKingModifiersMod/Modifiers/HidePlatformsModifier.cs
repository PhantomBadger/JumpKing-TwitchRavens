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
    /// An implementation of <see cref="IModifier"/> which hides the foreground platforms
    /// </summary>
    [ConfigurableModifier("Hide Platforms")]
    public class HidePlatformsModifier : IModifier
    {
        public string DisplayName => "Hide Platforms";

        private readonly IDrawPlatformObserver drawPlatformObserver;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="HidePlatformsModifier"/>
        /// </summary>
        /// <param name="drawForegroundObserver">An implementation of <see cref="IDrawPlatformObserver"/> for overriding the platform drawing logic</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public HidePlatformsModifier(IDrawPlatformObserver drawForegroundObserver, ILogger logger)
        {
            this.drawPlatformObserver = drawForegroundObserver ?? throw new ArgumentNullException(nameof(drawForegroundObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!drawPlatformObserver.GetDrawPlatformOverride())
            {
                logger.Error($"Failed to disable '{DisplayName}' Modifier as it is already disabled!");
                return false;
            }

            drawPlatformObserver.SetDrawPlatformOverride(drawPlatformOverride: false);
            logger.Information($"Disabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (drawPlatformObserver.GetDrawPlatformOverride())
            {
                logger.Error($"Failed to enable '{DisplayName}' Modifier as it is already enabled!");
                return false;
            }

            drawPlatformObserver.SetDrawPlatformOverride(drawPlatformOverride: true);
            logger.Information($"Enabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return drawPlatformObserver.GetDrawPlatformOverride();
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
