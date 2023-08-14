using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers.Poll
{
    public enum PollTriggerState
    {
        CreatingPoll,
        CollectingVotes,
        ClosingPoll,
        ExecutingWinningOption,
        DownTimeBetweenPolls,
    }
}
