using HarmonyLib;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Modifiers;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
using JumpKingModifiersMod.Triggers;
using JumpKingModifiersMod.Triggers.Poll;
using JumpKingModifiersMod.Visuals;
using Logging;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using PBJKModBase;
using PBJKModBase.API;
using PBJKModBase.Entities;
using PBJKModBase.Patching;
using PBJKModBase.Streaming.Settings;
using PBJKModBase.Twitch;
using PBJKModBase.Twitch.Settings;
using PBJKModBase.YouTube;
using PBJKModBase.YouTube.API;
using PBJKModBase.YouTube.Settings;
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
                var icePatch = new OnIceObserverManualPatch(Logger);
                icePatch.SetUpManualPatch(harmony);
                var drawForegroundPatch = new DrawPlatformsObserverManualPatch(Logger);
                drawForegroundPatch.SetUpManualPatch(harmony);

                // Make the Modifier Updating Entity
                var modifierUpdatingEntity = new ModifierUpdatingEntity(ModEntityManager.Instance, Logger);

                // Load the settings for the modifiers and triggers
                string rawEnabledModifiers = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.EnabledModifiersKey, "");
                HashSet<string> enabledModifierTypes = JumpKingModifiersModSettingsContext.ParseEnabledModifiers(rawEnabledModifiers);

                string rawModifierToggles = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.ModifierToggleKeysKey, "");
                Dictionary<string, Keys> modifierToggles = JumpKingModifiersModSettingsContext.ParseToggleKeys(rawModifierToggles);

                ModifierTriggerTypes triggerType = userSettings.GetSettingOrDefault(JumpKingModifiersModSettingsContext.TriggerTypeKey, ModifierTriggerTypes.Toggle);

                // Make all the modifiers
                List<IModifier> allModifiers = new List<IModifier>();
                var subtextGetter = new YouDiedSubtextFileGetter(Logger);
                var fallDamageModifier = new FallDamageModifier(
                    modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, GameStateObserverManualPatch.Instance,
                    subtextGetter, userSettings, Logger);
                var walkSpeedModifier = new WalkSpeedModifier(WalkSpeedModifier.DefaultModifier, playerValues, Logger);
                var bouncyFloorModifier = new BouncyFloorModifier(modifierUpdatingEntity, playerStatePatch, jumpStatePatch, Logger);
                var flipScreenModifier = new FlipScreenModifier(drawRenderTargetPatch, Logger);
                var invertControlsModifier = new InvertControlsModifier(playerStatePatch, Logger);
                var windModifier = new WindToggleModifier(windPatch, Logger);
                var lowVisibilityModifier = new LowVisibilityModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, Logger);
                var iceModifier = new OnIceModifier(icePatch, Logger);
                var hideForegroundModifier = new HidePlatformsModifier(drawForegroundPatch, Logger);
                var screenShakeModifier = new ScreenShakeModifier(Logger);
                var jumpTimeModifier = new JumpTimeModifier(JumpTimeModifier.DefaultModifier, playerValues, Logger);
                var risingLavaModifier = new RisingLavaModifier(modifierUpdatingEntity, ModEntityManager.Instance, playerStatePatch, GameStateObserverManualPatch.Instance, userSettings, Logger);
                var pollTimeModifier = new QuickerPollMetaModifier(Logger);
                var durationModifier = new LongerDurationMetaModifier(Logger);

                // Add the modifiers to a master list
                allModifiers.Add(walkSpeedModifier);
                allModifiers.Add(bouncyFloorModifier);
                allModifiers.Add(flipScreenModifier);
                allModifiers.Add(invertControlsModifier);
                allModifiers.Add(windModifier);
                allModifiers.Add(lowVisibilityModifier);
                allModifiers.Add(iceModifier);
                allModifiers.Add(hideForegroundModifier);
                allModifiers.Add(screenShakeModifier);
                allModifiers.Add(jumpTimeModifier);
                allModifiers.Add(fallDamageModifier);
                allModifiers.Add(risingLavaModifier);
                allModifiers.Add(pollTimeModifier);
                allModifiers.Add(durationModifier);

                // Low gravity requires some JK+ stuff, so if we deal with it seperately
                // We attempt to patch it, if we fail then the JK+ methods dont exist and we can't use it
                try
                {
                    var gravityPatch = new LowGravityObserverManualPatch(Logger);
                    gravityPatch.SetUpManualPatch(harmony);

                    var lowGravityModifier = new LowGravityModifier(gravityPatch, Logger);
                    allModifiers.Add(lowGravityModifier);
                }
                catch (Exception e)
                {
                    Logger.Warning($"The 'Low Gravity' Modifier requires JK+! It will not be used!");
                }

                // Find which modifiers have been enabled in the settings
                List<IModifier> triggerableModifiers = allModifiers.Where((IModifier modifier) => 
                enabledModifierTypes.Contains(modifier.GetType().ToString())).ToList();

                // Based on the selected trigger type initialise the trigger and add the triggerable modifiers
                IModifierTrigger modifierTrigger = null;
                switch (triggerType)
                {
                    case ModifierTriggerTypes.Toggle:
                        {
                            List<DebugTogglePair> debugToggles = new List<DebugTogglePair>();
                            for (int i = 0; i < triggerableModifiers.Count; i++)
                            {
                                string typeName = triggerableModifiers[i].GetType().ToString();
                                if (modifierToggles.ContainsKey(typeName))
                                {
                                    debugToggles.Add(new DebugTogglePair(triggerableModifiers[i], modifierToggles[typeName]));
                                }
                            }
                            modifierTrigger = new DebugModifierTrigger(ModEntityManager.Instance, debugToggles, userSettings, Logger);
                            break;
                        }
                    case ModifierTriggerTypes.ChatPoll:
                        {
                            IPollChatProvider chatProvider = null;

                            // Make twitch client factory
                            var streamSettings = new UserSettings(PBJKModBaseStreamingSettingsContext.SettingsFileName, PBJKModBaseStreamingSettingsContext.GetDefaultSettings(), Logger);
                            AvailableStreamingPlatforms selectedPlatform = streamSettings.GetSettingOrDefault(PBJKModBaseStreamingSettingsContext.SelectedStreamingPlatformKey, AvailableStreamingPlatforms.Twitch);
                            
                            switch (selectedPlatform)
                            {
                                case AvailableStreamingPlatforms.Twitch:
                                    var twitchSettings = new UserSettings(PBJKModBaseTwitchSettingsContext.SettingsFileName, PBJKModBaseTwitchSettingsContext.GetDefaultSettings(), Logger);
                                    var twitchClientFactory = new TwitchClientFactory(twitchSettings, Logger);

                                    chatProvider = new TwitchPollChatProvider(twitchClientFactory.GetTwitchClient(), Logger);
                                    break;
                                case AvailableStreamingPlatforms.YouTube:
                                    var youTubeSettings = new UserSettings(PBJKModBaseYouTubeSettingsContext.SettingsFileName, PBJKModBaseYouTubeSettingsContext.GetDefaultSettings(), Logger);
                                    var youTubeClientFactory = new YouTubeChatClientFactory(youTubeSettings, Logger);
                                    YouTubeChatClient youTubeClient = youTubeClientFactory.GetYouTubeClient();

                                    // YouTube Clients require us to prompt the user to connect due to the budget limits
                                    // present in the API (A connection request costs X 'tokens' and there's a limit to how
                                    // many tokens we have in a given period, making it difficult for us to automatically poll)
                                    
                                    // TODO: Deal with the YouTube connect text overlapping with the poll text
                                    IYouTubeClientConnector clientController = new ManualYouTubeClientConnector(youTubeClient, ModEntityManager.Instance, youTubeSettings, Logger);
                                    clientController.StartAttemptingConnection();

                                    chatProvider = new YouTubePollChatProvider(youTubeClient, Logger);
                                    break;
                                default:
                                    throw new NotImplementedException($"Unknown Streaming Platform Provided! Modifiers Mod does not know how to hanle {selectedPlatform.ToString()}");
                            }
                            
                            // Make the poll trigger and visual
                            PollTrigger pollTrigger = new PollTrigger(chatProvider, triggerableModifiers,
                                ModEntityManager.Instance, GameStateObserverManualPatch.Instance, userSettings, Logger);

                            PollVisual pollVisual = new PollVisual(ModEntityManager.Instance, pollTrigger,
                                GameStateObserverManualPatch.Instance, Logger);

                            // Add a reference to the meta modifiers - a bit jank doing it this way but heyo
                            pollTimeModifier.PollTrigger = pollTrigger;
                            durationModifier.PollTrigger = pollTrigger;

                            modifierTrigger = pollTrigger;
                            break;
                        }
                    case ModifierTriggerTypes.None:
                        {
                            // No trigger
                            modifierTrigger = null;
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException($"No implementation for '{triggerType.ToString()}' Trigger Type");
                        }
                }

                // Manual Resizing - Disabled, only used by Rainhoe for a one-off
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

                // Once the gameeis running then enable the twitch poll trigger
                if (modifierTrigger != null)
                {
                    Task.Run(() =>
                    {
                        while (!GameStateObserverManualPatch.Instance.IsGameLoopRunning())
                        {
                            Task.Delay(500).Wait();
                        }

                        modifierTrigger.EnableTrigger();
                    });

                    // Make the modifier notification visual
                    // this will display when a modifier is enabled or disabled
                    var modifierNotification = new ModifierToggleNotifications(ModEntityManager.Instance, new List<API.IModifierTrigger>() { modifierTrigger }, Logger);
                }
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Modifiers Init {e.ToString()}");
            }
            Logger.Information("Modifiers Init Called!");
        }
    }
}
