using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class QueueView : IEquatable<QueueView>
    {
        public string TargetName { get; set; }
        public string Tags { get; set; }
        public DateTime? DateReleased { get; set; }
        public int Priority { get; set; }
        public string DisplayName { get; set; }
        public bool Equals(QueueView other)
        {
            if (other is null)
                return false;

            return this.TargetName == other.TargetName &&
                   this.Tags == other.Tags &&
                   this.DateReleased == other.DateReleased &&
                   this.Priority == other.Priority &&
                   this.DisplayName == other.DisplayName;
        }

        public override bool Equals(object obj) => Equals(obj as QueueView);
        public override int GetHashCode() => (TargetName, Tags, DateReleased, Priority, DisplayName).GetHashCode();
    }
}
