﻿using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class TabletSubmittedBy
    {
        public int? ProjectNumber { get; set; }
        public int? RevisionNumber { get; set; }
        public string TabletSubmittedBy1 { get; set; }
        public DateTime? TimeSubmitted { get; set; }
    }
}
