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

namespace JumpKingMod.Entities.Raven.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/> which triggers <see cref="OnMessengerRavenTrigger"/>
    /// when a channel point reward is redeemed
    /// </summary>
    public class TwitchChannelPointMessengerRavenTrigger : IMessengerRavenTrigger, IDisposable
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private readonly UserSettings settings;
        private readonly ILogger logger;
        private readonly IExcludedTermFilter excludedTermFilter;
        private readonly TwitchClient twitchClient;
        private readonly BlockingCollection<OnMessageReceivedArgs> relayRequestQueue;
        private readonly Thread processingThread;
        private readonly string channelRewardID;

        /// <summary>
        /// Ctor for creating a <see cref="TwitchChannelPointMessengerRavenTrigger"/>
        /// </summary>
        public TwitchChannelPointMessengerRavenTrigger(TwitchClient twitchClient, UserSettings settings, IExcludedTermFilter excludedTermFilter, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.twitchClient = twitchClient ?? throw new ArgumentNullException(nameof(twitchClient));
            this.excludedTermFilter = excludedTermFilter ?? throw new ArgumentNullException(nameof(excludedTermFilter));
            this.relayRequestQueue = new BlockingCollection<OnMessageReceivedArgs>();

            // Get the Channel Reward ID we intend to use
            channelRewardID = settings.GetSettingOrDefault(JumpKingModSettingsContext.RavenChannelPointRewardIDKey, string.Empty);
            if (string.IsNullOrWhiteSpace(channelRewardID))
            {
                logger.Error($"Unable to identify a valid Channel Reward ID for Raven Spawning from the Settings File!");
                return;
            }
            else
            {
                logger.Information($"Listening for Channel Point ID '{channelRewardID}' to spawn Ravens");
            }

            twitchClient.OnMessageReceived += OnMessageReceived;

            // Kick off the processing thread
            processingThread = new Thread(ProcessRelayRequests);
            processingThread.Start();
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up events
        /// </summary>
        public void Dispose()
        {
            if (twitchClient != null)
            {
                twitchClient.OnMessageReceived -= OnMessageReceived;
            }
        }

        /// <summary>
        /// Called by the TwitchClient when a message is received
        /// </summary>
        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            if (e == null || e.ChatMessage == null || e.ChatMessage.CustomRewardId == null ||
                string.IsNullOrWhiteSpace(e?.ChatMessage.Message))
            {
                return;
            }

            try
            {
                if (e.ChatMessage.CustomRewardId.Equals(channelRewardID, StringComparison.OrdinalIgnoreCase))
                {
                    // Queue up the request
                    relayRequestQueue.Add(e);
                }
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
                    OnMessageReceivedArgs e = relayRequestQueue.Take();
                    string colourHex = e.ChatMessage.ColorHex;
                    Color nameColour = Color.White;
                    if (!string.IsNullOrWhiteSpace(colourHex) && colourHex.Length >= 7)
                    {
                        nameColour = TwitchHexColourParser.ParseColourFromHex(colourHex);
                    }

                    // Skip anything containing an excluded term
                    if (excludedTermFilter.ContainsExcludedTerm(e.ChatMessage.Message))
                    {
                        logger.Warning($"Skipped Triggered Chat Message from '{e.ChatMessage.DisplayName}', as it contained an excluded term");
                        continue;
                    }

                    OnMessengerRavenTrigger?.Invoke(e.ChatMessage.DisplayName, nameColour, e.ChatMessage.Message, e.ChatMessage.IsSubscriber, isPriority: false);
                }
                catch (Exception ex)
                {
                    logger.Error($"Encountered Exception during ProcessRelayRequests: {ex.ToString()}");
                }
            }
        }
    }
}
