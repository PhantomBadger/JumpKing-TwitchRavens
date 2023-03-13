using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Patching
{
    /// <summary>
    /// An aggregate class of jump data
    /// </summary>
    public class JumpState
    {
        /// <summary>
        /// The intensity of the previous jump
        /// </summary>
        public float Intensity { get; private set; }

        /// <summary>
        /// The X Value of the previous jump
        /// </summary>
        public int XValue { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="JumpState"/>
        /// </summary>
        public JumpState(float intensity, int xValue)
        {
            Intensity = intensity;
            XValue = xValue;
        }
    }
}
