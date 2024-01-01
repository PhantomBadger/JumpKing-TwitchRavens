using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Globalization;

namespace JumpKingPunishmentMod.Settings
{
    /// <summary>
    /// An aggregate of settings keys for use in <see cref="JumpKingPunishmentMod"/>
    /// </summary>
    public class JumpKingPunishmentModSettingsContext
    {
        public const string SettingsFileName = "JumpKingPunishmentMod.settings";

        public const string PunishmentModEnabledKey = "PunishmentModEnabled";

        public const string PunishmentModToggleKeyKey = "PunishmentModToggleKey";
        public const string PunishmentFeedbackTestKeyKey = "FeedbackTestKey";
        public const string RoundDurationsKey = "RoundDurations";

        public const string EnablePunishmentKey = "EnablePunishment";
        public const string MinPunishmentDurationKey = "MinPunishmentDuration";
        public const string MinPunishmentIntensityKey = "MinPunishmentIntensity";
        public const string MaxPunishmentDurationKey = "MaxPunishmentDuration";
        public const string MaxPunishmentIntensityKey = "MaxPunishmentIntensity";
        public const string MinPunishmentFallDistanceKey = "MinPunishmentFallDistance";
        public const string MaxPunishmentfallDistanceKey = "MaxPunishmentFallDistance";
        public const string PunishmentEasyModeKey = "PunishmentEasyMode";

        public const string EnableRewardsKey = "EnableRewards";
        public const string MinRewardDurationKey = "MinRewardDuration";
        public const string MinRewardIntensityKey = "MinRewardIntensity";
        public const string MaxRewardDurationKey = "MaxRewardDuration";
        public const string MaxRewardIntenityKey = "MaxRewardIntensity";
        public const string MinRewardProgressDistanceKey = "MinRewardProgressDistance";
        public const string MaxRewardProgressDistanceKey = "MaxRewardProgressDistance";
        public const string RewardProgressOnlyKey = "RewardProgressOnly";

        /// <summary>
        /// Gets the default state of the settings
        /// </summary>
        public static Dictionary<string, string> GetDefaultSettings()
        {
            return new Dictionary<string, string>()
            {
                // Enable
                { PunishmentModEnabledKey, true.ToString() },

                // General
                { PunishmentModToggleKeyKey, Keys.F8.ToString() },
                { PunishmentFeedbackTestKeyKey, Keys.F9.ToString() },
                { RoundDurationsKey, false.ToString() },

                // Punishment
                { EnablePunishmentKey, true.ToString() },
                { MinPunishmentDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture) },
                { MinPunishmentIntensityKey, 1.0f.ToString(CultureInfo.InvariantCulture) },
                { MaxPunishmentDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture) },
                { MaxPunishmentIntensityKey, 30.0f.ToString(CultureInfo.InvariantCulture) },
                { MinPunishmentFallDistanceKey, 150.0f.ToString(CultureInfo.InvariantCulture) },
                { MaxPunishmentfallDistanceKey, 1000.0f.ToString(CultureInfo.InvariantCulture) },
                { PunishmentEasyModeKey, false.ToString() },

                // Rewards
                { EnableRewardsKey, false.ToString() },
                { MinRewardDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture) },
                { MinRewardIntensityKey, 5.0f.ToString(CultureInfo.InvariantCulture) },
                { MaxRewardDurationKey, 1.0f.ToString(CultureInfo.InvariantCulture) },
                { MaxRewardIntenityKey, 50.0f.ToString(CultureInfo.InvariantCulture) },
                { MinRewardProgressDistanceKey, 0.0f.ToString(CultureInfo.InvariantCulture) },
                { MaxRewardProgressDistanceKey, 150.0f.ToString(CultureInfo.InvariantCulture) },
                { RewardProgressOnlyKey, true.ToString() },
             };
        }
    }
}
