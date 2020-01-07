using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class MaMachineVariables
    {
        public string WorkOrderNumber { get; set; }
        public string LineType { get; set; }
        public byte LineNumber { get; set; }
        public decimal ColletSize { get; set; }
        public decimal SteelType { get; set; }
        public decimal GrooveProgram { get; set; }
        public int? MultiTipSketchId { get; set; }
        public decimal StockDiameter { get; set; }
        public decimal OverallLength { get; set; }
        public decimal? WorkingLength { get; set; }
        public decimal? WorkingLengthMin { get; set; }
        public decimal? WorkingLengthMax { get; set; }
        public decimal? ProbeCupDepth { get; set; }
        public decimal? CutTipDiameter { get; set; }
        public decimal? NominalTipDiameter { get; set; }
        public decimal? TipWidth { get; set; }
        public decimal? TipReliefDiameter { get; set; }
        public decimal? TipStraight { get; set; }
        public decimal? TipRadius { get; set; }
        public decimal? TipAngle { get; set; }
        public decimal? ChamferAngleTip { get; set; }
        public decimal? ChamferLength { get; set; }
        public decimal? CutBarrelDiameter { get; set; }
        public decimal? FinishBarrelDiameter { get; set; }
        public bool? KeywayYn { get; set; }
        public decimal? KeywayPosition { get; set; }
        public decimal? KeywayLength { get; set; }
        public bool HoldLowBarrelYn { get; set; }
        public decimal? UndercutYn { get; set; }
        public decimal? UndercutDepth { get; set; }
        public decimal? TipReliefYn { get; set; }
        public decimal? GrooveTipReliefYn { get; set; }
        public decimal? HeadType { get; set; }
        public string HeadGaugeNumber { get; set; }
        public decimal? WaycenterYn { get; set; }
        public decimal? HeadFlat { get; set; }
        public decimal? HeadRadius { get; set; }
        public decimal? DomeHeightTopAngle { get; set; }
        public decimal? HeadOd { get; set; }
        public decimal? NeckAngle { get; set; }
        public decimal? HeadThickness { get; set; }
        public decimal? NeckRadius { get; set; }
        public decimal? NeckDiameter { get; set; }
        public decimal? NeckLength { get; set; }
        public decimal? NeckRadiusAtBarrel { get; set; }
        public decimal? ChamferLengthHead { get; set; }
        public decimal? ChamferAngleHead { get; set; }
        public bool? InspectConcentricity { get; set; }
        public bool IssueFixed { get; set; }
    }
}
