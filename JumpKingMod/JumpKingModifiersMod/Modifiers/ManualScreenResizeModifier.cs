using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> and <see cref="IDisposable"/> which lets the user manually
    /// scale up-and-down the screen using predefined keys
    /// </summary>
    public class ManualScreenResizeModifier : IModifier, IDisposable
    {
        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly UserSettings userSettings;
        private readonly ILogger logger;

        private readonly Keys sizeUpKey;
        private readonly Keys sizeDownKey;

        private int baselineWidth;
        private int baselineHeight;
        private bool sizeUpKeyReset;
        private bool sizeDownKeyReset;
        private bool isActive;

        private const float ScreenSizeChangeAmount = 0.9f;
        private const int MinimumXSize = 57;
        private const int MinimumYSize = 42;

        /// <summary>
        /// Ctor for creating a <see cref="ManualScreenResizeModifier"/>
        /// </summary>
        /// <param name="modifierUpdatingEntity">A <see cref="ModifierUpdatingEntity"/> to register against to call our Update method</param>
        /// <param name="userSettings">A <see cref="UserSettings"/> instance to query for settings</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        public ManualScreenResizeModifier(ModifierUpdatingEntity modifierUpdatingEntity, UserSettings userSettings, 
            ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            sizeUpKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeGrowKeyKey, Keys.Up);
            sizeDownKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeShrinkKeyKey, Keys.Down);

            isActive = false;
            sizeUpKeyReset = true;
            sizeDownKeyReset = true;

            modifierUpdatingEntity.RegisterModifier(this);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            DisableModifier();
            modifierUpdatingEntity?.UnregisterModifier(this);
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return isActive;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (isActive)
            {
                logger.Information($"Failed to Enable 'Manual Screen Resize' Modifier - Effect already active");
                return false;
            }

            baselineWidth = JumpKing.Game1.graphics.PreferredBackBufferWidth;
            baselineHeight = JumpKing.Game1.graphics.PreferredBackBufferHeight;
            JumpKing.Game1.graphics.ApplyChanges();
            isActive = true;
            logger.Information($"Enabled 'Manual Screen Resize' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!isActive)
            {
                logger.Information($"Failed to Disable 'Manual Screen Resize' Modifier - Effect already disabled");
                return false;
            }

            JumpKing.Game1.graphics.PreferredBackBufferWidth = baselineWidth;
            JumpKing.Game1.graphics.PreferredBackBufferHeight = baselineHeight;
            JumpKing.Game1.graphics.ApplyChanges();
            isActive = false;
            logger.Information($"Disabled 'Manual Screen Resize' Modifier");
            return true;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            KeyboardState kbState = Keyboard.GetState();

            bool stateChanged = false;
            
            // Check to size down the screen
            if (kbState.IsKeyDown(sizeDownKey))
            {
                if (sizeDownKeyReset)
                {
                    sizeDownKeyReset = false;
                    JumpKing.Game1.graphics.PreferredBackBufferWidth =
                        Math.Max(MinimumXSize, (int)(JumpKing.Game1.graphics.PreferredBackBufferWidth * ScreenSizeChangeAmount));
                    JumpKing.Game1.graphics.PreferredBackBufferHeight =
                        Math.Max(MinimumYSize, (int)(JumpKing.Game1.graphics.PreferredBackBufferHeight * ScreenSizeChangeAmount));

                    stateChanged = true;
                }
            }
            else
            {
                sizeDownKeyReset = true;
            }

            // Check to size up the screen
            if (kbState.IsKeyDown(sizeUpKey))
            {
                if (sizeUpKeyReset)
                {
                    sizeUpKeyReset = false;
                    JumpKing.Game1.graphics.PreferredBackBufferWidth =
                        Math.Min(baselineWidth, (int)(JumpKing.Game1.graphics.PreferredBackBufferWidth / ScreenSizeChangeAmount));
                    JumpKing.Game1.graphics.PreferredBackBufferHeight =
                        Math.Min(baselineHeight, (int)(JumpKing.Game1.graphics.PreferredBackBufferHeight / ScreenSizeChangeAmount));

                    stateChanged = true;
                }
            }
            else
            {
                sizeUpKeyReset = true;
            }

            // Apply any changes
            if (stateChanged)
            {
                JumpKing.Game1.graphics.ApplyChanges();
            }
        }
    }
}
