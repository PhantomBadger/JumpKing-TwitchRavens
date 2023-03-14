using JumpKingModifiersMod.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    public delegate void JumpTriggeredDelegate(JumpState jumpState);

    /// <summary>
    /// An interface representing an object capable of reading and triggering player jumps
    /// </summary>
    public interface IPlayerJumper
    {
        /// <summary>
        /// An event triggered whenever the player jumps
        /// </summary>
        event JumpTriggeredDelegate OnPlayerJumped;

        /// <summary>
        /// Gets the last jump state, null if none has been recorded yet
        /// </summary>
        /// <returns></returns>
        JumpState GetPreviousJumpState();

        /// <summary>
        /// Requests the player to jump
        /// </summary>
        void RequestJump(RequestedJumpState jumpState);
    }
}
