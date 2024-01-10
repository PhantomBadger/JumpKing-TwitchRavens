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
        private float LastActionDrawTimer;
        private UITextEntity IncomingPunishmentTextEntity;
        private UITextEntity LastActionTextEntity;

        private bool debugToggledOff;
        private bool wasToggleHeld;
        private bool wasTestHeld;

        private readonly Keys toggleKey;
        private readonly Keys testKey;
        private readonly bool displayFeedbackStrength;
        private readonly bool RoundDurations;

        private readonly bool EnablePunishment;
        private readonly float MinPunishmentDuration;
        private readonly float MinPunishmentIntensity;
        private readonly float MaxPunishmentDuration;
        private readonly float MaxPunishmentIntensity;
        private readonly float MinFallDistance;
        private readonly float MaxFallDistance;
        private readonly bool EasyModePunishment;

        private readonly bool EnableRewards;
        private readonly float MinRewardDuration;
        private readonly float MinRewardIntensity;
        private readonly float MaxRewardDuration;
        private readonly float MaxRewardIntensity;
        private readonly float MinRewardDistance;
        private readonly float MaxRewardDistance;
        private readonly bool ProgressOnlyRewards;

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

            IncomingPunishmentTextEntity = new UITextEntity(modEntityManager, new Vector2(240, 16), "", new Color(1.0f, 1.0f, 1.0f, 0.5f), UIEntityAnchor.Center, JKContentManager.Font.StyleFont);
            LastActionTextEntity = new UITextEntity(modEntityManager, new Vector2(240, 360), "", Color.White, UIEntityAnchor.Center, JKContentManager.Font.StyleFont);
            LastActionDrawTimer = 0.0f;

            debugToggledOff = false;
            wasToggleHeld = false;
            wasTestHeld = false;

            playerStateObserver.OnPlayerYTeleported += OnPlayerYTeleported;

            // Cache our settings off so we don't need to read them every time
            toggleKey = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentModToggleKeyKey, Keys.F8);
            testKey = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentFeedbackTestKeyKey, Keys.F9);
            displayFeedbackStrength = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.DisplayFeedbackStrengthKey, false);
            RoundDurations = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.RoundDurationsKey, false);

            EnablePunishment = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.EnablePunishmentKey, false);
            MinPunishmentDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentDurationKey, 1.0f);
            MinPunishmentIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentIntensityKey, 1.0f);
            MaxPunishmentDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentDurationKey, 1.0f);
            MaxPunishmentIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentIntensityKey, 1.0f);
            MinFallDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinPunishmentFallDistanceKey, 0.0f);
            MaxFallDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxPunishmentfallDistanceKey, 0.0f);
            EasyModePunishment = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.PunishmentEasyModeKey, false);

            EnableRewards = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.EnableRewardsKey, false);
            MinRewardDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardDurationKey, 1.0f);
            MinRewardIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardIntensityKey, 1.0f);
            MaxRewardDuration = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardDurationKey, 1.0f);
            MaxRewardIntensity = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardIntenityKey, 1.0f);
            MinRewardDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MinRewardProgressDistanceKey, 0.0f);
            MaxRewardDistance = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.MaxRewardProgressDistanceKey, 0.0f);
            ProgressOnlyRewards = userSettings.GetSettingOrDefault(JumpKingPunishmentModSettingsContext.RewardProgressOnlyKey, false);

            // Some validation on settings here so we don't have to worry about it below
            MaxPunishmentDuration = Math.Max(MinPunishmentDuration, MaxPunishmentDuration);
            MaxPunishmentIntensity = Math.Max(MinPunishmentIntensity, MaxPunishmentIntensity);
            MaxFallDistance = Math.Max(MinFallDistance, MaxFallDistance);
            MaxRewardDuration = Math.Max(MinRewardDuration, MaxRewardDuration);
            MaxRewardIntensity = Math.Max(MinRewardIntensity, MaxRewardIntensity);
            MaxRewardDistance = Math.Max(MinRewardDistance, MaxRewardDistance);

            modEntityManager.AddEntity(this, 0);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            IncomingPunishmentTextEntity?.Dispose();
            IncomingPunishmentTextEntity = null;

            LastActionTextEntity?.Dispose();
            LastActionTextEntity = null;

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
                }
                wasToggleHeld = toggleHeld;

                bool testHeld = keyboardState.IsKeyDown(testKey);
                if (testHeld && !wasTestHeld)
                {
                    logger.Information("Sending test feedback to your feedback device...");
                    punishmentDevice.Test(50.0f, 1.0f);
                }
                wasTestHeld = testHeld;

                LastActionDrawTimer = Math.Max(0.0f, LastActionDrawTimer - delta);

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
                                if (ProgressOnlyRewards)
                                {
                                    yDelta = yLocation - highestGroundY;
                                }
                                if (yDelta < 0.0f)  // Make sure we actually made progress in the case of progress only rewards
                                {
                                    var reward = CalculateReward(yDelta);
                                    if (reward.Item1)
                                    {
                                        punishmentDevice.Reward(reward.Item2, reward.Item3);
                                        // Do some rounding to keep the string length sane
                                        UpdateLastAction(displayFeedbackStrength ? $"Reward! ({Math.Round(reward.Item2)}% x {Math.Round(reward.Item3, 2)}s)" : "Reward!", Color.Lime);
                                    }
                                }
                            }
                            else if (yDelta > 0.0f)  // Negative progress
                            {
                                var punishment = CalculatePunishment(yDelta);
                                if (punishment.Item1)
                                {
                                    punishmentDevice.Punish(punishment.Item2, punishment.Item3, EasyModePunishment);
                                    // Do some rounding to keep the string length sane
                                    UpdateLastAction(displayFeedbackStrength ? $"Punishment! ({Math.Round(punishment.Item2)}% x {Math.Round(punishment.Item3, 2)}s)" : "Punishment!", EasyModePunishment ? Color.Lime : Color.Red);
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
                var incomingPunishment = (false, 0.0f, 0.0f);
                if (isInAir && hasValidGroundedY)
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
                    IncomingPunishmentTextEntity.TextValue = "";
                }
                else
                {
                    // Do some rounding to keep the string length sane
                    IncomingPunishmentTextEntity.TextValue = displayFeedbackStrength ? $"Incoming punishment ({Math.Round(incomingPunishment.Item2)}% x {Math.Round(incomingPunishment.Item3, 2)}s)..." : "Incoming punishment...";
                }

                // Fade the last action text out overtime (over the second half of it's lifetime)
                float Alpha = 1.0f;
                if (LastActionDrawTimer < (LastActionDisplayTime / 2.0f))
                {
                    Alpha = LastActionDrawTimer / (LastActionDisplayTime / 2.0f);
                }
                LastActionTextEntity.TextColor = new Color(LastActionTextEntity.TextColor, Alpha);
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
        private (bool, float, float) CalculateReward(float DeltaY, bool bLog = true)
        {
            bool receivingReward = false;
            float rewardIntensity = 0.0f;
            float rewardDuration = 0.0f;

            if (EnableRewards)
            {
                float rewardDistance = Math.Abs(DeltaY);
                if (rewardDistance >= MinRewardDistance)
                {
                    receivingReward = true;
                    float rewardFraction = 1.0f;

                    float rewardDistanceDiff = MaxRewardDistance - MinRewardDistance;
                    if (rewardDistanceDiff > 0.0f)
                    {
                        rewardFraction = (rewardDistance - MinRewardDistance) / rewardDistanceDiff;
                        rewardFraction = Math.Min(rewardFraction, 1.0f);
                    }

                    rewardIntensity = MinRewardIntensity + ((MaxRewardIntensity - MinRewardIntensity) * rewardFraction);
                    rewardDuration = MinRewardDuration + ((MaxRewardDuration - MinRewardDuration) * rewardFraction);

                    if (RoundDurations)
                    {
                        rewardDuration = (float)Math.Round(rewardDuration);
                        // If rounding turned the duration to zero return that we aren't actually doing a reward
                        if (rewardDuration == 0.0f)
                        {
                            receivingReward = false;
                        }
                    }
                }
                if (bLog)
                {
                    logger.Information($"Calculating reward for {rewardDistance} units progressed...");
                    logger.Information($"\tReward: '{receivingReward}' | Intensity: {rewardIntensity} | Duration: {rewardDuration}");
                }
            }

            return (receivingReward, rewardIntensity, rewardDuration);
        }

        /// <summary>
        /// Calculates a Punishment given the provided delta and considering the player settings
        /// </summary>
        private (bool, float, float) CalculatePunishment(float DeltaY, bool bLog = true)
        {
            bool receivingPunishment = false;
            float punishmentIntensity = 0.0f;
            float punishmentDuration = 0.0f;

            if (EnablePunishment)
            {
                float punishmentDistance = Math.Abs(DeltaY);
                if (punishmentDistance >= MinFallDistance)
                {
                    receivingPunishment = true;
                    float punishmentFraction = 1.0f;

                    float punishmentDistanceDiff = MaxFallDistance - MinFallDistance;
                    if (punishmentDistanceDiff > 0.0f)
                    {
                        punishmentFraction = (punishmentDistance - MinFallDistance) / punishmentDistanceDiff;
                        punishmentFraction = Math.Min(punishmentFraction, 1.0f);
                    }

                    punishmentIntensity = MinPunishmentIntensity + ((MaxPunishmentIntensity - MinPunishmentIntensity) * punishmentFraction);
                    punishmentDuration = MinPunishmentDuration + ((MaxPunishmentDuration - MinPunishmentDuration) * punishmentFraction);

                    if (RoundDurations)
                    {
                        punishmentDuration = (float)Math.Round(punishmentDuration);
                        // If rounding our punishment turned the duration to zero return that we aren't actually doing a punishment
                        if (punishmentDuration == 0.0f)
                        {
                            receivingPunishment = false;
                        }
                    }
                }
                if (bLog)
                {
                    logger.Information($"Calculating punishment for {punishmentDistance} units fell...");
                    logger.Information($"\tPunishment: '{receivingPunishment}' | Intensity: {punishmentIntensity} | Duration: {punishmentDuration}");
                }
            }

            return (receivingPunishment, punishmentIntensity, punishmentDuration);
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
        /// Updates the last action text and restarts its display timer
        /// </summary>
        private void UpdateLastAction(string ActionText, Color TextColor)
        {
            LastActionDrawTimer = LastActionDisplayTime;

            LastActionTextEntity.TextValue = ActionText;
            LastActionTextEntity.TextColor = TextColor;
        }
    }
}
