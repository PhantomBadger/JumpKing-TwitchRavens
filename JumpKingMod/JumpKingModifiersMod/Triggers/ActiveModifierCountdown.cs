using JumpKingModifiersMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// A class representing a <see cref="IModifier"/> and an appropriate countdown
    /// </summary>
    public class ActiveModifierCountdown
    {
        public IModifier Modifier { get; private set; }
        public float DurationCounter { get; private set; }

        public ActiveModifierCountdown(IModifier modifier, float maxDuration)
        {
            Modifier = modifier;
            DurationCounter = maxDuration;
        }

        public float DecreaseCounter(float durationToAdd)
        {
            return DurationCounter -= durationToAdd;
        }
    }
}
