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
    public class ModifierTwitchPoll
    {
        /// <summary>
        /// A read-only list of Twitch choices
        /// </summary>
        public IReadOnlyDictionary<int, ModifierTwitchPollOption> Choices
        {
            get
            {
                return choices;
            }
        }
        private Dictionary<int, ModifierTwitchPollOption> choices;

        /// <summary>
        /// How long is left on th poll in seconds
        /// </summary>
        public float TimeRemainingInSeconds
        {
            get; set;
        }

        private ModifierTwitchPollOption winningOption;
        private readonly Random random;

        /// <summary>
        /// Ctor for creating a <see cref="ModifierTwitchPoll"/>
        /// </summary>
        /// <param name="choices">A list of <see cref="IModifier"/> implementations to feed into the poll</param>
        public ModifierTwitchPoll(List<IModifier> choices)
        {
            this.choices = new Dictionary<int, ModifierTwitchPollOption>();
            winningOption = null;
            for (int i = 0; i < choices.Count; i++)
            {
                var option = new ModifierTwitchPollOption((i + 1), choices[i]);
                this.choices.Add(option.ChoiceNumber, option);
            }

            random = new Random();
        }

        /// <summary>
        /// Return the <see cref="ModifierTwitchPollOption"/> with the most votes. Null if there are no choices.
        /// </summary>
        public ModifierTwitchPollOption FindWinningModifier()
        {
            if (choices.Count == 0)
            {
                return null;
            }

            if (this.winningOption != null)
            {
                return this.winningOption;
            }

            Dictionary<int, ModifierTwitchPollOption> choicesCopy = new Dictionary<int, ModifierTwitchPollOption>(choices);
            int index = random.Next(choicesCopy.Count);

            ModifierTwitchPollOption winningOption = choicesCopy.ElementAt(index).Value;
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
