using JumpKingModifiersMod.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    public delegate void PollStartedDelegate(ModifierPoll poll);
    public delegate void PollClosedDelegate(ModifierPoll poll);
    public delegate void PollEndedDelegate(ModifierPoll poll);

    /// <summary>
    /// An interface extending <see cref="IModifierTrigger"/> to have additional poll-specific information
    /// </summary>
    public interface IModifierPollTrigger : IModifierTrigger
    {
        event PollStartedDelegate OnPollStarted;
        event PollClosedDelegate OnPollClosed;
        event PollEndedDelegate OnPollEnded;

        /// <summary>
        /// Returns a list of the active modifiers and their countdowns
        /// </summary>
        IReadOnlyList<ActiveModifierCountdown> GetActiveModifierCountdowns();
    }
}
