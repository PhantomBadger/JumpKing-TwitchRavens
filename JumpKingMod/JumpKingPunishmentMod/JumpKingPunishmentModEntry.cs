using System;
using System.Threading.Tasks;
using HarmonyLib;
using JumpKingPunishmentMod.Devices;
using JumpKingPunishmentMod.Entities;
using JumpKingPunishmentMod.Patching;
using JumpKingPunishmentMod.API;
using JumpKingPunishmentMod.Settings;
using Logging;
using Logging.API;
using PBJKModBase;
using PBJKModBase.Patching;
using PBJKModBase.Entities;
using PBJKModBase.FeedbackDevice.Settings;
using PBJKModBase.PiShock;
using PBJKModBase.PiShock.Settings;
using Settings;

namespace JumpKingPunishmentMod
{
    [JumpKingMod("Jump King Punishment", "Init")]
    public class JumpKingPunishmentModEntry
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
                var harmony = new Harmony("com.phantombadger.punishmentmod");
                harmony.PatchAll();

                Logger.Information($"====================================");
                Logger.Information($"Punishment Mod Loaded!");
                Logger.Information($"Written by Zarradeth, huge thanks to PhantomBadger as he laid much of the groundwork for this!");
                Logger.Information($"====================================");

                // Load Settings
                var punishmentSettings = new UserSettings(JumpKingPunishmentModSettingsContext.SettingsFileName, JumpKingPunishmentModSettingsContext.GetDefaultSettings(), Logger);
                var deviceSettings = new UserSettings(PBJKModBaseFeedbackDeviceSettingsContext.SettingsFileName, PBJKModBaseFeedbackDeviceSettingsContext.GetDefaultSettings(), Logger);

                 // Get the observer
                var gameStateObserver = GameStateObserverManualPatch.Instance;

                // Create the feedback device now so that we can run a test to let the user know it's working ASAP
                // (also we can make it so we only do the init if we actually have a configured device)
                bool punishmentModEnabled = punishmentSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentModEnabledKey, false);
                
                IPunishmentDevice punishmentDevice = null;
                if (punishmentModEnabled)
                {
                    AvailableFeedbackDevices selectedDevice = deviceSettings.GetSettingOrDefault(PBJKModBaseFeedbackDeviceSettingsContext.SelectedFeedbackDeviceKey, AvailableFeedbackDevices.None);
                    if (selectedDevice == AvailableFeedbackDevices.PiShock)
                    {
                        Logger.Information("Initializing a PiShock device for the punishment mod...");
                        var piShockSettings = new UserSettings(PBJKModBasePiShockSettingsContext.SettingsFileName, PBJKModBasePiShockSettingsContext.GetDefaultSettings(), Logger);
                        var piShockDeviceFactory = new PiShockDeviceFactory(piShockSettings, Logger);

                        punishmentDevice = new PiShockPunishmentDevice(piShockDeviceFactory.GetPiShockDevice(), Logger);
                    }
                    else
                    {
                        Logger.Information("No feedback device is configured, the punishment mod will not run!");
                    }
                }

                if (punishmentModEnabled && (punishmentDevice != null))
                {
                    // Send a test feedback event so the user can know the mod is working
                    punishmentDevice.Test(50.0f, 1.0f);

                    var playerStatePatch = new PunishmentPlayerStateObserverManualPatch();
                    playerStatePatch.SetUpManualPatch(harmony);

                    // Run the rest after the game loop has started
                    Task.Run(() =>
                    {
                        try
                        {
                            while (!gameStateObserver.IsGameLoopRunning())
                            {
                                Task.Delay(100).Wait();
                            }

                            Logger.Information("Initializing Punishment Mod Manager Entity");
                            PunishmentManagerEntity managerEntity = new PunishmentManagerEntity(punishmentSettings, ModEntityManager.Instance, punishmentDevice, playerStatePatch, isGameLoopRunning: true, Logger);

                            gameStateObserver.OnGameLoopRunning += managerEntity.OnGameLoopStarted;
                            gameStateObserver.OnGameLoopNotRunning += managerEntity.OnGameLoopStopped;
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Error on Punishment Mod Post-GameLoop Init: {e.ToString()}");
                        }
                    });
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Punishment Mod Init {e.ToString()}");
            }

            Logger.Information("Punishment Mod Init Called!");
        }
    }
}
