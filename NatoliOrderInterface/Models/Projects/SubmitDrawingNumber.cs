using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class SubmitDrawingNumber
    {
        public string ProjectNumber { get; set; }
        public string RevisionNumber { get; set; }
        public string DrawingNumber { get; set; }
        public string UserName { get; set; }
    }
}
