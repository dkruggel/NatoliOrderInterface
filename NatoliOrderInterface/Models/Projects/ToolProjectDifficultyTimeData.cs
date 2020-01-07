using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ToolProjectDifficultyTimeData
    {
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public TimeSpan? EstimatedTime { get; set; }
        public TimeSpan? ActualTime { get; set; }
    }
}
