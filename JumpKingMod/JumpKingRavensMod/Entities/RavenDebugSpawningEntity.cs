using JumpKing;
using JumpKingRavensMod.API;
using JumpKingRavensMod.Entities.Raven;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingRavensMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> which allows debug spawning of ravens
    /// </summary>
    public class RavenDebugSpawningEntity : IModEntity, IDisposable
    {
        private readonly ILogger logger;
        private readonly ModEntityManager modEntityManager;
        private readonly List<RavenEntity> ravenEntities;

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
            
            ravenEntities = new List<RavenEntity>();

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
                RavenEntity raven = ravenEntities[ravenEntities.Count - 1];
                raven?.Dispose();
                ravenEntities.RemoveAt(ravenEntities.Count - 1);
            }
        }

        /// <summary>
        /// Called by the entity manager, allows us to spawn debug ravens
        /// </summary>
        public void Update(float p_delta)
        {
            // Add Raven
            if (Keyboard.GetState().IsKeyDown(Keys.F2))
            {
                if (!addEntityCooldown)
                {
                    addEntityCooldown = true;
                    Vector2 spawnPos = new Vector2(-50, (-((Camera.CurrentScreen - 1) * 360) - 350));
                    ravenEntities.Add(new RavenEntity(spawnPos, modEntityManager, logger));
                }
            }
            else
            {
                addEntityCooldown = false;
            }

            // Remove Raven
            if (Keyboard.GetState().IsKeyDown(Keys.F3))
            {
                if (!removeEntityCooldown)
                {
                    removeEntityCooldown = true;
                    if (ravenEntities.Count > 0)
                    { 
                        RavenEntity ravenEntity = ravenEntities[ravenEntities.Count - 1];
                        ravenEntity?.Dispose();
                        ravenEntities.Remove(ravenEntity);
                    }
                }
            }
            else
            {
                removeEntityCooldown = false;
            }

            // Identify ravens we wanna destroy
            List<RavenEntity> ravensToDestroy = new List<RavenEntity>();
            foreach (var raven in ravenEntities)
            {
                if (raven.ReadyToBeDestroyed)
                {
                    ravensToDestroy.Add(raven);
                }
            }

            // Destroy the ravens
            foreach (var raven in ravensToDestroy)
            {
                if (raven != null)
                {
                    raven.Dispose();
                    ravenEntities.Remove(raven);
                }
            }
        }

        public void Draw()
        {
            // Draw nothing
        }
    }
}
