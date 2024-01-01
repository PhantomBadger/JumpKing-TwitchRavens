using HarmonyLib;
using JumpKing;
using JumpKing.Player;
using JumpKingPunishmentMod.Patching.States;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace JumpKingPunishmentMod.Patching
{
    public delegate void OnPlayerYTeleportedDelegate(float yDelta);

    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> to keep track of the player state
    /// </summary>
    public class PunishmentPlayerStateObserverManualPatch
    {
        private static MethodInfo isOnGroundMethod;
        private static MethodInfo isOnSandMethod;
        private static FieldInfo velocityField;
        private static FieldInfo knockedField;
        private static FieldInfo positionField;
        private static object bodyCompInstance;
        private static PunishmentPlayerStateObserverManualPatch instance;

        private static float preCapYLocation;
        public event OnPlayerYTeleportedDelegate OnPlayerYTeleported;

        /// <summary>
        /// Ctor for creating a <see cref="PunishmentPlayerStateObserverManualPatch"/>
        /// </summary>
        public PunishmentPlayerStateObserverManualPatch()
        {
            instance = this;
        }

        /// <summary>
        /// Sets up the manual patch to add some pre and post fix patches to functions on the <see cref="JumpKing.Player.BodyComp"/>
        /// </summary>
        /// <param name="harmony"></param>
        public void SetUpManualPatch(Harmony harmony)
        {
            var bodyCompMethod = AccessTools.Method("JumpKing.Player.BodyComp:Update");
            var postfixBodyCompMethod = AccessTools.Method($"JumpKingPunishmentMod.Patching.{this.GetType().Name}:PostfixBodyCompPatchMethod");
            harmony.Patch(bodyCompMethod, postfix: new HarmonyMethod(postfixBodyCompMethod));

            var capPositionMethod = AccessTools.Method("JumpKing.Player.BodyComp:CapPosition");
            var prefixCapPositionMethod = AccessTools.Method($"JumpKingPunishmentMod.Patching.{this.GetType().Name}:PrefixCapPositionMethod");
            var postfixCapPositionMethod = AccessTools.Method($"JumpKingPunishmentMod.Patching.{this.GetType().Name}:PostfixCapPositionMethod");
            harmony.Patch(capPositionMethod, prefix: new HarmonyMethod(prefixCapPositionMethod), postfix: new HarmonyMethod(postfixCapPositionMethod));
        }

        /// <summary>
        /// Runs after <see cref="JumpKing.Player.BodyComp.Update(float)"/> and allows us to track the state of the player
        /// </summary>
        public static void PostfixBodyCompPatchMethod(object __instance, float p_delta)
        {
            bodyCompInstance = __instance;
            if (isOnGroundMethod == null)
            {
                isOnGroundMethod = AccessTools.Method(bodyCompInstance.GetType(), "get_IsOnGround");
            }
            if (isOnSandMethod == null)
            {
                isOnSandMethod = AccessTools.Method(bodyCompInstance.GetType(), "get_IsOnSand");
            }
            if (velocityField == null)
            {
                velocityField = AccessTools.Field(bodyCompInstance.GetType(), "velocity");
            }
            if (knockedField == null)
            {
                knockedField = AccessTools.Field(bodyCompInstance.GetType(), "_knocked");
            }
            if (positionField == null)
            {
                positionField = AccessTools.Field(bodyCompInstance.GetType(), "position");
            }
        }

        /// <summary>
        /// Runs before <see cref="JumpKing.Player.BodyComp.CapPosition(bool)"/> and allows us to record the Y location of the player to detect teleports
        /// </summary>
        public static void PrefixCapPositionMethod(object __instance, bool p_capped_x)
        {
            // We could get the component and field accessor here but it shouldn't be an issue/doesn't really matter
            // Worst case we lose a frame of data on startup/init
            if (positionField != null && bodyCompInstance != null)
            {
                preCapYLocation = ((Vector2)positionField.GetValue(bodyCompInstance)).Y;
            }
        }

        /// <summary>
        /// Runs after <see cref="JumpKing.Player.BodyComp.CapPosition(bool)"/> and allows us tto check if the function modified the player Y for a teleport
        /// </summary>
        public static void PostfixCapPositionMethod(object __instance, bool p_capped_x)
        {
            if (positionField != null && bodyCompInstance != null)
            {
                float cappedYLocation = ((Vector2)positionField.GetValue(bodyCompInstance)).Y;
                if (cappedYLocation != preCapYLocation)
                {
                    instance?.OnPlayerYTeleported?.Invoke(preCapYLocation - cappedYLocation);
                }
            }
        }

        /// <summary>
        /// Returns the cached player state, or null if it hasnt been polled yet
        /// </summary>
        public PunishmentPlayerState GetPlayerState()
        {
            if (isOnGroundMethod != null &&
                isOnSandMethod != null &&
                velocityField != null &&
                positionField != null &&
                knockedField != null &&
                bodyCompInstance != null)
            {
                bool isOnGround = (bool)isOnGroundMethod.Invoke(bodyCompInstance, null);
                bool isOnSand = (bool)isOnSandMethod.Invoke(bodyCompInstance, null);
                Vector2 velocity = (Vector2)velocityField.GetValue(bodyCompInstance);
                Vector2 position = (Vector2)positionField.GetValue(bodyCompInstance);
                bool knocked = (bool)knockedField.GetValue(bodyCompInstance);
                PunishmentPlayerState state = new PunishmentPlayerState(isOnGround, isOnSand, velocity, position, knocked);
                return state;
            }
            else
            {
                return null;
            }
        }
    }
}
