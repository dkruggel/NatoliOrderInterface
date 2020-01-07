using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ToolDrawnBy
    {
        public int? ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string ToolDrawnBy1 { get; set; }
        public DateTime? TimeSubmitted { get; set; }
    }
}
