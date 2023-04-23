using HarmonyLib;
using JumpKingModifiersMod.Modifiers;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Triggers;
using JumpKingModifiersMod.Visuals;
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
                Logger.Information($"Jump King Fall Damage Mod!");
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
                // Look if you know how to compile code you can clearly enable all these. I ask that you don't
                // because I don't wanna release it yet, but I also dont wanna go through the faff of partially 
                // bringing stuff into a public branch. Please be a homie ty xoxox

                //var walkSpeedModifier = new WalkSpeedModifier(2f, playerValues, Logger);
                //var bouncyFloorModifier = new BouncyFloorModifier(modifierUpdatingEntity, playerStatePatch, jumpStatePatch, Logger);
                //var flipScreenModifier = new FlipScreenModifier(drawRenderTargetPatch, Logger);
                //var invertControlsModifier = new InvertControlsModifier(playerStatePatch, Logger);
                //var bombCountdownModifier = new BombCountdownModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, jumpStatePatch, Logger);
                //var windModifier = new WindToggleModifier(windPatch, Logger);
                //var lowVisibilityModifier = new LowVisibilityModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, Logger);

                List<DebugTogglePair> debugToggles = new List<DebugTogglePair>();
                //debugToggles.Add(new DebugTogglePair(risingLavaModifier, Keys.OemPeriod));

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

                //// Manual Resizing
                //bool isShrinkingEnabled = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ManualResizeEnabledKey, false);
                //if (isShrinkingEnabled)
                //{
                //    var manualResizeModifier = new ManualScreenResizeModifier(modifierUpdatingEntity, userSettings, Logger);
                //    Keys manualResizeToggleKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerManualResizeToggleKey, Keys.F9);

                //    var togglePair = new DebugTogglePair(manualResizeModifier, manualResizeToggleKey);
                //    debugToggles.Add(togglePair);
                //    Logger.Information($"Manual Resize Mod is Enabled! Press the Toggle Key ({manualResizeToggleKey.ToString()}) to activate once in game!");
                //}
                //else
                //{
                //    Logger.Error($"Manual Resize Mod is disabled in the settings! Run the Installer.UI.exe and click 'Load Settings' to enable");
                //}

                //// Rising Lava
                //bool isRisingLavaEnabled = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaEnabledKey, false);
                //if (isRisingLavaEnabled)
                //{
                //    var risingLavaModifier = new RisingLavaModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, GameStateObserverManualPatch.Instance, userSettings, Logger);
                //    Keys risingLavaToggleKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerLavaRisingToggleKeyKey, Keys.F7);

                //    var togglePair = new DebugTogglePair(risingLavaModifier, risingLavaToggleKey);
                //    debugToggles.Add(togglePair);
                //    Logger.Information($"Rising Lava Mod is Enabled! Press the Toggle Key ({risingLavaToggleKey.ToString()}) to activate once in game!");
                //}
                //else
                //{
                //    Logger.Error($"Rising Lava Mod is disabled in the settings! Run the Installer.UI.exe and click 'Load Settings' to enable");
                //}

                // Make the toggle trigger
                var debugTrigger = new DebugModifierTrigger(ModEntityManager.Instance, debugToggles, userSettings);
                debugTrigger.EnableTrigger();

                // Make the modifier notification
                var modifierNotification = new ModifierNotifications(ModEntityManager.Instance, new List<API.IModifierTrigger>() { debugTrigger }, Logger);
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Modifiers Init {e.ToString()}");
            }
            Logger.Information("Modifiers Init Called!");
        }
    }
}
