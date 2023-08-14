using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.YouTube
{
    /// <summary>
    /// An enum of possible states for the <see cref="ManualYouTubeClientConnector"/>
    /// </summary>
    public enum ManualConnectorStates
    {
        Inactive,
        NotConnected,
        AttemptingConnection,
        Connected,
    }
}
