using PBJKModBase.API;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Entities
{
    /// <summary>
    /// A wrapper for our own entity system which handles the batch drawing of custom
    /// entities and foreground entities.
    /// Requires because the vanilla entity manager is not thread safe, so we can't add/remove from it at runtime safely
    /// </summary>
    public class ModEntityManager
    {
        private readonly ConcurrentDictionary<IModEntity, byte> entities;
        private readonly ConcurrentDictionary<IForegroundModEntity, int> foregroundEntities;

        /// <summary>
        /// Singleton for accessing the ModEntityManager
        /// </summary>
        public static ModEntityManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ModEntityManager();
                }

                return instance;
            }
        }
        private static ModEntityManager instance;

        /// <summary>
        /// Default ctor for creating a <see cref="ModEntityManager"/>
        /// </summary>
        public ModEntityManager()
        {
            entities = new ConcurrentDictionary<IModEntity, byte>();
            foregroundEntities = new ConcurrentDictionary<IForegroundModEntity, int>();
        }

        /// <summary>
        /// Registers an <see cref="IModEntity"/> with this entity manager
        /// </summary>
        public bool AddEntity(IModEntity entity)
        {
            return entities.TryAdd(entity, 0);
        }

        /// <summary>
        /// Registers a <see cref="IForegroundModEntity"/> entity with this entity manager
        /// </summary>
        public bool AddForegroundEntity(IForegroundModEntity foregroundEntity, int zOrder = 0)
        {
            return foregroundEntities.TryAdd(foregroundEntity, zOrder);
        }

        /// <summary>
        /// Tries to remove an <see cref="Entity"/> from this entity manager
        /// </summary>
        public bool RemoveEntity(IModEntity entity)
        {
            return entities.TryRemove(entity, out _);
        }

        /// <summary>
        /// Tries to remove an <see cref="IForegroundModEntity"/> entity from this entity manager
        /// </summary>
        public bool RemoveForegroundEntity(IForegroundModEntity foregroundEntity)
        {
            return foregroundEntities.TryRemove(foregroundEntity, out _);
        }

        /// <summary>
        /// Draws all the non-foreground entities
        /// </summary>
        public void Draw()
        {
            var enumerator = entities.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Key?.Draw();
            }
        }

        /// <summary>
        /// Draws all the foreground entities
        /// </summary>
        public void DrawForeground()
        {
            var copyDictionary = new Dictionary<IForegroundModEntity, int>(foregroundEntities);
            var sortedDictionary = copyDictionary.OrderBy(x => x.Value);
            var enumerator = sortedDictionary.GetEnumerator();
            while (enumerator.MoveNext())
            {
                enumerator.Current.Key?.ForegroundDraw();
            }
        }

        /// <summary>
        /// Calls the update method on each entity
        /// </summary>
        public void Update(float delta)
        {
            var entityEnumerator = entities.GetEnumerator();
            while (entityEnumerator.MoveNext())
            {
                entityEnumerator.Current.Key?.Update(delta);
            }

            var foregroundEntityEnumerator = foregroundEntities.GetEnumerator();
            while (foregroundEntityEnumerator.MoveNext())
            {
                foregroundEntityEnumerator.Current.Key?.Update(delta);
            }
        }
    }
}
