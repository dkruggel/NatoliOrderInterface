using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OrderDetailOptions
    {
        public double OrderNumber { get; set; }
        public short OrderDetailLineNo { get; set; }
        public short? OptionLineNo { get; set; }
        public string OptionCode { get; set; }
        public string OptionType { get; set; }
        public string OptionText { get; set; }
        public float? OptionLookupPrice { get; set; }
        public float? OptionLookupPercnt { get; set; }
        public float? OrdDetOptPrice { get; set; }
        public float? OrdDetOptPercnt { get; set; }
        public string OptionComments { get; set; }
    }
}
