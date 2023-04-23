using HarmonyLib;
using JumpKing;
using JumpKingModifiersMod.API;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IWindObserver"/> to allow access to the level's wind state
    /// </summary>
    public class WindObserverManualPatch : IManualPatch, IWindObserver
    {
        private static ILogger logger;
        private static bool isWindOverridden;
        private static Texture2D overrideMask;
        private static Texture2D overrideTexture;

        /// <summary>
        /// Ctor for creating a <see cref="WindObserverManualPatch"/>
        /// </summary>
        /// <param name="logger">An implementation of <see cref="ILogger"/> for logging</param>
        public WindObserverManualPatch(ILogger logger)
        {
            WindObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            isWindOverridden = false;

            overrideMask = new Texture2D(JumpKing.Game1.graphics.GraphicsDevice, 1, 1);
            overrideMask.SetData(new Color[] { Color.Black });
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var windMethod = AccessTools.Method("JumpKing.Level.LevelScreen:get_WindEndabled"); // This is spelt 'Endabled' in the game its not a typo on my end...
            var postfixWindMethod = this.GetType().GetMethod("PostfixGetWindEnabled");
            harmony.Patch(windMethod, postfix: new HarmonyMethod(postfixWindMethod));

            var weatherManagerUpdateMethod = AccessTools.Method("JumpKing.WeatherManager:Update");
            var postfixWeatherManagerUpdateMethod = this.GetType().GetMethod("PostfixWeatherManagerUpdate");
            harmony.Patch(weatherManagerUpdateMethod, postfix: new HarmonyMethod(postfixWeatherManagerUpdateMethod));

            var drawWeatherMethod = AccessTools.Method("JumpKing.Level.LevelScreen:DrawWeather");
            var prefixDrawWeatherMethod = this.GetType().GetMethod("PrefixLevelScreenDrawWeather");
            harmony.Patch(drawWeatherMethod, prefix: new HarmonyMethod(prefixDrawWeatherMethod));
            
        }

        /// <summary>
        /// Called after 'JumpKing.Level.LevelScreen:get_WindEnabled' and optionally overrides the result
        /// </summary>
        public static void PostfixGetWindEnabled(object __instance, ref bool __result)
        {
            if (isWindOverridden)
            {
                __result = true;
            }
        }

        /// <summary>
        /// Called after 'JumpKing.WeatherManager:Update' and pulls out the Instance list, trawling it to find the weather texture we will use for our wind
        /// </summary>
        public static void PostfixWeatherManagerUpdate(object __instance)
        {
            if (overrideTexture == null)
            {
                FieldInfo weatherInstanceListField = AccessTools.Field(__instance.GetType(), "m_instances");
                List<WeatherInstance> weatherInstances = (List<WeatherInstance>)weatherInstanceListField.GetValue(__instance);

                // Iterate through the instances to find the wind
                for (int i = 0; i < weatherInstances.Count; i++)
                {
                    if (weatherInstances[i].Info.name.Equals("light_snow", StringComparison.OrdinalIgnoreCase))
                    {
                        overrideTexture = weatherInstances[i].GetTexture();
                        return;
                    }
                }

                logger.Warning($"The specified wind texture could not be found, no visual will be present for the wind!");
            }
        }

        /// <summary>
        /// Called before 'JumpKing.Level.LevelScreen:DrawWeather' and optionally replaces the method with a draw call to our found wind texture
        /// </summary>
        public static bool PrefixLevelScreenDrawWeather(object __instance)
        {
            if (isWindOverridden && overrideTexture != null)
            {
                JKContentManager.Shaders.Mask.Draw(overrideTexture, overrideMask);
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public bool GetWindOverrideState()
        {
            return isWindOverridden;
        }

        /// <inheritdoc/>
        public void SetWindOverrideState(bool shouldOverrideWind)
        {
            isWindOverridden = shouldOverrideWind;
        }
    }
}
