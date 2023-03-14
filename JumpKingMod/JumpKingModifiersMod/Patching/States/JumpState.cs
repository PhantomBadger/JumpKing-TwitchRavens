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
        /// <param name="intensity">The intensity of the jump</param>
        /// <param name="xValue">The X direction to jump in, 1 is right, -1 is left, 0 is up</param>
        public JumpState(float intensity, int xValue)
        {
            Intensity = intensity;
            XValue = xValue;
        }
    }

    /// <summary>
    /// An aggregate of jump data for a requested jump
    /// </summary>
    public class RequestedJumpState : JumpState
    {
        /// <summary>
        /// If <c>true</c> then the X Value provided will be overridden by any user input as long
        /// as a direction is being held, otherwise it will use the <see cref="base.XValue"/>
        /// </summary>
        public bool OverrideXValueWithUserInput { get; private set; }

        /// <summary>
        /// If <c>true</c> then the player's direction will be set to the opposite of the X value used
        /// Used to better simulate a "knock" state
        /// </summary>
        public bool OverrideDirectionToOppositeX { get; private set; }

        /// <summary>
        /// Ctor for creating a <see cref="RequestedJumpState"/>
        /// </summary>
        /// <param name="intensity">The intensity of the jump</param>
        /// <param name="xValue">The X direction to jump in, 1 is right, -1 is left, 0 is up</param>
        /// <param name="overrideXValue">If <c>true</c> then the X Value provided will be overridden by any user input</param>
        /// <param name="overrideDirectionOpposite">If <c>true</c> then the player's direction will be set to the opposite of the X value used</param>
        public RequestedJumpState(float intensity, int xValue, bool overrideXValue, bool overrideDirectionOpposite) : base(intensity, xValue)
        {
            OverrideXValueWithUserInput = overrideXValue;
            OverrideDirectionToOppositeX = overrideDirectionOpposite;
        }
    }
}
