using System;
using System.Collections.Generic;
using System.Linq;
using StardewModdingAPI;
using StarDo.Models;

namespace StarDo.Services
{
    public class TaskManager
    {
        private readonly IModHelper helper;
        private readonly IMonitor monitor;
        private PlannerData data;

        public PlannerData Data => this.data;

        public TaskManager(IModHelper helper, IMonitor monitor)
        {
            this.helper = helper;
            this.monitor = monitor;
            this.data = DataMigrator.LoadOrMigrate(helper, monitor);
            this.ValidateLoadedData();
        }

        private void ValidateLoadedData()
        {
            foreach (var task in this.data.Tasks)
            {
                if (!Enum.IsDefined(typeof(TaskCategory), task.Category))
                {
                    this.monitor.Log($"Task '{task.Title}' has invalid category {(int)task.Category}, resetting to Farm.", LogLevel.Warn);
                    task.Category = TaskCategory.Farm;
                }
                if (!Enum.IsDefined(typeof(TaskPriority), task.Priority))
                {
                    this.monitor.Log($"Task '{task.Title}' has invalid priority {(int)task.Priority}, resetting to LongTerm.", LogLevel.Warn);
                    task.Priority = TaskPriority.LongTerm;
                }
            }
        }

        public void AddTask(PlannerTask task)
        {
            if (task.SortOrder == 0 && this.data.Tasks.Count > 0)
                task.SortOrder = this.data.Tasks.Max(t => t.SortOrder) + 1;

            this.data.Tasks.Add(task);
            this.Save();
        }

        public void UpdateTask(PlannerTask task)
        {
            int idx = this.data.Tasks.FindIndex(t => t.Id == task.Id);
            if (idx >= 0)
            {
                this.data.Tasks[idx] = task;
                this.Save();
            }
        }

        public void DeleteTask(string taskId)
        {
            this.data.Tasks.RemoveAll(t => t.Id == taskId);
            this.Save();
        }

        public void ToggleComplete(string taskId, int currentDay)
        {
            var task = this.data.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return;

            task.IsCompleted = !task.IsCompleted;
            if (task.IsCompleted)
                task.CompletedDay = currentDay;
            else
                task.CompletedDay = null;

            this.Save();
        }

        public List<PlannerTask> GetActiveTasks(TaskCategory? categoryFilter = null)
        {
            var query = this.data.Tasks
                .Where(t => !t.IsCompleted);

            if (categoryFilter.HasValue)
                query = query.Where(t => t.Category == categoryFilter.Value);

            return query
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.SortOrder)
                .ToList();
        }

        public List<PlannerTask> GetCompletedToday(int currentDay)
        {
            return this.data.Tasks
                .Where(t => t.IsCompleted && t.CompletedDay == currentDay)
                .OrderBy(t => t.SortOrder)
                .ToList();
        }

        public List<PlannerTask> GetCompletedNonRecurring()
        {
            return this.data.Tasks
                .Where(t => t.IsCompleted && !t.IsRecurring)
                .OrderByDescending(t => t.CompletedDay)
                .ToList();
        }

        public void ProcessDayStart(int currentDay)
        {
            if (currentDay <= this.data.LastProcessedDay)
                return;

            this.data.LastProcessedDay = currentDay;

            foreach (var task in this.data.Tasks)
            {
                if (task.IsRecurring && task.IsCompleted)
                {
                    task.IsCompleted = false;
                    task.CompletedDay = null;
                    foreach (var sub in task.SubTasks)
                        sub.IsCompleted = false;
                }
            }

            this.Save();
            this.monitor.Log($"Day {currentDay}: Reset recurring tasks.", LogLevel.Trace);
        }

        public void RemoveTasksByTemplate(string templateId)
        {
            this.data.Tasks.RemoveAll(t => t.TemplateId == templateId);
            this.Save();
        }

        public bool HasTasksForTemplate(string templateId)
        {
            return this.data.Tasks.Any(t => t.TemplateId == templateId);
        }

        public void Save()
        {
            this.helper.Data.WriteJsonFile($"data/{Constants.SaveFolderName}.json", this.data);
        }
    }
}
