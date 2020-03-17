using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiAllTabletProjectsView : IEquatable<EoiAllTabletProjectsView>
    {
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string CustomerName { get; set; }
        public string Csr { get; set; }
        public string Drafter { get; set; }
        public string ProjectStartedTablet { get; set; }
        public string TabletDrawnBy { get; set; }
        public string TabletSubmittedBy { get; set; }
        public double? QuoteOrOrderNumber { get; set; }
        public int? QuoteRevNum { get; set; }
        public string Product { get; set; }
        public bool? Tools { get; set; }
        public string MarkedPriority { get; set; }
        public DateTime? DueDate { get; set; }
        public string HoldStatus { get; set; }
        public string ReturnToCsr { get; set; }

        public bool Equals(EoiAllTabletProjectsView other)
        {
            if (other is null)
                return false;

            return this.ProjectNumber == other.ProjectNumber &&
                   this.RevisionNumber == other.RevisionNumber &&
                   this.CustomerName == other.CustomerName &&
                   this.Csr == other.Csr &&
                   this.Drafter == other.Drafter &&
                   this.ProjectStartedTablet == other.ProjectStartedTablet &&
                   this.TabletDrawnBy == other.TabletDrawnBy &&
                   this.TabletSubmittedBy == other.TabletSubmittedBy &&
                   this.QuoteOrOrderNumber == other.QuoteOrOrderNumber &&
                   this.QuoteRevNum == other.QuoteRevNum &&
                   this.Product == other.Product &&
                   this.Tools == other.Tools &&
                   this.MarkedPriority == other.MarkedPriority &&
                   this.DueDate == other.DueDate &&
                   this.HoldStatus == other.HoldStatus &&
                   this.ReturnToCsr == other.ReturnToCsr;
        }

        public override bool Equals(object obj) => Equals(obj as EoiAllTabletProjectsView);
        public override int GetHashCode() => (ProjectNumber, RevisionNumber, CustomerName, Csr, Drafter, ProjectStartedTablet, TabletDrawnBy, TabletSubmittedBy, QuoteOrOrderNumber, QuoteRevNum,
                                              Product, Tools, MarkedPriority, DueDate, HoldStatus, ReturnToCsr).GetHashCode();
    }
}
