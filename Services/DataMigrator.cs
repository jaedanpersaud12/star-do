using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StarDo.Models;

namespace StarDo.Services
{
    public static class DataMigrator
    {
        public static PlannerData LoadOrMigrate(IModHelper helper, IMonitor monitor)
        {
            string path = $"data/{Constants.SaveFolderName}.json";

            // Try reading raw JSON to detect format
            var raw = helper.ReadJsonFile<JObject>(path);
            if (raw == null)
                return new PlannerData();

            // New format has DataVersion field
            if (raw.ContainsKey("DataVersion"))
                return helper.ReadJsonFile<PlannerData>(path) ?? new PlannerData();

            // Old format: { "SavedTasks": ["task1", "task2", ...] }
            monitor.Log("Migrating old task data to new planner format...", LogLevel.Info);

            var data = new PlannerData();
            var savedTasks = raw["SavedTasks"] as JArray;
            if (savedTasks != null)
            {
                int sortOrder = 0;
                foreach (var token in savedTasks)
                {
                    string title = token.ToString();
                    if (!string.IsNullOrWhiteSpace(title))
                    {
                        data.Tasks.Add(new PlannerTask
                        {
                            Title = title,
                            Category = TaskCategory.Farm,
                            Priority = TaskPriority.LongTerm,
                            IsRecurring = false,
                            SortOrder = sortOrder++
                        });
                    }
                }
            }

            // Save in new format immediately
            helper.WriteJsonFile(path, data);
            monitor.Log($"Migration complete. Converted {data.Tasks.Count} tasks.", LogLevel.Info);

            return data;
        }
    }
}
