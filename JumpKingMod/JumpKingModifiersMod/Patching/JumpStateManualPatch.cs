using BehaviorTree;
using HarmonyLib;
using JumpKingModifiersMod.API;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IPlayerJumper"/> which reads jumps performed by the player
    /// and allows us to trigger our own
    /// </summary>
    public class JumpStateManualPatch : IManualPatch, IPlayerJumper
    {
        private static ILogger logger;

        // Method/Field wrappers
        private static MethodInfo doJumpMethod;
        private static MethodInfo getInputMethod;
        private static MethodInfo getStateMethod;
        private static FieldInfo dpadLeftField;
        private static FieldInfo dpadRightField;

        private static ConcurrentQueue<JumpState> requestedJumps;
        private static int? overrideDPad;
        private static JumpState prevJumpState;

        /// <summary>
        /// Ctor for creating a <see cref="JumpStateManualPatch"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/></param>
        public JumpStateManualPatch(ILogger logger)
        {
            JumpStateManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            requestedJumps = new ConcurrentQueue<JumpState>();
            doJumpMethod = null;
            overrideDPad = null;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            getInputMethod = AccessTools.Method("JumpKing.Player.PlayerNode:get_input");
            getStateMethod = AccessTools.Method("JumpKing.Player.InputComponent:GetState");

            var getStatePostixMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:GetStatePostfixPatchMethod");
            harmony.Patch(getStateMethod, postfix: new HarmonyMethod(getStatePostixMethod));

            var myRunMethod = AccessTools.Method("JumpKing.Player.JumpState:MyRun");
            var myRunPrefixMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:MyRunPrefixPatchMethod");
            harmony.Patch(myRunMethod, prefix: new HarmonyMethod(myRunPrefixMethod));

            doJumpMethod = AccessTools.Method("JumpKing.Player.JumpState:DoJump");
            var doJumpPrefix = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:DoJumpPrefixPatchMethod");
            harmony.Patch(doJumpMethod, prefix: new HarmonyMethod(doJumpPrefix));
        }

        /// <summary>
        /// Called before 'JumpKing.Player.JumpState:MyRun' and triggers any requested jumps.
        /// If a jump is not requested the original method continues as normal
        /// </summary>
        public static bool MyRunPrefixPatchMethod(object __instance, TickData p_data)
        {
            if (requestedJumps.TryDequeue(out JumpState requestedJump))
            {
                // Set the dpad override
                overrideDPad = requestedJump.XValue;

                logger.Information($"Fake jumping now!");
                doJumpMethod?.Invoke(__instance, new object[1] { requestedJump.Intensity });

                // Unset the dpad override
                overrideDPad = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called before 'JumpKing.Player.JumpState:DoJump' and records the state of any jumps made
        /// </summary>
        public static void DoJumpPrefixPatchMethod(object __instance, float p_intensity)
        {
            object input = getInputMethod.Invoke(__instance, null);
            object state = getStateMethod.Invoke(input, null);
            if (dpadLeftField == null)
            {
                dpadLeftField = AccessTools.Field(state.GetType(), "left");
            }
            if (dpadRightField == null)
            {
                dpadRightField = AccessTools.Field(state.GetType(), "right");
            }
            bool left = (bool)dpadLeftField.GetValue(state);
            bool right = (bool)dpadRightField.GetValue(state);
            int num = 0;
            if (right)
            {
                num++;
            }
            if (left)
            {
                num--;
            }

            prevJumpState = new JumpState(p_intensity, num);
            logger.Information($"Logging jump state of '{p_intensity}' and '{num}'");
        }

        /// <summary>
        /// Called after 'JumpKing.Player.InputComponent:GetState' and applies an override if set internally
        /// </summary>
        public static void GetStatePostfixPatchMethod(object __instance, ref object __result)
        {
            if (overrideDPad.HasValue)
            {
                int value = overrideDPad.Value;
                dpadRightField.SetValue(__result, value > 0);
                dpadLeftField.SetValue(__result, value < 0);
            }
        }

        /// <summary>
        /// An implementation of <see cref="IPlayerJumper.RequestJump(JumpState)"/> which queues up a jump to be performed
        /// </summary>
        /// <param name="requestedJump">The <see cref="JumpState"/> to perform</param>
        public void RequestJump(JumpState requestedJump)
        {
            logger.Information($"Queueing up a fake jump!");
            JumpStateManualPatch.requestedJumps.Enqueue(requestedJump);
        }

        /// <summary>
        /// An implementation of <see cref="IPlayerJumper.GetPreviousJumpState"/> which returns the state of the last recorded jump
        /// If no jump has been recorded yet this will be null
        /// </summary>
        public JumpState GetPreviousJumpState()
        {
            return prevJumpState;
        }
    }
}
