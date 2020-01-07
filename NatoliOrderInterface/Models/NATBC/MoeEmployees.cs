using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class MoeEmployees
    {
        public string MoeEmployeeCode { get; set; }
        public int? PayrollEmployeeNumber { get; set; }
        public string EmpType { get; set; }
        public string MoeEmployeeName { get; set; }
        public string MoeDepartmentCode { get; set; }
        public string MoeFirstName { get; set; }
        public string MoeMiddleInitial { get; set; }
        public string MoeLastName { get; set; }
        public string PayrollFullName { get; set; }
        public string MoeJobTitleCode { get; set; }
        public string Schedule { get; set; }
        public string ReportsTo { get; set; }
        public byte InactiveFlag { get; set; }
        public int? DailyProductionGoal { get; set; }
        public int? SaturdayProductionGoal { get; set; }
        public int? WeeklyProductionGoal { get; set; }
        public short? IncludeInBarcodeReports { get; set; }
        public int? MasterScheduleId { get; set; }
        public int? MachineCount { get; set; }
    }
}
