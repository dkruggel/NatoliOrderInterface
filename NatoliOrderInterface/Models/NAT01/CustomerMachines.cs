using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class CustomerMachines
    {
        public int CustomerMachineID { get; set; }
        public string CustomerNo { get; set; }
        public short? MachineNo { get; set; }
        public string MachineDesc { get; set; }
        public double? OD { get; set; }
        public double? OL { get; set; }
        public string AddNotes { get; set; }
        public string UpperSize { get; set; }
        public string LowerSize { get; set; }
        public string CustAddressCode { get; set; }
        public string CustReference { get; set; }
        public short? KeyAngle { get; set; }
        public string KeyDirection { get; set; }
        public string MachineStyle { get; set; }
        public short? LowerKeyAngle { get; set; }
        public string LowerKeyDirection { get; set; }
        public int? UpperHeadGauge { get; set; }
        public int? UpperGrooveProgram { get; set; }
        public int? LowerGrooveProgram { get; set; }
        public int? LowerHeadGauge { get; set; }
    }
}
