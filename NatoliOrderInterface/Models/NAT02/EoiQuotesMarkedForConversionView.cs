using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesMarkedForConversionView
    {
        public double QuoteNo { get; set; }
        public short? QuoteRevNo { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string CsrMarked { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string Rush { get; set; }
        public int? DaysMarked { get; set; }
        public string ShipDate { get; set; }
    }
}
