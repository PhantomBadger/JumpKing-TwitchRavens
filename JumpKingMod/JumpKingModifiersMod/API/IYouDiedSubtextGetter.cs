using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing an object which can get a subtext entry for the 'You Died' screen
    /// </summary>
    public interface IYouDiedSubtextGetter
    {
        /// <summary>
        /// Gets a subtext value for the 'You Died' screen
        /// </summary>
        string GetYouDiedSubtext();
    }
}
