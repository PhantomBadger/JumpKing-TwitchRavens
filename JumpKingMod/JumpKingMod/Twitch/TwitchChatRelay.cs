using JumpKingMod.Entities;
using JumpKingMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TwitchLib.Client;
using TwitchLib.Client.Models;
using TwitchLib.Communication.Clients;
using TwitchLib.Communication.Models;

namespace JumpKingMod.Twitch
{
    /// <summary>
    /// A class responsible for relaying the twitch chat and acting on incoming messages
    /// </summary>
    public class TwitchChatRelay
    {
        private readonly ILogger logger;

        private bool isValid;
        private UITextEntity textEntity;
     
        private const string SettingsFileName = "TwitchChatRelay.settings";
        private readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>()
        {
            { "TwitchAccountName", "" },
            { "OAuth", "" },
        };

        /// <summary>
        /// Ctor for creating a <see cref="TwitchChatRelay"/>
        /// </summary>
        public TwitchChatRelay(ILogger logger)
        {
            // Keep track of the logger
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Initialise the settings and attempt to load the OAuth Token
            var userSettings = new UserSettings(SettingsFileName, DefaultSettings, logger);
            string oAuthToken = userSettings.GetSettingOrDefault("OAuth", string.Empty);
            string twitchName = userSettings.GetSettingOrDefault("TwitchAccountName", string.Empty);

            // If the Oauth Token is bad, exit now
            if (string.IsNullOrWhiteSpace(oAuthToken))
            {
                logger.Error($"No valid OAuth token found in the {SettingsFileName} file!");
                isValid = false;
                return;
            }
            if (string.IsNullOrWhiteSpace(twitchName))
            {
                logger.Error($"No valid TwitchAccountName found in the {SettingsFileName} file!");
                isValid = false;
                return;
            }

            var credentials = new ConnectionCredentials(twitchName, oAuthToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
            TwitchClient twitchClient = new TwitchClient(webSocketClient);
            twitchClient.Initialize(credentials, twitchName);

            twitchClient.OnMessageReceived += OnMessageReceived;

            twitchClient.Connect();
            isValid = true;
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, TwitchLib.Client.Events.OnMessageReceivedArgs e)
        {
            string displayString = $"{e.ChatMessage.Username}: {e.ChatMessage.Message}";
            logger.Information(displayString);
            if (textEntity == null)
            {
                textEntity = new UITextEntity(new Vector2(100, 100), displayString, Color.White, UITextEntityAnchor.BottomLeft);
            }
            else
            {
                textEntity.TextValue = displayString;
            }
        }
    }
}
