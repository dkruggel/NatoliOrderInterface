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
        public int VariablesExist { get; set; }
        public bool Generated { get; set; }
        public bool Generating { get; set; }

        public bool Equals(EoiAllOrdersView other)
        {
            if (other is null)
                return false;

            return this.OrderNumber == other.OrderNumber &&
                   this.QuoteNumber == other.QuoteNumber &&
                   this.QuoteRev == other.QuoteRev &&
                   this.CustomerName == other.CustomerName &&
                   this.NumDaysToShip == other.NumDaysToShip &&
                   this.RushYorN == other.RushYorN &&
                   this.PaidRushFee == other.PaidRushFee &&
                   this.ProcessState == other.ProcessState &&
                   this.TransitionName == other.TransitionName &&
                   this.DoNotProcess == other.DoNotProcess &&
                   this.DaysInDept == other.DaysInDept &&
                   this.EmployeeName == other.EmployeeName &&
                   this.BeingChecked == other.BeingChecked &&
                   this.MarkedForChecking == other.MarkedForChecking &&
                   this.Csr == other.Csr &&
                   this.CheckedBy == other.CheckedBy &&
                   this.Tm2 == other.Tm2 &&
                   this.Tablet == other.Tablet &&
                   this.Tool == other.Tool &&
                   this.VariablesExist == other.VariablesExist &&
                   this.Generated == other.Generated &&
                   this.Generating == other.Generating;
        }

        public override bool Equals(object obj) => Equals(obj as EoiAllOrdersView);
        public override int GetHashCode() => (OrderNumber, QuoteNumber, QuoteRev, CustomerName, NumDaysToShip, RushYorN, PaidRushFee, ProcessState, TransitionName, DoNotProcess, DaysInDept, EmployeeName, BeingChecked, MarkedForChecking, Csr, CheckedBy, Tm2, Tablet, Tool, VariablesExist, Generated, Generating).GetHashCode();
    }
}
