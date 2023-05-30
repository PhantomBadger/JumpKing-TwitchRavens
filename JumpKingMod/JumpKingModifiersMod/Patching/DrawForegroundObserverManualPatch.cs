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
    public class DrawForegroundObserverManualPatch : IManualPatch, IDrawForegroundObserver
    {
        private static ILogger logger;
        private static bool isOverridingDrawForeground;

        /// <summary>
        /// Ctor for creating a <see cref="DrawForegroundObserverManualPatch"/>
        /// </summary>
        public DrawForegroundObserverManualPatch(ILogger logger)
        {
            DrawForegroundObserverManualPatch.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            isOverridingDrawForeground = false;
        }

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var drawForegroundMethod = AccessTools.Method("JumpKing.Level.LevelScreen:DrawForeground");
            var prefixDrawForegroundMethod = this.GetType().GetMethod("PrefixDrawForeground");
            harmony.Patch(drawForegroundMethod, prefix: new HarmonyMethod(prefixDrawForegroundMethod));
        }

        /// <summary>
        /// Called before 'JumpKing.Level.LevelScreen:DrawForeground' and optionally prevents it running
        /// </summary>
        public static bool PrefixDrawForeground(object __instance)
        {
            if (isOverridingDrawForeground)
            {
                // TODO: Figure out how to get weather to still draw?
                return false;
            }
            return true;
        }

        /// <inheritdoc/>
        public void SetDrawForegroundOverride(bool drawForegroundOverride)
        {
            isOverridingDrawForeground = drawForegroundOverride;
        }

        /// <inheritdoc/>
        public bool GetDrawForegroundOverride()
        {
            return isOverridingDrawForeground;
        }
    }
}
