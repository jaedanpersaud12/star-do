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

        public QuestPage(Rectangle contentArea)
        {
            this.contentArea = contentArea;
            this.scrollableList = new ScrollableListComponent(
                new Rectangle(contentArea.X, contentArea.Y + 8, contentArea.Width, contentArea.Height - 16)
            );
            this.Refresh();
        }

        public void Refresh()
        {
            this.quests.Clear();
            this.specialOrders.Clear();

            if (!Context.IsWorldReady)
                return;

            // Read active quests
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

            // Read special orders
            foreach (var order in Game1.player.team.specialOrders)
            {
                if (order == null)
                    continue;

                var info = new SpecialOrderDisplayInfo
                {
                    Title = order.GetName(),
                    Requester = order.requester.Value ?? "",
                    DaysLeft = order.GetDaysLeft()
                };

                foreach (var obj in order.objectives)
                {
                    if (obj == null)
                        continue;

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

            // Calculate content height
            int height = 0;
            height += 40; // "Active Quests" header
            height += Math.Max(this.quests.Count, 1) * 80;
            height += 48; // spacing + "Special Orders" header
            height += Math.Max(this.specialOrders.Count, 1) * 120;

            this.scrollableList.SetContentHeight(height);
        }

        public void ReceiveLeftClick(int x, int y)
        {
            this.scrollableList.HandleLeftClick(x, y);
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

            // Active Quests Section
            b.DrawString(Game1.dialogueFont, "Active Quests", new Vector2(bounds.X + 8, y), Color.Gold);
            y += 44;

            if (this.quests.Count == 0)
            {
                b.DrawString(Game1.smallFont, "No active quests.", new Vector2(bounds.X + 16, y + 8), Color.Gray);
                y += 80;
            }
            else
            {
                foreach (var quest in this.quests)
                {
                    if (y + 80 > bounds.Y && y < bounds.Bottom)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                            new Rectangle(384, 396, 15, 15),
                            bounds.X + 4, y, contentWidth, 72,
                            Color.White, 4f, false);

                        Utility.drawTextWithShadow(b, quest.Title, Game1.smallFont,
                            new Vector2(bounds.X + 16, y + 8), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

                        if (!string.IsNullOrEmpty(quest.Description))
                        {
                            string desc = quest.Description.Length > 80 ? quest.Description[..77] + "..." : quest.Description;
                            b.DrawString(Game1.smallFont, desc, new Vector2(bounds.X + 16, y + 32), Color.Gray);
                        }

                        if (quest.DaysLeft > 0)
                        {
                            string daysText = $"{quest.DaysLeft}d left";
                            var daysSize = Game1.smallFont.MeasureString(daysText);
                            Color daysColor = quest.DaysLeft <= 3 ? Color.Red : Color.DarkGoldenrod;
                            b.DrawString(Game1.smallFont, daysText,
                                new Vector2(bounds.X + contentWidth - daysSize.X - 12, y + 8), daysColor);
                        }
                    }
                    y += 80;
                }
            }

            // Special Orders Section
            y += 8;
            b.DrawString(Game1.dialogueFont, "Special Orders", new Vector2(bounds.X + 8, y), Color.Gold);
            y += 44;

            if (this.specialOrders.Count == 0)
            {
                b.DrawString(Game1.smallFont, "No active special orders.", new Vector2(bounds.X + 16, y + 8), Color.Gray);
                y += 80;
            }
            else
            {
                foreach (var order in this.specialOrders)
                {
                    int orderHeight = 56 + order.Objectives.Count * 28;
                    if (y + orderHeight > bounds.Y && y < bounds.Bottom)
                    {
                        IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                            new Rectangle(384, 396, 15, 15),
                            bounds.X + 4, y, contentWidth, orderHeight,
                            Color.White, 4f, false);

                        Utility.drawTextWithShadow(b, order.Title, Game1.smallFont,
                            new Vector2(bounds.X + 16, y + 8), Game1.textColor, 1f, -1f, -1, -1, 1f, 3);

                        if (!string.IsNullOrEmpty(order.Requester))
                        {
                            b.DrawString(Game1.smallFont, $"From: {order.Requester}",
                                new Vector2(bounds.X + 16, y + 30), Color.Gray);
                        }

                        if (order.DaysLeft > 0)
                        {
                            string daysText = $"{order.DaysLeft}d left";
                            var daysSize = Game1.smallFont.MeasureString(daysText);
                            Color daysColor = order.DaysLeft <= 3 ? Color.Red : Color.DarkGoldenrod;
                            b.DrawString(Game1.smallFont, daysText,
                                new Vector2(bounds.X + contentWidth - daysSize.X - 12, y + 8), daysColor);
                        }

                        // Objectives with progress bars
                        int objY = y + 52;
                        foreach (var obj in order.Objectives)
                        {
                            // Progress bar background
                            int barWidth = 200;
                            int barHeight = 16;
                            int barX = bounds.X + 24;

                            b.Draw(Game1.staminaRect, new Rectangle(barX, objY + 4, barWidth, barHeight), Color.DarkGray * 0.4f);

                            // Progress bar fill
                            float progress = obj.MaxCount > 0 ? (float)obj.CurrentCount / obj.MaxCount : (obj.IsComplete ? 1f : 0f);
                            progress = Math.Clamp(progress, 0f, 1f);
                            Color barColor = obj.IsComplete ? Color.Green : Color.Gold;
                            b.Draw(Game1.staminaRect, new Rectangle(barX, objY + 4, (int)(barWidth * progress), barHeight), barColor * 0.7f);

                            // Objective text
                            string objText = obj.Description;
                            if (obj.MaxCount > 0)
                                objText += $" ({obj.CurrentCount}/{obj.MaxCount})";
                            b.DrawString(Game1.smallFont, objText,
                                new Vector2(barX + barWidth + 12, objY), obj.IsComplete ? Color.Green : Game1.textColor);

                            objY += 28;
                        }
                    }
                    y += orderHeight + 8;
                }
            }

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);
        }

        private class QuestDisplayInfo
        {
            public string Title;
            public string Description;
            public int DaysLeft;
        }

        private class SpecialOrderDisplayInfo
        {
            public string Title;
            public string Requester;
            public int DaysLeft;
            public List<ObjectiveInfo> Objectives = new();
        }

        private class ObjectiveInfo
        {
            public string Description;
            public int CurrentCount;
            public int MaxCount;
            public bool IsComplete;
        }
    }
}
