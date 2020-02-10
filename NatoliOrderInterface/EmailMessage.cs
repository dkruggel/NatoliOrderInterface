using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface
{
    public class EmailMessage
    {
        public EmailMessage()
        {
            ToAddresses = new List<EmailAddress>();
            FromAddresses = new List<EmailAddress>();
            CCAddresses = new List<EmailAddress>();
            BCCAddresses = new List<EmailAddress>();
        }
        public List<EmailAddress> ToAddresses { get; set; }
        public List<EmailAddress> FromAddresses { get; set; }
        public List<EmailAddress> CCAddresses { get; set; }
        public List<EmailAddress> BCCAddresses { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
    }
}
