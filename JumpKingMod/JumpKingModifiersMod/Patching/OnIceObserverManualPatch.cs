using HarmonyLib;
using JumpKingModifiersMod.API;
using Logging.API;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> for overriding IsOnIce
    /// </summary>
    public class OnIceObserverManualPatch : IManualPatch, IOnIceObserver
    {
        private static ILogger logger;
        private static bool isIsOnIceOverridden;
        private static MethodInfo isOnGroundMethod;

        public OnIceObserverManualPatch(ILogger logger)
        {
            OnIceObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            isIsOnIceOverridden = false;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            isOnGroundMethod = AccessTools.Method("JumpKing.Player.BodyComp:get_IsOnGround");

            var isOnIceMethod = AccessTools.Method("JumpKing.Player.BodyComp:get_IsOnIce");
            var postfixIsOnIceMethod = this.GetType().GetMethod("PostfixGetIsOnIce");
            harmony.Patch(isOnIceMethod, postfix: new HarmonyMethod(postfixIsOnIceMethod));
        }

        /// <summary>
        /// Called after 'JumpKing.Player.BodyComp:get_IsOnIce' with optional override
        /// </summary>
        public static void PostfixGetIsOnIce(object __instance, ref bool __result)
        {
            if (isIsOnIceOverridden)
            {
                __result = (bool)isOnGroundMethod.Invoke(__instance, null);
            }
        }

        /// <inheritdoc/>
        public bool GetIsOnIceOverrideState()
        {
            return isIsOnIceOverridden;
        }

        /// <inheritdoc/>
        public void SetIsOnIceOverrideState(bool shouldOverrideIsOnIce)
        {
            isIsOnIceOverridden = shouldOverrideIsOnIce;
        }

    }
}
