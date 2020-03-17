using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesMarkedForConversionView : IEquatable<EoiQuotesMarkedForConversionView>
    {
        public double QuoteNo { get; set; }
        public short? QuoteRevNo { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string CsrMarked { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string Rush { get; set; }
        public int? DaysMarked { get; set; }
        public string ShipDate { get; set; }

        public bool Equals(EoiQuotesMarkedForConversionView other)
        {
            if (other is null)
                return false;

            return this.QuoteNo == other.QuoteNo &&
                   this.QuoteRevNo == other.QuoteRevNo &&
                   this.CustomerName == other.CustomerName &&
                   this.Csr == other.Csr &&
                   this.CsrMarked == other.CsrMarked &&
                   this.TimeSubmitted == other.TimeSubmitted &&
                   this.Rush == other.Rush &&
                   this.DaysMarked == other.DaysMarked &&
                   this.ShipDate == other.ShipDate;
        }

        public override bool Equals(object obj) => Equals(obj as EoiQuotesMarkedForConversionView);
        public override int GetHashCode() => (QuoteNo, QuoteRevNo, CustomerName, Csr, CsrMarked, TimeSubmitted, Rush, DaysMarked, ShipDate).GetHashCode();
    }
}
