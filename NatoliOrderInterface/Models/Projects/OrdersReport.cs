using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class OrdersReport
    {
        public int OrdersIn { get; set; }
        public int OrdersOut { get; set; }
        public int OrdersToOffice { get; set; }
        public int TabletProjects { get; set; }
        public int ToolProjects { get; set; }
        public string Employee { get; set; }
    }
}
