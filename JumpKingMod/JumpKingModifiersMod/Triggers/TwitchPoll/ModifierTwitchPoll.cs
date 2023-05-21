using JumpKingModifiersMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// A class representing a poll of modifiers to be voted on by twitch chat
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
        /// Ctor for creating a <see cref="ModifierTwitchPoll"/>
        /// </summary>
        /// <param name="choices">A list of <see cref="IModifier"/> implementations to feed into the poll</param>
        public ModifierTwitchPoll(List<IModifier> choices)
        {
            this.choices = new Dictionary<int, ModifierTwitchPollOption>();
            for (int i = 0; i < choices.Count; i++)
            {
                var option = new ModifierTwitchPollOption((i + 1), choices[i]);
                this.choices.Add(option.ChoiceNumber, option);
            }
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

            Dictionary<int, ModifierTwitchPollOption> choicesCopy = new Dictionary<int, ModifierTwitchPollOption>(choices);

            ModifierTwitchPollOption winningOption = choicesCopy.First().Value;
            foreach (var choice in choicesCopy)
            {
                if (choice.Value.Count > winningOption.Count)
                {
                    winningOption = choice.Value;
                }
            }
            return winningOption;
        }
    }
}
