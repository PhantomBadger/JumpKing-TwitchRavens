using PBJKModBase.YouTube.Settings;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.YouTube
{
    /// <summary>
    /// A factory which produces and caches a <see cref="YouTubeChatClient"/> based on the
    /// provided settings
    /// </summary>
    public class YouTubeChatClientFactory
    {
        private readonly ILogger logger;
        private readonly UserSettings userSettings;

        private YouTubeChatClient youtubeClient;

        public YouTubeChatClientFactory(UserSettings userSettings, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

            youtubeClient = null;
        }

        /// <summary>
        /// Creates or gets a cached version of a <see cref="YouTubeChatClient"/>
        /// </summary>
        public YouTubeChatClient GetYouTubeClient(bool forceRemake = false)
        {
            if (youtubeClient != null)
            {
                if (forceRemake)
                {
                    ClearYouTubeClient();
                }
                else
                {
                    return youtubeClient;
                }
            }

            // Initialise the settings and attempt to load the api key
            string channelId = userSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeChannelNameKey, string.Empty);
            string apiKey = userSettings.GetSettingOrDefault(PBJKModBaseYouTubeSettingsContext.YouTubeApiKeyKey, string.Empty);

            // Check if any of the data is bad, exit now
            if (string.IsNullOrWhiteSpace(channelId))
            {
                logger.Error($"No valid YouTube ChannelName found in the {PBJKModBaseYouTubeSettingsContext.SettingsFileName} file!");
                return null;
            }
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                logger.Error($"Invalid API Key Provided!");
                return null;
            }

            channelId = channelId.Trim();
            apiKey = apiKey.Trim();
            logger.Information($"Setting up YouTube Chat Client for '{channelId}'");

            youtubeClient = new YouTubeChatClient(channelId, apiKey, logger);

            return youtubeClient;
        }

        public void ClearYouTubeClient()
        {
            if (youtubeClient != null)
            {
                try
                {
                    youtubeClient.Disconnect();
                }
                catch (ObjectDisposedException de)
                {
                    // ignore
                }
                finally
                {
                    youtubeClient = null;
                }
            }
        }
    }
}
