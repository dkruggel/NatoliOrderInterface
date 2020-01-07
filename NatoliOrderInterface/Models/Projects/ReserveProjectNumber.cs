using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ReserveProjectNumber
    {
        public int? ReserveProjectNumber1 { get; set; }
        public string DwspecificationId { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string SubmittedBy { get; set; }
        public int? InsertState { get; set; }
    }
}
