using System;
using System.Collections.Generic;
using System.Linq;
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
        private ClickableComponent newTaskButton;

        private readonly ClickableComponent[] seasonButtons = new ClickableComponent[4];
        private Season activeSeason;

        private readonly ClickableComponent[] filterButtons = new ClickableComponent[6];
        private readonly string[] filterLabels = { "All", "Farm", "Proc", "Social", "Goals", "Quests" };
        private TaskCategory? activeFilter;

        // Priority-grouped task lists
        private List<PlannerTask> dailyTasks = new();
        private List<PlannerTask> weeklyTasks = new();
        private List<PlannerTask> monthlyTasks = new();
        private List<PlannerTask> completedTasks = new();
        private int totalActiveCount;

        private string hoveredTaskId;
        private string hoverTooltip;

        // Layout constants computed from font
        private readonly int lineH;
        private readonly int seasonBarHeight;
        private readonly int filterBarHeight;
        private readonly int toolbarHeight;
        private readonly int summaryHeight;
        private readonly int sectionHeaderHeight;
        private readonly int sectionGap;
        private readonly int btnH;

        // Category colors for filter buttons
        private static readonly Color[] CategoryFilterColors =
        {
            Color.Gold,               // All
            new Color(76, 153, 0),    // Farm
            new Color(178, 102, 0),   // Processing
            new Color(204, 51, 153),  // Social
            new Color(51, 102, 204),  // Goals
            new Color(153, 102, 204), // Quests
        };

        // Section header colors
        private static readonly Color DailyColor = new Color(204, 51, 51);
        private static readonly Color WeeklyColor = new Color(204, 153, 0);
        private static readonly Color MonthlyColor = new Color(102, 140, 180);
        private static readonly Color CompletedColor = new Color(60, 160, 60);

        public TaskListPage(TaskManager taskManager, Rectangle contentArea, PlannerMenu parentMenu)
        {
            this.taskManager = taskManager;
            this.parentMenu = parentMenu;
            this.contentArea = contentArea;

            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.btnH = this.lineH + 48;
            this.seasonBarHeight = this.btnH + 8;
            this.filterBarHeight = this.btnH + 8;
            this.toolbarHeight = this.btnH + 8;
            this.summaryHeight = this.lineH + 8;
            this.sectionHeaderHeight = this.lineH + 16;
            this.sectionGap = 8;

            this.activeSeason = SeasonHelper.GetCurrentSeason();

            // Season tabs
            int seasonX = contentArea.X;
            for (int i = 0; i < SeasonHelper.OrderedSeasons.Length; i++)
            {
                string seasonText = SeasonHelper.GetDisplayName(SeasonHelper.OrderedSeasons[i]);
                int textW = (int)Game1.smallFont.MeasureString(seasonText).X;
                int btnWidth = textW + 56;
                this.seasonButtons[i] = new ClickableComponent(
                    new Rectangle(seasonX, contentArea.Y + 4, btnWidth, this.btnH),
                    i.ToString()
                ) { myID = 6800 + i };
                seasonX += btnWidth + 4;
            }

            // Filter buttons (secondary row)
            int filterX = contentArea.X;
            for (int i = 0; i < filterLabels.Length; i++)
            {
                int textW = (int)Game1.smallFont.MeasureString(filterLabels[i]).X;
                int btnWidth = textW + 56;
                filterButtons[i] = new ClickableComponent(
                    new Rectangle(filterX, contentArea.Y + this.seasonBarHeight + 4, btnWidth, this.btnH),
                    i.ToString()
                ) { myID = 7000 + i };
                filterX += btnWidth + 4;
            }

            // Toolbar row: quick-add input + Add button + New Task button
            int toolbarY = contentArea.Y + this.seasonBarHeight + this.filterBarHeight;

            int newBtnTextW = (int)Game1.smallFont.MeasureString("+ New Task").X;
            int newBtnW = newBtnTextW + 56;
            int addBtnTextW = (int)Game1.smallFont.MeasureString("+ Add").X;
            int addBtnW = addBtnTextW + 56;

            int inputWidth = contentArea.Width - addBtnW - newBtnW - 28;
            this.quickAddInput = new TextInputComponent(
                contentArea.X, toolbarY, inputWidth, this.btnH, "Quick add task..."
            );

            this.addButton = new ClickableComponent(
                new Rectangle(contentArea.X + inputWidth + 8, toolbarY, addBtnW, this.btnH),
                "addBtn"
            ) { myID = 8000 };

            this.newTaskButton = new ClickableComponent(
                new Rectangle(contentArea.X + inputWidth + addBtnW + 16, toolbarY, newBtnW, this.btnH),
                "newTaskBtn"
            ) { myID = 8001 };

            // Scrollable list — below summary
            int listTop = contentArea.Y + this.seasonBarHeight + this.filterBarHeight + this.toolbarHeight + this.summaryHeight;
            int listHeight = contentArea.Height - this.seasonBarHeight - this.filterBarHeight - this.toolbarHeight - this.summaryHeight - 8;
            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, listTop, contentArea.Width, Math.Max(listHeight, 100))
            );

            this.Refresh();
        }

        public void Refresh()
        {
            var allActive = this.taskManager.GetActiveTasks(this.activeFilter, this.activeSeason);
            this.dailyTasks = allActive.Where(t => t.Priority == TaskPriority.Daily).ToList();
            this.weeklyTasks = allActive.Where(t => t.Priority == TaskPriority.Weekly).ToList();
            this.monthlyTasks = allActive.Where(t => t.Priority == TaskPriority.Monthly).ToList();
            this.completedTasks = this.taskManager.GetCompletedTasks(this.activeFilter, this.activeSeason);
            this.totalActiveCount = allActive.Count;

            int totalHeight = 0;

            if (this.dailyTasks.Count > 0)
                totalHeight += this.sectionHeaderHeight + this.dailyTasks.Count * TaskRowComponent.RowHeight + this.sectionGap;
            if (this.weeklyTasks.Count > 0)
                totalHeight += this.sectionHeaderHeight + this.weeklyTasks.Count * TaskRowComponent.RowHeight + this.sectionGap;
            if (this.monthlyTasks.Count > 0)
                totalHeight += this.sectionHeaderHeight + this.monthlyTasks.Count * TaskRowComponent.RowHeight + this.sectionGap;
            if (this.completedTasks.Count > 0)
                totalHeight += this.sectionHeaderHeight + this.completedTasks.Count * TaskRowComponent.RowHeight + this.sectionGap;

            if (totalHeight == 0)
                totalHeight = 100;

            this.scrollableList.SetContentHeight(totalHeight);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            // Season tabs
            for (int i = 0; i < this.seasonButtons.Length; i++)
            {
                if (this.seasonButtons[i].containsPoint(x, y))
                {
                    this.activeSeason = SeasonHelper.OrderedSeasons[i];
                    this.scrollableList.Reset();
                    this.Refresh();
                    Game1.playSound("shwip");
                    return;
                }
            }

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

            // Add button (quick add)
            if (this.addButton.containsPoint(x, y))
            {
                this.DoQuickAdd();
                return;
            }

            // New Task button (opens full detail page)
            if (this.newTaskButton.containsPoint(x, y))
            {
                this.OpenNewTask();
                return;
            }

            if (this.quickAddInput.IsSelected)
                this.quickAddInput.Deselect();

            // Scrollbar
            if (this.scrollableList.HandleLeftClick(x, y))
                return;

            // Task rows - check each priority group
            var listBounds = this.scrollableList.Bounds;
            if (!listBounds.Contains(x, y))
                return;

            int rowY = listBounds.Y - this.scrollableList.ScrollOffset;
            int rowWidth = listBounds.Width - 40;

            if (this.dailyTasks.Count > 0)
            {
                rowY += this.sectionHeaderHeight;
                if (this.HandleTaskGroupClick(this.dailyTasks, x, y, ref rowY, listBounds, rowWidth))
                    return;
                rowY += this.sectionGap;
            }
            if (this.weeklyTasks.Count > 0)
            {
                rowY += this.sectionHeaderHeight;
                if (this.HandleTaskGroupClick(this.weeklyTasks, x, y, ref rowY, listBounds, rowWidth))
                    return;
                rowY += this.sectionGap;
            }
            if (this.monthlyTasks.Count > 0)
            {
                rowY += this.sectionHeaderHeight;
                if (this.HandleTaskGroupClick(this.monthlyTasks, x, y, ref rowY, listBounds, rowWidth))
                    return;
                rowY += this.sectionGap;
            }
            if (this.completedTasks.Count > 0)
            {
                rowY += this.sectionHeaderHeight;
                if (this.HandleTaskGroupClick(this.completedTasks, x, y, ref rowY, listBounds, rowWidth))
                    return;
            }
        }

        private bool HandleTaskGroupClick(List<PlannerTask> tasks, int x, int y, ref int rowY, Rectangle listBounds, int rowWidth)
        {
            foreach (var task in tasks)
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
                            this.parentMenu.OpenTaskDetail(task, false, this.activeSeason);
                        }
                        return true;
                    }
                }
                rowY += TaskRowComponent.RowHeight;
            }
            return false;
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

            var allGroups = new[] { this.dailyTasks, this.weeklyTasks, this.monthlyTasks };
            foreach (var group in allGroups)
            {
                if (group.Count == 0) continue;
                rowY += this.sectionHeaderHeight;

                foreach (var task in group)
                {
                    if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                    {
                        if (TaskRowComponent.GetRowBounds(listBounds.X, rowY, rowWidth).Contains(x, y))
                        {
                            this.hoveredTaskId = task.Id;
                            this.hoverTooltip = this.BuildTooltip(task);
                            return;
                        }
                    }
                    rowY += TaskRowComponent.RowHeight;
                }
                rowY += this.sectionGap;
            }
        }

        private string BuildTooltip(PlannerTask task)
        {
            string tip = "";
            if (!string.IsNullOrEmpty(task.Notes))
                tip += task.Notes;

            if (task.SubTasks.Count > 0)
            {
                int done = task.SubTasks.Count(s => s.IsCompleted);
                if (tip.Length > 0) tip += "\n\n";
                tip += $"Sub-tasks: {done}/{task.SubTasks.Count}";
                foreach (var sub in task.SubTasks)
                {
                    string check = sub.IsCompleted ? "[X]" : "[ ]";
                    tip += $"\n  {check} {sub.Text}";
                }
            }

            if (task.CompletionCount > 0)
            {
                if (tip.Length > 0) tip += "\n";
                tip += $"Completed {task.CompletionCount} times";
            }

            return string.IsNullOrEmpty(tip) ? null : tip;
        }

        public void Draw(SpriteBatch b)
        {
            // ── Season tabs ──
            for (int i = 0; i < this.seasonButtons.Length; i++)
            {
                Season season = SeasonHelper.OrderedSeasons[i];
                bool isActive = this.activeSeason == season;

                var sb = this.seasonButtons[i].bounds;
                Color seasonColor = SeasonHelper.GetColor(season);
                Color seasonBg = isActive ? seasonColor * 0.35f : Color.White;

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                    new Rectangle(384, 396, 15, 15),
                    sb.X, sb.Y, sb.Width, sb.Height,
                    seasonBg, 4f, false);

                if (isActive)
                {
                    b.Draw(Game1.staminaRect,
                        new Rectangle(sb.X + 20, sb.Y + sb.Height - 4, sb.Width - 40, 3),
                        seasonColor);
                }

                string seasonText = SeasonHelper.GetDisplayName(season);
                var ts = Game1.smallFont.MeasureString(seasonText);
                Color textColor = isActive ? seasonColor : Game1.textColor;
                Utility.drawTextWithShadow(b, seasonText, Game1.smallFont,
                    new Vector2(sb.X + (sb.Width - ts.X) / 2, sb.Y + (sb.Height - ts.Y) / 2),
                    textColor, 1f, -1f, -1, -1, 1f, 3);
            }

            // ── Filter bar ──
            for (int i = 0; i < this.filterButtons.Length; i++)
            {
                bool isActive = (i == 0 && !this.activeFilter.HasValue) ||
                                (i > 0 && this.activeFilter.HasValue && (int)this.activeFilter.Value == i - 1);

                var fb = this.filterButtons[i].bounds;
                Color filterBg = isActive ? CategoryFilterColors[i] * 0.3f : Color.White;

                IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                    new Rectangle(384, 396, 15, 15),
                    fb.X, fb.Y, fb.Width, fb.Height,
                    filterBg, 4f, false);

                if (isActive)
                {
                    b.Draw(Game1.staminaRect,
                        new Rectangle(fb.X + 20, fb.Y + fb.Height - 4, fb.Width - 40, 3),
                        CategoryFilterColors[i]);
                }

                var ts = Game1.smallFont.MeasureString(this.filterLabels[i]);
                Color textColor = isActive ? CategoryFilterColors[i] : Game1.textColor;
                Utility.drawTextWithShadow(b, this.filterLabels[i], Game1.smallFont,
                    new Vector2(fb.X + (fb.Width - ts.X) / 2, fb.Y + (fb.Height - ts.Y) / 2),
                    textColor, 1f, -1f, -1, -1, 1f, 3);
            }

            // ── Toolbar: quick add + buttons ──
            this.quickAddInput.Draw(b);
            DrawButton(b, "+ Add", this.addButton.bounds, Color.LightGreen);
            DrawButton(b, "+ New Task", this.newTaskButton.bounds, new Color(180, 210, 255));

            // ── Summary line ──
            int summaryY = contentArea.Y + this.seasonBarHeight + this.filterBarHeight + this.toolbarHeight;
            string summary = $"{this.totalActiveCount} active";
            if (this.completedTasks.Count > 0)
                summary += $"  /  {this.completedTasks.Count} completed";
            if (this.activeFilter.HasValue)
                summary += $"  ({this.filterLabels[(int)this.activeFilter.Value + 1]})";
            summary += $"  •  {SeasonHelper.GetDisplayName(this.activeSeason)}";
            b.DrawString(Game1.smallFont, summary,
                new Vector2(contentArea.X + 4, summaryY + 2), Color.Gray * 0.8f);

            // ── Scrollable task list ──
            this.scrollableList.BeginScissorRect(b);

            var listBounds = this.scrollableList.Bounds;
            int rowY = listBounds.Y - this.scrollableList.ScrollOffset;
            int rowWidth = listBounds.Width - 40;

            bool hasAnyTasks = this.dailyTasks.Count > 0 || this.weeklyTasks.Count > 0 ||
                               this.monthlyTasks.Count > 0 || this.completedTasks.Count > 0;

            if (!hasAnyTasks)
            {
                string msg = this.activeFilter.HasValue
                    ? $"No {this.filterLabels[(int)this.activeFilter.Value + 1]} tasks for {SeasonHelper.GetDisplayName(this.activeSeason)}."
                    : "No tasks yet! Use '+ Add' for a quick task or '+ New Task' for details.";
                var msgSize = Game1.smallFont.MeasureString(msg);
                b.DrawString(Game1.smallFont, msg,
                    new Vector2(listBounds.X + (listBounds.Width - msgSize.X) / 2, listBounds.Y + 40),
                    Color.Gray);
            }
            else
            {
                if (this.dailyTasks.Count > 0)
                {
                    this.DrawSectionHeader(b, $"Daily ({this.dailyTasks.Count})", DailyColor, listBounds, rowY);
                    rowY += this.sectionHeaderHeight;
                    foreach (var task in this.dailyTasks)
                    {
                        if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                            TaskRowComponent.Draw(b, task, listBounds.X, rowY, rowWidth, this.hoveredTaskId == task.Id);
                        rowY += TaskRowComponent.RowHeight;
                    }
                    rowY += this.sectionGap;
                }

                if (this.weeklyTasks.Count > 0)
                {
                    this.DrawSectionHeader(b, $"Weekly ({this.weeklyTasks.Count})", WeeklyColor, listBounds, rowY);
                    rowY += this.sectionHeaderHeight;
                    foreach (var task in this.weeklyTasks)
                    {
                        if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                            TaskRowComponent.Draw(b, task, listBounds.X, rowY, rowWidth, this.hoveredTaskId == task.Id);
                        rowY += TaskRowComponent.RowHeight;
                    }
                    rowY += this.sectionGap;
                }

                if (this.monthlyTasks.Count > 0)
                {
                    this.DrawSectionHeader(b, $"Monthly ({this.monthlyTasks.Count})", MonthlyColor, listBounds, rowY);
                    rowY += this.sectionHeaderHeight;
                    foreach (var task in this.monthlyTasks)
                    {
                        if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                            TaskRowComponent.Draw(b, task, listBounds.X, rowY, rowWidth, this.hoveredTaskId == task.Id);
                        rowY += TaskRowComponent.RowHeight;
                    }
                    rowY += this.sectionGap;
                }

                if (this.completedTasks.Count > 0)
                {
                    this.DrawSectionHeader(b, $"Completed ({this.completedTasks.Count})", CompletedColor, listBounds, rowY);
                    rowY += this.sectionHeaderHeight;
                    foreach (var task in this.completedTasks)
                    {
                        if (rowY + TaskRowComponent.RowHeight > listBounds.Y && rowY < listBounds.Bottom)
                            TaskRowComponent.Draw(b, task, listBounds.X, rowY, rowWidth, this.hoveredTaskId == task.Id, dimmed: true);
                        rowY += TaskRowComponent.RowHeight;
                    }
                }
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);

            // Hover tooltip
            if (!string.IsNullOrEmpty(this.hoverTooltip))
                IClickableMenu.drawHoverText(b, this.hoverTooltip, Game1.smallFont);
        }

        private void DrawSectionHeader(SpriteBatch b, string text, Color color, Rectangle listBounds, int y)
        {
            if (y + this.sectionHeaderHeight <= listBounds.Y || y >= listBounds.Bottom)
                return;

            b.Draw(Game1.staminaRect,
                new Rectangle(listBounds.X + 4, y + 6, listBounds.Width - 48, 2),
                color * 0.4f);

            Utility.drawTextWithShadow(b, text, Game1.smallFont,
                new Vector2(listBounds.X + 8, y + 10),
                color * 0.9f, 1f, -1f, -1, -1, 1f, 3);
        }

        private void DrawButton(SpriteBatch b, string text, Rectangle bounds, Color color)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                bounds.X, bounds.Y, bounds.Width, bounds.Height,
                color, 4f, false);
            var ts = Game1.smallFont.MeasureString(text);
            Utility.drawTextWithShadow(b, text, Game1.smallFont,
                new Vector2(bounds.X + (bounds.Width - ts.X) / 2, bounds.Y + (bounds.Height - ts.Y) / 2),
                Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
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
                Priority = TaskPriority.Daily,
                IsRecurring = true,
                TargetSeason = this.activeSeason
            };

            this.taskManager.AddTask(task);
            this.quickAddInput.Text = "";
            Game1.playSound("coin");
            this.Refresh();
        }

        private void OpenNewTask()
        {
            var task = new PlannerTask
            {
                Category = this.activeFilter ?? TaskCategory.Farm,
                Priority = TaskPriority.Daily,
                TargetSeason = this.activeSeason
            };
            this.parentMenu.OpenTaskDetail(task, true, this.activeSeason);
            Game1.playSound("bigSelect");
        }
    }
}
