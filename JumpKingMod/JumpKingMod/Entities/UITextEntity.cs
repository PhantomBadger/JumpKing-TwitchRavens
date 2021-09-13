using EntityComponent;
using HarmonyLib;
using JumpKing;
using JumpKing.Util;
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
    public class UITextEntity : Entity, IDisposable
    {
        private readonly Func<Vector2> screenSpacePositionGetter;
        private readonly Vector2? screenSpacePosition;
        private readonly Func<string> textValueGetter;
        private readonly string textValue;
        private readonly Color textColor;
        private readonly SpriteFont textFont;

        /// <summary>
        /// Ctor for creating a <see cref="UITextEntity"/> with a dynamic position and text value, and the default font
        /// </summary>
        /// <param name="screenSpacePositionGetter">A Function which returns the screen space position to draw the text</param>
        /// <param name="textValueGetter">A function which returns the string to draw on the screen</param>
        /// <param name="textColor">The <see cref="Color"/> to draw the text in</param>
        public UITextEntity(Func<Vector2> screenSpacePositionGetter, Func<string> textValueGetter, Color textColor)
            : this(screenSpacePositionGetter, textValueGetter, textColor, JKContentManager.Font.MenuFontSmall)
        {

        }

        /// <summary>
        /// Ctor for creating a <see cref="UITextEntity"/> with a dynamic position and text value, and a specified font
        /// </summary>
        /// <param name="screenSpacePositionGetter">A Function which returns the screen space position to draw the text</param>
        /// <param name="textValueGetter">A function which returns the string to draw on the screen</param>
        /// <param name="textColor">The <see cref="Color"/> to draw the text in</param>
        /// <param name="textFont">The <see cref="SpriteFont"/> to use for drawing the text</param>
        public UITextEntity(Func<Vector2> screenSpacePositionGetter, Func<string> textValueGetter, Color textColor, SpriteFont textFont) :
            this(screenSpacePosition: null, textValue: null, textColor, textFont)
        {
            this.screenSpacePositionGetter = screenSpacePositionGetter ?? throw new ArgumentNullException(nameof(screenSpacePositionGetter));
            this.textValueGetter = textValueGetter ?? throw new ArgumentNullException(nameof(textValueGetter));
        }

        /// <summary>
        /// Ctor for creating a <see cref="UITextEntity"/> with a static position and text value, and a specified font
        /// </summary>
        /// <param name="screenSpacePosition">The static position to draw the text at</param>
        /// <param name="textValue">The static value to display as text</param>
        /// <param name="textColor">The <see cref="Color"/> to draw the text in</param>
        /// <param name="textFont">The <see cref="SpriteFont"/> to use for drawing the text</param>
        public UITextEntity(Vector2? screenSpacePosition, string textValue, Color textColor, SpriteFont textFont)
        {
            this.screenSpacePosition = screenSpacePosition;
            this.textValue = textValue;
            this.textColor = textColor;
            this.textFont = textFont ?? throw new ArgumentNullException(nameof(textFont));

            EntityManager.instance.AddObject(this);
        }

        /// <summary>
        /// An implementation of <see cref="IDisposable.Dispose"/>
        /// which removed itself from the Entity Manager and then destroys itself
        /// </summary>
        public void Dispose()
        {
            EntityManager.instance.RemoveObject(this);
            this.Destroy();
        }

        /// <summary>
        /// Called by the Entity Manager in the draw loop, uses <see cref="TextHelper.DrawString(SpriteFont, string, Vector2, Color, Vector2)"/> to render
        /// text to the screen
        /// </summary>
        public override void Draw()
        {
            TextHelper.DrawString(textFont,
                                  textValue ?? textValueGetter.Invoke(),
                                  screenSpacePosition ?? screenSpacePositionGetter.Invoke(),
                                  textColor,
                                  new Vector2(0.5f, -0.5f));
        }
    }
}
