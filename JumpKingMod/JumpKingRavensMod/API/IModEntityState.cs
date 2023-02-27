using JumpKingRavensMod.Entities.Raven.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.API
{
    public interface IModEntityState
    {
        bool EvaluateState(out IModEntityState nextState);
        void Enter();
        void Exit();
    }
}
