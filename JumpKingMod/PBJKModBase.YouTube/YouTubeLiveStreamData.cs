using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.YouTube
{
    /// <summary>
    /// An aggregate class of Metadata about a YouTube Live Stream
    /// </summary>
    public class YouTubeLiveStreamData
    {
        public string VideoId { get; private set; }
        public string ChannelId { get; private set; }
        public string ChannelTitle { get; private set; }
        public string LiveStreamTitle { get; private set; }

        public YouTubeLiveStreamData(string videoId, string channelId, string channelTitle, string liveStreamTitle)
        {
            VideoId = videoId ?? throw new ArgumentNullException(nameof(videoId));
            ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
            ChannelTitle = channelTitle ?? throw new ArgumentNullException(nameof(channelTitle));
            LiveStreamTitle = liveStreamTitle ?? throw new ArgumentNullException(nameof(liveStreamTitle));
        }
    }
}
