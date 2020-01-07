using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class MachineList
    {
        public short MachineNo { get; set; }
        public string Description { get; set; }
        public string SpecialInfo { get; set; }
        public double? Od { get; set; }
        public double? Ol { get; set; }
        public string MachineTypePrCode { get; set; }
        public string UpperSize { get; set; }
        public string LowerSize { get; set; }
        public float? Uweight { get; set; }
        public float? Lweight { get; set; }
        public float? Dweight { get; set; }
        public float? Udiameter { get; set; }
        public float? Ldiameter { get; set; }
        public float? Ddiameter { get; set; }
        public bool? DieSegments { get; set; }
        public short? Stations { get; set; }
        public byte? SegmentQty { get; set; }
    }
}
