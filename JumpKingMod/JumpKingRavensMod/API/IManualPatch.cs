using HarmonyLib;
using Logging.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Patching
{
    /// <summary>
    /// Interface representing a manual patch of some game code using Harmony
    /// </summary>
    public interface IManualPatch
    {
        /// <summary>
        /// Sets up the manual patching using the harmony instance provided
        /// </summary>
        void SetUpManualPatch(Harmony harmony);
    }
}
