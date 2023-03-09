using JumpKingModifiersMod.API;
using Microsoft.Xna.Framework.Input;
using PBJKModBase.API;
using PBJKModBase.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JumpKingModifiersMod.Triggers
{
    public class DebugModifierTrigger : IModEntity
    {
        private IModifier modifier;
        private bool pressedCooldown = false;

        public DebugModifierTrigger(ModEntityManager modEntityManager, IModifier modifier)
        {
            modEntityManager.AddEntity(this);
            this.modifier = modifier ?? throw new ArgumentNullException(nameof(modifier));
        }

        public void Draw()
        {
            return;
        }

        public void Update(float p_delta)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.L))
            {
                if (!pressedCooldown)
                {
                    pressedCooldown = true;
                    if (modifier.IsModifierEnabled())
                    {
                        modifier.DisableModifier();
                    }
                    else
                    {
                        modifier.EnableModifier();
                    }
                }
            }
            else
            {
                pressedCooldown = false;
            }
        }
    }
}
