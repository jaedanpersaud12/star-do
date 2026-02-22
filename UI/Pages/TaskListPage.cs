using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StarDo.Models;
using StarDo.Services;
using StarDo.UI.Components;

namespace StarDo.UI.Pages
{
    public class TaskListPage
    {
        private readonly TaskManager taskManager;
        private readonly PlannerMenu parentMenu;
        private Rectangle contentArea;
        private ScrollableListComponent scrollableList;
        private TextInputComponent quickAddInput;
        private ClickableComponent addButton;

        private readonly ClickableComponent[] filterButtons = new ClickableComponent[6];
        private readonly string[] filterLabels = { "All", "Farm", "Proc", "Social", "Goals", "Quests" };
        private TaskCategory? activeFilter;

        private List<PlannerTask> activeTasks = new();
        private List<PlannerTask> doneTodayTasks = new();
        private string hoveredTaskId;
        private string hoverTooltip;

        private const int FilterBarHeight = 44;
        private const int QuickAddHeight = 52;
        private const int SectionHeaderHeight = 36;

        public TaskListPage(TaskManager taskManager, Rectangle contentArea, PlannerMenu parentMenu)
        {
            this.taskManager = taskManager;
            this.parentMenu = parentMenu;
            this.contentArea = contentArea;

            int listTop = contentArea.Y + FilterBarHeight + QuickAddHeight + 8;
            int listHeight = contentArea.Height - FilterBarHeight - QuickAddHeight - 16;
            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, listTop, contentArea.Width, listHeight)
            );

            // Quick add input
            int inputY = contentArea.Y + FilterBarHeight + 4;
            int inputWidth = contentArea.Width - 120;
            this.quickAddInput = new TextInputComponent(
                contentArea.X + 8, inputY, inputWidth, 40, "Quick add task..."
            );

            // Add button
            this.addButton = new ClickableComponent(
                new Rectangle(contentArea.X + inputWidth + 20, inputY, 88, 40),
                "addBtn"
            )
            {
                myID = 8000,
                leftNeighborID = -7777
            };

            // Filter buttons
            int filterX = contentArea.X + 4;
            for (int i = 0; i < filterLabels.Length; i++)
            {
                int btnWidth = (int)Game1.smallFont.MeasureString(filterLabels[i]).X + 24;
                filterButtons[i] = new ClickableComponent(
                    new Rectangle(filterX, contentArea.Y + 4, btnWidth, 32),
                    i.ToString()
                )
                {
                    myID = 7000 + i
                };
                filterX += btnWidth + 4;
            }

