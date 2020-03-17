using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersInOfficeView : IEquatable<EoiOrdersInOfficeView>
    {
        public int? OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public int? DaysInOffice { get; set; }
        public string EmployeeName { get; set; }
        public string Csr { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public int DoNotProcess { get; set; }

        public bool Equals(EoiOrdersInOfficeView other)
        {
            if (other is null)
                return false;

            return this.OrderNo == other.OrderNo &&
                   this.CustomerName == other.CustomerName &&
                   this.NumDaysToShip == other.NumDaysToShip &&
                   this.DaysInOffice == other.DaysInOffice &&
                   this.EmployeeName == other.EmployeeName &&
                   this.Csr == other.Csr &&
                   this.RushYorN == other.RushYorN &&
                   this.PaidRushFee == other.PaidRushFee &&
                   this.DoNotProcess == other.DoNotProcess;
        }

        public override bool Equals(object obj) => Equals(obj as EoiOrdersInOfficeView);
        public override int GetHashCode() => (OrderNo, CustomerName, NumDaysToShip, DaysInOffice, EmployeeName, Csr, RushYorN, PaidRushFee, DoNotProcess).GetHashCode();
    }
}
