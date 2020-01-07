using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class DieList
    {
        public string DieId { get; set; }
        public float? LengthMajorAxis { get; set; }
        public float? WidthMinorAxis { get; set; }
        public float? EndRadius { get; set; }
        public float? CornerRadius { get; set; }
        public float? OutsideDiameter { get; set; }
        public float? SideRadius { get; set; }
        public string Note1 { get; set; }
        public string Note2 { get; set; }
        public string Note3 { get; set; }
        public float? BlendingRadius { get; set; }
        public float? RefOutsideDiameter { get; set; }
        public short? ShapeId { get; set; }
        public string Shape { get; set; }
        public string Size { get; set; }
        public short? MasterDrawer { get; set; }
        public string CupDepth { get; set; }
        public string Land { get; set; }
        public string OwnerReservedFor { get; set; }
        public DateTime? DateDesigned { get; set; }
        public string ShapeCode { get; set; }
        public short? CupCode { get; set; }
        public string BisectedCode { get; set; }
        public double? Radius { get; set; }
        public string Embossed1 { get; set; }
        public string Embossed2 { get; set; }
        public short? HobDrawer { get; set; }
        public short? MasterDrawer2 { get; set; }
        public int? DrawingNo { get; set; }
        public short? DwgReviseNo { get; set; }
        public string TempDate { get; set; }
        public decimal? LengthMajorAxisM { get; set; }
        public decimal? WidthMinorAxisM { get; set; }
        public decimal? EndRadiusM { get; set; }
        public decimal? CornerRadiusM { get; set; }
        public decimal? SideRadiusM { get; set; }
        public decimal? BlendingRadiusM { get; set; }
        public decimal? RadiusM { get; set; }
        public decimal? OutsideDiameterM { get; set; }
        public decimal? RefOutsideDiameterM { get; set; }
        public string PlugGaugeStatus { get; set; }
        public string MasterDieStatus { get; set; }
    }
}