            this.Refresh();
        }

        public void Refresh()
        {
            this.activeTasks = this.taskManager.GetActiveTasks(this.activeFilter);
            this.doneTodayTasks = this.taskManager.GetCompletedToday(Game1.Date.TotalDays);

            int totalHeight = this.activeTasks.Count * TaskRowComponent.RowHeight;
            if (this.doneTodayTasks.Count > 0)
                totalHeight += SectionHeaderHeight + this.doneTodayTasks.Count * TaskRowComponent.RowHeight;

            this.scrollableList.SetContentHeight(totalHeight);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            // Filter buttons
            for (int i = 0; i < this.filterButtons.Length; i++)
            {
                if (this.filterButtons[i].containsPoint(x, y))
                {
                    this.activeFilter = i == 0 ? null : (TaskCategory)(i - 1);
                    this.scrollableList.Reset();
                    this.Refresh();
                    Game1.playSound("shwip");
                    return;
                }
            }

            // Quick add input click
            if (this.quickAddInput.ContainsPoint(x, y))
            {
                this.quickAddInput.Select();
                return;
            }

            // Add button
            if (this.addButton.containsPoint(x, y))
            {
                this.DoQuickAdd();
                return;
            }

            // Deselect input if clicking elsewhere
            if (this.quickAddInput.IsSelected)
                this.quickAddInput.Deselect();

            // Scrollbar
            if (this.scrollableList.HandleLeftClick(x, y))
                return;

            // Task rows
            var listBounds = this.scrollableList.Bounds;
            if (!listBounds.Contains(x, y))
                return;

            int rowY = listBounds.Y - this.scrollableList.ScrollOffset;

            // Active tasks
            foreach (var task in this.activeTasks)
            {
                if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                {
                    var rowBounds = TaskRowComponent.GetRowBounds(listBounds.X, rowY, listBounds.Width - 32);
                    if (rowBounds.Contains(x, y))
                    {
                        var cbBounds = TaskRowComponent.GetCheckboxBounds(listBounds.X, rowY);
                        if (cbBounds.Contains(x, y))
                        {
                            this.taskManager.ToggleComplete(task.Id, Game1.Date.TotalDays);
                            Game1.playSound("coin");
                            this.Refresh();
                        }
                        else
                        {
                            this.parentMenu.OpenTaskDetail(task, false);
                        }
                        return;
                    }
                }
                rowY += TaskRowComponent.RowHeight;
            }

            // Done today section
            if (this.doneTodayTasks.Count > 0)
            {
                rowY += SectionHeaderHeight;
                foreach (var task in this.doneTodayTasks)
                {
                    if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                    {
                        var rowBounds = TaskRowComponent.GetRowBounds(listBounds.X, rowY, listBounds.Width - 32);
                        if (rowBounds.Contains(x, y))
                        {
                            var cbBounds = TaskRowComponent.GetCheckboxBounds(listBounds.X, rowY);
                            if (cbBounds.Contains(x, y))
                            {
                                this.taskManager.ToggleComplete(task.Id, Game1.Date.TotalDays);
                                Game1.playSound("coin");
                                this.Refresh();
                            }
                            else
                            {
                                this.parentMenu.OpenTaskDetail(task, false);
                            }
                            return;
                        }
                    }
                    rowY += TaskRowComponent.RowHeight;
                }
            }
        }

        public void LeftClickHeld(int x, int y)
        {
            this.scrollableList.HandleLeftClickHeld(x, y);
        }

        public void ReleaseLeftClick(int x, int y)
        {
            this.scrollableList.HandleLeftClickReleased();
        }

        public void ReceiveScrollWheel(int direction)
        {
            this.scrollableList.HandleScrollWheel(direction);
        }

        public void ReceiveKeyPress(Keys key)
        {
            if (this.quickAddInput.IsSelected)
            {
                if (key == Keys.Enter)
                    this.DoQuickAdd();
                else if (key == Keys.Escape)
                    this.quickAddInput.Deselect();
                return;
            }
        }

        public void PerformHoverAction(int x, int y)
        {
            this.hoveredTaskId = null;
            this.hoverTooltip = null;

            var listBounds = this.scrollableList.Bounds;
            if (!listBounds.Contains(x, y))
                return;

            int rowY = listBounds.Y - this.scrollableList.ScrollOffset;

            foreach (var task in this.activeTasks)
            {
                if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                {
                    var rowBounds = TaskRowComponent.GetRowBounds(listBounds.X, rowY, listBounds.Width - 32);
                    if (rowBounds.Contains(x, y))
                    {
                        this.hoveredTaskId = task.Id;
                        if (!string.IsNullOrEmpty(task.Notes))
                            this.hoverTooltip = task.Notes;
                        return;
                    }
                }
                rowY += TaskRowComponent.RowHeight;
            }
        }

        public void Draw(SpriteBatch b)
        {
            // Filter bar
            for (int i = 0; i < this.filterButtons.Length; i++)
            {
                bool isActive = (i == 0 && !this.activeFilter.HasValue) ||
                                (i > 0 && this.activeFilter.HasValue && (int)this.activeFilter.Value == i - 1);

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                    new Rectangle(384, 396, 15, 15),
                    this.filterButtons[i].bounds.X, this.filterButtons[i].bounds.Y,
                    this.filterButtons[i].bounds.Width, this.filterButtons[i].bounds.Height,
                    isActive ? Color.Gold : Color.White, 4f, false);

                var textSize = Game1.smallFont.MeasureString(this.filterLabels[i]);
                b.DrawString(Game1.smallFont, this.filterLabels[i],
                    new Vector2(
                        this.filterButtons[i].bounds.X + (this.filterButtons[i].bounds.Width - textSize.X) / 2,
                        this.filterButtons[i].bounds.Y + (this.filterButtons[i].bounds.Height - textSize.Y) / 2
                    ),
                    isActive ? Color.DarkGoldenrod : Game1.textColor);
            }

            // Quick add bar
            this.quickAddInput.Draw(b);

            // Add button
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                this.addButton.bounds.X, this.addButton.bounds.Y,
                this.addButton.bounds.Width, this.addButton.bounds.Height,
                Color.LightGreen, 4f, false);
            var addTextSize = Game1.smallFont.MeasureString("+ Add");
            b.DrawString(Game1.smallFont, "+ Add",
                new Vector2(
                    this.addButton.bounds.X + (this.addButton.bounds.Width - addTextSize.X) / 2,
                    this.addButton.bounds.Y + (this.addButton.bounds.Height - addTextSize.Y) / 2
                ),
                Game1.textColor);

            // Scrollable task list
            this.scrollableList.BeginScissorRect(b);

            var listBounds = this.scrollableList.Bounds;
            int rowY = listBounds.Y - this.scrollableList.ScrollOffset;

            // Active tasks
            if (this.activeTasks.Count == 0 && this.doneTodayTasks.Count == 0)
            {
                string emptyMsg = "No tasks yet! Add one above or press + Add.";
                var emptySize = Game1.smallFont.MeasureString(emptyMsg);
                b.DrawString(Game1.smallFont, emptyMsg,
                    new Vector2(listBounds.X + (listBounds.Width - emptySize.X) / 2, listBounds.Y + 40),
                    Color.Gray);
            }
            else
            {
                foreach (var task in this.activeTasks)
                {
                    if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                    {
                        bool isHovered = this.hoveredTaskId == task.Id;
                        TaskRowComponent.Draw(b, task, listBounds.X, rowY, listBounds.Width - 32, isHovered);
                    }
                    rowY += TaskRowComponent.RowHeight;
                }

                // Done Today section
                if (this.doneTodayTasks.Count > 0)
                {
                    if (rowY + SectionHeaderHeight > listBounds.Y && rowY < listBounds.Bottom)
                    {
                        b.DrawString(Game1.smallFont, $"-- Done Today ({this.doneTodayTasks.Count}) --",
                            new Vector2(listBounds.X + 8, rowY + 8), Color.Gray);
                    }
                    rowY += SectionHeaderHeight;

                    foreach (var task in this.doneTodayTasks)
                    {
                        if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                        {
                            bool isHovered = this.hoveredTaskId == task.Id;
                            TaskRowComponent.Draw(b, task, listBounds.X, rowY, listBounds.Width - 32, isHovered, dimmed: true);
                        }
                        rowY += TaskRowComponent.RowHeight;
                    }
                }
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);

            // Tooltip
            if (!string.IsNullOrEmpty(this.hoverTooltip))
            {
                IClickableMenu.drawHoverText(b, this.hoverTooltip, Game1.smallFont);
            }
        }

        public bool IsTextInputActive()
        {
            return this.quickAddInput?.IsSelected ?? false;
        }

        public void Cleanup()
        {
            this.quickAddInput?.Deselect();
        }

        private void DoQuickAdd()
        {
            string text = this.quickAddInput.Text?.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            var task = new PlannerTask
            {
                Title = text,
                CreatedDay = Game1.Date.TotalDays,
                Category = this.activeFilter ?? TaskCategory.Farm,
                Priority = TaskPriority.LongTerm
            };

            this.taskManager.AddTask(task);
            this.quickAddInput.Text = "";
            Game1.playSound("coin");
            this.Refresh();
        }
    }
}
