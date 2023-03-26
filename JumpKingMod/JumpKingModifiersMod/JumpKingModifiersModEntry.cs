using HarmonyLib;
using JumpKingModifiersMod.Modifiers;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Triggers;
using Logging;
using Logging.API;
using PBJKModBase;
using PBJKModBase.API;
using PBJKModBase.Entities;
using PBJKModBase.Patching;
using Settings;
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

                // Load content & settings
                var userSettings = new UserSettings(JumpKingModifiersModSettingsContext.SettingsFileName, JumpKingModifiersModSettingsContext.GetDefaultSettings(), Logger);
                ModifiersModContentManager.LoadContent(Logger);

                // Set up player values patching
                var playerValues = new PlayerValuesManualPatch(Logger);
                playerValues.SetUpManualPatch(harmony);

                // Set up player state patching
                var playerStatePatch = new PlayerStateObserverManualPatch(Logger);
                playerStatePatch.SetUpManualPatch(harmony);

                // Set up jump state patching
                var jumpStatePatch = new JumpStateManualPatch(playerStatePatch, Logger);
                jumpStatePatch.SetUpManualPatch(harmony);

                // Set up the game rect patching
                var drawRenderTargetPatch = new DrawRenderTargetManualPatch();
                drawRenderTargetPatch.SetUpManualPatch(harmony);

                // Make the Modifier Updating Entity
                var modifierUpdatingEntity = new ModifierUpdatingEntity(ModEntityManager.Instance, Logger);

                // Set up modifiers and trigger
                var walkSpeedModifier = new WalkSpeedModifier(2f, playerValues, Logger);
                var bouncyFloorModifier = new BouncyFloorModifier(modifierUpdatingEntity, playerStatePatch, jumpStatePatch, Logger);
                var flipScreenModifier = new FlipScreenModifier(drawRenderTargetPatch, Logger);

                var subtextGetter = new YouDiedSubtextFileGetter(Logger);
                var fallDamageModifier = new FallDamageModifier(
                    modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, GameStateObserverManualPatch.Instance, 
                    subtextGetter, userSettings, Logger);

                var debugTrigger = new DebugModifierTrigger(ModEntityManager.Instance, flipScreenModifier, userSettings);
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
