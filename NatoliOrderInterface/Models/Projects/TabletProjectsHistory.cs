using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class TabletProjectsHistory
    {
        public int Proj { get; set; }
        public string DateCreated { get; set; }
        public string DueDate { get; set; }
        public string DateCompleted { get; set; }
        public string Priority { get; set; }
        public string ProjStarted { get; set; }
        public string DrawnBy { get; set; }
        public string CheckedBy { get; set; }
        public string Csr { get; set; }
        public string CustomerName { get; set; }
        public string Product { get; set; }
        public string UpHob { get; set; }
        public string LowHob { get; set; }
        public string MiscNotes { get; set; }
    }
}
