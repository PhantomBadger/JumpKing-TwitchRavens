using HarmonyLib;
using JumpKing.MiscSystems.Achievements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> which disables the achievement system
    /// </summary>
    public class AchievementRegisterDisableManualPatch : IManualPatch
    {
        public AchievementRegisterDisableManualPatch()
        {

        }

        public void SetUpManualPatch(Harmony harmony)
        {
            var achievementRegisterMethod = AccessTools.Method("JumpKing.MiscSystems.Achievements.AchievementRegister:RegisterAchievement");
            var disabledMethod = this.GetType().GetMethod("DisableAchievementRegisterPrefix");

            harmony.Patch(achievementRegisterMethod, prefix: new HarmonyMethod(disabledMethod));
        }

        public static bool DisableAchievementRegisterPrefix(AchievementCode p_achievement)
        {
            // Immediately return false, preventing the original from running
            return false;
        }
    }
}
