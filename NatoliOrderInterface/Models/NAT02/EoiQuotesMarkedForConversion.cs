using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesMarkedForConversion
    {
        public double QuoteNo { get; set; }
        public short? QuoteRevNo { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string CsrMarked { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string Rush { get; set; }

        public bool Equals(EoiQuotesMarkedForConversion other)
        {
            if (other is null)
                return false;

            return this.QuoteNo == other.QuoteNo &&
                   this.QuoteRevNo == other.QuoteRevNo &&
                   this.CustomerName == other.CustomerName &&
                   this.Csr == other.Csr &&
                   this.CsrMarked == other.CsrMarked &&
                   this.TimeSubmitted == other.TimeSubmitted &&
                   this.Rush == other.Rush;
        }

        public override bool Equals(object obj) => Equals(obj as EoiQuotesMarkedForConversion);
        public override int GetHashCode() => (QuoteNo, QuoteRevNo, CustomerName, Csr, CsrMarked, TimeSubmitted, Rush).GetHashCode();
    }
}
