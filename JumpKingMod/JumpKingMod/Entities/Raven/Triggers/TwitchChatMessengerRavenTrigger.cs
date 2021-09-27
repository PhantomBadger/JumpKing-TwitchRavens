using JumpKingMod.API;
using JumpKingMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
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

namespace JumpKingMod.Twitch
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/>
    /// which triggers whenever a message is received in Twitch Chat
    /// </summary>
    public class TwitchChatMessengerRavenTrigger : IMessengerRavenTrigger
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private readonly ILogger logger;
        private readonly BlockingCollection<OnMessageReceivedArgs> relayRequestQueue;
        private readonly TwitchClient twitchClient;
        private readonly Thread processingThread;

        private bool isValid;

        private const string SettingsFileName = "TwitchChatRelay.settings";
        private readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>()
        {
            { "TwitchAccountName", "" },
            { "OAuth", "" },
        };

        /// <summary>
        /// Constructor for creating a <see cref="TwitchChatMessengerRavenTrigger"/>
        /// </summary>
        /// <param name="logger">An <see cref="ILogger"/> implementation to use for logging</param>
        public TwitchChatMessengerRavenTrigger(ILogger logger)
        {
            // Keep track of the logger
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.relayRequestQueue = new BlockingCollection<OnMessageReceivedArgs>();

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
            logger.Information($"Setting up Twitch Relay for {twitchName}");

            var credentials = new ConnectionCredentials(twitchName, oAuthToken);
            var clientOptions = new ClientOptions
            {
                MessagesAllowedInPeriod = 750,
                ThrottlingPeriod = TimeSpan.FromSeconds(30)
            };
            WebSocketClient webSocketClient = new WebSocketClient(clientOptions);
            twitchClient = new TwitchClient(webSocketClient);
            twitchClient.Initialize(credentials, twitchName);

            twitchClient.OnMessageReceived += OnMessageReceived;

            twitchClient.Connect();
            isValid = true;

            // Kick off the processing thread
            processingThread = new Thread(ProcessRelayRequests);
            processingThread.Start();
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
                    try
                    {
                        int rValue = int.Parse(colourHex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
                        int gValue = int.Parse(colourHex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
                        int bValue = int.Parse(colourHex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
                        nameColour = new Color(rValue, gValue, bValue, 255);
                    }
                    catch (Exception ex)
                    {
                        logger.Error($"Failed to parse colour for '{e.ChatMessage.Username}' with colour hex '{colourHex}'");
                    }
                }

                OnMessengerRavenTrigger?.Invoke(e.ChatMessage.DisplayName, nameColour, e.ChatMessage.Message);
            }
        }
    }
}
