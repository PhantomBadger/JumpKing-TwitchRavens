using EntityComponent;
using JumpKing.Util.Tags;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities
{
    /// <summary>
    /// A wrapper for our own entity system which handles the batch drawing of custom
    /// entities and foreground entities.
    /// Requires because the vanilla entity manager is not thread safe, so we can't add/remove from it at runtime safely
    /// </summary>
    public class ModEntityManager
    {
        private readonly ConcurrentDictionary<Entity, byte> entities;
        private readonly ConcurrentDictionary<IForeground, byte> foregroundEntities;

        /// <summary>
        /// Default ctor for creating a <see cref="ModEntityManager"/>
        /// </summary>
        public ModEntityManager()
        {
            entities = new ConcurrentDictionary<Entity, byte>();
            foregroundEntities = new ConcurrentDictionary<IForeground, byte>();
        }

        /// <summary>
        /// Registers an <see cref="Entity"/> with this entity manager
        /// </summary>
        public bool AddEntity(Entity entity)
        {
            return entities.TryAdd(entity, 0);
        }

        /// <summary>
        /// Registers a <see cref="IForeground"/> entity with this entity manager
        /// </summary>
        public bool AddForegroundEntity(IForeground foregroundEntity)
        {
            return foregroundEntities.TryAdd(foregroundEntity, 0);
        }

        /// <summary>
        /// Tries to remove an <see cref="Entity"/> from this entity manager
        /// </summary>
        public bool RemoveEntity(Entity entity)
        {
            return entities.TryRemove(entity, out _);
        }

        /// <summary>
        /// Tries to remove an <see cref="IForeground"/> entity from this entity manager
        /// </summary>
        public bool RemoveForegroundEntity(IForeground foregroundEntity)
        {
            return foregroundEntities.TryRemove(foregroundEntity, out _);
        }

        /// <summary>
        /// Draws all the non-foreground entities
        /// </summary>
        public void Draw()
        {
            foreach(var entity in entities)
            {
                entity.Key.Draw();
            }
        }

        /// <summary>
        /// Draws all the foreground entities
        /// </summary>
        public void DrawForeground()
        {
            foreach (var foregroundEntity in foregroundEntities)
            {
                foregroundEntity.Key.ForegroundDraw();
            }
        }
    }
}
