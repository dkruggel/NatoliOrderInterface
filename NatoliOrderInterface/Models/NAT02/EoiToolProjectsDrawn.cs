using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiToolProjectsDrawn
    {
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string ReturnToCSR { get; set; }
        public string ProjectStartedTool { get; set; }
        public string ToolDrawnBy { get; set; }
        public double? QuoteOrOrderNumber { get; set; }
        public int? QuoteRevNum { get; set; }
        public string Product { get; set; }
        public string MarkedPriority { get; set; }
        public DateTime? DueDate { get; set; }
    }
}
