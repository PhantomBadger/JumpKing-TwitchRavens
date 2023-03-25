using JumpKing;
using Microsoft.Xna.Framework;
using PBJKModBase.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PBJKModBase.Entities
{
    public class WorldspaceImageEntity : IForegroundModEntity
    {
        public Vector2 WorldSpacePosition { get; set; }
        public Sprite ImageValue { get; set; }

        private readonly ModEntityManager modEntityManager;

        public WorldspaceImageEntity(ModEntityManager modEntityManager, Vector2 worldSpacePosition, Sprite imageValue, int zOrder = 0)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            WorldSpacePosition = worldSpacePosition;
            ImageValue = imageValue ?? throw new ArgumentNullException(nameof(imageValue));

            modEntityManager.AddForegroundEntity(this, zOrder);
        }

        public void Dispose()
        {
            modEntityManager.RemoveForegroundEntity(this);
        }

        public void ForegroundDraw()
        {
            // JK+ Changes some of the method signatures so we have to call a different one and pray
            ImageValue.Draw(Camera.TransformVector2(WorldSpacePosition).ToPoint(), Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
        }

        public void Update(float p_delta)
        {
            // Do Nothing
        }
    }
}
