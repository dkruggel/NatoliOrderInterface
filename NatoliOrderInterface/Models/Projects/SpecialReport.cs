using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class SpecialReport
    {
        public int ProjectNumber { get; set; }
        public string TabletDrawnBy { get; set; }
        public string ToolDrawnBy { get; set; }
        public string TabletCompleted { get; set; }
        public string ToolCompleted { get; set; }
    }
}
