using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HarmonyLib;
using Logging;
using Logging.API;

namespace JumpKingMod
{
    public class JumpKingModEntry
    {
        public static ILogger Logger;
        private static bool freeFlying = false;
        private static bool freeFlyingToggleCooldown = false;

        public static void Init()
        {
            Logger = new ConsoleLogger();

            try
            {
                Harmony harmony = new Harmony("com.phantombadger.jumpkingmod");
                harmony.PatchAll();

                var method = AccessTools.Method("JumpKing.Player.BodyComp:Update");
                var prefixMethod = AccessTools.Method("JumpKingMod.JumpKingModEntry:TestUpdatePatch");
                harmony.Patch(method, new HarmonyMethod(prefixMethod));
            }
            catch (Exception e)
            {
                Logger.Error($"Error on Init {e.ToString()}");
            }

            Logger.Information("Init Called!");
        }

        public static void TestUpdatePatch(object __instance, float p_delta)
        {
            try
            {
                if (Keyboard.IsKeyDown(Key.P) && !freeFlyingToggleCooldown)
                {
                    freeFlying = !freeFlying;
                    Logger.Information($"Setting Free Flying to {freeFlying}");

                    freeFlyingToggleCooldown = true;
                }
                else if (Keyboard.IsKeyUp(Key.P) && freeFlyingToggleCooldown)
                {
                    freeFlyingToggleCooldown = false;
                }

                // We are free flying
                if (freeFlying)
                {
                    FieldInfo velocityField = AccessTools.Field(__instance.GetType(), "velocity");
                    object velocity = velocityField.GetValue(__instance);
                    FieldInfo yField = AccessTools.Field(velocityField.FieldType, "Y");
                    FieldInfo xField = AccessTools.Field(velocityField.FieldType, "X");
                    float curX = (float)xField.GetValue(velocity);
                    float curY = (float)yField.GetValue(velocity);

                    curX = 0;
                    curY = 0;
                    
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

                    yField.SetValue(velocity, curY);
                    xField.SetValue(velocity, curX);
                    velocityField.SetValue(__instance, velocity);
                    Logger.Information($"Setting velocity to {velocity.ToString()}");
                }
            } 
            catch (Exception e)
            {
                Logger.Error($"Exception on UpdatePatch {e.ToString()}");
            }
        }
    }

    [HarmonyPatch(typeof(JumpKing.Game1))]
    [HarmonyPatch("Initialize")]
    public class GamePatch
    {
        static bool Prefix(JumpKing.Game1 __instance)
        {
            JumpKingModEntry.Logger.Information($"Prefix Called!");
            return true;
        }

        static void Postfix()
        {
            JumpKingModEntry.Logger.Information($"Postfix Called!");
        }
    }
}
