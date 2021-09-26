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

namespace JumpKingMod.Entities
{
    public class MessengerRavenSpawningEntity : IModEntity, IDisposable
    {
        private readonly ModEntityManager modEntityManager;
        private readonly IMessengerRavenTrigger messengerRavenTrigger;
        private readonly ILogger logger;
        private readonly IRavenLandingPositionsCache ravenLandingPositionsCache;
        private readonly ConcurrentDictionary<MessengerRavenEntity, byte> messengerRavens;
        private readonly Random random;

        private bool clearRavensCooldown;
        private bool toggleRavensCooldown;
        private bool isRavenSpawningActive;

        private const int MaxRavenCount = 5;
        private const Keys ClearRavensKey = Keys.F6;
        private const Keys ToggleRavenSpawningKey = Keys.F7;

        public MessengerRavenSpawningEntity(ModEntityManager modEntityManager, IMessengerRavenTrigger messengerRavenTrigger, 
            ILogger logger)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.messengerRavenTrigger = messengerRavenTrigger ?? throw new ArgumentNullException(nameof(messengerRavenTrigger));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ravenLandingPositionsCache = new RavenLandingPositionsCache(logger);
            messengerRavens = new ConcurrentDictionary<MessengerRavenEntity, byte>();
            random = new Random();

            clearRavensCooldown = false;
            toggleRavensCooldown = false;
            isRavenSpawningActive = true;

            messengerRavenTrigger.OnMessengerRavenTrigger += OnMessengerRavenTrigger;
            modEntityManager.AddEntity(this);
        }

        public void Dispose()
        {
            modEntityManager?.RemoveEntity(this);
            messengerRavenTrigger.OnMessengerRavenTrigger -= OnMessengerRavenTrigger;
        }

        private void OnMessengerRavenTrigger(string ravenName, Color ravenNameColour, string ravenMessage)
        {
            // TODO: Object Pool
            if (messengerRavens.Count >= MaxRavenCount)
            {
                return;
            }

            // Are we even meant to be spawning anything?
            if (!isRavenSpawningActive)
            {
                return;
            }

            bool leftSide = random.Next(1, 3) % 2 == 0;
            float xPos = leftSide ? -50 : 530;
            Vector2 spawnPos = new Vector2(xPos, (-((Camera.CurrentScreen - 1) * 360) - 350));
            messengerRavens.TryAdd(new MessengerRavenEntity(spawnPos, ravenMessage, ravenName, ravenNameColour,
                modEntityManager, ravenLandingPositionsCache, logger), 0);
        }

        public void Update(float p_delta)
        {
            // Debug controls
            KeyboardState keyboardState = Keyboard.GetState();
            // Clear all ravens from the screen
            if (keyboardState.IsKeyDown(ClearRavensKey))
            {
                if (!clearRavensCooldown)
                {
                    logger.Information($"{ClearRavensKey.ToString()} Pressed - Clearing Ravens!");
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
            if (keyboardState.IsKeyDown(ToggleRavenSpawningKey))
            {
                if (!toggleRavensCooldown)
                {
                    logger.Information($"{ToggleRavenSpawningKey.ToString()} Pressed - {(isRavenSpawningActive ? "Disabling" : "Enabling")} Spawning of Ravens");
                    isRavenSpawningActive = !isRavenSpawningActive;

                    toggleRavensCooldown = true;
                }
            }
            else
            {
                toggleRavensCooldown = false;
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

        public void Draw()
        {
            // Nothing
        }
    }
}
