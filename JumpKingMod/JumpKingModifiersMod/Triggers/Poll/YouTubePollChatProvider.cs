using Google.Apis.YouTube.v3.Data;
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
    /// <summary>
    /// An implementation of <see cref="IPollChatProvider"/> for YouTube integration
    /// </summary>
    public class YouTubePollChatProvider : IPollChatProvider
    {
        public event ChatVoteDelegate OnChatVote;

        private readonly ILogger logger;
        private readonly YouTubeChatClient youtubeClient;
        private readonly ConcurrentDictionary<string, byte> alreadyVotedChatters;

        private bool isEnabled;

        /// <summary>
        /// Ctor for creating a <see cref="YouTubePollChatProvider"/>
        /// </summary>
        /// <param name="youtubeClient">The YouTube Client to use</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public YouTubePollChatProvider(YouTubeChatClient youtubeClient, ILogger logger)
        {
            this.youtubeClient = youtubeClient ?? throw new ArgumentNullException(nameof(youtubeClient));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            alreadyVotedChatters = new ConcurrentDictionary<string, byte>(StringComparer.OrdinalIgnoreCase);
            isEnabled = false;

            youtubeClient.OnMessageBatchReceived += OnMessageReceived;
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> for cleaning up
        /// </summary>
        public void Dispose()
        {
            if (youtubeClient != null)
            {
                youtubeClient.OnMessageBatchReceived -= OnMessageReceived;
            }
        }

        /// <inheritdoc/>
        public bool EnableChatProvider()
        {
            isEnabled = true;
            alreadyVotedChatters.Clear();
            return true;
        }

        /// <inheritdoc/>
        public bool DisableChatProvider()
        {
            isEnabled = false;
            alreadyVotedChatters.Clear();
            return true;
        }

        /// <inheritdoc/>
        public void ClearPerPollData()
        {
            alreadyVotedChatters.Clear();
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

                // Normally we would unpack the batch of messages and replay them at set intervals to
                // simulate a stream of chatters, however in this case we want to just dump them all
                // at once so as to not cut off any votes
                DateTime? lastConnectedTime = youtubeClient.GetLastStartConnectionTime();

                for (int i = 0; i < chatMessages.LiveChatMessages.Count; i++)
                {
                    LiveChatMessage liveChatMessage = chatMessages.LiveChatMessages[i];

                    // Check if the message is valid
                    if (liveChatMessage == null || liveChatMessage.AuthorDetails == null || liveChatMessage.Snippet == null ||
                        string.IsNullOrWhiteSpace(liveChatMessage.Snippet.DisplayMessage))
                    {
                        continue;
                    }

                    // Check if the message is too old
                    if (liveChatMessage.Snippet.PublishedAt == null ||
                        lastConnectedTime == null ||
                        liveChatMessage.Snippet.PublishedAt < lastConnectedTime)
                    {
                        continue;
                    }

                    // If they have already voted, then dip (unless it's me because im special hehe)
                    if (alreadyVotedChatters.ContainsKey(liveChatMessage.AuthorDetails.DisplayName) &&
                        !liveChatMessage.AuthorDetails.DisplayName.Equals("PhantomBadger", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }

                    // Get the choice number from the message
                    if (!TryGetPollChoiceNumberFromMessage(liveChatMessage.Snippet.DisplayMessage, out int choiceNumber))
                    {
                        return;
                    }

                    alreadyVotedChatters.TryAdd(liveChatMessage.AuthorDetails.DisplayName, 0);
                    OnChatVote?.Invoke(liveChatMessage.Snippet.DisplayMessage, choiceNumber);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Encountered Exception during OnMessageReceived: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Try to parse a number for the poll from the provided message
        /// </summary>
        private bool TryGetPollChoiceNumberFromMessage(string message, out int choiceNumber)
        {
            choiceNumber = -1;
            if (string.IsNullOrWhiteSpace(message))
            {
                return false;
            }
            string trimmedMessage = message.Trim();
            if (trimmedMessage.Length == 0)
            {
                return false;
            }

            if (int.TryParse(trimmedMessage[0].ToString(), out choiceNumber))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
