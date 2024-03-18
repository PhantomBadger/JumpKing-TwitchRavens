using BehaviorTree;
using JumpKing;
using JumpKing.Controller;
using JumpKing.PauseMenu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;

namespace JumpKingRavensMod.Button
{
    public class DelegateButton : IBTdecorator, IMenuItem
    {
        public SpriteFont Font { get; private set; }

        public Color Color { get; private set; }

        public string Text { get; private set; }

        public Action OnClick { get; private set; }

        public bool DisplayExploreTexture { get; private set; }

        public DelegateButton(string text, bool displayExploreTexture, Action onClick) 
            : this(text, Game1.instance.contentManager.font.MenuFont, Color.White, displayExploreTexture, onClick)
        {

        }

        public DelegateButton(string text, Color color, bool displayExploreTexture, Action onClick)
            : this(text, Game1.instance.contentManager.font.MenuFont, color, displayExploreTexture, onClick)
        {

        }

        public DelegateButton(string text, SpriteFont font, Color color, bool displayExploreTexture, Action onClick)
        {
            Text = text;
            Font = font;
            Color = color;
            DisplayExploreTexture = displayExploreTexture;
            OnClick = onClick;
        }

        public void Draw(int x, int y, bool selected)
        {
            Game1.spriteBatch.DrawString(this.Font, this.Text, new Vector2((float)x, (float)y), this.Color);
            Point size = this.Font.MeasureString(this.Text).ToPoint();
            if (DisplayExploreTexture)
            {
                Game1.spriteBatch.Draw(Game1.instance.contentManager.gui.Explore.texture, new Vector2((float)(x + size.X + 2), (float)y + 2), Color);
            }
        }

        public Point GetSize()
        {
            Vector2 str = this.Font.MeasureString(this.Text);
            Vector2 icn = Vector2.Zero;
            if (DisplayExploreTexture)
            {
                icn = new Vector2(Game1.instance.contentManager.gui.Explore.texture.Width + 2, 0);
            }
            return Vector2.Add(str, icn).ToPoint();
        }

        protected override BTresult MyRun(TickData p_data)
        {
            if (base.last_result == BTresult.Running)
            {
                return base.Child.Run(p_data);
            }
            if (ControllerManager.instance.MenuController.GetPadState().confirm)
            {
                OnClick?.Invoke();
                return BTresult.Success;
            }
            return BTresult.Failure;
        }
    }
}
