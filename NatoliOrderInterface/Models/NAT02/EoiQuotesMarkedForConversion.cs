using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesMarkedForConversion
    {
        public double QuoteNo { get; set; }
        public short? QuoteRevNo { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string CsrMarked { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string Rush { get; set; }
    }
}
