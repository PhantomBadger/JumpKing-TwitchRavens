using JumpKingModifiersMod.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    public interface IMetaModifier : IModifier
    {
        PollTrigger PollTrigger { get; set; }
    }
}
