using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StarDo.UI.Components;

namespace StarDo.UI.Pages
{
    public class QuestPage
    {
        private Rectangle contentArea;
        private ScrollableListComponent scrollableList;

        private List<QuestDisplayInfo> quests = new();
        private List<SpecialOrderDisplayInfo> specialOrders = new();

        private readonly int lineH;
        private readonly int headerH;
        private readonly int questCardH;
        private readonly int objLineH;

        public QuestPage(Rectangle contentArea)
        {
            this.contentArea = contentArea;
            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.headerH = (int)Game1.dialogueFont.MeasureString("Tg").Y + 12;
            this.questCardH = this.lineH * 3 + 24;
            this.objLineH = this.lineH + 8;

            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, contentArea.Y, contentArea.Width, contentArea.Height)
            );
            this.Refresh();
        }

        public void Refresh()
        {
            this.quests.Clear();
            this.specialOrders.Clear();

            if (!Context.IsWorldReady)
                return;

            foreach (var quest in Game1.player.questLog)
            {
                if (quest == null || quest.completed.Value)
                    continue;
                this.quests.Add(new QuestDisplayInfo
                {
                    Title = quest.questTitle ?? "Unknown Quest",
                    Description = quest.questDescription ?? "",
                    DaysLeft = quest.daysLeft.Value > 0 ? quest.daysLeft.Value : -1
                });
            }

            foreach (var order in Game1.player.team.specialOrders)
            {
                if (order == null) continue;
                var info = new SpecialOrderDisplayInfo
                {
                    Title = order.GetName(),
                    Requester = order.requester.Value ?? "",
                    DaysLeft = order.GetDaysLeft()
                };
                foreach (var obj in order.objectives)
                {
                    if (obj == null) continue;
                    info.Objectives.Add(new ObjectiveInfo
                    {
                        Description = obj.GetDescription(),
                        CurrentCount = obj.GetCount(),
                        MaxCount = obj.GetMaxCount(),
                        IsComplete = obj.IsComplete()
                    });
                }
                this.specialOrders.Add(info);
            }

            int h = this.headerH;
            h += Math.Max(this.quests.Count, 1) * (this.questCardH + 8);
            h += 16 + this.headerH;
            foreach (var so in this.specialOrders)
                h += this.lineH * 2 + 24 + so.Objectives.Count * this.objLineH + 16;
            if (this.specialOrders.Count == 0)
                h += this.questCardH;
            this.scrollableList.SetContentHeight(h);
        }

        public void ReceiveLeftClick(int x, int y) => this.scrollableList.HandleLeftClick(x, y);
        public void ReceiveScrollWheel(int direction) => this.scrollableList.HandleScrollWheel(direction);

        public void Draw(SpriteBatch b)
        {
            this.scrollableList.BeginScissorRect(b);

            var bounds = this.scrollableList.Bounds;
            int y = bounds.Y - this.scrollableList.ScrollOffset;
            int cw = bounds.Width - 40;

            // Active Quests header
            Utility.drawTextWithShadow(b, "Active Quests", Game1.dialogueFont,
                new Vector2(bounds.X + 8, y), Color.Gold, 1f, -1f, -1, -1, 1f, 3);
            y += this.headerH;

            if (this.quests.Count == 0)
            {
                b.DrawString(Game1.smallFont, "No active quests.", new Vector2(bounds.X + 20, y + 8), Color.Gray);
                y += this.questCardH + 8;
            }
            else
            {
                foreach (var quest in this.quests)
                {
                    if (y + this.questCardH > bounds.Y && y < bounds.Bottom)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                            new Rectangle(384, 396, 15, 15),
                            bounds.X + 4, y, cw, this.questCardH,
                            Color.White, 4f, false);

                        Utility.drawTextWithShadow(b, quest.Title, Game1.smallFont,
                            new Vector2(bounds.X + 24, y + 20), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

                        if (!string.IsNullOrEmpty(quest.Description))
                        {
                            int maxChars = Math.Max(10, (cw - 48) / 8);
                            string desc = quest.Description.Length > maxChars
                                ? quest.Description[..(maxChars - 3)] + "..."
                                : quest.Description;
                            b.DrawString(Game1.smallFont, desc, new Vector2(bounds.X + 24, y + 20 + this.lineH + 4), Color.Gray);
                        }

                        if (quest.DaysLeft > 0)
                        {
                            string daysText = $"{quest.DaysLeft}d left";
                            var daysSize = Game1.smallFont.MeasureString(daysText);
                            Color daysColor = quest.DaysLeft <= 3 ? Color.Red : Color.DarkGoldenrod;
                            b.DrawString(Game1.smallFont, daysText,
                                new Vector2(bounds.X + cw - daysSize.X - 20, y + 20), daysColor);
                        }
                    }
                    y += this.questCardH + 8;
                }
            }

            // Special Orders header
            y += 8;
            Utility.drawTextWithShadow(b, "Special Orders", Game1.dialogueFont,
                new Vector2(bounds.X + 8, y), Color.Gold, 1f, -1f, -1, -1, 1f, 3);
            y += this.headerH;

            if (this.specialOrders.Count == 0)
            {
                b.DrawString(Game1.smallFont, "No active special orders.", new Vector2(bounds.X + 20, y + 8), Color.Gray);
            }
            else
            {
                foreach (var order in this.specialOrders)
                {
                    int orderH = this.lineH * 2 + 24 + order.Objectives.Count * this.objLineH;
                    if (y + orderH > bounds.Y && y < bounds.Bottom)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                            new Rectangle(384, 396, 15, 15),
                            bounds.X + 4, y, cw, orderH,
                            Color.White, 4f, false);

                        Utility.drawTextWithShadow(b, order.Title, Game1.smallFont,
                            new Vector2(bounds.X + 24, y + 20), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

                        if (!string.IsNullOrEmpty(order.Requester))
                            b.DrawString(Game1.smallFont, $"From: {order.Requester}",
                                new Vector2(bounds.X + 24, y + 20 + this.lineH + 4), Color.Gray);

                        if (order.DaysLeft > 0)
                        {
                            string dt = $"{order.DaysLeft}d left";
                            var ds = Game1.smallFont.MeasureString(dt);
                            Color dc = order.DaysLeft <= 3 ? Color.Red : Color.DarkGoldenrod;
                            b.DrawString(Game1.smallFont, dt, new Vector2(bounds.X + cw - ds.X - 20, y + 20), dc);
                        }

                        int objY = y + this.lineH * 2 + 20;
                        foreach (var obj in order.Objectives)
                        {
                            int barW = Math.Min(200, cw / 3);
                            int barH = this.lineH - 4;
                            int barX = bounds.X + 28;

                            b.Draw(Game1.staminaRect, new Rectangle(barX, objY + 2, barW, barH), Color.Black * 0.2f);
                            float progress = obj.MaxCount > 0 ? Math.Clamp((float)obj.CurrentCount / obj.MaxCount, 0, 1) : (obj.IsComplete ? 1 : 0);
                            Color barC = obj.IsComplete ? Color.Green : Color.Gold;
                            b.Draw(Game1.staminaRect, new Rectangle(barX, objY + 2, (int)(barW * progress), barH), barC * 0.7f);

                            string objText = obj.Description;
                            if (obj.MaxCount > 0) objText += $" ({obj.CurrentCount}/{obj.MaxCount})";
                            b.DrawString(Game1.smallFont, objText,
                                new Vector2(barX + barW + 12, objY), obj.IsComplete ? Color.Green : Game1.textColor);

                            objY += this.objLineH;
                        }
                    }
                    y += orderH + 8;
                }
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);
        }

        private class QuestDisplayInfo { public string Title; public string Description; public int DaysLeft; }
        private class SpecialOrderDisplayInfo { public string Title; public string Requester; public int DaysLeft; public List<ObjectiveInfo> Objectives = new(); }
        private class ObjectiveInfo { public string Description; public int CurrentCount; public int MaxCount; public bool IsComplete; }
    }
}
