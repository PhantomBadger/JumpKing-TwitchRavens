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
    /// An implementation of <see cref="IModifier"/> to modify the jump time
    /// </summary>
    [ConfigurableModifier("Decrease Jump Charge Time", "It takes less time to charge a full jump")]
    public class JumpTimeModifier : IModifier
    {
        public string DisplayName => "Decrease Jump Charge Time";

        private readonly float jumpTimeModifier;
        private readonly ILogger logger;
        private readonly IJumpTimeModifier jumpTimeModifierAccessor;

        private const float OriginalValue = 0.6f;
        public const float DefaultModifier = 0.25f;

        /// <summary>
        /// Ctor for creating a <see cref="JumpTimeModifier"/>
        /// </summary>
        public JumpTimeModifier(float jumpTimeModifier, IJumpTimeModifier jumpTimeModifierAccessor, ILogger logger)
        {
            this.jumpTimeModifier = jumpTimeModifier;
            this.jumpTimeModifierAccessor = jumpTimeModifierAccessor ?? throw new ArgumentNullException(nameof(jumpTimeModifierAccessor));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            logger.Information($"Disable Jump Time Modifier");
            jumpTimeModifierAccessor.SetJumpTimeModifer(OriginalValue);
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            logger.Information($"Enable Jump Time Modifier");
            jumpTimeModifierAccessor.SetJumpTimeModifer(jumpTimeModifier);
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return Math.Abs(jumpTimeModifierAccessor.GetJumpTimeModifier() - OriginalValue) > float.Epsilon;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // do nothing
        }
    }
}
