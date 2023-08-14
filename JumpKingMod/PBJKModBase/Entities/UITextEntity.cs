using EntityComponent;
using JumpKing;
using JumpKing.Util;
using PBJKModBase.API;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;
using static Microsoft.Xna.Framework.Graphics.SpriteFont;

namespace PBJKModBase.Entities
{
    /// <summary>
    /// An extension of <see cref="Entity"/> which when instantiated
    /// registers itself with the entity manager, and draws a set piece of text to the screen
    /// every draw call
    /// </summary>
    public class UITextEntity : IDisposable, IForegroundModEntity
    {
        public Vector2 ScreenSpacePosition { get; set; }
        public string TextValue 
        { 
            get
            {
                return textValue;
            }
            set
            {
                if (textValue != value)
                {
                    textValue = value;
                    formattedText = WrapText(textValue);
                    shouldUseFallbackFont = ShouldUseFallbackFont(formattedText);
                }
            }
        }
        public Color TextColor { get; set; }
        public SpriteFont TextFont 
        { 
            get
            {
                return shouldUseFallbackFont ? ModContentManager.ArialUnicodeMS : textFont;
            }
            set
            {
                if (textFont != value)
                {
                    textFont = value;
                    targetFontCharacterLookup = textFont.GetGlyphs();
                    formattedText = WrapText(textValue);
                    shouldUseFallbackFont = ShouldUseFallbackFont(formattedText);
                }
            }
        }
        public UIEntityAnchor AnchorPoint { get; set; }
        public Vector2 Size
        {
            get
            {
                return TextFont?.MeasureString(formattedText) ?? new Vector2(0, 0);
            }
        }

        public bool IsEnabled { get; set; }

        protected readonly ModEntityManager modEntityManager;

        protected SpriteFont textFont;
        protected string textValue;
        protected string formattedText;
        protected Dictionary<char, Glyph> targetFontCharacterLookup;
        protected bool shouldUseFallbackFont;

        protected const float MaxWidthOfLine = 300;

