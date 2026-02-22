using System;
using System.Collections.Generic;
using System.Linq;
using StarDo.Models;

namespace StarDo.Services
{
    public class TemplateDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TaskCategory Category { get; set; }
        public List<string> SubTasks { get; set; } = new();
        public string Group { get; set; } = "Daily Routines";
        public string[] Seasons { get; set; } // null = all seasons
        public TaskPriority Priority { get; set; } = TaskPriority.Daily;

        public bool IsRelevantForSeason(string season)
        {
            if (this.Seasons == null || this.Seasons.Length == 0)
                return true;
            return this.Seasons.Any(s => s.Equals(season, StringComparison.OrdinalIgnoreCase));
        }
    }

    public static class TemplateProvider
    {
        public static readonly string[] GroupOrder = { "Daily Routines", "Seasonal", "Long-term Goals" };

        public static readonly List<TemplateDefinition> Templates = new()
        {
            // ── Daily Routines ──────────────────────────────────────

            new TemplateDefinition
            {
                Id = "animal_chores",
                Name = "Animal Chores",
                Description = "Pet, feed, and collect from all animals",
                Category = TaskCategory.Farm,
                Group = "Daily Routines",
                SubTasks = new()
                {
                    "Pet each animal",
                    "Collect eggs from coop",
                    "Collect milk from barn",
                    "Collect wool / feathers",
                    "Check for truffles (pigs)",
                    "Refill hay if needed"
                }
            },
            new TemplateDefinition
            {
                Id = "processing_check",
                Name = "Processing Machines",
                Description = "Collect finished goods and reload machines",
                Category = TaskCategory.Processing,
                Group = "Daily Routines",
                SubTasks = new()
                {
                    "Collect from kegs",
                    "Reload kegs",
                    "Collect from preserve jars",
                    "Reload preserve jars",
                    "Collect from looms",
                    "Check oil makers",
                    "Check cheese press",
                    "Check mayonnaise machines"
                }
            },
            new TemplateDefinition
            {
                Id = "tapper_collection",
                Name = "Tapper Rounds",
                Description = "Check and collect from all tree tappers",
                Category = TaskCategory.Farm,
                Group = "Daily Routines",
                SubTasks = new()
                {
                    "Oak tappers (oak resin)",
                    "Maple tappers (maple syrup)",
                    "Pine tappers (pine tar)",
                    "Mushroom tree tappers"
                }
            },
            new TemplateDefinition
            {
                Id = "greenhouse_daily",
                Name = "Greenhouse Check",
                Description = "Maintain your greenhouse crops daily",
                Category = TaskCategory.Farm,
                Group = "Daily Routines",
                SubTasks = new()
                {
                    "Harvest ready crops",
                    "Replant empty spots",
                    "Check fruit trees"
                }
            },
            new TemplateDefinition
            {
                Id = "fishing_ponds",
                Name = "Fish Pond Rounds",
                Description = "Check fish ponds for produce and quests",
                Category = TaskCategory.Farm,
                Group = "Daily Routines",
                SubTasks = new()
                {
                    "Collect pond produce",
                    "Check for fish quests (!)",
                    "Throw in requested items"
                }
            },
            new TemplateDefinition
            {
                Id = "cave_check",
                Name = "Farm Cave",
                Description = "Collect from mushroom cave or fruit bat cave",
                Category = TaskCategory.Farm,
                Group = "Daily Routines",
                SubTasks = new()
                {
                    "Collect items from cave",
                    "Check all 6 spawn spots"
                }
            },

            // ── Seasonal ────────────────────────────────────────────

            new TemplateDefinition
            {
                Id = "spring_crops",
                Name = "Spring Crops",
                Description = "Key crops to plant and track in spring",
                Category = TaskCategory.Farm,
                Group = "Seasonal",
                Priority = TaskPriority.Monthly,
                Seasons = new[] { "spring" },
                SubTasks = new()
                {
                    "Plant parsnips (4 days)",
                    "Plant potatoes (6 days)",
                    "Plant cauliflower (12 days)",
                    "Plant strawberries (8 days)",
                    "Plant kale (6 days)",
                    "Water / check sprinklers"
                }
            },
            new TemplateDefinition
            {
                Id = "summer_crops",
                Name = "Summer Crops",
                Description = "Key crops to plant and track in summer",
                Category = TaskCategory.Farm,
                Group = "Seasonal",
                Priority = TaskPriority.Monthly,
                Seasons = new[] { "summer" },
                SubTasks = new()
                {
                    "Plant melons (12 days)",
                    "Plant blueberries (13 days)",
                    "Plant starfruit (13 days)",
                    "Plant red cabbage (9 days)",
                    "Plant hops (11 days)",
                    "Water / check sprinklers"
                }
            },
            new TemplateDefinition
            {
                Id = "fall_crops",
                Name = "Fall Crops",
                Description = "Key crops to plant and track in fall",
                Category = TaskCategory.Farm,
                Group = "Seasonal",
                Priority = TaskPriority.Monthly,
                Seasons = new[] { "fall" },
                SubTasks = new()
                {
                    "Plant cranberries (7 days)",
                    "Plant pumpkins (13 days)",
                    "Plant amaranth (7 days)",
                    "Plant grapes (10 days)",
                    "Plant sweet gem berry (24 days)",
                    "Water / check sprinklers"
                }
            },
            new TemplateDefinition
            {
                Id = "winter_tasks",
                Name = "Winter Activities",
                Description = "Things to do when crops can't grow outside",
                Category = TaskCategory.Farm,
                Group = "Seasonal",
                Priority = TaskPriority.Monthly,
                Seasons = new[] { "winter" },
                SubTasks = new()
                {
                    "Maintain greenhouse crops",
                    "Go mining / Skull Cavern",
                    "Upgrade tools at Clint",
                    "Reorganize chests",
                    "Fish for winter-only fish",
                    "Forage winter seeds"
                }
            },
            new TemplateDefinition
            {
                Id = "foraging_seasonal",
                Name = "Seasonal Foraging",
                Description = "Forage items available this season",
                Category = TaskCategory.Farm,
                Group = "Seasonal",
                Priority = TaskPriority.Monthly,
                Seasons = new[] { "spring", "summer", "fall" },
                SubTasks = new()
                {
                    "Check forest / mountain areas",
                    "Check beach",
                    "Check bus stop / backwoods",
                    "Check railroad area",
                    "Craft wild seeds if needed"
                }
            },

            // ── Long-term Goals ─────────────────────────────────────

            new TemplateDefinition
            {
                Id = "friendship_weekly",
                Name = "Friendship Rounds",
                Description = "Give loved gifts and talk to villagers",
                Category = TaskCategory.Social,
                Group = "Long-term Goals",
                Priority = TaskPriority.Weekly,
                SubTasks = new()
                {
                    "Give 2 loved gifts per week to targets",
                    "Talk to all villagers",
                    "Check birthdays this week",
                    "Attend any festivals"
                }
            },
            new TemplateDefinition
            {
                Id = "perfection_tracker",
                Name = "Perfection Checklist",
                Description = "Track progress toward 100% perfection",
                Category = TaskCategory.Goals,
                Group = "Long-term Goals",
                Priority = TaskPriority.Monthly,
                SubTasks = new()
                {
                    "All fish caught",
                    "All cooking recipes made",
                    "All crafting recipes made",
                    "All minerals found",
                    "All artifacts found",
                    "Max hearts with all villagers",
                    "All stardrops collected",
                    "All obelisks + gold clock",
                    "All monster slayer goals"
                }
            },
            new TemplateDefinition
            {
                Id = "mining_run",
                Name = "Mining Checklist",
                Description = "Prep and goals for a mining day",
                Category = TaskCategory.Goals,
                Group = "Long-term Goals",
                Priority = TaskPriority.Weekly,
                SubTasks = new()
                {
                    "Bring food / healing items",
                    "Bring bombs + staircases",
                    "Check elevator progress",
                    "Collect ores + gems",
                    "Fight for monster loot"
                }
            },
            new TemplateDefinition
            {
                Id = "fishing_goals",
                Name = "Fishing Goals",
                Description = "Track fishing collection and progress",
                Category = TaskCategory.Goals,
                Group = "Long-term Goals",
                Priority = TaskPriority.Monthly,
                SubTasks = new()
                {
                    "Check season-specific fish",
                    "Check weather-specific fish",
                    "Check night-only fish",
                    "Visit Ginger Island for tropical fish",
                    "Use crab pots daily"
                }
            }
        };

        public static TemplateDefinition GetById(string id)
        {
            return Templates.Find(t => t.Id == id);
        }

        public static List<(string Group, List<TemplateDefinition> Items)> GetGrouped()
        {
            var grouped = new List<(string, List<TemplateDefinition>)>();
            foreach (var group in GroupOrder)
            {
                var items = Templates.Where(t => t.Group == group).ToList();
                if (items.Count > 0)
                    grouped.Add((group, items));
            }
            return grouped;
        }

        public static PlannerTask CreateTaskFromTemplate(TemplateDefinition template, int currentDay)
        {
            var task = new PlannerTask
            {
                Title = template.Name,
                Notes = template.Description,
                Category = template.Category,
                Priority = template.Priority,
                IsRecurring = true,
                TemplateId = template.Id,
                CreatedDay = currentDay
            };

            foreach (var subText in template.SubTasks)
            {
                task.SubTasks.Add(new SubTask { Text = subText });
            }

            return task;
        }
    }
}
