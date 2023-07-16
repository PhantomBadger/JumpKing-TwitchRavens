using JumpKingModifiersMod.API;
using Logging.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Client;

namespace JumpKingModifiersMod.Triggers.Poll
{
    /// <summary>
    /// An implementation of <see cref="IPollChatProvider"/> for a Twitch Client
    /// </summary>
    public class TwitchPollChatProvider : IPollChatProvider
    {
        public event ChatVoteDelegate OnChatVote;

        private readonly ILogger logger;
        private readonly TwitchClient twitchClient;
        private readonly ConcurrentDictionary<string, byte> alreadyVotedChatters;

        private bool isEnabled;

        /// <summary>
        /// Ctor for creating a <see cref="TwitchPollTrigger"/>
        /// </summary>
        /// <param name="twitchClient">The Twitch Client to use</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public TwitchPollChatProvider(TwitchClient twitchClient, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.twitchClient = twitchClient ?? throw new ArgumentNullException(nameof(twitchClient));

            alreadyVotedChatters = new ConcurrentDictionary<string, byte>();
            isEnabled = false;

            twitchClient.OnMessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            twitchClient.OnMessageReceived -= OnMessageReceived;
            twitchClient.Disconnect();
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
            return false;
        }

        /// <inheritdoc/>
        public void ClearPerPollData()
        {
            alreadyVotedChatters.Clear();
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            try
            {
                if (!isEnabled || e == null || e.ChatMessage == null || string.IsNullOrWhiteSpace(e.ChatMessage.Message))
                {
                    return;
                }

                // If they have already voted, then dip (unless it's me because im special hehe)
                if (alreadyVotedChatters.ContainsKey(e.ChatMessage.UserId) &&
                    !e.ChatMessage.DisplayName.Equals("PhantomBadger", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                // Get the choice number from the message
                if (!TryGetPollChoiceNumberFromMessage(e.ChatMessage.Message, out int choiceNumber))
                {
                    return;
                }

                alreadyVotedChatters.TryAdd(e.ChatMessage.UserId, 0);
                OnChatVote?.Invoke(e.ChatMessage.DisplayName, choiceNumber);
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
