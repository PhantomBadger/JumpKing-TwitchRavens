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
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IPlayerStateGetter"/> to keep track of the player state
    /// </summary>
    public class PlayerStateObserverManualPatch : IManualPatch, IPlayerStateGetter
    {
        private static ILogger logger;

        private static FieldInfo isOnGroundField;

        private static PlayerState prevPlayerState;

        /// <summary>
        /// Ctor for creating a <see cref="PlayerStateObserverManualPatch"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> implementation to use</param>
        public PlayerStateObserverManualPatch(ILogger logger)
        {
            PlayerStateObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            PlayerStateObserverManualPatch.prevPlayerState = null;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var method = AccessTools.Method("JumpKing.Player.BodyComp:Update");
            var postfixMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixPatchMethod");
            harmony.Patch(method, new HarmonyMethod(postfixMethod));
        }

        /// <summary>
        /// Runs after <see cref="JumpKing.Player.BodyComp.Update(float)"/> and allows us to track the state of the player
        /// </summary>
        public static void PostfixPatchMethod(object __instance, float p_delta)
        {
            if (isOnGroundField == null)
            {
                isOnGroundField = AccessTools.Field(__instance.GetType(), "_is_on_ground");
            }
            bool isOnGroundValue = (bool)isOnGroundField.GetValue(__instance);
            PlayerState state = new PlayerState(isOnGroundValue);

            prevPlayerState = state;
        }

        /// <summary>
        /// Returns the cached player state, or null if it hasnt been polled yet
        /// </summary>
        public PlayerState GetPlayerState()
        {
            return prevPlayerState;
        }
    }
}
