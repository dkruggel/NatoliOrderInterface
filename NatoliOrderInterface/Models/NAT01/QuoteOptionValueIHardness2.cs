using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteOptionValueIHardness2
    {
        public int QuoteNo { get; set; }
        public short? RevNo { get; set; }
        public string QuoteDetailType { get; set; }
        public string OptionCode { get; set; }
        public short? Hardness1 { get; set; }
        public short? Hardness2 { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
