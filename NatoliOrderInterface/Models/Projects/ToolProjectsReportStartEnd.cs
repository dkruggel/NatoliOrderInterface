using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ToolProjectsReportStartEnd
    {
        public string Drafter { get; set; }
        public DateTime ToolStartedDateTime { get; set; }
        public DateTime ToolDrawnDateTime { get; set; }
        public int Minutes { get; set; }
        public decimal Hours { get; set; }
        public int ProjectNumber { get; set; }
        public int RevNumber { get; set; }
        public string TableName { get; set; }
    }
}
