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
using JumpKingMod.Patching;
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

                // Free Fly Patch
                IManualPatch freeFlyPatch = new FreeFlyManualPatch(Logger);
                freeFlyPatch.SetUpManualPatch(harmony);
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Init {e.ToString()}");
            }

            Logger.Information("Init Called!");
        }
    }
}
