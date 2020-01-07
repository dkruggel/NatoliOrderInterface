using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteOptionValueOInteger
    {
        public int QuoteNo { get; set; }
        public short? RevNo { get; set; }
        public string QuoteDetailType { get; set; }
        public string OptionCode { get; set; }
        public short? Integer { get; set; }
        public string Text { get; set; }
        public DateTime? DateVerified { get; set; }
        public float? BoreCircle { get; set; }
    }
}
