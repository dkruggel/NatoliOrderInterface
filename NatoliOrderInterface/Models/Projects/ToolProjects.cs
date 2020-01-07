using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ToolProjects
    {
        public int Proj { get; set; }
        public string CustomerName { get; set; }
        public string EndUser { get; set; }
        public string MiscNotes { get; set; }
        public string DateCreated { get; set; }
        public string TimeCreated { get; set; }
        public string DueDate { get; set; }
        public string Priority { get; set; }
        public string ProjStarted { get; set; }
        public string DrawnBy { get; set; }
        public string SubmittedBy { get; set; }
        public string Csr { get; set; }
        public string Product { get; set; }
        public string MachineNumber { get; set; }
        public string UpHob { get; set; }
        public string LowHob { get; set; }
        public string HoldStatus { get; set; }
        public int? QtyOfProjectsDelaying { get; set; }
        public int? DaysDelayed { get; set; }
        public string Comments { get; set; }
        public string ToolAssignedTo { get; set; }
    }
}
