using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class HoldStatus
    {
        public string ProjectNumber { get; set; }
        public string RevisionNumber { get; set; }
        public string HoldStatus1 { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string OnHoldComment { get; set; }
    }
}
