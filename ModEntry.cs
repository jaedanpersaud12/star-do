using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StarDo.Services;
using StarDo.UI.Components;
using StarDo.UI;

namespace StarDo
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private TaskManager TaskManager;
        private TaskHudOverlay hudOverlay;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>() ?? new ModConfig();

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
            helper.Events.GameLoop.ReturnedToTitle += this.OnReturnedToTitle;
            helper.Events.Display.RenderingHud += this.OnRenderingHud;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (e.Button == this.Config.OpenListKey)
            {
                this.OpenPlanner();
                return;
            }

            if (e.Button == SButton.MouseLeft && this.hudOverlay != null && this.hudOverlay.ContainsPoint(e.Cursor.ScreenPixels))
                this.OpenPlanner();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.TaskManager = new TaskManager(this.Helper, this.Monitor);
            this.hudOverlay = new TaskHudOverlay(this.TaskManager, this.Config);

            if (this.Config.OpenAtStartup)
                this.OpenPlanner();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.TaskManager?.ProcessDayStart(Game1.Date.TotalDays, Game1.Date.DayOfMonth, SeasonHelper.GetCurrentSeason());
        }

        private void OnReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            this.TaskManager = null;
            this.hudOverlay = null;
        }

        private void OnRenderingHud(object sender, RenderingHudEventArgs e)
        {
            if (this.TaskManager == null || this.hudOverlay == null)
                return;

            this.hudOverlay.Draw(e.SpriteBatch);
        }

        private void OpenPlanner()
        {
            if (this.TaskManager == null)
                return;

            if (Game1.activeClickableMenu != null)
                Game1.exitActiveMenu();

            Game1.activeClickableMenu = new PlannerMenu(this.TaskManager, this.Config);
        }
    }
}
