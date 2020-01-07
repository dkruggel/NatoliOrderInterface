using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ProjectSpecSheet
    {
        public int ProjectsId { get; set; }
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string CustomerNumber { get; set; }
        public string ShipToLocation { get; set; }
        public string Csr { get; set; }
        public string EnglastMod { get; set; }
        public string CustomerName { get; set; }
        public string Attention { get; set; }
        public string InternationalId { get; set; }
        public string EndUser { get; set; }
        public double? QuoteOrOrderNumber { get; set; }
        public int? QuoteRevNum { get; set; }
        public string Product { get; set; }
        public bool? Tablet { get; set; }
        public bool? Tools { get; set; }
        public bool? UpperPrint { get; set; }
        public bool? LowerPrint { get; set; }
        public bool? DiePrint { get; set; }
        public bool? RejectPrint { get; set; }
        public bool? AlignPrint { get; set; }
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
        public bool? UpperKey { get; set; }
        public bool? LowerKey { get; set; }
        public bool? RejectKey { get; set; }
        public string TypeOfKey { get; set; }
        public string UpperGroove { get; set; }
        public string LowerGroove { get; set; }
        public bool? TipReliefForUpper { get; set; }
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
        public double? SpecificationId { get; set; }
        public DateTime? DueDate { get; set; }
        public string DrawingNumber { get; set; }
        public DateTime? TabletCompletionDate { get; set; }
        public DateTime? ToolCompletionDate { get; set; }
        public string HoldStatus { get; set; }
        public string FilmCoated { get; set; }
        public DateTime? TimeSubmitted { get; set; }
        public string StationId { get; set; }
        public int? InsertState { get; set; }
        public int? Difficulty { get; set; }
        public string OnHoldComment { get; set; }
        public string DwgPicLoc { get; set; }
        public string TabletSubmittedBy { get; set; }
        public DateTime? TabletSubmittedDate { get; set; }
        public string ToolSubmittedBy { get; set; }
        public DateTime? ToolSubmittedDate { get; set; }
        public DateTime? EstimatedCompletion { get; set; }
        public string ToolAssignedTo { get; set; }
        public string ReturnToCsr { get; set; }
        public bool? MultiTipSketch { get; set; }
    }
}
