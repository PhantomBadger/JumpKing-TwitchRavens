using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching.Teleporting
{
    /// <summary>
    /// An extension of <see cref="EventArgs"/> for Teleported events
    /// </summary>
    public class OnTeleportedEventArgs : EventArgs
    {
        public int PreviousScreenIndex { get; private set; }
        public int NewScreenIndex { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="OnTeleportedEventArgs"/>
        /// </summary>
        public OnTeleportedEventArgs(int prevScreenIndex, int newScreenIndex) : base()
        {
            PreviousScreenIndex = prevScreenIndex;
            NewScreenIndex = newScreenIndex;
        }
    }
}
