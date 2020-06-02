using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class AllProjectsView
    {
        public int ProjectNumber { get; set; }
        public string RevNumber { get; set; }
        public string QuoteNumber { get; set; }
        public string CSR { get; set; }
        public string ReturnToCSR { get; set; }
        public string CustomerName { get; set; }
        public string EndUserName { get; set; }
        public string Product { get; set; }
        public string DieNumber { get; set; }
        public string DieShape { get; set; }
        public string UpperHobNumber { get; set; }
        public string LowerHobNumber { get; set; }
        public string ShortRejectHobNumber { get; set; }
        public string LongRejectHobNumber { get; set; }
        public DateTime DueDate { get; set; }
    }
}
