using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class PartAllocation
    {
        public string QuoteNumber { get; set; }
        public int QuoteRevNo { get; set; }
        public string WorkOrderNumber { get; set; }
        public string PartNumber { get; set; }
        public short? Quantity { get; set; }
        public DateTime? EnteredDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public decimal OD { get; set; }
        public decimal? ID { get; set; }
        public decimal OL { get; set; }
        public string ABC { get; set; }
        public string BinNumber { get; set; }
        public string Drawer { get; set; }
        public string Column1 { get; set; }
        public string Column2 { get; set; }
    }
}
