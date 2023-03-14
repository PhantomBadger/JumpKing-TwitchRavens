using HarmonyLib;
using JumpKingModifiersMod.API;
using Logging.API;
using Microsoft.Xna.Framework;
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
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IPlayerStateAccessor"/> to keep track of the player state
    /// </summary>
    public class PlayerStateObserverManualPatch : IManualPatch, IPlayerStateAccessor
    {
        private static ILogger logger;

        private static FieldInfo isOnGroundField;
        private static FieldInfo velocityField;
        private static FieldInfo knockedField;
        private static object bodyCompInstance;
        private static bool knockedOverrideIsActive;
        private static bool knockedOverrideValue;
        private static bool directionOverrideIsActive;
        private static int directionOverrideValue;

        private static PlayerState prevPlayerState;

        /// <summary>
        /// Ctor for creating a <see cref="PlayerStateObserverManualPatch"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> implementation to use</param>
        public PlayerStateObserverManualPatch(ILogger logger)
        {
            PlayerStateObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            PlayerStateObserverManualPatch.prevPlayerState = null;

            knockedOverrideIsActive = false;
            knockedOverrideValue = false;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var bodyCompMethod = AccessTools.Method("JumpKing.Player.BodyComp:Update");
            var postfixBodyCompMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixBodyCompPatchMethod");
            harmony.Patch(bodyCompMethod, new HarmonyMethod(postfixBodyCompMethod));

            var playerEntityMethod = AccessTools.Method("JumpKing.Player.PlayerEntity:SetDirection");
            var prefixPlayerEntityMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixPlayerEntityPatchMethod");
            harmony.Patch(playerEntityMethod, prefix: new HarmonyMethod(prefixPlayerEntityMethod));
        }

        /// <summary>
        /// Runs after <see cref="JumpKing.Player.BodyComp.Update(float)"/> and allows us to track the state of the player
        /// </summary>
        public static void PostfixBodyCompPatchMethod(object __instance, float p_delta)
        {
            bodyCompInstance = __instance;
            if (isOnGroundField == null)
            {
                isOnGroundField = AccessTools.Field(bodyCompInstance.GetType(), "_is_on_ground");
            }
            if (velocityField == null)
            {
                velocityField = AccessTools.Field(bodyCompInstance.GetType(), "velocity");
            }
            if (knockedField == null)
            {
                knockedField = AccessTools.Field(bodyCompInstance.GetType(), "_knocked");
            }
            bool isOnGroundValue = (bool)isOnGroundField.GetValue(bodyCompInstance);
            Vector2 velocity = (Vector2)velocityField.GetValue(bodyCompInstance);
            bool knocked = (bool)knockedField.GetValue(bodyCompInstance);
            PlayerState state = new PlayerState(isOnGroundValue, velocity, knocked);

            if (knockedOverrideIsActive)
            {
                knockedField.SetValue(bodyCompInstance, knockedOverrideValue);
            }

            prevPlayerState = state;
        }

        /// <summary>
        /// Runs before 'JumpKing.Player.PlayerEntity:SetDirection' and allows us to override the direction
        /// </summary>
        public static void PrefixPlayerEntityPatchMethod(object __instance, ref int p_direction)
        {
            if (directionOverrideIsActive)
            {
                p_direction = directionOverrideValue;
            }
        }

        /// <summary>
        /// Returns the cached player state, or null if it hasnt been polled yet
        /// </summary>
        public PlayerState GetPlayerState()
        {
            return prevPlayerState;
        }

        /// <inheritdoc/>
        public void SetKnockedStateOverride(bool isActive, bool newState)
        {
            knockedOverrideIsActive = isActive;
            knockedOverrideValue = newState;
        }

        /// <inheritdoc/>
        public void SetDirectionOverride(bool isActive, int newDirection)
        {
            directionOverrideIsActive = isActive;
            directionOverrideValue = newDirection;
        }
    }
}
