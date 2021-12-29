using Microsoft.Xna.Framework.Input;
using System.ComponentModel;

namespace JumpKingMod.Install.UI
{
    /// <summary>
    /// An aggregatce alss of YouTube Settings
    /// </summary>
    public class YouTubeSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The name of the youtube account to use
        /// </summary>
        public string YouTubeAccountName
        {
            get
            {
                return youTubeAccountName;
            }
            set
            {
                if (youTubeAccountName != value)
                {
                    youTubeAccountName = value;
                    RaisePropertyChanged(nameof(YouTubeAccountName));
                }
            }
        }
        private string youTubeAccountName;

        /// <summary>
        /// The API key to use for YouTube
        /// </summary>
        public string YouTubeAPIKey
        {
            get
            {
                return youTubeAPIKey;
            }
            set
            {
                if (youTubeAPIKey != value)
                {
                    youTubeAPIKey = value;
                    RaisePropertyChanged(nameof(YouTubeAPIKey));
                }
            }
        }
        private string youTubeAPIKey;

        /// <summary>
        /// The key to press to connect/disconnect from youtube
        /// </summary>
        public Keys ConnectKey
        {
            get
            {
                return connectKey;
            }
            set
            {
                if (connectKey != value)
                {
                    connectKey = value;
                    RaisePropertyChanged(nameof(ConnectKey));
                }
            }
        }
        private Keys connectKey;

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
