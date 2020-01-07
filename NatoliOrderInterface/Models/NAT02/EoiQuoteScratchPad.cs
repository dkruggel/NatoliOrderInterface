using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuoteScratchPad
    {
        public double QuoteNo { get; set; }
        public byte RevNo { get; set; }
        public short LineNo { get; set; }
        public string LineType { get; set; }
        public decimal BasePrice { get; set; }
        public decimal PercentMark { get; set; }
        public string Comment { get; set; }
        public string User { get; set; }
        public DateTime DateTimeStamp { get; set; }
    }
}
