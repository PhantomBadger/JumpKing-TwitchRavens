using BehaviorTree;
using HarmonyLib;
using JumpKing;
using JumpKing.Player;
using JumpKing.SaveThread;
using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Patching.States;
using Logging.API;
using Microsoft.Xna.Framework;
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
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IPlayerStateObserver"/> to keep track of the player state
    /// </summary>
    public class PlayerStateObserverManualPatch : IManualPatch, IPlayerStateObserver
    {
        private static ILogger logger;
        private static MethodInfo isOnGroundMethod;
        private static MethodInfo isOnSnowMethod;
        private static MethodInfo isInWaterMethod;
        private static MethodInfo getHitboxMethod;
        private static MethodInfo getCurrentScreenMethod;
        private static MethodInfo getCanTeleportMethod;
        private static FieldInfo velocityField;
        private static FieldInfo knockedField;
        private static FieldInfo positionField;
        private static FieldInfo leftField;
        private static FieldInfo rightField;
        private static FieldInfo jumpField;
        private static object bodyCompInstance;
        private static PlayerEntity playerEntityInstance;
        private static bool knockedOverrideIsActive;
        private static bool knockedOverrideValue;
        private static bool directionOverrideIsActive;
        private static int directionOverrideValue;
        private static bool isWalkingDisabled;
        private static bool isXVelocityDisabled;
        private static bool isInputInverted;
        private static bool isDrawDisabled;
        private static bool isBodyCompDisabled;
        private static InputState prevInputState;
        private static ConcurrentQueue<int> preTeleportIndexQueue;
        private static PlayerStateObserverManualPatch instance;

        public event OnPlayerTeleportedDelegate OnPlayerTeleported;

        /// <summary>
        /// Ctor for creating a <see cref="PlayerStateObserverManualPatch"/>
        /// </summary>
        /// <param name="logger">The <see cref="ILogger"/> implementation to use</param>
        public PlayerStateObserverManualPatch(ILogger logger)
        {
            PlayerStateObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            knockedOverrideIsActive = false;
            knockedOverrideValue = false;
            isWalkingDisabled = false;
            isXVelocityDisabled = false;
            isInputInverted = false;
            preTeleportIndexQueue = new ConcurrentQueue<int>();
            instance = this;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var bodyCompMethod = AccessTools.Method("JumpKing.Player.BodyComp:Update");
            var prefixBodyCompMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixBodyCompPatchMethod");
            var postfixBodyCompMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixBodyCompPatchMethod");
            harmony.Patch(bodyCompMethod, prefix: new HarmonyMethod(prefixBodyCompMethod), postfix: new HarmonyMethod(postfixBodyCompMethod));

            var playerEntitySetDirectionMethod = AccessTools.Method("JumpKing.Player.PlayerEntity:SetDirection");
            var prefixPlayerSetDirectionEntityMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixPlayerEntityPatchMethod");
            harmony.Patch(playerEntitySetDirectionMethod, prefix: new HarmonyMethod(prefixPlayerSetDirectionEntityMethod));

            var playerEntityUpdateMethod = AccessTools.Method("JumpKing.Player.PlayerEntity:Update");
            var postfixPlayerEntityUpdateMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixPlayerEntityUpdatePatchMethod");
            harmony.Patch(playerEntityUpdateMethod, postfix: new HarmonyMethod(postfixPlayerEntityUpdateMethod));

            var walkComponentMyRunMethod = AccessTools.Method("JumpKing.Player.Walk:MyRun");
            var prefixWalkComponentMyRunMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixWalkComponentMyRunMethod");
            harmony.Patch(walkComponentMyRunMethod, prefix: new HarmonyMethod(prefixWalkComponentMyRunMethod));

            var walkAnimComponentMyRunMethod = AccessTools.Method("JumpKing.Player.WalkAnim:MyRun");
            var prefixWalkAnimComponentMyRunMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixWalkAnimComponentMyRunMethod");
            harmony.Patch(walkAnimComponentMyRunMethod, prefix: new HarmonyMethod(prefixWalkAnimComponentMyRunMethod));

            var inputComponentGetStateMethod = AccessTools.Method("JumpKing.Player.InputComponent:GetState");
            var postfixInputComponentGetStateMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixInputComponentGetStateMethod");
            harmony.Patch(inputComponentGetStateMethod, postfix: new HarmonyMethod(postfixInputComponentGetStateMethod));

            var playerEntityDrawMethod = AccessTools.Method("JumpKing.Player.PlayerEntity:Draw");
            var prefixPlayerEntityDrawMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixPlayerEntityDrawMethod");
            harmony.Patch(playerEntityDrawMethod, prefix: new HarmonyMethod(prefixPlayerEntityDrawMethod));

            getCurrentScreenMethod = AccessTools.Method($"JumpKing.Level.LevelManager:get_CurrentScreen");
            getCanTeleportMethod = AccessTools.Method($"JumpKing.Level.LevelScreen:get_CanTeleport");
            var bodyCompCapPositionMethod = AccessTools.Method("JumpKing.Player.BodyComp:CapPosition");
            var prefixCapPositionMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PrefixCapPositionMethod");
            var postfixCapPositionMethod = AccessTools.Method($"JumpKingModifiersMod.Patching.{this.GetType().Name}:PostfixCapPositionMethod");
            harmony.Patch(bodyCompCapPositionMethod, prefix: new HarmonyMethod(prefixCapPositionMethod), postfix: new HarmonyMethod(postfixCapPositionMethod));
        }

        /// <summary>
        /// Tries to get whether the current screen can teleport
        /// </summary>
        private static bool CurrentScreenCanTeleport()
        {
            if (getCanTeleportMethod == null || getCanTeleportMethod == null)
            {
                return false;
            }
            object currentScreen = getCurrentScreenMethod.Invoke(null, null);
            bool result = (bool)getCanTeleportMethod.Invoke(currentScreen, null);
            return result;
        }

        /// <summary>
        /// Called before <see cref="JumpKing.Player.BodyComp.CapPosition"/> and checks to see if we expect it to teleport
        /// recording info if we are
        /// </summary>
        public static void PrefixCapPositionMethod(object __instance)
        {
            if (CurrentScreenCanTeleport())
            {
                if (getHitboxMethod != null && bodyCompInstance != null)
                {
                    Rectangle hitbox = (Rectangle)getHitboxMethod.Invoke(bodyCompInstance, null);
                    if (hitbox.Center.X < 0 || hitbox.Center.X > 480)
                    {
                        preTeleportIndexQueue.Enqueue(Camera.CurrentScreen);
                    }
                }
            }
        }

        /// <summary>
        /// Called after <see cref="JumpKing.Player.BodyComp.CapPosition"/> and records new info if we did teleport
        /// </summary>
        public static void PostfixCapPositionMethod(object __instance)
        {
            if (instance != null && preTeleportIndexQueue != null && preTeleportIndexQueue.TryDequeue(out int preTeleportIndex))
            {
                instance.OnPlayerTeleported?.Invoke(new Teleporting.OnTeleportedEventArgs(preTeleportIndex, Camera.CurrentScreen));
            }
        }

        /// <summary>
        /// Called before 'JumpKing.Player.Walk:MyRun' and optionally skips the method execution
        /// </summary>
        public static bool PrefixWalkComponentMyRunMethod(ref BTresult __result, TickData p_data)
        {
            // If we want to disable walking then exit early
            if (isWalkingDisabled)
            {
                if (velocityField != null && bodyCompInstance != null && isXVelocityDisabled)
                {
                    Vector2 curVelocity = (Vector2)velocityField.GetValue(bodyCompInstance);
                    velocityField.SetValue(bodyCompInstance, new Vector2(0, curVelocity.Y));
                }

                __result = BTresult.Success;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called before 'JumpKing.Player.WalkAnim:MyRun' and optionally skips the method execution
        /// </summary>
        public static bool PrefixWalkAnimComponentMyRunMethod(ref BTresult __result, TickData p_data)
        {
            // If we want to disable walking then exit early
            if (isWalkingDisabled)
            {
                __result = BTresult.Success;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Called after 'JumpKing.Player.InputComponent:GetState' and records the last received input state
        /// </summary>
        public static void PostfixInputComponentGetStateMethod(ref object __result)
        {
            if (leftField == null)
            {
                leftField = AccessTools.Field(__result.GetType(), "left");
            }
            if (rightField == null)
            {
                rightField = AccessTools.Field(__result.GetType(), "right");
            }
            if (jumpField == null)
            {
                jumpField = AccessTools.Field(__result.GetType(), "jump");
            }
            bool left = (bool)leftField.GetValue(__result);
            bool right = (bool)rightField.GetValue(__result);
            bool jump = (bool)jumpField.GetValue(__result);

            // Invert the input in the result and also for our caching
            if (isInputInverted)
            {
                rightField.SetValue(__result, left);
                leftField.SetValue(__result, right);
                bool tempRight = right;
                right = left;
                left = tempRight;
            }

            prevInputState = new InputState(left, right, jump);
        }

        /// <summary>
        /// Runs before <see cref="JumpKing.Player.BodyComp.Update(float)"/> and optionally prevents it from running
        /// </summary>
        public static bool PrefixBodyCompPatchMethod(object __instance)
        {
            if (isBodyCompDisabled)
            {
                return false;
            }
            return true;
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
            if (isOnSnowMethod == null)
            {
                isOnSnowMethod = AccessTools.Method(bodyCompInstance.GetType(), "get_IsOnSnow");
            }
            if (isInWaterMethod == null)
            {
                isInWaterMethod = AccessTools.Method(bodyCompInstance.GetType(), "get_IsInWater");
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
            if (getHitboxMethod == null)
            {
                getHitboxMethod = AccessTools.Method(bodyCompInstance.GetType(), "GetHitbox");
            }

            if (knockedOverrideIsActive)
            {
                knockedField.SetValue(bodyCompInstance, knockedOverrideValue);
            }
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
        /// Runs after 'JumpKing.Player.PlayerEntity:Update' and records the instance
        /// </summary>
        public static void PostfixPlayerEntityUpdatePatchMethod(object __instance)
        {
            playerEntityInstance = (PlayerEntity)__instance;
        }

        /// <summary>
        /// Runs before 'JumpKing.Player.PlayerEntity:Draw' and optionally prevents it running
        /// </summary>
        public static bool PrefixPlayerEntityDrawMethod(object __instance)
        {
            if (isDrawDisabled)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the cached player state, or null if it hasnt been polled yet
        /// </summary>
        public PlayerState GetPlayerState()
        {
            if (isOnGroundMethod     != null &&
                isOnSnowMethod       != null &&
                isInWaterMethod      != null &&
                velocityField       != null &&
                positionField       != null &&
                knockedField        != null && 
                bodyCompInstance    != null)
            {
                bool isOnGround = (bool)isOnGroundMethod.Invoke(bodyCompInstance, null);
                bool isOnSnow = (bool)isOnSnowMethod.Invoke(bodyCompInstance, null);
                bool isInWater = (bool)isInWaterMethod.Invoke(bodyCompInstance, null);
                Vector2 velocity = (Vector2)velocityField.GetValue(bodyCompInstance);
                Vector2 position = (Vector2)positionField.GetValue(bodyCompInstance);
                bool knocked = (bool)knockedField.GetValue(bodyCompInstance);
                PlayerState state = new PlayerState(isOnGround, isOnSnow, isInWater, velocity, position, knocked);
                return state;
            }
            else
            {
                return null;
            }
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

        /// <inheritdoc/>
        public void RestartPlayerPosition(bool niceSpawns)
        {
            if (playerEntityInstance != null)
            {
                SaveState defaultSaveState = SaveState.GetDefault();
                // If we're in NB+ then we will want to start them at the Imp room
                if (EventFlagsSave.ContainsFlag(StoryEventFlags.StartedNBP) && niceSpawns)
                {
                    logger.Information($"NBP Detected - Resetting to Imp Room");
                    defaultSaveState.position = new Vector2(177f, -15585f);
                }
                else if (EventFlagsSave.ContainsFlag(StoryEventFlags.StartedGhost) && niceSpawns)
                {
                    logger.Information($"GotB Detected - Resetting to bottom of Drop");
                    defaultSaveState.position = new Vector2(227.5f, -57300f);
                }
                else
                { 
                    defaultSaveState.position -= new Vector2(0, 10); // Move the player up a tiny bit, fixes odd issues where we clip into the ground a tad
                    logger.Information($"Applying Default Save State!");
                }
                playerEntityInstance.ApplySaveState(defaultSaveState);
                Camera.UpdateCamera(defaultSaveState.position.ToPoint());
            }
        }

        /// <inheritdoc/>
        public void DisablePlayerWalking(bool isWalkingDisabled, bool isXVelocityDisabled = false)
        {
            PlayerStateObserverManualPatch.isWalkingDisabled = isWalkingDisabled;
            if (isWalkingDisabled)
            {
                PlayerStateObserverManualPatch.isXVelocityDisabled = isXVelocityDisabled;
            }
            else
            {
                PlayerStateObserverManualPatch.isXVelocityDisabled = false;
            }
        }

        /// <inheritdoc/>
        public InputState GetInputState()
        {
            return prevInputState;
        }

        /// <inheritdoc/>
        public void SetInvertPlayerInputs(bool invertPlayerInputs)
        {
            isInputInverted = invertPlayerInputs;
        }

        /// <inheritdoc/>
        public bool GetInvertPlayerInputs()
        {
            return isInputInverted;
        }

        /// <inheritdoc/>
        public Rectangle GetPlayerHitbox()
        {
            if (getHitboxMethod != null && bodyCompInstance != null)
            {
                return (Rectangle)getHitboxMethod.Invoke(bodyCompInstance, null);
            }
            return Rectangle.Empty;
        }

        /// <inheritdoc/>
        public void SetPosition(Vector2 position)
        {
            if (bodyCompInstance != null && positionField != null)
            {
                positionField.SetValue(bodyCompInstance, position);
            }
        }

        /// <inheritdoc/>
        public void DisablePlayerDrawing(bool isDrawDisabled)
        {
            PlayerStateObserverManualPatch.isDrawDisabled = isDrawDisabled;
        }

        public void DisablePlayerBodyComp(bool isBodyCompDisabled)
        {
            PlayerStateObserverManualPatch.isBodyCompDisabled = isBodyCompDisabled;
        }
    }
}
