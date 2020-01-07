using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ProjectsToClear
    {
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public DateTime? TabletCompletionDate { get; set; }
        public DateTime? ToolCompletionDate { get; set; }
        public string Csr { get; set; }
    }
}
