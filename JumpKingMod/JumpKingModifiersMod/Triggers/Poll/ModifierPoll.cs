using JumpKingModifiersMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// A class representing a poll of modifiers to be voted on by twitch chat.
    /// Represents a Snapshot of the data, and is often updated by other classes
    /// </summary>
    public class ModifierPoll
    {
        /// <summary>
        /// A read-only list of Twitch choices
        /// </summary>
        public IReadOnlyDictionary<int, ModifierPollOption> Choices
        {
            get
            {
                return choices;
            }
        }
        private Dictionary<int, ModifierPollOption> choices;

        /// <summary>
        /// How long is left on th poll in seconds
        /// </summary>
        public float TimeRemainingInSeconds
        {
            get; set;
        }

        private ModifierPollOption winningOption;
        private readonly Random random;

        /// <summary>
        /// Ctor for creating a <see cref="ModifierPoll"/>
        /// </summary>
        /// <param name="choices">A list of <see cref="IModifier"/> implementations to feed into the poll</param>
        public ModifierPoll(List<IModifier> choices)
        {
            this.choices = new Dictionary<int, ModifierPollOption>();
            winningOption = null;
            for (int i = 0; i < choices.Count; i++)
            {
                var option = new ModifierPollOption((i + 1), choices[i]);
                this.choices.Add(option.ChoiceNumber, option);
            }

            random = new Random();
        }

        /// <summary>
        /// Return the <see cref="ModifierPollOption"/> with the most votes. Null if there are no choices.
        /// </summary>
        public ModifierPollOption FindWinningModifier()
        {
            if (choices.Count == 0)
            {
                return null;
            }

            if (this.winningOption != null)
            {
                return this.winningOption;
            }

            Dictionary<int, ModifierPollOption> choicesCopy = new Dictionary<int, ModifierPollOption>(choices);
            int index = random.Next(choicesCopy.Count);

            ModifierPollOption winningOption = choicesCopy.ElementAt(index).Value;
            foreach (var choice in choicesCopy)
            {
                if (choice.Value.Count > winningOption.Count)
                {
                    winningOption = choice.Value;
                }
            }
            this.winningOption = winningOption;
            return this.winningOption;
        }
    }
}
