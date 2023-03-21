using HarmonyLib;
using JumpKingModifiersMod.API;
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
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IWalkSpeedModifier"/> which allows us to apply modifiers to
    /// the values in 'JumpKing.PlayerValues'
    /// </summary>
    public class PlayerValuesManualPatch : IManualPatch, IWalkSpeedModifier
    {
        private static ILogger logger;
        private static float walkSpeedModifier = 1f;

        /// <summary>
        /// Ctor for creating a <see cref="PlayerValuesManualPatch"/>
        /// </summary>
        /// <param name="logger"></param>
        public PlayerValuesManualPatch(ILogger logger)
        {
            PlayerValuesManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var walkSpeedGetter = AccessTools.Method("JumpKing.PlayerValues:get_WALK_SPEED");
            var postfixwalkSpeedGetter = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixWalkSpeedGetter");
            harmony.Patch(walkSpeedGetter, postfix: new HarmonyMethod(postfixwalkSpeedGetter));
        }

        /// <summary>
        /// Called after 'JumpKing.PlayerValues:get_WALK_SPEED' and applies a modifier to the returned value
        /// </summary>
        public static void PostfixWalkSpeedGetter(object __instance, ref Single __result)
        {
            __result *= walkSpeedModifier;
        }

        /// <summary>
        /// An implementation of <see cref="IWalkSpeedModifier.SetWalkSpeedModifer(float)"/> which applies a modifier
        /// </summary>
        public void SetWalkSpeedModifer(float newModifier)
        {
            PlayerValuesManualPatch.walkSpeedModifier = newModifier;
        }

        /// <summary>
        /// An implementation of <see cref="IWalkSpeedModifier.GetWalkSpeedModifier"/> which gets the currently active modifier
        /// </summary>
        public float GetWalkSpeedModifier()
        {
            return PlayerValuesManualPatch.walkSpeedModifier;
        }
    }
}
