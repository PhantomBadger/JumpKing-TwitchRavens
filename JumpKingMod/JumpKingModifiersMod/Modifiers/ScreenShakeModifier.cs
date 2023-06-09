using JumpKing;
using JumpKing.MiscSystems;
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
    /// An implementation of <see cref="IModifier"/> which causes screen shake
    /// </summary>
    [ConfigurableModifier("Screen Shake", "The screen shakes left to right")]
    public class ScreenShakeModifier : IModifier
    {
        public string DisplayName => "Screen Shake";

        private readonly ILogger logger;
        
        private ScreenShakeController screenShakeController;

        private const float ScreenShakeIntensity = 0.9f;
        private const int ScreenShakeX = 2;

        /// <summary>
        /// Ctor for creating a <see cref="ScreenShakeModifier"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/> used for logging</param
        public ScreenShakeModifier(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (screenShakeController == null)
            {
                logger.Error($"Failed to disable '{DisplayName}' Modifier as it is already disabled!");
                return false;
            }

            screenShakeController.SetIntensity(0);
            screenShakeController.SetScreenshake(0, 0);
            screenShakeController.Dispose();
            screenShakeController = null;
            logger.Information($"Disabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (screenShakeController != null)
            {
                logger.Error($"Failed to enable '{DisplayName}' Modifier as it is already enabled!");
                return false;
            }

            screenShakeController = JumpGame.screenShakeManager.CreateShakeController();
            screenShakeController.SetIntensity(ScreenShakeIntensity);
            screenShakeController.SetScreenshake(ScreenShakeX, 0);
            logger.Information($"Enabled '{DisplayName}' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return screenShakeController != null;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
