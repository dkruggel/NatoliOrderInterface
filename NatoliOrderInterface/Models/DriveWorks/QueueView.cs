using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class QueueView
    {
        public string TargetName { get; set; }
        public string Tags { get; set; }
        public DateTime? DateReleased { get; set; }
        public int Priority { get; set; }
        public string DisplayName { get; set; }
    }
}
