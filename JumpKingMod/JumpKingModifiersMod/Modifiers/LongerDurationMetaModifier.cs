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
    /// An implementation of <see cref="IModifier"/> which increases the duration of activated modifiers by the <see cref="TwitchPollTrigger"/>
    /// </summary>
    [ConfigurableModifier("(Meta) Longer Modifier Duration")]
    public class LongerDurationMetaModifier : IModifier
    {
        public string DisplayName => "Longer Modifier Duration";

        private readonly TwitchPollTrigger twitchPollTrigger;
        private readonly ILogger logger;

        private const float DurationModifier = 2.0f;

        /// <summary>
        /// Ctor for creating a <see cref="LongerDurationMetaModifier"/>
        /// </summary>
        /// <param name="twitchPollTrigger">The <see cref="TwitchPollTrigger"/> to set the modifier on</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public LongerDurationMetaModifier(TwitchPollTrigger twitchPollTrigger, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.twitchPollTrigger = twitchPollTrigger ?? throw new ArgumentNullException(nameof(twitchPollTrigger));
        }

        /// <inheritdoc/>
        public bool DisableModifier()
        {
            if (!IsModifierEnabled())
            {
                logger.Information($"Can't disable '{this.DisplayName}' as it is already disabled!");
                return false;
            }

            twitchPollTrigger.ActiveModifierDurationModifier = TwitchPollTrigger.DefaultActiveModifierDurationModifier;
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

            twitchPollTrigger.ActiveModifierDurationModifier = DurationModifier;
            logger.Information($"Enabled '{this.DisplayName}' successfully!");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return Math.Abs(twitchPollTrigger.ActiveModifierDurationModifier - TwitchPollTrigger.DefaultActiveModifierDurationModifier) > float.Epsilon;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
