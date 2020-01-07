﻿using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteOptionValueKVendor
    {
        public int QuoteNo { get; set; }
        public short? RevNo { get; set; }
        public string QuoteDetailType { get; set; }
        public string OptionCode { get; set; }
        public string VendorId { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
