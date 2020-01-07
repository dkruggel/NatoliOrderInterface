using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.Projects
{
    public partial class ChangeLog
    {
        public int LogId { get; set; }
        public string TableName { get; set; }
        public string PrimaryKey1Name { get; set; }
        public string PrimaryKey1Value { get; set; }
        public string PrimaryKey2Name { get; set; }
        public string PrimaryKey2Value { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string UpdatedBy { get; set; }
        public string UpdatedByStation { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }
}
