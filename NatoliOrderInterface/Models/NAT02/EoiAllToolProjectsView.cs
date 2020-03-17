using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiAllToolProjectsView : IEquatable<EoiAllToolProjectsView>
    {
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string Drafter { get; set; }
        public string ProjectStartedTool { get; set; }
        public string ToolDrawnBy { get; set; }
        public double? QuoteOrOrderNumber { get; set; }
        public int? QuoteRevNum { get; set; }
        public string Product { get; set; }
        public string MarkedPriority { get; set; }
        public DateTime? DueDate { get; set; }
        public string HoldStatus { get; set; }
        public string ReturnToCsr { get; set; }

        public bool Equals(EoiAllToolProjectsView other)
        {
            if (other is null)
                return false;

            return this.ProjectNumber == other.ProjectNumber &&
                   this.RevisionNumber == other.RevisionNumber &&
                   this.CustomerName == other.CustomerName &&
                   this.Csr == other.Csr &&
                   this.Drafter == other.Drafter &&
                   this.ProjectStartedTool == other.ProjectStartedTool &&
                   this.ToolDrawnBy == other.ToolDrawnBy &&
                   this.QuoteOrOrderNumber == other.QuoteOrOrderNumber &&
                   this.QuoteRevNum == other.QuoteRevNum &&
                   this.Product == other.Product &&
                   this.MarkedPriority == other.MarkedPriority &&
                   this.DueDate == other.DueDate &&
                   this.HoldStatus == other.HoldStatus &&
                   this.ReturnToCsr == other.ReturnToCsr;
        }

        public override bool Equals(object obj) => Equals(obj as EoiAllToolProjectsView);
        public override int GetHashCode() => (ProjectNumber, RevisionNumber, CustomerName, Csr, Drafter, ProjectStartedTool, ToolDrawnBy, QuoteOrOrderNumber, QuoteRevNum,
                                              Product, MarkedPriority, DueDate, HoldStatus, ReturnToCsr).GetHashCode();
    }
}
