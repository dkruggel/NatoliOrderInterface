﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class NatoliOrderList : IEquatable<NatoliOrderList>
    {
        public double OrderNo { get; set; }
        public string Customer { get; set; }
        public DateTime ShipDate { get; set; }
        public string Rush { get; set; }
        public string OnHold { get; set; }
        public string RepInitials { get; set; }
        public bool Equals(NatoliOrderList other)
        {
            if (other is null)
                return false;

            return this.OrderNo == other.OrderNo &&
                   this.Customer == other.Customer &&
                   this.ShipDate == other.ShipDate &&
                   this.Rush == other.Rush &&
                   this.OnHold == other.OnHold &&
                   this.RepInitials == other.RepInitials;
        }

        public override bool Equals(object obj) => Equals(obj as NatoliOrderList);
        public override int GetHashCode() => (OrderNo, Customer, Customer, ShipDate, Rush, OnHold, RepInitials).GetHashCode();
    }
}
