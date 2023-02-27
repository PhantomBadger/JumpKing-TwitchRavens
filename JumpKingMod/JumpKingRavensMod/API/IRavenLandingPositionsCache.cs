using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.API
{
    /// <summary>
    /// Interface representing a cache of landing positions for the raven
    /// </summary>
    public interface IRavenLandingPositionsCache
    {
        /// <summary>
        /// Gets a list of possible floor positions for the raven to land on
        /// </summary>
        /// <param name="screenIndex">The current screen index to get values for</param>
        List<Vector2> GetPossibleFloorPositions(int screenIndex);

        /// <summary>
        /// Invalidates the cache for the specified screen
        /// </summary>
        void InvalidateCache(int screenIndex);

        /// <summary>
        /// Invalidates the entire cache
        /// </summary>
        void InvalidateCache();
    }
}
