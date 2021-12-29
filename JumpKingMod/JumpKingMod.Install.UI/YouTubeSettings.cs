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
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
