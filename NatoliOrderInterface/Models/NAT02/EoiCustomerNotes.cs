using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models
{
    public partial class EoiCustomerNotes
    {
        public int ID { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string CustomerNumber { get; set; }
        public string CustomerName { get; set; }
        public string ShipToNumber { get; set; }
        public string ShipToName { get; set; }
        public string EndUserNumber { get; set; }
        public string EndUserName { get; set; }
        public string Category { get; set; }
        public string Note { get; set; }
        public string QuoteNumbers { get; set; }
        public string OrderNumbers { get; set; }
        public DateTime? NotificationDate { get; set; }
    }
}
