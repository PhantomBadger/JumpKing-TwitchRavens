using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Triggers;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IMetaModifier"/> which increases the duration of activated modifiers by the <see cref="PollTrigger"/>
    /// </summary>
    [ConfigurableModifier("(Meta) Longer Modifier Duration", "Modifiers triggered by the Twitch Poll last longer")]
    public class LongerDurationMetaModifier : IMetaModifier
    {
        public string DisplayName => "Longer Modifier Duration";

        public PollTrigger PollTrigger { get; set; }

        private readonly ILogger logger;

        private const float DurationModifier = 2.0f;

        /// <summary>
        /// Ctor for creating a <see cref="LongerDurationMetaModifier"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public LongerDurationMetaModifier(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!IsModifierEnabled())
            {
                logger.Information($"Can't disable '{this.DisplayName}' as it is already disabled!");
                return false;
            }

            if (PollTrigger == null)
            {
                logger.Information($"Can't disable '{this.DisplayName}' as the Twitch Poll Trigger reference is invalid!");
                return false;
            }

            PollTrigger.ActiveModifierDurationModifier = PollTrigger.DefaultActiveModifierDurationModifier;
            logger.Information($"Disabled '{this.DisplayName}' successfully!");
            return true;
        }

        /// <inheritdoc/>
        public bool EnableModifier()
        {
            if (IsModifierEnabled())
            {
                logger.Information($"Can't enable '{this.DisplayName}' as it is already enabled!");
                return false;
            }

            if (PollTrigger == null)
            {
                logger.Information($"Can't enable '{this.DisplayName}' as the Twitch Poll Trigger reference is invalid!");
                return false;
            }

            PollTrigger.ActiveModifierDurationModifier = DurationModifier;
            logger.Information($"Enabled '{this.DisplayName}' successfully!");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return PollTrigger != null && Math.Abs(PollTrigger.ActiveModifierDurationModifier - PollTrigger.DefaultActiveModifierDurationModifier) > float.Epsilon;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
