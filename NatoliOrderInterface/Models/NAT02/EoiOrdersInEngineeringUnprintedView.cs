using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersInEngineeringUnprintedView : IEquatable<EoiOrdersInEngineeringUnprintedView>
    {
        public double OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string EmployeeName { get; set; }
        public int? DaysInEng { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public int DoNotProcess { get; set; }
        public int BeingChecked { get; set; }
        public int MarkedForChecking { get; set; }

        public bool Equals(EoiOrdersInEngineeringUnprintedView other)
        {
            if (other is null)
                return false;

            return this.OrderNo == other.OrderNo &&
                   this.CustomerName == other.CustomerName &&
                   this.NumDaysToShip == other.NumDaysToShip &&
                   this.EmployeeName == other.EmployeeName &&
                   this.DaysInEng == other.DaysInEng &&
                   this.RushYorN == other.RushYorN &&
                   this.PaidRushFee == other.PaidRushFee &&
                   this.DoNotProcess == other.DoNotProcess &&
                   this.BeingChecked == other.BeingChecked &&
                   this.MarkedForChecking == other.MarkedForChecking;
        }

        public override bool Equals(object obj) => Equals(obj as EoiOrdersInEngineeringUnprintedView);
        public override int GetHashCode() => (OrderNo, CustomerName, NumDaysToShip, EmployeeName, DaysInEng, RushYorN, PaidRushFee, DoNotProcess, BeingChecked, MarkedForChecking).GetHashCode();
    }
}
