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
    public class UIImageEntity : IForegroundModEntity, IDisposable
    {
        public Vector2 ScreenSpacePosition { get; set; }
        public Rectangle? DestinationRectangle { get; set; }
        public Sprite ImageValue { get; set; }

        private readonly ModEntityManager modEntityManager;

        public UIImageEntity(ModEntityManager modEntityManager, Vector2 screenSpacePosition, Sprite imageValue, int zOrder = 0)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            ScreenSpacePosition = screenSpacePosition;
            ImageValue = imageValue ?? throw new ArgumentNullException(nameof(imageValue));

            modEntityManager.AddForegroundEntity(this, zOrder);
        }

        public UIImageEntity(ModEntityManager modEntityManager, Rectangle destinationRectangle, Sprite imageValue, int zOrder = 0)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            DestinationRectangle = destinationRectangle;
            ImageValue = imageValue ?? throw new ArgumentNullException(nameof(imageValue));

            modEntityManager.AddForegroundEntity(this, zOrder);
        }

        public void Dispose()
        {
            modEntityManager.RemoveForegroundEntity(this);
        }

        public void ForegroundDraw()
        {
            if (!DestinationRectangle.HasValue)
            {
                ImageValue.Draw(ScreenSpacePosition);
            }
            else
            {
                ImageValue.Draw(DestinationRectangle.Value);
            }
        }

        public void Update(float p_delta)
        {
            // Do Nothing
        }
    }
}
