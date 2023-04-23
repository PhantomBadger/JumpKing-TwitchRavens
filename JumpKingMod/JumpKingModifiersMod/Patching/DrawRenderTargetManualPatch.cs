using HarmonyLib;
using Logging.API;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JumpKingModifiersMod.API;

namespace JumpKingModifiersMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> and <see cref="IGameRectFlipper"/> which lets us 
    /// potentially flip the render target
    /// </summary>
    public class DrawRenderTargetManualPatch : IManualPatch, IGameRectFlipper
    {
        private static bool shouldFlipGameRect;

        /// <inheritdoc/>
        public void SetUpManualPatch(Harmony harmony)
        {
            var getGameRectMethod = AccessTools.Method("JumpKing.Game1:GetGameRect");
            var modGetGameRectMethod = this.GetType().GetMethod("PostfixGetGameRect");

            harmony.Patch(getGameRectMethod, postfix: new HarmonyMethod(modGetGameRectMethod));
        }

        /// <summary>
        /// Called after 'JumpKing.Game1:GetGameRect' optionally flipping the result
        /// </summary>
        public static void PostfixGetGameRect(ref Rectangle __result)
        {
            if (shouldFlipGameRect)
            {
                var flippedRect = new Rectangle(new Point(__result.Right, __result.Bottom), new Point(-__result.Width, -__result.Height));
                __result = flippedRect;
            }
        }

        /// <inheritdoc/>
        public void SetShouldFlipGameRect(bool shouldFlip)
        {
            shouldFlipGameRect = shouldFlip;
        }

        /// <inheritdoc/>
        public bool IsGameRectFlipped()
        {
            return shouldFlipGameRect;
        }
    }
}
