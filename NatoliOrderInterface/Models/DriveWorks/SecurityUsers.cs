using System;
using System.Collections.Generic;
using System.Text;

namespace NatoliOrderInterface.Models.DriveWorks
{
    public partial class SecurityUsers
    {
        public Guid Id { get; set; }
        public string ProviderName { get; set; }
        public string PrincipalId { get; set; }
        public string PrincipalToken { get; set; }
        public bool IsEnabled { get; set; }
        public bool IsTeamLeader { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
    }
}
