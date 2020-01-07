using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OrdOptionValueCTolerance
    {
        public int OrderNo { get; set; }
        public string OrderDetailType { get; set; }
        public string OptionCode { get; set; }
        public double? TopValue { get; set; }
        public double? BottomValue { get; set; }
        public float? TopMm { get; set; }
        public float? BottomMm { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
