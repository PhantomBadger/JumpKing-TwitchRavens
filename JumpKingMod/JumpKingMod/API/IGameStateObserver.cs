using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingMod.API
{
    /// <summary>
    /// An interface representing an object which knows the current game state
    /// </summary>
    public interface IGameStateObserver
    {
        ManualResetEvent GameInitializedLatch { get; }

        /// <summary>
        /// Returns true if the game has initialised it's core systems
        /// </summary>
        bool IsGameInitialised();

        /// <summary>
        /// Returns true if the game loop is running (ie, we're not in the menu)
        /// </summary>
        bool IsGameLoopRunning();
    }
}
