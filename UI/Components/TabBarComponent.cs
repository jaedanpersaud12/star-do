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

        public TabBarComponent(string[] tabNames, int x, int y, int tabWidth = 100, int tabHeight = 48)
        {
            this.tabNames = tabNames;
            for (int i = 0; i < tabNames.Length; i++)
            {
                this.tabs.Add(new ClickableComponent(
                    new Rectangle(x + i * (tabWidth + 4), y, tabWidth, tabHeight),
                    i.ToString()
                )
                {
                    myID = 9000 + i,
                    rightNeighborID = i < tabNames.Length - 1 ? 9001 + i : -1,
                    leftNeighborID = i > 0 ? 8999 + i : -1,
                    downNeighborID = -7777
                });
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
                if (this.tabs[i].containsPoint(x, y) && i != this.activeTab)
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
                int yOffset = active ? -8 : 0;

                IClickableMenu.drawTextureBox(
                    b, Game1.mouseCursors,
                    new Rectangle(384, 396, 15, 15),
                    tab.bounds.X, tab.bounds.Y + yOffset,
                    tab.bounds.Width, tab.bounds.Height,
                    active ? Color.White : Color.LightGray,
                    4f, false
                );

                var textSize = Game1.smallFont.MeasureString(this.tabNames[i]);
                b.DrawString(
                    Game1.smallFont,
                    this.tabNames[i],
                    new Vector2(
                        tab.bounds.X + (tab.bounds.Width - textSize.X) / 2,
                        tab.bounds.Y + yOffset + (tab.bounds.Height - textSize.Y) / 2
                    ),
                    active ? Game1.textColor : Color.Gray
                );
            }
        }

        public List<ClickableComponent> GetClickableComponents() => this.tabs;
    }
}
