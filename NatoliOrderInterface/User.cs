using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.DriveWorks;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace NatoliOrderInterface
{
    public class User
    {
        public string DomainName
        {
            get;
        }
        private readonly string userName;
        private readonly string dwPrincipalId;
        private readonly string dwDisplayName;
        public string Department
        {
            get;
        }
        public string Password
        {
            get;
            set;
        }
        public string EmployeeCode
        {
            get;
        }

        public List<string> VisiblePanels
        {
            get; set;
        }
        public List<string> Subscribed
        {
            get; set;
        }
        public short? Width { get; set; }
        public short? Height { get; set; }
        public short? Top { get; set; }
        public short? Left { get; set; }
        public float SignatureLeft { get; set; }
        public float SignatureBottom { get; set; }
        public string DepartmentCode { get; set; }
        public bool Maximized { get; set; }
        public short QuoteDays { get; set; }
        public bool FilterActiveProjects { get; set; }
        public string PackageVersion { get; }

        public User()
        {

        }

        /// <summary>
        /// Instance of the current user of the application to house settings and preferences
        /// </summary>
        /// <param name="domainName"></param>
        public User(string domainName)
        {
            using var _nat02context = new NAT02Context();
            EoiSettings settings = _nat02context.EoiSettings.SingleOrDefault(row => row.DomainName.Trim().ToLower() == domainName.Trim().ToLower());
            try
            {
                var version = Windows.ApplicationModel.Package.Current.Id.Version;
                string packageVersion = string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
                if (settings.PackageVersion != packageVersion)
                {
                    settings.PackageVersion = packageVersion;
                    _nat02context.EoiSettings.Update(settings);
                    _nat02context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // IMethods.WriteToErrorLog("User.cs -> Export applications version.", ex.Message, null);
            }
            _nat02context.Dispose();
            DomainName = domainName;
            string deptCode = DomainName.Length == 0 ? "GUEST" : SetUserName();
            EmployeeCode = settings.EmployeeId;
            userName = settings.FullName;
            PackageVersion = settings.PackageVersion;
            using var _driveworksContext = new DriveWorksContext();
            dwDisplayName = userName == "Gregory Lyle" ? "Greg Lyle" : userName == "Nicholas Tarte" ? "Nick Tarte" : userName == "Floyd Smith" ? "Joe Smith" : userName == "Ronald Faltus" ? "Ron Faltus" : userName;
            if (_driveworksContext.SecurityUsers.Any(su => su.DisplayName == dwDisplayName))
            {
                dwPrincipalId = _driveworksContext.SecurityUsers.First(su => su.DisplayName == dwDisplayName).PrincipalId;
            }
            else
            {
                dwDisplayName = "";
                dwPrincipalId = "";
            }
            _driveworksContext.Dispose();
            Subscribed = settings.Subscribed.Split().ToList();
            Width = settings.Width;
            Height = settings.Height;
            Top = settings.Top;
            Left = settings.Left;
            Maximized = settings.Maximized;
            QuoteDays = settings.QuoteDays;
            FilterActiveProjects = settings.FilterActiveProjects;
            DepartmentCode = deptCode;
            if (deptCode == "D1153" || domainName == "pturner" || domainName == "rmouser")
            {
                Department = "Engineering";
            }
            else if (deptCode == "D1151")
            {
                Department = "Order Entry";
            }
            else if (deptCode == "D1133")
            {
                Department = "Barb";
            }
            else
            {
                Department = "Customer Service";
            }
            VisiblePanels = settings.Panels.Split(',').ToList();
            if (domainName == "dkruggel") { SignatureLeft = 950; SignatureBottom = 20; }
            else if (domainName == "twilliams") { SignatureLeft = 958; SignatureBottom = 20; }
            else { SignatureLeft = 0; SignatureBottom = 0; }
            //if (domainName == "dkruggel" || domainName == "twilliams")
            //{
            //    InputBox inputBox = new InputBox("Enter password", "Password");
            //    inputBox.ShowDialog();
            //    Password = inputBox.ReturnString;
            //}
        }

        private string SetUserName()
        {
            using var context = new NATBCContext();
            MoeEmployees emp;
            if (DomainName == "billt")
            {
                emp = context.MoeEmployees.Single(e => e.MoeFirstName == "Bill" && e.MoeLastName == "Turner");
            }
            else
            {
                emp = context.MoeEmployees.Where(e => e.MoeLastName == DomainName.Substring(1) && (e.MoeDepartmentCode == "D1153" ||
                                                                                                   e.MoeDepartmentCode == "D1151" ||
                                                                                                   e.MoeDepartmentCode == "D1149" ||
                                                                                                   e.MoeDepartmentCode == "D1147" ||
                                                                                                   e.MoeDepartmentCode == "D1143" ||
                                                                                                   e.MoeDepartmentCode == "D1133")).FirstOrDefault();
            }
            
            context.Dispose();
            return emp.MoeDepartmentCode.Trim();
        }
        /// <summary>
        /// This returns the full name of the user in the Settings table
        /// </summary>
        /// <returns></returns>
        public string GetUserName()
        {
            return userName;
        }
        public string GetDWPrincipalId()
        {
            return dwPrincipalId;
        }
        public string GetDWDisplayName()
        {
            return dwDisplayName;
        }
    }
}
