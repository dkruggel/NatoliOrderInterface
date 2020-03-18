using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesNotConvertedView : IEquatable<EoiQuotesNotConvertedView>
    {
        public double QuoteNo { get; set; }
        public short? QuoteRevNo { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string RushYorN { get; set; }
        public DateTime QuoteDate { get; set; }
        public string RepId { get; set; }

        public bool Equals(EoiQuotesNotConvertedView other)
        {
            if (other is null)
                return false;

            return this.QuoteNo == other.QuoteNo &&
                   this.QuoteRevNo == other.QuoteRevNo &&
                   this.CustomerName == other.CustomerName &&
                   this.Csr == other.Csr &&
                   this.RushYorN == other.RushYorN &&
                   this.QuoteDate == other.QuoteDate &&
                   this.RepId == other.RepId;
        }

        public override bool Equals(object obj) => Equals(obj as EoiQuotesNotConvertedView);
        public override int GetHashCode() => (QuoteNo, QuoteRevNo, CustomerName, Csr, RushYorN, QuoteDate, RepId).GetHashCode();
    }
}
