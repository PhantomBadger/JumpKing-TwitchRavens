using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Entities;
using JumpKingMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Settings;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingMod.YouTube
{
    /// <summary>
    /// An implementation of <see cref="IYouTubeClientConnector"/> which will wait for
    /// manual user input before attempting to connect the youtube chat client
    /// </summary>
    public class ManualYouTubeClientConnector : IYouTubeClientConnector, IModEntity
    {
        public YouTubeChatClient YoutubeClient
        { 
            get
            {
                return youtubeClient;
            } 
        }

        private readonly YouTubeChatClient youtubeClient;
        private readonly ModEntityManager modEntityManager;
        private readonly ILogger logger;

        private ManualConnectorStates manualConnectorState;
        private BlockingCollection<ManualConnectionRequest> connectionRequests;
        private Thread connectionThread;
        private bool buttonPressedCooldown;
        private UITextEntity connectionStatusText;
        private Keys connectKey;

        /// <summary>
        /// Ctor for creating a <see cref="ManualYouTubeClientConnector"/>
        /// </summary>
        public ManualYouTubeClientConnector(YouTubeChatClient youtubeClient, ModEntityManager modEntityManager, UserSettings userSettings, ILogger logger)
        {
            this.youtubeClient = youtubeClient ?? throw new ArgumentNullException(nameof(youtubeClient));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            connectionRequests = new BlockingCollection<ManualConnectionRequest>();

            // Bind to the YouTube Client Disconnect Event
            youtubeClient.OnDisconnected += OnYouTubeClientDisconnected;

            // Prime the UI Text
            connectKey = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.YouTubeConnectKeyKey, Keys.F9);
            connectionStatusText = new UITextEntity(modEntityManager, new Vector2(480, 0), string.Empty, Color.Red,
                            UITextEntityAnchor.TopRight, JKContentManager.Font.MenuFontSmall);

            // Set our initial state
            ChangeState(ManualConnectorStates.Inactive);

            // Set up our processing thread
            connectionThread = new Thread(YouTubeConnectionProcessingThread);
            connectionThread.Start();
            buttonPressedCooldown = false;

            // Add ourselves to the Mod Entity Manager
            modEntityManager.AddEntity(this);
        }

        /// <summary>
        /// Called if the YouTube Client Disconnects on it's side of things, handles everything on our end
        /// </summary>
        private void OnYouTubeClientDisconnected(object sender, EventArgs e)
        {
            if (manualConnectorState != ManualConnectorStates.Inactive)
            {
                ChangeState(ManualConnectorStates.NotConnected);
            }
        }

        /// <summary>
        /// Waits for user input before attempting the connection
        /// </summary>
        public void StartAttemptingConnection()
        {
            if (manualConnectorState == ManualConnectorStates.Inactive)
            {
                logger.Information($"Not Connected!");
                ChangeState(ManualConnectorStates.NotConnected);
            }
        }

        /// <summary>
        /// Called on each update tick by the game
        /// Manages the current connector state
        /// </summary>
        public void Update(float delta)
        {
            // Get the keybaord state for future checks
            KeyboardState keyboardState = Keyboard.GetState();

            switch (manualConnectorState)
            {
                // Exit early if we're inactive
                case ManualConnectorStates.Inactive:
                    return;

                // If we're not connected, begin listening for the appropriate Connection Key
                case ManualConnectorStates.NotConnected:
                    if (keyboardState.IsKeyDown(connectKey))
                    {
                        if (!buttonPressedCooldown)
                        {
                            buttonPressedCooldown = true;
                            ChangeState(ManualConnectorStates.AttemptingConnection);
                            logger.Information($"Attempting Connection...");


                            // Attempt to connect, setting a delegate to be ran with the result
                            var request = new ManualConnectionRequest((bool result) =>
                            {
                                if (result)
                                {
                                    ChangeState(ManualConnectorStates.Connected);
                                    logger.Information($"Connection Attempt Succeeded!");
                                }
                                else
                                {
                                    ChangeState(ManualConnectorStates.NotConnected);
                                    logger.Information($"Connection Attempt Failed!");
                                }
                            });
                            connectionRequests.Add(request);
                        }
                    }
                    else
                    {
                        buttonPressedCooldown = false;
                    }
                    break;

                // Don't need to do anything if we're currently attempting to connect
                case ManualConnectorStates.AttemptingConnection:
                    break;

                // If we're connected, we only need to listen for a disconnect request
                case ManualConnectorStates.Connected:
                    if (keyboardState.IsKeyDown(connectKey))
                    {
                        if (!buttonPressedCooldown)
                        {
                            buttonPressedCooldown = true;
                            ChangeState(ManualConnectorStates.NotConnected);

                            // Tell the client to disconnect
                            youtubeClient.Disconnect();
                            logger.Information($"Disconnected!");
                        }
                    }
                    else
                    {
                        buttonPressedCooldown = false;
                    }
                    break;
                default:
                    throw new NotImplementedException($"Unsupported Manual Connector State of {manualConnectorState}");
            }
        }

        /// <summary>
        /// Called by the game to draw stuff
        /// </summary>
        public void Draw()
        {
            // Do Nothing
        }

        /// <summary>
        /// Attempt to perform a YouTube Connection
        /// </summary>
        /// <returns></returns>
        private async Task<Tuple<bool, string>> AttemptYouTubeConnection()
        {
            try
            {
                // Get the available live streams for the user associated with the client
                List<YouTubeLiveStreamData> liveStreamData = await youtubeClient.GetActiveStreamsAsync(isUpcomingEvent: false);
                if (liveStreamData.Count <= 0)
                {
                    var errorText = "No Live Stream Data found for the specified user!";
                    logger.Error(errorText);
                    return new Tuple<bool, string>(false, errorText);
                }

                // Get the first valid video ID within
                string videoId = liveStreamData.FirstOrDefault()?.VideoId;
                if (string.IsNullOrWhiteSpace(videoId))
                {
                    var errorText = "Invalid Video ID found for the found Live Stream Data";
                    logger.Error(errorText);
                    return new Tuple<bool, string>(false, errorText);
                }

                // Get the Live Chat Id of that video
                string liveChatId = await youtubeClient.GetLiveChatIdAsync(videoId);

                // Connect using that live chat Id
                bool connectionResult = youtubeClient.Connect(liveChatId, out string error);
                return new Tuple<bool, string>(connectionResult, error);
            }
            catch (Exception e)
            {
                var errorText = $"Encountered Exception when attempting to connect to YouTube: {e.ToString()}";
                logger.Error(errorText);
                return new Tuple<bool, string>(false, errorText);
            }
        }

        /// <summary>
        /// A thread which runs parallel to this and handles the connection code, as it may be somewhat long-winded and
        /// we don't want to risk stalling the main thread
        /// </summary>
        private void YouTubeConnectionProcessingThread()
        {
            while (true)
            {
                ManualConnectionRequest connectionRequest = connectionRequests.Take();

                Tuple<bool, string> connectionResult = AttemptYouTubeConnection().Result;

                if (!string.IsNullOrWhiteSpace(connectionResult.Item2))
                {
                    logger.Error($"Encountered error during YouTube Connection: {connectionResult.Item2}");
                }
                connectionRequest.ResponseCallback.Invoke(connectionResult.Item1);
            }
        }

        /// <summary>
        /// Changes the state of the <see cref="ManualYouTubeClientConnector"/> and updates the appropriate UI Elements
        /// </summary>
        private void ChangeState(ManualConnectorStates newState)
        {
            if (connectionStatusText != null)
            {
                switch (newState)
                {
                    case ManualConnectorStates.Inactive:
                        connectionStatusText.TextValue = string.Empty;
                        break;
                    case ManualConnectorStates.NotConnected:
                        connectionStatusText.TextValue = $"Not Connected - Press {connectKey.ToString()} to Connect";
                        connectionStatusText.TextColor = Color.Red;
                        break;
                    case ManualConnectorStates.AttemptingConnection:
                        connectionStatusText.TextValue = $"Attempting Connection...";
                        connectionStatusText.TextColor = Color.Yellow;
                        break;
                    case ManualConnectorStates.Connected:
                        connectionStatusText.TextValue = $"Connected - Press {connectKey.ToString()} to Disconnect";
                        connectionStatusText.TextColor = Color.White;
                        break;
                    default:
                        throw new NotImplementedException($"Unrecognised ManualConnectorState of {newState}");
                }
            }
            manualConnectorState = newState;
        }
    }
}
