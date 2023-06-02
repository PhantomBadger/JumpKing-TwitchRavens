using JumpKingModifiersMod.API;
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
    /// An implementation of <see cref="IModifier"/> which speeds up the poll time for the <see cref="TwitchPollTrigger"/>
    /// </summary>
    public class QuickerPollMetaModifier : IModifier
    {
        public string DisplayName => "Quicker Poll Time";

        private readonly TwitchPollTrigger twitchPollTrigger;
        private readonly ILogger logger;

        private const float PollTimeModifier = 0.5f;

        /// <summary>
        /// Ctor for creating a <see cref="QuickerPollMetaModifier"/>
        /// </summary>
        /// <param name="twitchPollTrigger">The <see cref="TwitchPollTrigger"/> to set the modifier on</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to log to</param>
        public QuickerPollMetaModifier(TwitchPollTrigger twitchPollTrigger, ILogger logger)
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

            twitchPollTrigger.PollTimeModifier = TwitchPollTrigger.DefaultPollTimeModifier;
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

            twitchPollTrigger.PollTimeModifier = PollTimeModifier;
            logger.Information($"Enabled '{this.DisplayName}' successfully!");
            return true;
        }

        /// <inheritdoc/>
        public bool IsModifierEnabled()
        {
            return Math.Abs(twitchPollTrigger.PollTimeModifier - TwitchPollTrigger.DefaultPollTimeModifier) > float.Epsilon;
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do nothing
        }
    }
}
