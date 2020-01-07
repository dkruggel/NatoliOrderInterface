using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class JUnk
    {
        public string DateCreated { get; set; }
        public string DueDate { get; set; }
        public int ProjectNumber { get; set; }
        public int? RevNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string MiscNotes { get; set; }
        public string DrawingNumber { get; set; }
        public string HoldStatus { get; set; }
    }
}
