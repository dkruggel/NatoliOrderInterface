using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class HobList
    {
        public string HobNo { get; set; }
        public string Shape { get; set; }
        public string DieId { get; set; }
        public float? CupDepth { get; set; }
        public float? Land { get; set; }
        public string BisectCode { get; set; }
        public string Class { get; set; }
        public string DrawingYorN { get; set; }
        public string CopperYorN { get; set; }
        public string HobYorNorD { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string OwnerReservedFor { get; set; }
        public DateTime? DateDesigned { get; set; }
        public string ShapeCode { get; set; }
        public short? CupCode { get; set; }
        public string BisectedCode { get; set; }
        public double? Radius { get; set; }
        public string Embossed1 { get; set; }
        public string Embossed2 { get; set; }
        public short? HobDrawer { get; set; }
        public short? MstrDrawer { get; set; }
        public int? DrawingNo { get; set; }
        public short? DwgReviseNo { get; set; }
        public string Size { get; set; }
        public string Note3 { get; set; }
        public string TempDate { get; set; }
        public short? NewCupDepth { get; set; }
        public short? NewLand { get; set; }
        public string DrawingType { get; set; }
        public float? LandRange { get; set; }
        public string LandBlendedYorN { get; set; }
        public float? MeasurableCd { get; set; }
        public short? TipQty { get; set; }
        public double? CircleDiameter { get; set; }
        public int? ProgramNo { get; set; }
        public float? BoreCircle { get; set; }
        public bool? Flush { get; set; }
        public decimal? CupRadius { get; set; }
        public decimal? CupRadiusM { get; set; }
        public decimal? CupDepthM { get; set; }
        public decimal? LandM { get; set; }
        public decimal? LandRangeM { get; set; }
        public decimal? MeasurableCdm { get; set; }
        public string Embossing { get; set; }
        public int? Nnumber { get; set; }
        public bool? Dimple { get; set; }
    }
}
