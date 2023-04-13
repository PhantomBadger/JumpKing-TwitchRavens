using JumpKing;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Components
{
    /// <summary>
    /// An implementation of <see cref="IModComponent"/> which animates through a list of sprites
    /// on a loop
    /// </summary>
    public class AnimationComponent : IModComponent
    {
        private readonly Sprite[] spriteArray;
        private readonly float animationStep;
        private readonly bool looping;

        private int animationIndex;
        private float elapsedTime;

        /// <summary>
        /// Constructor taking in an array of sprites, and the time to wait between each loop
        /// </summary>
        /// <param name="spriteArray"></param>
        /// <param name="animationStep"></param>
        public AnimationComponent(Sprite[] spriteArray, float animationStep)
            : this (spriteArray, animationStep, looping: true)
        {
        }

        /// <summary>
        /// Constructor taking in an array of sprites, and the time to wait between each loop
        /// </summary>
        /// <param name="spriteArray"></param>
        /// <param name="animationStep"></param>
        /// <param name="looping">If <c>true</c> then the animation will loop, if <c>false</c> it will stop on the last frame</param>
        public AnimationComponent(Sprite[] spriteArray, float animationStep, bool looping)
        {
            this.spriteArray = spriteArray ?? throw new ArgumentNullException(nameof(spriteArray));
            if (spriteArray.Length <= 0)
            {
                throw new InvalidOperationException($"Need sprites to actually do anything");
            }
            this.animationStep = animationStep;
            this.looping = looping;
            animationIndex = 0;
            elapsedTime = 0;
        }

        /// <summary>
        /// Called by the parent entity, iterates through the animation at the set step rate
        /// </summary>
        /// <param name="delta"></param>
        public void Update(float delta)
        {
            if ((elapsedTime += delta) > animationStep)
            {
                int increment = (int)(elapsedTime / animationStep);
                elapsedTime = 0;
                animationIndex += increment;
                if (looping)
                {
                    animationIndex %= spriteArray.Length;
                }
                else
                {
                    animationIndex = Math.Min(animationIndex, spriteArray.Length - 1);
                }
            }
        }

        /// <summary>
        /// Returns the currently active sprite in the animation
        /// </summary>
        /// <returns></returns>
        public Sprite GetActiveSprite()
        {
            return spriteArray[animationIndex];
        }

        /// <summary>
        /// Resets the animation index back to 0
        /// </summary>
        public void ResetAnimation()
        {
            animationIndex = 0;
        }

        /// <summary>
        /// Returns if we're at the end of an animation. If it's a looping animation this will always return false
        /// </summary>
        public bool IsAtEndOfAnimation()
        {
            if (looping)
            {
                return false;
            }
            else
            {
                return animationIndex == (spriteArray.Length - 1);
            }
        }
    }
}
