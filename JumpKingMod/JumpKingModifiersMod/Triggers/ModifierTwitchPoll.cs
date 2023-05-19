using JumpKingModifiersMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    public class ModifierTwitchPoll
    {
        public IReadOnlyList<IModifier> Choices
        {
            get
            {
                return choices.AsReadOnly();
            }
        }
        private List<IModifier> choices;

        public ModifierTwitchPoll(List<IModifier> choices)
        {
            this.choices = choices ?? throw new ArgumentNullException(nameof(choices));
        }

        public IModifier FindWinningModifier()
        {
            // TODO: Sort this
            return choices[0];
        }
    }
}
