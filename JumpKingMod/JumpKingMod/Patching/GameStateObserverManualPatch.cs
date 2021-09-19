using HarmonyLib;
using JumpKingMod.API;
using JumpKingMod.Patching;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace JumpKingMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IGameStateObserver"/> which will
    /// keep track of the current state of the game, and allow others to query it
    /// </summary>
    public class GameStateObserverManualPatch : IManualPatch, IGameStateObserver
    {
        private readonly ILogger logger;
        private static bool isGameInitialized;
        private static object gameLoopObject;
        private static MethodInfo gameLoopIsRunningMethodInfo;

        public ManualResetEvent GameInitializedLatch
        {
            get
            {
                return gameInitializedLatch;
            }
        }
        private static ManualResetEvent gameInitializedLatch;

        /// <summary>
        /// Default ctor for creating a <see cref="GameStateObserverManualPatch"/>, initialises the states
        /// </summary>
        public GameStateObserverManualPatch(ILogger logger)
        {
            gameInitializedLatch = new ManualResetEvent(false);
            isGameInitialized = false;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        /// Sets up the manual patch to query the game states
        /// </summary>
        public void SetUpManualPatch(Harmony harmony)
        {
            try
            {
                var method = AccessTools.Method("JumpKing.JumpGame:MakeBT");
                var prefixMethod = AccessTools.Method($"JumpKingMod.Patching.{this.GetType().Name}:PostfixPatchMethod");
                harmony.Patch(method, postfix: new HarmonyMethod(prefixMethod));
            }
            catch (Exception e)
            {
                logger.Error(e.ToString());
            }
        }

        /// <summary>
        /// Called after the JumpGame ctor
        /// </summary>
        public static void PostfixPatchMethod(object __instance)
        {
            isGameInitialized = true;
            gameInitializedLatch?.Set();

            var gameLoopField = AccessTools.Field(__instance.GetType(), "m_game_loop");
            gameLoopObject = gameLoopField.GetValue(__instance);
            gameLoopIsRunningMethodInfo = gameLoopObject?.GetType().GetMethod("IsRunning");
        }
    }
}
