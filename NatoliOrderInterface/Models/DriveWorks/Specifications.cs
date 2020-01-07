using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class Specifications
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public Guid ProjectId { get; set; }
        public Guid CreatorId { get; set; }
        public Guid EditorId { get; set; }
        public string Name { get; set; }
        public string Directory { get; set; }
        public string MetadataDirectory { get; set; }
        public string OriginalProjectName { get; set; }
        public string OriginalProjectExtension { get; set; }
        public Guid StateId { get; set; }
        public string StateName { get; set; }
        public int StateType { get; set; }
        public bool IsArchived { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateEdited { get; set; }
        public bool IsUniversal { get; set; }
        public bool IsCreatorPermitted { get; set; }
        public bool IsOwnerPermitted { get; set; }
        public string SpecificationProjectExtension { get; set; }
        public string Tags { get; set; }
    }
}
