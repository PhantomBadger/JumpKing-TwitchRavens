using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.API
{
    /// <summary>
    /// An interface representing a component attached to an <see cref="IModEntity"/>
    /// </summary>
    public interface IModComponent
    {
        void Update(float delta);
    }
}
