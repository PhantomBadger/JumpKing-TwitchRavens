using JumpKingModifiersMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// A class representing a single option in a <see cref="ModifierTwitchPoll"/>
    /// </summary>
    public class ModifierTwitchPollOption
    {
        /// <summary>
        /// The number of this option in the poll
        /// </summary>
        public int ChoiceNumber { get; private set; }

        /// <summary>
        /// The <see cref="IModifier"/> implementation for this option
        /// </summary>
        public IModifier Modifier { get; private set; }

        /// <summary>
        /// A count of how many times this option has been chosen
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
        }
        private int count;

        /// <summary>
        /// Ctor for creating a <see cref="ModifierTwitchPollOption"/>
        /// </summary>
        /// <param name="choiceNumber">The number of this choice in the poll</param>
        /// <param name="modifier">An implementation of <see cref="IModifier"/></param>
        public ModifierTwitchPollOption(int choiceNumber, IModifier modifier)
        {
            this.ChoiceNumber = choiceNumber;
            this.Modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
        }

        /// <summary>
        /// Increment the <see cref="Count"/> in a thread safe way
        /// </summary>
        public void IncrementCount()
        {
            count = Interlocked.Increment(ref count);
        }
    }
}
