using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuoteSMICheck
    {
        public double QuoteNo { get; set; }
        public byte RevNo { get; set; }
        public string CustomerID { get; set; }
        public short Sequence { get; set; }
        public bool? AppliesToQuote { get; set; }
        public string Instruction { get; set; }
        public string User { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }
}
