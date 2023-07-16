using JumpKingModifiersMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers.Poll
{
    public class YouTubePollChatProvider : IPollChatProvider
    {
        public event ChatVoteDelegate OnChatVote;

        public void ClearPerPollData()
        {
            throw new NotImplementedException();
        }

        public bool DisableChatProvider()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool EnableChatProvider()
        {
            throw new NotImplementedException();
        }
    }
}
