using System.Collections.Generic;

namespace StarDo.Models
{
    public class PlannerData
    {
        public int DataVersion { get; set; } = 4;
        public List<PlannerTask> Tasks { get; set; } = new();
        public int LastProcessedDay { get; set; }
    }
}
