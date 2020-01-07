using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersInEngineeringUnprintedView
    {
        public double OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string EmployeeName { get; set; }
        public int? DaysInEng { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public int DoNotProcess { get; set; }
        public int BeingChecked { get; set; }
        public int MarkedForChecking { get; set; }
    }
}
