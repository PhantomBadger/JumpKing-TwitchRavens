using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using JumpKingMod.Settings;
using Logging.API;
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
    /// A class representing a YouTube Chat Client
    /// </summary>
    public class YouTubeChatClient
    {
        public event EventHandler<YouTubeChatMessageBatchArgs> OnMessageBatchReceived;
        public event EventHandler OnConnected;
        public event EventHandler OnDisconnected;

        private readonly string channelId;
        private readonly YouTubeService youtubeService;
        private readonly ILogger logger;

        private BlockingCollection<YouTubeChatSessionInfo> connectionQueue;
        private CancellationTokenSource cancellationTokenSource;
        private Thread messagePollingThread;
        private DateTime? lastConnectedStartTime;
        private bool isConnected;

        private const int DefaultPollingIntervalInMilliseconds = 1000;

        /// <summary>
        /// The Constructor for creating a <see cref="YouTubeChatClient"/>
        /// </summary>
        /// <param name="channelId">The ID of the channel we intend to listen to</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public YouTubeChatClient(string channelId, string apiKey, ILogger logger)
        {
            this.channelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "Jump King Chat Ravens"
            });
            connectionQueue = new BlockingCollection<YouTubeChatSessionInfo>();
            cancellationTokenSource = new CancellationTokenSource();

            messagePollingThread = new Thread(ListenToChatLoop);
            messagePollingThread.Start();
            lastConnectedStartTime = null;
        }

        /// <summary>
        /// Gets a List of <see cref="YouTubeLiveStreamData"/> representing any live streams currently active on the appropriate channel
        /// </summary>
        /// <param name="isUpcomingEvent">If enabled, we will only look for Upcoming streams, if disabled, we will only look for currently live streams</param>
        public async Task<List<YouTubeLiveStreamData>> GetActiveStreamsAsync(bool isUpcomingEvent)
        {
            var liveStreamDatas = new List<YouTubeLiveStreamData>();
            if (youtubeService == null)
            {
                return liveStreamDatas;
            }

            // Create and execute the search request
            SearchResource.ListRequest searchRequest = youtubeService.Search.List("snippet");
            searchRequest.ChannelId = channelId;
            searchRequest.Type = "video";
            searchRequest.EventType = isUpcomingEvent ? SearchResource.ListRequest.EventTypeEnum.Upcoming : SearchResource.ListRequest.EventTypeEnum.Live;
            SearchListResponse searchResponse = await searchRequest.ExecuteAsync();

            // Iterate over the data in the response and collate the information we need
            for (int i = 0; i < searchResponse.Items.Count; i++)
            {
                SearchResult searchResult = searchResponse.Items[i];
                var liveStreamData = new YouTubeLiveStreamData(
                    searchResult.Id?.VideoId,
                    searchResult.Snippet.ChannelId,
                    searchResult.Snippet.ChannelTitle,
                    searchResult.Snippet.Title);

                liveStreamDatas.Add(liveStreamData);
            }

            return liveStreamDatas;
        }

        /// <summary>
        /// From a given video ID gets the appropriate Live Chat ID
        /// </summary>
        /// <param name="videoId">The Id of the video to search</param>
        public async Task<string> GetLiveChatIdAsync(string videoId)
        {
            if (youtubeService == null)
            {
                return null;
            }

            // Create and execute the Video Request
            VideosResource.ListRequest videoRequest = youtubeService.Videos.List("snippet,liveStreamingDetails");
            videoRequest.Id = videoId;
            VideoListResponse videoResponse = await videoRequest.ExecuteAsync();

            // Get the Live Chat ID from the item within
            if (videoResponse.Items.Count > 0)
            {
                return videoResponse.Items[0]?.LiveStreamingDetails?.ActiveLiveChatId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Kick off the chat listening for the provided Live Chat ID
        /// </summary>
        public bool Connect(string liveChatId, out string error)
        {
            error = string.Empty;
            if (isConnected)
            {
                error = "There is already a connection established!";
                return false;
            }

            if (youtubeService == null)
            {
                error = "The YouTube Service failed to be created!";
                return false;
            }

            lastConnectedStartTime = DateTime.Now;
            cancellationTokenSource = new CancellationTokenSource();

            // Kick off the connection
            isConnected = true;
            connectionQueue.Add(new YouTubeChatSessionInfo()
            {
                LiveChatId = liveChatId,
                Token = cancellationTokenSource.Token
            });

            return true;
        }

        /// <summary>
        /// Stop listening to any currently active live chats
        /// </summary>
        public bool Disconnect()
        {
            if (!isConnected)
            {
                return false;
            }

            if (youtubeService == null)
            {
                return false;
            }

            cancellationTokenSource.Cancel();
            isConnected = false;
            return true;
        }

        /// <summary>
        /// Returns the <see cref="DateTime"/> a last successful connection was made
        /// </summary>
        public DateTime? GetLastStartConnectionTime()
        {
            return lastConnectedStartTime;
        }

        /// <summary>
        /// Internal loop to process the chat messages
        /// </summary>
        private void ListenToChatLoop()
        {
            string chatRequestPageToken = null;
            while (true)
            {
                YouTubeChatSessionInfo sessionInfo = connectionQueue.Take();
                
                // We have connected and are actively listening to chat
                logger.Information($"Invoking OnConnected Event");
                OnConnected?.Invoke(this, new EventArgs());

                try
                {
                    while (!sessionInfo.Token.IsCancellationRequested)
                    {
                        // Build and execute the Chat Message Request
                        LiveChatMessagesResource.ListRequest chatMessageRequest = youtubeService.LiveChatMessages.List(sessionInfo.LiveChatId, "snippet,authorDetails");
                        chatMessageRequest.PageToken = chatRequestPageToken ?? "";
                        LiveChatMessageListResponse chatMessageResponse = chatMessageRequest.ExecuteAsync().Result;
                        if (chatMessageResponse == null || chatMessageResponse.Items == null)
                        {
                            logger.Warning($"An Invalid Response was received from the Live Chat Message Request!");
                            break;
                        }

                        if (chatMessageResponse.OfflineAt != null)
                        {
                            logger.Warning($"Identified the connected stream as now being offline, stopping listening for messages!");
                            break;
                        }

                        // Record the next page token so we don't get emssages we've already gotten
                        chatRequestPageToken = chatMessageResponse.NextPageToken;

                        // Process all messages
                        logger.Information($"Received YouTube Chat Message Batch of {chatMessageResponse.Items.Count} Messages");
                        float pollingDelay = chatMessageResponse.PollingIntervalMillis ?? DefaultPollingIntervalInMilliseconds;
                        YouTubeChatMessageBatchArgs eventArgs = new YouTubeChatMessageBatchArgs()
                        {
                            LiveChatMessages = chatMessageResponse.Items,
                            MinDelayBeforeNextBatch = pollingDelay,
                        };
                        OnMessageBatchReceived?.Invoke(this, eventArgs);

                        // Wait the increment given to us
                        Task.Delay((int)pollingDelay).Wait();
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Encountered exception from Chat Loop Listener - Forcibly Disconnecting!: {e.ToString()}");
                }
                finally
                {
                    isConnected = false;

                    // If we got here a connection has been cancelled or otherwise terminated
                    logger?.Information($"Invoking OnDisconnected Event");
                    OnDisconnected?.Invoke(this, new EventArgs());
                }
            }
        }
    }
}
