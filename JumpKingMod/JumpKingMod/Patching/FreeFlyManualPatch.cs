using HarmonyLib;
using Logging.API;
using System;
using System.Reflection;
using System.Windows.Input;
using JumpKing.Util;
using Microsoft.Xna.Framework;
using JumpKing;
using EntityComponent;
using JumpKingMod.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JumpKingMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> which enables free flying for the player
    /// </summary>
    public class FreeFlyManualPatch : IManualPatch
    {
        private static bool freeFlying = false;
        private static bool freeFlyingToggleCooldown = false;
        private static ILogger logger;
        private static UITextEntity uiEntity;
        private static ModEntityManager modEntityManager;
        private static Random random;
        private static DateTime lastSpawnedTime;

        private const int millisecondTimeout = 50;

        /// <summary>
        /// Ctor for creating a <see cref="FreeFlyManualPatch"/>
        /// </summary>
        public FreeFlyManualPatch(ModEntityManager newModEntityManager, ILogger newLogger)
        {
            logger = newLogger ?? throw new ArgumentNullException(nameof(newLogger));
            modEntityManager = newModEntityManager ?? throw new ArgumentNullException(nameof(newModEntityManager));
            random = new Random();
            lastSpawnedTime = DateTime.Now;
        }

        /// <summary>
        /// Sets up the manual patch to add <see cref="PrefixPatchMethod(object, float)"/> as the Prefix method for the <see cref="JumpKing.Player.BodyComp.Update(float)"/>
        /// </summary>
        /// <param name="harmony"></param>
        public void SetUpManualPatch(Harmony harmony)
        {
            var method = AccessTools.Method("JumpKing.Player.BodyComp:Update");
            var prefixMethod = AccessTools.Method("JumpKingMod.Patching.FreeFlyManualPatch:PrefixPatchMethod");
            harmony.Patch(method, new HarmonyMethod(prefixMethod));
        }

        /// <summary>
        /// Runs before <see cref="JumpKing.Player.BodyComp.Update(float)"/> and allows us to override the velocity of the player after they press 'P' to toggle the free flying mode
        /// </summary>
        public static void PrefixPatchMethod(object __instance, float p_delta)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.P) && !freeFlyingToggleCooldown)
                {
                    freeFlying = !freeFlying;
                    logger.Information($"Setting Free Flying to {freeFlying}");

                    freeFlyingToggleCooldown = true;

                    if (freeFlying)
                    {
                        // Make a UI Object to display our position
                        uiEntity = new UITextEntity(modEntityManager, new Vector2(0, 0), "", Color.White, UITextEntityAnchor.Center);
                    }
                    else
                    {
                        // Clean up our UI Object
                        uiEntity?.Dispose();
                        uiEntity = null;
                    }
                }
                else if (Keyboard.IsKeyUp(Key.P) && freeFlyingToggleCooldown)
                {
                    freeFlyingToggleCooldown = false;
                }

                // We are free flying
                if (freeFlying)
                {
                    FieldInfo positionField = AccessTools.Field(__instance.GetType(), "position");
                    FieldInfo velocityField = AccessTools.Field(__instance.GetType(), "velocity");
                    Vector2 velocity = (Vector2)velocityField.GetValue(__instance);
                    Vector2 position = (Vector2)positionField.GetValue(__instance);
                    float curX = velocity.X;
                    float curY = velocity.Y;

                    curX = 0;
                    curY = 0;

                    // Dumb joke code that insults you whenever you go down
                    //if (curY > 0.5)
                    //{
                    //    DateTime curTime = DateTime.Now;
                    //    if ((curTime - lastSpawnedTime).TotalMilliseconds > millisecondTimeout)
                    //    {
                    //        UITextEntity tempEntity = new UITextEntity(modEntityManager, new Vector2(random.Next(10, 400), random.Next(10, 400)), "OMEGADOWN", Color.White, UITextEntityAnchor.BottomLeft);
                    //        Task.Run(() =>
                    //        {
                    //            Task.Delay(1000).Wait();
                    //            tempEntity.Dispose();
                    //        });
                    //        lastSpawnedTime = curTime;
                    //    }
                    //}

                    // Modify velocity if key is held
                    if (Keyboard.IsKeyDown(Key.W))
                    {
                        curY -= 5;
                    }
                    if (Keyboard.IsKeyDown(Key.A))
                    {
                        curX -= 5;
                    }
                    if (Keyboard.IsKeyDown(Key.D))
                    {
                        curX += 5;
                    }
                    if (Keyboard.IsKeyDown(Key.S))
                    {
                        curY += 5;
                    }

                    velocity.X = curX;
                    velocity.Y = curY;
                    velocityField.SetValue(__instance, velocity);
                    logger.Information($"Setting velocity to {velocity.ToString()}");

                    uiEntity.ScreenSpacePosition = Camera.TransformVector2(position + new Vector2(0, -50f));
                    uiEntity.TextValue = $"({position.X}, {position.Y})";
                }
            }
            catch (Exception e)
            {
                logger.Error($"Exception on UpdatePatch {e.ToString()}");
            }
        }
    }
}
