using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class NatoliOrderList
    {
        public double OrderNo { get; set; }
        public string Customer { get; set; }
        public DateTime ShipDate { get; set; }
        public string Rush { get; set; }
        public string OnHold { get; set; }
        public string RepInitials { get; set; }
    }
}
