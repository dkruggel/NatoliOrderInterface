using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersBeingEnteredView : IEquatable<EoiOrdersBeingEnteredView>
    {
        public double OrderNo { get; set; }
        public double? QuoteNo { get; set; }
        public short? Rev { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }

        public bool Equals(EoiOrdersBeingEnteredView other)
        {
            if (other is null)
                return false;

            return this.OrderNo == other.OrderNo &&
                   this.QuoteNo == other.QuoteNo &&
                   this.Rev == other.Rev &&
                   this.CustomerName == other.CustomerName &&
                   this.NumDaysToShip == other.NumDaysToShip &&
                   this.RushYorN == other.RushYorN &&
                   this.PaidRushFee == other.PaidRushFee;
        }

        public override bool Equals(object obj) => Equals(obj as EoiOrdersBeingEnteredView);
        public override int GetHashCode() => (OrderNo, QuoteNo, Rev, CustomerName, NumDaysToShip, RushYorN, PaidRushFee).GetHashCode();
    }
}
