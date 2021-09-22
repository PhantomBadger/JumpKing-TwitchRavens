using JumpKingMod.API;
using JumpKingMod.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven.States
{
    /// <summary>
    /// An implementation of <see cref="IModEntityState"/> which represents the 
    /// <see cref="RavenEntity"/> currently flying
    /// </summary>
    public class RavenFlyingState : IModEntityState
    {
        private readonly RavenEntity raven;
        private readonly LoopingAnimationComponent flyingAnimation;

        public RavenStateKey Key => RavenStateKey.Flying;

        /// <summary>
        /// Constructor for creating a <see cref="RavenFlyingState"/>
        /// </summary>
        public RavenFlyingState(RavenEntity raven, LoopingAnimationComponent flyingAnimation)
        {
            this.raven = raven ?? throw new ArgumentNullException(nameof(raven));
            this.flyingAnimation = flyingAnimation ?? throw new ArgumentNullException(nameof(flyingAnimation));
        }

        /// <summary>
        /// Called when entering the state, sets the correct animation
        /// </summary>
        public void Enter()
        {
            raven.SetLoopingAnimation(flyingAnimation);
        }

        /// <summary>
        /// Called when exiting the state, resets the current animation
        /// </summary>
        public void Exit()
        {
            flyingAnimation.ResetAnimation();
        }
    }
}
