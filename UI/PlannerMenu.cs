using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StarDo.Services;
using StarDo.UI.Components;
using StarDo.UI.Pages;

namespace StarDo.UI
{
    public class PlannerMenu : IClickableMenu
    {
        private readonly TaskManager taskManager;
        private readonly ModConfig config;
        private TabBarComponent tabBar;

        private TaskListPage taskListPage;
        private QuestPage questPage;
        private TemplatePage templatePage;
        private TaskDetailPage taskDetailPage;

        private bool canClose;
        private bool showingDetail;

        private static readonly string[] TabNames = { "Tasks", "Quests", "Templates" };

        public PlannerMenu(TaskManager taskManager, ModConfig config)
            : base(0, 0, 0, 0, true)
        {
            this.taskManager = taskManager;
            this.config = config;
            this.RebuildLayout();
        }

        public void OpenTaskDetail(Models.PlannerTask task, bool isNew)
        {
            this.taskDetailPage = new TaskDetailPage(task, isNew, this.taskManager, this);
            this.showingDetail = true;
        }

        public void CloseTaskDetail()
        {
            if (this.taskDetailPage != null)
            {
                this.taskDetailPage.Cleanup();
                this.taskDetailPage = null;
            }
            this.showingDetail = false;
            this.taskListPage.Refresh();
        }

        private void RebuildLayout()
        {
            int menuWidth = (int)(Game1.uiViewport.Width * 0.8f);
            int menuHeight = (int)(Game1.uiViewport.Height * 0.8f);
            menuWidth = Math.Clamp(menuWidth, 800, 1600);
            menuHeight = Math.Clamp(menuHeight, 600, 1000);

            this.width = menuWidth;
            this.height = menuHeight;
            var center = Utility.getTopLeftPositionForCenteringOnScreen(this.width, this.height);
            this.xPositionOnScreen = (int)center.X;
            this.yPositionOnScreen = (int)center.Y;

            this.upperRightCloseButton = new ClickableTextureComponent(
                new Rectangle(this.xPositionOnScreen + this.width - 48, this.yPositionOnScreen - 8, 48, 48),
                Game1.mouseCursors,
                new Rectangle(337, 494, 12, 12),
                4f
            );

            int lineH = (int)Game1.smallFont.MeasureString("Tg").Y;
            int tabBarH = lineH + 24;

            this.tabBar = new TabBarComponent(TabNames,
                this.xPositionOnScreen + IClickableMenu.borderWidth,
                this.yPositionOnScreen - tabBarH + 4);

            var content = this.GetContentArea();
            this.taskListPage = new TaskListPage(this.taskManager, content, this);
            this.questPage = new QuestPage(content);
            this.templatePage = new TemplatePage(this.taskManager, content);
        }

        private Rectangle GetContentArea()
        {
            int borderW = IClickableMenu.borderWidth;
            int topSpace = IClickableMenu.spaceToClearTopBorder;
            return new Rectangle(
                this.xPositionOnScreen + borderW + 8,
                this.yPositionOnScreen + borderW + topSpace,
                this.width - borderW * 2 - 16,
                this.height - borderW * 2 - topSpace - 8
            );
        }

