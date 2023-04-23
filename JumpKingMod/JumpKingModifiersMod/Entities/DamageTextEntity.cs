using Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Entities
{
    /// <summary>
    /// An extension of <see cref="UITextEntity"/> which displays a damage value and moves through
    /// the air in a simplistic parabolic curve
    /// </summary>
    public class DamageTextEntity : UITextEntity
    {
        public bool IsAlive { get; private set; }

        private readonly Random random;
        private readonly Vector2 initialDirection;
        private readonly bool isInitialDirectionGoingRight;

        private Vector2 velocity;
        private float lifetimeCounter;

        private const float MinXComponent = 1.2f;
        private const float MaxXComponent = 3.0f;
        private const float MinYComponent = 1.5f;
        private const float MaxYComponent = 2.5f;
        private const float LifetimeInSeconds = 0.75f;
        private const float Gravity = 9.8f;
        private const float MaxYVelocity = 5f;
        private const float AirResistance = 9.5f;

        /// <summary>
        /// Ctor for creating a <see cref="DamageTextEntity"/>
        /// </summary>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register ourselves to</param>
        /// <param name="screenSpacePosition">The Screen Space Position to spawn at, this will be changed by the entity over time</param>
        /// <param name="textValue">The text value to display</param>
        /// <param name="textColor">The colour of the text to display</param>
        /// <param name="spriteFont">The font to use for the text</param>
        /// <param name="random">An instance of <see cref="Random"/> to seed our decisions</param>
        /// <exception cref="ArgumentNullException"><paramref name="random"/> is not allowed to be null</exception>
        public DamageTextEntity(ModEntityManager modEntityManager, Vector2 screenSpacePosition, string textValue, Color textColor, SpriteFont spriteFont, Random random)
            : base(modEntityManager, screenSpacePosition, textValue, textColor, UIEntityAnchor.Center, spriteFont, zOrder: 1)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            initialDirection = GetInitialDirectionVector();
            isInitialDirectionGoingRight = (initialDirection.X > 0);

            velocity = initialDirection;
            lifetimeCounter = 0;
            IsAlive = true;
        }

        /// <summary>
        /// Each frame updates our position and decays our velocity with simulated air resistance and gravity
        /// then identifies if we've been alive too long and removes us from the <see cref="ModEntityManager"/> if so
        /// </summary>
        public override void Update(float p_delta)
        {
            base.Update(p_delta);

            // Update the position based on our current velocity
            ScreenSpacePosition += velocity;

            // Decay our velocity with fake air resistance and gravity
            float newX = velocity.X;
            if (isInitialDirectionGoingRight)
            {
                newX -= ((AirResistance * (Math.Abs(velocity.X) / MaxXComponent)) * p_delta);
                newX = Math.Max(0, newX);
            }
            else
            {
                newX += ((AirResistance * (Math.Abs(velocity.X) / MaxXComponent)) * p_delta);
                newX = Math.Min(0, newX);
            }
            float newY = velocity.Y + (Gravity * p_delta);
            newY = Math.Min(MaxYVelocity, newY);
            velocity = new Vector2(newX, newY);

            // Calculate our lifetime
            if ((lifetimeCounter += p_delta) > LifetimeInSeconds)
            {
                // We've been alive too long, time to destroy ourselves
                // we remove ourselves from the mod entity manager, which will prevent us being drawn or updated in future
                modEntityManager.RemoveForegroundEntity(this);
                IsAlive = false;
            }
        }

        /// <summary>
        /// Gets a random initial direction vector (not normalized) with the X value being between <see cref="MinXComponent"/> and <see cref="MaxXComponent"/>
        /// in either the positive or negative direction. And the Y value being between <see cref="MinYComponent"/> and <see cref="MaxYComponent"/>
        /// </summary>
        private Vector2 GetInitialDirectionVector()
        {
            // Decide if we're going left or right
            bool goingRight = (random.Next(100) % 2) == 0;

            // Determine the X component of our vector
            float xRange = MaxXComponent - MinXComponent;
            float xValue = ((float)random.NextDouble() * xRange) + MinXComponent;
            if (!goingRight)
            {
                xValue *= -1;
            }

            // Determine the Y component of our vector
            float yRange = MaxYComponent - MinYComponent;
            float yValue = -(((float)random.NextDouble() * yRange) + MinYComponent);

            return new Vector2(xValue, yValue);
        }
    }
}
