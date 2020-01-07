using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class PriorityProjects
    {
        public int Proj { get; set; }
        public string Priority { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string DateCreated { get; set; }
        public string DueDate { get; set; }
        public string TabletDrawnBy { get; set; }
        public string TabletCheckedBy { get; set; }
        public string ToolDrawnBy { get; set; }
        public string ToolCheckedBy { get; set; }
        public string Csr { get; set; }
        public string Product { get; set; }
        public string UpperHobNumber { get; set; }
        public string LowerHobNumber { get; set; }
    }
}
