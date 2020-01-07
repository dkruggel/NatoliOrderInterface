using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersReadyToPrintView
    {
        public double OrderNo { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string EmployeeName { get; set; }
        public string DepartmentDesc { get; set; }
        public string CheckedBy { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public int TM2 { get; set; }
        public int Tablet { get; set; }
        public int Tool { get; set; }
    }
}
