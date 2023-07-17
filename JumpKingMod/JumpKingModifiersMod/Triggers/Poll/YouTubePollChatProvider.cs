using JumpKingModifiersMod.API;
using Logging.API;
using PBJKModBase.YouTube;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers.Poll
{
    public class YouTubePollChatProvider : IPollChatProvider
    {
        public event ChatVoteDelegate OnChatVote;

        private readonly ILogger logger;
        private readonly YouTubeChatClient youtubeClient;
        private readonly ConcurrentDictionary<string, byte> alreadyVotedChatters;

        private bool isEnabled;

        public YouTubePollChatProvider(YouTubeChatClient youtubeClient, ILogger logger)
        {
            this.youtubeClient = youtubeClient ?? throw new ArgumentNullException(nameof(youtubeClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            alreadyVotedChatters = new ConcurrentDictionary<string, byte>();
            isEnabled = false;

            youtubeClient.OnMessageBatchReceived += OnMessageReceived;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public bool DisableChatProvider()
        {
            throw new NotImplementedException();
        }

        public bool EnableChatProvider()
        {
            throw new NotImplementedException();
        }

        public void ClearPerPollData()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, YouTubeChatMessageBatchArgs chatMessages)
        {
            try
            {
                if (!isEnabled || chatMessages == null || chatMessages.LiveChatMessages == null)
                {
                    return;
                }

                // TODO
                //// If they have already voted, then dip (unless it's me because im special hehe)
                //if (alreadyVotedChatters.ContainsKey(e.ChatMessage.UserId) &&
                //    !e.ChatMessage.DisplayName.Equals("PhantomBadger", StringComparison.OrdinalIgnoreCase))
                //{
                //    return;
                //}

                //// Get the choice number from the message
                //if (!TryGetPollChoiceNumberFromMessage(e.ChatMessage.Message, out int choiceNumber))
                //{
                //    return;
                //}

                //alreadyVotedChatters.TryAdd(e.ChatMessage.UserId, 0);
                //OnChatVote?.Invoke(e.ChatMessage.DisplayName, choiceNumber);
            }
            catch (Exception ex)
            {
                logger.Error($"Encountered Exception during OnMessageReceived: {ex.ToString()}");
            }
        }
    }
}
