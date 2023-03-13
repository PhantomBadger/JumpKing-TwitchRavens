using JumpKingModifiersMod.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    public interface IPlayerJumper
    {
        JumpState GetPreviousJumpState();

        void RequestJump(JumpState jumpState);
    }
}
