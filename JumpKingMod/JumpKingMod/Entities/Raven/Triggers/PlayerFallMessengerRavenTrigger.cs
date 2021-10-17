using HarmonyLib;
using JumpKingMod.API;
using JumpKingMod.Patching;
using JumpKingMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IMessengerRavenTrigger"/> which triggers when the player falls
    /// </summary>
    public class PlayerFallMessengerRavenTrigger : IMessengerRavenTrigger, IManualPatch
    {
        public event MessengerRavenTriggerArgs OnMessengerRavenTrigger;

        private readonly IInsultGetter insultGetter;
        private readonly UserSettings userSettings;
        private readonly int spawnCount;

        private static PlayerFallMessengerRavenTrigger instance;

        /// <summary>
        /// Constructor for creating a <see cref="PlayerFallMessengerRavenTrigger"/>
        /// </summary>
        /// <param name="logger">an <see cref="ILogger"/> implementation for logging</param>
        public PlayerFallMessengerRavenTrigger(UserSettings userSettings, IInsultGetter insultGetter, ILogger logger)
        {
            this.insultGetter = insultGetter ?? throw new ArgumentNullException(nameof(insultGetter));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

            spawnCount = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavenInsultSpawnCountKey, 3);

            instance = this;
        }

        /// <summary>
        /// Sets up the patching required for this trigger
        /// </summary>
        public void SetUpManualPatch(Harmony harmony)
        {
            var onPlayerFallMethod = AccessTools.Method("JumpKing.MiscSystems.Achievements.AchievementManager:OnPlayerFall");
            var postfixMethod = AccessTools.Method($"JumpKingMod.Entities.Raven.Triggers.{this.GetType().Name}:PostfixTriggerMethod");
            harmony.Patch(onPlayerFallMethod, postfix: new HarmonyMethod(postfixMethod));
        }

        /// <summary>
        /// Called by harmony after <see cref="JumpKing.MiscSystems.Achievements.AchievementManager"/> 'OnPlayerFall' private method
        /// </summary>
        public static void PostfixTriggerMethod(object __instance)
        {
            Task.Run(() =>
            {
                if (instance == null)
                {
                    return;
                }

                for (int i = 0; i < instance.spawnCount; i++)
                {
                    Task.Delay(500).Wait();
                    instance.OnMessengerRavenTrigger?.Invoke(null, Color.White, instance.insultGetter.GetInsult());
                }
            });
        }
    }
}
