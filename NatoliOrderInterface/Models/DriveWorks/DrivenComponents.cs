using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class DrivenComponents : IEquatable<DrivenComponents>
    {
        public Guid Id { get; set; }
        public string TargetName { get; set; }
        public bool Generating { get; set; }
        public bool Generated { get; set; }
        public bool Equals(DrivenComponents other)
        {
            if (other is null)
                return false;

            return this.Generating == other.Generating &&
                   this.Generated == other.Generated;
        }

        public override bool Equals(object obj) => Equals(obj as DrivenComponents);
        public override int GetHashCode() => (Id, TargetName, Generating, Generated).GetHashCode();
    }
}
