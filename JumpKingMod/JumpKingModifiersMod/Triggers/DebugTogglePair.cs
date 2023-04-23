using JumpKingModifiersMod.API;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// An aggregate class grouping a Modifier to activate and a Key to activate it with
    /// </summary>
    public class DebugTogglePair
    {
        public IModifier Modifier { get; private set; }
        public Keys ToggleKey { get; private set; }
        public bool ToggleKeyReset { get; set; }

        /// <summary>
        /// Ctor for creating a <see cref="DebugTogglePair"/>
        /// </summary>
        public DebugTogglePair(IModifier modifier, Keys toggleKey)
        {
            this.Modifier = modifier;
            this.ToggleKey = toggleKey;
            ToggleKeyReset = true;
        }
    }
}
