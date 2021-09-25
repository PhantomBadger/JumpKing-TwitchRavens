using HarmonyLib;
using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Components;
using JumpKingMod.Entities.Raven;
using JumpKingMod.Entities.Raven.States;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private readonly IRavenLandingPositionsCache landingPositionsCache;

        private LoopingAnimationComponent activeAnimation;
        private float width;
        private float height;
        private SpriteEffects spriteEffects;
        private IModEntityState activeState;

        /// <summary>
        /// Ctor for creating a <see cref="RavenEntity"/>
        /// Adds itself to the entity manager
        /// </summary>
        public RavenEntity(Vector2 transform, ModEntityManager modEntityManager, IRavenLandingPositionsCache landingPositionsCache,
            ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.landingPositionsCache = landingPositionsCache ?? throw new ArgumentNullException(nameof(landingPositionsCache));
            var settings = JKContentManager.RavenSprites.raven_settings;
            ravenContent = JKContentManager.RavenSprites.GetRavenContent(settings["raven"]) ?? throw new ArgumentNullException($"Unable to load animations for raven");

            // Initialise the state machine for the raven
            InitialiseRavenStates();

            width = ravenContent.Blink.source.Width;
            height = ravenContent.Blink.source.Height;
            spriteEffects = SpriteEffects.None;

            Transform = transform;
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
            // Manual Control
            DebugControl();

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
            }

            // Update the states
            if (activeState != null && activeState.EvaluateState(out IModEntityState nextState))
            {
                SetState(nextState);
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
            // If we have no active animation - there's nothing to draw!
            if (activeAnimation == null)
            {
                return;
            }

            // Get the active sprite from the active animation and draw it with our
            // positions and effects
            Sprite activeSprite = activeAnimation.GetActiveSprite();
            activeSprite.Draw(Camera.TransformVector2(Transform), spriteEffects);

            // Debug render possible floor positions
            if (Keyboard.GetState().IsKeyDown(Keys.P))
            {
                List<Vector2> hitPositions = landingPositionsCache.GetPossibleFloorPositions(Camera.CurrentScreen);
                if (hitPositions.Count > 0)
                {
                    for (int i = 0; i < hitPositions.Count; i++)
                    {
                        ravenContent.BlinkTreasure.Draw(Camera.TransformVector2(hitPositions[i]));
                    }
                }
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Delete))
            {
                landingPositionsCache.InvalidateCache(Camera.CurrentScreen);
            }
        }

        /// <summary>
        /// Sets the currently active looping animation
        /// </summary>
        public void SetLoopingAnimation(LoopingAnimationComponent loopingAnimation)
        {
            activeAnimation = loopingAnimation;
        }

        /// <summary>
        /// Initialises the states used by the raven
        /// </summary>
        private void InitialiseRavenStates()
        {
            // Set up states
            var idleAnimation = new LoopingAnimationComponent(ravenContent.IdleSprites, 0.1f);
            RavenIdleState idleState = new RavenIdleState(this, idleAnimation);

            var flyingAnimation = new LoopingAnimationComponent(ravenContent.Fly, 0.05f);
            RavenFlyingState flyingState = new RavenFlyingState(this, flyingAnimation);

            idleState.TransitionToState = flyingState;
            flyingState.TransitionToState = idleState;

            SetState(idleState);
        }

        /// <summary>
        /// Sets the active state to the one provided
        /// </summary>
        private void SetState(IModEntityState state)
        {
            if (activeState == null || activeState != state)
            {
                state.Enter();
                activeState?.Exit();

                // Update our active state
                activeState = state;
            }
        }

        /// <summary>
        /// Allows manual control of the raven
        /// </summary>
        private void DebugControl()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.A))
            {
                Velocity.X -= 3f;
            }
            if (keyboardState.IsKeyDown(Keys.S))
            {
                Velocity.Y += 3f;
            }
            if (keyboardState.IsKeyDown(Keys.D))
            {
                Velocity.X += 3f;
            }
            if (keyboardState.IsKeyDown(Keys.W))
            {
                Velocity.Y -= 3f;
            }
        }
    }
}
