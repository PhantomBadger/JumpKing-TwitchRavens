using HarmonyLib;
using Logging;
using Logging.API;
using PBJKModBase.Entities;
using PBJKModBase.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase
{
    /// <summary>
    /// The entry point for the PBJK Mod Base, sets up the baseline patching
    /// </summary>
    public class PBJKModBaseEntry
    {
        /// <summary>
        /// An implementation of <see cref="ILogger"/> to log to
        /// </summary>
        public static ILogger Logger;

        /// <summary>
        /// The entry point method for the PBJK Mod Base
        /// </summary>
        public static void Init()
        {
            Logger = ConsoleLogger.Instance;
            try
            {
                var harmony = new Harmony("com.phantombadger.pbjkmodbase");
                harmony.PatchAll();

                // Make our Mod Entity Manager and patch it
                var modEntityManagerPatch = new ModEntityManagerManualPatch(ModEntityManager.Instance);
                modEntityManagerPatch.SetUpManualPatch(harmony);
            }
            catch (Exception e)
            {
                Logger.Error($"Error on PBJKModBase Init {e.ToString()}");
            }
            Logger.Information("PBJKModBase Init Called!");
        }
    }
}
