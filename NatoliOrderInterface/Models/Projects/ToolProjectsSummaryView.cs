using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ToolProjectsSummaryView
    {
        public string Designer { get; set; }
        public int? ToolProjectsToday { get; set; }
        public int? ToolProjectsThisWeek { get; set; }
        public int? ToolProjectsThisMonth { get; set; }
    }
}
