using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OrdOptionValueIHardness2
    {
        public int OrderNo { get; set; }
        public string OrderDetailType { get; set; }
        public string OptionCode { get; set; }
        public short? Hardness1 { get; set; }
        public short? Hardness2 { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
