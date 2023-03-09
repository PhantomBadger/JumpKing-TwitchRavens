using HarmonyLib;
using Logging;
using Logging.API;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching
{
    public class PlayerValuesManualPatch : IManualPatch
    {
        private static ILogger logger;
        private static float walkSpeedModifier = 1f;

        public PlayerValuesManualPatch(ILogger logger)
        {
            PlayerValuesManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void SetUpManualPatch(Harmony harmony)
        {
            var walkSpeedGetter = AccessTools.Method("JumpKing.PlayerValues:get_WALK_SPEED");
            var postfixwalkSpeedGetter = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixWalkSpeedGetter");
            harmony.Patch(walkSpeedGetter, postfix: new HarmonyMethod(postfixwalkSpeedGetter));
            logger.Information($"Patching get_WALK_SPEED '{walkSpeedGetter}' to PostfixWalkSpeedGetter '{postfixwalkSpeedGetter}'");
        }

        public static void PostfixWalkSpeedGetter(object __instance, ref Single __result)
        {
            __result *= walkSpeedModifier;
        }

        public void SetWalkSpeedModifer(float newModifier)
        {
            PlayerValuesManualPatch.walkSpeedModifier = newModifier;
        }

        public float GetWalkSpeedModifier()
        {
            return PlayerValuesManualPatch.walkSpeedModifier;
        }
    }
}
