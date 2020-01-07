using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OrdOptionValuePDegDec
    {
        public int OrderNo { get; set; }
        public string OrderDetailType { get; set; }
        public string OptionCode { get; set; }
        public float? DegreesDecimal { get; set; }
        public string Text { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
