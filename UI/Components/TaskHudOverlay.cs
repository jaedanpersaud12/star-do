using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StarDo.Models;
using StarDo.Services;

namespace StarDo.UI.Components
{
    public class TaskHudOverlay
    {
        private readonly TaskManager taskManager;
        private readonly ModConfig config;

        private Rectangle lastDrawBounds;
        private bool wasDrawnThisFrame;

        private const int MaxVisibleItems = 5;
        private const int MarginX = 16;
        private const int MarginY = 16;
        private const int Padding = 12;

        public TaskHudOverlay(TaskManager taskManager, ModConfig config)
        {
            this.taskManager = taskManager;
            this.config = config;
        }

        public void Draw(SpriteBatch b)
        {
            this.wasDrawnThisFrame = false;
            this.lastDrawBounds = Rectangle.Empty;

            if (!this.ShouldRender())
                return;

            Season currentSeason = SeasonHelper.GetCurrentSeason();
            List<PlannerTask> tasks = this.taskManager
                .GetActiveTasks(seasonFilter: currentSeason)
                .Take(MaxVisibleItems + 1)
                .ToList();

            if (tasks.Count == 0)
                return;

            int extraCount = Math.Max(0, tasks.Count - MaxVisibleItems);
            if (tasks.Count > MaxVisibleItems)
                tasks = tasks.Take(MaxVisibleItems).ToList();

            var lines = new List<string>
            {
                $"Star-Do • {SeasonHelper.GetDisplayName(currentSeason)}"
            };

            foreach (var task in tasks)
                lines.Add($"☐ {this.Truncate(task.Title, 34)}");

            if (extraCount > 0)
                lines.Add($"+{extraCount} more");

            lines.Add($"[{this.config.OpenListKey}] Open planner");

            int lineH = (int)Game1.smallFont.MeasureString("Tg").Y + 2;
            int width = 0;
            foreach (string line in lines)
            {
                int lineWidth = (int)Game1.smallFont.MeasureString(line).X;
                if (lineWidth > width)
                    width = lineWidth;
            }
            width += Padding * 2;
            int height = lines.Count * lineH + Padding * 2;

            var bounds = new Rectangle(MarginX, MarginY, width, height);
            this.lastDrawBounds = bounds;
            this.wasDrawnThisFrame = true;

            IClickableMenu.drawTextureBox(
                b,
                Game1.mouseCursors,
                new Rectangle(384, 396, 15, 15),
                bounds.X,
                bounds.Y,
                bounds.Width,
                bounds.Height,
                new Color(245, 240, 225) * 0.9f,
                4f,
                false);

            int y = bounds.Y + Padding;
            for (int i = 0; i < lines.Count; i++)
            {
                Color color = i switch
                {
                    0 => SeasonHelper.GetColor(currentSeason) * 0.95f,
                    _ when i == lines.Count - 1 => Color.Gray,
                    _ => Game1.textColor
                };
                Utility.drawTextWithShadow(
                    b,
                    lines[i],
                    Game1.smallFont,
                    new Vector2(bounds.X + Padding, y),
                    color,
                    1f,
                    -1f,
                    -1,
                    -1,
                    1f,
                    3);
                y += lineH;
            }
        }

        public bool ContainsPoint(Vector2 point)
        {
            return this.wasDrawnThisFrame && this.lastDrawBounds.Contains((int)point.X, (int)point.Y);
        }

        private bool ShouldRender()
        {
            return this.config.ShowHudOverlay
                && Context.IsWorldReady
                && Context.IsPlayerFree
                && Game1.activeClickableMenu == null;
        }

        private string Truncate(string text, int maxLen)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLen)
                return text;
            return text[..(maxLen - 3)] + "...";
        }
    }
}
