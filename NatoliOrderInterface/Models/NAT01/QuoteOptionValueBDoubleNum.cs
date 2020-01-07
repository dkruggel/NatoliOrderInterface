using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteOptionValueBDoubleNum
    {
        public int QuoteNo { get; set; }
        public short? RevNo { get; set; }
        public string QuoteDetailType { get; set; }
        public string OptionCode { get; set; }
        public double? Number1 { get; set; }
        public double? Number2 { get; set; }
        public float? Number1Mm { get; set; }
        public float? Number2Mm { get; set; }
        public string Text1 { get; set; }
        public string Text2 { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
