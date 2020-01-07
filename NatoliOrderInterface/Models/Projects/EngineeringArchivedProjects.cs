﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class EngineeringArchivedProjects
    {
        public string ProjectNumber { get; set; }
        public string RevNumber { get; set; }
        public DateTime? TimeArchived { get; set; }
        public bool? ArchivedFromCheck { get; set; }
        public bool? ArchivedFromCancel { get; set; }
        public string ArchivedBy {get; set;}
        public string QuoteNumber { get; set; }
        public string QuoteRevNumber { get; set; }
        public string RefProjectNumber { get; set; }
        public string RefProjectRevNumber { get; set; }
        public string RefQuoteNumber { get; set; }
        public string RefQuoteRevNumber { get; set; }
        public string RefOrderNumber { get; set; }
        public string CSR { get; set; }
        public string ReturnToCSR { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string ShipToNumber { get; set; }
        public string ShipToLocNumber { get; set; }
        public string ShipToName { get; set; }
        public string EndUserNumber { get; set; }
        public string EndUserLocNumber { get; set; }
        public string EndUserName { get; set; }
        public string UnitOfMeasure { get; set; }
        public string Product { get; set; }
        public string Attention { get; set; }
        public string MachineNumber { get; set; }
        public string DieNumber { get; set; }
        public string DieShape { get; set; }
        public decimal? Width { get; set; }
        public decimal? Length { get; set; }
        public string UpperHobNumber { get; set; }
        public string UpperHobDescription { get; set; }
        public short? UpperCupType { get; set; }
        public decimal? UpperCupDepth { get; set; }
        public decimal? UpperLand { get; set; }
        public string LowerHobNumber { get; set; }
        public string LowerHobDescription { get; set; }
        public short? LowerCupType { get; set; }
        public decimal? LowerCupDepth { get; set; }
        public decimal? LowerLand { get; set; }
        public string ShortRejectHobNumber { get; set; }
        public string ShortRejectHobDescription { get; set; }
        public short? ShortRejectCupType { get; set; }
        public decimal? ShortRejectCupDepth { get; set; }
        public decimal? ShortRejectLand { get; set; }
        public string LongRejectHobNumber { get; set; }
        public string LongRejectHobDescription { get; set; }
        public short? LongRejectCupType { get; set; }
        public decimal? LongRejectCupDepth { get; set; }
        public decimal? LongRejectLand { get; set; }
        public string UpperTolerances { get; set; }
        public string LowerTolerances { get; set; }
        public string ShortRejectTolerances { get; set; }
        public string LongRejectTolerances { get; set; }
        public string DieTolerances { get; set; }
        public string Notes { get; set; }
        public DateTime TimeSubmitted { get; set; }
        public DateTime DueDate { get; set; }
        public bool Priority { get; set; }
        public bool TabletStarted { get; set; }
        public DateTime? TabletStartedDateTime { get; set; }
        public string TabletStartedBy { get; set; }
        public bool TabletDrawn { get; set; }
        public DateTime? TabletDrawnDateTime { get; set; }
        public string TabletDrawnBy { get; set; }
        public bool TabletSubmitted { get; set; }
        public DateTime? TabletSubmittedDateTime { get; set; }
        public string TabletSubmittedBy { get; set; }
        public bool TabletChecked { get; set; }
        public DateTime? TabletCheckedDateTime { get; set; }
        public string TabletCheckedBy { get; set; }
        public bool ToolStarted { get; set; }
        public DateTime? ToolStartedDateTime { get; set; }
        public string ToolStartedBy { get; set; }
        public bool ToolDrawn { get; set; }
        public DateTime? ToolDrawnDateTime { get; set; }
        public string ToolDrawnBy { get; set; }
        public bool ToolSubmitted { get; set; }
        public DateTime? ToolSubmittedDateTime { get; set; }
        public string ToolSubmittedBy { get; set; }
        public bool ToolChecked { get; set; }
        public DateTime? ToolCheckedDateTime { get; set; }
        public string ToolCheckedBy { get; set; }
        public bool NewDrawing { get; set; }
        public bool UpdateExistingDrawing { get; set; }
        public bool UpdateTextOnDrawing { get; set; }
        public bool PerSampleTablet { get; set; }
        public bool RefTabletDrawing { get; set; }
        public bool PerSampleTool { get; set; }
        public bool RefToolDrawing { get; set; }
        public bool PerSuppliedPicture { get; set; }
        public bool RefNatoliDrawing { get; set; }
        public bool RefNonNatoliDrawing { get; set; }
        public bool MultiTipSketch { get; set; }
        public string MultiTipSketchID { get; set; }
        public byte NumberOfTips { get; set; }
        public string BinLocation { get; set; }
        public bool MultiTipSolid { get; set; }
        public bool MultiTipAssembled { get; set; }
        public bool OnHold { get; set; }
        public string OnHoldComment { get; set; }
        public DateTime? OnHoldDateTime { get; set; }
        public string RevisedBy { get; set; }
        public string Changes { get; set; }
    }
}
