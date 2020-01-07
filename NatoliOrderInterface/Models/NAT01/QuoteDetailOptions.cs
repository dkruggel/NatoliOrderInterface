using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteDetailOptions
    {
        public double QuoteNumber { get; set; }
        public short? RevisionNo { get; set; }
        public short QuoteDetailLineNo { get; set; }
        public short? OptionLineNo { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string OptionText { get; set; }
        public float? OptionLookupPrice { get; set; }
        public float? OptionLookupPct { get; set; }
        public float? OrdDetOptPrice { get; set; }
        public float? OrdDetOptPercnt { get; set; }
        public string OptionComments { get; set; }
    }
}
