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
    //class EoiOrdersInOfficeEqualityComparer : IEqualityComparer<EoiAllOrdersView>
    //{

    //        // Products are equal if their names and product numbers are equal.
    //        public bool Equals(EoiAllOrdersView x, EoiAllOrdersView y)
    //        {

    //            //Check whether the compared objects reference the same data.
    //            if (Object.ReferenceEquals(x, y)) return true;

    //            //Check whether any of the compared objects is null.
    //            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
    //                return false;

    //            //Check whether the products' properties are equal.
    //            return x.OrderNumber == y.OrderNumber &&
    //               x.CustomerName == y.CustomerName &&
    //               x.NumDaysToShip == y.NumDaysToShip &&
    //               x.DaysInDept == y.DaysInDept &&
    //               x.EmployeeName == y.EmployeeName &&
    //               x.Csr == y.Csr &&
    //               x.RushYorN == y.RushYorN &&
    //               x.PaidRushFee == y.PaidRushFee &&
    //               x.DoNotProcess == y.DoNotProcess;
    //    }

    //        // If Equals() returns true for a pair of objects
    //        // then GetHashCode() must return the same value for these objects.

    //    public int GetHashCode(EoiAllOrdersView obj)
    //    {
    //        // Check whether the object is null
    //        if (Object.ReferenceEquals(obj, null)) return 0;

    //        return new { obj.OrderNumber, obj.CustomerName, obj.NumDaysToShip, obj.DaysInDept, obj.EmployeeName, obj.Csr, obj.RushYorN, obj.PaidRushFee, obj.DoNotProcess}.GetHashCode();
    //    }
    //}
}
