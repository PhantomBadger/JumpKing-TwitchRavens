using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.YouTube
{
    /// <summary>
    /// An aggregate class of arguments related to a batch of YouTube Chat Messages
    /// </summary>
    public class YouTubeChatMessageBatchArgs
    {
        public IList<LiveChatMessage> LiveChatMessages { get; set; }
        public float MinDelayBeforeNextBatch { get; set; }
    }
}
