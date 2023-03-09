using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing a possible trigger for a modifier
    /// </summary>
    public interface IModifierTrigger
    {
        bool EnableTrigger();

        bool DisableTrigger();

        bool IsTriggerEnabled();
    }
}
