using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    public delegate void ChatVoteDelegate(string chatName, int pollOption);

    /// <summary>
    /// A wrapper for receiving chat votes
    /// </summary>
    public interface IPollChatProvider : IDisposable
    {
        event ChatVoteDelegate OnChatVote;

        /// <summary>
        /// Whether or not the provider is ready to provide chat information
        /// </summary>
        /// <returns><c>true</c> if so, <c>false</c> if not</returns>
        bool IsReadyToProvide();

        /// <summary>
        /// Enables the Chat Provider
        /// </summary>
        /// <returns><c>true</c> if it enables successfully, <c>false</c> if it fails</returns>
        bool EnableChatProvider();

        /// <summary>
        /// Disables the Chat Provider
        /// </summary>
        /// <returns><c>true</c> if it disables successfully, <c>false</c> if it fails</returns>
        bool DisableChatProvider();

        /// <summary>
        /// Clears any per-poll data that may be cached
        /// </summary>
        void ClearPerPollData();
    }
}
