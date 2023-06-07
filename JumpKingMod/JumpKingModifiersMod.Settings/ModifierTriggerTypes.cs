using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Settings
{
    /// <summary>
    /// An enum representing known trigger types
    /// </summary>
    public enum ModifierTriggerTypes
    {
        /// <summary>
        /// Manually toggle the enabled modifiers
        /// </summary>
        Toggle,

        /// <summary>
        /// The modifiers will be enabled pending a regular poll in twitch chat
        /// in a 'chaos mod' style
        /// </summary>
        TwitchPoll,

        /// <summary>
        /// No trigger/modifiers active at all
        /// </summary>
        None,
    }
}
