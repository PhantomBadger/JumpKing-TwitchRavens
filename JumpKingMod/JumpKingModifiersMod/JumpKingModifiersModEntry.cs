using HarmonyLib;
using JumpKingModifiersMod.API;
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
using PBJKModBase.Twitch;
using PBJKModBase.Twitch.Settings;
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

                // Set up patching
                var playerValues = new PlayerValuesManualPatch(Logger);
                playerValues.SetUpManualPatch(harmony);
                var playerStatePatch = new PlayerStateObserverManualPatch(Logger);
                playerStatePatch.SetUpManualPatch(harmony);
                var jumpStatePatch = new JumpStateManualPatch(playerStatePatch, Logger);
                jumpStatePatch.SetUpManualPatch(harmony);
                var drawRenderTargetPatch = new DrawRenderTargetManualPatch();
                drawRenderTargetPatch.SetUpManualPatch(harmony);
                var windPatch = new WindObserverManualPatch(Logger);
                windPatch.SetUpManualPatch(harmony);
                var gravityPatch = new LowGravityObserverManualPatch(Logger);
                gravityPatch.SetUpManualPatch(harmony);
                var icePatch = new OnIceObserverManualPatch(Logger);
                icePatch.SetUpManualPatch(harmony);
                var drawForegroundPatch = new DrawPlatformsObserverManualPatch(Logger);
                drawForegroundPatch.SetUpManualPatch(harmony);

                // Make the Modifier Updating Entity
                var modifierUpdatingEntity = new ModifierUpdatingEntity(ModEntityManager.Instance, Logger);

                // Set up modifiers and trigger
                List<IModifier> availableModifiers = new List<IModifier>();
                var walkSpeedModifier = new WalkSpeedModifier(2f, playerValues, Logger);
                var bouncyFloorModifier = new BouncyFloorModifier(modifierUpdatingEntity, playerStatePatch, jumpStatePatch, Logger);
                var flipScreenModifier = new FlipScreenModifier(drawRenderTargetPatch, Logger);
                var invertControlsModifier = new InvertControlsModifier(playerStatePatch, Logger);
                var bombCountdownModifier = new BombCountdownModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, jumpStatePatch, Logger);
                var windModifier = new WindToggleModifier(windPatch, Logger);
                var lowVisibilityModifier = new LowVisibilityModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, Logger);
                var lowGravityModifier = new LowGravityModifier(gravityPatch, Logger);
                var iceModifier = new OnIceModifier(icePatch, Logger);
                var hideForegroundModifier = new HidePlatformsModifier(drawForegroundPatch, Logger);
                var screenShakeModifier = new ScreenShakeModifier(Logger);

                availableModifiers.Add(walkSpeedModifier);
                availableModifiers.Add(bouncyFloorModifier);
                availableModifiers.Add(flipScreenModifier);
                availableModifiers.Add(invertControlsModifier);
                availableModifiers.Add(windModifier);
                availableModifiers.Add(lowVisibilityModifier);
                availableModifiers.Add(lowGravityModifier);
                availableModifiers.Add(iceModifier);
                availableModifiers.Add(hideForegroundModifier);
                availableModifiers.Add(screenShakeModifier);

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

                // Rising Lava
                bool isRisingLavaEnabled = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.RisingLavaEnabledKey, false);
                if (isRisingLavaEnabled)
                {
                    var risingLavaModifier = new RisingLavaModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, GameStateObserverManualPatch.Instance, userSettings, Logger);
                    Keys risingLavaToggleKey = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.DebugTriggerLavaRisingToggleKeyKey, Keys.F7);

                    var togglePair = new DebugTogglePair(risingLavaModifier, risingLavaToggleKey);
                    debugToggles.Add(togglePair);
                    availableModifiers.Add(risingLavaModifier);
                    Logger.Information($"Rising Lava Mod is Enabled! Press the Toggle Key ({risingLavaToggleKey.ToString()}) to activate once in game!");
                }
                else
                {
                    Logger.Error($"Rising Lava Mod is disabled in the settings! Run the Installer.UI.exe and click 'Load Settings' to enable");
                }

                // Make twitch client factory
                var twitchSettings = new UserSettings(PBJKModBaseTwitchSettingsContext.SettingsFileName, PBJKModBaseTwitchSettingsContext.GetDefaultSettings(), Logger);
                var clientFactory = new TwitchClientFactory(twitchSettings, Logger);

                // Make the toggle trigger
                var twitchPollTrigger = new TwitchPollTrigger(clientFactory.GetTwitchClient(), availableModifiers, 
                    ModEntityManager.Instance, GameStateObserverManualPatch.Instance, Logger);
                var pollVisual = new TwitchPollVisual(ModEntityManager.Instance, twitchPollTrigger, GameStateObserverManualPatch.Instance, Logger);

                var debugTrigger = new DebugModifierTrigger(ModEntityManager.Instance, debugToggles, userSettings);
                debugTrigger.EnableTrigger();

                // Once the gamee is running then enable the twitch poll trigger
                Task.Run(() =>
                {
                    while (!GameStateObserverManualPatch.Instance.IsGameLoopRunning())
                    {
                        Task.Delay(500).Wait();
                    }

                    twitchPollTrigger.EnableTrigger();
                });

                // Make the modifier notification
                var modifierNotification = new ModifierToggleNotifications(ModEntityManager.Instance, new List<API.IModifierTrigger>() { debugTrigger, twitchPollTrigger }, Logger);
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Modifiers Init {e.ToString()}");
            }
            Logger.Information("Modifiers Init Called!");
        }
    }
}
