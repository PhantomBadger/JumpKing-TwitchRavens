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
    /// An implementation of <see cref="IManualPatch"/> to optionally override the DrawForeground method
    /// </summary>
    public class DrawPlatformsObserverManualPatch : IManualPatch, IDrawPlatformObserver
    {
        private static ILogger logger;
        private static bool isOverridingDrawPlatform;

        /// <summary>
        /// Ctor for creating a <see cref="DrawPlatformsObserverManualPatch"/>
        /// </summary>
        public DrawPlatformsObserverManualPatch(ILogger logger)
        {
            DrawPlatformsObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            isOverridingDrawPlatform = false;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var drawPlatformMethod = AccessTools.Method("JumpKing.Level.LevelScreen:DrawMG");
            var prefixDrawPlatformMethod = this.GetType().GetMethod("PrefixDrawMG");
            harmony.Patch(drawPlatformMethod, prefix: new HarmonyMethod(prefixDrawPlatformMethod));
        }

        /// <summary>
        /// Called before 'JumpKing.Level.LevelScreen:DrawForeground' and optionally prevents it running
        /// </summary>
        public static bool PrefixDrawMG(object __instance)
        {
            if (isOverridingDrawPlatform)
            {
                // TODO: Figure out how to get weather to still draw?
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public void SetDrawPlatformOverride(bool drawPlatformOverride)
        {
            isOverridingDrawPlatform = drawPlatformOverride;
        }

        /// <inheritdoc/>
        public bool GetDrawPlatformOverride()
        {
            return isOverridingDrawPlatform;
        }
    }
}