        /// <summary>
        /// Ctor for creating a <see cref="UITextEntity"/> with a position and text value, and the default font
        /// </summary>
        /// <param name="screenSpacePosition">The position to draw the text at</param>
        /// <param name="textValue">The value to display as text</param>
        /// <param name="textColor">The <see cref="Color"/> to draw the text in</param>
        /// <param name="anchorPoint">An enum of possible anchor points of where the position is relative to the text</param>
        /// <param name="zOrder">The z order to display the entity in the foreground queue. Defaults to zero. Higher number is closer to the screen.</param>
        public UITextEntity(ModEntityManager modEntityManager, Vector2 screenSpacePosition, string textValue, Color textColor, UIEntityAnchor anchorPoint, int zOrder = 0)
            : this(modEntityManager, screenSpacePosition, textValue, textColor, anchorPoint, JKContentManager.Font.MenuFont, zOrder)
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
        /// <param name="zOrder">The z order to display the entity in the foreground queue. Defaults to zero. Higher number is closer to the screen.</param>
        public UITextEntity(ModEntityManager modEntityManager, Vector2 screenSpacePosition, string textValue, Color textColor, UIEntityAnchor anchorPoint, SpriteFont textFont, int zOrder = 0)
        {
            this.modEntityManager = modEntityManager ?? throw new ArgumentNullException(nameof(modEntityManager));

            ScreenSpacePosition = screenSpacePosition;
            TextValue = textValue;
            TextColor = textColor;
            AnchorPoint = anchorPoint;
            TextFont = textFont ?? throw new ArgumentNullException(nameof(textFont));
            IsEnabled = true;

            modEntityManager.AddForegroundEntity(this, zOrder);
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
        protected Vector2 GetAnchorVector()
        {
            switch (AnchorPoint)
            {
                default:
                case UIEntityAnchor.Center:
                    return new Vector2(0.5f, -0.5f);
                case UIEntityAnchor.BottomLeft:
                    return new Vector2(0, 0);
                case UIEntityAnchor.BottomRight:
                    return new Vector2(1, 0);
                case UIEntityAnchor.TopLeft:
                    return new Vector2(0, -1);
                case UIEntityAnchor.TopRight:
                    return new Vector2(1, -1);
            }
        }

        /// <summary>
        /// Called by the Entity Manager in the draw loop, uses <see cref="TextHelper.DrawString(SpriteFont, string, Vector2, Color, Vector2)"/> to render
        /// text to the screen
        /// </summary>
        public virtual void ForegroundDraw()
        {
            if (IsEnabled)
            {
                Vector2 textSize = Size;
                Vector2 modifiedPosition = ScreenSpacePosition - new Vector2(0, textSize.Y);
                if (AnchorPoint == UIEntityAnchor.Center)
                {
                    modifiedPosition.Y -= textSize.Y / 2f;
                }
                TextHelper.DrawString(TextFont, formattedText, modifiedPosition, TextColor, GetAnchorVector());
            }
        }

        /// <summary>
        /// Update method, do nothing
        /// </summary>
        public virtual void Update(float delta)
        {
            // Do Nothing
        }

        /// <summary>
        /// Given an input string, creates a new string with newlines added based on the 
        /// calculated size of the string with the <see cref="TextFont"/> currently set
        /// </summary>
        protected string WrapText(string input)
        {
            if (TextFont == null || input == null)
            {
                return input;
            }

            string textToWrap = input ?? "";
            StringBuilder formattedStringBuilder = new StringBuilder();
            List<string> wordsInText = new List<string>(TextSplit(textValue, new char[] { ' ' }));

            float runningWidthCounter = 0;
            for (int i = 0; i < wordsInText.Count; i++)
            {
                string word = wordsInText[i];
                Vector2 sizeOfWord = TextFont.MeasureString(wordsInText[i]);
                runningWidthCounter += sizeOfWord.X;

                // If when we add this word, we are above the max width
                if (runningWidthCounter > MaxWidthOfLine)
                {
                    if (sizeOfWord.X > MaxWidthOfLine)
                    {
                        // The word on its own is too big!
                        // We need to find the point at which it exceeds the limit, 
                        // and add a new line there
                        StringBuilder sb = new StringBuilder();
                        List<int> indexOfSplits = new List<int>();
                        float widthOfCandidate = 0;
                        for (int j = 0; j < word.Length; j++)
                        {
                            sb.Append(word[j]);
                            Vector2 sizeOfCandidate = TextFont.MeasureString(sb.ToString());
                            widthOfCandidate = sizeOfCandidate.X;

                            if (widthOfCandidate > MaxWidthOfLine)
                            {
                                // Here is where we need to split it!
                                indexOfSplits.Add(j);
                                sb.Clear();
                            }
                        }
                        for (int j = 0; j < indexOfSplits.Count; j++)
                        {
                            word = word.Insert(indexOfSplits[j] + (j * 2), "-\n");
                        }
                        runningWidthCounter = sizeOfWord.X - widthOfCandidate;
                    }
                    else
                    {
                        // The word is a reasonable size, so we can newline it on its own
                        word = $"\n{word}";
                        runningWidthCounter = sizeOfWord.X;
                    }
                }
                formattedStringBuilder.Append(word);
            }

            return formattedStringBuilder.ToString();
        }

        /// <summary>
        /// Splits the given string based on the provided deliminators, but keeps
        /// the deliminators in the resulting split string array
        /// </summary>
        protected IEnumerable<string> TextSplit(string inputText, char[] deliminators)
        {
            int startIndex = 0;
            int index = 0;

            while ((index = inputText.IndexOfAny(deliminators, startIndex)) != -1)
            {
                if (index - startIndex > 0)
                {
                    yield return inputText.Substring(startIndex, index - startIndex);
                }
                yield return inputText.Substring(index, 1);
                startIndex = index + 1;
            }

            if (startIndex < inputText.Length)
            {
                yield return inputText.Substring(startIndex);
            }
        }

        /// <summary>
        /// Determines whether we should use the fallback font for this string or not
        /// </summary>
        protected bool ShouldUseFallbackFont(string input)
        {
            if (targetFontCharacterLookup == null)
            {
                return true;
            }

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == '\n')
                {
                    continue;
                }

                if (!targetFontCharacterLookup.ContainsKey(input[i]))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
