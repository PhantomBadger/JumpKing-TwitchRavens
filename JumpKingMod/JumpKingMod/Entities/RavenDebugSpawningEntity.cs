using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Entities.Raven;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> which allows debug spawning of ravens
    /// </summary>
    public class RavenDebugSpawningEntity : IModEntity, IDisposable
    {
        private readonly ILogger logger;
        private readonly ModEntityManager modEntityManager;
        private readonly IRavenLandingPositionsCache ravenLandingPositionsCache;
        private readonly Queue<RavenEntity> ravenEntities;

        private bool addEntityCooldown;
        private bool removeEntityCooldown;

        /// <summary>
        /// Constructor for creating a <see cref="RavenDebugSpawningEntity"/>
        /// </summary>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> that the ravens will bind to</param>
        /// <param name="logger">An implementation of <see cref="ILogger"/> to use for logging</param>
        public RavenDebugSpawningEntity(ModEntityManager modEntityManager, ILogger logger)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ravenLandingPositionsCache = new RavenLandingPositionsCache(logger);
            ravenEntities = new Queue<RavenEntity>();

            addEntityCooldown = false;
            removeEntityCooldown = false;

            modEntityManager.AddEntity(this);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up this entity 
        /// and any ravens its spawned
        /// </summary>
        public void Dispose()
        {
            modEntityManager.RemoveEntity(this);
            while (ravenEntities.Count > 0)
            {
                RavenEntity raven = ravenEntities.Dequeue();
                raven?.Dispose();
            }
        }

        /// <summary>
        /// Called by the entity manager, allows us to spawn debug ravens
        /// </summary>
        public void Update(float p_delta)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                if (!addEntityCooldown)
                {
                    addEntityCooldown = true;
                    Vector2 spawnPos = new Vector2(100, -((Camera.CurrentScreen - 1) * 360));
                    ravenEntities.Enqueue(new RavenEntity(spawnPos, modEntityManager, ravenLandingPositionsCache, logger));
                }
            }
            else
            {
                addEntityCooldown = false;
            }

            if (Keyboard.GetState().IsKeyDown(Keys.F3))
            {
                if (!removeEntityCooldown)
                {
                    removeEntityCooldown = true;
                    RavenEntity ravenEntity = ravenEntities.Dequeue();
                    ravenEntity?.Dispose();
                }
            }
            else
            {
                removeEntityCooldown = false;
            }
        }

        public void Draw()
        {
            // Draw nothing
        }
    }
}
