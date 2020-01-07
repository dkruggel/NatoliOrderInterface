using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiOrderEntryInstructions
    {
        public double QuoteNo { get; set; }
        public short RevNo { get; set; }
        public bool Checked { get; set; }
        public string Instruction { get; set; }
        public string User { get; set; }
        public DateTime TimeEntered { get; set; }
        public DateTime? TimeChecked { get; set; }
    }
}
