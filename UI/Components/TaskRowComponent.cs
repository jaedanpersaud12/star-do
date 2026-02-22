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
        public const int RowHeight = 72;
        private const int CheckboxSize = 36;
        private const int Padding = 12;

        private static readonly string[] CategoryLabels = { "Farm", "Proc", "Social", "Goals", "Quest" };
        private static readonly Color[] CategoryColors =
        {
            new Color(76, 153, 0),     // Farm - green
            new Color(178, 102, 0),    // Processing - orange
            new Color(204, 51, 153),   // Social - pink
            new Color(51, 102, 204),   // Goals - blue
            new Color(153, 102, 204),  // Quests - purple
        };

        private static readonly string[] PriorityLabels = { "Daily", "Weekly", "Long" };
        private static readonly Color[] PriorityColors =
        {
            new Color(204, 51, 51),    // Daily - red
            new Color(204, 153, 0),    // Weekly - yellow
            new Color(102, 102, 102),  // LongTerm - gray
        };

        public static Rectangle GetCheckboxBounds(int rowX, int rowY)
        {
            return new Rectangle(rowX + Padding, rowY + (RowHeight - CheckboxSize) / 2, CheckboxSize, CheckboxSize);
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

            // Category badge
            int catIdx = (int)task.Category;
            string catLabel = CategoryLabels[catIdx];
            Color catColor = CategoryColors[catIdx] * alpha;
            int badgeX = cbBounds.Right + Padding;
            int badgeY = y + 8;
            DrawBadge(b, catLabel, badgeX, badgeY, catColor);

            // Priority badge
            int priIdx = (int)task.Priority;
            string priLabel = PriorityLabels[priIdx];
            Color priColor = PriorityColors[priIdx] * alpha;
            var catTextSize = Game1.smallFont.MeasureString(catLabel);
            int priBadgeX = badgeX + (int)catTextSize.X + 28;
            DrawBadge(b, priLabel, priBadgeX, badgeY, priColor);

            // Recurring icon
            if (task.IsRecurring)
            {
                var priTextSize = Game1.smallFont.MeasureString(priLabel);
                int recurX = priBadgeX + (int)priTextSize.X + 28;
                b.DrawString(Game1.smallFont, "\u21BB", new Vector2(recurX, badgeY), Color.DarkCyan * alpha);
            }

            // Title
            string title = task.Title;
            int titleX = cbBounds.Right + Padding;
            int titleY = y + 36;
            Color titleColor = dimmed ? Color.Gray : Game1.textColor;

            if (dimmed)
            {
                // Strikethrough effect: draw a line through the text
                var titleSize = Game1.smallFont.MeasureString(title);
                b.DrawString(Game1.smallFont, title, new Vector2(titleX, titleY), titleColor * alpha);
                int lineY = titleY + (int)(titleSize.Y / 2);
                DrawHorizontalLine(b, titleX, lineY, (int)titleSize.X, Color.Gray * alpha);
            }
            else
            {
                Utility.drawTextWithShadow(b, title, Game1.smallFont,
                    new Vector2(titleX, titleY), titleColor, 1f, -1f, -1, -1, 1f, 3);
            }

            // Sub-task count indicator
            if (task.SubTasks.Count > 0)
            {
                int completedSubs = 0;
                foreach (var s in task.SubTasks)
                    if (s.IsCompleted) completedSubs++;

                string subText = $"{completedSubs}/{task.SubTasks.Count}";
                var subSize = Game1.smallFont.MeasureString(subText);
                b.DrawString(Game1.smallFont, subText,
                    new Vector2(x + width - subSize.X - 40, y + (RowHeight - subSize.Y) / 2),
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
                // Draw checkmark using the game's tick sprite
                b.Draw(Game1.mouseCursors,
                    new Rectangle(bounds.X + 4, bounds.Y + 4, bounds.Width - 8, bounds.Height - 8),
                    new Rectangle(236, 425, 9, 9),
                    Color.White * alpha);
            }
        }

        private static void DrawBadge(SpriteBatch b, string text, int x, int y, Color color)
        {
            var textSize = Game1.smallFont.MeasureString(text);
            int badgeWidth = (int)textSize.X + 16;
            int badgeHeight = (int)textSize.Y + 4;

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                x, y, badgeWidth, badgeHeight,
                color * 0.3f, 4f, false);

            b.DrawString(Game1.smallFont, text,
                new Vector2(x + 8, y + 2), color);
        }

        private static void DrawHorizontalLine(SpriteBatch b, int x, int y, int width, Color color)
        {
            b.Draw(Game1.staminaRect, new Rectangle(x, y, width, 2), color);
        }
    }
}
