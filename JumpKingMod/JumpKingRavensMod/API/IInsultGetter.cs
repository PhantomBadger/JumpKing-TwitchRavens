using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.API
{
    /// <summary>
    /// Interface representing an object which can produce an insult
    /// </summary>
    public interface IInsultGetter
    {
        /// <summary>
        /// Gets an insult for the user
        /// </summary>
        string GetInsult();
    }
}
