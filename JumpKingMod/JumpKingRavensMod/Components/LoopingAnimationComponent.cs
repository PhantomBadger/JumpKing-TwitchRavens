using JumpKing;
using JumpKingRavensMod.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Components
{
    /// <summary>
    /// An implementation of <see cref="IModComponent"/> which animates through a list of sprites
    /// on a loop
    /// </summary>
    public class LoopingAnimationComponent : IModComponent
    {
        private readonly Sprite[] spriteArray;
        private readonly float animationStep;

        private int animationIndex;
        private float elapsedTime;

        /// <summary>
        /// Constructor taking in an array of sprites, and the time to wait between each loop
        /// </summary>
        /// <param name="spriteArray"></param>
        /// <param name="animationStep"></param>
        public LoopingAnimationComponent(Sprite[] spriteArray, float animationStep)
        {
            this.spriteArray = spriteArray ?? throw new ArgumentNullException(nameof(spriteArray));
            if (spriteArray.Length <= 0)
            {
                throw new InvalidOperationException($"Need sprites to actually do anything");
            }
            this.animationStep = animationStep;
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
                elapsedTime = 0;
                animationIndex += 1;
                animationIndex %= spriteArray.Length;
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
    }
}
