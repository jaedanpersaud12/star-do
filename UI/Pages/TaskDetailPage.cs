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
    public class TaskDetailPage
    {
        private readonly PlannerTask task;
        private readonly bool isNew;
        private readonly TaskManager taskManager;
        private readonly PlannerMenu parentMenu;

        private TextInputComponent titleInput;
        private TextInputComponent notesInput;
        private TextInputComponent subTaskInput;

        private ClickableComponent categoryButton;
        private ClickableComponent priorityButton;
        private ClickableComponent recurringToggle;
        private ClickableComponent saveButton;
        private ClickableComponent deleteButton;
        private ClickableComponent cancelButton;

        private ScrollableListComponent scrollableList;

        private readonly Rectangle dialogBounds;

        private static readonly string[] CategoryNames = { "Farm", "Processing", "Social", "Goals", "Quests" };
        private static readonly string[] PriorityNames = { "Daily", "Weekly", "Long-term" };

        public TaskDetailPage(PlannerTask task, bool isNew, TaskManager taskManager, PlannerMenu parentMenu)
        {
            this.task = task;
            this.isNew = isNew;
            this.taskManager = taskManager;
            this.parentMenu = parentMenu;

            int dialogWidth = Math.Min(700, (int)(Game1.uiViewport.Width * 0.5f));
            int dialogHeight = Math.Min(700, (int)(Game1.uiViewport.Height * 0.7f));
            var center = Utility.getTopLeftPositionForCenteringOnScreen(dialogWidth, dialogHeight);
            this.dialogBounds = new Rectangle((int)center.X, (int)center.Y, dialogWidth, dialogHeight);

            this.scrollableList = new ScrollableListComponent(
                new Rectangle(dialogBounds.X + 20, dialogBounds.Y + 20, dialogBounds.Width - 40, dialogBounds.Height - 80)
            );

            int inputX = dialogBounds.X + 40;
            int inputWidth = dialogBounds.Width - 80;

            this.titleInput = new TextInputComponent(inputX, 0, inputWidth, 40, "Task title...");
            this.titleInput.Text = task.Title;

            this.notesInput = new TextInputComponent(inputX, 0, inputWidth, 40, "Why am I doing this?");
            this.notesInput.Text = task.Notes;

            this.subTaskInput = new TextInputComponent(inputX, 0, inputWidth - 80, 36, "Add sub-task...");

            this.categoryButton = new ClickableComponent(new Rectangle(inputX, 0, 160, 36), "category") { myID = 5000 };
            this.priorityButton = new ClickableComponent(new Rectangle(inputX + 170, 0, 160, 36), "priority") { myID = 5001 };
            this.recurringToggle = new ClickableComponent(new Rectangle(inputX + 340, 0, 160, 36), "recurring") { myID = 5002 };

            int btnWidth = 100;
            int btnSpacing = 12;
            int totalBtnWidth = btnWidth * 3 + btnSpacing * 2;
            int btnX = dialogBounds.X + (dialogBounds.Width - totalBtnWidth) / 2;
            int btnY = dialogBounds.Bottom - 56;

            this.saveButton = new ClickableComponent(new Rectangle(btnX, btnY, btnWidth, 40), "save") { myID = 5010 };
            this.cancelButton = new ClickableComponent(new Rectangle(btnX + btnWidth + btnSpacing, btnY, btnWidth, 40), "cancel") { myID = 5011 };
            this.deleteButton = new ClickableComponent(new Rectangle(btnX + (btnWidth + btnSpacing) * 2, btnY, btnWidth, 40), "delete") { myID = 5012 };

            this.UpdateContentHeight();
        }

        public void ReceiveLeftClick(int x, int y)
        {
            // Save button
            if (this.saveButton.containsPoint(x, y))
            {
                this.SaveTask();
                return;
            }

            // Cancel button
            if (this.cancelButton.containsPoint(x, y))
            {
                this.parentMenu.CloseTaskDetail();
                return;
            }

            // Delete button
            if (this.deleteButton.containsPoint(x, y) && !this.isNew)
            {
                this.taskManager.DeleteTask(this.task.Id);
                Game1.playSound("trashcan");
                this.parentMenu.CloseTaskDetail();
                return;
            }

            // Click outside dialog to cancel
            if (!this.dialogBounds.Contains(x, y))
            {
                this.parentMenu.CloseTaskDetail();
                return;
            }

            // Deselect all inputs first
            this.titleInput.Deselect();
            this.notesInput.Deselect();
            this.subTaskInput.Deselect();

            // Compute positions for interactive elements
            int contentY = this.scrollableList.Bounds.Y - this.scrollableList.ScrollOffset;
            int inputX = this.dialogBounds.X + 40;
            int inputWidth = this.dialogBounds.Width - 80;

            // Title (at contentY + 24)
            int titleY = contentY + 24;
            if (new Rectangle(inputX, titleY, inputWidth, 40).Contains(x, y))
            {
                this.titleInput.Select();
                return;
            }

            // Notes (at titleY + 64)
            int notesY = titleY + 64;
            if (new Rectangle(inputX, notesY, inputWidth, 40).Contains(x, y))
            {
                this.notesInput.Select();
                return;
            }

            // Category/Priority/Recurring (at notesY + 60)
            int optionsY = notesY + 60;
            this.categoryButton.bounds = new Rectangle(inputX, optionsY, 160, 36);
            this.priorityButton.bounds = new Rectangle(inputX + 170, optionsY, 160, 36);
            this.recurringToggle.bounds = new Rectangle(inputX + 340, optionsY, 160, 36);

            if (this.categoryButton.containsPoint(x, y))
            {
                this.task.Category = (TaskCategory)(((int)this.task.Category + 1) % 5);
                Game1.playSound("shwip");
                return;
            }
            if (this.priorityButton.containsPoint(x, y))
            {
                this.task.Priority = (TaskPriority)(((int)this.task.Priority + 1) % 3);
                Game1.playSound("shwip");
                return;
            }
            if (this.recurringToggle.containsPoint(x, y))
            {
                this.task.IsRecurring = !this.task.IsRecurring;
                Game1.playSound("shwip");
                return;
            }

            // Sub-tasks area
            int subY = optionsY + 56;
            // Sub-task input
            int subInputY = subY + 28;
            if (new Rectangle(inputX, subInputY, inputWidth - 80, 36).Contains(x, y))
            {
                this.subTaskInput.Select();
                return;
            }
            // Add sub-task button
            if (new Rectangle(inputX + inputWidth - 72, subInputY, 72, 36).Contains(x, y))
            {
                this.AddSubTask();
                return;
            }

            // Sub-task checkboxes and delete
            int itemY = subInputY + 48;
            for (int i = 0; i < this.task.SubTasks.Count; i++)
            {
                // Checkbox
                var cbBounds = new Rectangle(inputX, itemY + 4, 28, 28);
                if (cbBounds.Contains(x, y))
                {
                    this.task.SubTasks[i].IsCompleted = !this.task.SubTasks[i].IsCompleted;
                    Game1.playSound("coin");
                    return;
                }

                // Delete button (X)
                var delBounds = new Rectangle(inputX + inputWidth - 32, itemY + 4, 28, 28);
                if (delBounds.Contains(x, y))
                {
                    this.task.SubTasks.RemoveAt(i);
                    Game1.playSound("trashcan");
                    this.UpdateContentHeight();
                    return;
                }

                itemY += 40;
            }
        }

        public void ReceiveKeyPress(Keys key)
        {
            if (this.titleInput.IsSelected || this.notesInput.IsSelected || this.subTaskInput.IsSelected)
            {
                if (key == Keys.Enter && this.subTaskInput.IsSelected)
                {
                    this.AddSubTask();
                    return;
                }
                if (key == Keys.Escape)
                {
                    this.titleInput.Deselect();
                    this.notesInput.Deselect();
                    this.subTaskInput.Deselect();
                }
                return;
            }

            if (key == Keys.Escape)
            {
                this.parentMenu.CloseTaskDetail();
            }
        }

        public void ReceiveScrollWheel(int direction)
        {
            this.scrollableList.HandleScrollWheel(direction);
        }

        public void Draw(SpriteBatch b)
        {
            // Dark backdrop
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);

            // Dialog box
            Game1.drawDialogueBox(
                this.dialogBounds.X, this.dialogBounds.Y,
                this.dialogBounds.Width, this.dialogBounds.Height,
                false, true);

            // Scrollable content
            this.scrollableList.BeginScissorRect(b);

            int contentY = this.scrollableList.Bounds.Y - this.scrollableList.ScrollOffset;
            int inputX = this.dialogBounds.X + 40;
            int inputWidth = this.dialogBounds.Width - 80;

            // Title label + input
            b.DrawString(Game1.smallFont, "Title:", new Vector2(inputX, contentY + 4), Game1.textColor);
            int titleY = contentY + 24;
            this.titleInput.Reposition(inputX, titleY, inputWidth, 40);
            this.titleInput.Draw(b);

            // Notes label + input
            int notesY = titleY + 64;
            b.DrawString(Game1.smallFont, "Notes (why?):", new Vector2(inputX, notesY - 20), Game1.textColor);
            this.notesInput.Reposition(inputX, notesY, inputWidth, 40);
            this.notesInput.Draw(b);

            // Category / Priority / Recurring
            int optionsY = notesY + 60;
            this.DrawOptionButton(b, "Cat: " + CategoryNames[(int)this.task.Category], inputX, optionsY, 160, 36, Color.LightBlue);
            this.DrawOptionButton(b, "Pri: " + PriorityNames[(int)this.task.Priority], inputX + 170, optionsY, 160, 36, Color.LightGoldenrodYellow);
            this.DrawOptionButton(b, this.task.IsRecurring ? "Recurring: ON" : "Recurring: OFF",
                inputX + 340, optionsY, 160, 36,
                this.task.IsRecurring ? Color.LightGreen : Color.LightGray);

            // Sub-tasks section
            int subY = optionsY + 56;
            b.DrawString(Game1.smallFont, "Sub-tasks:", new Vector2(inputX, subY), Game1.textColor);

            int subInputY = subY + 28;
            this.subTaskInput.Reposition(inputX, subInputY, inputWidth - 80, 36);
            this.subTaskInput.Draw(b);
            this.DrawOptionButton(b, "+ Add", inputX + inputWidth - 72, subInputY, 72, 36, Color.LightGreen);

            int itemY = subInputY + 48;
            for (int i = 0; i < this.task.SubTasks.Count; i++)
            {
                var sub = this.task.SubTasks[i];

                // Checkbox
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                    new Rectangle(403, 383, 6, 6),
                    inputX, itemY + 4, 28, 28,
                    Color.White, 4f, false);
                if (sub.IsCompleted)
                {
                    b.Draw(Game1.mouseCursors,
                        new Rectangle(inputX + 4, itemY + 8, 20, 20),
                        new Rectangle(236, 425, 9, 9),
                        Color.White);
                }

                // Text
                Color textColor = sub.IsCompleted ? Color.Gray : Game1.textColor;
                b.DrawString(Game1.smallFont, sub.Text, new Vector2(inputX + 36, itemY + 6), textColor);

                // Delete X
                this.DrawOptionButton(b, "X", inputX + inputWidth - 32, itemY + 4, 28, 28, Color.IndianRed);

                itemY += 40;
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);

            // Bottom buttons
            this.DrawOptionButton(b, "Save", this.saveButton.bounds.X, this.saveButton.bounds.Y,
                this.saveButton.bounds.Width, this.saveButton.bounds.Height, Color.LightGreen);
            this.DrawOptionButton(b, "Cancel", this.cancelButton.bounds.X, this.cancelButton.bounds.Y,
                this.cancelButton.bounds.Width, this.cancelButton.bounds.Height, Color.LightGray);
            if (!this.isNew)
            {
                this.DrawOptionButton(b, "Delete", this.deleteButton.bounds.X, this.deleteButton.bounds.Y,
                    this.deleteButton.bounds.Width, this.deleteButton.bounds.Height, Color.IndianRed);
            }
        }

        public void Cleanup()
        {
            this.titleInput?.Deselect();
            this.notesInput?.Deselect();
            this.subTaskInput?.Deselect();
        }

        private void DrawOptionButton(SpriteBatch b, string text, int x, int y, int width, int height, Color bgColor)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                x, y, width, height,
                bgColor, 4f, false);

            var textSize = Game1.smallFont.MeasureString(text);
            b.DrawString(Game1.smallFont, text,
                new Vector2(x + (width - textSize.X) / 2, y + (height - textSize.Y) / 2),
                Game1.textColor);
        }

        private void AddSubTask()
        {
            string text = this.subTaskInput.Text?.Trim();
            if (string.IsNullOrEmpty(text))
                return;

            this.task.SubTasks.Add(new SubTask { Text = text });
            this.subTaskInput.Text = "";
            Game1.playSound("coin");
            this.UpdateContentHeight();
        }

        private void SaveTask()
        {
            this.task.Title = this.titleInput.Text?.Trim() ?? "";
            this.task.Notes = this.notesInput.Text?.Trim() ?? "";

            if (string.IsNullOrEmpty(this.task.Title))
            {
                Game1.playSound("cancel");
                return;
            }

            if (this.isNew)
            {
                this.task.CreatedDay = Game1.Date.TotalDays;
                this.taskManager.AddTask(this.task);
            }
            else
            {
                this.taskManager.UpdateTask(this.task);
            }

            Game1.playSound("coin");
            this.parentMenu.CloseTaskDetail();
        }

        private void UpdateContentHeight()
        {
            int height = 300 + this.task.SubTasks.Count * 40;
            this.scrollableList.SetContentHeight(height);
        }
    }
}
