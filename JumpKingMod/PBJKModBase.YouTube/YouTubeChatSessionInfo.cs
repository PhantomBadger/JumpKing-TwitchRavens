using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PBJKModBase.YouTube
{
    /// <summary>
    /// An aggregate class representing info about a YouTube LiveStream Chat connection
    /// </summary>
    public class YouTubeChatSessionInfo
    {
        public string LiveChatId { get; set; }
        public CancellationToken Token { get; set; }
    }
}
