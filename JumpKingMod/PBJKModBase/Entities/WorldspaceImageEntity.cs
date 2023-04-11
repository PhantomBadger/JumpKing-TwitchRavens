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
    /// <summary>
    /// An implementation of <see cref="IForegroundModEntity"/> which displays an image in worldspace
    /// </summary>
    public class WorldspaceImageEntity : IForegroundModEntity
    {
        /// <summary>
        /// The world space position of the entity
        /// </summary>
        public Vector2 WorldSpacePosition { get; set; }

        /// <summary>
        /// The image for the entity to draw
        /// </summary>
        public Sprite ImageValue { get; set; }

        private readonly ModEntityManager modEntityManager;

        /// <summary>
        /// Ctor for creating a <see cref="WorldspaceImageEntity"/>
        /// </summary>
        /// <param name="modEntityManager">A <see cref="ModEntityManager"/> instance to register with</param>
        /// <param name="worldSpacePosition">A world space position to draw at</param>
        /// <param name="imageValue">An image to draw at the set position</param>
        /// <param name="zOrder">The Z order to draw the sprite</param>
        public WorldspaceImageEntity(ModEntityManager modEntityManager, Vector2 worldSpacePosition, Sprite imageValue, int zOrder = 0)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));
            WorldSpacePosition = worldSpacePosition;
            ImageValue = imageValue ?? throw new ArgumentNullException(nameof(imageValue));

            modEntityManager.AddForegroundEntity(this, zOrder);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/> to clean up
        /// </summary>
        public void Dispose()
        {
            modEntityManager.RemoveForegroundEntity(this);
        }

        /// <inheritdoc/>
        public void ForegroundDraw()
        {
            // JK+ Changes some of the method signatures so we have to call a different one and pray
            ImageValue.Draw(Camera.TransformVector2(WorldSpacePosition).ToPoint(), Microsoft.Xna.Framework.Graphics.SpriteEffects.None);
        }

        /// <inheritdoc/>
        public void Update(float p_delta)
        {
            // Do Nothing
        }
    }
}
