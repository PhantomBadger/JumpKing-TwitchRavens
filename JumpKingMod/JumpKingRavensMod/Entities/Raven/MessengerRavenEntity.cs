using JumpKing;
using JumpKingRavensMod.API;
using JumpKingRavensMod.Components;
using JumpKingRavensMod.Settings;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.Entities;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Entities.Raven
{
    // TODO: Move this logic into OOP States with a State Machine that we can give to RavenEntity
    /// <summary>
    /// An extension of <see cref="RavenEntity"/> that lands and reports a message
    /// </summary>
    public class MessengerRavenEntity : RavenEntity
    {
        public enum RavenLogicState
        {
            Starting,
            FlyingToPoint,
            Messaging,
            FlyingAway,
            Ending,
            Killed,
            KilledFalling,
        }

        protected readonly IRavenLandingPositionsCache landingPositionsCache;
        protected readonly string ravenName;
        protected readonly Color ravenNameColour;

        protected UITextEntity ravenNameEntity;
        protected RavenLogicState ravenLogicState;
        protected Vector2 startingPosition;
        protected Vector2 landingPosition;
        protected float landingProgress;
        protected string landingMessage;
        protected UITextEntity messageEntity;
        protected float messageTimer;
        protected float maxMessageTimeInSeconds;
        protected float killStunTimer;
        protected float killFallTimer;
        protected Vector2 entryVector;
        protected float nameYOffset;
        protected Sprite killFallSprite;
        protected Sprite killStunSprite;

        protected const float LandingDistanceThreshold = 0.001f;
        protected const float DefaultMessageTimeInSeconds = 3.0f;
        protected const float MaxKillStunTimeInSeconds = 0.25f; // should be like 0.25f
        protected const float MaxKillFallTimeInSeconds = 3.0f;
        protected const float RavenSpeed = 3f;
        protected const float RavenFallSpeed = 5f;
        protected const float HardcodedNameYOffset = 20;

        /// <summary>
        /// Ctor for creating a <see cref="MessengerRavenEntity"/>
        /// </summary>
        /// <param name="startingPosition">The position to spawn the raven at in world space</param>
        /// <param name="messageText">The message for the raven to say</param>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to add this to</param>
        /// <param name="landingPositionsCache">The <see cref="IRavenLandingPositionsCache"/> implementation to use for floor positions</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        public MessengerRavenEntity(Vector2 startingPosition, string messageText, ModEntityManager modEntityManager, 
            IRavenLandingPositionsCache landingPositionsCache, ILogger logger, UserSettings settings)
            : this(startingPosition, messageText, null, Color.White, modEntityManager, landingPositionsCache, logger, settings)
        {

        }

        /// <summary>
        /// Ctor for creating a <see cref="MessengerRavenEntity"/>
        /// </summary>
        /// <param name="startingPosition">The position to spawn the raven at in world space</param>
        /// <param name="messageText">The message for the raven to say</param>
        /// <param name="ravenName">The name of the raven to be displayed underneath, if null or empty nothing will be displayed</param>
        /// <param name="ravenNameColour">The colour to use for the name of the raven</param>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to add this to</param>
        /// <param name="landingPositionsCache">The <see cref="IRavenLandingPositionsCache"/> implementation to use for floor positions</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation for logging</param>
        public MessengerRavenEntity(Vector2 startingPosition, string messageText, string ravenName, Color ravenNameColour,
            ModEntityManager modEntityManager, IRavenLandingPositionsCache landingPositionsCache, ILogger logger, UserSettings settings) 
            : base(startingPosition, modEntityManager, logger)
        {
            this.ravenName = ravenName;
            this.ravenNameColour = ravenNameColour;
            this.landingPositionsCache = landingPositionsCache ?? throw new ArgumentNullException(nameof(landingPositionsCache));

            ravenLogicState = RavenLogicState.Starting;
            landingMessage = messageText;
            ReadyToBeDestroyed = false;
            messageTimer = 0;
            killStunTimer = 0;
            killFallTimer = 0;

            maxMessageTimeInSeconds = settings.GetSettingOrDefault(JumpKingModSettingsContext.RavensDisplayTimeInSecondsKey, DefaultMessageTimeInSeconds);

            if (!string.IsNullOrWhiteSpace(ravenName))
            {
                ravenNameEntity = new UITextEntity(modEntityManager, Camera.TransformVector2(Transform), 
                    ravenName, ravenNameColour, UIEntityAnchor.Center, JKContentManager.Font.MenuFontSmall);
                nameYOffset = (ravenNameEntity.Size.Y / 2) + HardcodedNameYOffset;
            }

            killStunSprite = Sprite.CreateSpriteWithCenter(RavensModContentManager.RavenStunnedTexture, 
                new Rectangle(0, 0, RavensModContentManager.RavenStunnedTexture.Width, RavensModContentManager.RavenStunnedTexture.Height),
                new Vector2(0.5f, 0.5f));
            killFallSprite = Sprite.CreateSpriteWithCenter(RavensModContentManager.RavenFallingTexture,
                new Rectangle(0, 0, RavensModContentManager.RavenFallingTexture.Width, RavensModContentManager.RavenFallingTexture.Height),
                new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public override void Dispose()
        {
            ravenNameEntity?.Dispose();
            messageEntity?.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Called by the Entity Manager each frame, updates the logic of the raven
        /// </summary>
        public override void Update(float delta)
        {
            switch (ravenLogicState)
            {
                case RavenLogicState.Starting:
                    // Initialise the landing point we want to go to
                    List<Vector2> possiblePositions = landingPositionsCache.GetPossibleFloorPositions(Camera.CurrentScreen);
                    if (possiblePositions.Count > 0)
                    {
                        landingPosition = possiblePositions[random.Next(0, possiblePositions.Count)];
                        startingPosition = Transform;
                        landingProgress = 0;
                        ravenLogicState = RavenLogicState.FlyingToPoint;
                    }
                    break;
                case RavenLogicState.FlyingToPoint:
                    // Fly to that landing point
                    entryVector = landingPosition - startingPosition;
                    float entryDistance = entryVector.Length();
                    entryVector.Normalize();
                    landingProgress += RavenSpeed;

                    Velocity = entryVector * RavenSpeed;

                    if (landingProgress >= entryDistance)
                    {
                        Transform = landingPosition;
                        ravenLogicState = RavenLogicState.Messaging;
                    }
                    break;
                case RavenLogicState.Messaging:
                    // Say a message
                    if (messageEntity == null)
                    {
                        Vector2 messagePosition = Transform + new Vector2(0, -20);
                        messageEntity = new UITextEntity(modEntityManager, Camera.TransformVector2(messagePosition), landingMessage, Color.White, UIEntityAnchor.Center, JKContentManager.Font.MenuFontSmall);
                        Vector2 textSize = messageEntity.Size;

                        // Do a rough check to ensure the message is always on screen,
                        // favouring the left of it to be on screen if it's so big it goes entirely off both sides
                        float rightXPosition = messagePosition.X + (textSize.X / 2);
                        if (rightXPosition > 480)
                        {
                            messagePosition.X -= (rightXPosition - 480);
                            messageEntity.ScreenSpacePosition = Camera.TransformVector2(messagePosition);
                        }
                        float leftXPosition = messagePosition.X - (textSize.X / 2);
                        if (leftXPosition < 0)
                        {
                            messagePosition.X += Math.Abs(leftXPosition);
                            messageEntity.ScreenSpacePosition = Camera.TransformVector2(messagePosition);
                        }
                    }

                    // After a pre-set time, dispose of the message
                    if ((messageTimer += delta) > maxMessageTimeInSeconds)
                    {
                        messageEntity?.Dispose();
                        messageEntity = null;
                        ravenLogicState = RavenLogicState.FlyingAway;
                    }
                    break;
                case RavenLogicState.FlyingAway:
                    // Fly off screen
                    Vector2 exitVector = entryVector;
                    exitVector.Y *= -1;
                    exitVector.Normalize();

                    Velocity = exitVector * RavenSpeed;

                    if (Transform.X < -width || Transform.X > (480 + width))
                    {
                        ravenLogicState = RavenLogicState.Ending;
                    }
                    break;
                case RavenLogicState.Ending:
                    ReadyToBeDestroyed = true;
                    break;
                case RavenLogicState.Killed:
                    // After a pre-set time, start falling
                    if ((killStunTimer += delta) > MaxKillStunTimeInSeconds)
                    {
                        SetOverrideAnimation(new LoopingAnimationComponent(new Sprite[] { killFallSprite }, 1f));
                        ravenLogicState = RavenLogicState.KilledFalling;

                        // Dispose of the name early
                        ravenNameEntity?.Dispose();
                        ravenNameEntity = null;

                        // Dispose of the text early
                        messageEntity?.Dispose();
                        messageEntity = null;
                    }
                    break;
                case RavenLogicState.KilledFalling:
                    // Fall off screen
                    Vector2 downVector = new Vector2(0, 1);
                    Velocity = downVector * RavenFallSpeed;

                    // After a pre-set time, destroy the raven
                    if ((killFallTimer += delta) > MaxKillFallTimeInSeconds)
                    {
                        ravenLogicState = RavenLogicState.Ending;
                    }
                    break;
                default:
                    throw new NotImplementedException($"No logic supporting a state of {ravenLogicState.ToString()}");
            }

            // Call the base update for velocity, animations, and the like
            base.Update(delta);

            // Update the name text's position
            if (ravenNameEntity != null)
            {
                ravenNameEntity.ScreenSpacePosition = Camera.TransformVector2(Transform + new Vector2(0, nameYOffset));
            }
        }

        /// <summary>
        /// If called will set the Raven's internal state to 'Killed' which will cause
        /// it to fall for a set time
        /// </summary>
        public void SetKillState()
        {
            SetOverrideAnimation(new LoopingAnimationComponent(new Sprite[] { killStunSprite }, 1f));
            ravenLogicState = RavenLogicState.Killed;
        }

        /// <summary>
        /// Called by the Entity Manager to render this raven
        /// </summary>
        public override void Draw()
        {
            // Debug render possible floor positions
            //if (Keyboard.GetState().IsKeyDown(Keys.P))
            //{
            //    List<Vector2> hitPositions = landingPositionsCache.GetPossibleFloorPositions(Camera.CurrentScreen);
            //    if (hitPositions.Count > 0)
            //    {
            //        for (int i = 0; i < hitPositions.Count; i++)
            //        {
            //            ravenContent.BlinkTreasure.Draw(Camera.TransformVector2(hitPositions[i]));
            //        }
            //    }
            //}
            //if (Keyboard.GetState().IsKeyDown(Keys.Delete))
            //{
            //    landingPositionsCache.InvalidateCache(Camera.CurrentScreen);
            //}

            // Base draw call for drawing the sprites
            base.Draw();
        }
    }
}
