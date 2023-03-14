using HarmonyLib;
using JumpKingModifiersMod.Modifiers;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Triggers;
using Logging;
using Logging.API;
using PBJKModBase;
using PBJKModBase.API;
using PBJKModBase.Entities;
using PBJKModBase.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod
{
    [JumpKingMod("Jump King Modifiers", "Init")]
    public class JumpKingModifiersModEntry
    {
        public static ILogger Logger;

        /// <summary>
        /// Entry point for our mod, initiates all of the patching
        /// </summary>
        public static void Init()
        {
            Logger = ConsoleLogger.Instance;
            try
            {
                var harmony = new Harmony("com.phantombadger.jumpkingmodifiersmod");
                harmony.PatchAll();

                Logger.Information($"====================================");
                Logger.Information($"Jump King Modifiers Pre-Release!");
                Logger.Information($"====================================");

                var playerValues = new PlayerValuesManualPatch(Logger);
                playerValues.SetUpManualPatch(harmony);

                // Make our Mod Entity Manager and patch it
                var modEntityManagerPatch = new ModEntityManagerManualPatch(ModEntityManager.Instance);
                modEntityManagerPatch.SetUpManualPatch(harmony);

                // Set up player state patching
                var playerStatePatch = new PlayerStateObserverManualPatch(Logger);
                playerStatePatch.SetUpManualPatch(harmony);

                // Set up jump state patching
                var jumpStatePatch = new JumpStateManualPatch(playerStatePatch, Logger);
                jumpStatePatch.SetUpManualPatch(harmony);

                // Set up modifiers and trigger
                var walkSpeedModifier = new WalkSpeedModifier(2f, playerValues, Logger);
                var bouncyFloorModifier = new BouncyFloorModifier(playerStatePatch, jumpStatePatch, Logger);
                var debugTrigger = new DebugModifierTrigger(ModEntityManager.Instance, bouncyFloorModifier);
                debugTrigger.EnableTrigger();
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Modifiers Init {e.ToString()}");
            }
            Logger.Information("Modifiers Init Called!");
        }
    }
}
