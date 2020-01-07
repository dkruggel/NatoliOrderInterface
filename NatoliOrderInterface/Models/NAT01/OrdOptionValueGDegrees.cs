using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OrdOptionValueGDegrees
    {
        public int OrderNo { get; set; }
        public string OrderDetailType { get; set; }
        public string OptionCode { get; set; }
        public short? Degrees { get; set; }
        public string Text { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
