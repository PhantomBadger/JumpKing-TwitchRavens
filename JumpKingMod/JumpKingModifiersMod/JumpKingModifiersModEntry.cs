using HarmonyLib;
using JumpKingModifiersMod.Modifiers;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Triggers;
using Logging;
using Logging.API;
using Microsoft.Xna.Framework.Input;
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
                Logger.Information($"Jump King Modifiers Mod!");
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

                // Set up the wind patching
                var windPatch = new WindObserverManualPatch(Logger);
                windPatch.SetUpManualPatch(harmony);

                // Make the Modifier Updating Entity
                var modifierUpdatingEntity = new ModifierUpdatingEntity(ModEntityManager.Instance, Logger);

                // Set up modifiers and trigger
                // Not released yet ssshh....
                var walkSpeedModifier = new WalkSpeedModifier(2f, playerValues, Logger);
                var bouncyFloorModifier = new BouncyFloorModifier(modifierUpdatingEntity, playerStatePatch, jumpStatePatch, Logger);
                var flipScreenModifier = new FlipScreenModifier(drawRenderTargetPatch, Logger);
                var invertControlsModifier = new InvertControlsModifier(playerStatePatch, Logger);
                var bombCountdownModifier = new BombCountdownModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, jumpStatePatch, Logger);
                var windModifier = new WindToggleModifier(windPatch, Logger);

                List<DebugTogglePair> debugToggles = new List<DebugTogglePair>();
                debugToggles.Add(new DebugTogglePair(windModifier, Keys.OemPeriod));

                // Fall Damage
                bool isFallDamageEnabled = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.FallDamageEnabledKey, false);
                if (isFallDamageEnabled)
                {
                    var subtextGetter = new YouDiedSubtextFileGetter(Logger);
                    var fallDamageModifier = new FallDamageModifier(
                        modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, GameStateObserverManualPatch.Instance,
                        subtextGetter, userSettings, Logger);
                    Keys fallDamageToggleKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerFallDamageToggleKeyKey, Keys.F11);

                    var togglePair = new DebugTogglePair(fallDamageModifier, fallDamageToggleKey);
                    debugToggles.Add(togglePair);

                    Logger.Information($"Fall Damage Mod is Enabled! Press the Toggle Key ({fallDamageToggleKey.ToString()}) to activate once in game!");
                }
                else
                {
                    Logger.Error($"Fall Damage Mod is disabled in the settings! Run the Installer.UI.exe and click 'Load Settings' to enable");
                }

                // Manual Resizing
                bool isShrinkingEnabled = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeEnabledKey, false);
                if (isShrinkingEnabled)
                {
                    var manualResizeModifier = new ManualScreenResizeModifier(modifierUpdatingEntity, userSettings, Logger);
                    Keys manualResizeToggleKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerManualResizeToggleKey, Keys.F9);

                    var togglePair = new DebugTogglePair(manualResizeModifier, manualResizeToggleKey);
                    debugToggles.Add(togglePair);
                    Logger.Information($"Manual Resize Mod is Enabled! Press the Toggle Key ({manualResizeToggleKey.ToString()}) to activate once in game!");
                }
                else
                {
                    Logger.Error($"Manual Resize Mod is disabled in the settings! Run the Installer.UI.exe and click 'Load Settings' to enable");
                }

                // Make the toggle trigger
                var debugTrigger = new DebugModifierTrigger(ModEntityManager.Instance, debugToggles, userSettings);
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
