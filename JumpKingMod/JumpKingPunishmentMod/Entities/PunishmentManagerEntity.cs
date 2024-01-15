﻿using JumpKing;
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

        private bool isInAir;
        private float lastGroundedY;
        private float highestGroundY;
        private bool hasValidGroundedY;
        private float teleportCompensation;

        private const float LastActionDisplayTime = 3.0f;

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

            ResetState();

            incomingPunishmentTextEntity = new UITextEntity(modEntityManager, new Vector2(240, 16), "", new Color(1.0f, 1.0f, 1.0f, 0.5f), UIEntityAnchor.Center, JKContentManager.Font.StyleFont);
            lastActionTextEntity = new UITextEntity(modEntityManager, new Vector2(240, 360), "", Color.White, UIEntityAnchor.Center, JKContentManager.Font.StyleFont);
            lastActionDrawTimer = 0.0f;

            debugToggledOff = false;
            wasToggleHeld = false;
            wasTestHeld = false;

            playerStateObserver.OnPlayerYTeleported += OnPlayerYTeleported;

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

            modEntityManager?.RemoveEntity(this);
        }

        /// <summary>
        /// Called each frame by the Mod Entity Manager, allows control of the mod, managers punishing and rewarding the player,
        /// and handles updating on screen messages related to the mod
        /// </summary>
        public void Update(float delta)
        {
            try
            {
                punishmentDevice.Update(delta);

                // Debug controls
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
                    }
                    ResetState();
                }
                wasToggleHeld = toggleHeld;

                bool testHeld = keyboardState.IsKeyDown(testKey);
                if (testHeld && !wasTestHeld)
                {
                    logger.Information("Sending test feedback to your feedback device...");
                    punishmentDevice.Test(50.0f, 1.0f);
                }
                wasTestHeld = testHeld;

                lastActionDrawTimer = Math.Max(0.0f, lastActionDrawTimer - delta);

                PunishmentPlayerState playerState = playerStateObserver.GetPlayerState();
                if (!debugToggledOff && isGameLoopRunning && (playerState != null))
                {
                    // Consider sand ground too for our purposes
                    bool isOnGround = playerState.IsOnGround || playerState.IsOnSand;
                    if (isOnGround)
                    {
                        // Add the teleport compensation at all times so we are always working with 'Y's in 'non-teleported' space
                        float yLocation = playerState.Position.Y + teleportCompensation;
                        if (isInAir && hasValidGroundedY)
                        {
                            // We have landed, calculate and execute a shock/reward
                            float yDelta = yLocation - lastGroundedY;
                            // Note, Y DECREASES as you move upward
                            if (yDelta < 0.0f)  // Positive progress
                            {
                                if (progressOnlyRewards)
                                {
                                    yDelta = yLocation - highestGroundY;
                                }
                                if (yDelta < 0.0f)  // Make sure we actually made progress in the case of progress only rewards
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
                        lastGroundedY = yLocation;
                        if (!hasValidGroundedY)
                        {
                            // If we don't have a valid grounded Y that means our highest grounded
                            // isn't valid either so we should just take the location without a min check
                            highestGroundY = yLocation;
                        }
                        else
                        {
                            // Again, upward progress is NEGATIVE
                            highestGroundY = Math.Min(yLocation, highestGroundY);
                        }
                        hasValidGroundedY = true;
                    }
                    isInAir = !isOnGround;
                }

                // Update the incoming text entity
                // Only show punishments incoming as rewards will be weird with the arcs of a jump (and punishments
                // generally can't be avoided once they start)
                var incomingPunishment = (false, 0.0f, 0.0f, 0.0f);
                if (!debugToggledOff && isInAir && hasValidGroundedY)
                {
                    // Again add teleport compensation to work in 'non-teleported' space
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

                // Fade the last action text out overtime (over the second half of it's lifetime)
                float Alpha = 1.0f;
                if (lastActionDrawTimer < (LastActionDisplayTime / 2.0f))
                {
                    Alpha = lastActionDrawTimer / (LastActionDisplayTime / 2.0f);
                }
                lastActionTextEntity.TextColor = new Color(lastActionTextEntity.TextColor, Alpha);
            }
            catch (Exception e)
            {
                logger.Error($"Error updating Punishment Manager {e.ToString()}");
            }
        }

        /// <summary>
        /// Called each frame by the Mod Entity Manager
        /// </summary>
        public void Draw()
        {
            // Nothing to do here
        }

        /// <summary>
        /// Called by the <see cref="GameStateObserverManualPatch.OnGameLoopRunning"/> and used to cleanup state between games
        /// </summary>
        public void OnGameLoopStarted()
        {
            isGameLoopRunning = true;

            ResetState();
        }

        /// <summary>
        /// Called by the <see cref="GameStateObserverManualPatch.OnGameLoopNotRunning"/> and used to cleanup state between games
        /// </summary>
        public void OnGameLoopStopped()
        {
            isGameLoopRunning = false;

            ResetState();
        }

        /// <summary>
        /// Called by the <see cref="PlayerStateObserverManualPatch.OnPlayerYTeleported"/> and used to detect the player's
        /// Y position being modified for a teleport when moving between screens so we can account for the change in Y location
        /// when calculating rewards/punishments
        /// </summary>
        public void OnPlayerYTeleported(float yDelta)
        {
            teleportCompensation += yDelta;
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
                    rewardFraction = 1.0f;

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
                        // If rounding turned the duration to zero return that we aren't actually doing a reward
                        if (rewardDuration == 0.0f)
                        {
                            receivingReward = false;
                        }
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
                    punishmentFraction = 1.0f;

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
                        // If rounding our punishment turned the duration to zero return that we aren't actually doing a punishment
                        if (punishmentDuration == 0.0f)
                        {
                            receivingPunishment = false;
                        }
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
        /// Resets falling/grounded/teleport/etc. state
        /// </summary>
        private void ResetState()
        {
            isInAir = false;
            lastGroundedY = 0.0f;
            highestGroundY = 0.0f;
            hasValidGroundedY = false;
            teleportCompensation = 0.0f;
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
    }
}