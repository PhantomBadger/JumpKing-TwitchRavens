using EntityComponent;
using HarmonyLib;
using JumpKing;
using JumpKing.Util;
using JumpKing.Util.Tags;
using JumpKingMod.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JumpKingMod.Entities
{
    /// <summary>
    /// An extension of <see cref="Entity"/> which when instantiated
    /// registers itself with the entity manager, and draws a set piece of text to the screen
    /// every draw call
    /// </summary>
    public class UITextEntity : IDisposable, IForegroundModEntity
    {
        public Vector2 ScreenSpacePosition { get; set; }
        public string TextValue { get; set; }
        public Color TextColor { get; set; }
        public SpriteFont TextFont { get; set; }
        public UITextEntityAnchor AnchorPoint { get; set; }
        public Vector2 Size
        {
            get
            {
                return TextFont?.MeasureString(TextValue) ?? new Vector2(0, 0);
            }
        }

        private readonly ModEntityManager modEntityManager;

        /// <summary>
        /// Ctor for creating a <see cref="UITextEntity"/> with a position and text value, and the default font
        /// </summary>
        /// <param name="screenSpacePosition">The position to draw the text at</param>
        /// <param name="textValue">The value to display as text</param>
        /// <param name="textColor">The <see cref="Color"/> to draw the text in</param>
        /// <param name="anchorPoint">An enum of possible anchor points of where the position is relative to the text</param>
        public UITextEntity(ModEntityManager modEntityManager, Vector2 screenSpacePosition, string textValue, Color textColor, UITextEntityAnchor anchorPoint)
            : this(modEntityManager, screenSpacePosition, textValue, textColor, anchorPoint, JKContentManager.Font.MenuFont)
        {
        }


        /// <summary>
        /// Ctor for creating a <see cref="UITextEntity"/> with a position and text value, and a specified font
        /// </summary>
        /// <param name="screenSpacePosition">The position to draw the text at</param>
        /// <param name="textValue">The value to display as text</param>
        /// <param name="textColor">The <see cref="Color"/> to draw the text in</param>
        /// <param name="textFont">The <see cref="SpriteFont"/> to use for drawing the text</param>
        /// <param name="anchorPoint">An enum of possible anchor points of where the position is relative to the text</param>
        public UITextEntity(ModEntityManager modEntityManager, Vector2 screenSpacePosition, string textValue, Color textColor, UITextEntityAnchor anchorPoint, SpriteFont textFont)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));

            ScreenSpacePosition = screenSpacePosition;
            TextValue = textValue;
            TextColor = textColor;
            AnchorPoint = anchorPoint;
            TextFont = textFont ?? throw new ArgumentNullException(nameof(textFont));

            modEntityManager.AddForegroundEntity(this);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/>
        /// which removed itself from the Entity Manager and then destroys itself
        /// </summary>
        public void Dispose()
        {
            modEntityManager.RemoveForegroundEntity(this);
        }

        /// <summary>
        /// Gets the appropriate anchor position based on the set <see cref="AnchorPoint"/>
        /// </summary>
        private Vector2 GetAnchorVector()
        {
            switch (AnchorPoint)
            {
                default:
                case UITextEntityAnchor.Center:
                    return new Vector2(0.5f, -0.5f);
                case UITextEntityAnchor.BottomLeft:
                    return new Vector2(0, 0);
                case UITextEntityAnchor.BottomRight:
                    return new Vector2(1, 0);
                case UITextEntityAnchor.TopLeft:
                    return new Vector2(0, -1);
                case UITextEntityAnchor.TopRight:
                    return new Vector2(1, -1);
            }
        }

        /// <summary>
        /// Called by the Entity Manager in the draw loop, uses <see cref="TextHelper.DrawString(SpriteFont, string, Vector2, Color, Vector2)"/> to render
        /// text to the screen
        /// </summary>
        public void ForegroundDraw()
        {
            TextHelper.DrawString(TextFont, TextValue, ScreenSpacePosition, TextColor, GetAnchorVector());
        }

        /// <summary>
        /// Update method, do nothing
        /// </summary>
        public void Update(float delta)
        {
            // Do Nothing
        }
    }
}
