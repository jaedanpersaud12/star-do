using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StarDo.Models;

namespace StarDo.UI.Components
{
    public static class TaskRowComponent
    {
        // Computed once from font metrics
        private static int lineHeight = -1;
        private static int LineH
        {
            get
            {
                if (lineHeight < 0)
                    lineHeight = (int)Game1.smallFont.MeasureString("Tg").Y;
                return lineHeight;
            }
        }

        // Box border sizes: 15x15 src at 4x = 20px per side, 6x6 src at 4x = 8px per side
        private const int BoxPad = 20;
        private const int SmallBoxPad = 8;

        public static int RowHeight => LineH * 3 + 8;
        private static int CheckboxSize => LineH + SmallBoxPad * 2;
        private const int Padding = 16;

        private static readonly string[] CategoryLabels = { "Farm", "Proc", "Social", "Goals", "Quest" };
        private static readonly Color[] CategoryColors =
        {
            new Color(76, 153, 0),
            new Color(178, 102, 0),
            new Color(204, 51, 153),
            new Color(51, 102, 204),
            new Color(153, 102, 204),
        };

        private static readonly string[] PriorityLabels = { "Daily", "Weekly", "Long" };
        private static readonly Color[] PriorityColors =
        {
            new Color(204, 51, 51),
            new Color(204, 153, 0),
            new Color(102, 102, 102),
        };

        public static Rectangle GetCheckboxBounds(int rowX, int rowY)
        {
            int size = CheckboxSize;
            return new Rectangle(
                rowX + BoxPad + 4,
                rowY + (RowHeight - size) / 2,
                size, size
            );
        }

        public static Rectangle GetRowBounds(int x, int y, int width)
        {
            return new Rectangle(x, y, width, RowHeight);
        }

        public static void Draw(SpriteBatch b, PlannerTask task, int x, int y, int width, bool isHovered, bool dimmed = false)
        {
            float alpha = dimmed ? 0.5f : 1f;
            Color bgColor = isHovered ? Color.Wheat * alpha : Color.White * alpha;

            // Row background
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                x, y, width, RowHeight,
                bgColor, 4f, false);

            // Checkbox
            var cbBounds = GetCheckboxBounds(x, y);
            DrawCheckbox(b, cbBounds, task.IsCompleted, alpha);

            int contentX = cbBounds.Right + Padding;
            int contentWidth = x + width - contentX - BoxPad;

            // Badge row (top half)
            int badgeY = y + BoxPad;
            int badgeH = LineH + 4;

            // Category badge
            int catIdx = (int)task.Category;
            string catLabel = CategoryLabels[catIdx];
            Color catColor = CategoryColors[catIdx] * alpha;
            int catBadgeW = (int)Game1.smallFont.MeasureString(catLabel).X + SmallBoxPad * 2 + 4;
            DrawBadge(b, catLabel, contentX, badgeY, catBadgeW, badgeH, catColor);

            // Priority badge
            int priIdx = (int)task.Priority;
            string priLabel = PriorityLabels[priIdx];
            Color priColor = PriorityColors[priIdx] * alpha;
            int priBadgeX = contentX + catBadgeW + 8;
            int priBadgeW = (int)Game1.smallFont.MeasureString(priLabel).X + SmallBoxPad * 2 + 4;
            DrawBadge(b, priLabel, priBadgeX, badgeY, priBadgeW, badgeH, priColor);

            // Recurring indicator
            if (task.IsRecurring)
            {
                int recurX = priBadgeX + priBadgeW + 8;
                Utility.drawTextWithShadow(b, "[R]", Game1.smallFont,
                    new Vector2(recurX, badgeY + 2), Color.DarkCyan * alpha, 1f, -1f, -1, -1, 1f, 3);
            }

            // Title (bottom half)
            int titleY = badgeY + badgeH + 4;
            Color titleColor = dimmed ? Color.Gray : Game1.textColor;

            if (dimmed)
            {
                var titleSize = Game1.smallFont.MeasureString(task.Title);
                b.DrawString(Game1.smallFont, task.Title, new Vector2(contentX, titleY), titleColor * alpha);
                int strikeY = titleY + (int)(titleSize.Y / 2);
                b.Draw(Game1.staminaRect, new Rectangle(contentX, strikeY, Math.Min((int)titleSize.X, contentWidth), 2), Color.Gray * alpha);
            }
            else
            {
                Utility.drawTextWithShadow(b, task.Title, Game1.smallFont,
                    new Vector2(contentX, titleY), titleColor, 1f, -1f, -1, -1, 1f, 3);
            }

            // Sub-task count (right-aligned)
            if (task.SubTasks.Count > 0)
            {
                int completedSubs = 0;
                foreach (var s in task.SubTasks)
                    if (s.IsCompleted) completedSubs++;

                string subText = $"{completedSubs}/{task.SubTasks.Count}";
                var subSize = Game1.smallFont.MeasureString(subText);
                b.DrawString(Game1.smallFont, subText,
                    new Vector2(x + width - subSize.X - BoxPad - 8, y + (RowHeight - subSize.Y) / 2),
                    Color.Gray * alpha);
            }
        }

        private static void DrawCheckbox(SpriteBatch b, Rectangle bounds, bool isChecked, float alpha)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                Color.White * alpha, 4f, false);

            if (isChecked)
            {
                int inset = SmallBoxPad;
                b.Draw(Game1.mouseCursors,
                    new Rectangle(bounds.X + inset, bounds.Y + inset,
                                  bounds.Width - inset * 2, bounds.Height - inset * 2),
                    new Rectangle(236, 425, 9, 9),
                    Color.White * alpha);
            }
        }

        private static void DrawBadge(SpriteBatch b, string text, int x, int y, int width, int height, Color color)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                x, y, width, height,
                color * 0.3f, 4f, false);

            var textSize = Game1.smallFont.MeasureString(text);
            b.DrawString(Game1.smallFont, text,
                new Vector2(x + (width - textSize.X) / 2, y + (height - textSize.Y) / 2),
                color);
        }
    }
}
