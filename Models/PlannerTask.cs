using System;
using System.Collections.Generic;

namespace StarDo.Models
{
    public class PlannerTask
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
        public string Title { get; set; } = "";
        public string Notes { get; set; } = "";
        public List<SubTask> SubTasks { get; set; } = new();
        public TaskCategory Category { get; set; } = TaskCategory.Farm;
        public TaskPriority Priority { get; set; } = TaskPriority.LongTerm;
        public bool IsCompleted { get; set; }
        public bool IsRecurring { get; set; }
        public string TemplateId { get; set; }
        public int CreatedDay { get; set; }
        public int? CompletedDay { get; set; }
        public int SortOrder { get; set; }
    }
}
