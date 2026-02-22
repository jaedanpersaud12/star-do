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
                var raw = helper.Data.ReadJsonFile<JObject>(path);
                if (raw == null)
                    return new PlannerData();

                if (raw.ContainsKey("DataVersion"))
                {
                    int version = raw["DataVersion"]?.Value<int>() ?? 0;

                    // v2 → v3: rename LongTerm → Monthly, init CompletionCount
                    if (version == 2)
                    {
                        monitor.Log("Migrating planner data v2 -> v3...", LogLevel.Info);
                        var tasks = raw["Tasks"] as JArray;
                        if (tasks != null)
                        {
                            foreach (JObject taskObj in tasks)
                            {
                                if (taskObj["Priority"]?.ToString() == "LongTerm")
                                    taskObj["Priority"] = "Monthly";

                                if (taskObj["CompletionCount"] == null)
                                    taskObj["CompletionCount"] = 0;
                            }
                        }
                        raw["DataVersion"] = 3;
                        helper.Data.WriteJsonFile(path, raw);
                        monitor.Log("Migration v2 -> v3 complete.", LogLevel.Info);
                    }

                    // Deserialize from the (possibly migrated) JObject
                    var data = raw.ToObject<PlannerData>();
                    if (data == null)
                    {
                        monitor.Log("Failed to deserialize planner data, starting fresh.", LogLevel.Error);
                        return new PlannerData();
                    }
                    return data;
                }

                // v1: old format { "SavedTasks": ["task1", "task2", ...] }
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
                                Priority = TaskPriority.Monthly,
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
