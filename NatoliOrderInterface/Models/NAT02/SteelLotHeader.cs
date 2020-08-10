using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.NAT02
{
    public partial class SteelLotHeader
    {
        public int OrderNo { get; set; }
        public short OrderLineNumber { get; set; }
        public string DetailTypeId { get; set; }
        public int SteelLotNumber { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public short? ProblemCode { get; set; }
        public string Notes { get; set; }
        public string TravellerNo { get; set; }
        public string BarcodeCardCode { get; set; }
    }
}
