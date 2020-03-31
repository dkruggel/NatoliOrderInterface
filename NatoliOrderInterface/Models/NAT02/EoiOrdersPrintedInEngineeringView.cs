using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersPrintedInEngineeringView : IEquatable<EoiOrdersPrintedInEngineeringView>
    {
        public double OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string EmployeeName { get; set; }
        public string CheckedBy { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public int TM2 { get; set; }
        public int Tablet { get; set; }
        public int Tool { get; set; }
        public int VariablesExist { get; set; }

        public bool Equals(EoiOrdersPrintedInEngineeringView other)
        {
            if (other is null)
                return false;

            return this.OrderNo == other.OrderNo &&
                   this.CustomerName == other.CustomerName &&
                   this.NumDaysToShip == other.NumDaysToShip &&
                   this.EmployeeName == other.EmployeeName &&
                   this.CheckedBy == other.CheckedBy &&
                   this.RushYorN == other.RushYorN &&
                   this.PaidRushFee == other.PaidRushFee &&
                   this.TM2 == other.TM2 &&
                   this.Tablet == other.Tablet &&
                   this.Tool == other.Tool &&
                   this.VariablesExist == other.VariablesExist;
        }

        public override bool Equals(object obj) => Equals(obj as EoiOrdersPrintedInEngineeringView);
        public override int GetHashCode() => (OrderNo, CustomerName, NumDaysToShip, EmployeeName, CheckedBy, RushYorN, PaidRushFee, TM2, Tablet, Tool, VariablesExist).GetHashCode();
    }
}
