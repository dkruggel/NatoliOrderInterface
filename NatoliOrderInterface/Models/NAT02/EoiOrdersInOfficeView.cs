using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrdersInOfficeView
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
    }
}
