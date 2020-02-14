using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiAllOrdersView
    {
        public double OrderNumber { get; set; }
        public double? QuoteNumber { get; set; }
        public short? QuoteRev { get; set; }
        public string CustomerName { get; set; }
        public int? NumDaysToShip { get; set; }
        public string RushYorN { get; set; }
        public string PaidRushFee { get; set; }
        public int BeingEntered { get; set; }
        public int EnteredUnscanned { get; set; }
        public int InTheOffice { get; set; }
        public int InEngineering { get; set; }
        public int ReadyToPrint { get; set; }
        public int Printed { get; set; }
        public string ProcessState { get; set; }
        public string TransitionName { get; set; }
        public int DoNotProcess { get; set; }
        public string EmployeeName { get; set; }
        public int? DaysInDept { get; set; }
        public int BeingChecked { get; set; }
        public int MarkedForChecking { get; set; }
        public string Csr { get; set; }
        public string CheckedBy { get; set; }
        public int Tm2 { get; set; }
        public int Tablet { get; set; }
        public int Tool { get; set; }
    }
}
