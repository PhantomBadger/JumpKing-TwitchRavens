using JumpKingMod.Entities.Raven.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.API
{
    interface IModEntityState
    {
        RavenStateKey Key { get; }

        void Enter();
        void Exit();
    }
}
