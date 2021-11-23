using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven.Triggers.EasterEgg
{
    /// <summary>
    /// An aggregate class containing info on the easter egg messages
    /// </summary>
    public class EasterEggMessageInfo
    {
        public string RavenMessage { get; set; }
        public string RavenName { get; set; }
        public Color RavenNameColor { get; set; }
        public uint MessageOdds { get; set; }

        public EasterEggMessageInfo()
        {
            RavenName = "";
            RavenNameColor = Color.White;
            MessageOdds = 1;
        }
    }
}
