using System;
using System.Collections.Generic;
using System.Linq;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersEnteredAndUnscannedView
    {
        public double OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public int? NumDaysIn { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public string ProcessState { get; set; }
        public string TransitionName { get; set; }
        public int DoNotProcess { get; set; }
    }
}
