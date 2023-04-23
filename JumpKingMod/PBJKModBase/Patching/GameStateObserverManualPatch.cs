using HarmonyLib;
using Logging.API;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PBJKModBase.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IGameStateObserver"/> which will
    /// keep track of the current state of the game, and allow others to query it
    /// </summary>
    public class GameStateObserverManualPatch : IManualPatch, IGameStateObserver
    {
        public event GameLoopRunningDelegate OnGameLoopRunning;
        public event GameLoopNotRunningDelegate OnGameLoopNotRunning;

        private static ILogger logger;
        private static bool isGameInitialized;
        private static bool areGameAssetsLoaded;
        private static object gameLoopObject;
        private static MethodInfo gameLoopIsRunningMethodInfo;

        private bool prevGameLoopState;
        private Task gameStatePollingTask;
        private CancellationTokenSource cancellationTokenSource;

        public static IGameStateObserver Instance
        {
            get
            {
                if (instance == null)
                {
                    throw new InvalidOperationException($"GameStateObserver was in a null state when requested. This shouldn't happen!");
                }
                return instance;
            }
        }
        private static IGameStateObserver instance;

        /// <summary>
        /// A latch which lets you wait until the game has been initialised
        /// </summary>
        public ManualResetEvent GameInitializedLatch
        {
            get
            {
                return gameInitializedLatch;
            }
        }
        private static ManualResetEvent gameInitializedLatch;

        /// <summary>
        /// A latch which lets you wait until the game has loaded assets
        /// </summary>
        public ManualResetEvent AssetsLoadedLatch
        {
            get
            {
                return assetsLoadedLatch;
            }
        }
        private static ManualResetEvent assetsLoadedLatch;

        /// <summary>
        /// Default ctor for creating a <see cref="GameStateObserverManualPatch"/>, initialises the states
        /// </summary>
        public GameStateObserverManualPatch(ILogger newLogger)
        {
            GameStateObserverManualPatch.instance = this;

            gameInitializedLatch = new ManualResetEvent(false);
            assetsLoadedLatch = new ManualResetEvent(false);
            isGameInitialized = false;
            areGameAssetsLoaded = false;
            logger = newLogger ?? throw new ArgumentNullException(nameof(newLogger));

            cancellationTokenSource = new CancellationTokenSource();
            prevGameLoopState = false;
            gameStatePollingTask = Task.Run(() => PollGameStateLoop(cancellationTokenSource.Token));
        }

        /// <summary>
        /// Returns whether the game has been initialised or not
        /// </summary>
        public bool IsGameInitialised()
        {
            return isGameInitialized;
        }

        /// <summary>
        /// Returns whether the game loop is running
        /// </summary>
        public bool IsGameLoopRunning()
        {
            return gameLoopObject != null && (bool)gameLoopIsRunningMethodInfo.Invoke(gameLoopObject, null);
        }

        /// <summary>
        /// Returns true if the game has loaded its assets
        /// </summary>
        /// <returns></returns>
        public bool AreGameAssetsLoaded()
        {
            return areGameAssetsLoaded;
        }

        /// <summary>
        /// Sets up the manual patch to query the game states
        /// </summary>
        public void SetUpManualPatch(Harmony harmony)
        {
            try
            {
                var makeBTMethod = AccessTools.Method("JumpKing.JumpGame:MakeBT");
                var postfixBTPatchMethod = AccessTools.Method($"PBJKModBase.Patching.{this.GetType().Name}:PostfixMakeBTPatchMethod");
                harmony.Patch(makeBTMethod, postfix: new HarmonyMethod(postfixBTPatchMethod));

                var loadAssetsMethod = AccessTools.Method("JumpKing.JKContentManager:LoadAssets");
                var postfixLoadAssetsMethod = AccessTools.Method($"PBJKModBase.Patching.{this.GetType().Name}:PostfixLoadAssetsPatchMethod");
                harmony.Patch(loadAssetsMethod, postfix: new HarmonyMethod(postfixLoadAssetsMethod));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Called after the JumpGame.MakeBT
        /// Sets the latch and local var
        /// Gets a reference to the game loop for querying
        /// </summary>
        public static void PostfixMakeBTPatchMethod(object __instance)
        {
            isGameInitialized = true;
            gameInitializedLatch?.Set();

            var gameLoopField = AccessTools.Field(__instance.GetType(), "m_game_loop");
            gameLoopObject = gameLoopField.GetValue(__instance);
            gameLoopIsRunningMethodInfo = gameLoopObject?.GetType().GetMethod("IsRunning");

            logger.Information($"Base Game has finished calling JumpGame.MakeBT!");
        }

        /// <summary>
        /// Called after JKContentManager.LoadAssets
        /// Sets the latch and local var
        /// </summary>
        public static void PostfixLoadAssetsPatchMethod(object __instance)
        {
            areGameAssetsLoaded = true;
            assetsLoadedLatch?.Set();

            logger.Information($"Base Game has finished calling JKContentManager.LoadAssets!");
        }

        /// <summary>
        /// Poll the game loop state to trigger the appropriate events
        /// </summary>
        private void PollGameStateLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                bool gameLoopState = gameLoopObject != null && gameLoopIsRunningMethodInfo != null && (bool)gameLoopIsRunningMethodInfo.Invoke(gameLoopObject, null);
                if (gameLoopState != prevGameLoopState)
                {
                    prevGameLoopState = gameLoopState;

                    // Trigger the appropriate events
                    if (gameLoopState)
                    {
                        logger.Information($"Game Loop is now running!");
                        OnGameLoopRunning?.Invoke();
                    }
                    else
                    {
                        logger.Information($"Game Loop is no longer running!");
                        OnGameLoopNotRunning?.Invoke();
                    }
                }

                Task.Delay(100).Wait();
            }
        }
    }
}
