namespace PBJKModBase.YouTube.API
{
    /// <summary>
    /// Interface representing an object which knows how to connect a YouTube Client
    /// </summary>
    public interface IYouTubeClientConnector
    {
        YouTubeChatClient YoutubeClient { get; }

        void StartAttemptingConnection();
    }
}
