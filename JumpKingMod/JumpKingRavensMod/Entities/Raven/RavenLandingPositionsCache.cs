using EntityComponent;
using HarmonyLib;
using JumpKing;
using JumpKing.Level;
using JumpKing.Props.RaymanWall;
using JumpKingRavensMod.API;
using Logging.API;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Entities.Raven
{
    /// <summary>
    /// An implementation of <see cref="IRavenLandingPositionsCache"/> which does vertical ray marches across
    /// the screen and caches the results for re-use
    /// </summary>
    public class RavenLandingPositionsCache : IRavenLandingPositionsCache
    {
        private readonly ILogger logger;
        private readonly ConcurrentDictionary<int, List<Vector2>> floorPositionCache;
        private readonly MethodInfo checkCollisionMethod;
        private readonly LevelScreen[] screens;
        private readonly List<RaymanData> raymanWallDatas;

        private const float MinimimXPollingValue = 30;
        private const float MaximumXPollingValue = 470;
        private const float HitboxWidth = 14;
        private const float HitboxHeight = 28;
        private const float VerticalQueryYOffset = 300;

        /// <summary>
        /// Constructor for creating a <see cref="RavenLandingPositionsCache"/>
        /// Gets the methods required for reflection
        /// </summary>
        /// <param name="newLogger"></param>
        public RavenLandingPositionsCache(ILogger newLogger)
        {
            logger = newLogger ?? throw new ArgumentNullException(nameof(newLogger));
            floorPositionCache = new ConcurrentDictionary<int, List<Vector2>>();

            // Set up reflection references
            checkCollisionMethod = AccessTools.Method(typeof(LevelManager), "CheckCollisionInternal")
                ?? throw new InvalidOperationException($"Cannot find 'JumpKing.Level.LevelManager:CheckCollisionInternal' method in Jump King");
            FieldInfo screensField = AccessTools.Field(typeof(LevelManager), "m_screens");
            screens = (LevelScreen[])screensField.GetValue(LevelManager.Instance);

            // Get all the Rayman Wall Entities and the means to query them
            raymanWallDatas = new List<RaymanData>();
            Type raymanWallEntityType = AccessTools.TypeByName("JumpKing.Props.RaymanWall.RaymanWallEntity");
            FieldInfo raymanWallDataField = AccessTools.Field(raymanWallEntityType, "m_data");
            IReadOnlyList<Entity> entities = EntityManager.instance.Entities;
            for (int i = 0; i < entities.Count; i++)
            {
                if (entities[i].GetType() == raymanWallEntityType)
                {
                    raymanWallDatas.Add((RaymanData)raymanWallDataField.GetValue(entities[i]));
                }
            }
        }

        /// <summary>
        /// Gets a list of possible floor positions on the screen, using a cached value if it is available
        /// </summary>
        /// <param name="screenIndex">The index of the screen to test on</param>
        /// <returns>A list of <see cref="Vector2"/> positions of valid floor locations</returns>
        public List<Vector2> GetPossibleFloorPositions(int screenIndex)
        {
            if (floorPositionCache.ContainsKey(screenIndex))
            {
                return floorPositionCache[screenIndex];
            }
            else
            {
                List<Vector2> floorPositions = new List<Vector2>();
                bool result = TryFindValidGround(
                    screenIndex, 
                    (int)HitboxWidth, 
                    (int)HitboxHeight, 
                    VerticalQueryYOffset, 
                    HitboxHeight / 2, 
                    HitboxWidth / 2, 
                    out floorPositions);
                floorPositionCache.TryAdd(screenIndex, floorPositions);

                if (!result)
                {
                    logger.Error($"Failed to find any valid Raven landing position for screen index: '{screenIndex}', Ravens may not spawn during this time");
                }

                return floorPositions;
            }
        }

        /// <summary>
        /// Clears the current cache for the floor positions
        /// </summary>
        public void InvalidateCache(int screenIndex)
        {
            floorPositionCache.TryRemove(screenIndex, out _);
        }

        /// <summary>
        /// Invalidates the entire cache, clearing all floor positions
        /// </summary>
        public void InvalidateCache()
        {
            floorPositionCache.Clear();
        }

        /// <summary>
        /// Gets a <see cref="Rectangle"/> representing a hitbox at the specified position
        /// </summary>
        private Rectangle GetHitbox(Vector2 transform, int hitboxWidth, int hitboxHeight)
        {
            Vector2 bottomLeft = transform;
            bottomLeft.X -= hitboxWidth / 2;
            bottomLeft.Y -= hitboxHeight / 2;
            return new Rectangle(bottomLeft.ToPoint(), new Point((int)hitboxWidth, (int)hitboxHeight));
        }

        /// <summary>
        /// Attempts to find valid ground placement on the current screen below the raven's current Y position
        /// </summary>
        private bool TryFindValidGround(int screenIndex, int hitboxWidth, int hitboxHeight, float startingYOffset, float verticalPollingIncrement, float horizontalPollingIncrement, out List<Vector2> hitPositions)
        {
            // Initialise hit positions
            hitPositions = new List<Vector2>();

            // The max length of the ray is until the bottom of the screen, this will ensure we
            // don't accidentally go to a different screen 
            float bottomScreenY = GetBottomOfScreenY(screenIndex);
            float yHeight = bottomScreenY - startingYOffset;
            float maxDownRayLength = Math.Abs(yHeight - bottomScreenY);

            // Start marching rays downwards moving horizontally across the screen
            float leftMax = MinimimXPollingValue + hitboxWidth;
            float rightMax = MaximumXPollingValue - hitboxWidth;
            for (float x = leftMax; x < rightMax; x += horizontalPollingIncrement)
            {
                // Get the start of our downward rays
                Vector2 verticalRayStartPosition = new Vector2(x, yHeight);

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
                    Rectangle testHitbox = GetHitbox(testPosition, hitboxWidth, hitboxHeight);
                    object[] parameters = new object[] { Camera.CurrentScreen, screens, LevelManager.TotalScreens, testHitbox, null, null };
                    bool result = (bool)checkCollisionMethod.Invoke(null, parameters);

                    // If we've collided with something
                    if (result)
                    {
                        // And we have already not collided with something on this ray
                        // (ie, we're not stuck inside ONLY collision)
                        if (hasNotCollidedYet)
                        {
                            // March back up in smaller increments until we are not colliding with anything
                            // which will mean we're at the 'floor'
                            Vector2 hitLocation = new Vector2(testPosition.X, testPosition.Y);
                            object[] floorTestParameters;
                            do
                            {
                                hitLocation.Y -= 1;
                                floorTestParameters = new object[] { Camera.CurrentScreen, screens, LevelManager.TotalScreens, new Rectangle(hitLocation.ToPoint(), new Point(1, 1)), null, null };
                            } while ((bool)checkCollisionMethod.Invoke(null, floorTestParameters));

                            // Add our adjusted position to the list
                            if (hitLocation.Y > bottomScreenY)
                            {
                                // We've gone off the bottom of the screen and dont care
                                break;
                            }
                            hitPositions.Add(hitLocation);

                            // No need to query this ray anymore
                            break;
                        }
                    }
                    else
                    {
                        // Skip over any rayman walls (The walls with no collision but a visual that obfuscates
                        if (IsCollidingWithRaymanWall(testHitbox, screenIndex))
                        {
                            continue;
                        }
                        hasNotCollidedYet = true;
                    }
                }
            }

            return hitPositions.Count > 0;
        }

        /// <summary>
        /// Gets the y value of the bottom of the screen in world space
        /// </summary>
        private float GetBottomOfScreenY(int screenIndex)
        {
            // (X - 1) * 360 then flip the sign
            float unflippedValue = (screenIndex - 1) * 360;
            return -unflippedValue;
        }

        /// <summary>
        /// Determines if the provided hitbox is colliding with any of the Rayman Walls on the current screen
        /// </summary>
        private bool IsCollidingWithRaymanWall(Rectangle hitbox, int screenIndex)
        {
            float yBottom = GetBottomOfScreenY(screenIndex);
            float yTop = yBottom - 360;

            for (int i = 0; i < raymanWallDatas.Count; i++)
            {
                RaymanData raymanWall = raymanWallDatas[i];

                // If this rayman wall has no data skip it
                if (raymanWall == null || raymanWall.hitboxes == null || raymanWall.hitboxes.Length <= 0)
                {
                    continue;
                }

                // If its not on this screen skip it
                if (raymanWall.Position.Y > yBottom || raymanWall.Position.Y < yTop)
                {
                    continue;
                }

                // Otherwise check its hitboxes
                for (int j = 0; j < raymanWall.hitboxes.Length; j++)
                {
                    if (raymanWall.hitboxes[j].Intersects(hitbox))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
