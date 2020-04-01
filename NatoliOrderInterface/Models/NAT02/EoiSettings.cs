using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiSettings
    {
        public string EmployeeId { get; set; }
        public string DomainName { get; set; }
        public string FullName { get; set; }
        public string? Subscribed { get; set; }
        public string? Panels { get; set; }
        public short? Width { get; set; }
        public short? Height { get; set; }
        public short? Top { get; set; }
        public short? Left { get; set; }
        public bool Maximized { get; set; }
        public short QuoteDays { get; set; }
        public bool FilterActiveProjects { get; set; }
        public string PackageVersion { get; set; }
        public decimal Zoom { get; set; }
        public short? ModuleRows { get; set; }
    }
}
