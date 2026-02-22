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

            int lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            if (height < lineH + 16)
                height = lineH + 16;

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
            int lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            if (height < lineH + 16)
                height = lineH + 16;

            this.TextBox.X = x;
            this.TextBox.Y = y;
            this.TextBox.Width = width;
            this.TextBox.Height = height;
            this.Bounds.bounds = new Rectangle(x, y, width, height);
        }

        public void Draw(SpriteBatch b)
        {
            int pad = 4;
            int bx = this.TextBox.X - pad;
            int by = this.TextBox.Y - pad;
            int bw = this.TextBox.Width + pad * 2;
            int bh = this.TextBox.Height + pad * 2;

            // Simple filled background with thin border
            Color bgColor = this.IsSelected ? new Color(255, 250, 230) : new Color(250, 245, 235);
            b.Draw(Game1.staminaRect, new Rectangle(bx, by, bw, bh), bgColor);

            // Border lines
            Color borderColor = this.IsSelected ? new Color(180, 140, 60) : new Color(160, 130, 90) * 0.6f;
            b.Draw(Game1.staminaRect, new Rectangle(bx, by, bw, 2), borderColor);
            b.Draw(Game1.staminaRect, new Rectangle(bx, by + bh - 2, bw, 2), borderColor);
            b.Draw(Game1.staminaRect, new Rectangle(bx, by, 2, bh), borderColor);
            b.Draw(Game1.staminaRect, new Rectangle(bx + bw - 2, by, 2, bh), borderColor);

            this.TextBox.Draw(b);

            // Placeholder text
            if (string.IsNullOrEmpty(this.TextBox.Text) && !this.TextBox.Selected && !string.IsNullOrEmpty(this.placeholder))
            {
                b.DrawString(Game1.smallFont, this.placeholder,
                    new Vector2(this.TextBox.X + 12, this.TextBox.Y + 8),
                    Color.Gray * 0.5f);
            }
        }
    }
}
