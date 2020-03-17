using System;
using System.Collections.Generic;
using System.Linq;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersEnteredAndUnscannedView : IEquatable<EoiOrdersEnteredAndUnscannedView>
    {
        public double OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public int? NumDaysIn { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public string ProcessState { get; set; }
        public string TransitionName { get; set; }
        public int DoNotProcess { get; set; }

        public bool Equals(EoiOrdersEnteredAndUnscannedView other)
        {
            if (other is null)
                return false;

            return this.OrderNo == other.OrderNo &&
                   this.CustomerName == other.CustomerName &&
                   this.NumDaysToShip == other.NumDaysToShip &&
                   this.NumDaysIn == other.NumDaysIn &&
                   this.RushYorN == other.RushYorN &&
                   this.PaidRushFee == other.PaidRushFee &&
                   this.ProcessState == other.ProcessState &&
                   this.TransitionName == other.TransitionName &&
                   this.DoNotProcess == other.DoNotProcess;
        }

        public override bool Equals(object obj) => Equals(obj as EoiOrdersEnteredAndUnscannedView);
        public override int GetHashCode() => (OrderNo, CustomerName, NumDaysToShip, NumDaysIn, RushYorN, PaidRushFee, ProcessState, TransitionName, DoNotProcess).GetHashCode();
    }
}
