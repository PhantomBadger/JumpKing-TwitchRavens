using HarmonyLib;
using JumpKingMod.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Patching
{
    /// <summary>
    /// An implementation of <see cref="IManualPatch"/> which hooks up the ModEntityManager to the appropriate
    /// draw calls
    /// </summary>
    public class ModEntityManagerManualPatch : IManualPatch
    {
        private static ModEntityManager modEntityManager;

        public ModEntityManagerManualPatch(ModEntityManager modEntityManager)
        {
            ModEntityManagerManualPatch.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
        }

        public void SetUpManualPatch(Harmony harmony)
        {
            var entityManagerDrawMethod = AccessTools.Method("EntityComponent.EntityManager:Draw");
            var modEntityManageDrawMethod = this.GetType().GetMethod("DrawWrapper");

            var jumpKingGameDrawMethod = AccessTools.Method("JumpKing.JumpGame:Draw");
            var modEntityForegroundDrawMethod = this.GetType().GetMethod("ForegroundDrawWrapper");

            harmony.Patch(entityManagerDrawMethod, postfix: new HarmonyMethod(modEntityManageDrawMethod));
            harmony.Patch(jumpKingGameDrawMethod, postfix: new HarmonyMethod(modEntityForegroundDrawMethod));
        }

        public static void DrawWrapper()
        {
            modEntityManager?.Draw();
        }

        public static void ForegroundDrawWrapper()
        {
            modEntityManager?.DrawForeground();
        }
    }
}
