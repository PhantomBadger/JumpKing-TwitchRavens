using HarmonyLib;
using JumpKing;
using JumpKingPunishmentMod.Patching;
using JumpKingPunishmentMod.Patching.States;
using JumpKingPunishmentMod.Settings;
using JumpKingPunishmentMod.API;
using Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace JumpKingPunishmentMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> and <see cref="IDisposable"/> which keeps track of player state
    /// in order to accomplish most mod functionality with providing rewards/punishments/on screen messages/etc.
    /// </summary>
    public class PunishmentManagerEntity : IModEntity, IDisposable
    {
        private readonly UserSettings userSettings;
        private readonly ModEntityManager modEntityManager;
        private readonly ILogger logger;
        private PunishmentPlayerStateObserverManualPatch playerStateObserver;
        private readonly IPunishmentDevice punishmentDevice;

        private bool isGameLoopRunning;

        private readonly MethodInfo getPlayerValuesJumpMethod;
        private readonly MethodInfo getPlayerValuesMaxFallMethod;

        private bool isInAir;
        private float lastGroundedY;
        private float highestGroundY;
        private float preResetHighestGroundY;
        private float lastPlayerY;
        private float teleportCompensation;
        private float feedbackPauseTimer;

        private const float LastActionDisplayTime = 3.0f;
        private const float TeleportPauseTime = 0.1f;
        private const float TeleportDetectionMaxExpectedVelocityMultipler = 1.25f;

        private float lastActionDrawTimer;
        private UITextEntity incomingPunishmentTextEntity;
        private UITextEntity lastActionTextEntity;

        private bool debugToggledOff;
        private bool wasToggleHeld;
        private bool wasTestHeld;

        private readonly Keys toggleKey;
        private readonly Keys testKey;
        private readonly PunishmentOnScreenDisplayBehavior onScreenDisplayBehavior;
        private readonly bool roundDurations;

        private readonly bool enablePunishment;
        private readonly float minPunishmentDuration;
        private readonly float minPunishmentIntensity;
        private readonly float maxPunishmentDuration;
        private readonly float maxPunishmentIntensity;
        private readonly float minFallDistance;
        private readonly float maxFallDistance;
        private readonly bool easyModePunishment;

        private readonly bool enableRewards;
        private readonly float minRewardDuration;
        private readonly float minRewardIntensity;
        private readonly float maxRewardDuration;
        private readonly float maxRewardIntensity;
        private readonly float minRewardDistance;
        private readonly float maxRewardDistance;
        private readonly bool progressOnlyRewards;

        /// <summary>
        /// Ctor for creating a <see cref="PunishmentManagerEntity"/>
        /// </summary>
        public PunishmentManagerEntity(UserSettings userSettings, ModEntityManager modEntityManager, IPunishmentDevice punishmentDevice, PunishmentPlayerStateObserverManualPatch playerStateObserver, bool isGameLoopRunning, ILogger logger)
        {
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.punishmentDevice = punishmentDevice ?? throw new ArgumentNullException(nameof(punishmentDevice));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentNullException(nameof(playerStateObserver));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.isGameLoopRunning = isGameLoopRunning;

            getPlayerValuesJumpMethod = AccessTools.Method("JumpKing.PlayerValues:get_JUMP")
                ?? throw new InvalidOperationException($"Cannot find 'JumpKing.PlayerValues:get_JUMP' method in Jump King");
            getPlayerValuesMaxFallMethod = AccessTools.Method("JumpKing.PlayerValues:get_MAX_FALL")
                ?? throw new InvalidOperationException($"Cannot find 'JumpKing.PlayerValues:get_MAX_FALL' method in Jump King");

            ResetState();

            incomingPunishmentTextEntity = new UITextEntity(modEntityManager, new Vector2(240, 16), "", new Color(1.0f, 1.0f, 1.0f, 0.5f), UIEntityAnchor.Center, JKContentManager.Font.StyleFont, zOrder: 10);
            lastActionTextEntity = new UITextEntity(modEntityManager, new Vector2(240, 360), "", Color.White, UIEntityAnchor.Center, JKContentManager.Font.StyleFont, zOrder: 10);
            lastActionDrawTimer = 0.0f;

            debugToggledOff = false;
            wasToggleHeld = false;
            wasTestHeld = false;

            playerStateObserver.OnPlayerYTeleported += OnPlayerYTeleported;
            playerStateObserver.OnPlayerSaveStateApplying += OnPlayerSaveStateApplying;

            // Cache our settings off so we don't need to read them every time
            toggleKey = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentModToggleKeyKey, Keys.F8);
            testKey = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentFeedbackTestKeyKey, Keys.F9);
            onScreenDisplayBehavior = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.OnScreenDisplayBehaviorKey, PunishmentOnScreenDisplayBehavior.None);
            roundDurations = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.RoundDurationsKey, false);

            enablePunishment = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.EnablePunishmentKey, false);
            minPunishmentDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentDurationKey, 1.0f);
            minPunishmentIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentIntensityKey, 1.0f);
            maxPunishmentDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentDurationKey, 1.0f);
            maxPunishmentIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentIntensityKey, 1.0f);
            minFallDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentFallDistanceKey, 0.0f);
            maxFallDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentfallDistanceKey, 0.0f);
            easyModePunishment = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentEasyModeKey, false);

            enableRewards = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.EnableRewardsKey, false);
            minRewardDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardDurationKey, 1.0f);
            minRewardIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardIntensityKey, 1.0f);
            maxRewardDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardDurationKey, 1.0f);
            maxRewardIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardIntenityKey, 1.0f);
            minRewardDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardProgressDistanceKey, 0.0f);
            maxRewardDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardProgressDistanceKey, 0.0f);
            progressOnlyRewards = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.RewardProgressOnlyKey, false);

            // Some validation on settings here so we don't have to worry about it below
            maxPunishmentDuration = Math.Max(minPunishmentDuration, maxPunishmentDuration);
            maxPunishmentIntensity = Math.Max(minPunishmentIntensity, maxPunishmentIntensity);
            maxFallDistance = Math.Max(minFallDistance, maxFallDistance);
            maxRewardDuration = Math.Max(minRewardDuration, maxRewardDuration);
            maxRewardIntensity = Math.Max(minRewardIntensity, maxRewardIntensity);
            maxRewardDistance = Math.Max(minRewardDistance, maxRewardDistance);

            modEntityManager.AddEntity(this, 0);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            incomingPunishmentTextEntity?.Dispose();
            incomingPunishmentTextEntity = null;

            lastActionTextEntity?.Dispose();
            lastActionTextEntity = null;

            playerStateObserver.OnPlayerYTeleported -= OnPlayerYTeleported;
            playerStateObserver.OnPlayerSaveStateApplying -= OnPlayerSaveStateApplying;

            modEntityManager?.RemoveEntity(this);
        }

        /// <summary>
        /// Called each frame by the Mod Entity Manager, handles the main logic for updating the mod
        /// </summary>
        public void Update(float delta)
        {
            try
            {
                feedbackPauseTimer = Math.Max(0.0f, feedbackPauseTimer - delta);

                punishmentDevice.Update(delta);

                UpdateInput();

                PunishmentPlayerState playerState = playerStateObserver.GetPlayerState();
                if (IsFeedbackEnabled())
                {
                    if (!UpdateTeleportDetection(delta, playerState))
                    {
                        // We only need to do normal feedback updates if we didn't detect a teleport as teleports reset state
                        CheckAndTriggerFeedback(playerState);
                    }
                }

                UpdateOnScreenText(delta, playerState);
            }
            catch (Exception e)
            {
                logger.Error($"Error updating Punishment Manager {e.ToString()}");
            }
        }

        /// <summary>
        /// Called each frame by <see cref="PunishmentManagerEntity.Update"/>
        /// Handles updating debug/keyboard input
        /// </summary>
        private void UpdateInput()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool toggleHeld = keyboardState.IsKeyDown(toggleKey);
            if (toggleHeld && !wasToggleHeld)
            {
                debugToggledOff = !debugToggledOff;
                if (debugToggledOff)
                {
                    logger.Information("Toggling Punishment mod off...");
                }
                else
                {
                    logger.Information("Toggling Punishment mod back on!");
                    ResetState(rememberProgress: true);
                }
            }
            wasToggleHeld = toggleHeld;

            bool testHeld = keyboardState.IsKeyDown(testKey);
            if (testHeld && !wasTestHeld)
            {
                logger.Information("Sending test feedback to your feedback device...");
                punishmentDevice.Test(50.0f, 1.0f);
            }
            wasTestHeld = testHeld;
        }

        /// <summary>
        /// Called each frame by <see cref="PunishmentManagerEntity.Update"/>
        /// Handles updating teleport detection and handling feedback when a teleport is detected
        /// </summary>
        private bool UpdateTeleportDetection(float delta, PunishmentPlayerState playerState)
        {
            if (playerState == null)
            {
                return false;
            }

            float yLocation = playerState.Position.Y + teleportCompensation;

            bool teleportDetected = false;
            if (!float.IsNaN(lastPlayerY))
            {
                // We don't need to divide out delta here as Jump King doesn't apply velocities based on delta time (the current velocity
                // is just added to the player position each frame- entities in Jump King update with a fixed time step).
                // MAX_FALL and JUMP from PlayerValues are the same- they are fixed numbers that are never multiplied with DT when they
                // influence the player's velocity.
                float lastYVelocity = (yLocation - lastPlayerY);

                // Values to check against to see if the player has exceeded what should be the max possible velocity in a single frame.
                // TeleportDetectionMaxExpectedVelocityMultipler exists to help prevent false positives as well as potentially allow
                // some wiggle room (the value may need to be adjusted) if mods ever do weird things with launching the player or something
                float maxExpectedNegativeVelocity = GetPlayerValuesJUMP() * TeleportDetectionMaxExpectedVelocityMultipler;
                float maxExpectedPositiveVelocity = GetPlayerValuesMAX_FALL() * TeleportDetectionMaxExpectedVelocityMultipler;

                teleportDetected = (lastYVelocity > 0.0f) ? (lastYVelocity > maxExpectedPositiveVelocity) : (lastYVelocity < maxExpectedNegativeVelocity);
            }

            // If we detected a teleport trigger a punishment from before the teleport (if needed) and then reset state
            // like we do for OnPlayerSaveStateApplying
            if (teleportDetected)
            {
                logger.Information("The punishment mod detected a potential teleport- triggering punishments now and resetting state!");
                // Trigger feedback (so they can't get out of it) from the point they were at before the teleport
                CheckAndTriggerFeedback(playerState, lastPlayerY, true);
                // Reset state since we don't want to carry any positioning/movement/feedback/etc through the teleport
                ResetState(TeleportPauseTime, true);
                // No need to update lastPlayerY as it was cleared in ResetState and will be updated again on future ticks
            }
            else
            {
                lastPlayerY = yLocation;
            }

            return teleportDetected;
        }

        /// <summary>
        /// Called each frame by <see cref="PunishmentManagerEntity.Update"/>
        /// Updates on screen text for punishment actions and incoming punishment
        /// </summary>
        void UpdateOnScreenText(float delta, PunishmentPlayerState playerState)
        {
            // Fade the last action text out overtime (over the second half of it's lifetime)
            lastActionDrawTimer = Math.Max(0.0f, lastActionDrawTimer - delta);
            float Alpha = 1.0f;
            if (lastActionDrawTimer < (LastActionDisplayTime / 2.0f))
            {
                Alpha = lastActionDrawTimer / (LastActionDisplayTime / 2.0f);
            }
            lastActionTextEntity.TextColor = new Color(lastActionTextEntity.TextColor, Alpha);

            // Update the incoming text entity
            // Only show punishments incoming as rewards will be weird with the arcs of a jump (and punishments
            // generally can't be avoided once they start)
            var incomingPunishment = (false, 0.0f, 0.0f, 0.0f);
            if (IsFeedbackEnabled() && (playerState != null) && isInAir && !float.IsNaN(lastGroundedY))
            {
                float currentYDelta = (playerState.Position.Y + teleportCompensation) - lastGroundedY;
                if (currentYDelta > 0.0f)
                {
                    incomingPunishment = CalculatePunishment(currentYDelta, false);
                }
            }

            if (!incomingPunishment.Item1)
            {
                incomingPunishmentTextEntity.TextValue = "";
            }
            else
            {
                incomingPunishmentTextEntity.TextValue = GenerateFeedbackInfoString("Incoming punishment", incomingPunishment.Item2, incomingPunishment.Item3, incomingPunishment.Item4, "...");
            }
        }

        /// <summary>
        /// Checks and updates state to see if feedback should be triggered, and triggers it if needed
        /// Called each frame from <see cref="PunishmentManagerEntity.Update"/>
        /// Also called manually when teleports are detected/handled to immediately trigger feedback
        /// </summary>
        private void CheckAndTriggerFeedback(PunishmentPlayerState providedPlayerState = null, float overridePlayerY = float.NaN, bool forcePunishment = false)
        {
            PunishmentPlayerState playerState = (providedPlayerState != null) ? providedPlayerState : playerStateObserver.GetPlayerState();
            if (playerState == null)
            {
                return;
            }

            // Consider sand ground too for our purposes
            bool isOnGround = playerState.IsOnGround || playerState.IsOnSand;
            if (isOnGround || forcePunishment)
            {
                float yLocation = !float.IsNaN(overridePlayerY) ? overridePlayerY : (playerState.Position.Y + teleportCompensation);
                if (isInAir && !float.IsNaN(lastGroundedY))
                {
                    // We have landed, calculate and execute a shock/reward
                    // Note, Y DECREASES as you move upward
                    float yDelta = yLocation - lastGroundedY;
                    
                    // Positive progress- we only want to trigger positive progress if you are actually on the ground
                    // as forced rewards should only really be possible if the player is mid jump when something happens
                    // and triggering a reward would be incorrect/potentially problematic in that case
                    if ((yDelta < 0.0f) && isOnGround && !float.IsNaN(highestGroundY))
                    {
                        if (progressOnlyRewards)
                        {
                            yDelta = yLocation - highestGroundY;
                        }

                        // Make sure we actually made progress in the case of progress only rewards
                        if (yDelta < 0.0f)
                        {
                            var reward = CalculateReward(yDelta);
                            if (reward.Item1)
                            {
                                punishmentDevice.Reward(reward.Item3, reward.Item4);
                                UpdateLastAction(GenerateFeedbackInfoString("Reward!", reward.Item2, reward.Item3, reward.Item4), Color.Lime);
                            }
                        }
                    }
                    else if (yDelta > 0.0f)  // Negative progress
                    {
                        var punishment = CalculatePunishment(yDelta);
                        if (punishment.Item1)
                        {
                            punishmentDevice.Punish(punishment.Item3, punishment.Item4, easyModePunishment);
                            UpdateLastAction(GenerateFeedbackInfoString("Punishment!", punishment.Item2, punishment.Item3, punishment.Item4), easyModePunishment ? Color.Lime : Color.Red);
                        }
                    }
                }
                // This can short the player rewards if they are making positive progress when we try to
                // force a punishment, but that generally shouldn't happen and isn't a huge deal if it does
                lastGroundedY = yLocation;
                if (isOnGround)     // Don't update highest grounded if they aren't actually on the ground
                {
                    if (float.IsNaN(highestGroundY))
                    {
                        // When first updating highestGroundY use preResetHighestGroundY if we have it set and it's further progress.
                        // This works in tandem with ResetState() calls to prevent the player from being rewarded for progress from stuff
                        // like teleports or toggling the mod off and on- but we also won't reset/lower the furthest progress in the case
                        // of teleporting downward or more importantly warping for something like a modifier death.
                        // preResetHighestGroundY will not be set when returning to the main menu so it won't carry through different runs.
                        highestGroundY = float.IsNaN(preResetHighestGroundY) ? yLocation : Math.Min(yLocation, preResetHighestGroundY);
                        preResetHighestGroundY = float.NaN;
                    }
                    else
                    {
                        highestGroundY = Math.Min(yLocation, highestGroundY);
                    }
                }
            }
            isInAir = !isOnGround;
        }

        /// <summary>
        /// Called each frame by the Mod Entity Manager
        /// </summary>
        public void Draw()
        {
            // Nothing to do here
        }

        /// <summary>
        /// Called by the <see cref="GameStateObserverManualPatch.OnGameLoopRunning"/> event and used to cleanup state between games
        /// </summary>
        public void OnGameLoopStarted()
        {
            isGameLoopRunning = true;

            ResetState();
        }

        /// <summary>
        /// Called by the <see cref="GameStateObserverManualPatch.OnGameLoopNotRunning"/> event and used to cleanup state between games
        /// </summary>
        public void OnGameLoopStopped()
        {
            isGameLoopRunning = false;

            ResetState();
        }

        /// <summary>
        /// Called by the <see cref="PunishmentPlayerStateObserverManualPatch.OnPlayerYTeleported"/> event and used to detect the player's
        /// Y position being modified for a teleport when moving between screens so we can account for the change in Y location
        /// when calculating rewards/punishments
        /// </summary>
        private void OnPlayerYTeleported(float yDelta)
        {
            teleportCompensation += yDelta;
        }

        /// <summary>
        /// Called by the <see cref="PunishmentPlayerStateObserverManualPatch.OnPlayerSaveStateApplying"/> event and used to detect when the player's state
        /// is about to be modified due to a save state will be applied.
        /// </summary>
        private void OnPlayerSaveStateApplying()
        {
            try
            {
                if (IsFeedbackEnabled())
                {
                    // We could calculate out the distance moved by the apply (as teleport compensation) and still punish/reward the player- but given the player is
                    // either loading a save or is being moved by a death from a modifier we will instead just trigger feedback immediately (so they don't get out of it)...
                    CheckAndTriggerFeedback(forcePunishment: true);

                    // Then reset state since we are about to be warped and don't want our current state to carry through the warp.
                    // We also want to pause state updates for a bit as it seems to take a couple frames for the player to properly update state (such as IsOnGround)
                    // when save state is applied
                    ResetState(TeleportPauseTime, true);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Error handling PlayerSaveStateApplying {e.ToString()}");
            }
        }

        /// <summary>
        /// Returns whether feedback is enabled and should generate or not
        /// </summary>
        private bool IsFeedbackEnabled()
        {
            return isGameLoopRunning && !debugToggledOff && (feedbackPauseTimer <= 0.0f);
        }

        /// <summary>
        /// Calculates a Reward given the provided delta and considering the player settings
        /// </summary>
        private (bool, float, float, float) CalculateReward(float yDelta, bool logResult = true)
        {
            bool receivingReward = false;
            float rewardFraction = 0.0f;
            float rewardIntensity = 0.0f;
            float rewardDuration = 0.0f;

            if (enableRewards)
            {
                float rewardDistance = Math.Abs(yDelta);
                if (rewardDistance >= minRewardDistance)
                {
                    receivingReward = true;
                    rewardFraction = 0.0f;

                    float rewardDistanceDiff = maxRewardDistance - minRewardDistance;
                    if (rewardDistanceDiff > 0.0f)
                    {
                        rewardFraction = (rewardDistance - minRewardDistance) / rewardDistanceDiff;
                        rewardFraction = Math.Min(rewardFraction, 1.0f);
                    }

                    rewardIntensity = minRewardIntensity + ((maxRewardIntensity - minRewardIntensity) * rewardFraction);
                    rewardDuration = minRewardDuration + ((maxRewardDuration - minRewardDuration) * rewardFraction);

                    if (roundDurations)
                    {
                        rewardDuration = (float)Math.Round(rewardDuration);
                    }

                    // If we didn't calculate a positive intensity or duration we aren't actually receiving a reward
                    if ((rewardDuration <= 0.0f) || (rewardIntensity <= 0.0f))
                    {
                        receivingReward = false;
                    }
                }
                if (logResult)
                {
                    logger.Information($"Calculating reward for {rewardDistance} units progressed...");
                    logger.Information($"\tReward: '{receivingReward}' | Intensity: {rewardIntensity} | Duration: {rewardDuration}");
                }
            }

            return (receivingReward, rewardFraction, rewardIntensity, rewardDuration);
        }

        /// <summary>
        /// Calculates a Punishment given the provided delta and considering the player settings
        /// </summary>
        private (bool, float, float, float) CalculatePunishment(float yDelta, bool logResult = true)
        {
            bool receivingPunishment = false;
            float punishmentFraction = 0.0f;
            float punishmentIntensity = 0.0f;
            float punishmentDuration = 0.0f;

            if (enablePunishment)
            {
                float punishmentDistance = Math.Abs(yDelta);
                if (punishmentDistance >= minFallDistance)
                {
                    receivingPunishment = true;
                    punishmentFraction = 0.0f;

                    float punishmentDistanceDiff = maxFallDistance - minFallDistance;
                    if (punishmentDistanceDiff > 0.0f)
                    {
                        punishmentFraction = (punishmentDistance - minFallDistance) / punishmentDistanceDiff;
                        punishmentFraction = Math.Min(punishmentFraction, 1.0f);
                    }

                    punishmentIntensity = minPunishmentIntensity + ((maxPunishmentIntensity - minPunishmentIntensity) * punishmentFraction);
                    punishmentDuration = minPunishmentDuration + ((maxPunishmentDuration - minPunishmentDuration) * punishmentFraction);

                    if (roundDurations)
                    {
                        punishmentDuration = (float)Math.Round(punishmentDuration);
                    }

                    // If we didn't calculate a positive intensity or duration we aren't actually receiving a punishment
                    if ((punishmentDuration <= 0.0f) || (punishmentIntensity <= 0.0f))
                    {
                        receivingPunishment = false;
                    }
                }
                if (logResult)
                {
                    logger.Information($"Calculating punishment for {punishmentDistance} units fell...");
                    logger.Information($"\tPunishment: '{receivingPunishment}' | Intensity: {punishmentIntensity} | Duration: {punishmentDuration}");
                }
            }

            return (receivingPunishment, punishmentFraction, punishmentIntensity, punishmentDuration);
        }

        /// <summary>
        /// Resets falling/grounded/teleport/etc. state and allows pausing future state updates for a provided amount of time.
        /// </summary>
        private void ResetState(float pauseTime = 0.0f, bool rememberProgress = false)
        {
            preResetHighestGroundY = rememberProgress ? highestGroundY : float.NaN;

            isInAir = false;
            lastGroundedY = float.NaN;
            highestGroundY = float.NaN;
            lastPlayerY = float.NaN;
            teleportCompensation = 0.0f;

            feedbackPauseTimer = pauseTime;
        }

        /// <summary>
        /// Generations a string for display on screen feedback information, taking the current on screen display behavior into account
        /// </summary>
        private string GenerateFeedbackInfoString(string baseString, float distanceFraction, float intensity, float duration, string postFix = "")
        {
            // When displaying values do some rounding to keep the string length sane
            switch (onScreenDisplayBehavior)
            {
                case PunishmentOnScreenDisplayBehavior.FeedbackIntensityAndDuration:
                    return $"{baseString} ({Math.Round(intensity)}% x {Math.Round(duration, 2)}s){postFix}";
                case PunishmentOnScreenDisplayBehavior.DistanceBasedPercentage:
                    return $"{baseString} ({Math.Round(distanceFraction * 100.0f)}%){postFix}";
                case PunishmentOnScreenDisplayBehavior.MessageOnly:
                    return baseString+postFix;
                default:
                    return "";
            }
        }

        /// <summary>
        /// Updates the last action text and restarts its display timer
        /// </summary>
        private void UpdateLastAction(string actionText, Color textColor)
        {
            lastActionDrawTimer = LastActionDisplayTime;

            lastActionTextEntity.TextValue = actionText;
            lastActionTextEntity.TextColor = textColor;
        }

        /// <summary>
        /// Returns <see cref="JumpKing.PlayerValues.JUMP"/>
        /// </summary>
        private float GetPlayerValuesJUMP()
        {
            return (float)getPlayerValuesJumpMethod.Invoke(null, null);
        }

        /// <summary>
        /// Returns <see cref="JumpKing.PlayerValues.MAX_FALL"/>
        /// </summary>
        private float GetPlayerValuesMAX_FALL()
        {
            return (float)getPlayerValuesMaxFallMethod.Invoke(null, null);
        }
    }
}
