using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class OptionsList
    {
        public string OptionCode { get; set; }
        public string OptionDescription { get; set; }
        public string OptionType { get; set; }
        public float OptionPercentage { get; set; }
        public float OptionPrice { get; set; }
        public bool Uppers { get; set; }
        public bool Lowers { get; set; }
        public bool Dies { get; set; }
        public bool Hobs { get; set; }
        public bool Pmisc { get; set; }
        public bool Rejects { get; set; }
        public bool Alignments { get; set; }
        public bool Zdocs { get; set; }
        public bool? DieSegments { get; set; }
        public bool? CoreRod { get; set; }
        public bool? Tm2dataCd { get; set; }
        public bool? Edocs { get; set; }
        public bool? Charge { get; set; }
        public bool? ToolBox { get; set; }
        public string ValueType { get; set; }
        public double Number1 { get; set; }
        public double Number2 { get; set; }
        public float Metric1 { get; set; }
        public float Metric2 { get; set; }
        public string Small { get; set; }
        public string Large { get; set; }
        public short Integer1 { get; set; }
        public short Integer2 { get; set; }
        public string Label { get; set; }
        public string OptionMultiplier { get; set; }
        public string Screw { get; set; }
        public string Color { get; set; }
        public string Vendor { get; set; }
        public string SurfaceTreatment { get; set; }
        public bool? KeyPart { get; set; }
        public bool? LowerAssembly { get; set; }
        public bool? LowerCap { get; set; }
        public bool? LowerHolder { get; set; }
        public bool? LowerHead { get; set; }
        public bool? LowerTip { get; set; }
        public bool? RejectAssembly { get; set; }
        public bool? RejectCap { get; set; }
        public bool? RejectHolder { get; set; }
        public bool? RejectHead { get; set; }
        public bool? RejectTip { get; set; }
        public bool? UpperAssembly { get; set; }
        public bool? UpperCap { get; set; }
        public bool? UpperHolder { get; set; }
        public bool? UpperHead { get; set; }
        public bool? UpperTip { get; set; }
        public bool? LowerCoreRodPunch { get; set; }
        public bool? CoreRodKey { get; set; }
        public bool? CoreRodKeyCollar { get; set; }
        public bool? CopperTablet { get; set; }
        public bool? DieAssembly { get; set; }
        public bool? DieHolder { get; set; }
        public bool? DieInsert { get; set; }
        public bool? DiePlate { get; set; }
        public bool? DieComponent { get; set; }
    }
}
