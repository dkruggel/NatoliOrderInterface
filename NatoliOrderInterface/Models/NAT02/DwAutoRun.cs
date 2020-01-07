using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class DwAutoRun
    {
        public int SpecificationId { get; set; }
        public double WorkOrderNumber { get; set; }
        public string ProcessState { get; set; }
        public string TransitionName { get; set; }
    }
}
