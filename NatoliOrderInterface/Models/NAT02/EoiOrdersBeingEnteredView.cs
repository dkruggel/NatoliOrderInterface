using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersBeingEnteredView
    {
        public double OrderNo { get; set; }
        public double? QuoteNo { get; set; }
        public short? Rev { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
    }
}
