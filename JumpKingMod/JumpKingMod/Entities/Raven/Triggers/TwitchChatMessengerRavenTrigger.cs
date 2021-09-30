using JumpKingMod.API;
using JumpKingMod.Settings;
using JumpKingMod.Twitch;
using Logging.API;
using Microsoft.Xna.Framework;
using Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace JumpKingMod.Entities.Raven.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/>
    /// which triggers whenever a message is received in Twitch Chat
    /// </summary>
    public class TwitchChatMessengerRavenTrigger : IMessengerRavenTrigger, IDisposable
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private readonly ILogger logger;
        private readonly BlockingCollection<OnMessageReceivedArgs> relayRequestQueue;
        private readonly TwitchClient twitchClient;
        private readonly Thread processingThread;

        /// <summary>
        /// Constructor for creating a <see cref="TwitchChatMessengerRavenTrigger"/>
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> implementation to use for logging</param>
        public TwitchChatMessengerRavenTrigger(TwitchClient twitchClient, UserSettings userSettings, ILogger logger)
        {
            // Keep track of the logger
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.relayRequestQueue = new BlockingCollection<OnMessageReceivedArgs>();
            this.twitchClient = twitchClient ?? throw new ArgumentNullException(nameof(twitchClient));
            twitchClient.OnMessageReceived += OnMessageReceived;

            // Kick off the processing thread
            processingThread = new Thread(ProcessRelayRequests);
            processingThread.Start();
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            if (twitchClient != null)
            {
                twitchClient.OnMessageReceived -= OnMessageReceived;
            }
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            // Print the message to the console
            string displayString = $"{e?.ChatMessage?.Username}: {e?.ChatMessage?.Message}";
            logger.Information(displayString);

            if (e == null || e.ChatMessage == null || string.IsNullOrWhiteSpace(e.ChatMessage.Message))
            {
                return;
            }

            // Queue up the request
            relayRequestQueue.Add(e);
        }

        /// <summary>
        /// Runs a loop which processes the chat relay requests
        /// </summary>
        private void ProcessRelayRequests()
        {
            while (true)
            {
                // Pop off a request
                OnMessageReceivedArgs e = relayRequestQueue.Take();
                string colourHex = e.ChatMessage.ColorHex;
                Color nameColour = Color.White;
                if (!string.IsNullOrWhiteSpace(colourHex) && colourHex.Length >= 7)
                {
                    nameColour = TwitchHexColourParser.ParseColourFromHex(colourHex);
                }
                
                OnMessengerRavenTrigger?.Invoke(e.ChatMessage.DisplayName, nameColour, e.ChatMessage.Message);
            }
        }
    }
}
