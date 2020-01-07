using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OrdOptionValueLSurfaceTreat
    {
        public int OrderNo { get; set; }
        public string OrderDetailType { get; set; }
        public string OptionCode { get; set; }
        public string SurfaceTreatment { get; set; }
        public string VendorId { get; set; }
        public DateTime? DateVerified { get; set; }
    }
}
