using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiBasePriceList
    {
        public string Category { get; set; }
        public string MachineType { get; set; }
        public string PunchType { get; set; }
        public string Shape { get; set; }
        public string SteelPriceCode { get; set; }
        public float BasePrice { get; set; }
        public short OrderQty { get; set; }
        public short Expr1 { get; set; }
        public short QuantityOrdered { get; set; }
        public double QuoteNo { get; set; }
        public string InternationalYorN { get; set; }
        public short? Revision { get; set; }
    }
}
