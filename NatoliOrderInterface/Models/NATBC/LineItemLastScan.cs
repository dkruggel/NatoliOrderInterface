using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class LineItemLastScan
    {
        public string OrderDetailTypeDescription { get; set; }
        public short OrderLineNumber {get; set;}
        public DateTime ScanTimeStamp { get; set; }
        public string Department { get; set; }
        public string Employee { get; set; }
    }
}
