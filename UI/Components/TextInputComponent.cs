using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StarDo.UI.Components
{
    public class TextInputComponent
    {
        public TextBox TextBox { get; private set; }
        public ClickableComponent Bounds { get; private set; }

        private readonly string placeholder;

        public string Text
        {
            get => this.TextBox.Text;
            set => this.TextBox.Text = value;
        }

        public TextInputComponent(int x, int y, int width, int height, string placeholder = "")
        {
            this.placeholder = placeholder;
            this.TextBox = new TextBox(null, null, Game1.smallFont, Game1.textColor)
            {
                X = x,
                Y = y,
                Width = width,
                Height = height,
                Text = "",
                Selected = false,
                limitWidth = true
            };

            this.Bounds = new ClickableComponent(
                new Rectangle(x, y, width, height), ""
            );
        }

        public void Select()
        {
            this.TextBox.Selected = true;
            Game1.keyboardDispatcher.Subscriber = this.TextBox;
        }

        public void Deselect()
        {
            this.TextBox.Selected = false;
            if (Game1.keyboardDispatcher.Subscriber == this.TextBox)
                Game1.keyboardDispatcher.Subscriber = null;
        }

        public bool IsSelected => this.TextBox.Selected;

        public bool ContainsPoint(int x, int y)
        {
            return this.Bounds.containsPoint(x, y);
        }

        public void Reposition(int x, int y, int width, int height)
        {
            this.TextBox.X = x;
            this.TextBox.Y = y;
            this.TextBox.Width = width;
            this.TextBox.Height = height;
            this.Bounds.bounds = new Rectangle(x, y, width, height);
        }

        public void Draw(SpriteBatch b)
        {
            // Draw background
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                this.TextBox.X - 8, this.TextBox.Y - 4,
                this.TextBox.Width + 16, this.TextBox.Height + 8,
                Color.White, 4f, false);

            this.TextBox.Draw(b);

            // Draw placeholder if empty and not selected
            if (string.IsNullOrEmpty(this.TextBox.Text) && !this.TextBox.Selected && !string.IsNullOrEmpty(this.placeholder))
            {
                b.DrawString(Game1.smallFont, this.placeholder,
                    new Vector2(this.TextBox.X + 8, this.TextBox.Y + 4),
                    Color.Gray * 0.6f);
            }
        }
    }
}
