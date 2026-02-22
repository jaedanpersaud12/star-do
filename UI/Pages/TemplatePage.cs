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
        private readonly int btnH;

        public TemplatePage(TaskManager taskManager, Rectangle contentArea)
        {
            this.taskManager = taskManager;
            this.contentArea = contentArea;

            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.headerH = (int)Game1.dialogueFont.MeasureString("Tg").Y + 12;
            this.btnH = this.lineH + 24;

            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, contentArea.Y, contentArea.Width, contentArea.Height)
            );
            this.Refresh();
        }

        private int GetTemplateRowHeight(TemplateDefinition template)
        {
            int previewLines = Math.Min(template.SubTasks.Count, 3);
            if (template.SubTasks.Count > 3) previewLines++;
            return this.lineH * 2 + 28 + previewLines * (this.lineH + 2) + 16;
        }

        public void Refresh()
        {
            int totalH = this.headerH + 8;
            foreach (var t in TemplateProvider.Templates)
                totalH += this.GetTemplateRowHeight(t) + 8;
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

            foreach (var template in TemplateProvider.Templates)
            {
                int rowH = this.GetTemplateRowHeight(template);
                if (rowY + rowH > bounds.Y && rowY < bounds.Bottom)
                {
                    // Toggle button
                    string btnText = this.taskManager.HasTasksForTemplate(template.Id) ? "Disable" : "Enable";
                    int btnW = (int)Game1.smallFont.MeasureString(btnText).X + 40;
                    int toggleX = bounds.X + bounds.Width - btnW - 52;
                    int toggleY = rowY + 20;

                    if (new Rectangle(toggleX, toggleY, btnW, this.btnH).Contains(x, y))
                    {
                        this.ToggleTemplate(template);
                        return;
                    }
                }
                rowY += rowH + 8;
            }
        }

        public void ReceiveScrollWheel(int direction) => this.scrollableList.HandleScrollWheel(direction);

        public void Draw(SpriteBatch b)
        {
            this.scrollableList.BeginScissorRect(b);

            var bounds = this.scrollableList.Bounds;
            int y = bounds.Y - this.scrollableList.ScrollOffset;
            int cw = bounds.Width - 40;

            Utility.drawTextWithShadow(b, "Task Templates", Game1.dialogueFont,
                new Vector2(bounds.X + 8, y), Color.Gold, 1f, -1f, -1, -1, 1f, 3);
            y += this.headerH + 8;

            foreach (var template in TemplateProvider.Templates)
            {
                int rowH = this.GetTemplateRowHeight(template);
                if (y + rowH > bounds.Y && y < bounds.Bottom)
                {
                    bool isEnabled = this.taskManager.HasTasksForTemplate(template.Id);

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                        new Rectangle(384, 396, 15, 15),
                        bounds.X + 4, y, cw, rowH,
                        isEnabled ? new Color(210, 255, 210) : Color.White,
                        4f, false);

                    // Name
                    Utility.drawTextWithShadow(b, template.Name, Game1.smallFont,
                        new Vector2(bounds.X + 24, y + 20), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

                    // Description
                    b.DrawString(Game1.smallFont, template.Description,
                        new Vector2(bounds.X + 24, y + 20 + this.lineH + 4), Color.Gray);

                    // Sub-task preview
                    int subY = y + 20 + this.lineH * 2 + 8;
                    int maxPreview = Math.Min(template.SubTasks.Count, 3);
                    for (int i = 0; i < maxPreview; i++)
                    {
                        b.DrawString(Game1.smallFont, $"  - {template.SubTasks[i]}",
                            new Vector2(bounds.X + 32, subY), Color.DimGray);
                        subY += this.lineH + 2;
                    }
                    if (template.SubTasks.Count > 3)
                    {
                        b.DrawString(Game1.smallFont, $"  ... +{template.SubTasks.Count - 3} more",
                            new Vector2(bounds.X + 32, subY), Color.DimGray);
                    }

                    // Toggle button
                    string btnText = isEnabled ? "Disable" : "Enable";
                    int btnW = (int)Game1.smallFont.MeasureString(btnText).X + 40;
                    int toggleX = bounds.X + cw - btnW - 12;
                    int toggleY = y + 20;
                    Color btnColor = isEnabled ? Color.IndianRed : Color.LightGreen;

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                        new Rectangle(384, 396, 15, 15),
                        toggleX, toggleY, btnW, this.btnH,
                        btnColor, 4f, false);

                    var ts = Game1.smallFont.MeasureString(btnText);
                    Utility.drawTextWithShadow(b, btnText, Game1.smallFont,
                        new Vector2(toggleX + (btnW - ts.X) / 2, toggleY + (this.btnH - ts.Y) / 2),
                        Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
                }
                y += rowH + 8;
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);
        }

        private void ToggleTemplate(TemplateDefinition template)
        {
            if (this.taskManager.HasTasksForTemplate(template.Id))
            {
                this.taskManager.RemoveTasksByTemplate(template.Id);
                this.taskManager.Data.EnabledTemplateIds.Remove(template.Id);
                Game1.playSound("trashcan");
            }
            else
            {
                var task = TemplateProvider.CreateTaskFromTemplate(template, Game1.Date.TotalDays);
                this.taskManager.AddTask(task);
                if (!this.taskManager.Data.EnabledTemplateIds.Contains(template.Id))
                    this.taskManager.Data.EnabledTemplateIds.Add(template.Id);
                Game1.playSound("coin");
            }
            this.taskManager.Save();
        }
    }
}
