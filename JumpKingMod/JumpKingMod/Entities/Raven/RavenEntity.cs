using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Components;
using JumpKingMod.Entities.Raven;
using JumpKingMod.Entities.Raven.States;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> which acts as a Raven
    /// </summary>
    public class RavenEntity : IModEntity, IDisposable
    {
        public Vector2 Transform;
        public Vector2 Velocity;

        private readonly ModEntityManager modEntityManager;
        private readonly ILogger logger;
        private readonly JKContentManager.RavenSprites.RavenContent ravenContent;
        private readonly IReadOnlyDictionary<RavenStateKey, IModEntityState> ravenStates;

        private LoopingAnimationComponent activeAnimation;
        private SpriteEffects spriteEffects;
        private RavenStateKey activeState;

        /// <summary>
        /// Ctor for creating a <see cref="RavenEntity"/>
        /// Adds itself to the entity manager
        /// </summary>
        public RavenEntity(ModEntityManager modEntityManager, ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            var settings = JKContentManager.RavenSprites.raven_settings;
            ravenContent = JKContentManager.RavenSprites.GetRavenContent(settings["raven"]) ?? throw new ArgumentNullException($"Unable to load animations for raven");

            // Set up states
            var idleAnimation = new LoopingAnimationComponent(ravenContent.IdleSprites, 0.1f);
            IModEntityState idleState = new RavenIdleState(this, idleAnimation);

            var flyingAnimation = new LoopingAnimationComponent(ravenContent.Fly, 0.05f);
            IModEntityState flyingState = new RavenFlyingState(this, flyingAnimation);

            ravenStates = new Dictionary<RavenStateKey, IModEntityState>()
            {
                { RavenStateKey.Idle, idleState },
                { RavenStateKey.Flying, flyingState },
            };

            // Set the starting state
            SetState(RavenStateKey.Idle);
            spriteEffects = SpriteEffects.None;

            Transform = new Vector2(100, 100);
            Velocity = new Vector2(0, 0);

            modEntityManager.AddEntity(this);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// Removes itself from the entity managera
        /// </summary>
        public void Dispose()
        {
            modEntityManager?.RemoveEntity(this);
        }

        /// <summary>
        /// Called by the entity manager to update the logic of this entity
        /// </summary>
        public void Update(float delta)
        {
            // Update the transform based on the velocity
            Transform += Velocity;

            // Update the current state based on the velocity
            if (Velocity.Length() > float.Epsilon)
            {
                if (Velocity.X < 0)
                {
                    spriteEffects |= SpriteEffects.FlipHorizontally;
                }
                else
                {
                    spriteEffects &= ~SpriteEffects.FlipHorizontally;
                }
                SetState(RavenStateKey.Flying);
            }
            else
            {
                SetState(RavenStateKey.Idle);
            }

            // Update the animations
            activeAnimation?.Update(delta);

            // Reset the velocity
            Velocity = new Vector2(0, 0);
        }

        /// <summary>
        /// Called by the entity manager to draw this entity
        /// </summary>
        public void Draw()
        {
            if (activeAnimation == null)
            {
                return;
            }

            // Get the active sprite from the active animation and draw it with our
            // positions and effects
            Sprite activeSprite = activeAnimation.GetActiveSprite();
            activeSprite.Draw(Camera.TransformVector2(Transform), spriteEffects);
        }

        /// <summary>
        /// Sets the currently active looping animation
        /// </summary>
        public void SetLoopingAnimation(LoopingAnimationComponent loopingAnimation)
        {
            activeAnimation = loopingAnimation;
        }

        /// <summary>
        /// Sets the active state based on the <see cref="RavenStateKey"/>
        /// Handles the <see cref="IModEntityState.Exit"/> and <see cref="IModEntityState.Enter"/>
        /// calls as appropriate
        /// </summary>
        private void SetState(RavenStateKey key)
        {
            if (ravenStates.ContainsKey(key))
            {
                if (activeState != key)
                {
                    // Call the Enter/Exit code for the appropriate state
                    IModEntityState newState = ravenStates[key];
                    if (ravenStates.ContainsKey(activeState))
                    {
                        ravenStates[activeState].Exit();
                    }
                    newState.Enter();

                    // Update our active state
                    activeState = key;
                }
            }
        }
    }
}
