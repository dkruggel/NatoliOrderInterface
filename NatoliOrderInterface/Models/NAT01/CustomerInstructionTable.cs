using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class CustomerInstructionTable
    {
        public string CustomerId { get; set; }
        public string Instruction { get; set; }
        public short Sequence { get; set; }
        public bool AutoInclude { get; set; }
        public string Category { get; set; }
        public DateTime EffectiveDate { get; set; }
        public string UserStamp { get; set; }
        public DateTime DateStamp { get; set; }
        public string TimeStamp { get; set; }
        public bool Inactive { get; set; }
        public string OptionCode { get; set; }
        public bool Selected { get; set; }
        public bool? Smiapplied { get; set; }
    }
}
