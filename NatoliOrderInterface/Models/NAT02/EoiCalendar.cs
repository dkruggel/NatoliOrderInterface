using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiCalendar
    {
        public Int16 Year { get; set; }
        public Byte Month { get; set; }
        public Byte Day { get; set; }
        public string Notes { get; set; }
        public string DomainName { get; set; }
        public DateTime? ActionDateTime { get; set; }
    }
}
