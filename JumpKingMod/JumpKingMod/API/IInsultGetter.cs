using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.API
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