        private string GetDateString()
        {
            if (!Context.IsWorldReady)
                return "";
            var date = Game1.Date;
            return $"Day {date.DayOfMonth}, {date.Season}, Year {date.Year}";
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (this.showingDetail && this.taskDetailPage != null)
            {
                this.taskDetailPage.ReceiveLeftClick(x, y);
                return;
            }

            base.receiveLeftClick(x, y, playSound);
            if (Game1.activeClickableMenu == null)
                return;

            int tabClicked = this.tabBar.HandleClick(x, y);
            if (tabClicked >= 0)
            {
                this.OnTabChanged(tabClicked);
                return;
            }

            switch (this.tabBar.ActiveTab)
            {
                case 0:
                    this.taskListPage.ReceiveLeftClick(x, y);
                    break;
                case 1:
                    this.questPage.ReceiveLeftClick(x, y);
                    break;
                case 2:
                    this.templatePage.ReceiveLeftClick(x, y);
                    break;
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            if (this.showingDetail)
                return;

            if (this.tabBar.ActiveTab == 0)
                this.taskListPage.LeftClickHeld(x, y);
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (this.showingDetail)
                return;

            if (this.tabBar.ActiveTab == 0)
                this.taskListPage.ReleaseLeftClick(x, y);
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (this.showingDetail && this.taskDetailPage != null)
            {
                this.taskDetailPage.ReceiveScrollWheel(direction);
                return;
            }

            switch (this.tabBar.ActiveTab)
            {
                case 0: this.taskListPage.ReceiveScrollWheel(direction); break;
                case 1: this.questPage.ReceiveScrollWheel(direction); break;
                case 2: this.templatePage.ReceiveScrollWheel(direction); break;
            }
        }

        public override void receiveKeyPress(Keys key)
        {
            if (this.showingDetail && this.taskDetailPage != null)
            {
                this.taskDetailPage.ReceiveKeyPress(key);
                return;
            }

            if (this.tabBar.ActiveTab == 0 && this.taskListPage.IsTextInputActive())
            {
                this.taskListPage.ReceiveKeyPress(key);
                return;
            }

            SButton sButton = key.ToSButton();

            // On the first keypress after opening, only block the key that opened the menu
            // (prevents F2 from immediately re-closing). Escape always works on first press.
            if (!this.canClose)
            {
                this.canClose = true;
                if (sButton == this.config.OpenListKey)
                    return;
            }

            if ((sButton == SButton.Escape || sButton == this.config.OpenListKey) && this.readyToClose())
            {
                Game1.playSound("bigDeSelect");
                this.exitThisMenu();
                return;
            }

            if (this.tabBar.ActiveTab == 0)
                this.taskListPage.ReceiveKeyPress(key);
        }

        public override void receiveGamePadButton(Buttons key)
        {
            if (this.showingDetail)
                return;

            if (key == Buttons.LeftShoulder)
            {
                int newTab = Math.Max(0, this.tabBar.ActiveTab - 1);
                if (newTab != this.tabBar.ActiveTab)
                {
                    this.tabBar.SetActiveTab(newTab);
                    this.OnTabChanged(newTab);
                    Game1.playSound("shwip");
                }
            }
            else if (key == Buttons.RightShoulder)
            {
                int newTab = Math.Min(TabNames.Length - 1, this.tabBar.ActiveTab + 1);
                if (newTab != this.tabBar.ActiveTab)
                {
                    this.tabBar.SetActiveTab(newTab);
                    this.OnTabChanged(newTab);
                    Game1.playSound("shwip");
                }
            }

            base.receiveGamePadButton(key);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            if (!this.showingDetail && this.tabBar.ActiveTab == 0)
                this.taskListPage.PerformHoverAction(x, y);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            if (this.showingDetail)
                this.CloseTaskDetail();
            this.RebuildLayout();
        }

        public override void draw(SpriteBatch b)
        {
            // Dim background
            b.Draw(Game1.fadeToBlackRect,
                new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
                Color.Black * 0.6f);

            // Main dialog box
            Game1.drawDialogueBox(
                this.xPositionOnScreen, this.yPositionOnScreen,
                this.width, this.height, false, true);

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);

            // Date in header
            string dateStr = this.GetDateString();
            if (!string.IsNullOrEmpty(dateStr))
            {
                var dateSize = Game1.smallFont.MeasureString(dateStr);
                Utility.drawTextWithShadow(b, dateStr, Game1.smallFont,
                    new Vector2(this.xPositionOnScreen + this.width - dateSize.X - 80,
                                this.yPositionOnScreen - dateSize.Y - 8),
                    Color.Gold, 1f, -1f, -1, -1, 1f, 3);
            }

            // Tab bar
            this.tabBar.Draw(b, this.xPositionOnScreen, this.yPositionOnScreen);

            // Active page
            switch (this.tabBar.ActiveTab)
            {
                case 0: this.taskListPage.Draw(b); break;
                case 1: this.questPage.Draw(b); break;
                case 2: this.templatePage.Draw(b); break;
            }

            // Detail overlay
            if (this.showingDetail && this.taskDetailPage != null)
                this.taskDetailPage.Draw(b);

            // Close button + cursor
            base.draw(b);
            Game1.mouseCursorTransparency = 1f;
            this.drawMouse(b);
        }

        public override void receiveRightClick(int x, int y, bool playSound = true) { }

        protected override void cleanupBeforeExit()
        {
            this.taskListPage?.Cleanup();
            this.taskDetailPage?.Cleanup();
            base.cleanupBeforeExit();
        }

        private void OnTabChanged(int newTab)
        {
            this.taskListPage?.Cleanup();
            if (newTab == 0) this.taskListPage.Refresh();
            if (newTab == 1) this.questPage.Refresh();
            if (newTab == 2) this.templatePage.Refresh();
        }
    }
}
