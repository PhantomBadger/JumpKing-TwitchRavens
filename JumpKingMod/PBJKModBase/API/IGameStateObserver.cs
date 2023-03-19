using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PBJKModBase.API
{
    /// <summary>
    /// An interface representing an object which knows the current game state
    /// </summary>
    public delegate void GameLoopRunningDelegate();
    public delegate void GameLoopNotRunningDelegate();
    public interface IGameStateObserver
    {
        event GameLoopRunningDelegate OnGameLoopRunning;
        event GameLoopNotRunningDelegate OnGameLoopNotRunning;
        ManualResetEvent GameInitializedLatch { get; }
        ManualResetEvent AssetsLoadedLatch { get; }

        /// <summary>
        /// Returns true if the game has initialised it's core systems
        /// </summary>
        bool IsGameInitialised();

        /// <summary>
        /// Returns true if the game loop is running (ie, we're not in the menu)
        /// </summary>
        bool IsGameLoopRunning();

        /// <summary>
        /// Returns true if the games assets have been loaded
        /// </summary>
        bool AreGameAssetsLoaded();
    }
}
