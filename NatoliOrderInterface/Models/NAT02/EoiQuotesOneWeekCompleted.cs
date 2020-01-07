using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiQuotesOneWeekCompleted
    {
        public double QuoteNo { get; set; }
        public int? QuoteRevNo { get; set; }
        public DateTime? TimeSubmitted { get; set; }
    }
}
