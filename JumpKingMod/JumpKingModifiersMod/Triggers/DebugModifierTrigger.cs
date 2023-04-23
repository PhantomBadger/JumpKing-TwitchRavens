using JumpKingModifiersMod.API;
using JumpKingModifiersMod.Modifiers;
using JumpKingModifiersMod.Patching;
using JumpKingModifiersMod.Settings;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.API;
using PBJKModBase.Entities;
using Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    /// <summary>
    /// An implementation of <see cref="IModifierTrigger"/> and <see cref="IModEntity"/> which
    /// listens for keyboard input within the Monogame application as an entity to toggle a provided 
    /// modifier
    /// </summary>
    public class DebugModifierTrigger : IModifierTrigger, IModEntity, IDisposable
    {
        public event ModifierEnabledDelegate OnModifierEnabled;
        public event ModifierDisabledDelegate OnModifierDisabled;

        private readonly ModEntityManager modEntityManager;
        private readonly List<DebugTogglePair> modifierToggles;
        private readonly UserSettings userSettings;

        private bool isTriggerActive;

        /// <summary>
        /// Ctor for creating a <see cref="DebugModifierTrigger"/>
        /// </summary>
        /// <param name="modEntityManager">The <see cref="ModEntityManager"/> to register itself to</param>
        /// <param name="modifierToggles">A collection of <see cref="DebugTogglePair"/> to link a <see cref="IModifier"/> to a toggle key</param>
        internal DebugModifierTrigger(ModEntityManager modEntityManager,
            List<DebugTogglePair> modifierToggles, UserSettings userSettings)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            this.modifierToggles = modifierToggles ?? throw new ArgumentNullException(nameof(modifierToggles));
            this.userSettings = userSettings ?? throw new ArgumentNullException(nameof(userSettings));

            isTriggerActive = false;
            modEntityManager.AddEntity(this, 0);
        }

        /// <summary>
        /// Implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            modEntityManager.RemoveEntity(this);
        }

        /// <inheritdoc/>
        public bool DisableTrigger()
        {
            isTriggerActive = false;
            return true;
        }

        /// <inheritdoc/>
        public bool EnableTrigger()
        {
            isTriggerActive = true;
            return true;
        }

        /// <inheritdoc/>
        public bool IsTriggerEnabled()
        {
            return isTriggerActive;
        }

        /// <summary>
        /// Checks for keyboard input each frame, and toggles the modifier. Does nothing if the trigger is not active
        /// </summary>
        public void Update(float p_delta)
        {
            if (!isTriggerActive)
            {
                return;
            }

            // Toggle the modifiers
            var kbState = Keyboard.GetState();
            for (int i = 0; i < modifierToggles.Count; i++)
            {
                DebugTogglePair togglePair = modifierToggles[i];

                if (kbState.IsKeyDown(togglePair.ToggleKey))
                {
                    if (togglePair.ToggleKeyReset)
                    {
                        togglePair.ToggleKeyReset = false;
                        if (togglePair.Modifier.IsModifierEnabled())
                        {
                            if (togglePair.Modifier.DisableModifier())
                            {
                                OnModifierDisabled?.Invoke(togglePair.Modifier);
                            }
                        }
                        else
                        {
                            if (togglePair.Modifier.EnableModifier())
                            {
                                OnModifierEnabled?.Invoke(togglePair.Modifier);
                            }
                        }
                    }
                }
                else
                {
                    togglePair.ToggleKeyReset = true;
                }
            }
        }

        /// <inheritdoc/>
        public void Draw()
        {
            // Do nothing
        }
    }
}
