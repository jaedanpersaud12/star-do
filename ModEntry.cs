using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StarDo.Services;
using StarDo.UI;

namespace StarDo
{
    public class ModEntry : Mod
    {
        private ModConfig Config;
        private TaskManager TaskManager;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>() ?? new ModConfig();

            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady || !Context.IsPlayerFree)
                return;

            if (e.Button == this.Config.OpenListKey)
                this.OpenPlanner();
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            this.TaskManager = new TaskManager(this.Helper, this.Monitor);

            if (this.Config.OpenAtStartup)
                this.OpenPlanner();
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            this.TaskManager?.ProcessDayStart(Game1.Date.TotalDays);
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
