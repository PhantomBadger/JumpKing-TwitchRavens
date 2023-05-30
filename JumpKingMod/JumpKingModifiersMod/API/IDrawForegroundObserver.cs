using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing an object able to set and observe an override state for whether to draw the foreground
    /// </summary>
    public interface IDrawForegroundObserver
    {
        /// <summary>
        /// Sets whether we override the 'DrawForeground' logic
        /// </summary>
        void SetDrawForegroundOverride(bool drawForegroundOverride);

        /// <summary>
        /// Gets whether we are currently overriding the 'DrawForeground' logic
        /// </summary>
        bool GetDrawForegroundOverride();
    }
}
