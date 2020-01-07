using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class ShapeFields
    {
        public short ShapeID { get; set; }
        public string ShapeDescription { get; set; }
        public bool OutsideDiameter { get; set; }
        public bool Length { get; set; }
        public bool Width { get; set; }
        public bool EndRadius { get; set; }
        public bool BlendingRadius { get; set; }
        public bool SideRadius { get; set; }
        public bool CornerRadius { get; set; }
        public bool RefOutsideDiameter { get; set; }
        public string ShapeIdPriceCode { get; set; }
        public float HobFee { get; set; }
        public string TM2ID { get; set; }
    }
}
