using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class Keys
    {
        public string DrawingNo { get; set; }
        public string CatalogNo { get; set; }
        public float Width { get; set; }
        public float WidthMetric { get; set; }
        public float Length { get; set; }
        public float LengthMetric { get; set; }
        public float Height { get; set; }
        public float HeightMetric { get; set; }
        public short HoleQty { get; set; }
        public float HoleSpacing { get; set; }
        public string Drill { get; set; }
        public string Tap { get; set; }
        public string MachineName { get; set; }
        public string CustomerNo { get; set; }
    }
}
