using JumpKingModifiersMod.API;
using Logging.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Modifiers
{
    /// <summary>
    /// An implementation of <see cref="IModifier"/> and <see cref="IDisposable"/> which has lava slowly rise from the bottom of the map. Touching the lava will mean death and a reset!
    /// </summary>
    [Obsolete("Not yet complete!")]
    public class RisingLavaModifier : IModifier, IDisposable
    {
        public string DisplayName => "Rising Lava";

        private enum RisingLavaModifierState
        {
            Rising,                         // The Lava is rising as normal, we check for player contact
            PlayerDeathAnim,                // The player is forced into a Mario-esque death bounce
            FadingCutout,                   // A cutout fades across the screen like a Bowser cutout would in Mario
            DisplayingYouDied,              // Displaying the primary "You Died" text
            DisplayingYouDiedSubtext,       // Displaying the secondary "You Died" subtext
            WaitingForInput                 // Waiting for user input to reset
        }

        private readonly ModifierUpdatingEntity modifierUpdatingEntity;
        private readonly IPlayerStateObserver playerStateObserver;
        private readonly IYouDiedSubtextGetter youDiedSubtextGetter;
        private readonly ILogger logger;
        private readonly Random random;

        private UIImageEntity lavaEntity;
        private RisingLavaModifierState lavaModifierState;

        public RisingLavaModifier(ModifierUpdatingEntity modifierUpdatingEntity, IPlayerStateObserver playerStateObserver, IYouDiedSubtextGetter youDiedSubtextGetter, ILogger logger)
        {
            this.modifierUpdatingEntity = modifierUpdatingEntity ?? throw new ArgumentNullException(nameof(modifierUpdatingEntity));
            this.playerStateObserver = playerStateObserver ?? throw new ArgumentException(nameof(playerStateObserver));
            this.youDiedSubtextGetter = youDiedSubtextGetter ?? throw new ArgumentNullException(nameof(youDiedSubtextGetter));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            random = new Random(DateTime.Now.Second + DateTime.Now.Millisecond);

            lavaEntity = null;

            modifierUpdatingEntity.RegisterModifier(this);
        }

        public void Dispose()
        {
            modifierUpdatingEntity.UnregisterModifier(this);
        }

        public bool IsModifierEnabled()
        {
            return lavaEntity != null;
        }

        public bool DisableModifier()
        {
            throw new NotImplementedException();
        }

        public bool EnableModifier()
        {
            throw new NotImplementedException();
        }

        public void Update(float p_delta)
        {
            throw new NotImplementedException();
        }
    }
}
