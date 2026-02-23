using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly Rectangle contentArea;
        private readonly ScrollableListComponent scrollableList;

        private readonly List<QuestDisplayInfo> quests = new();
        private readonly List<SpecialOrderDisplayInfo> specialOrders = new();
        private readonly List<CompletedEntry> completedArchive = new();

        private readonly int lineH;
        private readonly int headerH;
        private readonly int sectionHeaderH;
        private readonly int cardPad;
        private readonly int rowGap;
        private readonly int objLineH;

        public QuestPage(Rectangle contentArea)
        {
            this.contentArea = contentArea;
            this.lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            this.headerH = (int)Game1.dialogueFont.MeasureString("Tg").Y + 12;
            this.sectionHeaderH = this.lineH + 20;
            this.cardPad = 20;
            this.rowGap = 10;
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
            this.completedArchive.Clear();

            if (!Context.IsWorldReady)
                return;

            foreach (var quest in Game1.player.questLog)
            {
                if (quest == null)
                    continue;

                var info = new QuestDisplayInfo
                {
                    Title = quest.questTitle ?? "Unknown Quest",
                    FlavorTitle = this.GetFlavorHeading(quest.questTitle ?? "Unknown Quest", "quest"),
                    Description = quest.questDescription ?? "",
                    DaysLeft = quest.daysLeft.Value > 0 ? quest.daysLeft.Value : -1,
                    RewardPreview = this.GetQuestRewardPreview(quest.moneyReward.Value),
                    XpPreview = this.GetQuestXpPreview(quest.moneyReward.Value, quest.questDescription ?? "")
                };

                if (quest.completed.Value)
                {
                    this.completedArchive.Add(new CompletedEntry
                    {
                        Title = info.Title,
                        FlavorTitle = info.FlavorTitle,
                        RewardPreview = info.RewardPreview,
                        Source = "Quest Board"
                    });
                }
                else
                {
                    this.quests.Add(info);
                }
            }

            foreach (var order in Game1.player.team.specialOrders)
            {
                if (order == null)
                    continue;

                var info = new SpecialOrderDisplayInfo
                {
                    Title = order.GetName(),
                    FlavorTitle = this.GetFlavorHeading(order.GetName(), "special"),
                    Requester = order.requester.Value ?? "",
                    DaysLeft = order.GetDaysLeft()
                };

                if (order.objectives != null)
                {
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
                }

                info.RewardPreview = this.GetSpecialOrderRewardPreview(info);
                info.XpPreview = this.GetSpecialOrderXpPreview(info);

                bool isComplete = info.Objectives.Count > 0 && info.Objectives.All(p => p.IsComplete);
                if (isComplete)
                {
                    this.completedArchive.Add(new CompletedEntry
                    {
                        Title = info.Title,
                        FlavorTitle = info.FlavorTitle,
                        RewardPreview = info.RewardPreview,
                        Source = "Special Order"
                    });
                }
                else
                {
                    this.specialOrders.Add(info);
                }
            }

            var trimmedArchive = this.completedArchive
                .OrderByDescending(c => c.Source)
                .ThenBy(c => c.Title)
                .Take(15)
                .ToList();
            this.completedArchive.Clear();
            this.completedArchive.AddRange(trimmedArchive);

            int h = this.headerH + 8;
            h += this.sectionHeaderH + this.GetSectionHeightForQuests();
            h += this.sectionHeaderH + this.GetSectionHeightForSpecialOrders();
            h += this.sectionHeaderH + this.GetSectionHeightForArchive();
            this.scrollableList.SetContentHeight(Math.Max(h, this.contentArea.Height));
        }

        public void ReceiveLeftClick(int x, int y) => this.scrollableList.HandleLeftClick(x, y);
        public void ReceiveScrollWheel(int direction) => this.scrollableList.HandleScrollWheel(direction);

        public void Draw(SpriteBatch b)
        {
            this.scrollableList.BeginScissorRect(b);

            var bounds = this.scrollableList.Bounds;
            int y = bounds.Y - this.scrollableList.ScrollOffset;
            int cardW = bounds.Width - 40;

            this.DrawPageTitle(b, bounds.X + 8, y);
            y += this.headerH + 8;

            y = this.DrawSectionHeader(b, "Active Journal", bounds, y, new Color(152, 113, 66));
            y = this.DrawQuestSection(b, bounds, y, cardW);

            y = this.DrawSectionHeader(b, "Guild Contracts", bounds, y, new Color(120, 90, 55));
            y = this.DrawSpecialOrderSection(b, bounds, y, cardW);

            y = this.DrawSectionHeader(b, "Completed Archive", bounds, y, new Color(110, 110, 110));
            this.DrawArchiveSection(b, bounds, y, cardW);

            this.scrollableList.EndScissorRect(b);
            this.scrollableList.DrawScrollbar(b);
        }

        private void DrawPageTitle(SpriteBatch b, int x, int y)
        {
            string season = SeasonHelper.GetDisplayName(SeasonHelper.GetCurrentSeason());
            Utility.drawTextWithShadow(b, "Adventurer's Journal", Game1.dialogueFont,
                new Vector2(x, y), Color.Gold, 1f, -1f, -1, -1, 1f, 3);

            string subtitle = $"Season: {season}";
            b.DrawString(Game1.smallFont, subtitle, new Vector2(x + 6, y + this.lineH + 8), Color.SaddleBrown * 0.9f);
        }

        private int DrawSectionHeader(SpriteBatch b, string text, Rectangle bounds, int y, Color accent)
        {
            if (y + this.sectionHeaderH > bounds.Y && y < bounds.Bottom)
            {
                b.Draw(Game1.staminaRect,
                    new Rectangle(bounds.X + 8, y + this.sectionHeaderH - 6, bounds.Width - 56, 2),
                    accent * 0.45f);

                Utility.drawTextWithShadow(b, text, Game1.smallFont,
                    new Vector2(bounds.X + 12, y + 8), accent, 1f, -1f, -1, -1, 1f, 3);
            }
            return y + this.sectionHeaderH;
        }

        private int DrawQuestSection(SpriteBatch b, Rectangle bounds, int y, int cardW)
        {
            if (this.quests.Count == 0)
            {
                this.DrawEmptyCard(b, bounds.X + 4, y, cardW, "No active quests right now.");
                return y + this.GetQuestCardHeight() + this.rowGap;
            }

            foreach (var quest in this.quests)
            {
                int cardH = this.GetQuestCardHeight();
                if (y + cardH > bounds.Y && y < bounds.Bottom)
                    this.DrawQuestCard(b, quest, bounds.X + 4, y, cardW, cardH);
                y += cardH + this.rowGap;
            }
            return y;
        }

        private int DrawSpecialOrderSection(SpriteBatch b, Rectangle bounds, int y, int cardW)
        {
            if (this.specialOrders.Count == 0)
            {
                this.DrawEmptyCard(b, bounds.X + 4, y, cardW, "No active special orders.");
                return y + this.GetSpecialOrderCardHeight(new SpecialOrderDisplayInfo()) + this.rowGap;
            }

            foreach (var order in this.specialOrders)
            {
                int cardH = this.GetSpecialOrderCardHeight(order);
                if (y + cardH > bounds.Y && y < bounds.Bottom)
                    this.DrawSpecialOrderCard(b, order, bounds.X + 4, y, cardW, cardH);
                y += cardH + this.rowGap;
            }
            return y;
        }

        private int DrawArchiveSection(SpriteBatch b, Rectangle bounds, int y, int cardW)
        {
            int rowH = this.lineH + 10;
            int cardH = Math.Max(this.lineH * 2 + 22, this.completedArchive.Count * rowH + this.cardPad * 2);

            if (y + cardH > bounds.Y && y < bounds.Bottom)
            {
                this.DrawParchmentCard(b, bounds.X + 4, y, cardW, cardH, new Color(230, 220, 200));

                int textY = y + this.cardPad;
                if (this.completedArchive.Count == 0)
                {
                    b.DrawString(Game1.smallFont, "No completed quests archived yet.",
                        new Vector2(bounds.X + 24, textY), Color.Gray);
                }
                else
                {
                    foreach (var entry in this.completedArchive)
                    {
                        string row = $"• {entry.FlavorTitle}  [{entry.Source}]";
                        b.DrawString(Game1.smallFont, this.TrimToWidth(row, cardW - 48),
                            new Vector2(bounds.X + 24, textY), Color.DimGray);
                        textY += rowH;
                    }
                }
            }
            return y + cardH + this.rowGap;
        }

        private void DrawQuestCard(SpriteBatch b, QuestDisplayInfo quest, int x, int y, int w, int h)
        {
            this.DrawParchmentCard(b, x, y, w, h, new Color(244, 232, 206));

            int textX = x + this.cardPad;
            int textY = y + this.cardPad;
            int infoX = x + w - this.cardPad - 210;

            Utility.drawTextWithShadow(b, this.TrimToWidth(quest.FlavorTitle, w - this.cardPad * 2),
                Game1.smallFont, new Vector2(textX, textY), Color.SaddleBrown, 1f, -1f, -1, -1, 1f, 3);
            textY += this.lineH + 6;

            b.DrawString(Game1.smallFont, this.TrimToWidth(quest.Title, w - this.cardPad * 2),
                new Vector2(textX, textY), Color.DimGray);
            textY += this.lineH + 6;

            b.DrawString(Game1.smallFont, this.TrimToWidth(quest.Description, w - this.cardPad * 2),
                new Vector2(textX, textY), new Color(90, 70, 50));
            textY += this.lineH + 6;

            b.DrawString(Game1.smallFont, $"XP Preview: +{quest.XpPreview}",
                new Vector2(textX, textY), new Color(60, 120, 60));
            b.DrawString(Game1.smallFont, $"Reward: {quest.RewardPreview}",
                new Vector2(infoX, textY), new Color(120, 90, 40));

            if (quest.DaysLeft > 0)
            {
                string daysText = $"{quest.DaysLeft} day{(quest.DaysLeft == 1 ? "" : "s")} left";
                var daysSize = Game1.smallFont.MeasureString(daysText);
                Color daysColor = quest.DaysLeft <= 3 ? Color.Red : Color.DarkGoldenrod;
                b.DrawString(Game1.smallFont, daysText,
                    new Vector2(x + w - daysSize.X - this.cardPad, y + this.cardPad), daysColor);
            }
        }

        private void DrawSpecialOrderCard(SpriteBatch b, SpecialOrderDisplayInfo order, int x, int y, int w, int h)
        {
            this.DrawParchmentCard(b, x, y, w, h, new Color(235, 226, 204));

            int textX = x + this.cardPad;
            int textY = y + this.cardPad;

            Utility.drawTextWithShadow(b, this.TrimToWidth(order.FlavorTitle, w - this.cardPad * 2),
                Game1.smallFont, new Vector2(textX, textY), new Color(90, 70, 45), 1f, -1f, -1, -1, 1f, 3);
            textY += this.lineH + 4;

            b.DrawString(Game1.smallFont, this.TrimToWidth(order.Title, w - this.cardPad * 2),
                new Vector2(textX, textY), Color.DimGray);
            textY += this.lineH + 4;

            string contractLine = string.IsNullOrEmpty(order.Requester)
                ? "Posted at the board"
                : $"Contract: {order.Requester}";
            b.DrawString(Game1.smallFont, this.TrimToWidth(contractLine, w - this.cardPad * 2),
                new Vector2(textX, textY), new Color(100, 80, 60));
            textY += this.lineH + 4;

            b.DrawString(Game1.smallFont, $"XP Preview: +{order.XpPreview}",
                new Vector2(textX, textY), new Color(60, 120, 60));
            b.DrawString(Game1.smallFont, $"Reward: {order.RewardPreview}",
                new Vector2(textX + 220, textY), new Color(120, 90, 40));
            textY += this.lineH + 8;

            foreach (var obj in order.Objectives)
            {
                int barW = Math.Min(220, w / 3);
                int barH = this.lineH - 4;
                int barX = textX;

                b.Draw(Game1.staminaRect, new Rectangle(barX, textY + 2, barW, barH), Color.Black * 0.2f);
                float progress = obj.MaxCount > 0
                    ? Math.Clamp((float)obj.CurrentCount / obj.MaxCount, 0, 1)
                    : (obj.IsComplete ? 1 : 0);
                Color barColor = obj.IsComplete ? Color.Green : Color.Goldenrod;
                b.Draw(Game1.staminaRect, new Rectangle(barX, textY + 2, (int)(barW * progress), barH), barColor * 0.8f);

                string objText = obj.Description;
                if (obj.MaxCount > 0)
                    objText += $" ({obj.CurrentCount}/{obj.MaxCount})";
                b.DrawString(Game1.smallFont, this.TrimToWidth(objText, w - this.cardPad * 2 - barW - 16),
                    new Vector2(barX + barW + 12, textY),
                    obj.IsComplete ? Color.ForestGreen : Game1.textColor);

                textY += this.objLineH;
            }

            if (order.DaysLeft > 0)
            {
                string days = $"{order.DaysLeft} day{(order.DaysLeft == 1 ? "" : "s")} left";
                var size = Game1.smallFont.MeasureString(days);
                Color daysColor = order.DaysLeft <= 3 ? Color.Red : Color.DarkGoldenrod;
                b.DrawString(Game1.smallFont, days, new Vector2(x + w - size.X - this.cardPad, y + this.cardPad), daysColor);
            }
        }

        private void DrawParchmentCard(SpriteBatch b, int x, int y, int w, int h, Color tint)
        {
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15), x, y, w, h, tint, 4f, false);

            b.Draw(Game1.staminaRect, new Rectangle(x + 6, y + 6, w - 12, 2), new Color(140, 110, 75) * 0.25f);
            b.Draw(Game1.staminaRect, new Rectangle(x + 6, y + h - 8, w - 12, 2), new Color(140, 110, 75) * 0.25f);
        }

        private void DrawEmptyCard(SpriteBatch b, int x, int y, int w, string text)
        {
            int h = this.GetQuestCardHeight();
            this.DrawParchmentCard(b, x, y, w, h, new Color(238, 230, 210));
            b.DrawString(Game1.smallFont, text, new Vector2(x + this.cardPad, y + this.cardPad), Color.Gray);
        }

        private int GetSectionHeightForQuests()
        {
            if (this.quests.Count == 0)
                return this.GetQuestCardHeight() + this.rowGap;
            return this.quests.Count * (this.GetQuestCardHeight() + this.rowGap);
        }

        private int GetSectionHeightForSpecialOrders()
        {
            if (this.specialOrders.Count == 0)
                return this.GetSpecialOrderCardHeight(new SpecialOrderDisplayInfo()) + this.rowGap;

            int total = 0;
            foreach (var order in this.specialOrders)
                total += this.GetSpecialOrderCardHeight(order) + this.rowGap;
            return total;
        }

        private int GetSectionHeightForArchive()
        {
            int rowH = this.lineH + 10;
            int cardH = Math.Max(this.lineH * 2 + 22, this.completedArchive.Count * rowH + this.cardPad * 2);
            return cardH + this.rowGap;
        }

        private int GetQuestCardHeight()
        {
            return this.cardPad * 2 + this.lineH * 4 + 20;
        }

        private int GetSpecialOrderCardHeight(SpecialOrderDisplayInfo order)
        {
            int objectiveCount = Math.Max(order.Objectives.Count, 1);
            return this.cardPad * 2 + this.lineH * 4 + objectiveCount * this.objLineH + 10;
        }

        private string GetQuestRewardPreview(int moneyReward)
        {
            if (moneyReward > 0)
                return $"{moneyReward}g + goodwill";
            return "Item/friendship reward";
        }

        private int GetQuestXpPreview(int moneyReward, string description)
        {
            int descBonus = Math.Clamp(description?.Length ?? 0, 0, 120) / 4;
            int rewardBonus = Math.Clamp(moneyReward / 50, 0, 60);
            return 20 + descBonus + rewardBonus;
        }

        private string GetSpecialOrderRewardPreview(SpecialOrderDisplayInfo order)
        {
            int objectiveWeight = Math.Max(order.Objectives.Count, 1);
            return objectiveWeight >= 4 ? "Large payout + board reputation" : "Moderate payout + town favor";
        }

        private int GetSpecialOrderXpPreview(SpecialOrderDisplayInfo order)
        {
            int objectiveWeight = 0;
            foreach (var obj in order.Objectives)
                objectiveWeight += Math.Max(obj.MaxCount, 1);
            objectiveWeight = Math.Clamp(objectiveWeight, 10, 120);
            return 40 + objectiveWeight / 2;
        }

        private string GetFlavorHeading(string title, string type)
        {
            if (string.IsNullOrWhiteSpace(title))
                return type == "special" ? "A Notice from the Board" : "A Farmer's Duty";

            string lowered = title.ToLowerInvariant();
            if (lowered.Contains("fish")) return "The Call of the Waters";
            if (lowered.Contains("mine") || lowered.Contains("ore")) return "Depths and Determination";
            if (lowered.Contains("crop") || lowered.Contains("harvest")) return "Fields of Promise";
            if (lowered.Contains("gift") || lowered.Contains("friend")) return "Bonds of the Valley";
            if (lowered.Contains("monster") || lowered.Contains("slay")) return "Shadows in the Wild";

            string[] prefixes = { "A Farmer's Duty", "The Next Venture", "A Call to Action", "The Road Ahead" };
            int index = (title.GetHashCode() & int.MaxValue) % prefixes.Length;
            return prefixes[index];
        }

        private string TrimToWidth(string text, int maxPixelWidth)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "";

            string result = text;
            while (result.Length > 3 && Game1.smallFont.MeasureString(result).X > maxPixelWidth)
                result = result[..^1];
            if (result.Length < text.Length)
                result += "...";
            return result;
        }

        private class QuestDisplayInfo
        {
            public string Title;
            public string FlavorTitle;
            public string Description;
            public int DaysLeft;
            public string RewardPreview;
            public int XpPreview;
        }

        private class SpecialOrderDisplayInfo
        {
            public string Title;
            public string FlavorTitle;
            public string Requester;
            public int DaysLeft;
            public string RewardPreview;
            public int XpPreview;
            public List<ObjectiveInfo> Objectives = new();
        }

        private class CompletedEntry
        {
            public string Title;
            public string FlavorTitle;
            public string RewardPreview;
            public string Source;
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
