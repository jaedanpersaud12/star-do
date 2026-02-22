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
        private Rectangle contentArea;
        private ScrollableListComponent scrollableList;

        private const int TemplateRowHeight = 120;

        public TemplatePage(TaskManager taskManager, Rectangle contentArea)
        {
            this.taskManager = taskManager;
            this.contentArea = contentArea;
            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, contentArea.Y + 8, contentArea.Width, contentArea.Height - 16)
            );
            this.Refresh();
        }

        public void Refresh()
        {
            int totalHeight = 44 + TemplateProvider.Templates.Count * (TemplateRowHeight + 8);
            this.scrollableList.SetContentHeight(totalHeight);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            if (this.scrollableList.HandleLeftClick(x, y))
                return;

            var bounds = this.scrollableList.Bounds;
            if (!bounds.Contains(x, y))
                return;

            int rowY = bounds.Y - this.scrollableList.ScrollOffset + 44;

            foreach (var template in TemplateProvider.Templates)
            {
                if (rowY + TemplateRowHeight > bounds.Y && rowY < bounds.Bottom)
                {
                    // Toggle button area (right side of the row)
                    int toggleX = bounds.X + bounds.Width - 120;
                    int toggleY = rowY + 8;
                    var toggleBounds = new Rectangle(toggleX, toggleY, 100, 36);

                    if (toggleBounds.Contains(x, y))
                    {
                        this.ToggleTemplate(template);
                        return;
                    }
                }
                rowY += TemplateRowHeight + 8;
            }
        }

        public void ReceiveScrollWheel(int direction)
        {
            this.scrollableList.HandleScrollWheel(direction);
        }

        public void Draw(SpriteBatch b)
        {
            this.scrollableList.BeginScissorRect(b);

            var bounds = this.scrollableList.Bounds;
            int y = bounds.Y - this.scrollableList.ScrollOffset;
            int contentWidth = bounds.Width - 32;

            b.DrawString(Game1.dialogueFont, "Task Templates", new Vector2(bounds.X + 8, y), Color.Gold);
            y += 44;

            foreach (var template in TemplateProvider.Templates)
            {
                if (y + TemplateRowHeight > bounds.Y && y < bounds.Bottom)
                {
                    bool isEnabled = this.taskManager.HasTasksForTemplate(template.Id);

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                        new Rectangle(384, 396, 15, 15),
                        bounds.X + 4, y, contentWidth, TemplateRowHeight,
                        isEnabled ? new Color(200, 255, 200) : Color.White,
                        4f, false);

                    // Template name
                    Utility.drawTextWithShadow(b, template.Name, Game1.smallFont,
                        new Vector2(bounds.X + 16, y + 8), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

                    // Description
                    b.DrawString(Game1.smallFont, template.Description,
                        new Vector2(bounds.X + 16, y + 32), Color.Gray);

                    // Sub-task preview
                    int subY = y + 56;
                    for (int i = 0; i < Math.Min(template.SubTasks.Count, 3); i++)
                    {
                        b.DrawString(Game1.smallFont, $"  - {template.SubTasks[i]}",
                            new Vector2(bounds.X + 24, subY), Color.DimGray);
                        subY += 20;
                    }
                    if (template.SubTasks.Count > 3)
                    {
                        b.DrawString(Game1.smallFont, $"  ... +{template.SubTasks.Count - 3} more",
                            new Vector2(bounds.X + 24, subY), Color.DimGray);
                    }

                    // Toggle button
                    int toggleX = bounds.X + contentWidth - 108;
                    int toggleY = y + 8;
                    Color btnColor = isEnabled ? Color.IndianRed : Color.LightGreen;
                    string btnText = isEnabled ? "Disable" : "Enable";

                    IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                        new Rectangle(384, 396, 15, 15),
                        toggleX, toggleY, 100, 36,
                        btnColor, 4f, false);

                    var textSize = Game1.smallFont.MeasureString(btnText);
                    b.DrawString(Game1.smallFont, btnText,
                        new Vector2(toggleX + (100 - textSize.X) / 2, toggleY + (36 - textSize.Y) / 2),
                        Game1.textColor);
                }
                y += TemplateRowHeight + 8;
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
