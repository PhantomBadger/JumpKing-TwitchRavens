using JumpKing;
using JumpKingMod.API;
using JumpKingMod.Entities.Raven;
using Logging.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JumpKingMod.Settings;
using Settings;

namespace JumpKingMod.Entities
{
    /// <summary>
    /// An implementation of <see cref="IModEntity"/> which spawns racens based on a provided trigger
    /// </summary>
    public class MessengerRavenSpawningEntity : IModEntity, IDisposable
    {
        private readonly UserSettings userSettings;
        private readonly ModEntityManager modEntityManager;
        private readonly List<IMessengerRavenTrigger> messengerRavenTriggers;
        private readonly ILogger logger;
        private readonly IRavenLandingPositionsCache ravenLandingPositionsCache;
        private readonly ConcurrentDictionary<MessengerRavenEntity, byte> messengerRavens;
        private readonly Random random;
        private readonly int maxRavenCount;
        private readonly Keys clearRavensKey;
        private readonly Keys toggleRavenSpawningKey;
        private readonly Keys toggleSubModeKey;

        private bool clearRavensCooldown;
        private bool toggleRavensCooldown;
        private bool toggleSubModeCooldown;
        private bool isRavenSpawningActive;
        private bool isInSubMode;

        /// <summary>
        /// Ctor for creating a <see cref="MessengerRavenSpawningEntity"/>
        /// </summary>
        public MessengerRavenSpawningEntity(UserSettings userSettings, ModEntityManager modEntityManager, 
            List<IMessengerRavenTrigger> messengerRavenTriggers, ILogger logger)
        {
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.messengerRavenTriggers = messengerRavenTriggers ?? throw new ArgumentNullException(nameof(messengerRavenTriggers));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ravenLandingPositionsCache = new RavenLandingPositionsCache(logger);
            messengerRavens = new ConcurrentDictionary<MessengerRavenEntity, byte>();
            random = new Random();

            clearRavensCooldown = false;
            toggleRavensCooldown = false;
            isRavenSpawningActive = true;

            maxRavenCount = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensMaxCountKey, 5);
            clearRavensKey = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensClearDebugKeyKey, Keys.F2);
            toggleRavenSpawningKey = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensToggleDebugKeyKey, Keys.F3);
            toggleSubModeKey = userSettings.GetSettingOrDefault(JumpKingModSettingsContext.RavensSubModeToggleKeyKey, Keys.F4);
            isInSubMode = false;

            for (int i = 0; i < messengerRavenTriggers.Count; i++)
            {
                messengerRavenTriggers[i].OnMessengerRavenTrigger += OnMessengerRavenTrigger;
            }
            modEntityManager.AddEntity(this);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            modEntityManager?.RemoveEntity(this);
            for (int i = 0; i < messengerRavenTriggers.Count; i++)
            {
                messengerRavenTriggers[i].OnMessengerRavenTrigger -= OnMessengerRavenTrigger;
            }
        }

        /// <summary>
        /// Called by the Trigger implementation, spawns the raven
        /// </summary>
        private void OnMessengerRavenTrigger(string ravenName, Color ravenNameColour, string ravenMessage, bool isFromSubscriber)
        {
            if (messengerRavens.Count >= maxRavenCount)
            {
                return;
            }

            // Are we even meant to be spawning anything?
            if (!isRavenSpawningActive)
            {
                return;
            }

            // If we are gating behind subscribers and this isn't from a subscriber
            if (isInSubMode && !isFromSubscriber)
            {
                return;
            }

            bool leftSide = random.Next(1, 3) % 2 == 0;
            float xPos = leftSide ? -50 : 530;
            Vector2 spawnPos = new Vector2(xPos, (-((Camera.CurrentScreen - 1) * 360) - 350));
            messengerRavens.TryAdd(new MessengerRavenEntity(spawnPos, ravenMessage, ravenName, ravenNameColour,
                modEntityManager, ravenLandingPositionsCache, logger), 0);
        }

        /// <summary>
        /// Called each frame by the Mod Entity Manager, allows debug controls and cleans up the ravens
        /// </summary>
        public void Update(float delta)
        {
            // Debug controls
            KeyboardState keyboardState = Keyboard.GetState();
            // Clear all ravens from the screen
            if (keyboardState.IsKeyDown(clearRavensKey))
            {
                if (!clearRavensCooldown)
                {
                    logger.Information($"{clearRavensKey.ToString()} Pressed - Clearing Ravens!");
                    foreach (var raven in messengerRavens)
                    {
                        if (raven.Key != null)
                        {
                            raven.Key.ReadyToBeDestroyed = true;
                        }
                    }

                    clearRavensCooldown = true;
                }
            }
            else
            {
                clearRavensCooldown = false;
            }

            // Toggle Ravens
            if (keyboardState.IsKeyDown(toggleRavenSpawningKey))
            {
                if (!toggleRavensCooldown)
                {
                    logger.Information($"{toggleRavenSpawningKey.ToString()} Pressed - {(isRavenSpawningActive ? "Disabling" : "Enabling")} Spawning of Ravens");
                    isRavenSpawningActive = !isRavenSpawningActive;

                    toggleRavensCooldown = true;
                }
            }
            else
            {
                toggleRavensCooldown = false;
            }    

            // Sub Mode
            if (keyboardState.IsKeyDown(toggleSubModeKey))
            {
                if (!toggleSubModeCooldown)
                {
                    logger.Information($"{toggleSubModeKey.ToString()} Pressed - {(isInSubMode ? "Disabling" : "Enabling")} Sub-Only mode for Raven Spawning!");
                    isInSubMode = !isInSubMode;

                    toggleSubModeCooldown = true;
                }
            }
            else
            {
                toggleSubModeCooldown = false;
            }

            // Identify ravens we wanna destroy
            List<MessengerRavenEntity> ravensToDestroy = new List<MessengerRavenEntity>();
            foreach (var raven in messengerRavens)
            {
                if (raven.Key.ReadyToBeDestroyed)
                {
                    ravensToDestroy.Add(raven.Key);
                }
            }

            // Destroy the ravens
            foreach (var raven in ravensToDestroy)
            {
                if (raven != null)
                {
                    raven.Dispose();
                    messengerRavens.TryRemove(raven, out _);
                }
            }
        }

        /// <summary>
        /// Called each frame by the Mod Entity Manager, doesnt draw anything
        /// </summary>
        public void Draw()
        {
            // Nothing
        }
    }
}
