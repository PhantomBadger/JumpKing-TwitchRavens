using HarmonyLib;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
        private float width;
        private float height;
        private SpriteEffects spriteEffects;
        private RavenStateKey activeState;
        private Type collisionInfoType;
        private MethodInfo checkCollisionMethod;

        private const float MinimimXPollingValue = 30;
        private const float MaximumXPollingValue = 470;

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

            width = ravenContent.Blink.source.Width;
            height = ravenContent.Blink.source.Height;

            ravenStates = new Dictionary<RavenStateKey, IModEntityState>()
            {
                { RavenStateKey.Idle, idleState },
                { RavenStateKey.Flying, flyingState },
            };

            // Set the starting state
            SetState(RavenStateKey.Idle);
            idleState.Enter();
            spriteEffects = SpriteEffects.None;

            Transform = new Vector2(100, 100);
            Velocity = new Vector2(0, 0);

            // Set up reflection references
            collisionInfoType = AccessTools.TypeByName("JumpKing.Level.LevelScreen+CollisionInfo") 
                ?? throw new InvalidOperationException($"Cannot find 'JumpKing.Level.LevelScreen+CollisionInfo' type in Jump King");
            checkCollisionMethod = AccessTools.Method("JumpKing.Level.LevelManager:CheckCollision") 
                ?? throw new InvalidOperationException($"Cannot find 'JumpKing.Level.LevelManager:CheckCollision' method in Jump King");

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
            if (Keyboard.IsKeyDown(Key.F2))
            {
                TryFindValidGround(5, 5, out List<Vector2> hitPositions);
                if (hitPositions.Count > 0)
                {
                    for (int i = 0; i < hitPositions.Count; i++)
                    {
                        ravenContent.BlinkTreasure.Draw(Camera.TransformVector2(hitPositions[i]));
                    }
                }
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

        /// <summary>
        /// Gets the hitbox of the raven at a predetermined location
        /// </summary>
        private Rectangle GetHitbox(Vector2 transform)
        {
            Vector2 bottomLeft = transform;
            bottomLeft.X -= width / 2;
            bottomLeft.Y -= width / 2;
            return new Rectangle(bottomLeft.ToPoint(), new Point((int)width, (int)height));
        }

        /// <summary>
        /// Attempts to find valid ground placement on the current screen below the raven's current Y position
        /// </summary>
        private bool TryFindValidGround(float verticalPollingIncrement, float horizontalPollingIncrement, out List<Vector2> hitPositions)
        {
            // Initialise hit positions
            hitPositions = new List<Vector2>();

            // The max length of the ray is until the bottom of the screen, this will ensure we
            // don't accidentally go to a different screen 
            float maxDownRayLength = Math.Abs(Transform.Y - (Camera.Offset.Y - height));

            // Start marching rays downwards moving horizontally across the screen
            float leftMax = MinimimXPollingValue + width;
            float rightMax = MaximumXPollingValue - width;
            for (float x = leftMax; x < rightMax; x += horizontalPollingIncrement)
            {
                // Get the start of our downward rays
                Vector2 verticalRayStartPosition = new Vector2(x, Transform.Y);

                // Keep track of whether we have hit anything that isnt collision on this
                // ray yet, this will ensure we dont get any false positives inside a wall or something similar
                bool hasNotCollidedYet = false;

                // March down the ray until we hit the end
                for (float y = 0; y < maxDownRayLength; y += verticalPollingIncrement)
                {
                    // Create a new test position along our ray
                    Vector2 testPosition = verticalRayStartPosition + ((new Vector2(0, 1) * y));

                    // Check to see if we hit anything
                    // Providing null in the parameters array will allow the reflection
                    // method info to properly populate it with the out parameters when we invoke
                    Rectangle testHitbox = GetHitbox(testPosition);
                    object[] parameters = new object[] { testHitbox, null, null };
                    bool result = (bool)checkCollisionMethod.Invoke(null, parameters);

                    // If we've collided with something
                    if (result)
                    {
                        // And we have already not collided with something on this ray
                        // (ie, we're not stuck inside ONLY collision)
                        if (hasNotCollidedYet)
                        {
                            Rectangle outOverlap = (Rectangle)parameters[1];
                            object outCollisionInfo = parameters[2];

                            // March back up in smaller increments until we are not colliding with anything
                            // which will mean we're at the 'floor'
                            Vector2 hitLocation = outOverlap.Location.ToVector2();
                            object[] floorTestParameters;
                            do
                            {
                                hitLocation.Y -= 1;
                                floorTestParameters = new object[] { new Rectangle(hitLocation.ToPoint(), new Point(1, 1)), null, null };
                            } while ((bool)checkCollisionMethod.Invoke(null, floorTestParameters));

                            // Add our adjusted position to the list
                            hitPositions.Add(hitLocation);

                            // No need to query this ray anymore
                            break;
                        }
                    }
                    else
                    {
                        hasNotCollidedYet = true;
                    }
                }
            }

            return hitPositions.Count > 0;
        }
    }
}
