using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OedetailType
    {
        public string TypeId { get; set; }
        public string Description { get; set; }
        public bool? FinishedGood { get; set; }
        public string ParentTypeId { get; set; }
        public byte? Sequence { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? ActiveFlag { get; set; }
        public string CommodityCode { get; set; }
        public bool? CommercialInvoiceLineItemFlag { get; set; }
        public string ShortDesc { get; set; }
        public string ShippingReportCategory { get; set; }
        public string PartsPerManhourReportCategory { get; set; }
    }
}
