using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface
{
    public class EmailAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public EmailAddress(string name, string address)
        {
            Name = name;
            Address = address;
        }
    }
}
