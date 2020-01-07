using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class SteelType
    {
        public string TypeId { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string UnitofMeasure { get; set; }
        public string SteelPriceCode { get; set; }
        public string PunchMinRc { get; set; }
        public string PunchMaxRc { get; set; }
        public string DieMinRc { get; set; }
        public string DiaMaxRc { get; set; }
        public string DrawingDescription { get; set; }
        public byte? IncludeInBarcodePreHeatTreatReport { get; set; }
        public string BarcodeHeatTreatDisplayGroup { get; set; }
        public byte? BarcodeHeatTreatDisplaySequence { get; set; }
        public string HeatTreatPattern { get; set; }
        public string CryoYesOrNo { get; set; }
    }
}
