using JumpKingMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.YouTube
{
    /// <summary>
    /// An implementation of <see cref="IYouTubeClientConnector"/> which will wait for
    /// manual user input before attempting to connect the youtube chat client
    /// </summary>
    public class ManualYouTubeClientConnector : IYouTubeClientConnector
    {
        public YouTubeChatClient YoutubeClient
        { 
            get
            {
                return youtubeClient;
            } 
        }

        private readonly YouTubeChatClient youtubeClient;

        /// <summary>
        /// Ctor for creating a <see cref="ManualYouTubeClientConnector"/>
        /// </summary>
        public ManualYouTubeClientConnector(YouTubeChatClient youtubeClient)
        {
            this.youtubeClient = youtubeClient ?? throw new ArgumentNullException(nameof(youtubeClient));
        }

        /// <summary>
        /// Waits for user input before attempting the connection
        /// </summary>
        public Task<bool> StartAttemptingConnection()
        {
            //throw new NotImplementedException();
            return Task.FromResult(false);
        }
    }
}
