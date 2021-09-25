using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HarmonyLib;
using JumpKingMod.Entities;
using JumpKingMod.Patching;
using JumpKingMod.Twitch;
using Logging;
using Logging.API;

namespace JumpKingMod
{
    public class JumpKingModEntry
    {
        public static ILogger Logger;

        /// <summary>
        /// Entry point for our mod, initiates all of the patching
        /// </summary>
        public static void Init()
        {
            Logger = new ConsoleLogger();

            try
            {
                Harmony harmony = new Harmony("com.phantombadger.jumpkingmod");
                harmony.PatchAll();

                // Set up observer
                GameStateObserverManualPatch gameStateObserver = new GameStateObserverManualPatch(Logger);
                gameStateObserver.SetUpManualPatch(harmony);

                // Make our Mod Entity Manager and patch it
                ModEntityManager modEntityManager = new ModEntityManager();
                IManualPatch modEntityManagerPatch = new ModEntityManagerManualPatch(modEntityManager);
                modEntityManagerPatch.SetUpManualPatch(harmony);

                // Free Fly Patch
                IManualPatch freeFlyPatch = new FreeFlyManualPatch(modEntityManager, Logger);
                freeFlyPatch.SetUpManualPatch(harmony);

                // Twitch Chat
                TwitchChatRelay relay = new TwitchChatRelay(modEntityManager, gameStateObserver, Logger);

                Task.Run(() =>
                {
                    while (!gameStateObserver.IsGameLoopRunning())
                    {
                        Task.Delay(100).Wait();
                    }

                    // Ravens
                    RavenDebugSpawningEntity ravenSpawner = new RavenDebugSpawningEntity(modEntityManager, Logger);
                });
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Init {e.ToString()}");
            }

            Logger.Information("Init Called!");
        }
    }
}
