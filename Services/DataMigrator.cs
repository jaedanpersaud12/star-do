using System;
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

            try
            {
                // Try reading raw JSON to detect format
                var raw = helper.Data.ReadJsonFile<JObject>(path);
                if (raw == null)
                    return new PlannerData();

                // New format has DataVersion field
                if (raw.ContainsKey("DataVersion"))
                {
                    var data = helper.Data.ReadJsonFile<PlannerData>(path);
                    if (data == null)
                    {
                        monitor.Log("Failed to deserialize planner data, starting fresh.", LogLevel.Error);
                        return new PlannerData();
                    }
                    return data;
                }

                // Old format: { "SavedTasks": ["task1", "task2", ...] }
                monitor.Log("Migrating old task data to new planner format...", LogLevel.Info);

                var migrated = new PlannerData();
                var savedTasks = raw["SavedTasks"] as JArray;
                if (savedTasks != null)
                {
                    int sortOrder = 0;
                    foreach (var token in savedTasks)
                    {
                        string title = token?.ToString();
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            migrated.Tasks.Add(new PlannerTask
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
                else
                {
                    monitor.Log("Old save data found but no 'SavedTasks' field detected. Starting fresh.", LogLevel.Warn);
                }

                // Save in new format immediately
                helper.Data.WriteJsonFile(path, migrated);
                monitor.Log($"Migration complete. Converted {migrated.Tasks.Count} tasks.", LogLevel.Info);

                return migrated;
            }
            catch (Exception ex)
            {
                monitor.Log($"Error loading planner data: {ex.Message}", LogLevel.Error);
                monitor.Log("Starting with empty task list to avoid data loss. Check the file manually.", LogLevel.Warn);
                return new PlannerData();
            }
        }
    }
}
