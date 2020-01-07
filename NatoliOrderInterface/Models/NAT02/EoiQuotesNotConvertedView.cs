using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesNotConvertedView
    {
        public double QuoteNo { get; set; }
        public short? QuoteRevNo { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string RushYorN { get; set; }
        public DateTime QuoteDate { get; set; }
    }
}
