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

        private ClickableComponent saveButton;
        private ClickableComponent deleteButton;
        private ClickableComponent cancelButton;

        private ScrollableListComponent scrollableList;
        private readonly Rectangle dialogBounds;
        private bool confirmingDelete;

        private static readonly string[] CategoryNames = { "Farm", "Processing", "Social", "Goals", "Quests" };
        private static readonly int CategoryCount = Enum.GetValues(typeof(TaskCategory)).Length;
        private static readonly string[] PriorityNames = { "Daily", "Weekly", "Long-term" };
        private static readonly int PriorityCount = Enum.GetValues(typeof(TaskPriority)).Length;

        // Border overhead for drawTextureBox with (384,396,15,15) at 4f = 20px per side
        private const int BoxBorder = 20;

        private readonly int lineH;
        private readonly int btnH;
        private readonly int inputH;
        private readonly int spacing;
        private readonly int subItemH;
        private readonly int checkboxSize;

        public TaskDetailPage(PlannerTask task, bool isNew, TaskManager taskManager, PlannerMenu parentMenu)
        {
            this.task = task;
            this.isNew = isNew;
            this.taskManager = taskManager;
            this.parentMenu = parentMenu;

            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.btnH = this.lineH + BoxBorder * 2 + 8; // text + borders + 4px padding each side
            this.inputH = this.lineH + 16;
            this.spacing = 16;
            this.subItemH = this.lineH + 16;
            this.checkboxSize = this.lineH + 8;

            int dialogWidth = Math.Clamp((int)(Game1.uiViewport.Width * 0.55f), 640, 960);
            int dialogHeight = Math.Clamp((int)(Game1.uiViewport.Height * 0.8f), 540, 860);
            var center = Utility.getTopLeftPositionForCenteringOnScreen(dialogWidth, dialogHeight);
            this.dialogBounds = new Rectangle((int)center.X, (int)center.Y, dialogWidth, dialogHeight);

            int borderW = IClickableMenu.borderWidth;
            int topSpace = IClickableMenu.spaceToClearTopBorder;
            int innerX = this.dialogBounds.X + borderW + 12;
            int innerY = this.dialogBounds.Y + borderW + topSpace;
            int innerW = this.dialogBounds.Width - borderW * 2 - 24;
            int innerH = this.dialogBounds.Height - borderW * 2 - topSpace - this.btnH - 32;

            this.scrollableList = new ScrollableListComponent(
                new Rectangle(innerX, innerY, innerW, innerH)
            );

            this.titleInput = new TextInputComponent(0, 0, innerW, this.inputH, "Task title...");
            this.titleInput.Text = task.Title;

            this.notesInput = new TextInputComponent(0, 0, innerW, this.inputH, "Why am I doing this?");
            this.notesInput.Text = task.Notes;

            this.subTaskInput = new TextInputComponent(0, 0, innerW - 120, this.inputH, "Add sub-task...");

            // Bottom buttons — auto-sized to text
            int saveBtnW = MeasureBtnWidth("Save");
            int cancelBtnW = MeasureBtnWidth("Cancel");
            int deleteBtnW = MeasureBtnWidth("Confirm?"); // size for widest text
            int btnGap = 16;
            int totalBtnW = saveBtnW + cancelBtnW + (this.isNew ? 0 : deleteBtnW + btnGap) + btnGap;
            int btnX = this.dialogBounds.X + (this.dialogBounds.Width - totalBtnW) / 2;
            int btnY = this.dialogBounds.Bottom - borderW - this.btnH - 12;

            this.saveButton = new ClickableComponent(new Rectangle(btnX, btnY, saveBtnW, this.btnH), "save");
            this.cancelButton = new ClickableComponent(new Rectangle(btnX + saveBtnW + btnGap, btnY, cancelBtnW, this.btnH), "cancel");
            this.deleteButton = new ClickableComponent(new Rectangle(btnX + saveBtnW + cancelBtnW + btnGap * 2, btnY, deleteBtnW, this.btnH), "delete");

            this.UpdateContentHeight();
        }

        private int MeasureBtnWidth(string text)
        {
            return (int)Game1.smallFont.MeasureString(text).X + BoxBorder * 2 + 16;
        }

        private int GetInputX() => this.scrollableList.Bounds.X;
        private int GetInputW() => this.scrollableList.Bounds.Width;

        // Shared layout calculation — returns Y offsets for all sections
        private void CalcLayout(int startY, int iw, out int titleY, out int notesLabelY, out int notesY,
                                out int optY, out int[] optBtnWidths, out int[] optBtnX,
                                out int subLabelY, out int subInputY, out int addBtnW, out int itemsStartY)
        {
            int ix = this.GetInputX();

            // Title
            titleY = startY + this.lineH + 8;

            // Notes
            notesLabelY = titleY + this.inputH + this.spacing;
            notesY = notesLabelY + this.lineH + 4;

            // Options — auto-sized buttons in a row
            optY = notesY + this.inputH + this.spacing;

            string catText = CategoryNames[(int)this.task.Category];
            string priText = PriorityNames[(int)this.task.Priority];
            string recurText = this.task.IsRecurring ? "Recurring: ON" : "Recurring: OFF";

            optBtnWidths = new int[]
            {
                MeasureBtnWidth(catText),
                MeasureBtnWidth(priText),
                MeasureBtnWidth(recurText)
            };
            optBtnX = new int[]
            {
                ix,
                ix + optBtnWidths[0] + 8,
                ix + optBtnWidths[0] + optBtnWidths[1] + 16
            };

            // Sub-tasks
            subLabelY = optY + this.btnH + this.spacing;
            addBtnW = MeasureBtnWidth("+ Add");
            subInputY = subLabelY + this.lineH + 8;

            // Items
            itemsStartY = subInputY + this.inputH + this.spacing;
        }

        public void ReceiveLeftClick(int x, int y)
        {
            if (this.saveButton.containsPoint(x, y)) { this.SaveTask(); return; }
            if (this.cancelButton.containsPoint(x, y)) { this.confirmingDelete = false; this.parentMenu.CloseTaskDetail(); return; }
            if (this.deleteButton.containsPoint(x, y) && !this.isNew)
            {
                if (this.confirmingDelete)
                {
                    this.taskManager.DeleteTask(this.task.Id);
                    Game1.playSound("trashcan");
                    this.parentMenu.CloseTaskDetail();
                }
                else
                {
                    this.confirmingDelete = true;
                    Game1.playSound("dwop");
                }
                return;
            }

            // Clicking anywhere else resets delete confirmation
            this.confirmingDelete = false;

            if (!this.dialogBounds.Contains(x, y))
            {
                this.parentMenu.CloseTaskDetail();
                return;
            }

            this.titleInput.Deselect();
            this.notesInput.Deselect();
            this.subTaskInput.Deselect();

            int cy = this.scrollableList.Bounds.Y - this.scrollableList.ScrollOffset;
            int ix = this.GetInputX();
            int iw = this.GetInputW();

            this.CalcLayout(cy, iw, out int titleY, out int notesLabelY, out int notesY,
                            out int optY, out int[] optBtnWidths, out int[] optBtnX,
                            out int subLabelY, out int subInputY, out int addBtnW, out int itemsStartY);

            // Title input
            if (new Rectangle(ix, titleY, iw, this.inputH).Contains(x, y)) { this.titleInput.Select(); return; }

            // Notes input
            if (new Rectangle(ix, notesY, iw, this.inputH).Contains(x, y)) { this.notesInput.Select(); return; }

            // Option buttons
            if (new Rectangle(optBtnX[0], optY, optBtnWidths[0], this.btnH).Contains(x, y))
            {
                this.task.Category = (TaskCategory)(((int)this.task.Category + 1) % CategoryCount);
                Game1.playSound("shwip"); return;
            }
            if (new Rectangle(optBtnX[1], optY, optBtnWidths[1], this.btnH).Contains(x, y))
            {
                this.task.Priority = (TaskPriority)(((int)this.task.Priority + 1) % PriorityCount);
                Game1.playSound("shwip"); return;
            }
            if (new Rectangle(optBtnX[2], optY, optBtnWidths[2], this.btnH).Contains(x, y))
            {
                this.task.IsRecurring = !this.task.IsRecurring;
                Game1.playSound("shwip"); return;
            }

            // Sub-task input
            if (new Rectangle(ix, subInputY, iw - addBtnW - 8, this.inputH).Contains(x, y))
            {
                this.subTaskInput.Select(); return;
            }
            // Add sub-task button
            if (new Rectangle(ix + iw - addBtnW, subInputY, addBtnW, this.btnH).Contains(x, y))
            {
                this.AddSubTask(); return;
            }

            // Sub-task items
            int itemY = itemsStartY;
            for (int i = 0; i < this.task.SubTasks.Count; i++)
            {
                var cbRect = new Rectangle(ix, itemY + (this.subItemH - this.checkboxSize) / 2, this.checkboxSize, this.checkboxSize);
                if (cbRect.Contains(x, y))
                {
                    this.task.SubTasks[i].IsCompleted = !this.task.SubTasks[i].IsCompleted;
                    Game1.playSound("coin"); return;
                }

                var delRect = new Rectangle(ix + iw - 40, itemY + (this.subItemH - this.checkboxSize) / 2, this.checkboxSize, this.checkboxSize);
                if (delRect.Contains(x, y))
                {
                    this.task.SubTasks.RemoveAt(i);
                    Game1.playSound("trashcan");
                    this.UpdateContentHeight(); return;
                }

                itemY += this.subItemH;
            }
        }

        public void ReceiveKeyPress(Keys key)
        {
            if (this.titleInput.IsSelected || this.notesInput.IsSelected || this.subTaskInput.IsSelected)
            {
                if (key == Keys.Enter && this.subTaskInput.IsSelected) { this.AddSubTask(); return; }
                if (key == Keys.Escape)
                {
                    this.titleInput.Deselect();
                    this.notesInput.Deselect();
                    this.subTaskInput.Deselect();
                }
                return;
            }
            if (key == Keys.Escape)
                this.parentMenu.CloseTaskDetail();
        }

        public void ReceiveScrollWheel(int direction) => this.scrollableList.HandleScrollWheel(direction);

        public void Draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.5f);

            Game1.drawDialogueBox(
                this.dialogBounds.X, this.dialogBounds.Y,
                this.dialogBounds.Width, this.dialogBounds.Height,
                false, true);

            this.scrollableList.BeginScissorRect(b);

            int cy = this.scrollableList.Bounds.Y - this.scrollableList.ScrollOffset;
            int ix = this.GetInputX();
            int iw = this.GetInputW();

            this.CalcLayout(cy, iw, out int titleY, out int notesLabelY, out int notesY,
                            out int optY, out int[] optBtnWidths, out int[] optBtnX,
                            out int subLabelY, out int subInputY, out int addBtnW, out int itemsStartY);

            // Title
            Utility.drawTextWithShadow(b, "Title:", Game1.smallFont, new Vector2(ix, cy), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            this.titleInput.Reposition(ix, titleY, iw, this.inputH);
            this.titleInput.Draw(b);

            // Notes
            Utility.drawTextWithShadow(b, "Notes (why?):", Game1.smallFont, new Vector2(ix, notesLabelY), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
            this.notesInput.Reposition(ix, notesY, iw, this.inputH);
            this.notesInput.Draw(b);

            // Options row — auto-sized buttons
            string catText = CategoryNames[(int)this.task.Category];
            string priText = PriorityNames[(int)this.task.Priority];
            string recurText = this.task.IsRecurring ? "Recurring: ON" : "Recurring: OFF";

            DrawBtn(b, catText, optBtnX[0], optY, optBtnWidths[0], this.btnH, Color.LightBlue);
            DrawBtn(b, priText, optBtnX[1], optY, optBtnWidths[1], this.btnH, Color.LightGoldenrodYellow);
            DrawBtn(b, recurText, optBtnX[2], optY, optBtnWidths[2], this.btnH,
                this.task.IsRecurring ? Color.LightGreen : Color.LightGray);

            // Sub-tasks
            Utility.drawTextWithShadow(b, "Sub-tasks:", Game1.smallFont, new Vector2(ix, subLabelY), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

            this.subTaskInput.Reposition(ix, subInputY, iw - addBtnW - 8, this.inputH);
            this.subTaskInput.Draw(b);
            DrawBtn(b, "+ Add", ix + iw - addBtnW, subInputY, addBtnW, this.btnH, Color.LightGreen);

            int itemY = itemsStartY;
            for (int i = 0; i < this.task.SubTasks.Count; i++)
            {
                var sub = this.task.SubTasks[i];
                int cbY = itemY + (this.subItemH - this.checkboxSize) / 2;

                // Checkbox
                IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                    new Rectangle(403, 383, 6, 6),
                    ix, cbY, this.checkboxSize, this.checkboxSize,
                    sub.IsCompleted ? new Color(200, 240, 200) : Color.White, 4f, false);
                if (sub.IsCompleted)
                {
                    int inset = 8;
                    b.Draw(Game1.mouseCursors,
                        new Rectangle(ix + inset, cbY + inset, this.checkboxSize - inset * 2, this.checkboxSize - inset * 2),
                        new Rectangle(236, 425, 9, 9), Color.White);
                }

                // Text
                Color textColor = sub.IsCompleted ? Color.Gray : Game1.textColor;
                b.DrawString(Game1.smallFont, sub.Text,
                    new Vector2(ix + this.checkboxSize + 12, itemY + (this.subItemH - this.lineH) / 2), textColor);

                // Delete X
                DrawBtn(b, "X", ix + iw - 40, cbY, this.checkboxSize, this.checkboxSize, Color.IndianRed);

                itemY += this.subItemH;
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);

            // Bottom buttons
            DrawBtn(b, "Save", this.saveButton.bounds, Color.LightGreen);
            DrawBtn(b, "Cancel", this.cancelButton.bounds, Color.LightGray);
            if (!this.isNew)
            {
                string delText = this.confirmingDelete ? "Confirm?" : "Delete";
                Color delColor = this.confirmingDelete ? Color.Red : Color.IndianRed;
                DrawBtn(b, delText, this.deleteButton.bounds, delColor);
            }
        }

        public void Cleanup()
        {
            this.titleInput?.Deselect();
            this.notesInput?.Deselect();
            this.subTaskInput?.Deselect();
        }

        private void DrawBtn(SpriteBatch b, string text, int x, int y, int w, int h, Color color)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15), x, y, w, h, color, 4f, false);
            var ts = Game1.smallFont.MeasureString(text);
            Utility.drawTextWithShadow(b, text, Game1.smallFont,
                new Vector2(x + (w - ts.X) / 2, y + (h - ts.Y) / 2),
                Game1.textColor, 1f, -1f, -1, -1, 1f, 3);
        }

        private void DrawBtn(SpriteBatch b, string text, Rectangle r, Color color)
        {
            DrawBtn(b, text, r.X, r.Y, r.Width, r.Height, color);
        }

        private void AddSubTask()
        {
            string text = this.subTaskInput.Text?.Trim();
            if (string.IsNullOrEmpty(text)) return;
            this.task.SubTasks.Add(new SubTask { Text = text });
            this.subTaskInput.Text = "";
            Game1.playSound("coin");
            this.UpdateContentHeight();
        }

        private void SaveTask()
        {
            this.task.Title = this.titleInput.Text?.Trim() ?? "";
            this.task.Notes = this.notesInput.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(this.task.Title)) { Game1.playSound("cancel"); return; }

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
            int h = this.lineH + 8 + this.inputH + this.spacing         // Title label + input
                   + this.lineH + 4 + this.inputH + this.spacing         // Notes label + input
                   + this.btnH + this.spacing                            // Options row
                   + this.lineH + 8 + this.inputH + this.spacing         // Sub-task label + input
                   + this.task.SubTasks.Count * this.subItemH + 24;      // Sub-task items + bottom pad
            this.scrollableList.SetContentHeight(h);
        }
    }
}
