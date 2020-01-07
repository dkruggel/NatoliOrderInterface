using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ProjectSpecSheetTest
    {
        public int ProjectsId { get; set; }
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string CustomerNumber { get; set; }
        public int? ShipToLocation { get; set; }
        public string Csr { get; set; }
        public string CustomerName { get; set; }
        public string Attention { get; set; }
        public string InternationalId { get; set; }
        public string EndUser { get; set; }
        public double? QuoteOrOrderNumber { get; set; }
        public int? QuoteRevNum { get; set; }
        public string Product { get; set; }
        public string Tablet { get; set; }
        public string Tools { get; set; }
        public string UpperPrint { get; set; }
        public string LowerPrint { get; set; }
        public string DiePrint { get; set; }
        public string RejectPrint { get; set; }
        public string AlignPrint { get; set; }
        public string MachineNumber { get; set; }
        public string MachName { get; set; }
        public string MachineType { get; set; }
        public string MachineStyle { get; set; }
        public string SpecialMachineInstructions { get; set; }
        public double? DieHeight { get; set; }
        public double? DieDiameter { get; set; }
        public string CupType { get; set; }
        public string UpperHobNumber { get; set; }
        public string UpperHobDescription { get; set; }
        public string LowerHobNumber { get; set; }
        public string LowerHobDescription { get; set; }
        public string Shape { get; set; }
        public string OtherShape { get; set; }
        public double? TabletWidth { get; set; }
        public double? TabletLength { get; set; }
        public double? CupDepth { get; set; }
        public double? Land { get; set; }
        public string PunchSteel { get; set; }
        public string DieSteel { get; set; }
        public string DieInsert { get; set; }
        public string UpperKey { get; set; }
        public string LowerKey { get; set; }
        public string RejectKey { get; set; }
        public string TypeOfKey { get; set; }
        public string UpperGroove { get; set; }
        public string LowerGroove { get; set; }
        public string TipReliefForUpper { get; set; }
        public string Ccwangle { get; set; }
        public string Cwangle { get; set; }
        public string MiscNotes { get; set; }
        public string TabletDrawnBy { get; set; }
        public string ToolDrawnBy { get; set; }
        public string TabletCheckedBy { get; set; }
        public string ToolCheckedBy { get; set; }
        public string MarkedPriority { get; set; }
        public string ProjectStartedTablet { get; set; }
        public string ProjectStartedTool { get; set; }
        public int? SpecificationId { get; set; }
        public string DueDate { get; set; }
        public string DrawingNumber { get; set; }
        public DateTime? TabletCompletionDate { get; set; }
        public DateTime? ToolCompletionDate { get; set; }
        public string HoldStatus { get; set; }
        public string FilmCoated { get; set; }
        public DateTime? TimeSubmitted { get; set; }
    }
}
