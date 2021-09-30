using JumpKingMod.API;
using JumpKingMod.Settings;
using JumpKingMod.Twitch;
using Logging.API;
using Microsoft.Xna.Framework;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly TwitchClient twitchClient;

        private readonly string channelRewardID;

        /// <summary>
        /// Ctor for creating a <see cref="TwitchChannelPointMessengerRavenTrigger"/>
        /// </summary>
        public TwitchChannelPointMessengerRavenTrigger(TwitchClient twitchClient, UserSettings settings, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.twitchClient = twitchClient ?? throw new ArgumentNullException(nameof(twitchClient));

            // Get the Channel Reward ID we intend to use
            channelRewardID = settings.GetSettingOrDefault(JumpKingModSettingsContext.RavenChannelPointRewardIDKey, string.Empty);
            if (string.IsNullOrWhiteSpace(channelRewardID))
            {
                logger.Error($"Unable to identify a valid Channel Reward ID for Raven Spawning from the Settings File!");
                return;
            }

            twitchClient.OnMessageReceived += OnMessageReceived;
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
            if (e.ChatMessage.CustomRewardId.Equals(channelRewardID, StringComparison.OrdinalIgnoreCase))
            {
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
