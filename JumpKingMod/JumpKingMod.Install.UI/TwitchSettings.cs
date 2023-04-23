using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Install.UI
{
    /// <summary>
    /// An aggregate class of Twitch Settings
    /// </summary>
    public class TwitchSettings : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// The name of the twitch account to use
        /// </summary>
        public string TwitchAccountName
        {
            get
            {
                return twitchAccountName;
            }
            set
            {
                if (twitchAccountName != value)
                {
                    twitchAccountName = value;
                    RaisePropertyChanged(nameof(TwitchAccountName));
                }
            }
        }
        private string twitchAccountName;

        /// <summary>
        /// The OAuth token to use for Twitch Chat
        /// </summary>
        public string TwitchOAuth
        {
            get
            {
                return twitchOAuth;
            }
            set
            {
                if (twitchOAuth != value)
                {
                    twitchOAuth = value;
                    RaisePropertyChanged(nameof(TwitchOAuth));
                }
            }
        }
        private string twitchOAuth;

        /// <summary>
        /// Invokes the <see cref="PropertyChanged"/> event
        /// </summary>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
