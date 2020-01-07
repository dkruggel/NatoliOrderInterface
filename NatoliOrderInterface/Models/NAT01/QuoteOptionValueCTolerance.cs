using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteOptionValueCTolerance
    {
        public int QuoteNo { get; set; }
        public short? RevNo { get; set; }
        public string QuoteDetailType { get; set; }
        public string OptionCode { get; set; }
        public double? TopValue { get; set; }
        public double? BottomValue { get; set; }
        public float? TopMm { get; set; }
        public float? BottomMm { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
