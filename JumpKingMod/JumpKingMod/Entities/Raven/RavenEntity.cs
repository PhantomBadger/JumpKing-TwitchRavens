using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Components;
using JumpKingMod.Entities.Raven;
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
        private readonly ModEntityManager modEntityManager;
        private readonly ILogger logger;
        private readonly JKContentManager.RavenSprites.RavenContent ravenContent;
        public Vector2 Transform;
        public Vector2 Velocity;

        private LoopingAnimationComponent flyingAnimation;
        private LoopingAnimationComponent idleAnimation;
        private LoopingAnimationComponent activeAnimation;
        private SpriteEffects spriteEffects;
        private ModRavenState ravenState;

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

            flyingAnimation = new LoopingAnimationComponent(ravenContent.Fly, 0.05f);
            idleAnimation = new LoopingAnimationComponent(ravenContent.IdleSprites, 0.1f);
            activeAnimation = idleAnimation;
            ravenState = ModRavenState.Idle;
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
            Transform += Velocity;

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
                ravenState = ModRavenState.Flying;
            }
            else
            {
                ravenState = ModRavenState.Idle;
            }

            HandleAnimationUpdates(delta);
            Velocity = new Vector2(0, 0);
        }

        /// <summary>
        /// Called by the entity manager to draw this entity
        /// </summary>
        public void Draw()
        {
            Sprite activeSprite = activeAnimation.GetActiveSprite();
            activeSprite.Draw(Camera.TransformVector2(Transform), spriteEffects);
        }

        /// <summary>
        /// Updates all the appropriate animation values based on the current state
        /// </summary>
        private void HandleAnimationUpdates(float delta)
        {
            switch (ravenState)
            {
                case ModRavenState.Flying:
                    if (activeAnimation != flyingAnimation)
                    {
                        activeAnimation = flyingAnimation;
                        activeAnimation.ResetAnimation();
                    }
                    break;
                case ModRavenState.Idle:
                    if (activeAnimation != idleAnimation)
                    {
                        activeAnimation = idleAnimation;
                        activeAnimation.ResetAnimation();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            activeAnimation.Update(delta);
        }
    }
}
