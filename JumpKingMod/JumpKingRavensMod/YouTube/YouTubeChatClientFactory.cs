using JumpKingRavensMod.Settings;
using Logging.API;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.YouTube
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
        public YouTubeChatClient GetYouTubeClient()
        {
            if (youtubeClient != null)
            {
                return youtubeClient;
            }
            else
            {
                // Initialise the settings and attempt to load the api key
                string channelId = userSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.YouTubeChannelNameKey, string.Empty);
                string apiKey = userSettings.GetSettingOrDefault(JumpKingRavensModSettingsContext.YouTubeApiKeyKey, string.Empty);

                // Check if any of the data is bad, exit now
                if (string.IsNullOrWhiteSpace(channelId))
                {
                    logger.Error($"No valid YouTube ChannelName found in the {JumpKingRavensModSettingsContext.SettingsFileName} file!");
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
        }
    }
}
