using JumpKingMod.YouTube;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.API
{
    /// <summary>
    /// Interface representing an object which knows how to connect a YouTube Client
    /// </summary>
    public interface IYouTubeClientConnector
    {
        YouTubeChatClient YoutubeClient { get; }

        Task<bool> StartAttemptingConnection();
    }
}
