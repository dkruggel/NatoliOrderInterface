using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class DelayedProjects
    {
        public int ProjectNumber { get; set; }
        public int? QtyProjectsDelaying { get; set; }
        public int? NumberOfDaysDelayed { get; set; }
        public string Comments { get; set; }
    }
}
