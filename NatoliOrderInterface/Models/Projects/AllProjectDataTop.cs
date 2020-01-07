using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class AllProjectDataTop
    {
        public int Proj { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string MiscNotes { get; set; }
        public string DateCreated { get; set; }
        public string DueDate { get; set; }
        public string TabletCheckedBy { get; set; }
        public string TabletCompletionDate { get; set; }
        public string ToolCheckedBy { get; set; }
        public string ToolCompletionDate { get; set; }
        public string Csr { get; set; }
        public string DrawingNumber { get; set; }
        public string UpperHobNumber { get; set; }
        public string LowerHobNumber { get; set; }
        public string MachName { get; set; }
        public string MachineNumber { get; set; }
        public string Product { get; set; }
        public string EndUser { get; set; }
        public string Attention { get; set; }
    }
}
