using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteOptionValueDDegreeVal
    {
        public int QuoteNo { get; set; }
        public short? RevNo { get; set; }
        public string QuoteDetailType { get; set; }
        public string OptionCode { get; set; }
        public short? Degrees { get; set; }
        public string Text { get; set; }
        public double? Value { get; set; }
        public float? ValueMm { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
