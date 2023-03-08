using HarmonyLib;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching
{
    public class PlayerValuesManualPatch : IManualPatch
    {
        private const float DefaultSpeed = 7f;
        private const float DefaultBounce = 0.5f;
        private const float DefaultJumpTime = 0.6f;

        public void SetUpManualPatch(Harmony harmony)
        {
            // TODO: patch static getters for player values
        }
    }
}
