using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models
{
    public partial class EoiTrackedDocuments
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public int MovementId { get; set; }
        public string User { get; set; }
        public byte[] Timestamp { get; set; }
    }
}
