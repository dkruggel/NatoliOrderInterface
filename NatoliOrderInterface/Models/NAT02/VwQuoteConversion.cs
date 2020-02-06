using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class VwQuoteConversion
    {
        public string Rep { get; set; }
        public int? Converted { get; set; }
        public int? NotConverted { get; set; }
        public int? Total { get; set; }
        public string Rate { get; set; }
    }
}
