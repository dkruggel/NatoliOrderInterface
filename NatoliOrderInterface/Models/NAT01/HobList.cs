﻿using System;
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

        public bool Equals(HobList other)
        {
            if (other is null)
                return false;

            return this.HobNo == other.HobNo &&
                   this.Shape == other.Shape &&
                   this.DieId == other.DieId &&
                   this.CupDepth == other.CupDepth &&
                   this.Land == other.Land &&
                   this.BisectCode == other.BisectCode &&
                   this.Class == other.Class &&
                   this.DrawingYorN == other.DrawingYorN &&
                   this.CopperYorN == other.CopperYorN &&
                   this.HobYorNorD == other.HobYorNorD &&
                   this.Note1 == other.Note1 &&
                   this.Note2 == other.Note2 &&
                   this.OwnerReservedFor == other.OwnerReservedFor &&
                   this.DateDesigned == other.DateDesigned &&
                   this.ShapeCode == other.ShapeCode &&
                   this.CupCode == other.CupCode &&
                   this.BisectedCode == other.BisectedCode &&
                   this.Radius == other.Radius &&
                   this.Embossed1 == other.Embossed1 &&
                   this.Embossed2 == other.Embossed2 &&
                   this.HobDrawer == other.HobDrawer &&
                   this.MstrDrawer == other.MstrDrawer &&
                   this.DrawingNo == other.DrawingNo &&
                   this.DwgReviseNo == other.DwgReviseNo &&
                   this.Size == other.Size &&
                   this.Note3 == other.Note3 &&
                   this.TempDate == other.TempDate &&
                   this.NewCupDepth == other.NewCupDepth &&
                   this.NewLand == other.NewLand &&
                   this.DrawingType == other.DrawingType &&
                   this.LandRange == other.LandRange &&
                   this.LandBlendedYorN == other.LandBlendedYorN &&
                   this.MeasurableCd == other.MeasurableCd &&
                   this.TipQty == other.TipQty &&
                   this.CircleDiameter == other.CircleDiameter &&
                   this.ProgramNo == other.ProgramNo &&
                   this.BoreCircle == other.BoreCircle &&
                   this.Flush == other.Flush &&
                   this.CupRadius == other.CupRadius &&
                   this.CupRadiusM == other.CupRadiusM &&
                   this.CupDepthM == other.CupDepthM &&
                   this.LandM == other.LandM &&
                   this.LandRangeM == other.LandRangeM &&
                   this.MeasurableCdm == other.MeasurableCdm &&
                   this.Embossing == other.Embossing &&
                   this.Nnumber == other.Nnumber &&
                   this.Dimple == other.Dimple;
        }

        public override bool Equals(object obj) => Equals(obj as HobList);
        public override int GetHashCode() => (
                    HobNo
                   , Shape
                   , DieId
                   , CupDepth
                   , Land
                   , BisectCode
                   , Class
                   , DrawingYorN
                   , CopperYorN
                   , HobYorNorD
                   , Note1
                   , Note2
                   , OwnerReservedFor
                   , DateDesigned
                   , ShapeCode
                   , CupCode
                   , BisectedCode
                   , Radius
                   , Embossed1
                   , Embossed2
                   , HobDrawer
                   , MstrDrawer
                   , DrawingNo
                   , DwgReviseNo
                   , Size
                   , Note3
                   , TempDate
                   , NewCupDepth
                   , NewLand
                   , DrawingType
                   , LandRange
                   , LandBlendedYorN
                   , MeasurableCd
                   , TipQty
                   , CircleDiameter
                   , ProgramNo
                   , BoreCircle
                   , Flush
                   , CupRadius
                   , CupRadiusM
                   , CupDepthM
                   , LandM
                   , LandRangeM
                   , MeasurableCdm
                   , Embossing
                   , Nnumber
                   , Dimple
            ).GetHashCode();
    }
}
