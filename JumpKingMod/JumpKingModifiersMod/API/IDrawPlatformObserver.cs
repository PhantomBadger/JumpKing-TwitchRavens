using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.API
{
    /// <summary>
    /// An interface representing an object able to set and observe an override state for whether to draw the platforms
    /// </summary>
    public interface IDrawPlatformObserver
    {
        /// <summary>
        /// Sets whether we override the 'Draw Platform' logic
        /// </summary>
        void SetDrawPlatformOverride(bool drawPlatformOverride);

        /// <summary>
        /// Gets whether we are currently overriding the 'Draw Platform' logic
        /// </summary>
        bool GetDrawPlatformOverride();
    }
}
