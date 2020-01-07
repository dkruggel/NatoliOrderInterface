using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiProjectsFinished
    {
        public int ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string Csr { get; set; }
    }
}
