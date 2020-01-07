using System;
using System.Collections.Generic;

namespace NatoliOrderInterface.Models.NAT01
{
    public partial class QuoteRepresentative
    {
        public string RepId { get; set; }
        public string Name { get; set; }
        public string SignatureFile { get; set; }
        public string EmailAddress { get; set; }
    }
}
