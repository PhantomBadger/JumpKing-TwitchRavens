using Google.Apis.YouTube.v3.Data;
using JumpKingMod.API;
using JumpKingMod.YouTube;
using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/> which triggers
    /// whenever a message is received in the YouTube Chat
    /// </summary>
    public class YouTubeChatMessengerRavenTrigger : IMessengerRavenTrigger, IDisposable
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private readonly ILogger logger;
        private readonly IExcludedTermFilter excludedTermFilter;
        private readonly YouTubeChatClient youtubeClient;
        private readonly BlockingCollection<YouTubeChatMessageBatchArgs> relayRequestQueue;
        private readonly Thread processingThread;

        public YouTubeChatMessengerRavenTrigger(YouTubeChatClient youtubeClient, IExcludedTermFilter excludedTermFilter, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.excludedTermFilter = excludedTermFilter ?? throw new ArgumentNullException(nameof(excludedTermFilter));
            this.youtubeClient = youtubeClient ?? throw new ArgumentNullException(nameof(youtubeClient));
            relayRequestQueue = new BlockingCollection<YouTubeChatMessageBatchArgs>();

            youtubeClient.OnMessageBatchReceived += OnMessageReceived;
            
            // Kick off the processing thread
            processingThread = new Thread(ProcessRelayRequests);
            processingThread.Start();
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            if (youtubeClient != null)
            {
                youtubeClient.OnMessageBatchReceived -= OnMessageReceived;
            }
        }

        /// <summary>
        /// Called by the underlying YouTube Chat Client when a message is received
        /// </summary>
        private void OnMessageReceived(object sender, YouTubeChatMessageBatchArgs chatMessages)
        {
            try
            {
                // Queue up the request
                relayRequestQueue.Add(chatMessages);
            }
            catch (Exception ex)
            {
                logger.Error($"Encountered Exception during OnMessageReceived: {ex.ToString()}");
            }
        }

        /// <summary>
        /// Runs a loop which processes the chat relay requests
        /// </summary>
        private void ProcessRelayRequests()
        {
            while (true)
            {
                try
                {
                    // Pop off a request
                    YouTubeChatMessageBatchArgs chatMessageBatch = relayRequestQueue.Take();

                    // Calculate the delay time
                    // This is calculated by taking the minimum time expected before we receive another batch and diving it by the number
                    // of messages to simulate a steady stream
                    float delayBetweenMessagesInMilliseconds = chatMessageBatch.MinDelayBeforeNextBatch / (chatMessageBatch.LiveChatMessages.Count + 1);

                    DateTime? lastConnectedTime = youtubeClient.GetLastStartConnectionTime();

                    // Go over all of our messages and process them
                    for (int i = 0; i < chatMessageBatch.LiveChatMessages.Count; i++)
                    {
                        LiveChatMessage liveChatMessage = chatMessageBatch.LiveChatMessages[i];

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

                        // Skip anything containing an excluded term
                        if (excludedTermFilter.ContainsExcludedTerm(liveChatMessage.Snippet.DisplayMessage))
                        {
                            logger.Warning($"Skipped Triggered Chat Message from '{liveChatMessage.Snippet.DisplayMessage}', as it contained an excluded term");
                            continue;
                        }

                        // Generate a deterministic colour from the name
                        Color nameColour = YouTubeHexColourGenerator.GenerateColourFromName(liveChatMessage.AuthorDetails.DisplayName, logger);

                        bool isPriority = false;
                        if (liveChatMessage.AuthorDetails.DisplayName.Equals("PhantomBadger", StringComparison.OrdinalIgnoreCase))
                        {
                            isPriority = true;
                        }

                        // Invoke the Raven Trigger
                        OnMessengerRavenTrigger?.Invoke(liveChatMessage.AuthorDetails.DisplayName, nameColour, liveChatMessage.Snippet.DisplayMessage, liveChatMessage.AuthorDetails.IsChatSponsor ?? false, isPriority);

                        // Wait a fixed amount based on the polling interval and the number of messages
                        Task.Delay((int)delayBetweenMessagesInMilliseconds).Wait();
                    }
                }
                catch (Exception ex)
                {
                    logger.Error($"Encountered Exception during ProcessRelayRequests: {ex.ToString()}");
                }
            }
        }
    }
}
