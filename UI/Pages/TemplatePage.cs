using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StarDo.Models;
using StarDo.Services;
using StarDo.UI.Components;

namespace StarDo.UI.Pages
{
    public class TemplatePage
    {
        private readonly TaskManager taskManager;
        private readonly Rectangle contentArea;
        private ScrollableListComponent scrollableList;

        private readonly int lineH;
        private readonly int headerH;
        private readonly int groupHeaderH;
        private readonly int btnH;
        private readonly int seasonBadgeH;
        private readonly int seasonBadgeW;
        private readonly int groupGap;
        private readonly int rowPad;

        private static readonly Dictionary<string, Color> SeasonColors = new()
        {
            { "spring", new Color(60, 180, 75) },
            { "summer", new Color(220, 180, 30) },
            { "fall", new Color(210, 120, 40) },
            { "winter", new Color(80, 140, 210) }
        };

        private static readonly Dictionary<string, string> SeasonLabels = new()
        {
            { "spring", "Spr" },
            { "summer", "Sum" },
            { "fall", "Fall" },
            { "winter", "Win" }
        };

        public TemplatePage(TaskManager taskManager, Rectangle contentArea)
        {
            this.taskManager = taskManager;
            this.contentArea = contentArea;

            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.headerH = (int)Game1.dialogueFont.MeasureString("Tg").Y + 12;
            this.groupHeaderH = this.lineH + 16;
            this.btnH = this.lineH + 48;
            this.seasonBadgeH = this.lineH - 2;
            this.seasonBadgeW = (int)Game1.smallFont.MeasureString("Sum").X + 16;
            this.groupGap = 20;
            this.rowPad = 20;

            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, contentArea.Y, contentArea.Width, contentArea.Height)
            );
            this.Refresh();
        }

        private int GetTemplateRowHeight(TemplateDefinition template)
        {
            // Name line + description line + season badges + subtask preview + padding
            int h = this.rowPad; // top pad
            h += this.lineH; // name
            h += 4 + this.lineH; // description
            if (template.Seasons != null && template.Seasons.Length > 0)
                h += 6 + this.seasonBadgeH; // season badges row
            int previewLines = Math.Min(template.SubTasks.Count, 4);
            if (template.SubTasks.Count > 4) previewLines++; // "+N more" line
            h += 8 + previewLines * (this.lineH + 2);
            h += this.rowPad; // bottom pad
            return h;
        }

        public void Refresh()
        {
            var groups = TemplateProvider.GetGrouped();
            int totalH = this.headerH + 8;
            foreach (var (group, items) in groups)
            {
                totalH += this.groupHeaderH + 8;
                foreach (var t in items)
                    totalH += this.GetTemplateRowHeight(t) + 8;
                totalH += this.groupGap;
            }
            this.scrollableList.SetContentHeight(totalH);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            if (this.scrollableList.HandleLeftClick(x, y))
                return;

            var bounds = this.scrollableList.Bounds;
            if (!bounds.Contains(x, y))
                return;

            int rowY = bounds.Y - this.scrollableList.ScrollOffset + this.headerH + 8;
            int cw = bounds.Width - 40;
            var groups = TemplateProvider.GetGrouped();

            foreach (var (group, items) in groups)
            {
                rowY += this.groupHeaderH + 8;

                foreach (var template in items)
                {
                    int rowH = this.GetTemplateRowHeight(template);
                    if (rowY + rowH > bounds.Y && rowY < bounds.Bottom)
                    {
                        string btnText = this.taskManager.HasTasksForTemplate(template.Id) ? "Disable" : "Enable";
                        int btnW = (int)Game1.smallFont.MeasureString(btnText).X + 56;
                        int toggleX = bounds.X + cw - btnW - 12;
                        int toggleY = rowY + this.rowPad;

                        if (new Rectangle(toggleX, toggleY, btnW, this.btnH).Contains(x, y))
                        {
                            this.ToggleTemplate(template);
                            return;
                        }
                    }
                    rowY += rowH + 8;
                }
                rowY += this.groupGap;
            }
        }

        public void ReceiveScrollWheel(int direction) => this.scrollableList.HandleScrollWheel(direction);

        public void Draw(SpriteBatch b)
        {
            this.scrollableList.BeginScissorRect(b);

            var bounds = this.scrollableList.Bounds;
            int y = bounds.Y - this.scrollableList.ScrollOffset;
            int cw = bounds.Width - 40;
            string currentSeason = Game1.currentSeason ?? "spring";

            // Page title
            Utility.drawTextWithShadow(b, "Task Templates", Game1.dialogueFont,
                new Vector2(bounds.X + 8, y), Color.Gold, 1f, -1f, -1, -1, 1f, 3);
            y += this.headerH + 8;

            var groups = TemplateProvider.GetGrouped();

            foreach (var (group, items) in groups)
            {
                // Group header
                if (y + this.groupHeaderH > bounds.Y && y < bounds.Bottom)
                {
                    // Divider line
                    b.Draw(Game1.staminaRect,
                        new Rectangle(bounds.X + 8, y + 4, cw - 8, 2),
                        Color.Gold * 0.4f);

                    Utility.drawTextWithShadow(b, group, Game1.smallFont,
                        new Vector2(bounds.X + 12, y + 8), Color.Gold * 0.9f, 1f, -1f, -1, -1, 1f, 3);
                }
                y += this.groupHeaderH + 8;

                foreach (var template in items)
                {
                    int rowH = this.GetTemplateRowHeight(template);
                    if (y + rowH > bounds.Y && y < bounds.Bottom)
                        this.DrawTemplateRow(b, template, bounds.X + 4, y, cw, rowH, currentSeason);
                    y += rowH + 8;
                }
                y += this.groupGap;
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);
        }

        private void DrawTemplateRow(SpriteBatch b, TemplateDefinition template, int x, int y, int w, int h, string currentSeason)
        {
            bool isEnabled = this.taskManager.HasTasksForTemplate(template.Id);
            bool isCurrentSeason = template.IsRelevantForSeason(currentSeason);

            // Background card
            Color bgColor = isEnabled
                ? new Color(200, 245, 200)
                : (isCurrentSeason ? new Color(255, 255, 240) : new Color(230, 230, 230));

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                x, y, w, h,
                bgColor, 4f, false);

            // Current-season left accent bar
            if (isCurrentSeason && SeasonColors.TryGetValue(currentSeason, out var accentColor))
            {
                b.Draw(Game1.staminaRect,
                    new Rectangle(x + 20, y + 20, 4, h - 40),
                    accentColor * 0.8f);
            }

            int textX = x + 32;
            int contentY = y + this.rowPad;

            // Toggle button (top right)
            string btnText = isEnabled ? "Disable" : "Enable";
            int btnW = (int)Game1.smallFont.MeasureString(btnText).X + 56;
            int toggleX = x + w - btnW - 12;
            Color btnColor = isEnabled ? Color.IndianRed : Color.LightGreen;

            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                toggleX, contentY, btnW, this.btnH,
                btnColor, 4f, false);

            var btnTs = Game1.smallFont.MeasureString(btnText);
            Utility.drawTextWithShadow(b, btnText, Game1.smallFont,
                new Vector2(toggleX + (btnW - btnTs.X) / 2, contentY + (this.btnH - btnTs.Y) / 2),
                Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

            // Name
            Utility.drawTextWithShadow(b, template.Name, Game1.smallFont,
                new Vector2(textX, contentY), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            contentY += this.lineH + 4;

            // Description
            Color descColor = isCurrentSeason ? Color.DimGray : Color.Gray;
            b.DrawString(Game1.smallFont, template.Description,
                new Vector2(textX, contentY), descColor);
            contentY += this.lineH;

            // Season badges
            if (template.Seasons != null && template.Seasons.Length > 0)
            {
                contentY += 6;
                int badgeX = textX;
                foreach (var season in template.Seasons)
                {
                    if (!SeasonColors.TryGetValue(season, out var sColor)) continue;
                    if (!SeasonLabels.TryGetValue(season, out var sLabel)) continue;

                    bool isCurrent = season.Equals(currentSeason, StringComparison.OrdinalIgnoreCase);
                    float alpha = isCurrent ? 1f : 0.5f;

                    // Badge background
                    b.Draw(Game1.staminaRect,
                        new Rectangle(badgeX, contentY, this.seasonBadgeW, this.seasonBadgeH),
                        sColor * (alpha * 0.35f));

                    // Badge border (highlight if current)
                    if (isCurrent)
                    {
                        // Top
                        b.Draw(Game1.staminaRect, new Rectangle(badgeX, contentY, this.seasonBadgeW, 2), sColor * 0.8f);
                        // Bottom
                        b.Draw(Game1.staminaRect, new Rectangle(badgeX, contentY + this.seasonBadgeH - 2, this.seasonBadgeW, 2), sColor * 0.8f);
                        // Left
                        b.Draw(Game1.staminaRect, new Rectangle(badgeX, contentY, 2, this.seasonBadgeH), sColor * 0.8f);
                        // Right
                        b.Draw(Game1.staminaRect, new Rectangle(badgeX + this.seasonBadgeW - 2, contentY, 2, this.seasonBadgeH), sColor * 0.8f);
                    }

                    // Badge text
                    var labelSize = Game1.smallFont.MeasureString(sLabel);
                    b.DrawString(Game1.smallFont, sLabel,
                        new Vector2(badgeX + (this.seasonBadgeW - labelSize.X) / 2, contentY + (this.seasonBadgeH - labelSize.Y) / 2),
                        sColor * alpha);

                    badgeX += this.seasonBadgeW + 6;
                }
                contentY += this.seasonBadgeH;
            }

            // Sub-task preview
            contentY += 8;
            int maxPreview = Math.Min(template.SubTasks.Count, 4);
            string subtaskHeader = $"{template.SubTasks.Count} sub-tasks:";
            b.DrawString(Game1.smallFont, subtaskHeader,
                new Vector2(textX, contentY), Color.Gray * 0.8f);
            contentY += this.lineH + 2;

            for (int i = 0; i < maxPreview; i++)
            {
                string bullet = isEnabled ? "  [_]  " : "  -  ";
                b.DrawString(Game1.smallFont, $"{bullet}{template.SubTasks[i]}",
                    new Vector2(textX, contentY), Color.DimGray);
                contentY += this.lineH + 2;
            }
            if (template.SubTasks.Count > 4)
            {
                b.DrawString(Game1.smallFont, $"     ... +{template.SubTasks.Count - 4} more",
                    new Vector2(textX, contentY), Color.Gray);
            }
        }

        private void ToggleTemplate(TemplateDefinition template)
        {
            if (this.taskManager.HasTasksForTemplate(template.Id))
            {
                this.taskManager.RemoveTasksByTemplate(template.Id);
                Game1.playSound("trashcan");
            }
            else
            {
                var task = TemplateProvider.CreateTaskFromTemplate(template, Game1.Date.TotalDays);
                this.taskManager.AddTask(task);
                Game1.playSound("coin");
            }
            this.taskManager.Save();
        }
    }
}
