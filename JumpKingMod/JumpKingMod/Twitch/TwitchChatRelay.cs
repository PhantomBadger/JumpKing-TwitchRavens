using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Entities;
using JumpKingMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
    /// A class responsible for relaying the twitch chat and acting on incoming messages
    /// </summary>
    public class TwitchChatRelay : IDisposable
    {
        private readonly ILogger logger;
        private readonly IGameStateObserver gameStateObserver;
        private readonly BlockingCollection<OnMessageReceivedArgs> relayRequestQueue;
        private readonly Thread processingThread;
        private readonly TwitchClient twitchClient;
        private readonly ModEntityManager modEntityManager;

        private bool isValid;
        private LinkedList<UITextEntity> textEntities;

        private const int NumberOfChatEntities = 5;
        private const string SettingsFileName = "TwitchChatRelay.settings";
        private readonly Dictionary<string, string> DefaultSettings = new Dictionary<string, string>()
        {
            { "TwitchAccountName", "" },
            { "OAuth", "" },
        };
        private readonly Vector2 ChatBoxBottomLeft = new Vector2(10, 100);

        /// <summary>
        /// Ctor for creating a <see cref="TwitchChatRelay"/>
        /// </summary>
        public TwitchChatRelay(ModEntityManager modEntityManager, IGameStateObserver gameStateObserver, ILogger logger)
        {
            // Keep track of the logger
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.gameStateObserver = gameStateObserver ?? throw new ArgumentNullException(nameof(gameStateObserver));
            this.relayRequestQueue = new BlockingCollection<OnMessageReceivedArgs>();

            // Set up the object pool once the game is initialised
            Task.Run(() =>
            {
                gameStateObserver.GameInitializedLatch.WaitOne();
                SetupObjectPool();
            });

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
        /// Implements <see cref="IDisposable.Dispose"/>
        /// Ends the processing thread
        /// </summary>
        public void Dispose()
        {
            twitchClient?.Disconnect();
            processingThread?.Abort();
        }

        /// <summary>
        /// Called by the underlying Twitch Client when a message is received, updates the UI Entity
        /// </summary>
        private void OnMessageReceived(object sender, OnMessageReceivedArgs e)
        {
            // Print the message to the console
            string displayString = $"{e?.ChatMessage?.Username}: {e?.ChatMessage?.Message}";
            logger.Information(displayString);

            // If the game isnt yet initialised, then exit now
            if (!gameStateObserver.IsGameLoopRunning())
            {
                return;
            }

            // If we havent got any text entities yet, set up our object pool
            if (textEntities == null || textEntities.Count <= 0)
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

                if (textEntities == null || textEntities.Count <= 0)
                {
                    continue;
                }

                // Otherwise construct the message to display
                string displayString = $"{e.ChatMessage.Username}: {e.ChatMessage.Message}";

                // Pull in the last UI Text Entity, and set it
                UITextEntity nextEntity = textEntities.Last.Value;
                textEntities.RemoveLast();
                nextEntity.ScreenSpacePosition = ChatBoxBottomLeft;
                nextEntity.TextValue = displayString;
                float prevEntityHeight = nextEntity.Size.Y;

                // Then loop over all the other text entities, offsetting their height as appropriate
                foreach (UITextEntity textEntity in textEntities)
                {
                    Vector2 size = textEntity.Size;
                    textEntity.ScreenSpacePosition -= new Vector2(0, prevEntityHeight);
                    prevEntityHeight = size.Y;
                }

                // Then add our text entity back onto the bottom
                textEntities.AddFirst(nextEntity);
            }
        }

        /// <summary>
        /// Initialises the object pool of <see cref="UITextEntity"/> instances to re-use
        /// </summary>
        private void SetupObjectPool()
        {
            textEntities = new LinkedList<UITextEntity>();
            for (int i = 0; i < NumberOfChatEntities; i++)
            {
                textEntities.AddLast(new UITextEntity(modEntityManager, ChatBoxBottomLeft, "", Color.White, UITextEntityAnchor.BottomLeft, JKContentManager.Font.MenuFontSmall));
            }
        }
    }
}
