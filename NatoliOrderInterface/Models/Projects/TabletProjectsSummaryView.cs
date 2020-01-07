using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class TabletProjectsSummaryView
    {
        public string Designer { get; set; }
        public int? TabletProjectsToday { get; set; }
        public int? TabletProjectsThisWeek { get; set; }
        public int? TabletProjectsThisMonth { get; set; }
    }
}
