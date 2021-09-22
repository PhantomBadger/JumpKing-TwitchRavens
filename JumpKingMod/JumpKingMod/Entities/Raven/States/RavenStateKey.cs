using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven.States
{
    /// <summary>
    /// Enum to act as a shorthand for the available States
    /// In the long term this should be replaced by giving the State objects 
    /// full control over how they can change states and to where
    /// </summary>
    public enum RavenStateKey
    {
        Idle,
        Flying,
    }
}
