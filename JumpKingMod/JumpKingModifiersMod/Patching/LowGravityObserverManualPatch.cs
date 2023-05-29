using HarmonyLib;
using JumpKingModifiersMod.API;
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
    /// An implementation of <see cref="IManualPatch"/> to override the 
    /// Low Gravity state of the Level Manager
    /// </summary>
    public class LowGravityObserverManualPatch : IManualPatch, ILowGravityObserver
    {
        private static ILogger logger;
        private static bool isLowGravityOverridden;

        public LowGravityObserverManualPatch(ILogger logger)
        {
            LowGravityObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            isLowGravityOverridden = false;
        }

        /// <inheritdoc/>
        public bool GetLowGravityOverrideState()
        {
            return isLowGravityOverridden;
        }

        /// <inheritdoc/>
        public void SetLowGravityOverrideState(bool isActive)
        {
            isLowGravityOverridden = isActive;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var lowGravityMethod = AccessTools.Method("JumpKing.Level.LevelManager:IsInLowGravity");
            var postfixLowGravityMethod = this.GetType().GetMethod("PostfixIsInLowGravity");
            harmony.Patch(lowGravityMethod, postfix: new HarmonyMethod(postfixLowGravityMethod));
        }

        /// <summary>
        /// Called after 'JumpKing.Level.LevelScreen:IsInLowGravity' and optionally overrides the result
        /// </summary>
        public static void PostfixIsInLowGravity(object __instance, ref bool __result)
        {
            if (isLowGravityOverridden)
            {
                __result = true;
            }
        }
    }
}
