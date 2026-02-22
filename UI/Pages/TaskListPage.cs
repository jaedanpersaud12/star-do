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
        private readonly Rectangle contentArea;
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

        // Computed layout values
        private readonly int lineH;
        private readonly int filterBarHeight;
        private readonly int quickAddHeight;
        private readonly int sectionHeaderHeight;

        public TaskListPage(TaskManager taskManager, Rectangle contentArea, PlannerMenu parentMenu)
        {
            this.taskManager = taskManager;
            this.parentMenu = parentMenu;
            this.contentArea = contentArea;

            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.filterBarHeight = this.lineH + 24;
            this.quickAddHeight = this.lineH + 28;
            this.sectionHeaderHeight = this.lineH + 12;

            // Filter buttons - auto-sized to text
            int filterX = contentArea.X;
            int filterBtnH = this.lineH + 16;
            for (int i = 0; i < filterLabels.Length; i++)
            {
                int textW = (int)Game1.smallFont.MeasureString(filterLabels[i]).X;
                int btnWidth = textW + 40;
                filterButtons[i] = new ClickableComponent(
                    new Rectangle(filterX, contentArea.Y + 4, btnWidth, filterBtnH),
                    i.ToString()
                ) { myID = 7000 + i };
                filterX += btnWidth + 6;
            }

            // Quick add input
            int inputY = contentArea.Y + this.filterBarHeight;
            int addBtnTextW = (int)Game1.smallFont.MeasureString("+ Add").X;
            int addBtnW = addBtnTextW + 40;
            int inputWidth = contentArea.Width - addBtnW - 20;
            this.quickAddInput = new TextInputComponent(
                contentArea.X, inputY, inputWidth, this.lineH + 8, "Quick add task..."
            );

            this.addButton = new ClickableComponent(
                new Rectangle(contentArea.X + inputWidth + 12, inputY - 4, addBtnW, this.lineH + 24),
                "addBtn"
            ) { myID = 8000 };

            // Scrollable list area
            int listTop = contentArea.Y + this.filterBarHeight + this.quickAddHeight + 8;
            int listHeight = contentArea.Height - this.filterBarHeight - this.quickAddHeight - 16;
            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, listTop, contentArea.Width, Math.Max(listHeight, 100))
            );

            this.Refresh();
        }

        public void Refresh()
        {
            this.activeTasks = this.taskManager.GetActiveTasks(this.activeFilter);
            this.doneTodayTasks = this.taskManager.GetCompletedToday(Game1.Date.TotalDays);

            int totalHeight = this.activeTasks.Count * TaskRowComponent.RowHeight;
            if (this.doneTodayTasks.Count > 0)
                totalHeight += this.sectionHeaderHeight + this.doneTodayTasks.Count * TaskRowComponent.RowHeight;

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

            // Quick add input
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
            int rowWidth = listBounds.Width - 40;

            foreach (var task in this.activeTasks)
            {
                if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                {
                    var rowBounds = TaskRowComponent.GetRowBounds(listBounds.X, rowY, rowWidth);
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

            // Done today
            if (this.doneTodayTasks.Count > 0)
            {
                rowY += this.sectionHeaderHeight;
                foreach (var task in this.doneTodayTasks)
                {
                    if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                    {
                        var rowBounds = TaskRowComponent.GetRowBounds(listBounds.X, rowY, rowWidth);
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

        public void LeftClickHeld(int x, int y) => this.scrollableList.HandleLeftClickHeld(x, y);
        public void ReleaseLeftClick(int x, int y) => this.scrollableList.HandleLeftClickReleased();
        public void ReceiveScrollWheel(int direction) => this.scrollableList.HandleScrollWheel(direction);

        public void ReceiveKeyPress(Keys key)
        {
            if (this.quickAddInput.IsSelected)
            {
                if (key == Keys.Enter) this.DoQuickAdd();
                else if (key == Keys.Escape) this.quickAddInput.Deselect();
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
            int rowWidth = listBounds.Width - 40;

            foreach (var task in this.activeTasks)
            {
                if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                {
                    if (TaskRowComponent.GetRowBounds(listBounds.X, rowY, rowWidth).Contains(x, y))
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

                var fb = this.filterButtons[i].bounds;
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                    new Rectangle(384, 396, 15, 15),
                    fb.X, fb.Y, fb.Width, fb.Height,
                    isActive ? Color.Gold : Color.White, 4f, false);

                var ts = Game1.smallFont.MeasureString(this.filterLabels[i]);
                Utility.drawTextWithShadow(b, this.filterLabels[i], Game1.smallFont,
                    new Vector2(fb.X + (fb.Width - ts.X) / 2, fb.Y + (fb.Height - ts.Y) / 2),
                    isActive ? Color.DarkGoldenrod : Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            }

            // Quick add
            this.quickAddInput.Draw(b);

            // Add button
            var ab = this.addButton.bounds;
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                ab.X, ab.Y, ab.Width, ab.Height,
                Color.LightGreen, 4f, false);
            var addTs = Game1.smallFont.MeasureString("+ Add");
            Utility.drawTextWithShadow(b, "+ Add", Game1.smallFont,
                new Vector2(ab.X + (ab.Width - addTs.X) / 2, ab.Y + (ab.Height - addTs.Y) / 2),
                Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

            // Scrollable list
            this.scrollableList.BeginScissorRect(b);

            var listBounds = this.scrollableList.Bounds;
            int rowY = listBounds.Y - this.scrollableList.ScrollOffset;
            int rowWidth = listBounds.Width - 40;

            if (this.activeTasks.Count == 0 && this.doneTodayTasks.Count == 0)
            {
                string msg = "No tasks yet! Add one above.";
                var msgSize = Game1.smallFont.MeasureString(msg);
                b.DrawString(Game1.smallFont, msg,
                    new Vector2(listBounds.X + (listBounds.Width - msgSize.X) / 2, listBounds.Y + 40),
                    Color.Gray);
            }
            else
            {
                foreach (var task in this.activeTasks)
                {
                    if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                        TaskRowComponent.Draw(b, task, listBounds.X, rowY, rowWidth, this.hoveredTaskId == task.Id);
                    rowY += TaskRowComponent.RowHeight;
                }

                if (this.doneTodayTasks.Count > 0)
                {
                    if (rowY + this.sectionHeaderHeight > listBounds.Y && rowY < listBounds.Bottom)
                    {
                        string hdr = $"-- Done Today ({this.doneTodayTasks.Count}) --";
                        b.DrawString(Game1.smallFont, hdr, new Vector2(listBounds.X + 8, rowY + 4), Color.Gray);
                    }
                    rowY += this.sectionHeaderHeight;

                    foreach (var task in this.doneTodayTasks)
                    {
                        if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                            TaskRowComponent.Draw(b, task, listBounds.X, rowY, rowWidth, this.hoveredTaskId == task.Id, dimmed: true);
                        rowY += TaskRowComponent.RowHeight;
                    }
                }
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);

            if (!string.IsNullOrEmpty(this.hoverTooltip))
                IClickableMenu.drawHoverText(b, this.hoverTooltip, Game1.smallFont);
        }

        public bool IsTextInputActive() => this.quickAddInput?.IsSelected ?? false;

        public void Cleanup() => this.quickAddInput?.Deselect();

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
