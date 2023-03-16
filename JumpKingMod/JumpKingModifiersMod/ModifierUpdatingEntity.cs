using JumpKingModifiersMod.API;
using Logging.API;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> and <see cref="IDisposable"/> which keeps track of registered <see cref="IModifier"/>
    /// implementations and calls Update on the active ones
    /// </summary>
    public class ModifierUpdatingEntity : IModEntity, IDisposable
    {
        private readonly ConcurrentDictionary<Type, IModifier> registeredModifiers;
        private readonly ModEntityManager modEntityManager;
        private readonly ILogger logger;

        /// <summary>
        /// Ctor for creating a <see cref="ModifierUpdatingEntity"/>
        /// </summary>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> instance to register to</param>
        /// <param name="logger">An <see cref="ILogger"/> implementation to log to</param>
        public ModifierUpdatingEntity(ModEntityManager modEntityManager, ILogger logger)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            registeredModifiers = new ConcurrentDictionary<Type, IModifier>();

            modEntityManager.AddEntity(this);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            modEntityManager.RemoveEntity(this);
        }

        /// <summary>
        /// Registers an <see cref="IModifier"/> implementation to keep track of
        /// </summary>
        /// <returns><c>true</c> if successfully registered, <c>false</c> if not</returns>
        public bool RegisterModifier(IModifier modifier)
        {
            Type modifierType = modifier.GetType();
            if (registeredModifiers.ContainsKey(modifierType))
            {
                return false;
            }

            return registeredModifiers.TryAdd(modifierType, modifier);
        }

        /// <summary>
        /// Unregisters an <see cref="IModifier"/> implementation to keep track of
        /// </summary>
        /// <returns><c>true</c> if successfully unregistered, <c>false</c> if not</returns>
        public bool UnregisterModifier(IModifier modifier)
        {
            Type modifierType = modifier.GetType();
            if (!registeredModifiers.ContainsKey(modifierType))
            {
                return false;
            }

            return registeredModifiers.TryRemove(modifierType, out _);
        }

        /// <summary>
        /// Called each frame by the Game, updates all active modifiers
        /// </summary>
        public void Update(float p_delta)
        {
            foreach (var kvp in registeredModifiers)
            {
                IModifier modifier = kvp.Value;
                if (modifier.IsModifierEnabled())
                {
                    modifier.Update();
                }
            }
        }

        /// <summary>
        /// Called each frame by the Game, does nothing
        /// </summary>
        public void Draw()
        {
            // do nothing
        }
    }
}
