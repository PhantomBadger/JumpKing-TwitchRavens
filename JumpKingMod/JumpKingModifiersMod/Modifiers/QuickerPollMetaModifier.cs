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
    /// An implementation of <see cref="IMetaModifier"/> which speeds up the poll time for the <see cref="TwitchPollTrigger"/>
    /// </summary>
    [ConfigurableModifier("(Meta) Quicker Poll Time", "The Polls created by the Twitch Poll trigger end quicker")]
    public class QuickerPollMetaModifier : IMetaModifier
    {
        public string DisplayName => "Quicker Poll Time";

        public TwitchPollTrigger TwitchPollTrigger { get; set; }

        private readonly ILogger logger;

        private const float PollTimeModifier = 0.5f;

        /// <summary>
        /// Ctor for creating a <see cref="QuickerPollMetaModifier"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public QuickerPollMetaModifier(ILogger logger)
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

            if (TwitchPollTrigger == null)
            {
                logger.Information($"Can't disable '{this.DisplayName}' as the Twitch Poll Trigger reference is invalid!");
                return false;
            }

            TwitchPollTrigger.PollTimeModifier = TwitchPollTrigger.DefaultPollTimeModifier;
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

            if (TwitchPollTrigger == null)
            {
                logger.Information($"Can't enable '{this.DisplayName}' as the Twitch Poll Trigger reference is invalid!");
                return false;
            }

            TwitchPollTrigger.PollTimeModifier = PollTimeModifier;
            logger.Information($"Enabled '{this.DisplayName}' successfully!");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return TwitchPollTrigger != null && Math.Abs(TwitchPollTrigger.PollTimeModifier - TwitchPollTrigger.DefaultPollTimeModifier) > float.Epsilon;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
