using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace StarDo.UI.Components
{
    public class TabBarComponent
    {
        private readonly List<ClickableComponent> tabs = new();
        private readonly string[] tabNames;
        private int activeTab;

        public int ActiveTab => this.activeTab;

        public TabBarComponent(string[] tabNames, int x, int y)
        {
            this.tabNames = tabNames;

            int lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            int tabHeight = lineH + 24;
            int tabX = x;
            int gap = 8;

            for (int i = 0; i < tabNames.Length; i++)
            {
                int textW = (int)Game1.smallFont.MeasureString(tabNames[i]).X;
                int tabWidth = textW + 48;

                this.tabs.Add(new ClickableComponent(
                    new Rectangle(tabX, y, tabWidth, tabHeight),
                    i.ToString()
                )
                {
                    myID = 9000 + i,
                    rightNeighborID = i < tabNames.Length - 1 ? 9001 + i : -1,
                    leftNeighborID = i > 0 ? 8999 + i : -1,
                    downNeighborID = -7777
                });
                tabX += tabWidth + gap;
            }
        }

        public void SetActiveTab(int index)
        {
            this.activeTab = Math.Clamp(index, 0, this.tabNames.Length - 1);
        }

        public int HandleClick(int x, int y)
        {
            for (int i = 0; i < this.tabs.Count; i++)
            {
                // Extend hitbox up for the active tab offset
                var hitbox = this.tabs[i].bounds;
                hitbox.Y -= 12;
                hitbox.Height += 12;
                if (hitbox.Contains(x, y) && i != this.activeTab)
                {
                    this.activeTab = i;
                    Game1.playSound("shwip");
                    return i;
                }
            }
            return -1;
        }

        public void Draw(SpriteBatch b, int menuX, int menuY)
        {
            for (int i = 0; i < this.tabs.Count; i++)
            {
                var tab = this.tabs[i];
                bool active = i == this.activeTab;
                int yOffset = active ? -12 : 0;

                IClickableMenu.drawTextureBox(
                    b, Game1.mouseCursors,
                    new Rectangle(384, 396, 15, 15),
                    tab.bounds.X, tab.bounds.Y + yOffset,
                    tab.bounds.Width, tab.bounds.Height + (active ? 12 : 0),
                    active ? Color.White : new Color(200, 200, 200),
                    4f, false
                );

                var textSize = Game1.smallFont.MeasureString(this.tabNames[i]);
                Utility.drawTextWithShadow(
                    b, this.tabNames[i], Game1.smallFont,
                    new Vector2(
                        tab.bounds.X + (tab.bounds.Width - textSize.X) / 2,
                        tab.bounds.Y + yOffset + (tab.bounds.Height - textSize.Y) / 2
                    ),
                    active ? Game1.textColor : Color.DimGray,
                    1f, -1f, -1, -1, 1f, 3
                );
            }
        }

        public List<ClickableComponent> GetClickableComponents() => this.tabs;
    }
}
