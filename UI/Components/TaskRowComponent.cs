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

        private const int BoxPad = 20;
        private const int SmallBoxPad = 8;

        public static int RowHeight => LineH * 3 + 12;
        private static int CheckboxSize => LineH + SmallBoxPad * 2;
        private const int Padding = 12;
        private const int StripeWidth = 6;

        private static readonly string[] CategoryLabels = { "Farm", "Proc", "Social", "Goals", "Quest" };
        private static readonly Color[] CategoryColors =
        {
            new Color(76, 153, 0),    // Farm - green
            new Color(178, 102, 0),   // Processing - amber
            new Color(204, 51, 153),  // Social - pink
            new Color(51, 102, 204),  // Goals - blue
            new Color(153, 102, 204), // Quests - purple
        };

        private static readonly string[] PriorityLabels = { "Daily", "Weekly", "Monthly" };
        private static readonly Color[] PriorityColors =
        {
            new Color(204, 51, 51),   // Daily - red
            new Color(204, 153, 0),   // Weekly - gold
            new Color(102, 102, 102), // Long-term - gray
        };

        // Progress bar colors
        private static readonly Color ProgressBg = new Color(40, 40, 40) * 0.2f;
        private static readonly Color ProgressFillIncomplete = new Color(100, 180, 60);
        private static readonly Color ProgressFillComplete = new Color(60, 200, 60);

        public static Rectangle GetCheckboxBounds(int rowX, int rowY)
        {
            int size = CheckboxSize;
            return new Rectangle(
                rowX + BoxPad + StripeWidth + 8,
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

            // Row background
            Color bgColor = isHovered
                ? new Color(255, 248, 220) * alpha
                : Color.White * alpha;

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                x, y, width, RowHeight,
                bgColor, 4f, false);

            // Category color stripe (left edge, inside the card border)
            int catIdx = (int)task.Category;
            Color stripeColor = CategoryColors[catIdx] * alpha;
            b.Draw(Game1.staminaRect,
                new Rectangle(x + BoxPad, y + BoxPad, StripeWidth, RowHeight - BoxPad * 2),
                stripeColor * 0.85f);

            // Checkbox
            var cbBounds = GetCheckboxBounds(x, y);
            DrawCheckbox(b, cbBounds, task.IsCompleted, alpha);

            int contentX = cbBounds.Right + Padding;
            int contentRight = x + width - BoxPad - 8;
            int contentWidth = contentRight - contentX;

            // ── Top row: badges ──
            int badgeY = y + BoxPad + 2;
            int badgeH = LineH;
            int badgeX = contentX;

            // Category badge
            string catLabel = CategoryLabels[catIdx];
            Color catColor = CategoryColors[catIdx];
            int catBadgeW = (int)Game1.smallFont.MeasureString(catLabel).X + 16;
            DrawBadge(b, catLabel, badgeX, badgeY, catBadgeW, badgeH, catColor, alpha);
            badgeX += catBadgeW + 6;

            // Priority badge
            int priIdx = (int)task.Priority;
            string priLabel = PriorityLabels[priIdx];
            Color priColor = PriorityColors[priIdx];
            int priBadgeW = (int)Game1.smallFont.MeasureString(priLabel).X + 16;
            DrawBadge(b, priLabel, badgeX, badgeY, priBadgeW, badgeH, priColor, alpha);
            badgeX += priBadgeW + 6;

            // Recurring badge
            if (task.IsRecurring)
            {
                int recurBadgeW = (int)Game1.smallFont.MeasureString("Recur").X + 16;
                DrawBadge(b, "Recur", badgeX, badgeY, recurBadgeW, badgeH, new Color(0, 140, 140), alpha);
                badgeX += recurBadgeW + 6;
            }

            // Template indicator
            if (!string.IsNullOrEmpty(task.TemplateId))
            {
                int tmplW = (int)Game1.smallFont.MeasureString("T").X + 12;
                DrawBadge(b, "T", badgeX, badgeY, tmplW, badgeH, new Color(120, 100, 160), alpha);
                badgeX += tmplW + 6;
            }

            // Completion count badge
            if (task.CompletionCount > 0)
            {
                string countLabel = $"\u00d7{task.CompletionCount}";
                int countW = (int)Game1.smallFont.MeasureString(countLabel).X + 16;
                DrawBadge(b, countLabel, badgeX, badgeY, countW, badgeH, new Color(60, 160, 60), alpha);
            }

            // ── Bottom row: title + progress ──
            int titleY = badgeY + badgeH + 6;
            Color titleColor = dimmed ? Color.Gray : Game1.textColor;

            // Draw title
            string displayTitle = task.Title;
            // Truncate if too long for available space
            int maxTitleW = contentWidth;
            if (task.SubTasks.Count > 0)
                maxTitleW -= 160; // reserve space for progress bar

            var titleSize = Game1.smallFont.MeasureString(displayTitle);
            if (titleSize.X > maxTitleW && maxTitleW > 50)
            {
                while (displayTitle.Length > 3 && Game1.smallFont.MeasureString(displayTitle + "...").X > maxTitleW)
                    displayTitle = displayTitle[..^1];
                displayTitle += "...";
                titleSize = Game1.smallFont.MeasureString(displayTitle);
            }

            if (dimmed)
            {
                b.DrawString(Game1.smallFont, displayTitle, new Vector2(contentX, titleY), titleColor * alpha);
                int strikeY = titleY + (int)(titleSize.Y / 2);
                b.Draw(Game1.staminaRect,
                    new Rectangle(contentX, strikeY, (int)titleSize.X, 2),
                    Color.Gray * alpha);
            }
            else
            {
                Utility.drawTextWithShadow(b, displayTitle, Game1.smallFont,
                    new Vector2(contentX, titleY), titleColor, 1f, -1f, -1, -1, 1f, 3);
            }

            // ── Sub-task progress bar (right-aligned on title row) ──
            if (task.SubTasks.Count > 0)
            {
                int completedSubs = 0;
                foreach (var s in task.SubTasks)
                    if (s.IsCompleted) completedSubs++;

                float progress = (float)completedSubs / task.SubTasks.Count;
                bool allDone = completedSubs == task.SubTasks.Count;

                // Progress text
                string progText = $"{completedSubs}/{task.SubTasks.Count}";
                var progTextSize = Game1.smallFont.MeasureString(progText);
                int progTextX = contentRight - (int)progTextSize.X;
                Color progTextColor = allDone ? ProgressFillComplete : Color.Gray;
                b.DrawString(Game1.smallFont, progText,
                    new Vector2(progTextX, titleY), progTextColor * alpha);

                // Progress bar — only draw if there's enough room
                int barW = 80;
                int barH = 8;
                int barX = progTextX - barW - 8;

                if (barX > contentX + 20)
                {
                    int barY = titleY + (LineH - barH) / 2;

                    // Background track
                    b.Draw(Game1.staminaRect,
                        new Rectangle(barX, barY, barW, barH),
                        ProgressBg);

                    // Fill
                    Color fillColor = allDone ? ProgressFillComplete : ProgressFillIncomplete;
                    int fillW = (int)(barW * progress);
                    if (fillW > 0)
                    {
                        b.Draw(Game1.staminaRect,
                            new Rectangle(barX, barY, fillW, barH),
                            fillColor * (alpha * 0.8f));
                    }

                    // Border around track
                    b.Draw(Game1.staminaRect, new Rectangle(barX, barY, barW, 1), Color.Black * (alpha * 0.15f));
                    b.Draw(Game1.staminaRect, new Rectangle(barX, barY + barH - 1, barW, 1), Color.Black * (alpha * 0.15f));
                    b.Draw(Game1.staminaRect, new Rectangle(barX, barY, 1, barH), Color.Black * (alpha * 0.15f));
                    b.Draw(Game1.staminaRect, new Rectangle(barX + barW - 1, barY, 1, barH), Color.Black * (alpha * 0.15f));
                }
            }
        }

        private static void DrawCheckbox(SpriteBatch b, Rectangle bounds, bool isChecked, float alpha)
        {
            Color boxColor = isChecked ? new Color(200, 240, 200) : Color.White;
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(403, 383, 6, 6),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                boxColor * alpha, 4f, false);

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

        private static void DrawBadge(SpriteBatch b, string text, int x, int y, int width, int height, Color color, float alpha)
        {
            // Badge background - subtle fill
            b.Draw(Game1.staminaRect,
                new Rectangle(x, y, width, height),
                color * (alpha * 0.15f));

            // Badge border
            b.Draw(Game1.staminaRect, new Rectangle(x, y, width, 1), color * (alpha * 0.3f));
            b.Draw(Game1.staminaRect, new Rectangle(x, y + height - 1, width, 1), color * (alpha * 0.3f));
            b.Draw(Game1.staminaRect, new Rectangle(x, y, 1, height), color * (alpha * 0.3f));
            b.Draw(Game1.staminaRect, new Rectangle(x + width - 1, y, 1, height), color * (alpha * 0.3f));

            // Badge text
            var textSize = Game1.smallFont.MeasureString(text);
            b.DrawString(Game1.smallFont, text,
                new Vector2(x + (width - textSize.X) / 2, y + (height - textSize.Y) / 2),
                color * alpha);
        }
    }
}
