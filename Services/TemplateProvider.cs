using System.Collections.Generic;
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
    }

    public static class TemplateProvider
    {
        public static readonly List<TemplateDefinition> Templates = new()
        {
            new TemplateDefinition
            {
                Id = "animal_chores",
                Name = "Animal Chores",
                Description = "Daily animal care routine",
                Category = TaskCategory.Farm,
                SubTasks = new()
                {
                    "Pet animals",
                    "Collect eggs",
                    "Collect milk",
                    "Collect wool/feathers",
                    "Check for truffles"
                }
            },
            new TemplateDefinition
            {
                Id = "processing_check",
                Name = "Processing Check",
                Description = "Check all processing machines",
                Category = TaskCategory.Processing,
                SubTasks = new()
                {
                    "Check kegs",
                    "Check preserve jars",
                    "Check looms"
                }
            },
            new TemplateDefinition
            {
                Id = "harvest_replant",
                Name = "Harvest & Replant",
                Description = "Crop management routine",
                Category = TaskCategory.Farm,
                SubTasks = new()
                {
                    "Check greenhouse crops",
                    "Check outdoor plots",
                    "Replant as needed"
                }
            },
            new TemplateDefinition
            {
                Id = "tapper_collection",
                Name = "Tapper Collection",
                Description = "Collect from all tappers",
                Category = TaskCategory.Farm,
                SubTasks = new()
                {
                    "Collect from oak tappers",
                    "Collect from pine tappers",
                    "Collect from maple tappers"
                }
            }
        };

        public static TemplateDefinition GetById(string id)
        {
            return Templates.Find(t => t.Id == id);
        }

        public static PlannerTask CreateTaskFromTemplate(TemplateDefinition template, int currentDay)
        {
            var task = new PlannerTask
            {
                Title = template.Name,
                Notes = template.Description,
                Category = template.Category,
                Priority = TaskPriority.Daily,
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
