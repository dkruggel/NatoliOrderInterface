using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteDetails
    {
        public double QuoteNo { get; set; }
        public short LineNumber { get; set; }
        public short QuantityOrdered { get; set; }
        public short? QuantityShipped { get; set; }
        public string DetailTypeId { get; set; }
        public string HobNoShapeId { get; set; }
        public short? MachineNo { get; set; }
        public string SteelId { get; set; }
        public double? UnitPrice { get; set; }
        public double? ExtendedPrice { get; set; }
        public string PriceOptionsAdded { get; set; }
        public string CompletedYn { get; set; }
        public string Desc1 { get; set; }
        public string Desc2 { get; set; }
        public string Desc3 { get; set; }
        public string Desc4 { get; set; }
        public string Desc5 { get; set; }
        public string Desc6 { get; set; }
        public string Desc7 { get; set; }
        public string Desc8 { get; set; }
        public string HoldFlag { get; set; }
        public short? OptionLastLine { get; set; }
        public float? BasePrice { get; set; }
        public float? OptionsIncrements { get; set; }
        public float? OptionsPercentage { get; set; }
        public string MachinePriceCode { get; set; }
        public string SteelPriceCode { get; set; }
        public string ShapePriceCode { get; set; }
        public bool? UnitPriceOverride { get; set; }
        public short? RemakeQty { get; set; }
        public bool? TaxFlag { get; set; }
        public float? LineTaxes { get; set; }
        public string SheetColor { get; set; }
        public string PrintStatus { get; set; }
        public short? Revision { get; set; }
        public short? Sequence { get; set; }
        public short? TipQty { get; set; }
        public short? DieShapeId { get; set; }
        public float? DieMinorDiameter { get; set; }
        public float? DieMajorDiameter { get; set; }
        public bool? FinishedGood { get; set; }
        public float? BoreCircle { get; set; }
    }
}
