using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiToolProjectsOutOfOrder
    {
        public string ProjectNumber { get; set; }
        public string RevNumber { get; set; }
        public DateTime DueDate { get; set; }
        public bool Priority { get; set; }
        public short ProjectsWithHigherPriority { get; set; }
    }
}
