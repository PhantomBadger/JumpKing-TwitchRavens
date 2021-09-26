using JumpKing;
using JumpKingMod.API;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities.Raven
{
    public class MessengerRavenEntity : RavenEntity
    {
        protected readonly IRavenLandingPositionsCache landingPositionsCache;
        protected readonly string ravenName;
        protected readonly Color ravenNameColour;
        protected readonly UITextEntity ravenNameEntity;

        protected RavenLogicState ravenLogicState;
        protected Vector2 startingPosition;
        protected Vector2 landingPosition;
        protected float landingProgress;
        protected string landingMessage;
        protected UITextEntity messageEntity;
        protected float messageTimer;
        protected Vector2 entryVector;
        protected float nameYOffset = 10;

        protected const float LandingDistanceThreshold = 0.001f;
        protected const float MaxMessageTimeInSeconds = 3.0f;
        protected const float RavenSpeed = 3f;

        public MessengerRavenEntity(Vector2 startingPosition, string messageText, ModEntityManager modEntityManager, 
            IRavenLandingPositionsCache landingPositionsCache, ILogger logger)
            : this(startingPosition, messageText, null, Color.White, modEntityManager, landingPositionsCache, logger)
        {

        }

        public MessengerRavenEntity(Vector2 startingPosition, string messageText, string ravenName, Color ravenNameColour,
            ModEntityManager modEntityManager, IRavenLandingPositionsCache landingPositionsCache, ILogger logger) 
            : base(startingPosition, modEntityManager, logger)
        {
            this.ravenName = ravenName;
            this.ravenNameColour = ravenNameColour;
            this.landingPositionsCache = landingPositionsCache ?? throw new ArgumentNullException(nameof(landingPositionsCache));

            ravenLogicState = RavenLogicState.Starting;
            landingMessage = messageText;
            ReadyToBeDestroyed = false;

            if (!string.IsNullOrWhiteSpace(ravenName))
            {
                ravenNameEntity = new UITextEntity(modEntityManager, Camera.TransformVector2(Transform), 
                    ravenName, ravenNameColour, UITextEntityAnchor.Center, JKContentManager.Font.MenuFontSmall);
                nameYOffset = (ravenNameEntity.Size.Y / 2) + 2;
            }
        }

        public override void Dispose()
        {
            ravenNameEntity?.Dispose();
            messageEntity?.Dispose();
            base.Dispose();
        }

        public override void Update(float delta)
        {
            switch (ravenLogicState)
            {
                case RavenLogicState.Starting:
                    // Initialise the landing point we want to go to
                    List<Vector2> possiblePositions = landingPositionsCache.GetPossibleFloorPositions(Camera.CurrentScreen);
                    landingPosition = possiblePositions[random.Next(0, possiblePositions.Count)];
                    startingPosition = Transform;
                    landingProgress = 0;
                    ravenLogicState = RavenLogicState.FlyingToPoint;
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
                        // TODO: Word wrap???
                        Vector2 messagePosition = Transform + new Vector2(0, -40);
                        messageEntity = new UITextEntity(modEntityManager, Camera.TransformVector2(messagePosition), landingMessage, Color.White, UITextEntityAnchor.Center, JKContentManager.Font.MenuFontSmall);
                        Vector2 textSize = messageEntity.Size;
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

                    if ((messageTimer += delta) > MaxMessageTimeInSeconds)
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
                default:
                    throw new NotImplementedException($"No logic supporting a state of {ravenLogicState.ToString()}");
            }

            base.Update(delta);

            if (ravenNameEntity != null)
            {
                ravenNameEntity.ScreenSpacePosition = Camera.TransformVector2(Transform + new Vector2(0, nameYOffset));
            }
        }

        public override void Draw()
        {
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

            base.Draw();
        }
    }
}
