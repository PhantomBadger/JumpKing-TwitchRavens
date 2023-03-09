using JumpKingRavensMod.API;
using JumpKingRavensMod.Components;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Entities.Raven.States
{
    /// <summary>
    /// An implementation of <see cref="IModEntityState"/> which represents the 
    /// <see cref="RavenEntity"/> currently being idle
    /// </summary>
    public class RavenIdleState : IModEntityState
    {
        private readonly RavenEntity raven;
        private readonly LoopingAnimationComponent idleAnimation;
        public IModEntityState TransitionToState;

        /// <summary>
        /// Constructor for creating a <see cref="RavenIdleState"/>
        /// </summary>
        public RavenIdleState(RavenEntity raven, LoopingAnimationComponent idleAnimation)
        {
            this.raven = raven ?? throw new ArgumentNullException(nameof(raven));
            this.idleAnimation = idleAnimation ?? throw new ArgumentNullException(nameof(idleAnimation));
        }

        /// <summary>
        /// Evlauates the current state, returning a new state if it changes
        /// </summary>
        /// <returns></returns>
        public bool EvaluateState(out IModEntityState nextState)
        {
            nextState = null;
            if (TransitionToState == null)
            {
                return false;
            }

            if (raven.Velocity.Length() > float.Epsilon)
            {
                nextState = TransitionToState;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when entering the state, sets the correct animation
        /// </summary>
        public void Enter()
        {
            raven.SetLoopingAnimation(idleAnimation);
        }

        /// <summary>
        /// Called when exiting the state, resets the current animation
        /// </summary>
        public void Exit()
        {
            idleAnimation.ResetAnimation();
        }
    }
}
