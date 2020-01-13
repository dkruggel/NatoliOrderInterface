using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.DriveWorks;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.Projects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
// using WpfAnimatedGif;
//using XamlAnimatedGif;
using Colors = System.Windows.Media.Colors;

namespace NatoliOrderInterface
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        #region Declarations
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("USER32.DLL", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);
        public string connectionString;
        private bool _panelLoading;
        private string _panelMainMessage = "Main Loading Message";
        private string _panelSubMessage = "Sub Loading Message";


        const string QUOTE_PATH = @"\\nsql03\data1\Quotes\";
        const string ORDER_PATH = @"\\nsql03\data1\WorkOrders\";
        readonly Dictionary<string, string> headers = new Dictionary<string, string>() {
            { "BeingEntered", "Orders Being Entered" },
            { "InTheOffice", "Orders In The Office" },
            { "EnteredUnscanned", "Orders Not Started In Engineering" },
            { "InEngineering", "Orders Being Processed In Engineering" },
            { "ReadyToPrint", "Orders Ready To Be Printed" },
            { "PrintedInEngineering", "Orders Printed And Ready For Production" },
            { "QuotesNotConverted", "Quotes Not Converted To An Order" },
            { "QuotesToConvert", "Quotes Ready To Be Converted" },
            { "AllTabletProjects", "All Tablet Projects" },
            { "TabletProjectsNotStarted", "Tablet Projects Not Started" },
            { "TabletProjectsStarted", "Tablet Projects Started" },
            { "TabletProjectsDrawn", "Tablet Projects Drawn" },
            { "TabletProjectsSubmitted", "Tablet Projects Submitted" },
            { "TabletProjectsOnHold", "Tablet Projects On Hold" },
            { "AllToolProjects", "All Tool Projects" },
            { "ToolProjectsNotStarted", "Tool Projects Not Started" },
            { "ToolProjectsStarted", "Tool Projects Started" },
            { "ToolProjectsDrawn", "Tool Projects Drawn" },
            { "ToolProjectsOnHold", "Tool Projects On Hold" },
            { "DriveWorksQueue", "Driveworks Models In Queue" },
            { "NatoliOrderList", "Natoli Order List" }
        };

        public User User
        {
            get; set;
        }
        private WorkOrder workOrder;
        private Quote quote;
        readonly List<string> originalProps;
        private readonly System.Timers.Timer mainTimer = new System.Timers.Timer();
        private readonly System.Timers.Timer quoteTimer = new System.Timers.Timer();
        private readonly System.Timers.Timer oqTimer = new System.Timers.Timer();
        private readonly System.Timers.Timer NatoliOrderListTimer = new System.Timers.Timer();
        private int _projectNumber = 0;
        private int? _revNumber = 0;
        private double _quoteNumber = 0;
        private int? _quoteRevNumber = 0;
        private bool quotesCompletedChanged = false;
        private int quotesCompletedCount = 0;
        private double _orderNumber = 0;
        private bool _filterProjects = false;
        private List<(string, CheckBox)> selectedOrders = new List<(string, CheckBox)>();
        private List<string> selectedLineItems = new List<string>();
        private List<(string, string, CheckBox)> selectedProjects = new List<(string, string, CheckBox)>();
        private List<(string, string, CheckBox)> selectedQuotes = new List<(string, string, CheckBox)>();

        NAT01Context _nat01context = new NAT01Context();
        public string ChildWindow { get; set; }
        public event EventHandler RemovedFromSelectedOrders;
        protected virtual void OnRemovedFromSelectedOrders(EventArgs e)
        {
            EventHandler handler = RemovedFromSelectedOrders;
            handler?.Invoke(this, e);
        }
        public delegate void SomeBoolChangedEvent();
        public event SomeBoolChangedEvent UpdatedFromChild;
        private Boolean _boolValue;
        public Boolean BoolValue
        {
            get { return _boolValue; }
            set
            {
                _boolValue = value;
                if (UpdatedFromChild != null)
                {
                    UpdatedFromChild();
                }
            }
        }

        #region View Dictionaries
        Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, string background, string foreground, string fontWeight)> quotesNotConvertedDict;
        Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> quotesToConvertDict;
        Dictionary<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)> ordersBeingEnteredDict;
        Dictionary<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)> ordersInTheOfficeDict;
        Dictionary<double, (string customerName, int daysToShip, string background, string foreground, string fontWeight)> ordersEnteredUnscannedDict;
        Dictionary<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)> ordersInEngineeringUnprintedDict;
        Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> ordersReadyToPrintDict;
        Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> ordersPrintedInEngineeringDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsNotStartedDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsStartedDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsDrawnDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsSubmittedDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsOnHoldDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> allTabletProjectsDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsNotStartedDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsStartedDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsDrawnDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsOnHoldDict;
        Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> allToolProjectsDict;
        Dictionary<string, (string releasedBy, string tag, string releaseTime, int priority)> driveWorksQueueDict;
        Dictionary<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string background)> natoliOrderListDict;
        List<object> dictList;
        #endregion

        Dictionary<string, string> oeDetailTypes = new Dictionary<string, string>() { { "U", "Upper" }, { "L", "Lower" }, { "D", "Die" }, { "DS", "Die" }, { "R", "Reject" }, { "A", "Alignment" } };
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            App.GetConnectionString();
            UpdatedFromChild = MainRefresh;
            try
            {
                User = new User(Environment.UserName);
                // User = new User("pturner");
            }
            catch
            {
                User = new User("");
            }
            Width = (double)User.Width;
            Height = (double)User.Height;
            Top = (double)User.Top;
            Left = (double)User.Left;
            Title = "Natoli Order Interface";
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { GetPercentages(); }
            this.Show();
            originalProps = new List<string>();
            dictList = new List<object>();
            foreach (string s in User.VisiblePanels)
            {
                originalProps.Add(s);
            }
            ConstructModules();
            BuildMenus();
            // ProjectWindow projectWindow = new ProjectWindow("110000", "0", this, User, false);
            // MainMenu.Background = SystemParameters.WindowGlassBrush; // Sets it to be the same color as the accent color in Windows
            InitializingMenuItem.Visibility = Visibility.Collapsed;
            mainTimer.Elapsed += MainTimer_Elapsed;
            mainTimer.Interval = User.Department == "Engineering" ? 0.5 * (60 * 1000) : 5 * (60 * 1000); // 0.5 or 5 minutes
            mainTimer.Enabled = true;
            quoteTimer.Elapsed += QuoteTimer_Elapsed;
            quoteTimer.Interval = 5 * (60 * 1000); // 5 minutes
            quoteTimer.Enabled = true;
            NatoliOrderListTimer.Elapsed += NatoliOrderListTimer_Elapsed;
            NatoliOrderListTimer.Interval = 15 * (60 * 1000); // 15 minutes
            NatoliOrderListTimer.Enabled = true;
            oqTimer.Elapsed += OQTimer_Elapsed;
            oqTimer.Interval = 2 * (60 * 1000); // 2 minutes
            oqTimer.Enabled = true;
        }

        /// <summary>
        /// Takes List<string> of Driveworks.SecurityUsers.PrincipalId's for to, cc, and bcc
        /// </summary>
        /// <param name="to"></param>
        /// <param name="cc"></param>
        /// <param name="bcc"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <param name="priority"></param>
        public static void SendEmail(List<string> to, List<string> cc = null, List<string> bcc = null, string subject = "", string body = "", List<string> attachments = null, MailPriority priority = MailPriority.Normal)
        {
            SmtpClient smtpServer = new SmtpClient();
            MailMessage mail = new MailMessage();
            try
            {
                smtpServer.Port = 25;
                smtpServer.Host = "192.168.1.186";
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("AutomatedEmail@natoli.com");
                if (to != null)
                {
                    foreach (string recipient in to)
                    {
                        mail.To.Add(GetEmailAddressFromDWFirstName(recipient));
                    }
                }
                if (cc != null)
                {
                    foreach (string recipient in cc)
                    {
                        mail.CC.Add(GetEmailAddressFromDWFirstName(recipient));
                    }
                }
                if (bcc != null)
                {
                    foreach (string recipient in bcc)
                    {
                        mail.Bcc.Add(GetEmailAddressFromDWFirstName(recipient));
                    }
                }
                if (attachments != null)
                {
                    foreach (string path in attachments)
                    {
                        Attachment attachment = new Attachment(path);
                        mail.Attachments.Add(attachment);
                    }
                }
                mail.Subject = subject;
                mail.Body = body;
                mail.Priority = priority;
                smtpServer.Send(mail);
                smtpServer.Dispose();
                mail.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            smtpServer.Dispose();
            mail.Dispose();
        }
        public static void BringProcessToFront(string processName)
        {
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            try
            {
                // Get process Name
                process = System.Diagnostics.Process.GetProcessesByName(processName)[0];
            }
            catch
            {
                process.Dispose();
                return;
            }
            process.WaitForInputIdle(5000); // wait 5 seconds

            // Get a handle of the process.
            IntPtr handle = FindWindow(null, process.MainWindowTitle);

            // Verify that it is a running process.
            if (handle == IntPtr.Zero)
            {
                process.Dispose();
                return;
            }

            // Make it the foreground application
            SetForegroundWindow(handle);
            process.Dispose();
        }
        private void MainRefresh()
        {
            BindData("Main");
            BindData("QuotesNotConverted");
            BindData("NatoliOrderList");

            mainTimer.Stop();
            mainTimer.Start();
            quoteTimer.Stop();
            quoteTimer.Start();
            NatoliOrderListTimer.Stop();
            NatoliOrderListTimer.Start();
        }
        public static string GetEmailAddress(string userName)
        {
            try
            {
                switch (userName)
                {
                    case "GREGORY":
                        userName = "Greg";
                        return "intlcs1@natoli.com";
                    case "NICHOLAS":
                        userName = "Nick";
                        return "intlcs1@natoli.com";
                    default:
                        break;
                }
                using var _driveworksContext = new DriveWorksContext();
                return _driveworksContext.SecurityUsers.Where(u => u.DisplayName.Contains(userName)).FirstOrDefault().EmailAddress;
            }
            catch (Exception eSql)
            {
                MessageBox.Show("Error resolving email address.\n" + eSql.Message);
                return null;
            }
        }
        /// <summary>
        /// Takes the PrincipalId of the Driveworks.SecurityUsers table and returns the EmailAddress.
        /// </summary>
        /// <param name="dWfirstName"></param>
        /// <returns></returns>
        public static string GetEmailAddressFromDWFirstName(string dWfirstName)
        {
            try
            {
                using var _driveworksContext = new DriveWorksContext();
                switch (dWfirstName)
                {
                    case "Greg":
                        _driveworksContext.Dispose();
                        return "intlcs1@natoli.com";
                    case "Nick":
                        _driveworksContext.Dispose();
                        return "intlcs3@natoli.com";
                    default:
                        if (_driveworksContext.SecurityUsers.Any(su => su.PrincipalId.Trim() == dWfirstName.Trim()))
                        {
                            string fullName = _driveworksContext.SecurityUsers.First(su => su.PrincipalId.Trim() == dWfirstName.Trim()).EmailAddress.Trim();
                            _driveworksContext.Dispose();
                            return fullName;
                        }
                        else
                        {
                            _driveworksContext.Dispose();
                            return "";
                        }
                }
            }
            catch //(Exception ex)
            {
                return "";
                //MessageBox.Show(ex.Message);
            }

        }
        public static void CreateZipFile(string inputDirectory, string outputZipFile)
        {
            System.IO.Compression.ZipFile.CreateFromDirectory(inputDirectory, outputZipFile, 0, false);
        }
        public static void SendProjectCompletedEmailToCSR(List<string> CSRs, string _projectNumber, string _revNo)
        {
            SmtpClient smtpServer = new SmtpClient();
            MailMessage mail = new MailMessage();
            try
            {
                // Send email
                smtpServer.Port = 25;
                smtpServer.Host = "192.168.1.186";
                mail.IsBodyHtml = true;
                mail.From = new MailAddress("AutomatedEmail@natoli.com");
                if (CSRs != null)
                {
                    foreach (string CSR in CSRs)
                    {
                        mail.To.Add(GetEmailAddressFromDWFirstName(CSR));
                    }
                    //mail.Bcc.Add("eng6@natoli.com");
                    //mail.Bcc.Add("eng5@natoli.com");
                    string projectNumber = _projectNumber ?? "";
                    string revNo = _revNo ?? "";
                    mail.Subject = "Project# " + projectNumber.Trim() + "-" + revNo.Trim() + " Completed";
                    string filesForCustomerDirectory = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + @"\FILES_FOR_CUSTOMER\";
                    string zipFile = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + @"\FILES_FOR_CUSTOMER.zip";
                    if (System.IO.Directory.Exists(filesForCustomerDirectory))
                    {
                        CreateZipFile(filesForCustomerDirectory, zipFile);
                        //string[] files = System.IO.Directory.GetFiles(filesForCustomerDirectory);

                        //foreach (string file in files)
                        //{
                        //    string fileName = file.Substring(file.LastIndexOf(@"\") + 1, file.Length - file.LastIndexOf(@"\") - 1);
                        //    Attachment attachment = new Attachment(file);
                        //    mail.Attachments.Add(attachment);
                        //}

                        Attachment attachment = new Attachment(zipFile);
                        mail.Attachments.Add(attachment);
                    }
                    mail.IsBodyHtml = true;
                    mail.Body = "Dear " + CSRs.First() + ",<br><br>" +

                    @"Project# <a href=&quot;\\engserver\workstations\TOOLING%20AUTOMATION\Project%20Specifications\" + projectNumber + @"\&quot;>" + projectNumber + " </a> is completed and ready to be viewed.<br> " +
                    "The drawings for the customer are attached.<br><br>" +
                    "Thanks,<br>" +
                    "Engineering Team<br><br><br>" +


                    "This is an automated email and not monitored by any person(s).";
                    smtpServer.Send(mail);
                    smtpServer.Dispose();
                    mail.Dispose();
                    System.IO.File.Delete(zipFile);
                    //MessageBox.Show("Message sent to CSR.");
                }
                else
                {
                    MessageBox.Show("List of strings 'CSRs' was null.");
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.Message);
                // WriteToErrorLog("SendEmailToCSR", ex.Message);
            }
            smtpServer.Dispose();
            mail.Dispose();
        }
        public delegate void RemovedFromSelectedOrdersEventHandler(object sender, EventArgs e);

        #region Main Window Events
        private void GridWindow_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void GridWindow_ContentRendered(object sender, EventArgs e)
        {
            ConstructExpanders();
            BindData("Main");
            BindData("QuotesNotConverted");
            BindData("NatoliOrderList");
        }
        private void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BindData("Main");
        }
        private void QuoteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BindData("QuotesNotConverted");
        }
        private void NatoliOrderListTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BindData("NatoliOrderList");
        }
        private void OQTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { GetPercentages(); }
            if (User.Department == "Engineering")
            {
                QuotesAndOrders();
            }
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
        }
        private void GridWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using var context = new NAT02Context();
            try
            {
                EoiSettings eoiSettings = context.EoiSettings.Where(s => s.EmployeeId == User.EmployeeCode).First();
                eoiSettings.Width = (short?)Width;
                eoiSettings.Height = (short?)Height;
                eoiSettings.Top = (short?)Top;
                eoiSettings.Left = (short?)Left;
                context.EoiSettings.Update(eoiSettings);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("GridWindow_Closing - Save Settings", ex.Message);
            }
            context.Dispose();

            if (System.Diagnostics.Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetEntryAssembly().Location)).Length < 2)
            {
                using var nat02context = new NAT02Context();
                try
                {
                    if (nat02context.EoiOrdersBeingChecked.Where(o => o.User == User.GetUserName()).Any())
                    {
                        var ordersBeingChecked = nat02context.EoiOrdersBeingChecked.Where(o => o.User == User.GetUserName());
                        nat02context.EoiOrdersBeingChecked.RemoveRange(ordersBeingChecked);
                        nat02context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    WriteToErrorLog("GridWindow_Closing - Remove from OrdersBeingChecked", ex.Message);
                }
                nat02context.Dispose();
            }
            Dispose();
        }
        private void GridWindow_Activated(object sender, EventArgs e)
        {
            if (Height < 200) { Height = 250; }
            if (Width < 150) { Width = 200; }
            if (mainTimer.Enabled == false) { mainTimer.Start(); }
            if (quoteTimer.Enabled == false) { quoteTimer.Start(); }
            if (NatoliOrderListTimer.Enabled == false) { NatoliOrderListTimer.Start(); }
        }
        private void GridWindow_Deactivated(object sender, EventArgs e)
        {
        }
        private void GridWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                mainTimer.Stop();
                quoteTimer.Stop();
                NatoliOrderListTimer.Stop();
            }
            else if (WindowState != WindowState.Minimized)
            {
                MainRefresh();
                mainTimer.Start();
                quoteTimer.Start();
                NatoliOrderListTimer.Start();
            }
        }
        #endregion

        #region MenuStuff
        public void BuildMenus()
        {
            MainMenu.Items.Clear();

            #region FileMenuRegion
            MenuItem fileMenu = new MenuItem
            {
                Header = "File",
                Height = MainMenu.Height
            };
            MenuItem createProject = new MenuItem()
            {
                Header = "Create Project"
            };
            MenuItem projectSearch = new MenuItem()
            {
                Header = "Project Search"
            };
            MenuItem forceRefresh = new MenuItem
            {
                Header = "Force Refresh"
            };
            MenuItem editLayout = new MenuItem
            {
                Header = "Edit Layout"
            };
            MenuItem checkMissingVariables = new MenuItem
            {
                Header = "Missing Automation Info"
            };
            MenuItem filterProjects = new MenuItem
            {
                Header = "Filter Projects"
            };
            MenuItem printDrawings = new MenuItem
            {
                Header = "Print Drawings"
            };
            createProject.Click += CreateProject_Click;
            projectSearch.Click += ProjectSearch_Click;
            forceRefresh.Click += ForceRefresh_Click;
            editLayout.Click += EditLayout_Click;
            checkMissingVariables.Click += CheckMissingVariables_Click;
            filterProjects.Click += FilterProjects_Click;
            printDrawings.Click += PrintDrawings_Click;
            // if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.Department == "Customer Service") { fileMenu.Items.Add(createProject); }
            fileMenu.Items.Add(projectSearch);
            fileMenu.Items.Add(forceRefresh);
            fileMenu.Items.Add(editLayout);
            if (User.Department == "Engineering") { fileMenu.Items.Add(checkMissingVariables); }
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { fileMenu.Items.Add(filterProjects); }
            if (User.Department == "Engineering" && !User.GetUserName().StartsWith("Phyllis")) { fileMenu.Items.Add(printDrawings); }
            MainMenu.Items.Add(fileMenu);
            #endregion
            #region SubsMenuRegion
            MenuItem subsMenu = new MenuItem();
            subsMenu.Header = "Subscriptions";
            subsMenu.Height = MainMenu.Height;
            subsMenu.SubmenuClosed += Subscriptions_SubmenuClosed;
            using var natbccontext = new NATBCContext();
            using var nat02context = new NAT02Context();
            List<string> csrList = natbccontext.MoeEmployees.Where(e => (e.MoeDepartmentCode == "D1149" || e.MoeDepartmentCode == "D1147" || e.MoeDepartmentCode == "D1143")
                                                                         && e.InactiveFlag == 0 && e.MoeEmployeeCode != "E3167" && e.MoeEmployeeCode != "E4840"
                                                                         && e.MoeEmployeeCode != "E4345" && e.MoeEmployeeCode != "E5047" && e.MoeEmployeeCode != "E5347")
                                                            .Select(e => e.MoeFirstName.Trim()).ToList();
            IQueryable<string> subList = nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                                 .Select(e => e.Subscribed);
            string[] subs = subList.First().Split(',');
            foreach (string CSR in csrList)
            {
                MenuItem menuItem = new MenuItem();
                menuItem.Name = CSR;
                menuItem.Header = CSR;
                menuItem.IsCheckable = true;
                if (subs.Contains(CSR))
                {
                    menuItem.IsChecked = true;
                }
                else
                {
                    menuItem.IsChecked = false;
                }
                subsMenu.Items.Add(menuItem);
            }
            MainMenu.Items.Add(subsMenu);
            #endregion
            #region LegendRegion
            MenuItem legendMenu = new MenuItem
            {
                Header = "Legend",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            MenuItem orderRushMenu = new MenuItem
            {
                Header = "RUSH",
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.DarkRed),
                FontWeight = FontWeights.Bold
            };
            MenuItem quoteRushMenu = new MenuItem
            {
                Header = "RUSH",
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.DarkRed),
                FontWeight = FontWeights.Bold
            };
            MenuItem priorityMenu = new MenuItem
            {
                Header = "Priority",
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.DarkRed),
                FontWeight = FontWeights.Bold
            };
            MenuItem tabletIsToolProjectMenu = new MenuItem
            {
                Header = "Tablet/Tools",
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Oblique
            };
            MenuItem doNotProcessMenu = new MenuItem
            {
                Header = "Do Not Process",
                Background = new SolidColorBrush(Colors.Pink),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem readyToCheckMenu = new MenuItem
            {
                Header = "Ready To Check",
                Background = new SolidColorBrush(Colors.GreenYellow),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem beingLookedAtMenu = new MenuItem
            {
                Header = "Being Looked At",
                Background = new SolidColorBrush(Colors.DodgerBlue),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem missingVariablesMenu = new MenuItem
            {
                Header = "Missing Automation Variables",
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem missingPrintsMenu = new MenuItem
            {
                Header = "Missing Prints",
                Background = new SolidColorBrush(Colors.MediumPurple),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem driveworksErrorsMenu = new MenuItem
            {
                Header = "DriveWorks Error -- Did Not Run",
                Background = new SolidColorBrush(Colors.Black),
                Foreground = new SolidColorBrush(Colors.White),
                FontWeight = FontWeights.Normal
            };
            MenuItem lateProjectMenu = new MenuItem
            {
                Header = "Late Project",
                Background = new SolidColorBrush(Colors.Red),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem sentToEngineeringProjectMenu = new MenuItem
            {
                Header = "Sent to Engineering",
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF")),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem startedProjectMenu = new MenuItem
            {
                Header = "Project Started",
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF")),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem drawnProjectMenu = new MenuItem
            {
                Header = "Project Drawn",
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#52A3FF")),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem submittedProjectMenu = new MenuItem
            {
                Header = "Project Submitted",
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0A7DFF")),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem checkedProjectMenu = new MenuItem
            {
                Header = "Project Checked",
                Background = new SolidColorBrush(Colors.GreenYellow),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem onHoldProjectMenu = new MenuItem
            {
                Header = "Project On Hold",
                Background = new SolidColorBrush(Colors.MediumPurple),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem toolStillTabletProjectMenu = new MenuItem
            {
                Header = "Still on Tablet Side",
                Background = new SolidColorBrush(Colors.Yellow),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem toolMultiTipSketchMenu = new MenuItem
            {
                Header = "Multi Tip Sketch",
                Background = new SolidColorBrush(Colors.Gray),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem followupQuoteCheckMenu = new MenuItem
            {
                Header = "Quote needs follow up",
                Background = new SolidColorBrush(Colors.Pink),
                Foreground = new SolidColorBrush(Colors.Black),
                FontWeight = FontWeights.Normal
            };
            MenuItem ordersHeader = new MenuItem
            {
                Header = "***Orders***",
                IsEnabled = false,
                Background = new SolidColorBrush(Colors.Yellow),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            MenuItem quotesHeader = new MenuItem
            {
                Header = "***Quotes***",
                IsEnabled = false,
                Background = new SolidColorBrush(Colors.Yellow),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            MenuItem projectsHeader = new MenuItem
            {
                Header = "***Projects***",
                IsEnabled = false,
                Background = new SolidColorBrush(Colors.Yellow),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E4509")
            {
                MainMenu.Items.Add(legendMenu);
                legendMenu.Items.Add(ordersHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(orderRushMenu);
                legendMenu.Items.Add(doNotProcessMenu);
                legendMenu.Items.Add(readyToCheckMenu);
                legendMenu.Items.Add(beingLookedAtMenu);
                legendMenu.Items.Add(missingVariablesMenu);
                legendMenu.Items.Add(missingPrintsMenu);
                legendMenu.Items.Add(driveworksErrorsMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(projectsHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(tabletIsToolProjectMenu);
                legendMenu.Items.Add(priorityMenu);
                legendMenu.Items.Add(lateProjectMenu);
                legendMenu.Items.Add(sentToEngineeringProjectMenu);
                legendMenu.Items.Add(startedProjectMenu);
                legendMenu.Items.Add(drawnProjectMenu);
                legendMenu.Items.Add(submittedProjectMenu);
                legendMenu.Items.Add(checkedProjectMenu);
                legendMenu.Items.Add(onHoldProjectMenu);
                legendMenu.Items.Add(toolStillTabletProjectMenu);
                legendMenu.Items.Add(toolMultiTipSketchMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quotesHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quoteRushMenu);
            }
            else if (User.Department == "Engineering")
            {
                MainMenu.Items.Add(legendMenu);
                legendMenu.Items.Add(ordersHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(orderRushMenu);
                legendMenu.Items.Add(doNotProcessMenu);
                legendMenu.Items.Add(readyToCheckMenu);
                legendMenu.Items.Add(beingLookedAtMenu);
                legendMenu.Items.Add(missingVariablesMenu);
                legendMenu.Items.Add(missingPrintsMenu);
                legendMenu.Items.Add(driveworksErrorsMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(projectsHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(tabletIsToolProjectMenu);
                legendMenu.Items.Add(priorityMenu);
                legendMenu.Items.Add(lateProjectMenu);
                legendMenu.Items.Add(sentToEngineeringProjectMenu);
                legendMenu.Items.Add(startedProjectMenu);
                legendMenu.Items.Add(drawnProjectMenu);
                legendMenu.Items.Add(submittedProjectMenu);
                legendMenu.Items.Add(checkedProjectMenu);
                legendMenu.Items.Add(onHoldProjectMenu);
                legendMenu.Items.Add(toolStillTabletProjectMenu);
                legendMenu.Items.Add(toolMultiTipSketchMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quotesHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quoteRushMenu);
                legendMenu.Items.Add(followupQuoteCheckMenu);
            }
            else if (User.Department == "Order Entry")
            {
                MainMenu.Items.Add(legendMenu);
                legendMenu.Items.Add(ordersHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(orderRushMenu);
                legendMenu.Items.Add(doNotProcessMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quotesHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quoteRushMenu);
            }
            else if (User.Department == "Barb")
            {
                MainMenu.Items.Add(legendMenu);
                legendMenu.Items.Add(ordersHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(orderRushMenu);
                legendMenu.Items.Add(doNotProcessMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quotesHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quoteRushMenu);
                legendMenu.Items.Add(followupQuoteCheckMenu);
            }
            else
            {
                MainMenu.Items.Add(legendMenu);
                legendMenu.Items.Add(ordersHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(orderRushMenu);
                legendMenu.Items.Add(doNotProcessMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(projectsHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(tabletIsToolProjectMenu);
                legendMenu.Items.Add(priorityMenu);
                legendMenu.Items.Add(sentToEngineeringProjectMenu);
                legendMenu.Items.Add(startedProjectMenu);
                legendMenu.Items.Add(drawnProjectMenu);
                legendMenu.Items.Add(submittedProjectMenu);
                legendMenu.Items.Add(checkedProjectMenu);
                legendMenu.Items.Add(onHoldProjectMenu);
                legendMenu.Items.Add(toolStillTabletProjectMenu);
                legendMenu.Items.Add(toolMultiTipSketchMenu);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quotesHeader);
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(new Separator());
                legendMenu.Items.Add(quoteRushMenu);
                legendMenu.Items.Add(followupQuoteCheckMenu);
            }

            #endregion
            #region RightClickRegion
            MenuItem startOrder = new MenuItem
            {
                Header = "Start"
            };
            #endregion
        }

        private void PrintDrawings_Click(object sender, RoutedEventArgs e)
        {
            //if (User.EmployeeCode == "E4408")
            //{
            //    foreach (string file in System.IO.Directory.GetFiles(@"C:\Users\" + User.DomainName + @"\Desktop\WorkOrdersToPrint\").AsEnumerable().OrderBy(f => f))
            //    {
            //        Acrobat.AcroAVDoc doc = new Acrobat.AcroAVDoc();
            //        doc.Open(file, "");
            //        Acrobat.CAcroPDDoc tempDoc = (Acrobat.CAcroPDDoc)doc.GetPDDoc();
            //        doc.PrintPagesSilentEx(1, tempDoc.GetNumPages(), 0, 0, 1, 0, 0, 0, 0);
            //        tempDoc.Close();
            //    }
            //}
            //else
            //{
            //    //PdfPrinter.PrintHelper printHelper = new PdfPrinter.PrintHelper().PrintHelperFactory();
            //    //string ret = printHelper.PrintAllFiles();
            //}
            //PdfPrinter.PrintHelper printHelper = new PdfPrinter.PrintHelper().PrintHelperFactory();
            //string ret = printHelper.PrintAllFiles();

            MessageBoxResult res = MessageBox.Show("Delete pdfs?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
            switch (res)
            {
                case MessageBoxResult.Yes:
                    foreach (string file in System.IO.Directory.GetFiles(@"C:\Users\" + User.DomainName + @"\Desktop\WorkOrdersToPrint\").AsEnumerable().OrderBy(f => f))
                    {
                        System.IO.File.Delete(file);
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                default:
                    break;
            }
        }

        private void DeleteMachineVariables(string orderNo, int lineNumber = 0)
        {
            if (lineNumber != 0)
            {
                using var _nat02Context = new NAT02Context();
                _nat02Context.MaMachineVariables.RemoveRange(_nat02Context.MaMachineVariables.Where(m => m.WorkOrderNumber.Trim() == orderNo && m.LineNumber == lineNumber));
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
            }
            else
            {
                using var _nat02Context = new NAT02Context();
                _nat02Context.MaMachineVariables.RemoveRange(_nat02Context.MaMachineVariables.Where(m => m.WorkOrderNumber.Trim() == orderNo));
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
            }
        }

        private void BarcodeTransfer(string employeeCode, string departmentCode, string travellerNumber, bool firstScan = false)
        {
            if (firstScan)
            {
                string lineNumber = travellerNumber.Substring(1, 2);
                string strSQL = "Update [NAT01].[dbo].[OrderDetails] set [TravellerNo] = '" + travellerNumber + "' where [OrderNo] = '" + workOrder.OrderNumber + "00' and [LineNumber] = '" + lineNumber + "'";
                SqlConnection updateConnection = new SqlConnection();
                SqlCommand updateCommand = new SqlCommand();
                try
                {
                    updateCommand.Connection = updateConnection;
                    updateCommand.CommandText = strSQL;
                    updateCommand.CommandType = CommandType.Text;
                    using (updateConnection)
                    {
                        updateConnection.ConnectionString = "Data Source=NSQL05;Initial Catalog=NATBC;Persist Security Info=True;User ID=BarcodeUser;Password=PrivateKey(0)";
                        updateConnection.Open();
                        DataTable TransferBatch = new DataTable();
                        TransferBatch.Load(updateCommand.ExecuteReader());
                        TransferBatch.Dispose();
                    }
                    updateCommand.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    updateCommand.Dispose();
                    updateConnection.Close();
                }
            }

            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = con;
                cmd.CommandText = "spValidateScanTransfer";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Terminal ID
                cmd.Parameters.Add("@TerminalID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@TerminalID"].Size = 50;
                cmd.Parameters["@TerminalID"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@TerminalID"].Value = Environment.MachineName;

                // NTUserID
                cmd.Parameters.Add("@NTUserID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@NTUserID"].Size = 35;
                cmd.Parameters["@NTUserID"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@NTUserID"].Value = Environment.MachineName;

                // ApplicationVersion
                cmd.Parameters.Add("@ApplicationVersion", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@ApplicationVersion"].Size = 25;
                cmd.Parameters["@ApplicationVersion"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@ApplicationVersion"].Value = "V3.4";

                // Browser Details
                cmd.Parameters.Add("@BrowserDetails", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@BrowserDetails"].Size = 75;
                cmd.Parameters["@BrowserDetails"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@BrowserDetails"].Value = "";

                // ActionPerformed
                cmd.Parameters.Add("@ActionPerformed", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@ActionPerformed"].Size = 75;
                cmd.Parameters["@ActionPerformed"].Direction = System.Data.ParameterDirection.Output;

                // EmployeeCode
                cmd.Parameters.Add("@EmployeeCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@EmployeeCode"].Size = 7;
                cmd.Parameters["@EmployeeCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@EmployeeCode"].Value = employeeCode;

                // CardCode
                cmd.Parameters.Add("@CardCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@CardCode"].Size = 7;
                cmd.Parameters["@CardCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@CardCode"].Value = "";


                // EmployeeValidationCode
                cmd.Parameters.Add("@EmployeeValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@EmployeeValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // EmployeeValidationText
                cmd.Parameters.Add("@EmployeeValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@EmployeeValidationText"].Size = 75;
                cmd.Parameters["@EmployeeValidationText"].Direction = System.Data.ParameterDirection.Output;

                // DepartmentCode
                cmd.Parameters.Add("@DepartmentCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@DepartmentCode"].Size = 7;
                cmd.Parameters["@DepartmentCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@DepartmentCode"].Value = departmentCode;

                // DepartmentValidationCode
                cmd.Parameters.Add("@DepartmentValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@DepartmentValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // DepartmentValidationText
                cmd.Parameters.Add("@DepartmentValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@DepartmentValidationText"].Size = 75;
                cmd.Parameters["@DepartmentValidationText"].Direction = System.Data.ParameterDirection.Output;


                // TravellerNo
                cmd.Parameters.Add("@TravellerNo", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@TravellerNo"].Size = 11;
                cmd.Parameters["@TravellerNo"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@TravellerNo"].Value = travellerNumber;

                // WorkOrderNumber
                cmd.Parameters.Add("@WorkOrderNumber", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@WorkOrderNumber"].Size = 11;
                cmd.Parameters["@WorkOrderNumber"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@WorkOrderNumber"].Value = "W" + travellerNumber.Substring(3, 6) + "S0" + travellerNumber.Substring(1, 2);

                // WorkOrderNumberValidated
                cmd.Parameters.Add("@WorkOrderNumberValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@WorkOrderNumberValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // WorkOrderNumberValidationText
                cmd.Parameters.Add("@WorkOrderNumberValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@WorkOrderNumberValidationText"].Size = 75;
                cmd.Parameters["@WorkOrderNumberValidationText"].Direction = System.Data.ParameterDirection.Output;

                // BatchID
                cmd.Parameters.Add("@BatchID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@BatchID"].Size = 27;
                cmd.Parameters["@BatchID"].Direction = System.Data.ParameterDirection.InputOutput;
                cmd.Parameters["@BatchID"].Value = "none";


                // SaveValidationCode
                cmd.Parameters.Add("@SaveValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@SaveValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // SaveValidationText
                cmd.Parameters.Add("@SaveValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@SaveValidationText"].Size = 75;
                cmd.Parameters["@SaveValidationText"].Direction = System.Data.ParameterDirection.Output;

                using (con)
                {
                    con.ConnectionString = "Data Source=NSQL05;Initial Catalog=NATBC;Persist Security Info=True;User ID=BarcodeUser;Password=PrivateKey(0)";
                    con.Open();
                    DataTable TransferBatch = new DataTable();
                    TransferBatch.Load(cmd.ExecuteReader());
                    TransferBatch.Dispose();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
        }

        #region Clicks
        private void StartTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        // Get project revision number
                        // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                        string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;

                        // Insert into StartedBy

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            ProjectWindow.StartProject(project.Item1, project.Item2, "TABLETS", User);
                        }
                        else
                        {
                            ProjectStartedTablet tabletProjectStarted = new ProjectStartedTablet();
                            tabletProjectStarted.ProjectNumber = int.Parse(project.Item1);
                            tabletProjectStarted.RevisionNumber = int.Parse(project.Item2);
                            tabletProjectStarted.TimeSubmitted = DateTime.Now;
                            tabletProjectStarted.ProjectStartedTablet1 = User.GetUserName().Split(' ')[0] == "Floyd" ? "Joe" :
                                                                         User.GetUserName().Split(' ')[0] == "Ronald" ? "Ron" :
                                                                         User.GetUserName().Split(' ')[0] == "Phyllis" ? new InputBox("Drafter?", "Whom?", this).ReturnString : User.GetUserName().Split(' ')[0];
                            _projectsContext.ProjectStartedTablet.Add(tabletProjectStarted);

                            // Drive specification transition name to "Started - Tablets"
                            // Auto archive project specification
                            string _name = project.Item1 + (int.Parse(project.Item2) > 0 ? "_" + project.Item2 : "");
                            Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                            spec.StateName = "Started - Tablets";
                            _driveworksContext.Specifications.Update(spec);
                        }

                        _projectsContext.SaveChanges();
                        _driveworksContext.SaveChanges();
                        _projectsContext.Dispose();
                        _driveworksContext.Dispose();

                        // Email CSR
                        // SendEmailToCSR(_csr, _projectNumber.ToString());
                        
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        WriteToErrorLog("StartTabletProject_CLick", ex.Message);
                    }
                }
                MainRefresh();
            }
        }

        private void FinishTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            ProjectWindow.DrawProject(project.Item1, project.Item2, "TABLETS", User);
                        }
                        else
                        {
                            // Get project revision number
                            // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                            string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;

                            // Insert into CheckedBy
                            TabletDrawnBy tabletDrawnBy = new TabletDrawnBy();
                            tabletDrawnBy.ProjectNumber = int.Parse(project.Item1);
                            tabletDrawnBy.RevisionNumber = int.Parse(project.Item2);
                            tabletDrawnBy.TimeSubmitted = DateTime.Now;
                            tabletDrawnBy.TabletDrawnBy1 = User.GetUserName().Split(' ')[0] == "Floyd" ? "Joe" :
                                                           User.GetUserName().Split(' ')[0] == "Ronald" ? "Ron" :
                                                           User.GetUserName().Split(' ')[0] == "Phyllis" ? new InputBox("Drafter?", "Whom?", this).ReturnString : User.GetUserName().Split(' ')[0];
                            _projectsContext.TabletDrawnBy.Add(tabletDrawnBy);

                            // Drive specification transition name to "Drawn - Tablets"
                            // Auto archive project specification
                            string _name = project.Item1 + (int.Parse(project.Item2) > 0 ? "_" + project.Item2 : "");
                            Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                            spec.StateName = "Drawn - Tablets";
                            _driveworksContext.Specifications.Update(spec);
                        }
                        _projectsContext.SaveChanges();
                        _driveworksContext.SaveChanges();
                        _projectsContext.Dispose();
                        _driveworksContext.Dispose();

                        // Email CSR
                        // SendEmailToCSR(_csr, _projectNumber.ToString());
                        
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        WriteToErrorLog("FinishTabletProject_Click", ex.Message);
                    }
                }
                MainRefresh();
            }
        }

        private void SubmitTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i< count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            ProjectWindow.SubmitProject(project.Item1, project.Item2, "TABLETS", User);
                        }
                        else
                        {
                            // Get project revision number
                            // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                            //string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;

                            // Insert into CheckedBy
                            TabletSubmittedBy tabletSubmittedBy = new TabletSubmittedBy();
                            tabletSubmittedBy.ProjectNumber = int.Parse(project.Item1);
                            tabletSubmittedBy.RevisionNumber = int.Parse(project.Item2);
                            tabletSubmittedBy.TimeSubmitted = DateTime.Now;
                            tabletSubmittedBy.TabletSubmittedBy1 = User.GetUserName().Split(' ')[0] == "Floyd" ? "Joe" :
                                                                   User.GetUserName().Split(' ')[0] == "Ronald" ? "Ron" : User.GetUserName().Split(' ')[0];
                            _projectsContext.TabletSubmittedBy.Add(tabletSubmittedBy);

                            // Drive specification transition name to "Submitted - Tablets"
                            // Auto archive project specification

                            string _name = project.Item1 + (int.Parse(project.Item2) > 0 ? "_" + project.Item2 : "");
                            if (_driveworksContext.Specifications.Any(s => s.Name == _name))
                            {
                                Specifications spec = _driveworksContext.Specifications.First(s => s.Name == _name);
                                spec.StateName = "Submitted - Tablets";
                                _driveworksContext.Specifications.Update(spec);
                            }
                            else
                            {
                                MessageBox.Show(
                                    "It appears there is not a specification matching this Project Number and Revision Number in the Specifications table.\n" +
                                    "Perhaps it is in a save state.",
                                    "No Specification by that Name", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }

                        _projectsContext.SaveChanges();
                        _driveworksContext.SaveChanges();
                        _projectsContext.Dispose();
                        _driveworksContext.Dispose();

                        // Email CSR
                        // SendEmailToCSR(_csr, _projectNumber.ToString());
                        
                    }
                    catch (Exception ex)
                    {
                        //MessageBox.Show(ex.Message);
                        WriteToErrorLog("SubmitTabletProject_Click", ex.Message);
                    }
                }
                selectedProjects.Clear();
                MainRefresh();
            }
        }

        private void OnHoldTabletProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Uncheck project expander
                selectedProjects.First(p => p.Item1 == _projectNumber.ToString() && p.Item2 == _revNumber.ToString()).Item3.IsChecked = false;

                OnHoldCommentWindow orderInfoWindow = new OnHoldCommentWindow("Tablets", _projectNumber, _revNumber, this, User)
                {
                    Left = Left,
                    Top = Top
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("OnHoldTabletProject_Click", ex.Message);
            }
        }

        private void OffHoldTabletProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Uncheck project expander
                selectedProjects.First(p => p.Item1 == _projectNumber.ToString() && p.Item2 == _revNumber.ToString()).Item3.IsChecked = false;

                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();

                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == _projectNumber.ToString() && p.RevNumber == _revNumber.ToString()))
                {
                    ProjectWindow.TakeProjectOffHold(_projectNumber.ToString(), _revNumber.ToString());
                }
                else
                {
                    if (_projectsContext.HoldStatus.Any(p => p.ProjectNumber == _projectNumber.ToString() && p.RevisionNumber == _revNumber.ToString()))
                    {
                        HoldStatus holdStatus = _projectsContext.HoldStatus.Where(p => p.ProjectNumber == _projectNumber.ToString() && p.RevisionNumber == _revNumber.ToString()).First();
                        holdStatus.HoldStatus1 = "OFF HOLD";
                        holdStatus.TimeSubmitted = DateTime.Now;
                        _projectsContext.HoldStatus.Update(holdStatus);
                    }
                    else
                    {
                        // Insert into HoldStatus
                        HoldStatus holdStatus = new HoldStatus();
                        holdStatus.ProjectNumber = _projectNumber.ToString();
                        holdStatus.RevisionNumber = _revNumber.ToString();
                        holdStatus.TimeSubmitted = DateTime.Now;
                        holdStatus.HoldStatus1 = "OFF HOLD";
                        _projectsContext.HoldStatus.Add(holdStatus);
                    }

                    // Drive specification transition name to "Off Hold - Tablets"
                    string _name = _projectNumber.ToString() + (_revNumber > 0 ? "_" + _revNumber : "");
                    Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                    spec.StateName = "Off Hold - Tablets";
                    _driveworksContext.Specifications.Update(spec);
                }

                _projectsContext.SaveChanges();
                _driveworksContext.SaveChanges();
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
                MainRefresh();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("OffHoldTabletProject_Click", ex.Message);
            }
        }

        private void CompleteTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _nat02Context = new NAT02Context();

                        EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First();
                        _nat02Context.EoiProjectsFinished.Remove(projectsFinished);

                        _nat02Context.SaveChanges();
                        _nat02Context.Dispose();
                        selectedProjects.Clear();
                        
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        WriteToErrorLog("CompleteTabletProject_Click", ex.Message);
                    }
                }
                MainRefresh();
            }
        }

        private void CheckTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();
                        using var _nat02Context = new NAT02Context();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            ProjectWindow.CheckProject(project.Item1, project.Item2, "TABLETS", User);
                        }
                        else
                        {
                            // Get project revision number
                            // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;

                            // Insert into CheckedBy
                            TabletCheckedBy tabletCheckedBy = new TabletCheckedBy();
                            tabletCheckedBy.ProjectNumber = int.Parse(project.Item1);
                            tabletCheckedBy.RevisionNumber = int.Parse(project.Item2);
                            tabletCheckedBy.TimeSubmitted = DateTime.Now;
                            tabletCheckedBy.TabletCheckedBy1 = User.GetUserName().Split(' ')[0];
                            _projectsContext.TabletCheckedBy.Add(tabletCheckedBy);

                            // Insert into ProjectsFinished  (Now a Trigger)
                            //if (!(bool)_tools)
                            //{
                            // using var _nat02Context = new NAT02Context();
                            //    EoiProjectsFinished finished = new EoiProjectsFinished();
                            //    finished.ProjectNumber = _projectNumber;
                            //    finished.RevisionNumber = _revNumber;
                            //    finished.Csr = _csr;
                            //    _nat02Context.EoiProjectsFinished.Add(finished);
                            //    _nat02Context.SaveChanges();
                            //}

                            // Drive specification transition name to "Completed"
                            // Auto archive project specification
                            string _name = project.Item1 + (int.Parse(project.Item2) > 0 ? "_" + project.Item2 : "");
                            bool? _tools = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Tools;
                            Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                            spec.StateName = _tools == true ? "Sent to Tools" : "Completed";
                            spec.IsArchived = (bool)!_tools;
                            _driveworksContext.Specifications.Update(spec);



                            //Send Email To CSR
                            if (!(bool)_tools)
                            {
                                List<string> _CSRs = new List<string>();
                                
                                if (!string.IsNullOrEmpty(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).Csr))
                                { 
                                    _CSRs.Add(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).Csr);
                                }
                                if (!string.IsNullOrEmpty(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ReturnToCsr))
                                {
                                    _CSRs.Add(_projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ReturnToCsr);
                                }
                                SendProjectCompletedEmailToCSR(_CSRs, project.Item1, project.Item2);
                            }
                            // Save pending changes
                            _projectsContext.SaveChanges();
                            _driveworksContext.SaveChanges();
                        }
                        // Dispose of contexts
                        _projectsContext.Dispose();
                        _driveworksContext.Dispose();
                        _nat02Context.Dispose();
                        
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        WriteToErrorLog("CheckTabletProject_Click", ex.Message);
                    }
                }
                MainRefresh();
            }
        }

        private void CancelTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {

                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();
                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        ProjectWindow.CheckProject(project.Item1, project.Item2, "TABLETS", User);
                    }
                    else
                    {

                        MessageBoxResult res = MessageBox.Show("Are you sure you want to cancel project# " + int.Parse(project.Item1) + "_" + int.Parse(project.Item2) + "?");
                        if (res == MessageBoxResult.Yes)
                        {
                            try
                            {
                                // Uncheck project expander
                                project.Item3.IsChecked = false;

                                

                                if (_projectsContext.HoldStatus.Any(p => p.ProjectNumber == project.Item1 && p.RevisionNumber == project.Item2))
                                {
                                    // Update data in HoldStatus
                                    HoldStatus holdStatus = _projectsContext.HoldStatus.Where(p => p.ProjectNumber == project.Item1 && p.RevisionNumber == project.Item2).First();
                                    holdStatus.HoldStatus1 = "CANCELLED";
                                    holdStatus.TimeSubmitted = DateTime.Now;
                                    holdStatus.OnHoldComment = "";
                                    _projectsContext.HoldStatus.Update(holdStatus);
                                }
                                else
                                {
                                    // Insert into HoldStatus
                                    HoldStatus holdStatus = new HoldStatus();
                                    holdStatus.ProjectNumber = project.Item1;
                                    holdStatus.RevisionNumber = project.Item2;
                                    holdStatus.TimeSubmitted = DateTime.Now;
                                    holdStatus.HoldStatus1 = "CANCELLED";
                                    holdStatus.OnHoldComment = "";
                                    _projectsContext.HoldStatus.Add(holdStatus);
                                }

                                // Drive specification transition name to "On Hold - " projectType
                                string _name = project.Item1 + (Convert.ToInt32(project.Item2) > 0 ? "_" + project.Item2 : "");
                                Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                                spec.StateName = "Cancelled - Tablets";
                                spec.IsArchived = true;
                                _driveworksContext.Specifications.Update(spec);

                                _projectsContext.SaveChanges();
                                _driveworksContext.SaveChanges();
                                
                            }
                            catch (Exception ex)
                            {
                                // MessageBox.Show(ex.Message);
                                WriteToErrorLog("SetOnHold", ex.Message);
                            }
                        }
                    }
                    _projectsContext.Dispose();
                    _driveworksContext.Dispose();
                }
                MainRefresh();
            }
        }

        private void StartToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int _count = selectedProjects.Count;
                for (int i = 0; i < _count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            ProjectWindow.StartProject(project.Item1, project.Item2, "TOOLS", User);
                        }
                        else
                        {
                            // Get project revision number
                            // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                            string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;
                            string usrName = User.GetUserName().Split(" ")[0];
                            int count = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectStartedTool == usrName && string.IsNullOrEmpty(p.ToolDrawnBy) &&
                                                                                string.IsNullOrEmpty(p.ToolCheckedBy) && p.HoldStatus != "CANCELED" && !p.HoldStatus.Contains("ON HOLD") &&
                                                                                p.ProjectsId > 80000).Count();
                            if (false) //(count > 5)
                            {
                                MessageBox.Show(
                                    "Maximum simultaneous projects limit reached.\n" +
                                    "Please finish a project before starting more.");
                            }
                            else
                            {
                                // Insert into CheckedBy
                                ProjectStartedTool toolProjectStarted = new ProjectStartedTool();
                                toolProjectStarted.ProjectNumber = int.Parse(project.Item1);
                                toolProjectStarted.RevisionNumber = int.Parse(project.Item2);
                                toolProjectStarted.TimeSubmitted = DateTime.Now;
                                toolProjectStarted.ProjectStartedTool1 = User.GetUserName().Split(' ')[0];
                                _projectsContext.ProjectStartedTool.Add(toolProjectStarted);

                                // Drive specification transition name to "Started - Tools"
                                // Auto archive project specification
                                string _name = int.Parse(project.Item1).ToString() + (int.Parse(project.Item2) > 0 ? "_" + int.Parse(project.Item2) : "");
                                Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                                spec.StateName = "Started - Tools";
                                _driveworksContext.Specifications.Update(spec);

                                _projectsContext.SaveChanges();
                                _driveworksContext.SaveChanges();
                                _projectsContext.Dispose();
                                _driveworksContext.Dispose();

                                // Email CSR
                                // SendEmailToCSR(_csr, _projectNumber.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        WriteToErrorLog("StartToolProject_Click", ex.Message);
                    }
                }
                selectedProjects.Clear();
                MainRefresh();
            }

        }

        private void FinishToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    try
                    {
                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();


                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            ProjectWindow.DrawProject(project.Item1, project.Item2, "TOOLS", User);
                        }
                        else
                        {
                            // Get project revision number
                            // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                            string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;

                            // Insert into CheckedBy
                            ToolDrawnBy toolDrawnBy = new ToolDrawnBy();
                            toolDrawnBy.ProjectNumber = int.Parse(project.Item1);
                            toolDrawnBy.RevisionNumber = int.Parse(project.Item2);
                            toolDrawnBy.TimeSubmitted = DateTime.Now;
                            toolDrawnBy.ToolDrawnBy1 = User.GetUserName().Split(' ')[0];
                            _projectsContext.ToolDrawnBy.Add(toolDrawnBy);

                            // Drive specification transition name to "Drawn - Tools"
                            // Auto archive project specification
                            string _name = int.Parse(project.Item1).ToString() + (int.Parse(project.Item2) > 0 ? "_" + int.Parse(project.Item2) : "");
                            Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                            spec.StateName = "Drawn - Tools";
                            _driveworksContext.Specifications.Update(spec);
                        }

                        _projectsContext.SaveChanges();
                        _driveworksContext.SaveChanges();
                        _projectsContext.Dispose();
                        _driveworksContext.Dispose();
                        // Email CSR
                        // SendEmailToCSR(_csr, _projectNumber.ToString());
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        WriteToErrorLog("FinishToolProject_Click", ex.Message);
                    }
                }
                selectedProjects.Clear();
                MainRefresh();
            }
        }

        private void CheckToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    using var _nat02Context = new NAT02Context();
                    bool alreadyThere = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).Any();
                    _nat02Context.Dispose();

                    if (!alreadyThere)
                    {
                        try
                        {
                            // Uncheck project expander
                            project.Item3.IsChecked = false;

                            using var _projectsContext = new ProjectsContext();
                            using var _driveworksContext = new DriveWorksContext();


                            if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                            {
                                ProjectWindow.CheckProject(project.Item1, project.Item2, "TOOLS", User);
                            }
                            else
                            {
                                // Get project revision number
                                // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;


                                // Insert into CheckedBy
                                ToolCheckedBy toolCheckedBy = new ToolCheckedBy();
                                toolCheckedBy.ProjectNumber = int.Parse(project.Item1);
                                toolCheckedBy.RevisionNumber = int.Parse(project.Item2);
                                toolCheckedBy.TimeSubmitted = DateTime.Now;
                                toolCheckedBy.ToolCheckedBy1 = User.GetUserName().Split(' ')[0];
                                _projectsContext.ToolCheckedBy.Add(toolCheckedBy);

                                // Drive specification transition name to "Completed"
                                // Auto archive project specification
                                string _name = int.Parse(project.Item1).ToString() + (int.Parse(project.Item2) > 0 ? "_" + int.Parse(project.Item2) : "");
                                Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                                spec.StateName = "Completed";
                                spec.IsArchived = true;
                                _driveworksContext.Specifications.Update(spec);

                                //Send Email To CSR
                                List<string> _CSRs = new List<string>();
                                _CSRs.Add(_projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr);
                                if (!string.IsNullOrEmpty(_projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr))
                                {
                                    _CSRs.Add(_projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr);
                                }
                                SendProjectCompletedEmailToCSR(_CSRs, int.Parse(project.Item1).ToString(), int.Parse(project.Item2).ToString());

                            }
                            // Save pending changes
                            _projectsContext.SaveChanges();
                            _driveworksContext.SaveChanges();


                            // Dispose of contexts
                            _projectsContext.Dispose();
                            _driveworksContext.Dispose();
                        }
                        catch (Exception ex)
                        {
                            // MessageBox.Show(ex.Message);
                            WriteToErrorLog("CheckToolProject_Click", ex.Message);
                        }
                    }
                }
                selectedProjects.Clear();
                MainRefresh();
            }
        }

        private void OnHoldToolProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnHoldCommentWindow orderInfoWindow = new OnHoldCommentWindow("Tools", _projectNumber, _revNumber, this, User)
                {
                    Left = Left,
                    Top = Top
                };
            }
            catch (Exception ex)
            {
                WriteToErrorLog("OnHoldToolProject_Click", ex.Message);
            }
        }

        private void OffHoldToolProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Uncheck project expander
                selectedProjects.First(p => p.Item1 == _projectNumber.ToString() && p.Item2 == _revNumber.ToString()).Item3.IsChecked = false;

                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();

                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == _projectNumber.ToString() && p.RevNumber == _revNumber.ToString()))
                {
                    ProjectWindow.TakeProjectOffHold(_projectNumber.ToString(), _revNumber.ToString());
                }
                else
                {
                    if (_projectsContext.HoldStatus.Any(p => p.ProjectNumber == _projectNumber.ToString() && p.RevisionNumber == _revNumber.ToString()))
                    {
                        HoldStatus holdStatus = _projectsContext.HoldStatus.Where(p => p.ProjectNumber == _projectNumber.ToString() && p.RevisionNumber == _revNumber.ToString()).First();
                        holdStatus.HoldStatus1 = "OFF HOLD";
                        _projectsContext.HoldStatus.Update(holdStatus);
                    }
                    else
                    {
                        // Insert into HoldStatus
                        HoldStatus holdStatus = new HoldStatus();
                        holdStatus.ProjectNumber = _projectNumber.ToString();
                        holdStatus.RevisionNumber = _revNumber.ToString();
                        holdStatus.TimeSubmitted = DateTime.Now;
                        holdStatus.HoldStatus1 = "OFF HOLD";
                        _projectsContext.HoldStatus.Add(holdStatus);
                    }

                    // Drive specification transition name to "Off Hold - Tools"
                    string _name = _projectNumber.ToString() + (_revNumber > 0 ? "_" + _revNumber : "");
                    Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                    spec.StateName = "Off Hold - Tools";
                    _driveworksContext.Specifications.Update(spec);
                }

                _projectsContext.SaveChanges();
                _driveworksContext.SaveChanges();
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
                MainRefresh();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("OffHoldToolProject_Click", ex.Message);
            }
        }

        private void CompleteToolProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Uncheck project expander
                selectedProjects.First(p => p.Item1 == _projectNumber.ToString() && p.Item2 == _revNumber.ToString()).Item3.IsChecked = false;

                using var _nat02Context = new NAT02Context();

                if (selectedProjects.Any())
                {
                    //foreach ((string, string, CheckBox) project in selectedProjects)
                    int count = selectedProjects.Count;
                    for (int i = 0; i < count; i++)
                    {
                        (string, string, CheckBox) project = selectedProjects[0];
                        EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First();
                        _nat02Context.EoiProjectsFinished.Remove(projectsFinished);
                    }
                }
                else
                {
                    EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).First();
                    _nat02Context.EoiProjectsFinished.Remove(projectsFinished);
                }

                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                selectedProjects.Clear();
                MainRefresh();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("CompleteToolProject_Click", ex.Message);
            }
        }

        private void CancelToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = selectedProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) project = selectedProjects[0];
                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        ProjectWindow.CheckProject(project.Item1, project.Item2, "TOOLS", User);
                    }
                    else
                    {
                        MessageBoxResult res = MessageBox.Show("Are you sure you want to cancel project# " + project.Item1 + "_" + project.Item2 + "?");
                        if (res == MessageBoxResult.Yes)
                        {
                            try
                            {
                                // Uncheck project expander
                                project.Item3.IsChecked = false;

                                if (_projectsContext.HoldStatus.Any(p => p.ProjectNumber == project.Item1 && p.RevisionNumber == project.Item2))
                                {
                                    // Update data in HoldStatus
                                    HoldStatus holdStatus = _projectsContext.HoldStatus.Where(p => p.ProjectNumber == project.Item1 && p.RevisionNumber == project.Item2).First();
                                    holdStatus.HoldStatus1 = "CANCELLED";
                                    holdStatus.TimeSubmitted = DateTime.Now;
                                    holdStatus.OnHoldComment = "";
                                    _projectsContext.HoldStatus.Update(holdStatus);
                                }
                                else
                                {
                                    // Insert into HoldStatus
                                    HoldStatus holdStatus = new HoldStatus();
                                    holdStatus.ProjectNumber = project.Item1;
                                    holdStatus.RevisionNumber = project.Item2;
                                    holdStatus.TimeSubmitted = DateTime.Now;
                                    holdStatus.HoldStatus1 = "CANCELLED";
                                    holdStatus.OnHoldComment = "";
                                    _projectsContext.HoldStatus.Add(holdStatus);
                                }

                                // Drive specification transition name to "On Hold - " projectType
                                string _name = project.Item1 + (Convert.ToInt32(project.Item2) > 0 ? "_" + project.Item2 : "");
                                Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                                spec.StateName = "Cancelled - Tools";
                                spec.IsArchived = true;
                                _driveworksContext.Specifications.Update(spec);

                                _projectsContext.SaveChanges();
                                _driveworksContext.SaveChanges();
                            }
                            catch (Exception ex)
                            {
                                // MessageBox.Show(ex.Message);
                                WriteToErrorLog("SetOnHold", ex.Message);
                            }
                        }
                    }
                    _projectsContext.Dispose();
                    _driveworksContext.Dispose();
                }
                MainRefresh();
            }
        }

        private void DoNotProcessMenuItem_Click(object sender, RoutedEventArgs e)
        {
            bool doNotProc = false;
            DataGrid dataGrid = (DataGrid)sender;
            DataGridCellInfo cell = dataGrid.SelectedCells[0];
            string orderNumber = ((TextBlock)cell.Column.GetCellContent(cell.Item)).Text;
            using var nat02context = new NAT02Context();

            doNotProc = nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == double.Parse(orderNumber)).Any();

            if (doNotProc)
            {
                EoiOrdersDoNotProcess p = new EoiOrdersDoNotProcess() { OrderNo = double.Parse(orderNumber) };
                nat02context.EoiOrdersDoNotProcess.Add(p);
                nat02context.SaveChanges();
            }
            else
            {
                EoiOrdersDoNotProcess p = new EoiOrdersDoNotProcess() { OrderNo = double.Parse(orderNumber), UserName = User.GetUserName() };
                nat02context.EoiOrdersDoNotProcess.Remove(p);
                nat02context.SaveChanges();
            }
            doNotProc = !doNotProc;
            MainRefresh();
        }

        private void SendToOfficeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Scan selected line items first and then clear the list
            if (selectedLineItems.Any())
            {
                foreach (string travellerNumber in selectedLineItems)
                {
                    BarcodeTransfer(User.EmployeeCode, "D080", travellerNumber);
                }
                selectedLineItems.Clear();
            }

            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                foreach ((string, CheckBox) order in selectedOrders)
                {
                    Expander expander = sender as Expander;
                    workOrder = new WorkOrder(int.Parse(order.Item1));

                    foreach (KeyValuePair<int, string> kvp in workOrder.lineItems)
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + workOrder.OrderNumber + "00";
                            BarcodeTransfer(User.EmployeeCode, "D080", travellerNumber);
                        }
                    }

                    if (workOrder.Finished)
                    {
                        using var context = new NAT02Context();
                        if (context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == workOrder.OrderNumber).Any())
                        {
                            var orderMarkedForChecking = new EoiOrdersMarkedForChecking()
                            {
                                OrderNo = workOrder.OrderNumber
                            };
                            context.EoiOrdersMarkedForChecking.Remove(orderMarkedForChecking);

                            context.SaveChanges();
                        }
                    }

                    DeleteMachineVariables(((int)_orderNumber).ToString());
                }

                try
                {
                    Cursor = Cursors.Wait;
                    Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                    Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)
                        app.Application.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                    mailItem.Subject = "REQUEST FOR CHANGES WO# " + string.Join(",", selectedOrders.Select(o => o.Item1));
                    mailItem.To = GetEmailAddress(workOrder.Csr);
                    mailItem.Body = "";
                    mailItem.BCC = "intlcs6@natoli.com;customerservice5@natoli.com";
                    mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
                    mailItem.Display(false);
                    Cursor = Cursors.Arrow;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Cursor = Cursors.Arrow;
                }
            }
            // Scan just the order that was right clicked if nothing else has been selected
            else
            {
                workOrder = new WorkOrder((int)_orderNumber);

                foreach (KeyValuePair<int, string> kvp in workOrder.lineItems)
                {
                    string lineType = kvp.Value.Trim();
                    if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                    {
                        string travellerNumber = "1" + kvp.Key.ToString("00") + workOrder.OrderNumber + "00";
                        BarcodeTransfer(User.EmployeeCode, "D080", travellerNumber);
                    }
                }

                if (workOrder.Finished)
                {
                    using var context = new NAT02Context();
                    if (context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == workOrder.OrderNumber).Any())
                    {
                        var orderMarkedForChecking = new EoiOrdersMarkedForChecking()
                        {
                            OrderNo = workOrder.OrderNumber
                        };
                        context.EoiOrdersMarkedForChecking.Remove(orderMarkedForChecking);

                        context.SaveChanges();
                    }
                }
                try
                {
                    Cursor = Cursors.Wait;
                    Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                    Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)
                        app.Application.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                    mailItem.Subject = "REQUEST FOR CHANGES WO# " + ((int)_orderNumber).ToString();
                    mailItem.To = GetEmailAddress(workOrder.Csr.Split(' ')[0]);
                    mailItem.Body = "";
                    mailItem.BCC = "intlcs6@natoli.com;customerservice5@natoli.com";
                    mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
                    mailItem.Display(false);
                    Cursor = Cursors.Arrow;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    Cursor = Cursors.Arrow;
                }

                DeleteMachineVariables(((int)_orderNumber).ToString());
            }

            selectedOrders.Clear();

            MainRefresh();
        }

        private void StartWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                foreach ((string, CheckBox) order in selectedOrders)
                {
                    using var nat02context = new NAT02Context();
                    if (!nat02context.EoiOrdersEnteredAndUnscannedView.Any(o => o.OrderNo.ToString() == order.Item1))
                    {
                        nat02context.Dispose();
                        continue;
                    }
                    else
                    {
                        nat02context.Dispose();
                    }
                    Expander expander = sender as Expander;
                    workOrder = new WorkOrder(int.Parse(order.Item1));

                    foreach (KeyValuePair<int, string> kvp in workOrder.lineItems)
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + workOrder.OrderNumber + "00";
                            BarcodeTransfer(User.EmployeeCode, "D040", travellerNumber);
                        }
                    }
                }
            }
            // Scan just the order that was right clicked if nothing else has been selected
            else
            {
                workOrder = new WorkOrder((int)_orderNumber);

                foreach (KeyValuePair<int, string> kvp in workOrder.lineItems)
                {
                    string lineType = kvp.Value.Trim();
                    if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                    {
                        string travellerNumber = "1" + kvp.Key.ToString("00") + workOrder.OrderNumber + "00";
                        BarcodeTransfer(User.EmployeeCode, "D040", travellerNumber);
                    }
                }
            }

            selectedOrders.Clear();

            MainRefresh();
        }

        private void ToProdManOrder_Click(object sender, RoutedEventArgs e)
        {
            // Scan selected line items first and then clear the list
            if (selectedLineItems.Any())
            {
                foreach (string travellerNumber in selectedLineItems)
                {
                    using var nat02context = new NAT02Context();
                    if (!nat02context.EoiOrdersPrintedInEngineeringView.Any(o => o.OrderNo.ToString() == travellerNumber.Substring(2, 6)))
                    {
                        nat02context.Dispose();
                        continue;
                    }
                    else
                    {
                        nat02context.Dispose();
                    }
                    BarcodeTransfer(User.EmployeeCode, "D921", travellerNumber);
                }
                selectedLineItems.Clear();
            }

            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                foreach ((string, CheckBox) order in selectedOrders)
                {
                    using var nat02context = new NAT02Context();
                    if (!nat02context.EoiOrdersPrintedInEngineeringView.Any(o => o.OrderNo.ToString() == order.Item1))
                    {
                        nat02context.Dispose();
                        continue;
                    }
                    else
                    {
                        nat02context.Dispose();
                    }
                    Expander expander = sender as Expander;
                    workOrder = new WorkOrder(int.Parse(order.Item1));

                    foreach (KeyValuePair<int, string> kvp in workOrder.lineItems)
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + workOrder.OrderNumber + "00";
                            BarcodeTransfer(User.EmployeeCode, "D921", travellerNumber);
                        }
                    }
                }
            }
            // Scan just the order that was right clicked if nothing else has been selected
            else
            {
                Expander expander = sender as Expander;
                workOrder = new WorkOrder((int)_orderNumber);

                foreach (KeyValuePair<int, string> kvp in workOrder.lineItems)
                {
                    string lineType = kvp.Value.Trim();
                    if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                    {
                        string travellerNumber = "1" + kvp.Key.ToString("00") + workOrder.OrderNumber + "00";
                        BarcodeTransfer(User.EmployeeCode, "D921", travellerNumber);
                    }
                }
            }

            selectedOrders.Clear();

            MainRefresh();
        }

        private void ReadyToPrintMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainRefresh();
        }

        private void ForceRefresh_Click(object sender, RoutedEventArgs e)
        {
            MainRefresh();
        }

        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            // Select max project number
            using var projectsContext = new ProjectsContext();
            int engProjMax = Convert.ToInt32(projectsContext.EngineeringProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber)).First().ProjectNumber) + 1;
            int engProjArchMax = Convert.ToInt32(projectsContext.EngineeringArchivedProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber)).First().ProjectNumber) + 1;
            string projectNumber = engProjArchMax > engProjMax ? engProjArchMax.ToString() : engProjMax.ToString();
            

            // Dispose of project context
            projectsContext.Dispose();

            // Create new project window/project
            ProjectWindow projectWindow = new ProjectWindow(projectNumber, "0", this, User, true);

            // Dispose of project window
            projectWindow.Dispose();
        }

        private void ProjectSearch_Click(object sender, RoutedEventArgs e)
        {
            ProjectSearchWindow projectSearchWindow = new ProjectSearchWindow()
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner
            };
            projectSearchWindow.Show();
        }

        private void EditLayout_Click(object sender, RoutedEventArgs e)
        {
            EditLayoutWindow editLayoutWindow = new EditLayoutWindow(User, this);
            editLayoutWindow.Show();
        }

        private void CheckMissingVariables_Click(object sender, RoutedEventArgs e)
        {
            Window missing = new Window()
            {
                Width = 200,
                MinHeight = 200,
                Left = Left,
                Top = Top
            };
            Grid MainGrid = new Grid();
            TextBlock textBlock = new TextBlock()
            {
                Margin = new Thickness(50, 50, 50, 0)
            };
            string ordersMissingVars = "";

            using var nat02context = new NAT02Context();
            List<EoiMissingAutomationVariablesView> list = nat02context.EoiMissingAutomationVariablesView.ToList();
            int count = list.Count;
            missing.Height = count * 20;

            foreach (EoiMissingAutomationVariablesView row in list)
            {
                ordersMissingVars += row.OrderNumber.ToString() + "\n";
            }

            textBlock.Text = ordersMissingVars;

            missing.Content = MainGrid;
            MainGrid.Children.Add(textBlock);
            missing.Show();
        }

        private void CompletedQuoteCheck_Click(object sender, RoutedEventArgs e)
        {
            using var _nat02context = new NAT02Context();
            if (selectedQuotes.Any())
            {
                //foreach ((string, string, CheckBox) quote in selectedQuotes)
                int count = selectedQuotes.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox) quote = selectedQuotes[0];
                    quote.Item3.IsChecked = false;

                    EoiQuotesOneWeekCompleted q = new EoiQuotesOneWeekCompleted()
                    {
                        QuoteNo = double.Parse(quote.Item1),
                        QuoteRevNo = int.Parse(quote.Item2),
                        TimeSubmitted = DateTime.Now
                    };
                    _nat02context.Add(q);
                }
            }
            else
            {
                EoiQuotesOneWeekCompleted q = new EoiQuotesOneWeekCompleted()
                {
                    QuoteNo = _quoteNumber,
                    QuoteRevNo = _quoteRevNumber,
                    TimeSubmitted = DateTime.Now
                };
                _nat02context.Add(q);
            }
            _nat02context.SaveChanges();
            _nat02context.Dispose();
            MainRefresh();
        }

        private void SubmitQuote_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            using var context = new NAT01Context();
            using var nat02Context = new NAT02Context();
            using var necContext = new NECContext();
            if (selectedQuotes.Any())
            {
                //foreach ((string, string, CheckBox) selectedQuote in selectedQuotes)
                int _count = selectedQuotes.Count;
                for (int i = 0; i < _count; i++)
                {
                    (string, string, CheckBox) selectedQuote = selectedQuotes[0];
                    selectedQuote.Item3.IsChecked = false;

                    quote = new Quote(int.Parse(selectedQuote.Item1), short.Parse(selectedQuote.Item2));
                    QuoteHeader r = context.QuoteHeader.Where(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo).FirstOrDefault();
                    string customerName = necContext.Rm00101.Where(c => c.Custnmbr == r.UserAcctNo).First().Custname;
                    string csr = context.QuoteRepresentative.Where(r => r.RepId == quote.QuoteRepID).First().Name;
                    EoiQuotesMarkedForConversion q = new EoiQuotesMarkedForConversion()
                    {
                        QuoteNo = quote.QuoteNumber,
                        QuoteRevNo = quote.QuoteRevNo,
                        CustomerName = customerName,
                        Csr = csr,
                        CsrMarked = User.GetUserName(),
                        TimeSubmitted = DateTime.Now,
                        Rush = r.RushYorN
                    };
                    nat02Context.EoiQuotesMarkedForConversion.Add(q);
                }
            }
            else
            {
                quote = new Quote((int)_quoteNumber, (short)_quoteRevNumber);
                QuoteHeader r = context.QuoteHeader.Where(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo).FirstOrDefault();
                string customerName = necContext.Rm00101.Where(c => c.Custnmbr == r.UserAcctNo).First().Custname;
                string csr = context.QuoteRepresentative.Where(r => r.RepId == quote.QuoteRepID).First().Name;
                EoiQuotesMarkedForConversion q = new EoiQuotesMarkedForConversion()
                {
                    QuoteNo = quote.QuoteNumber,
                    QuoteRevNo = quote.QuoteRevNo,
                    CustomerName = customerName,
                    Csr = csr,
                    CsrMarked = User.GetUserName(),
                    TimeSubmitted = DateTime.Now,
                    Rush = r.RushYorN
                };
                nat02Context.EoiQuotesMarkedForConversion.Add(q);
            }
            nat02Context.SaveChanges();
            nat02Context.Dispose();
            necContext.Dispose();
            context.Dispose();
            Cursor = Cursors.Arrow;
            MainRefresh();
        }

        private void FilterProjects_Click(object sender, RoutedEventArgs e)
        {
            _filterProjects = !_filterProjects;
            MenuItem menuItem = sender as MenuItem;
            menuItem.IsChecked = _filterProjects;
            MainRefresh();
        }

        private void LineItemCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var order = ((((checkBox.Parent as Grid).Parent as StackPanel).Parent as Expander).Header as Grid).Children[0].GetValue(ContentProperty).ToString();
            int lineNumber = int.Parse((checkBox.Parent as Grid).Tag.ToString());
            string travellerNumber = "1" + lineNumber.ToString("00") + order + "00";
            selectedLineItems.Add(travellerNumber);
        }

        private void LineItemCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                var order = ((((checkBox.Parent as Grid).Parent as StackPanel).Parent as Expander).Header as Grid).Children[0].GetValue(ContentProperty).ToString();
                int lineNumber = int.Parse((checkBox.Parent as Grid).Tag.ToString());
                string travellerNumber = "1" + lineNumber.ToString("00") + order + "00";
                selectedLineItems.Remove(travellerNumber);
            }
            catch
            {

            }
        }
        #endregion

        private void Subscriptions_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            MenuItem topMenu = (MenuItem)sender;
            using var nat02context = new NAT02Context();
            string subs = "";
            foreach (MenuItem item in topMenu.Items)
            {
                if (item.IsChecked) { subs += item.Header.ToString() + ','; }
            }
            if (subs.Length > 0) { subs = subs.Substring(0, subs.Length - 1); }
            EoiSettings sub = nat02context.EoiSettings.Where(u => u.EmployeeId == User.EmployeeCode).First();
            sub.Subscribed = subs;
            nat02context.Update(sub);
            nat02context.SaveChanges();
            MainRefresh();
            //if (!(subscribedOnOpen.Cast<string>().ToList().SequenceEqual(Properties.Settings.Default.Subscribed.Cast<string>().ToList())))
            //{
            //    ExecuteQueries();
            //}
        }
        #endregion

        #region MainWindowSearches
        private void QuoteSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            using var nat01context = new NAT01Context();
            try
            {
                string quoteNumber = QuoteSearchTextBlock.Text;
                string revNumber = QuoteRevNoSearchTextBlock.Text;
                WindowCollection collection = App.Current.Windows;
                foreach (Window w in collection)
                {
                    if (w.Title.Contains(quoteNumber))
                    {
                        nat01context.Dispose();
                        w.WindowState = WindowState.Normal;
                        w.Show();
                        goto AlreadyOpen;
                    }
                }
                quote = new Quote(int.Parse(quoteNumber), short.Parse(revNumber));
                nat01context.Dispose();
                mainTimer.Stop();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("QuoteSearchButton_Click - Before new window instance", ex.Message);
            }
            try
            {
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, "", User)
                {
                    Left = Left,
                    Top = Top
                };
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("QuoteSearchButton_Click - After new window instance", ex.Message);
            }
        AlreadyOpen:
            nat01context.Dispose();
            Cursor = Cursors.Arrow;
            QuoteSearchTextBlock.Text = "";
            QuoteRevNoSearchTextBlock.Text = "";
        }

        private void QuoteSearchTextBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (QuoteSearchTextBlock.Text.Length > 0)
                {
                    using var _nat01context = new NAT01Context();
                    var revNo = _nat01context.QuoteHeader.Where(q => q.QuoteNo == int.Parse(QuoteSearchTextBlock.Text)).Max(q => q.QuoteRevNo);
                    QuoteRevNoSearchTextBlock.Text = revNo.ToString();
                    _nat01context.Dispose();
                }
            }
            catch //(Exception ex)
            {

            }
        }

        private void ProjectSearchTextBlock_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProjectSearchTextBlock.Text.Length > 0)
                {
                    using var _projectscontext = new ProjectsContext();
                    var revNo = _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)).Max(p => p.RevisionNumber);
                    ProjectRevNoSearchTextBlock.Text = revNo.ToString();
                    _projectscontext.Dispose();
                }
            }
            catch //(Exception ex)
            {

            }
        }

        private void ProjectSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            try
            {
                string projectNumber = ProjectSearchTextBlock.Text;
                string revNumber = ProjectRevNoSearchTextBlock.Text;
                //try
                //{
                //    ProjectWindow projectWindow = new ProjectWindow(projectNumber, revNumber, this, User, false);
                //    projectWindow.Dispose();
                //}
                //catch (Exception ex)
                //{
                //    // MessageBox.Show(ex.Message);
                //    WriteToErrorLog("ProjectSearchButton_Click - After new window instance", ex.Message);
                //}
                string path = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber;
                try
                {
                    if (revNumber != "0")
                    {
                        if (System.IO.Directory.Exists(path + "_" + revNumber + @"\"))
                        {
                            System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path + "_" + revNumber + @"\");
                        }
                        else
                        {
                            if (!System.IO.Directory.Exists(path + @"\"))
                                System.IO.Directory.CreateDirectory(path + @"\");
                            System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path + @"\");
                        }
                    }
                    else
                    {
                        if (!System.IO.Directory.Exists(path + @"\"))
                            System.IO.Directory.CreateDirectory(path + @"\");
                        System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path + @"\");
                    }
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    WriteToErrorLog("ProjectSearchButton_Click - Before new window instance", ex.Message);
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("ProjectSearchButton_Click - After new window instance", ex.Message);
            }
            Cursor = Cursors.Arrow;
            ProjectSearchTextBlock.Text = "";
            ProjectRevNoSearchTextBlock.Text = "";
        }

        private void OrderSearchButton_Click(object sender, RoutedEventArgs e)
        {
            if (OrderSearchTextBlock.Text.Length != 6)
            {
                MessageBox.Show("Please enter a 6 digit order number." + "\n" + "Example: 123456 or 654321", "Open Work Order Window", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            else
            {
                Cursor = Cursors.AppStarting;

                using var context = new NAT02Context();
                using NAT01Context nat01context = new NAT01Context();
                try
                {
                    string orderNumber = OrderSearchTextBlock.Text;
                    if (!nat01context.OrderHeader.Any(o => (int)o.OrderNo == 100 * Convert.ToInt32(orderNumber)))
                    {
                        MessageBox.Show("This order does not exist in the Order Header Table." + "\n" + "Please check your input.", "Open Work Order Window", MessageBoxButton.OK, MessageBoxImage.Information);
                        context.Dispose();
                        nat01context.Dispose();
                        Cursor = Cursors.Arrow;
                        return;
                    }
                    else
                    {
                        workOrder = new WorkOrder(int.Parse(orderNumber));
                        WindowCollection collection = App.Current.Windows;
                        foreach (Window w in collection)
                        {
                            if (w.Title.Contains(workOrder.OrderNumber.ToString()))
                            {
                                context.Dispose();
                                nat01context.Dispose();
                                w.WindowState = WindowState.Normal;
                                w.Show();
                                goto AlreadyOpen;
                            }
                        }
                        if (context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User != User.GetUserName()))
                        {
                            MessageBox.Show("BEWARE!!\n" + context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User != User.GetUserName()).FirstOrDefault().User + " is in this order at the moment.");
                            mainTimer.Stop();
                        }
                        else if (context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User == User.GetUserName()))
                        {
                            MessageBox.Show("You already have this order open.");
                            context.Dispose();
                            nat01context.Dispose();
                        }
                        mainTimer.Stop();
                    }
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    WriteToErrorLog("OrderSearchButton_Click - Before new window instance", ex.Message);
                }
                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, this, "", User)
                {
                    Left = Left,
                    Top = Top
                };
            AlreadyOpen:
                context.Dispose();
                nat01context.Dispose();
                Cursor = Cursors.Arrow;
            }
            OrderSearchTextBlock.Text = "";
        }

        private void ProjectSearchTextBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (ProjectSearchTextBlock.Text.Length > 0 && ProjectRevNoSearchTextBlock.Text.Length == 0)
                    {
                        using var _projectscontext = new ProjectsContext();
                        var revNo = _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)).Max(p => p.RevisionNumber);
                        ProjectRevNoSearchTextBlock.Text = revNo.ToString();
                        _projectscontext.Dispose();
                    }
                }
                catch //(Exception ex)
                {

                }
                ProjectSearchButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void QuoteSearchTextBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    if (QuoteSearchTextBlock.Text.Length > 0 && QuoteRevNoSearchTextBlock.Text.Length == 0)
                    {
                        using var _nat01context = new NAT01Context();
                        var revNo = _nat01context.QuoteHeader.Where(q => q.QuoteNo == int.Parse(QuoteSearchTextBlock.Text)).Max(q => q.QuoteRevNo);
                        QuoteRevNoSearchTextBlock.Text = revNo.ToString();
                        _nat01context.Dispose();
                    }
                }
                catch //(Exception ex)
                {

                }
                QuoteSearchButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void OrderSearchTextBlock_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OrderSearchButton_Click(sender, new RoutedEventArgs());
            }
        }
        #endregion

        #region ModuleBuilding

        #region Panel Construction
        private void ConstructModules()
        {
            int panel_count = User.VisiblePanels.Count;
            ColumnDefinition colDef;
            RowDefinition rowDef;
            int colCount = 0;
            int rowCount = 0;

            if (User.VisiblePanels.Count == 1)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }
            else if (User.VisiblePanels.Count == 2)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }
            else if (User.VisiblePanels.Count == 3)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }
            else if (User.VisiblePanels.Count == 4)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }
            else if (User.VisiblePanels.Count == 5)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }
            else if (User.VisiblePanels.Count == 6)
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }
            else
            {
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                MainGrid.ColumnDefinitions.Add(colDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
                rowDef = new RowDefinition();
                rowDef.Height = new GridLength(1, GridUnitType.Star);
                MainGrid.RowDefinitions.Add(rowDef);
            }

            int i = 0;
            colCount = MainGrid.ColumnDefinitions.Count;
            int j = 0;
            rowCount = MainGrid.RowDefinitions.Count;
            int k = 0;
            for (j = 0; j < colCount && k < User.VisiblePanels.Count; j++)
            {
                for (i = 0; i < rowCount && k < User.VisiblePanels.Count; i++)
                {
                    Border border = new Border();
                    if (User.VisiblePanels.Count == 5 && k == 4)
                    {
                        border = ConstructBorder();
                        Grid.SetRow(border, i);
                        Grid.SetColumn(border, j);
                        border.SetValue(Grid.RowSpanProperty, 2);
                    }
                    else
                    {
                        border = ConstructBorder();
                        Grid.SetRow(border, i);
                        Grid.SetColumn(border, j);
                    }
                    try
                    {
                        MainGrid.Children.Add(border);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                    k++;
                }
            }
        }

        private Border ConstructBorder()
        {
            Border border = new Border()
            {
                Name = "Border_" + MainGrid.Children.Count.ToString(),
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1)
            };

            // Count label
            Label countLabel = new Label()
            {
                Name = "TotalLabel",
                Content = "Total: ", // + interiorStackPanel.Children.Count,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Height = 20,
                Padding = new Thickness(0, 0, 5, 0),
                Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0F0")),
                BorderBrush = new SolidColorBrush(SystemColors.ControlDarkDarkColor)
            };

            DockPanel dockPanel = ConstructDockPanel("DockPanel_" + MainGrid.Children.Count.ToString(),
                                                     CreateHeaderGrid("A_" + MainGrid.Children.Count.ToString() + "_", "Test"),
                                                     ConstructScrollViewer(),
                                                     countLabel);
            border.Child = dockPanel;

            return border;
        }

        private static DockPanel ConstructDockPanel(string name, Grid headerGrid, ScrollViewer scrollViewer, Label countLabel)
        {
            DockPanel dockPanel = new DockPanel()
            {
                Name = name,
                LastChildFill = true
            };

            DockPanel.SetDock(headerGrid, Dock.Top);
            dockPanel.Children.Add(headerGrid);

            DockPanel.SetDock(countLabel, Dock.Bottom);
            dockPanel.Children.Add(countLabel);

            dockPanel.Children.Add(scrollViewer);
            Image image = new Image()
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                MaxWidth = 200,
                MinWidth = 100
            };
            var bitImage = new BitmapImage();
            bitImage.BeginInit();
            bitImage.UriSource = new Uri("NATOLI_ANIMATION.gif", UriKind.Relative);
            bitImage.EndInit();
            //AnimationBehavior.SetSourceUri(image, new Uri("NATOLI_ANIMATION.gif", UriKind.Relative));
            // ImageBehavior.SetAnimatedSource(image, bitImage);
            dockPanel.Children.Add(image);

            return dockPanel;
        }

        private Grid CreateHeaderGrid(string name, string title)
        {
            Grid headerLabelGrid = new Grid()
            {
                Name = name + "HeaderLabelGrid",
                Height = 31,
                Background = new SolidColorBrush(SystemColors.GradientActiveCaptionColor),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Header label
            Label headerLabel = new Label()
            {
                Name = name + "Label",
                Content = title,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 3, 0, 1),
                Height = 31,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Padding = new Thickness(0),
                Background = new SolidColorBrush(SystemColors.GradientActiveCaptionColor),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Header search filter
            TextBox searchBox = new TextBox()
            {
                Name = name + "SearchBox",
                Style = App.Current.Resources["SearchBox"] as Style
            };
            searchBox.PreviewKeyUp += SearchBox_PreviewKeyUp;
            searchBox.TextChanged += SearchBox_TextChanged;

            Grid.SetColumnSpan(headerLabel, 2);
            Grid.SetColumn(searchBox, 1);

            AddColumn(headerLabelGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), headerLabel);
            AddColumn(headerLabelGrid, CreateColumnDefinition(new GridLength(150)), searchBox);

            return headerLabelGrid;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
            TextBlock textBlock = (sender as TextBox).Template.FindName("SearchTextBlock", sender as TextBox) as TextBlock;
            Image image = (sender as TextBox).Template.FindName("MagImage", (sender as TextBox)) as Image;

            if (textBox.Text.Length > 0)
            {
                image.Source = ((Image)App.Current.Resources["xImage"]).Source;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                image.Source = ((Image)App.Current.Resources["MagnifyingGlassImage"]).Source;
                textBlock.Visibility = Visibility.Visible;
            }
        }

        private void SearchBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
                textBox.Text = "";
            }
        }

        private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            TextBox textBox = (image.Parent as Grid).Children.OfType<TextBox>().First();
            textBox.Text = "";
        }

        private ScrollViewer ConstructScrollViewer()
        {
            ScrollViewer scrollViewer = new ScrollViewer()
            {
                Style = FindResource("ScrollViewerStyle") as Style,
                Visibility = Visibility.Collapsed
            };

            StackPanel stackPanel = new StackPanel();
            scrollViewer.Content = stackPanel;

            return scrollViewer;
        }

        // Build expanders
        private void ConstructExpanders()
        {
            for (int i = 0; i < User.VisiblePanels.Count; i++)
            {
                DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel;

                Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();

                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

                string panel = User.VisiblePanels[i];

                BuildPanel(dockPanel, sp, panel);

                Label countLabel = dockPanel.Children.OfType<Label>().First();

                countLabel.Content = "Total: " + sp.Children.OfType<Expander>().Count();
            }
        }

        private void BuildPanel(DockPanel dockPanel, StackPanel sp, string panel)
        {
            Border headerBorder = new Border()
            {
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0, 1, 0, 1)
            };

            Grid headerGrid = new Grid()
            {
                Background = MainMenu.Background,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Check all checkbox
            CheckBox checkBox = new CheckBox()
            {
                IsChecked = false,
                Style = App.Current.Resources["CheckBox"] as Style,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(20, 0, 0, 0),
                LayoutTransform = new ScaleTransform(1.1, 1.1)
            };
            checkBox.Checked += CheckBox_Checked;
            checkBox.Unchecked += CheckBox_Checked;

            switch (panel)
            {
                case "BeingEntered":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(65)), CreateLabel("Quote No", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(30)), CreateLabel("Rev", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Ships", 0, 5, FontWeights.Normal));
                    // if (ordersBeingEnteredDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += OrdersBeingEnteredSearchBox_TextChanged;

                    break;
                case "InTheOffice":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(45)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("In Office", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(100)), CreateLabel("Employee Name", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("CSR", 0, 6, FontWeights.Normal));
                    // if (ordersInTheOfficeDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += OrdersInTheOfficeSearchBox_TextChanged;

                    break;
                case "QuotesNotConverted":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(65)), CreateLabel("Quote No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Rev No", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
                    //if (quotesNotConvertedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += QuotesNotConvertedSearchBox_TextChanged;

                    break;
                case "EnteredUnscanned":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
                    // if (ordersEnteredUnscannedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += OrdersEnteredUnscannedSearchBox_TextChanged;

                    break;
                case "InEngineering":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(45)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("In Eng", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(125)), CreateLabel("Employee Name", 0, 5, FontWeights.Normal));
                    // if (ordersInEngineeringUnprintedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += OrdersInEngineeringUnprintedSearchBox_TextChanged;

                    break;
                case "QuotesToConvert":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(65)), CreateLabel("Quote No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Rev No", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(148)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Days In", 0, 5, FontWeights.Normal));
                    // if (quotesToConvertDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += QuotesToConvertSearchBox_TextChanged;

                    break;
                case "ReadyToPrint":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(110)), CreateLabel("Checker", 0, 5, FontWeights.Normal));
                    // if (ordersReadyToPrintDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += OrdersReadyToPrintSearchBox_TextChanged;

                    break;
                case "PrintedInEngineering":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(45)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Checker", 0, 5, FontWeights.Normal));
                    // if (ordersPrintedInEngineeringDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += OrdersPrintedInEngineeringSearchBox_TextChanged;

                    break;
                case "AllTabletProjects":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (allTabletProjectsDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += AllTabletProjectsSearchBox_TextChanged;

                    break;
                case "TabletProjectsNotStarted":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 5, FontWeights.Normal));
                    // if (tabletProjectsNotStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += TabletProjectsNotStartedSearchBox_TextChanged;

                    break;
                case "TabletProjectsStarted":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (tabletProjectsStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += TabletProjectsStartedSearchBox_TextChanged;

                    break;
                case "TabletProjectsDrawn":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (tabletProjectsDrawnDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += TabletProjectsDrawnSearchBox_TextChanged;

                    break;
                case "TabletProjectsSubmitted":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (tabletProjectsSubmittedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += TabletProjectsSubmittedSearchBox_TextChanged;

                    break;
                case "TabletProjectsOnHold":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (tabletProjectsOnHoldDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += TabletProjectsOnHoldSearchBox_TextChanged;

                    break;
                case "AllToolProjects":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (allToolProjectsDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += AllToolProjectsSearchBox_TextChanged;

                    break;
                case "ToolProjectsNotStarted":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 5, FontWeights.Normal));
                    // if (toolProjectsNotStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += ToolProjectsNotStartedSearchBox_TextChanged;

                    break;
                case "ToolProjectsStarted":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (toolProjectsStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += ToolProjectsStartedSearchBox_TextChanged;

                    break;
                case "ToolProjectsDrawn":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (toolProjectsDrawnDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += ToolProjectsDrawnSearchBox_TextChanged;

                    break;
                case "ToolProjectsOnHold":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
                    // if (toolProjectsOnHoldDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += ToolProjectsOnHoldSearchBox_TextChanged;

                    break;
                case "DriveWorksQueue":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Model Name", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(170)), CreateLabel("Released By", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Tag", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(85)), CreateLabel("Release Time", 0, 4, FontWeights.Normal));
                    // if (driveWorksQueueDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += DriveWorksQueueSearchBox_TextChanged;

                    break;
                case "NatoliOrderList":
                    // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
                    Grid.SetColumn(checkBox, 0);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order #", 0, 1, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer", 0, 2, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Ship Date", 0, 3, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rush", 0, 4, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("On Hold", 0, 5, FontWeights.Normal));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rep", 0, 6, FontWeights.Normal));
                    // if (natoliOrderListDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

                    headerBorder.Child = headerGrid;
                    DockPanel.SetDock(headerBorder, Dock.Top);
                    dockPanel.Children.Insert(1, headerBorder);

                    dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().First().TextChanged += NatoliOrderListSearchBox_TextChanged;

                    break;
                default:
                    break;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            var expanders = ((((checkBox.Parent as Grid).Parent as Border).Parent as DockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.OfType<Expander>();
            foreach (Expander expander in expanders)
            {
                var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = checkBox.IsChecked;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        private ColumnDefinition CreateColumnDefinition(GridLength width)
        {
            ColumnDefinition colDef = new ColumnDefinition();
            try
            {
                colDef.Width = width;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return colDef;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private Label CreateLabel(string content, int row, int column, FontWeight weight, SolidColorBrush textColor = null, FontStyle? fontStyle = null, double fontSize = 12, bool addPadding = false)
        {
            Label label = new Label();
            try
            {
                label.Content = content;
                label.HorizontalAlignment = HorizontalAlignment.Stretch;
                label.HorizontalContentAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Top;
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.FontSize = fontSize;
                if (addPadding) { label.Padding = new Thickness(0, 0, 0, 0); }
                if (!(textColor is null))
                {
                    label.Foreground = textColor;
                }
                label.FontWeight = weight;
                label.FontStyle = !(fontStyle is null) ? (FontStyle)fontStyle : FontStyles.Normal;
                Grid.SetRow(label, row);
                Grid.SetColumn(label, column);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return label;
        }

        private CheckBox CreateCheckBox(int row, int column, bool isChecked)
        {
            CheckBox checkBox = new CheckBox();
            try
            {
                checkBox.IsChecked = isChecked;
                checkBox.VerticalAlignment = VerticalAlignment.Center;
                checkBox.LayoutTransform = new ScaleTransform(0.7, 0.7, 0, 0);
                checkBox.Checked += LineItemCheckBox_Checked;
                checkBox.Unchecked += LineItemCheckBox_Unchecked;
                Grid.SetRow(checkBox, row);
                Grid.SetColumn(checkBox, column);
            }
            catch
            {
                return null;
            }
            return checkBox;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="columnDefinition"></param>
        /// <param name="label"></param>
        private static void AddColumn(Grid grid, ColumnDefinition columnDefinition = null, UIElement element = null)
        {
            try
            {
                grid.ColumnDefinitions.Add(columnDefinition);
                if (!(element is null)) { grid.Children.Add(element); };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        //Handle refreshes
        private void BindData(string timer)
        {
            try
            {
                foreach (string panel in User.VisiblePanels)
                {
                    switch (panel, timer)
                    {
                        case ("BeingEntered", "Main"):
                            Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.Invoke(() => BindBeingEntered()), TaskScheduler.Current);
                            break;
                        case ("InTheOffice", "Main"):
                            Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.Invoke(() => BindInTheOffice()), TaskScheduler.Current);
                            break;
                        case ("QuotesNotConverted", "QuotesNotConverted"):
                            Task.Run(() => GetQuotesNotConverted()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesNotConverted()), TaskScheduler.Current);
                            break;
                        case ("EnteredUnscanned", "Main"):
                            Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.Invoke(() => BindEnteredUnscanned()), TaskScheduler.Current);
                            break;
                        case ("InEngineering", "Main"):
                            Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindInEngineering()), TaskScheduler.Current);
                            break;
                        case ("QuotesToConvert", "Main"):
                            Task.Run(() => GetQuotesToConvert()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesToConvert()), TaskScheduler.Current);
                            break;
                        case ("ReadyToPrint", "Main"):
                            Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.Invoke(() => BindReadyToPrint()), TaskScheduler.Current);
                            break;
                        case ("PrintedInEngineering", "Main"):
                            Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindPrintedInEngineering()), TaskScheduler.Current);
                            break;
                        case ("AllTabletProjects", "Main"):
                            Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()), TaskScheduler.Current);
                            break;
                        case ("TabletProjectsNotStarted", "Main"):
                            Task.Run(() => GetTabletProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsNotStarted()), TaskScheduler.Current);
                            break;
                        case ("TabletProjectsStarted", "Main"):
                            Task.Run(() => GetTabletProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsStarted()), TaskScheduler.Current);
                            break;
                        case ("TabletProjectsDrawn", "Main"):
                            Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
                            break;
                        case ("TabletProjectsSubmitted", "Main"):
                            Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
                            break;
                        case ("TabletProjectsOnHold", "Main"):
                            Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
                            break;
                        case ("AllToolProjects", "Main"):
                            Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()), TaskScheduler.Current);
                            break;
                        case ("ToolProjectsNotStarted", "Main"):
                            Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
                            break;
                        case ("ToolProjectsStarted", "Main"):
                            Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
                            break;
                        case ("ToolProjectsDrawn", "Main"):
                            Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
                            break;
                        case ("ToolProjectsOnHold", "Main"):
                            Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
                            break;
                        case ("DriveWorksQueue", "Main"):
                            Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()), TaskScheduler.Current);
                            break;
                        case ("NatoliOrderList", "NatoliOrderList"):
                            Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()), TaskScheduler.Current);
                            break;
                        default:
                            break;
                    }
                }
            }
            catch
            {

            }
        }
        private int GetNumberOfDays(string csr)
        {
            switch (csr)
            {
                case "Alex Heimberger":
                    return 14;
                case "Anna King":
                    return 7;
                case "Bryan Foy":
                    return 7;
                case "David Nelson":
                    return 7;
                case "Gregory Lyle":
                    return 14;
                case "Heather Lane":
                    return 7;
                case "Humberto Zamora":
                    return 14;
                case "James Willis":
                    return 14;
                case "Miral Bouzitoun":
                    return 14;
                case "Nicholas Tarte":
                    return 14;
                case "Samantha Bowman":
                    return 7;
                case "Tiffany Simonpietri":
                    return 7;
                default:
                    return 14;
            }
        }

        #region GetsAndBinds
        private void GetBeingEntered()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiOrdersBeingEnteredView> eoiOrdersBeingEnteredView = _nat02context.EoiOrdersBeingEnteredView.OrderBy(o => o.OrderNo).ToList();

                ordersBeingEnteredDict = new Dictionary<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)>();

                foreach (EoiOrdersBeingEnteredView order in eoiOrdersBeingEnteredView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        if (_nat02context.EoiOrdersBeingChecked.Where(o => o.OrderNo == order.OrderNo).Any())
                        {
                            back = new SolidColorBrush(Colors.DodgerBlue);
                        }
                        else
                        {
                            back = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFFFF");
                        }
                    }
                    ordersBeingEnteredDict.Add(order.OrderNo, ((double)order.QuoteNo, (short)order.Rev, order.CustomerName, (int)order.NumDaysToShip, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                dictList.Add(ordersBeingEnteredDict);
                eoiOrdersBeingEnteredView.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindBeingEntered()
        {
            int i = User.VisiblePanels.IndexOf("BeingEntered");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                TextBox textBox = moduleHeader.Children.OfType<TextBox>().First();

                RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

                textBox.TextChanged += OrdersBeingEnteredSearchBox_TextChanged;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "BeingEntered");
            }

            BeingEnteredExpanders(ordersBeingEnteredDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetInTheOffice()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiOrdersInOfficeView> eoiOrdersInOfficeView = new List<EoiOrdersInOfficeView>();
                if (User.Department == "Customer Service" && !(User.GetUserName().StartsWith("Tiffany") || User.GetUserName().StartsWith("James W")))
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiOrdersInOfficeView = _nat02context.EoiOrdersInOfficeView.Where(o => o.Csr.StartsWith(usrName)).OrderBy(o => o.NumDaysToShip).ThenBy(o => o.DaysInOffice).ToList();
                }
                else
                {
                    eoiOrdersInOfficeView = _nat02context.EoiOrdersInOfficeView.OrderBy(o => o.NumDaysToShip).ThenBy(o => o.DaysInOffice).ToList();
                }

                ordersInTheOfficeDict = new Dictionary<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)>();

                foreach (EoiOrdersInOfficeView order in eoiOrdersInOfficeView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    ordersInTheOfficeDict.Add((double)order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, (int)order.DaysInOffice, order.EmployeeName, order.Csr, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }

                eoiOrdersInOfficeView.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindInTheOffice()
        {
            int i = User.VisiblePanels.IndexOf("InTheOffice");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                TextBox textBox = moduleHeader.Children.OfType<TextBox>().First();

                RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

                textBox.TextChanged += OrdersInTheOfficeSearchBox_TextChanged;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "InTheOffice");
            }

            InTheOfficeExpanders(ordersInTheOfficeDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetEnteredUnscanned()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiOrdersEnteredAndUnscannedView> eoiOrdersEnteredAndUnscannedView = _nat02context.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();

                ordersEnteredUnscannedDict = new Dictionary<double, (string customerName, int daysToShip, string background, string foreground, string fontWeight)>();

                foreach (EoiOrdersEnteredAndUnscannedView order in eoiOrdersEnteredAndUnscannedView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    bool doNotProcess = Convert.ToBoolean(order.DoNotProcess);
                    string[] errRes;
                    errRes = new string[2] { order.ProcessState,
                                         order.TransitionName };

                    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
                    {
                        fore = new SolidColorBrush(Colors.White);
                        weight = FontWeights.Normal;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    ordersEnteredUnscannedDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                eoiOrdersEnteredAndUnscannedView.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindEnteredUnscanned()
        {
            int i = User.VisiblePanels.IndexOf("EnteredUnscanned");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "EnteredUnscanned");
            }

            OrdersEnteredUnscannedExpanders(ordersEnteredUnscannedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetInEngineering()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                using var nat01context = new NAT01Context();
                List<EoiOrdersInEngineeringUnprintedView> eoiOrdersInEngineeringUnprintedView = _nat02context.EoiOrdersInEngineeringUnprintedView.OrderByDescending(o => o.DaysInEng).ThenBy(o => o.NumDaysToShip).ToList();

                ordersInEngineeringUnprintedDict = new Dictionary<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)>();

                foreach (EoiOrdersInEngineeringUnprintedView order in eoiOrdersInEngineeringUnprintedView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        int count = _nat02context.MaMachineVariables.Where(o => o.WorkOrderNumber == order.OrderNo.ToString()).Count();
                        string machineType;
                        machineType = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100).FirstOrDefault().MachinePriceCode.Trim();
                        var lineType = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100 && (o.DetailTypeId == "U" || o.DetailTypeId == "L" || o.DetailTypeId == "R")).ToList();
                        if (_nat02context.EoiOrdersBeingChecked.Where(o => o.OrderNo == order.OrderNo).Any())
                        {
                            back = new SolidColorBrush(Colors.DodgerBlue);
                        }
                        else if (count == 0 && (machineType == "BB" || machineType == "B" || machineType == "D") && lineType.Count != 0)
                        {
                            back = new SolidColorBrush(Colors.Red);
                        }
                        else if (_nat02context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == order.OrderNo).Any())
                        {
                            back = new SolidColorBrush(Colors.GreenYellow);
                        }
                        else
                        {
                            back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                        }
                    }
                    ordersInEngineeringUnprintedDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, (int)order.DaysInEng, order.EmployeeName, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                eoiOrdersInEngineeringUnprintedView.Clear();
                _nat02context.Dispose();
                nat01context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindInEngineering()
        {
            int i = User.VisiblePanels.IndexOf("InEngineering");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "InEngineering");
            }

            OrdersInEngineeringUnprintedExpanders(ordersInEngineeringUnprintedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetReadyToPrint()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                using var nat01context = new NAT01Context();
                List<EoiOrdersReadyToPrintView> eoiOrdersReadyToPrintView = _nat02context.EoiOrdersReadyToPrintView.OrderBy(o => o.OrderNo).ToList();

                ordersReadyToPrintDict = new Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)>();

                foreach (EoiOrdersReadyToPrintView order in eoiOrdersReadyToPrintView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        bool tm2 = Convert.ToBoolean(order.TM2);
                        bool tabletPrints = Convert.ToBoolean(order.Tablet);
                        bool toolPrints = Convert.ToBoolean(order.Tool);
                        List<OrderDetails> orderDetails;
                        List<OrderHeader> orderHeader;
                        orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100).ToList();
                        orderHeader = nat01context.OrderHeader.Where(o => o.OrderNo == order.OrderNo * 100).ToList();

                        if (tm2 || tabletPrints)
                        {
                            foreach (OrderDetails od in orderDetails)
                            {
                                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
                                {
                                    string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + od.HobNoShapeId.Trim() + ".pdf";
                                    if (!System.IO.File.Exists(path))
                                    {
                                        goto Missing;
                                    }
                                }
                            }
                        }

                        if (tm2 || toolPrints)
                        {
                            foreach (OrderDetails od in orderDetails)
                            {
                                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
                                {
                                    string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
                                    detailType = detailType == "MISC" ? "REJECT" : detailType;
                                    string international = orderHeader.FirstOrDefault().UnitOfMeasure;
                                    string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + detailType + ".pdf";
                                    if (!System.IO.File.Exists(path))
                                    {
                                        goto Missing;
                                    }
                                    if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                                    {
                                        goto Missing;
                                    }
                                }
                            }
                        }

                        goto NotMissing;

                    Missing:;
                        if (User.Department == "Engineering")
                        {
                            back = new SolidColorBrush(Colors.MediumPurple);
                        }
                        else
                        {
                            back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                        }
                        goto Finished;

                    NotMissing:;
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

                    Finished:;
                    }
                    ordersReadyToPrintDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, order.EmployeeName, order.CheckedBy, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                eoiOrdersReadyToPrintView.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindReadyToPrint()
        {
            int i = User.VisiblePanels.IndexOf("ReadyToPrint");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "ReadyToPrint");
            }

            OrdersReadyToPrintExpanders(ordersReadyToPrintDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetPrintedInEngineering()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                using var nat01context = new NAT01Context();
                List<EoiOrdersPrintedInEngineeringView> eoiOrdersPrintedInEngineeringView = _nat02context.EoiOrdersPrintedInEngineeringView.OrderBy(o => o.OrderNo).ToList();

                ordersPrintedInEngineeringDict = new Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)>();

                foreach (EoiOrdersPrintedInEngineeringView order in eoiOrdersPrintedInEngineeringView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        bool tm2 = Convert.ToBoolean(order.TM2);
                        bool tabletPrints = Convert.ToBoolean(order.Tablet);
                        bool toolPrints = Convert.ToBoolean(order.Tool);
                        List<OrderDetails> orderDetails;
                        List<OrderHeader> orderHeader;
                        orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100).ToList();
                        orderHeader = nat01context.OrderHeader.Where(o => o.OrderNo == order.OrderNo * 100).ToList();

                        if (tm2 || tabletPrints)
                        {
                            foreach (OrderDetails od in orderDetails)
                            {
                                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
                                {
                                    string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + od.HobNoShapeId.Trim() + ".pdf";
                                    if (!System.IO.File.Exists(path))
                                    {
                                        goto Missing;
                                    }
                                }
                            }
                        }

                        if (tm2 || toolPrints)
                        {
                            foreach (OrderDetails od in orderDetails)
                            {
                                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
                                {
                                    string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
                                    detailType = detailType == "MISC" ? "REJECT" : detailType;
                                    string international = orderHeader.FirstOrDefault().UnitOfMeasure;
                                    string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + detailType + ".pdf";
                                    if (!System.IO.File.Exists(path))
                                    {
                                        goto Missing;
                                    }
                                    if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                                    {
                                        goto Missing;
                                    }
                                }
                            }
                        }

                        goto NotMissing;

                    Missing:;
                        if (User.Department == "Engineering")
                        {
                            back = new SolidColorBrush(Colors.MediumPurple);
                        }
                        else
                        {
                            back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                        }
                        goto Finished;

                    NotMissing:;
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

                    Finished:;
                    }
                    ordersPrintedInEngineeringDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, order.EmployeeName, order.CheckedBy, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                eoiOrdersPrintedInEngineeringView.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindPrintedInEngineering()
        {
            int i = User.VisiblePanels.IndexOf("PrintedInEngineering");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "PrintedInEngineering");
            }

            OrdersPrintedInEngineeringExpanders(ordersPrintedInEngineeringDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetQuotesNotConverted()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                         .Select(e => e.Subscribed);
                string[] subs = subList.First().Split(',');
                quotesCompletedChanged = (quotesCompletedCount != _nat02context.EoiQuotesOneWeekCompleted.Count());
                quotesCompletedCount = _nat02context.EoiQuotesOneWeekCompleted.Count();
                List<EoiQuotesNotConvertedView> _eoiQuotesNotConvertedView = new List<EoiQuotesNotConvertedView>();
                foreach (string sub in subs)
                {
                    string s = sub;
                    if (sub == "Nicholas")
                    {
                        s = "Nick";
                    }
                    _eoiQuotesNotConvertedView.AddRange(_nat02context.EoiQuotesNotConvertedView.Where(q => q.Csr.Contains(s)).ToList());
                }
                List<EoiQuotesNotConvertedView> eoiQuotesNotConvertedView = _eoiQuotesNotConvertedView.OrderByDescending(q => q.QuoteNo).ThenByDescending(q => q.QuoteRevNo).ToList();

                quotesNotConvertedDict = new Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, string background, string foreground, string fontWeight)>();

                foreach (EoiQuotesNotConvertedView quote in eoiQuotesNotConvertedView)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (quote.RushYorN == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    int days = GetNumberOfDays(quote.Csr);

                    bool needs_followup = !_nat02context.EoiQuotesOneWeekCompleted.Where(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).Any() &&
                                          DateTime.Today.Subtract(_nat02context.EoiQuotesNotConvertedView.First(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).QuoteDate).Days > days;
                    if (needs_followup)
                    {
                        back = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }

                    ExpanderAttributes expanderAttributes = new ExpanderAttributes(back, fore, weight);
                    quotesNotConvertedDict.Add((quote.QuoteNo, quote.QuoteRevNo), (quote.CustomerName, quote.Csr, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                eoiQuotesNotConvertedView.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindQuotesNotConverted()
        {
            int i = User.VisiblePanels.IndexOf("QuotesNotConverted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                TextBox textBox = moduleHeader.Children.OfType<TextBox>().First();

                RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

                textBox.TextChanged += QuotesNotConvertedSearchBox_TextChanged;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "QuotesNotConverted");
            }

            QuotesNotConvertedExpanders(quotesNotConvertedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetQuotesToConvert()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiQuotesMarkedForConversionView> eoiQuotesMarkedForConversion = new List<EoiQuotesMarkedForConversionView>();
                // Tiffany or James
                if (User.EmployeeCode == "E4516" || User.EmployeeCode == "E4816" || User.EmployeeCode == "E4852") //Tiffany, Samantha, James
                {
                    IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                        .Select(e => e.Subscribed);
                    string[] subs = subList.First().Split(',');
                    List<EoiQuotesMarkedForConversionView> _eoiQuotesMarkedForConversion = new List<EoiQuotesMarkedForConversionView>();
                    foreach (string sub in subs)
                    {
                        string s = sub;
                        if (sub == "Nicholas")
                        {
                            s = "Nick";
                        }
                        _eoiQuotesMarkedForConversion.AddRange(_nat02context.EoiQuotesMarkedForConversionView.Where(q => q.Csr.Contains(s)).ToList());
                    }
                    eoiQuotesMarkedForConversion = _eoiQuotesMarkedForConversion.OrderBy(q => q.TimeSubmitted).ToList();
                }
                else if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    if (usrName == "Nicholas")
                    {
                        usrName = "NICK";
                    }
                    eoiQuotesMarkedForConversion = _nat02context.EoiQuotesMarkedForConversionView.Where(q => q.Csr.Contains(usrName)).OrderBy(q => q.TimeSubmitted).ToList();
                }
                else
                {
                    eoiQuotesMarkedForConversion = _nat02context.EoiQuotesMarkedForConversionView.OrderBy(q => q.TimeSubmitted).ToList();
                }


                quotesToConvertDict = new Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)>();

                foreach (EoiQuotesMarkedForConversionView quote in eoiQuotesMarkedForConversion)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (quote.Rush.Trim() == "Y")
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    using var nAT01Context = new NAT01Context();
                    string shipment = nAT01Context.QuoteHeader.First(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).Shipment ?? "";
                    nAT01Context.Dispose();
                    quotesToConvertDict.Add((quote.QuoteNo, quote.QuoteRevNo), (quote.CustomerName, quote.Csr, (int)quote.DaysMarked, (DateTime)quote.TimeSubmitted, shipment.Trim(), back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }
                eoiQuotesMarkedForConversion.Clear();
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindQuotesToConvert()
        {
            int i = User.VisiblePanels.IndexOf("QuotesToConvert");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "QuotesToConvert");
            }

            QuotesToConvertExpanders(quotesToConvertDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetAllTabletProjects()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiAllTabletProjectsView> eoiAllTabletProjects = new List<EoiAllTabletProjectsView>();
                IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                                     .Select(e => e.Subscribed);
                eoiAllTabletProjects = new List<EoiAllTabletProjectsView>();
                string[] subs = subList.First().Split(',');
                List<EoiAllTabletProjectsView> projects = new List<EoiAllTabletProjectsView>();
                foreach (string sub in subs)
                {
                    string s = sub;
                    if (sub == "Gregory") { s = "Greg"; }
                    if (sub == "Nicholas") { s = "Nick"; }
                    eoiAllTabletProjects.AddRange(_nat02context.EoiAllTabletProjectsView.Where(q => q.Csr.Contains(s) || q.ReturnToCsr.Contains(s)).ToList());
                }
                if (_filterProjects)
                {
                    eoiAllTabletProjects = eoiAllTabletProjects.Where(p => p.HoldStatus != "On Hold" &&
                                           !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                           .OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                }
                else
                {
                    eoiAllTabletProjects = eoiAllTabletProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                }
                _nat02context.Dispose();

                allTabletProjectsDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiAllTabletProjectsView project in eoiAllTabletProjects)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    using var nat02context = new NAT02Context();
                    bool finished = nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any();
                    nat02context.Dispose();
                    bool onHold = project.HoldStatus == "On Hold";
                    bool submitted = project.TabletSubmittedBy is null ? false : project.TabletSubmittedBy.Length > 0;
                    bool drawn = project.TabletDrawnBy.Length > 0;
                    bool started = project.ProjectStartedTablet.Length > 0;

                    if ((bool)project.Tools)
                    {
                        fontStyle = FontStyles.Oblique;
                    }
                    else
                    {
                        fontStyle = FontStyles.Normal;
                    }

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (onHold)
                    {
                        back = new SolidColorBrush(Colors.MediumPurple);
                    }
                    else if (finished)
                    {
                        back = new SolidColorBrush(Colors.GreenYellow);
                    }
                    else if (submitted)
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0A7DFF"));
                    }
                    else if (drawn)
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#52A3FF"));
                    }
                    else if (started)
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    allTabletProjectsDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.Drafter, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                dictList.Add(allTabletProjectsDict);
                eoiAllTabletProjects.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindAllTabletProjects()
        {
            int i = User.VisiblePanels.IndexOf("AllTabletProjects");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                TextBox textBox = moduleHeader.Children.OfType<TextBox>().First();

                RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

                textBox.TextChanged += AllTabletProjectsSearchBox_TextChanged;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "AllTabletProjects");
            }

            AllTabletProjectsExpanders(allTabletProjectsDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetTabletProjectsNotStarted()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiTabletProjectsNotStarted> eoiTabletProjectsNotStarted = new List<EoiTabletProjectsNotStarted>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiTabletProjectsNotStarted = _nat02context.EoiTabletProjectsNotStarted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiTabletProjectsNotStarted = _nat02context.EoiTabletProjectsNotStarted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                tabletProjectsNotStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiTabletProjectsNotStarted project in eoiTabletProjectsNotStarted)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    if ((bool)project.Tools)
                    {
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Oblique;
                    }
                    else
                    {
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }
                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    tabletProjectsNotStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void BindTabletProjectsNotStarted()
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsNotStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsNotStarted");
            }

            TabletProjectsNotStartedExpanders(tabletProjectsNotStartedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetTabletProjectsStarted()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiTabletProjectsStarted> eoiTabletProjectsStarted = new List<EoiTabletProjectsStarted>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiTabletProjectsStarted = _nat02context.EoiTabletProjectsStarted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiTabletProjectsStarted = _nat02context.EoiTabletProjectsStarted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                tabletProjectsStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiTabletProjectsStarted project in eoiTabletProjectsStarted)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    if ((bool)project.Tools)
                    {
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Oblique;
                    }
                    else
                    {
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }
                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    tabletProjectsStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.ProjectStartedTablet, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiTabletProjectsStarted.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindTabletProjectsStarted()
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsStarted");
            }

            TabletProjectsStartedExpanders(tabletProjectsStartedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetTabletProjectsDrawn()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiTabletProjectsDrawn> eoiTabletProjectsDrawn = new List<EoiTabletProjectsDrawn>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiTabletProjectsDrawn = _nat02context.EoiTabletProjectsDrawn.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiTabletProjectsDrawn = _nat02context.EoiTabletProjectsDrawn.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                tabletProjectsDrawnDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiTabletProjectsDrawn project in eoiTabletProjectsDrawn)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    if ((bool)project.Tools)
                    {
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Oblique;
                    }
                    else
                    {
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    tabletProjectsDrawnDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.TabletDrawnBy, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiTabletProjectsDrawn.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindTabletProjectsDrawn()
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsDrawn");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsDrawn");
            }

            TabletProjectsDrawnExpanders(tabletProjectsDrawnDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetTabletProjectsSubmitted()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiTabletProjectsSubmitted> eoiTabletProjectsSubmitted = new List<EoiTabletProjectsSubmitted>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiTabletProjectsSubmitted = _nat02context.EoiTabletProjectsSubmitted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiTabletProjectsSubmitted = _nat02context.EoiTabletProjectsSubmitted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                tabletProjectsSubmittedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiTabletProjectsSubmitted project in eoiTabletProjectsSubmitted)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    if ((bool)project.Tools)
                    {
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Oblique;
                    }
                    else
                    {
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    tabletProjectsSubmittedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.TabletDrawnBy ?? project.ProjectStartedTablet ?? project.TabletSubmittedBy, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiTabletProjectsSubmitted.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindTabletProjectsSubmitted()
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsSubmitted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsSubmitted");
            }

            TabletProjectsSubmittedExpanders(tabletProjectsSubmittedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetTabletProjectsOnHold()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiProjectsOnHold> eoiTabletProjectsOnHold = new List<EoiProjectsOnHold>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    if (usrName == "Gregory") { usrName = "Greg"; }
                    if (usrName == "Nicholas") { usrName = "Nick"; }
                    eoiTabletProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Csr.StartsWith(usrName) && p.Tablet == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiTabletProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Tablet == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                tabletProjectsOnHoldDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiProjectsOnHold project in eoiTabletProjectsOnHold)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    if ((bool)project.Tools)
                    {
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Oblique;
                    }
                    else
                    {
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    tabletProjectsOnHoldDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiTabletProjectsOnHold.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindTabletProjectsOnHold()
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsOnHold");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsOnHold");
            }

            TabletProjectsOnHoldExpanders(tabletProjectsOnHoldDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetAllToolProjects()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiAllToolProjectsView> eoiAllToolProjects = new List<EoiAllToolProjectsView>();
                try
                {
                    IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                                     .Select(e => e.Subscribed);
                    string[] subs = subList.First().Split(',');
                    if (string.IsNullOrEmpty(subs[0]))
                    {
                        eoiAllToolProjects.AddRange(_nat02context.EoiAllToolProjectsView.ToList());
                    }
                    else
                    {
                        foreach (string sub in subs)
                        {
                            string s = sub;
                            if (sub == "Gregory") { s = "Greg"; }
                            if (sub == "Nicholas") { s = "Nick"; }
                            eoiAllToolProjects.AddRange(_nat02context.EoiAllToolProjectsView.Where(q => q.Csr.Contains(s) || q.ReturnToCsr.Contains(s)).ToList());
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                if (_filterProjects)
                {
                    eoiAllToolProjects = eoiAllToolProjects.Where(p => p.HoldStatus != "On Hold" &&
                                                                           !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber)).ToList();
                }
                _nat02context.Dispose();

                eoiAllToolProjects = eoiAllToolProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();

                allToolProjectsDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiAllToolProjectsView project in eoiAllToolProjects)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    using var nat02context = new NAT02Context();
                    bool finished = nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any();
                    nat02context.Dispose();
                    bool onHold = project.HoldStatus == "On Hold";
                    using var projectsContext = new ProjectsContext();
                    bool tablets = (bool)projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Tablet &&
                                   string.IsNullOrEmpty(projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).TabletCheckedBy);
                    bool multitip = (bool)projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).MultiTipSketch;
                    projectsContext.Dispose();
                    bool drawn = project.ToolDrawnBy.Length > 0;
                    bool started = project.ProjectStartedTool.Length > 0;

                    fontStyle = FontStyles.Normal;

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                    }

                    if (onHold)
                    {
                        back = new SolidColorBrush(Colors.MediumPurple);
                    }
                    else if (finished)
                    {
                        back = new SolidColorBrush(Colors.GreenYellow);
                    }
                    else if (drawn)
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3594FF"));
                    }
                    else if (started)
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
                    }
                    else if (multitip)
                    {
                        back = new SolidColorBrush(Colors.Gray);
                    }
                    else if (tablets)
                    {
                        back = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    allToolProjectsDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.Drafter, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiAllToolProjects.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindAllToolProjects()
        {
            int i = User.VisiblePanels.IndexOf("AllToolProjects");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "AllToolProjects");
            }

            AllToolProjectsExpanders(allToolProjectsDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetToolProjectsNotStarted()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiToolProjectsNotStarted> eoiToolProjectsNotStarted = new List<EoiToolProjectsNotStarted>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiToolProjectsNotStarted = _nat02context.EoiToolProjectsNotStarted.Where(p => p.Csr.StartsWith(usrName) && (p.Tablet == false || (p.Tablet == true && p.TabletCheckedBy.Length > 0))).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiToolProjectsNotStarted = _nat02context.EoiToolProjectsNotStarted.Where(p => p.Tablet == false || (p.Tablet == true && p.TabletCheckedBy.Length > 0)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                toolProjectsNotStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiToolProjectsNotStarted project in eoiToolProjectsNotStarted)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    fontWeight = FontWeights.Normal;
                    fontStyle = FontStyles.Normal;

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    toolProjectsNotStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiToolProjectsNotStarted.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindToolProjectsNotStarted()
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsNotStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsNotStarted");
            }

            ToolProjectsNotStartedExpanders(toolProjectsNotStartedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetToolProjectsStarted()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiToolProjectsStarted> eoiToolProjectsStarted = new List<EoiToolProjectsStarted>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiToolProjectsStarted = _nat02context.EoiToolProjectsStarted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiToolProjectsStarted = _nat02context.EoiToolProjectsStarted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                toolProjectsStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiToolProjectsStarted project in eoiToolProjectsStarted)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    fontWeight = FontWeights.Normal;
                    fontStyle = FontStyles.Normal;

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    toolProjectsStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.ProjectStartedTool, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiToolProjectsStarted.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindToolProjectsStarted()
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsStarted");
            }

            ToolProjectsStartedExpanders(toolProjectsStartedDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetToolProjectsDrawn()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiToolProjectsDrawn> eoiToolProjectsDrawn = new List<EoiToolProjectsDrawn>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    eoiToolProjectsDrawn = _nat02context.EoiToolProjectsDrawn.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiToolProjectsDrawn = _nat02context.EoiToolProjectsDrawn.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                toolProjectsDrawnDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiToolProjectsDrawn project in eoiToolProjectsDrawn)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    fontWeight = FontWeights.Normal;
                    fontStyle = FontStyles.Normal;

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    toolProjectsDrawnDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.ToolDrawnBy, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiToolProjectsDrawn.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindToolProjectsDrawn()
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsDrawn");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsDrawn");
            }

            ToolProjectsDrawnExpanders(toolProjectsDrawnDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetToolProjectsOnHold()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiProjectsOnHold> eoiToolProjectsOnHold = new List<EoiProjectsOnHold>();
                if (User.Department == "Customer Service")
                {
                    string usrName = User.GetUserName().Split(' ')[0];
                    if (usrName == "Gregory") { usrName = "Greg"; }
                    if (usrName == "Nicholas") { usrName = "Nick"; }
                    eoiToolProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Csr.StartsWith(usrName) && p.Tools == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                else
                {
                    eoiToolProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Tools == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
                }
                _nat02context.Dispose();

                toolProjectsOnHoldDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

                foreach (EoiProjectsOnHold project in eoiToolProjectsOnHold)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight fontWeight;
                    FontStyle fontStyle;
                    bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                    bool late = project.DueDate < DateTime.Now.Date;
                    using var nat02context = new NAT02Context();
                    nat02context.Dispose();

                    fontWeight = FontWeights.Normal;
                    fontStyle = FontStyles.Normal;

                    if (priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                        fontStyle = FontStyles.Normal;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        fontWeight = FontWeights.Normal;
                        fontStyle = FontStyles.Normal;
                    }

                    if (late && User.Department == "Engineering")
                    {
                        back = new SolidColorBrush(Colors.Red);
                        fore = new SolidColorBrush(Colors.DarkRed);
                        fontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    toolProjectsOnHoldDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
                }
                eoiToolProjectsOnHold.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindToolProjectsOnHold()
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsOnHold");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsOnHold");
            }

            ToolProjectsOnHoldExpanders(toolProjectsOnHoldDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetDriveWorksQueue()
        {
            try
            {
                using var _driveworkscontext = new DriveWorksContext();
                List<QueueView> queueView = _driveworkscontext.QueueView.OrderBy(t => t.Priority).ThenBy(t => t.DateReleased).ToList();
                _driveworkscontext.Dispose();

                driveWorksQueueDict = new Dictionary<string, (string releasedBy, string tag, string releaseTime, int priority)>();

                foreach (QueueView queueItem in queueView)
                {
                    driveWorksQueueDict.Add(queueItem.TargetName, (queueItem.DisplayName, queueItem.Tags, queueItem.DateReleased.Value.ToShortTimeString(), queueItem.Priority));
                }
            }
            catch (Exception ex)
            {

            }
        }
        private void BindDriveWorksQueue()
        {
            int i = User.VisiblePanels.IndexOf("DriveWorksQueue");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "DriveWorksQueue");
            }

            DriveWorksQueueExpanders(driveWorksQueueDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }
        private void GetNatoliOrderList()
        {
            try
            {
                using var _natbcContext = new NATBCContext();
                List<NatoliOrderList> natoliOrderList = new List<NatoliOrderList>();
                string username = Environment.UserDomainName + "\\" + Environment.UserName;
                if (User.Department == "D1133")
                {
                    natoliOrderList = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
                }
                else if (User.EmployeeCode == "E4408")
                {
                    natoliOrderList = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).ToList();
                    natoliOrderList = natoliOrderList.OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
                }
                else
                {
                    natoliOrderList = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).ToList();
                    natoliOrderList = natoliOrderList.OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
                }
                _natbcContext.Dispose();

                natoliOrderListDict = new Dictionary<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string background)>();

                foreach (NatoliOrderList order in natoliOrderList)
                {
                    int daysToShip = (order.ShipDate.Date - DateTime.Now.Date).Days;
                    SolidColorBrush back;

                    if (daysToShip < 0)
                    {
                        back = new SolidColorBrush(Colors.Red);
                    }
                    else if (daysToShip == 0)
                    {
                        back = new SolidColorBrush(Colors.Orange);
                    }
                    else if (daysToShip > 0 && daysToShip < 4)
                    {
                        back = new SolidColorBrush(Colors.Yellow);
                    }
                    else
                    {
                        back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    natoliOrderListDict.Add((order.OrderNo / 100).ToString(), (order.Customer, order.ShipDate, order.Rush, order.OnHold, order.RepInitials, back.Color.ToString()));
                }
                natoliOrderList.Clear();
            }
            catch (Exception ex)
            {

            }
        }
        private void BindNatoliOrderList()
        {
            int i = User.VisiblePanels.IndexOf("NatoliOrderList");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
            {
                moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

                TextBox textBox = moduleHeader.Children.OfType<TextBox>().First();

                RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

                textBox.TextChanged += NatoliOrderListSearchBox_TextChanged;

                dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

                (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

                BuildPanel(dockPanel, interiorStackPanel, "NatoliOrderList");
            }

            NatoliOrderListExpanders(natoliOrderListDict);

            StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

            ScrollViewer sv = sp.Parent as ScrollViewer;
            if (sv.Visibility != Visibility.Visible)
            {
                sv.Visibility = Visibility.Visible;
                Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
                (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
            }
        }

        /// <summary>
        /// Removes all event handlers subscribed to the specified routed event from the specified element.
        /// </summary>
        /// <param name="element">The UI element on which the routed event is defined.</param>
        /// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        {
            // Get the EventHandlersStore instance which holds event handlers for the specified element.
            // The EventHandlersStore class is declared as internal.
            var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
                "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
            object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

            if (eventHandlersStore == null) return;

            // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
            // for getting an array of the subscribed event handlers.
            var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
                "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
                eventHandlersStore, new object[] { routedEvent });

            // Iteratively remove all routed event handlers from the element.
            try
            {
                foreach (var routedEventHandler in routedEventHandlers)
                    element.RemoveHandler(routedEvent, routedEventHandler.Handler);
            }
            catch
            {

            }
        }
        #endregion

        #region Expander Addition/Subtraction
        private void BeingEnteredExpanders(Dictionary<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("BeingEntered");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

            IEnumerable<double> newOrders = dict.Keys.Except(orders);
            foreach (double order in newOrders)
            {
                int index = dict.Keys.ToList().IndexOf(dict.First(kvp => kvp.Key == order).Key);
                Expander expander = CreateBeingEnteredExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(kvp => kvp.Key == double.Parse(_order)).Value.background)));
            }
            foreach (Expander expander1 in removeThese) { interiorStackPanel.Children.Remove(expander1); }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "BeingEntered").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
        }
        private void InTheOfficeExpanders(Dictionary<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("InTheOffice");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

            IEnumerable<double> newOrders = dict.Keys.Except(orders);
            foreach (double order in newOrders)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
                Expander expander = CreateInTheOfficeExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
            }
            foreach (Expander expander1 in removeThese) { interiorStackPanel.Children.Remove(expander1); }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "InTheOffice").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
        }
        private void QuotesNotConvertedExpanders(Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("QuotesNotConverted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(double, short)> quotes = interiorStackPanel.Children.OfType<Expander>().Select(e => (double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                               , short.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(double, short)> newQuotes = dict.Keys.AsEnumerable().Select(q => (q.quoteNumber, (short)q.revNumber)).Except(quotes);
            foreach ((double, short?) quote in newQuotes)
            {
                int index = dict.ToList().IndexOf(dict.First(o => (o.Key.quoteNumber, (short)o.Key.revNumber) == (quote.Item1, quote.Item2)));
                Expander expander = CreateQuotesNotConvertedExpander(dict.First(x => (x.Key.quoteNumber, (short)x.Key.revNumber) == (quote.Item1, quote.Item2)));
                Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                double _quote = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                short _rev = short.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.quoteNumber, (short)kvp.Key.revNumber) == (_quote, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_quote, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 5)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "QuotesNotConverted").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void OrdersEnteredUnscannedExpanders(Dictionary<double, (string customerName, int daysToShip, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("EnteredUnscanned");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

            IEnumerable<double> newOrders = dict.Keys.Except(orders);
            foreach (double order in newOrders)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
                Expander expander = CreateEnteredUnscannedExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                {
                    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background));
                    foreach (Label label in (expander.Header as Grid).Children.OfType<Label>())
                    {
                        label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.foreground));
                    }
                });

            }
            foreach (Expander expander1 in removeThese) { interiorStackPanel.Children.Remove(expander1); }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "EnteredUnscanned").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
        }
        private void OrdersInEngineeringUnprintedExpanders(Dictionary<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("InEngineering");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

            IEnumerable<double> newOrders = dict.Keys.Except(orders);
            foreach (double order in newOrders)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
                Expander expander = CreateInEngineeringUnprintedExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();

            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "InEngineering").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void QuotesToConvertExpanders(Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("QuotesToConvert");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(double, short)> quotes = interiorStackPanel.Children.OfType<Expander>().Select(e => (double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                               , short.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(double, short)> newQuotes = dict.Keys.AsEnumerable().Select(o => (o.quoteNumber, (short)o.revNumber)).Except(quotes)
                                                              .OrderBy(kvp => dict.First(q => q.Key.quoteNumber == kvp.Item1 && q.Key.revNumber == kvp.Item2).Value.timeSubmitted);
            foreach ((double, short?) quote in newQuotes)
            {
                int index = dict.ToList().IndexOf(dict.First(o => (o.Key.quoteNumber, (short)o.Key.revNumber) == (quote.Item1, quote.Item2)));
                Expander expander = CreateQuotesToConvertExpander(dict.First(q => (q.Key.quoteNumber, (short)q.Key.revNumber) == (quote.Item1, quote.Item2)));
                Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                double _quote = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                short _rev = short.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.quoteNumber, (short)kvp.Key.revNumber) == (_quote, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_quote, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Grid>().First(p => !p.Name.EndsWith("HeaderLabelGrid"));
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Grid>().First(p => !p.Name.EndsWith("HeaderLabelGrid"));
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "QuotesToConvert").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void OrdersReadyToPrintExpanders(Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("ReadyToPrint");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

            IEnumerable<double> newOrders = dict.Keys.Except(orders);
            foreach (double order in newOrders)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
                Expander expander = CreateReadyToPrintExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ReadyToPrint").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void OrdersPrintedInEngineeringExpanders(Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> dict)
        {
            int i = User.VisiblePanels.IndexOf("PrintedInEngineering");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

            IEnumerable<double> newOrders = dict.Keys.Except(orders);
            foreach (double order in newOrders)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
                Expander expander = CreatePrintedInEngineeringExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "PrintedInEngineering").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void AllTabletProjectsExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("AllTabletProjects");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);
            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateAllTabletProjectsExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                try
                {
                    Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
                }
                catch
                {

                }
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                {
                    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict
                                                                                   .First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.background));
                    (expander.Header as Grid).Children[4].SetValue(ContentProperty, dict.First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.drafter);
                });
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "AllTabletProjects").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void TabletProjectsNotStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsNotStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateTabletProjectsNotStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsNotStarted").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void TabletProjectsStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateTabletProjectsStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsStarted").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void TabletProjectsDrawnExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsDrawn");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateTabletProjectsDrawnExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsDrawn").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void TabletProjectsSubmittedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsSubmitted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateTabletProjectsSubmittedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsSubmitted").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void TabletProjectsOnHoldExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("TabletProjectsOnHold");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateTabletProjectsOnHoldExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 15)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsOnHold").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void AllToolProjectsExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("AllToolProjects");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);
            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateAllToolProjectsExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                try
                {
                    Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                {
                    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict
                                                                                   .First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.background));
                    (expander.Header as Grid).Children[4].SetValue(ContentProperty, dict.First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.drafter);
                });
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "AllToolProjects").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void ToolProjectsNotStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsNotStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateToolProjectsNotStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsNotStarted").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void ToolProjectsStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsStarted");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateToolProjectsStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 15)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsStarted").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void ToolProjectsDrawnExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsDrawn");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateToolProjectsDrawnExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 15)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsDrawn").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void ToolProjectsOnHoldExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        {
            int i = User.VisiblePanels.IndexOf("ToolProjectsOnHold");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
                                                                                                          , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

            IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

            foreach ((int, int?) project in newProjects)
            {
                int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Expander expander = CreateToolProjectsOnHoldExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
                int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
                if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
                {
                    removeThese.Add(expander);
                    continue;
                }
                Dispatcher.Invoke(() =>
                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 15)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsOnHold").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void DriveWorksQueueExpanders(Dictionary<string, (string releasedBy, string tag, string releaseTime, int priority)> dict)
        {
            int i = User.VisiblePanels.IndexOf("DriveWorksQueue");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<string> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => (e.Header as Grid).Children[0].GetValue(ContentProperty).ToString());

            IEnumerable<string> newModels = dict.Keys.Except(orders);
            foreach (string model in newModels)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == model));
                Expander expander = CreateDriveWorksQueueExpander(dict.First(x => x.Key == model));
                Dispatcher.Invoke(() =>
                interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string modelName = ((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString();
                if (!dict.Any(kvp => kvp.Key == modelName))
                {
                    removeThese.Add(expander);
                    continue;
                }
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 15)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 5)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "DriveWorksQueue").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        private void NatoliOrderListExpanders(Dictionary<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string background)> dict)
        {
            int i = User.VisiblePanels.IndexOf("NatoliOrderList");
            DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
            Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
            StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

            IEnumerable<string> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => (e.Header as Grid).Children[0].GetValue(ContentProperty).ToString());

            IEnumerable<string> newOrders = dict.Keys.Except(orders).OrderBy(o => dict.First(ol => ol.Key == o).Value.shipDate);
            foreach (string order in newOrders)
            {
                int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
                Expander expander = CreateNatoliOrderListExpander(dict.First(x => x.Key == order));
                Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
            }

            List<Expander> removeThese = new List<Expander>();
            foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
            {
                string order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString();
                if (!dict.Any(kvp => kvp.Key == order))
                {
                    removeThese.Add(expander);
                    continue;
                }
            }
            foreach (Expander expander1 in removeThese)
            {
                interiorStackPanel.Children.Remove(expander1);
            }
            if (dict.Keys.Count > 16)
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 5)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                }
            }
            else
            {
                if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
                {
                    Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                    headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                }
            }

            dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "NatoliOrderList").First().Value;
            dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        }
        #endregion

        #region Expander Creation
        private Expander CreateBeingEnteredExpander(KeyValuePair<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(65)), CreateLabel(kvp.Value.quoteNumber.ToString(), 0, 1, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(30)), CreateLabel(kvp.Value.revNumber.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 3, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.numDaysToShip.ToString(), 0, 4, fontWeight, foreground, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black)
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += OrderDataGrid_MouseRightButtonUp;
            return expander;
        }
        private Expander CreateInTheOfficeExpander(KeyValuePair<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Value.daysInOffice.ToString(), 0, 3, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(100)), CreateLabel(kvp.Value.employeeName, 0, 4, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.csr, 0, 5, fontWeight, foreground, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black)
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += OrderDataGrid_MouseRightButtonUp;

            // expander.Expanded += InTheOfficeExpander_Expanded;
            return expander;
        }
        private Expander CreateEnteredUnscannedExpander(KeyValuePair<double, (string customerName, int daysToShip, string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black)
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += OrdersEnteredUnscannedDataGrid_MouseRightButtonUp;

            // expander.Expanded += EnteredUnscannedExpander_Expanded;
            return expander;
        }
        private Expander CreateInEngineeringUnprintedExpander(KeyValuePair<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysInEng.ToString(), 0, 3, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.employeeName, 0, 4, fontWeight, foreground, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += OrderDataGrid_MouseRightButtonUp;

            expander.Expanded += InEngineeringExpander_Expanded;
            return expander;
        }
        private Expander CreateReadyToPrintExpander(KeyValuePair<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            try
            {
                AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.employeeName, 0, 3, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(105)), CreateLabel(kvp.Value.checkedBy, 0, 4, fontWeight, foreground, null, 14, true));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += OrderDataGrid_MouseRightButtonUp;

            //expander.Expanded += ReadyToPrintExpander_Expanded;
            return expander;
        }
        private Expander CreatePrintedInEngineeringExpander(KeyValuePair<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.employeeName, 0, 3, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.checkedBy, 0, 4, fontWeight, foreground, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += OrderPrintedInEngineeringDataGrid_MouseRightButtonUp;

            //expander.Expanded += PrintedInEngineeringExpander_Expanded;
            return expander;
        }
        private Expander CreateQuotesNotConvertedExpander(KeyValuePair<(double quoteNumber, short? revNumber), (string customerName, string csr, string background, string foreground, string fontWeight)> kvp)
        {
            try
            {
                Grid grid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
                FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
                AddColumn(grid, CreateColumnDefinition(new GridLength(65)), CreateLabel(kvp.Key.quoteNumber.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName, 0, 2, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.csr, 0, 3, fontWeight, foreground, null, 14, true));

                Expander expander = new Expander()
                {
                    IsExpanded = false,
                    Header = grid,
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
                expander.MouseDoubleClick += QuoteDataGrid_MouseDoubleClick;
                expander.PreviewKeyDown += QuoteDataGrid_PreviewKeyDown;
                expander.PreviewMouseDown += QuoteDataGrid_PreviewMouseDown;
                expander.MouseRightButtonUp += QuoteDataGrid_MouseRightButtonUp;

                //expander.Expanded += QuotesNotConvertedExpander_Expanded;
                return expander;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new Expander();
            }
        }
        private Expander CreateQuotesToConvertExpander(KeyValuePair<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> kvp)
        {
            try
            {
                Grid grid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch
                };

                SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
                FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
                AddColumn(grid, CreateColumnDefinition(new GridLength(65)), CreateLabel(kvp.Key.quoteNumber.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName, 0, 2, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(150)), CreateLabel(kvp.Value.csr, 0, 3, fontWeight, foreground, null, 14, true));
                AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Value.daysIn.ToString(), 0, 4, fontWeight, foreground, null, 14, true));

                Expander expander = new Expander()
                {
                    IsExpanded = false,
                    Header = grid,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    ToolTip = kvp.Value.shipment
                };

                expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
                expander.MouseDoubleClick += QuoteDataGrid_MouseDoubleClick;
                expander.PreviewKeyDown += QuoteDataGrid_PreviewKeyDown;
                expander.PreviewMouseDown += QuoteDataGrid_PreviewMouseDown;
                expander.MouseRightButtonUp += QuoteDataGrid_MouseRightButtonUp;

                //expander.Expanded += QuotesNotConvertedExpander_Expanded;
                return expander;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new Expander();
            }
        }
        private Expander CreateAllTabletProjectsExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate.Trim(), 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += AllTabletProjectsDataGrid_MouseRightButtonUp;

            // expander.Expanded += AllTabletProjectsExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateTabletProjectsNotStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 4, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += TabletProjectNotStartedDataGrid_MouseRightButtonUp;

            // expander.Expanded += TabletProjectsNotStartedExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateTabletProjectsStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += TabletProjectStartedDataGrid_MouseRightButtonUp;

            // expander.Expanded += TabletProjectsStartedExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateTabletProjectsDrawnExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += TabletProjectDrawnDataGrid_MouseRightButtonUp;

            // expander.Expanded += TabletProjectsDrawnExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateTabletProjectsSubmittedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += TabletProjectSubmittedDataGrid_MouseRightButtonUp;

            // expander.Expanded += TabletProjectsSubmittedExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateTabletProjectsOnHoldExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.priority.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += TabletProjectOnHoldDataGrid_MouseRightButtonUp;

            // expander.Expanded += TabletProjectsOnHoldExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateAllToolProjectsExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate.Trim(), 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += AllToolProjectsDataGrid_MouseRightButtonUp;

            // expander.Expanded += AllToolProjectsExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateToolProjectsNotStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 4, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += ToolProjectNotStartedDataGrid_MouseRightButtonUp;

            // expander.Expanded += ToolProjectsNotStartedExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateToolProjectsStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += ToolProjectStartedDataGrid_MouseRightButtonUp;

            // expander.Expanded += ToolProjectsStartedExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateToolProjectsDrawnExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += ToolProjectDrawnDataGrid_MouseRightButtonUp;

            // expander.Expanded += ToolProjectsDrawnExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateToolProjectsOnHoldExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.priority.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += ToolProjectOnHoldDataGrid_MouseRightButtonUp;

            // expander.Expanded += ToolProjectsOnHoldExpander_Expanded;
            using var __nat02context = new NAT02Context();
            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
            {
                expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
            }
            __nat02context.Dispose();
            return expander;
        }
        private Expander CreateDriveWorksQueueExpander(KeyValuePair<string, (string releasedBy, string tag, string releaseTime, int priority)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Key.Trim(), 0, 0, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(170)), CreateLabel(kvp.Value.releasedBy.Trim(), 0, 1, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.tag.Trim(), 0, 2, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.releaseTime, 0, 3, FontWeights.Normal, null, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null,
                Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFFFF")
            };

            expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
            expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
            expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
            expander.MouseRightButtonUp += ToolProjectOnHoldDataGrid_MouseRightButtonUp;

            // expander.Expanded += DriveWorksQueueExpander_Expanded;
            return expander;
        }
        private Expander CreateNatoliOrderListExpander(KeyValuePair<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string background)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.shipDate.ToShortDateString(), 0, 2, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Value.rush.Trim(), 0, 3, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Value.onHold, 0, 4, FontWeights.Normal, null, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Value.rep, 0, 5, FontWeights.Normal, null, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black),
                ToolTip = null
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            expander.Expanded += OrderListExpander_Expanded;
            expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;

            return expander;
        }
        #endregion

        #region Module Search Box Text Changed Events
        private void OrdersBeingEnteredSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
            TextBlock textBlock = (sender as TextBox).Template.FindName("SearchTextBlock", sender as TextBox) as TextBlock;
            Image image = (sender as TextBox).Template.FindName("MagImage", (sender as TextBox)) as Image;
            string searchString = textBox.Text.ToLower();

            if (textBox.Text.Length > 0)
            {
                image.Source = ((Image)App.Current.Resources["xImage"]).Source;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                image.Source = ((Image)App.Current.Resources["MagnifyingGlassImage"]).Source;
                textBlock.Visibility = Visibility.Visible;
            }

            // Filter databased on text entry
            var _filtered =
                ordersBeingEnteredDict.Where(o => o.Key.ToString().ToLower().Contains(searchString) ||
                                                  o.Value.quoteNumber.ToString().Contains(searchString) ||
                                                  o.Value.customerName.ToLower().Contains(searchString))
                                      .OrderBy(kvp => kvp.Key)
                                      .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            BeingEnteredExpanders(_filtered);
        }
        private void OrdersInTheOfficeSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
            TextBlock textBlock = (sender as TextBox).Template.FindName("SearchTextBlock", sender as TextBox) as TextBlock;
            Image image = (sender as TextBox).Template.FindName("MagImage", (sender as TextBox)) as Image;
            string searchString = textBox.Text.ToLower();

            if (textBox.Text.Length > 0)
            {
                image.Source = ((Image)App.Current.Resources["xImage"]).Source;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                image.Source = ((Image)App.Current.Resources["MagnifyingGlassImage"]).Source;
                textBlock.Visibility = Visibility.Visible;
            }

            // Filter databased on text entry
            var _filtered =
                ordersInTheOfficeDict.Where(o => o.Key.ToString().ToLower().Contains(searchString) ||
                                                 o.Value.customerName.ToString().Contains(searchString) ||
                                                 o.Value.employeeName.ToLower().Contains(searchString) ||
                                                 o.Value.csr.ToLower().Contains(searchString))
                                     .OrderBy(kvp => kvp.Value.daysToShip)
                                     .ThenBy(kvp => kvp.Value.daysInOffice)
                                     .ThenBy(kvp => kvp.Key)
                                     .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            InTheOfficeExpanders(_filtered);
        }
        private void QuotesNotConvertedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
            TextBlock textBlock = (sender as TextBox).Template.FindName("SearchTextBlock", sender as TextBox) as TextBlock;
            Image image = (sender as TextBox).Template.FindName("MagImage", (sender as TextBox)) as Image;
            string searchString = textBox.Text.ToLower();

            if (textBox.Text.Length > 0)
            {
                image.Source = ((Image)App.Current.Resources["xImage"]).Source;
                image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
                textBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                image.Source = ((Image)App.Current.Resources["MagnifyingGlassImage"]).Source;
                textBlock.Visibility = Visibility.Visible;
            }

            // Filter data based on text entry
            var _filtered =
            quotesNotConvertedDict.Where(p => p.Key.quoteNumber.ToString().ToLower().Contains(searchString) ||
                                              p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                              p.Value.customerName.ToLower().Contains(searchString) ||
                                              p.Value.csr.ToLower().Contains(searchString))
                                  .OrderByDescending(kvp => kvp.Key.quoteNumber)
                                  .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            QuotesNotConvertedExpanders(_filtered);
        }
        private void OrdersEnteredUnscannedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            ordersEnteredUnscannedDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
                                                  p.Value.customerName.ToLower().Contains(searchString))
                                      .OrderBy(kvp => kvp.Key)
                                      .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            OrdersEnteredUnscannedExpanders(_filtered);
        }
        private void OrdersInEngineeringUnprintedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            ordersInEngineeringUnprintedDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
                                                        p.Value.customerName.ToLower().Contains(searchString) ||
                                                        p.Value.employeeName.ToLower().Contains(searchString))
                                            .OrderByDescending(kvp => kvp.Value.daysInEng)
                                            .ThenBy(kvp => kvp.Value.daysToShip)
                                            .ThenBy(kvp => kvp.Key)
                                            .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            OrdersInEngineeringUnprintedExpanders(_filtered);
        }
        private void QuotesToConvertSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            quotesToConvertDict.Where(p => p.Key.quoteNumber.ToString().ToLower().Contains(searchString) ||
                                           p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                           p.Value.customerName.ToLower().Contains(searchString) ||
                                           p.Value.csr.ToLower().Contains(searchString))
                               .OrderBy(kvp => kvp.Key.quoteNumber)
                               .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            QuotesToConvertExpanders(_filtered);
        }
        private void OrdersReadyToPrintSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            ordersReadyToPrintDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
                                              p.Value.customerName.ToLower().Contains(searchString) ||
                                              p.Value.employeeName.ToLower().Contains(searchString) ||
                                              p.Value.checkedBy.ToLower().Contains(searchString))
                                  .OrderBy(kvp => kvp.Key)
                                  .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            OrdersReadyToPrintExpanders(_filtered);
        }
        private void OrdersPrintedInEngineeringSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            ordersPrintedInEngineeringDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
                                                      p.Value.customerName.ToLower().Contains(searchString) ||
                                                      p.Value.employeeName.ToLower().Contains(searchString) ||
                                                      p.Value.checkedBy.ToLower().Contains(searchString))
                                          .OrderByDescending(kvp => kvp.Key)
                                          .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            OrdersPrintedInEngineeringExpanders(_filtered);
        }
        private void AllTabletProjectsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            allTabletProjectsDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                             p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                             p.Value.customerName.ToLower().Contains(searchString) ||
                                             p.Value.csr.ToLower().Contains(searchString) ||
                                             p.Value.drafter.ToLower().Contains(searchString))
                                 .OrderByDescending(kvp => kvp.Value.priority)
                                 .ThenBy(kvp => kvp.Value.dueDate)
                                 .ThenBy(kvp => kvp.Key.projectNumber)
                                 .ToDictionary(x => x.Key, x => x.Value);


            // Remove/Add expanders based on filtering
            AllTabletProjectsExpanders(_filtered);
        }
        private void TabletProjectsNotStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            tabletProjectsNotStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                    p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                    p.Value.customerName.ToLower().Contains(searchString) ||
                                                    p.Value.csr.ToLower().Contains(searchString))
                                        .OrderByDescending(kvp => kvp.Value.priority)
                                        .ThenBy(kvp => kvp.Value.dueDate)
                                        .ThenBy(kvp => kvp.Key.projectNumber)
                                        .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            TabletProjectsNotStartedExpanders(_filtered);
        }
        private void TabletProjectsStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            tabletProjectsStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                 p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                 p.Value.customerName.ToLower().Contains(searchString) ||
                                                 p.Value.csr.ToLower().Contains(searchString) ||
                                                 p.Value.drafter.ToLower().Contains(searchString))
                                     .OrderByDescending(kvp => kvp.Value.priority)
                                     .ThenBy(kvp => kvp.Value.dueDate)
                                     .ThenBy(kvp => kvp.Key.projectNumber)
                                     .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            TabletProjectsStartedExpanders(_filtered);
        }
        private void TabletProjectsDrawnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            tabletProjectsDrawnDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                               p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                               p.Value.customerName.ToLower().Contains(searchString) ||
                                               p.Value.csr.ToLower().Contains(searchString) ||
                                               p.Value.drafter.ToLower().Contains(searchString))
                                   .OrderByDescending(kvp => kvp.Value.priority)
                                   .ThenBy(kvp => kvp.Value.dueDate)
                                   .ThenBy(kvp => kvp.Key.projectNumber)
                                   .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            TabletProjectsDrawnExpanders(_filtered);
        }
        private void TabletProjectsSubmittedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            tabletProjectsSubmittedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                   p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                   p.Value.customerName.ToLower().Contains(searchString) ||
                                                   p.Value.csr.ToLower().Contains(searchString) ||
                                                   p.Value.drafter.ToLower().Contains(searchString))
                                       .OrderByDescending(kvp => kvp.Value.priority)
                                       .ThenBy(kvp => kvp.Value.dueDate)
                                       .ThenBy(kvp => kvp.Key.projectNumber)
                                       .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            TabletProjectsSubmittedExpanders(_filtered);
        }
        private void TabletProjectsOnHoldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            tabletProjectsOnHoldDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                p.Value.customerName.ToLower().Contains(searchString) ||
                                                p.Value.csr.ToLower().Contains(searchString))
                                    .OrderByDescending(kvp => kvp.Value.priority)
                                    .ThenBy(kvp => kvp.Value.dueDate)
                                    .ThenBy(kvp => kvp.Key.projectNumber)
                                    .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            TabletProjectsOnHoldExpanders(_filtered);
        }
        private void AllToolProjectsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            allToolProjectsDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                             p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                             p.Value.customerName.ToLower().Contains(searchString) ||
                                             p.Value.csr.ToLower().Contains(searchString) ||
                                             p.Value.drafter.ToLower().Contains(searchString))
                                 .OrderByDescending(kvp => kvp.Value.priority)
                                 .ThenBy(kvp => kvp.Value.dueDate)
                                 .ThenBy(kvp => kvp.Key.projectNumber)
                                 .ToDictionary(x => x.Key, x => x.Value);


            // Remove/Add expanders based on filtering
            AllToolProjectsExpanders(_filtered);
        }
        private void ToolProjectsNotStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            toolProjectsNotStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                    p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                    p.Value.customerName.ToLower().Contains(searchString) ||
                                                    p.Value.csr.ToLower().Contains(searchString))
                                        .OrderByDescending(kvp => kvp.Value.priority)
                                        .ThenBy(kvp => kvp.Value.dueDate)
                                        .ThenBy(kvp => kvp.Key.projectNumber)
                                        .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            ToolProjectsNotStartedExpanders(_filtered);
        }
        private void ToolProjectsStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            toolProjectsStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                 p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                 p.Value.customerName.ToLower().Contains(searchString) ||
                                                 p.Value.csr.ToLower().Contains(searchString) ||
                                                 p.Value.drafter.ToLower().Contains(searchString))
                                     .OrderByDescending(kvp => kvp.Value.priority)
                                     .ThenBy(kvp => kvp.Value.dueDate)
                                     .ThenBy(kvp => kvp.Key.projectNumber)
                                     .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            ToolProjectsStartedExpanders(_filtered);
        }
        private void ToolProjectsDrawnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            toolProjectsDrawnDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                               p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                               p.Value.customerName.ToLower().Contains(searchString) ||
                                               p.Value.csr.ToLower().Contains(searchString) ||
                                               p.Value.drafter.ToLower().Contains(searchString))
                                   .OrderByDescending(kvp => kvp.Value.priority)
                                   .ThenBy(kvp => kvp.Value.dueDate)
                                   .ThenBy(kvp => kvp.Key.projectNumber)
                                   .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            ToolProjectsDrawnExpanders(_filtered);
        }
        private void ToolProjectsOnHoldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            toolProjectsOnHoldDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
                                                p.Value.customerName.ToLower().Contains(searchString) ||
                                                p.Value.csr.ToLower().Contains(searchString))
                                    .OrderByDescending(kvp => kvp.Value.priority)
                                    .ThenBy(kvp => kvp.Value.dueDate)
                                    .ThenBy(kvp => kvp.Key.projectNumber)
                                    .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            ToolProjectsOnHoldExpanders(_filtered);
        }
        private void DriveWorksQueueSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            driveWorksQueueDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
                                           p.Value.releasedBy.ToLower().Contains(searchString) ||
                                           p.Value.tag.ToLower().Contains(searchString))
                               .OrderBy(kvp => kvp.Value.priority)
                               .ThenBy(kvp => kvp.Value.releaseTime)
                               .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            DriveWorksQueueExpanders(_filtered);
        }
        private void NatoliOrderListSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchString = ((sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox).Text.ToLower();
            // Filter data based on text entry
            var _filtered =
            natoliOrderListDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
                                           p.Value.customerName.ToLower().Contains(searchString))
                               .OrderBy(kvp => kvp.Value.shipDate)
                               .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            NatoliOrderListExpanders(_filtered);
        }
        #endregion

        #region Expanders Expanding Events
        private void OrderListExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = (Expander)sender;
            Grid grid = (Grid)expander.Header;
            UIElementCollection collection = grid.Children;
            string orderNumber = collection[0].GetValue(ContentProperty).ToString() + "00";
            using var _natbcContext = new NATBCContext();

            List<LineItemLastScan> lines = _natbcContext.LineItemLastScan.FromSqlRaw("SELECT DISTINCT OrderDetailTypeDescription, OrderLineNumber, (SELECT TOP 1 ScanTimeStamp FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'ScanTimeStamp', (SELECT TOP 1 DepartmentDesc FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Department', (SELECT TOP 1 EmployeeName FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Employee' FROM NATBC.dbo.TravellerScansAudit TSA WITH (NOLOCK) WHERE TSA.OrderNumber = {0} AND TSA.OrderDetailTypeID NOT IN('E','H','MC','RET','T','TM','Z') AND TSA.OrderDetailTypeDescription <> 'PARTS' AND TSA.DepartmentDesc <> 'Production Mgmnt' ORDER BY OrderLineNumber", orderNumber).ToList();
            _natbcContext.Dispose();

            StackPanel lineItemsStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            foreach (LineItemLastScan lineItem in lines)
            {
                Grid lineItemGrid = new Grid();
                // lineItemGrid.Width = expander.Width - 30 - 22;
                lineItemGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.OrderDetailTypeDescription, 0, 0, FontWeights.Normal));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(string.Format("{0:d} {0:t}", lineItem.ScanTimeStamp), 0, 1, FontWeights.Normal));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.Department, 0, 2, FontWeights.Normal));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.Employee, 0, 3, FontWeights.Normal));

                lineItemsStackPanel.Children.Add(lineItemGrid);
            }

            expander.Content = lineItemsStackPanel;
        }
        private void InEngineeringExpander_Expanded(object sender, RoutedEventArgs e)
        {
            Expander expander = sender as Expander;
            string orderNumber = (expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString() + "00";
            using var _natbcContext = new NATBCContext();

            List<LineItemLastScan> lines = _natbcContext.LineItemLastScan.FromSqlRaw("SELECT DISTINCT OrderDetailTypeDescription, OrderLineNumber, (SELECT TOP 1 ScanTimeStamp FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'ScanTimeStamp', (SELECT TOP 1 DepartmentDesc FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Department', (SELECT TOP 1 EmployeeName FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Employee' FROM NATBC.dbo.TravellerScansAudit TSA WITH (NOLOCK) WHERE TSA.OrderNumber = {0} AND TSA.OrderDetailTypeID NOT IN('E','H','MC','RET','T','TM','Z') AND TSA.OrderDetailTypeDescription <> 'PARTS' AND TSA.DepartmentDesc <> 'Production Mgmnt' ORDER BY OrderLineNumber", orderNumber).ToList();
            _natbcContext.Dispose();

            StackPanel lineItemsStackPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical
            };

            foreach (LineItemLastScan lineItem in lines)
            {
                Grid lineItemGrid = new Grid();
                // lineItemGrid.Width = expander.Width - 30 - 22;
                lineItemGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

                bool isChecked = selectedOrders.Any(o => o.Item1.Contains((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString())) ||
                                 selectedLineItems.Any(o => o.Contains(orderNumber) && o.Substring(1, 2) == lineItem.OrderLineNumber.ToString("00"));

                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(36)));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(18)), CreateCheckBox(0, 1, isChecked));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.OrderDetailTypeDescription, 0, 2, FontWeights.Normal));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(string.Format("{0:d} {0:t}", lineItem.ScanTimeStamp), 0, 3, FontWeights.Normal));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(lineItem.Department, 0, 4, FontWeights.Normal));
                AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(150)), CreateLabel(lineItem.Employee, 0, 5, FontWeights.Normal));

                lineItemGrid.Tag = lineItem.OrderLineNumber;

                lineItemsStackPanel.Children.Add(lineItemGrid);
            }

            expander.Content = lineItemsStackPanel;
        }
        #endregion
        #endregion

        #region DataGridLoadingRow
        private void InEngineeringDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiOrdersInEngineeringUnprintedView rowView = dataGrid.Items[index] as EoiOrdersInEngineeringUnprintedView;
                bool rush = rowView.RushYorN.ToString().Trim() == "Y" || rowView.PaidRushFee.ToString().Trim() == "Y";
                bool doNotProcess = Convert.ToBoolean(rowView.DoNotProcess);
                bool beingChecked = Convert.ToBoolean(rowView.BeingChecked);
                bool markedForChecking = Convert.ToBoolean(rowView.MarkedForChecking);
                if (rush)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (doNotProcess)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Pink);
                }
                else
                {
                    if (beingChecked && User.Department == "Engineering")
                    {
                        e.Row.Background = new SolidColorBrush(Colors.DodgerBlue);
                    }
                    //else if (count == 0 && (machineType == "BB" || machineType == "B" || machineType == "D") && lineType.Count != 0)
                    //{
                    //    e.Row.Background = new SolidColorBrush(Colors.Red);
                    //}
                    else if (markedForChecking)
                    {
                        e.Row.Background = new SolidColorBrush(Colors.GreenYellow);
                    }
                    else
                    {
                        e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                }
            }
            catch //(Exception ex)
            {
                // MessageBox.Show(ex.Message);
                // WriteToErrorLog("InEngineeringDataGrid_LoadingRow", ex.Message);
            }
        }

        private void ReadyToPrintDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiOrdersReadyToPrintView rowView = dataGrid.Items[index] as EoiOrdersReadyToPrintView;
                double orderNumber = rowView.OrderNo;
                bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
                bool tm2 = Convert.ToBoolean(rowView.TM2);
                bool tabletPrints = Convert.ToBoolean(rowView.Tablet);
                bool toolPrints = Convert.ToBoolean(rowView.Tool);
                List<OrderDetails> orderDetails;
                List<OrderHeader> orderHeader;
                orderDetails = _nat01context.OrderDetails.Where(o => o.OrderNo == orderNumber * 100).ToList();
                orderHeader = _nat01context.OrderHeader.Where(o => o.OrderNo == orderNumber * 100).ToList();
                if (rush)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }

                if (tm2 || tabletPrints)
                {
                    foreach (OrderDetails od in orderDetails)
                    {
                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
                        {
                            string path = @"\\engserver\workstations\tool_drawings\" + orderNumber + @"\" + od.HobNoShapeId.Trim() + ".pdf";
                            if (!System.IO.File.Exists(path))
                            {
                                goto Missing;
                            }
                        }
                    }
                }

                if (tm2 || toolPrints)
                {
                    foreach (OrderDetails od in orderDetails)
                    {
                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
                        {
                            string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
                            detailType = detailType == "MISC" ? "REJECT" : detailType;
                            string international = orderHeader.FirstOrDefault().UnitOfMeasure;
                            string path = @"\\engserver\workstations\tool_drawings\" + orderNumber + @"\" + detailType + ".pdf";
                            if (!System.IO.File.Exists(path))
                            {
                                goto Missing;
                            }
                            if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                            {
                                goto Missing;
                            }
                        }
                    }
                }

                goto NotMissing;

            Missing:;
                if (User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
                goto Finished;

            NotMissing:;
                e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

            Finished:;
            }
            catch
            {

            }
        }

        private void InTheOfficeDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiOrdersInOfficeView rowView = dataGrid.Items[index] as EoiOrdersInOfficeView;
                int? orderNumber = rowView.OrderNo;
                bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
                bool doNotProcess = Convert.ToBoolean(rowView.DoNotProcess);
                if (rush)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (doNotProcess)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Pink);
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            {

            }
        }

        private void EnteredUnscannedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiOrdersEnteredAndUnscannedView rowView = dataGrid.Items[index] as EoiOrdersEnteredAndUnscannedView;
                bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
                bool doNotProcess = Convert.ToBoolean(rowView.DoNotProcess);
                string[] errRes;
                errRes = new string[2] { rowView.ProcessState,
                          rowView.TransitionName };
                if (rush)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.White);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (doNotProcess)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Pink);
                }
                else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            {
            }
        }

        private void BeingEnteredDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiOrdersBeingEnteredView rowView = dataGrid.Items[index] as EoiOrdersBeingEnteredView;
                bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
                if (rush)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
            }
            catch
            {
            }
        }

        private void QuotesNotConvertedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiQuotesNotConvertedView rowView = dataGrid.Items[index] as EoiQuotesNotConvertedView;
                int daysOld = (DateTime.Now - _nat01context.QuoteHeader.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).Select(q => q.QuoteDate).First()).Days;
                string rush = rowView.RushYorN;
                if (rush == "Y")
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (daysOld > 6)
                {
                    using var _nat02context = new NAT02Context();
                    if (!_nat02context.EoiQuotesOneWeekCompleted.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).Any())
                    {
                        e.Row.Background = new SolidColorBrush(Colors.Pink);
                    }
                    else
                    {
                        e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                    }
                    _nat02context.Dispose();
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            {
            }
        }

        private void QuotesToConvertDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiQuotesMarkedForConversionView rowView = dataGrid.Items[index] as EoiQuotesMarkedForConversionView;
                string rush = rowView.Rush.Trim();
                e.Row.ToolTip = null;
                if (rush == "Y")
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                using var _nat01context = new NAT01Context();
                e.Row.ToolTip = string.IsNullOrEmpty(_nat01context.QuoteHeader.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).First().Shipment.Trim()) ? "No Comment" : _nat01context.QuoteHeader.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).First().Shipment.Trim();
            }
            catch
            {
            }
        }

        private void PrintedInEngineeringDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiOrdersPrintedInEngineeringView rowView = dataGrid.Items[index] as EoiOrdersPrintedInEngineeringView;
                bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
                bool tm2 = Convert.ToBoolean(rowView.TM2);
                bool tabletPrints = Convert.ToBoolean(rowView.Tablet);
                bool toolPrints = Convert.ToBoolean(rowView.Tool);
                List<OrderDetails> orderDetails;
                List<OrderHeader> orderHeader;
                orderDetails = _nat01context.OrderDetails.Where(o => o.OrderNo == rowView.OrderNo * 100).ToList();
                orderHeader = _nat01context.OrderHeader.Where(o => o.OrderNo == rowView.OrderNo * 100).ToList();
                if (rush)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }

                if (tm2 || tabletPrints)
                {
                    foreach (OrderDetails od in orderDetails)
                    {
                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
                        {
                            string path = @"\\engserver\workstations\tool_drawings\" + rowView.OrderNo + @"\" + od.HobNoShapeId.Trim() + ".pdf";
                            if (!System.IO.File.Exists(path))
                            {
                                goto Missing;
                            }
                        }
                    }
                }

                if (tm2 || toolPrints)
                {
                    foreach (OrderDetails od in orderDetails)
                    {
                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
                        {
                            string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
                            detailType = detailType == "MISC" ? "REJECT" : detailType;
                            string international = orderHeader.FirstOrDefault().UnitOfMeasure;
                            string path = @"\\engserver\workstations\tool_drawings\" + rowView.OrderNo + @"\" + detailType + ".pdf";
                            if (!System.IO.File.Exists(path))
                            {
                                goto Missing;
                            }
                            if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                            {
                                goto Missing;
                            }
                        }
                    }
                }

                goto NotMissing;

            Missing:;
                if (User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
                goto Finished;

            NotMissing:;
                e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

            Finished:;
            }
            catch
            {

            }
        }

        private void AllTabletProjectsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiAllTabletProjectsView rowView = dataGrid.Items[index] as EoiAllTabletProjectsView;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                using var _nat02context = new NAT02Context();
                bool finished = _nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).Any();
                _nat02context.Dispose();
                bool onHold = rowView.HoldStatus == "On Hold";
                bool submitted = rowView.TabletSubmittedBy is null ? false : rowView.TabletSubmittedBy.Length > 0;
                bool drawn = rowView.TabletDrawnBy.Length > 0;
                bool started = rowView.ProjectStartedTablet.Length > 0;
                e.Row.ToolTip = null;
                if ((bool)rowView.Tools)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    e.Row.FontWeight = FontWeights.Normal;
                    e.Row.FontStyle = FontStyles.Normal;
                }
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (onHold)
                {
                    e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
                    using var __nat02context = new NAT02Context();
                    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber))
                    {
                        e.Row.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim();
                    }
                    __nat02context.Dispose();
                }
                else if (finished)
                {
                    e.Row.Background = new SolidColorBrush(Colors.GreenYellow);
                }
                else if (submitted)
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0A7DFF"));
                }
                else if (drawn)
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#52A3FF"));
                }
                else if (started)
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            {
            }
        }

        private void TabletProjectsNotStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiTabletProjectsNotStarted rowView = dataGrid.Items[index] as EoiTabletProjectsNotStarted;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if ((bool)rowView.Tools)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    e.Row.FontWeight = FontWeights.Normal;
                    e.Row.FontStyle = FontStyles.Normal;
                }
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void TabletProjectsStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiTabletProjectsStarted rowView = dataGrid.Items[index] as EoiTabletProjectsStarted;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if ((bool)rowView.Tools)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    e.Row.FontWeight = FontWeights.Normal;
                    e.Row.FontStyle = FontStyles.Normal;
                }
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void TabletProjectsDrawnDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiTabletProjectsDrawn rowView = dataGrid.Items[index] as EoiTabletProjectsDrawn;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if ((bool)rowView.Tools)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    e.Row.FontWeight = FontWeights.Normal;
                    e.Row.FontStyle = FontStyles.Normal;
                }
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void TabletProjectsSubmittedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiTabletProjectsSubmitted rowView = dataGrid.Items[index] as EoiTabletProjectsSubmitted;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if ((bool)rowView.Tools)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.FontStyle = FontStyles.Oblique;
                }
                else
                {
                    e.Row.FontWeight = FontWeights.Normal;
                    e.Row.FontStyle = FontStyles.Normal;
                }
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void TabletProjectsOnHoldDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiProjectsOnHold rowView = dataGrid.Items[index] as EoiProjectsOnHold;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if ((bool)rowView.Tools)
                {
                    e.Row.FontWeight = FontWeights.Bold;
                    e.Row.FontStyle = FontStyles.Oblique;
                }
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
                e.Row.ToolTip = string.IsNullOrEmpty(rowView.OnHoldComment.Trim()) ? "No Comment" : rowView.OnHoldComment.Trim();
            }
            catch
            { }
        }

        private void AllToolProjectsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiAllToolProjectsView rowView = dataGrid.Items[index] as EoiAllToolProjectsView;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                using var _nat02context = new NAT02Context();
                bool finished = _nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).Any();
                _nat02context.Dispose();
                using var _projectscontext = new ProjectsContext();
                bool tablet = (bool)_projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().Tablet &&
                              _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().TabletCheckedBy.Trim().Length == 0;
                bool multi_tip = (bool)_projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().MultiTipSketch;
                _projectscontext.Dispose();
                bool onHold = rowView.HoldStatus == "On Hold";
                bool drawn = rowView.ToolDrawnBy.Trim().Length > 0;
                bool started = rowView.ProjectStartedTool.Trim().Length > 0;
                e.Row.ToolTip = null;
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (onHold)
                {
                    e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
                    using var __nat02context = new NAT02Context();
                    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber))
                    {
                        e.Row.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim();
                    }
                    __nat02context.Dispose();
                }
                else if (finished)
                {
                    e.Row.Background = new SolidColorBrush(Colors.GreenYellow);
                }
                else if (drawn)
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3594FF"));
                }
                else if (started)
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
                }
                else if (multi_tip)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Gray);
                }
                else if (tablet)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void ToolProjectsNotStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiToolProjectsNotStarted rowView = dataGrid.Items[index] as EoiToolProjectsNotStarted;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                using var _projectscontext = new ProjectsContext();
                bool multi_tip = (bool)_projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().MultiTipSketch;
                _projectscontext.Dispose();
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (multi_tip)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Gray);
                }
                else if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void ToolProjectsStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiToolProjectsStarted rowView = dataGrid.Items[index] as EoiToolProjectsStarted;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void ToolProjectsDrawnDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiToolProjectsDrawn rowView = dataGrid.Items[index] as EoiToolProjectsDrawn;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }

        private void ToolProjectsOnHoldDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                EoiProjectsOnHold rowView = dataGrid.Items[index] as EoiProjectsOnHold;
                bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
                bool late = rowView.DueDate < DateTime.Now.Date;
                if (priority)
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                    e.Row.FontWeight = FontWeights.Normal;
                }
                if (late && User.Department == "Engineering")
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
                    e.Row.FontWeight = FontWeights.Bold;
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
                e.Row.ToolTip = string.IsNullOrEmpty(rowView.OnHoldComment.Trim()) ? "No Comment" : rowView.OnHoldComment.Trim();
            }
            catch
            { }
        }

        private void NatoliOrderListDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            try
            {
                DataGrid dataGrid = sender as DataGrid;
                int index = e.Row.GetIndex();
                NatoliOrderList rowView = dataGrid.Items[index] as NatoliOrderList;
                int daysToShip = (rowView.ShipDate.Date - DateTime.Now.Date).Days;
                if (daysToShip < 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Red);
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (daysToShip == 0)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Orange);
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                }
                else if (daysToShip > 0 && daysToShip < 4)
                {
                    e.Row.Background = new SolidColorBrush(Colors.Yellow);
                    e.Row.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
                }
            }
            catch
            { }
        }
        #endregion

        #region Folder Management
        private void QuotesAndOrders()
        {
            FindMissingOrders(ListTop100Orders());
            FindExtraQuoteFolders();
        }

        static List<int> ListTop100OrderFolders()
        {
            string path = @"\\nsql03\data1\WorkOrders";
            string[] f = System.IO.Directory.GetDirectories(path);
            List<string> folders = new List<string>();
            foreach (string folder in f)
            {
                folders.Add(folder);
            }
            folders.Sort();
            folders.Reverse();
            folders.RemoveRange(100, folders.Count - 100);
            List<int> orders = new List<int>();
            folders.ForEach(x => orders.Add(int.Parse(x.Substring(x.LastIndexOf(@"\") + 1))));
            return orders;
        }

        static Dictionary<double, double> ListTop100Orders()
        {
            string connectionString = @"Data Source=NSQL05;Initial Catalog=Projects;Persist Security Info=True; User ID=DWInterferenceUser;Password=PrivateKey(86)";
            const string GetTop100OrdersQuery = "SELECT TOP 100 OrderNo / 100, QuoteNumber FROM NAT01.dbo.OrderHeader ORDER BY OrderNo DESC";

            var orders = new Dictionary<double, double>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetTop100OrdersQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    orders.Add(reader.GetDouble(0), reader.GetDouble(1));
                                }
                            }
                        }
                    }
                }
                return orders;
            }
            catch (Exception eSql)
            {
                Console.WriteLine("Exception: " + eSql.Message);
            }
            return null;
        }

        static void FindMissingOrders(Dictionary<double, double> orders_in)
        {
            List<int> folders = ListTop100OrderFolders();
            IEnumerable<KeyValuePair<double, double>> orders = orders_in.Where(kvp => !folders.Contains((int)kvp.Key));
            if (orders.Any())
            {
                foreach (KeyValuePair<double, double> kvp in orders)
                {
                    string path = @"\\nsql03\data1\Quotes\" + kvp.Value;
                    if (System.IO.Directory.Exists(path))
                    {
                        if (System.IO.Directory.Exists(path.Replace(@"Quotes\" + kvp.Value, @"WorkOrders\" + kvp.Key)))
                        {
                            string[] files = System.IO.Directory.GetFiles(path);
                            foreach (string file in files)
                            {
                                System.IO.File.Move(file, file.Replace(@"Quotes\" + kvp.Value, @"WorkOrders\" + kvp.Key));
                            }
                            System.IO.Directory.Delete(path);
                        }
                        else
                        {
                            try
                            {
                                System.IO.Directory.Move(path, path.Replace(@"Quotes\" + kvp.Value, @"WorkOrders\" + kvp.Key));
                            }
                            catch
                            {

                            }
                        }
                    }
                    else
                    {
                        System.IO.Directory.CreateDirectory(path.Replace(@"Quotes\" + kvp.Value, @"WorkOrders\" + kvp.Key));
                    }
                }
            }
            else
            {
                Console.WriteLine("Empty");
            }
        }

        static void FindExtraQuoteFolders()
        {
            string connectionString = @"Data Source=NSQL05;Initial Catalog=Projects;Persist Security Info=True; User ID=DWInterferenceUser;Password=PrivateKey(86)";
            const string GetTop100OrdersQuery = "SELECT TOP 250 (OrderNo / 100) AS 'OrderNo', QuoteNumber FROM NAT01.dbo.OrderHeader ORDER BY OrderNo DESC";

            var orders = new Dictionary<double, double>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = GetTop100OrdersQuery;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    double order = reader.GetDouble(0);
                                    double quote = reader.GetDouble(1);
                                    if (System.IO.Directory.Exists(ORDER_PATH + order))
                                    {
                                        if (System.IO.Directory.Exists(QUOTE_PATH + quote))
                                        {
                                            string[] files = System.IO.Directory.GetFiles(QUOTE_PATH + quote);
                                            if (files.Length > 0)
                                            {
                                                foreach (string file in files)
                                                {
                                                    System.IO.File.Move(file, file.Replace(@"Quotes\" + quote, @"WorkOrders\" + order));
                                                }
                                            }
                                            string[] folders = System.IO.Directory.GetDirectories(QUOTE_PATH + quote);
                                            if (folders.Length > 0)
                                            {
                                                foreach (string folder in folders)
                                                {
                                                    string replacePath = folder.Replace(@"Quotes\" + quote, @"WorkOrders\" + order);
                                                    System.IO.Directory.Move(folder, replacePath);
                                                }
                                            }
                                            System.IO.Directory.Delete(QUOTE_PATH + quote);
                                            orders.Add(order, quote);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception eSql)
            {
                Console.WriteLine("Exception: " + eSql.Message);
            }
        }

        public void GetPercentages()
        {
            string connectionString = @"Data Source=NSQL05;Initial Catalog=NAT01;Persist Security Info=True; User ID=DWInterferenceUser;Password=PrivateKey(86)";
            const string query = "SELECT * FROM (SELECT COUNT(OrderNo) AS NumberOfOrders2018 FROM NAT01.dbo.OrderHeader OH WITH (NOLOCK) WHERE " +
                                 "OH.OrderDate >= CONCAT('01-01-', DATEPART(year, DATEADD(year, -1, GETDATE()))) AND OH.OrderDate <= " +
                                 "DATEADD(year, -1, GETDATE())) a JOIN (SELECT COUNT(OrderNo) AS NumberOfOrders2019 FROM NAT01.dbo.OrderHeader OH WITH (NOLOCK) WHERE " +
                                 "OH.OrderDate >= CONCAT('01-01-', DATEPART(year, GETDATE())) AND OH.OrderDate <= GETDATE()) b ON a.NumberOfOrders2018 <> b.NumberOfOrders2019";

            List<int> devs = new List<int>();
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    if (conn.State == System.Data.ConnectionState.Open)
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = query;
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    devs.Add(reader.GetInt32(0));
                                    devs.Add(reader.GetInt32(1));
                                }
                            }
                        }
                    }
                }
                Title = "Natoli Order Interface      " + string.Format("{0:P2}", (double)devs[1] / devs[0] - 1);
            }
            catch (Exception eSql)
            {
                Console.WriteLine("Exception: " + eSql.Message);
            }
        }
        #endregion        

        #region DataGrid Events
        private void OrderDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using var _context = new NAT02Context();
            using var _nat01context = new NAT01Context();
            Expander expander = (Expander)sender;
            Cursor = Cursors.AppStarting;

            try
            {
                Grid grid = expander.Header as Grid;
                string orderNumber = grid.Children[0].GetValue(ContentProperty).ToString();
                workOrder = new WorkOrder(int.Parse(orderNumber));
                WindowCollection collection = App.Current.Windows;
                foreach (Window w in collection)
                {
                    if (w.Title.Contains(workOrder.OrderNumber.ToString()))
                    {
                        _context.Dispose();
                        _nat01context.Dispose();
                        w.WindowState = WindowState.Normal;
                        w.Show();
                        goto AlreadyOpen2;
                    }
                }
                if (_context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User != User.GetUserName()))
                {
                    MessageBox.Show("BEWARE!!\n" + _context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User != User.GetUserName()).FirstOrDefault().User + " is in this order at the moment.");
                    mainTimer.Stop();
                }
                else if (_context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User == User.GetUserName()))
                {
                    MessageBox.Show("You already have this order open.");
                    _context.Dispose();
                    _nat01context.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                string location = headers.Where(kvp => kvp.Value == (((expander.Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString()).First().Key;

                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, this, location, User)
                {
                    Left = Left,
                    Top = Top
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        AlreadyOpen2:
            _context.Dispose();
            _nat01context.Dispose();
            expander.IsExpanded = false;
            Cursor = Cursors.Arrow;
        }

        private void OrderDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Cursor = Cursors.AppStarting;
                Expander expander = (Expander)sender;
                using var context = new NAT02Context();
                using NAT01Context nat01context = new NAT01Context();
                try
                {
                    Grid grid = expander.Header as Grid;
                    string orderNumber = grid.Children[0].GetValue(ContentProperty).ToString();
                    workOrder = new WorkOrder(int.Parse(orderNumber));
                    WindowCollection collection = App.Current.Windows;
                    foreach (Window w in collection)
                    {
                        if (w.Title.Contains(workOrder.OrderNumber.ToString()))
                        {
                            context.Dispose();
                            nat01context.Dispose();
                            w.WindowState = WindowState.Normal;
                            w.Show();
                            goto AlreadyOpen;
                        }
                    }
                    if (context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User != User.GetUserName()))
                    {
                        MessageBox.Show("BEWARE!!\n" + context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User != User.GetUserName()).FirstOrDefault().User + " is in this order at the moment.");
                        mainTimer.Stop();
                    }
                    else if (context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User == User.GetUserName()))
                    {
                        MessageBox.Show("You already have this order open.");
                        context.Dispose();
                        nat01context.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    string location = headers.Where(kvp => kvp.Value == (((expander.Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString()).First().Key;
                    OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, this, location, User)
                    {
                        Left = Left,
                        Top = Top
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            AlreadyOpen:
                context.Dispose();
                nat01context.Dispose();
                Cursor = Cursors.Arrow;
            }
        }

        private void QuoteDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            using var nat01context = new NAT01Context();
            Expander expander = sender as Expander;
            Grid grid = expander.Header as Grid;
            Cursor = Cursors.AppStarting;
            try
            {
                int quoteNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                short revNumber = short.Parse(grid.Children[1].GetValue(ContentProperty).ToString());
                WindowCollection collection = App.Current.Windows;
                foreach (Window w in collection)
                {
                    if (w.Title.Contains(quoteNumber.ToString()))
                    {
                        nat01context.Dispose();
                        w.WindowState = WindowState.Normal;
                        w.Show();
                        goto AlreadyOpen;
                    }
                }
                quote = new Quote(quoteNumber, revNumber);
                mainTimer.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, "", User)
                {
                    Left = Left,
                    Top = Top
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        AlreadyOpen:
            nat01context.Dispose();
            Cursor = Cursors.Arrow;
        }

        private void QuoteDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Cursor = Cursors.AppStarting;
                using var nat01context = new NAT01Context();
                Expander expander = sender as Expander;
                Grid grid = expander.Header as Grid;
                Cursor = Cursors.AppStarting;
                try
                {
                    int quoteNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    short revNumber = short.Parse(grid.Children[1].GetValue(ContentProperty).ToString());
                    WindowCollection collection = App.Current.Windows;
                    foreach (Window w in collection)
                    {
                        if (w.Title.Contains(quoteNumber.ToString()))
                        {
                            nat01context.Dispose();
                            w.WindowState = WindowState.Normal;
                            w.Show();
                            goto AlreadyOpen;
                        }
                    }
                    quote = new Quote(quoteNumber, revNumber);
                    mainTimer.Stop();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, "", User)
                    {
                        Left = Left,
                        Top = Top
                    };
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            AlreadyOpen:
                nat01context.Dispose();
                Cursor = Cursors.Arrow;
            }
        }

        private void OrderDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl) || sender.GetType().Name == "Expander")
            {

            }
            else
            {
                //(sender as DataGrid).SelectedItem = null;
            }
        }

        private void QuoteDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {

            }
            else
            {
                //(sender as DataGrid).SelectedItem = null;
            }
        }

        private void OrderDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem toOfficeOrder = new MenuItem
            {
                Header = "Send to Office"
            };

            Expander expander = sender as Expander;
            toOfficeOrder.Click += SendToOfficeMenuItem_Click;

            RightClickMenu.Items.Add(toOfficeOrder);
            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;
            _orderNumber = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        }

        private void OrdersEnteredUnscannedDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem toOfficeOrder = new MenuItem
            {
                Header = "Send to Office"
            };
            MenuItem startOrder = new MenuItem
            {
                Header = "Start Order"
            };

            Expander expander = sender as Expander;
            toOfficeOrder.Click += SendToOfficeMenuItem_Click;
            startOrder.Click += StartWorkOrder_Click;

            RightClickMenu.Items.Add(toOfficeOrder);
            RightClickMenu.Items.Add(startOrder);
            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            _orderNumber = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        }

        private void OrderPrintedInEngineeringDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem toOfficeOrder = new MenuItem
            {
                Header = "Send to Office"
            };
            MenuItem toProdManOrder = new MenuItem
            {
                Header = "Send to Production"
            };

            Expander expander = sender as Expander;
            toOfficeOrder.Click += SendToOfficeMenuItem_Click;
            toProdManOrder.Click += ToProdManOrder_Click;

            RightClickMenu.Items.Add(toOfficeOrder);
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E3236")
            {
                RightClickMenu.Items.Add(toProdManOrder);
            }
            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            _orderNumber = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        }

        private void ProjectDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Expander expander = sender as Expander;
            Grid grid = expander.Header as Grid;
            Cursor = Cursors.AppStarting;
            try
            {
                string projectNumber = grid.Children[0].GetValue(ContentProperty).ToString();
                string revNumber = grid.Children[1].GetValue(ContentProperty).ToString();
                try
                {
                    string path = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + @"\"; // + (revNumber != "0" ? "_" + revNumber : "")
                    if (!System.IO.Directory.Exists(path))
                        System.IO.Directory.CreateDirectory(path);
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
                    //ProjectWindow projectWindow = new ProjectWindow(projectNumber, revNumber, this, User, false);
                    //projectWindow.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Cursor = Cursors.Arrow;
        }

        private void ProjectDataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Cursor = Cursors.AppStarting;
                try
                {
                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;
                    string projectNumber = grid.Children[0].GetValue(ContentProperty).ToString();
                    string revNumber = grid.Children[1].GetValue(ContentProperty).ToString();
                    try
                    {
                        string path = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + @"\"; // + (revNumber != "0" ? "_" + revNumber : "")
                        if (!System.IO.Directory.Exists(path))
                            System.IO.Directory.CreateDirectory(path);
                        System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                Cursor = Cursors.Arrow;
            }
        }

        private void ProjectDataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right || Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {

            }
            else
            {
                // (sender as DataGrid).SelectedItem = null;
            }
        }

        private void AllTabletProjectsDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ContextMenu RightClickMenu = new ContextMenu();
                MenuItem completedTabletProject = new MenuItem
                {
                    Header = "Completed",
                    IsEnabled = false
                };
                MenuItem startTabletProject = new MenuItem
                {
                    Header = "Start",
                    IsEnabled = false
                };
                MenuItem finishTabletProject = new MenuItem
                {
                    Header = "Finish",
                    IsEnabled = false
                };
                MenuItem submitTabletProject = new MenuItem
                {
                    Header = "Submit",
                    IsEnabled = false
                };
                MenuItem checkTabletProject = new MenuItem
                {
                    Header = "Check",
                    IsEnabled = false
                };
                MenuItem onHoldTabletProject = new MenuItem
                {
                    Header = "Put On Hold"
                };
                MenuItem offHoldTabletProject = new MenuItem
                {
                    Header = "Put Off Hold"
                };
                MenuItem cancelTabletProject = new MenuItem
                {
                    Header = "Cancel",
                    IsEnabled = true
                };
                startTabletProject.Click += StartTabletProject_Click;
                finishTabletProject.Click += FinishTabletProject_Click;
                submitTabletProject.Click += SubmitTabletProject_Click;
                checkTabletProject.Click += CheckTabletProject_Click;
                onHoldTabletProject.Click += OnHoldTabletProject_Click;
                offHoldTabletProject.Click += OffHoldTabletProject_Click;
                completedTabletProject.Click += CompleteTabletProject_Click;
                cancelTabletProject.Click += CancelTabletProject_Click;
                Expander expander = sender as Expander;
                Grid grid = expander.Header as Grid;

                // Check the checkbox for the right-clicked expander
                var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());
                using var _nat02context = new NAT02Context();
                bool _finished = _nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _notStarted = _nat02context.EoiTabletProjectsNotStarted.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _started = _nat02context.EoiTabletProjectsStarted.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _drawn = _nat02context.EoiTabletProjectsDrawn.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _submitted = _nat02context.EoiTabletProjectsSubmitted.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                _nat02context.Dispose();
                if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E4509")
                {
                    if (_finished)
                    {
                        completedTabletProject.IsEnabled = true;
                    }
                    else if (_submitted)
                    {
                        checkTabletProject.IsEnabled = true;
                    }
                    else if (_drawn)
                    {
                        submitTabletProject.IsEnabled = true;
                    }
                    else if (_started)
                    {
                        finishTabletProject.IsEnabled = true;
                    }
                    else if (_notStarted)
                    {
                        startTabletProject.IsEnabled = true;
                    }
                    cancelTabletProject.IsEnabled = true;
                }
                else if (User.Department == "Engineering")
                {
                    if (_submitted)
                    {
                        checkTabletProject.IsEnabled = true;
                    }
                    else if (_drawn)
                    {
                        submitTabletProject.IsEnabled = true;
                    }
                    else if (_started)
                    {
                        finishTabletProject.IsEnabled = true;
                    }
                    else if (_notStarted)
                    {
                        startTabletProject.IsEnabled = true;
                    }
                    cancelTabletProject.IsEnabled = true;
                }
                else if (User.Department == "Customer Service")
                {
                    if (_finished)
                    {
                        completedTabletProject.IsEnabled = true;
                    }
                }
                RightClickMenu.Items.Add(startTabletProject);
                RightClickMenu.Items.Add(finishTabletProject);
                RightClickMenu.Items.Add(checkTabletProject);
                RightClickMenu.Items.Add(submitTabletProject);
                RightClickMenu.Items.Add(completedTabletProject);
                RightClickMenu.Items.Add(onHoldTabletProject);
                if (User.Department == "Engineering") { RightClickMenu.Items.Add(offHoldTabletProject); }
                RightClickMenu.Items.Add(cancelTabletProject);
                expander.ContextMenu = RightClickMenu;
                expander.ContextMenu.Tag = "RightClickMenu";
                expander.ContextMenu.Closed += ContextMenu_Closed;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("AllTabletProjectsDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void AllToolProjectsDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ContextMenu RightClickMenu = new ContextMenu();
                MenuItem completedToolProject = new MenuItem
                {
                    Header = "Completed",
                    IsEnabled = false
                };
                MenuItem startToolProject = new MenuItem
                {
                    Header = "Start",
                    IsEnabled = false
                };
                MenuItem finishToolProject = new MenuItem
                {
                    Header = "Finish",
                    IsEnabled = false
                };
                MenuItem checkToolProject = new MenuItem
                {
                    Header = "Check",
                    IsEnabled = false
                };
                MenuItem onHoldToolProject = new MenuItem
                {
                    Header = "Put On Hold"
                };
                MenuItem offHoldToolProject = new MenuItem
                {
                    Header = "Put Off Hold"
                };
                MenuItem cancelToolProject = new MenuItem
                {
                    Header = "Cancel",
                    IsEnabled = true
                };
                startToolProject.Click += StartToolProject_Click;
                finishToolProject.Click += FinishToolProject_Click;
                checkToolProject.Click += CheckToolProject_Click;
                onHoldToolProject.Click += OnHoldToolProject_Click;
                offHoldToolProject.Click += OffHoldToolProject_Click;
                completedToolProject.Click += CompleteToolProject_Click;
                cancelToolProject.Click += CancelToolProject_Click;
                Expander expander = sender as Expander;
                Grid grid = expander.Header as Grid;

                // Check the checkbox for the right-clicked expander
                var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());
                using var _nat02context = new NAT02Context();
                bool _finished = _nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _notStarted = _nat02context.EoiToolProjectsNotStarted.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _started = _nat02context.EoiToolProjectsStarted.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                bool _drawn = _nat02context.EoiToolProjectsDrawn.Where(p => p.ProjectNumber == _projectNumber && p.RevisionNumber == _revNumber).Any();
                _nat02context.Dispose();
                if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E4509")
                {
                    if (_finished)
                    {
                        completedToolProject.IsEnabled = true;
                    }
                    else if (_drawn)
                    {
                        checkToolProject.IsEnabled = true;
                    }
                    else if (_started)
                    {
                        finishToolProject.IsEnabled = true;
                    }
                    else if (_notStarted)
                    {
                        startToolProject.IsEnabled = true;
                    }
                    cancelToolProject.IsEnabled = true;
                }
                else if (User.EmployeeCode == "E4345") // Phyllis
                {
                    cancelToolProject.IsEnabled = true;
                }
                else if (User.Department == "Engineering")
                {
                    if (_drawn)
                    {
                        checkToolProject.IsEnabled = true;
                    }
                    else if (_started)
                    {
                        finishToolProject.IsEnabled = true;
                    }
                    else if (_notStarted)
                    {
                        startToolProject.IsEnabled = true;
                    }
                    cancelToolProject.IsEnabled = true;
                }
                else if (User.Department == "Customer Service")
                {
                    if (_finished)
                    {
                        completedToolProject.IsEnabled = true;
                    }
                }
                RightClickMenu.Items.Add(startToolProject);
                RightClickMenu.Items.Add(finishToolProject);
                RightClickMenu.Items.Add(checkToolProject);
                RightClickMenu.Items.Add(completedToolProject);
                RightClickMenu.Items.Add(onHoldToolProject);
                if (User.Department == "Engineering") { RightClickMenu.Items.Add(offHoldToolProject); }
                RightClickMenu.Items.Add(cancelToolProject);
                expander.ContextMenu = RightClickMenu;
                expander.ContextMenu.Tag = "RightClickMenu";
                expander.ContextMenu.Closed += ContextMenu_Closed;
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("AllToolProjectsDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void TabletProjectNotStartedDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem startTabletProject = new MenuItem
                    {
                        Header = "Start"
                    };
                    startTabletProject.Click += StartTabletProject_Click;

                    MenuItem onHoldTabletProject = new MenuItem
                    {
                        Header = "Set On Hold"
                    };
                    onHoldTabletProject.Click += OnHoldTabletProject_Click;

                    MenuItem cancelTabletProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelTabletProject.Click += CancelTabletProject_Click;

                    RightClickMenu.Items.Add(startTabletProject);
                    RightClickMenu.Items.Add(onHoldTabletProject);
                    RightClickMenu.Items.Add(cancelTabletProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("TabletProjectNotStartedDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void TabletProjectStartedDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem drawnTabletProject = new MenuItem
                    {
                        Header = "Finish"
                    };
                    drawnTabletProject.Click += FinishTabletProject_Click;

                    MenuItem onHoldTabletProject = new MenuItem
                    {
                        Header = "Set On Hold"
                    };
                    onHoldTabletProject.Click += OnHoldTabletProject_Click;

                    MenuItem cancelTabletProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelTabletProject.Click += CancelTabletProject_Click;

                    RightClickMenu.Items.Add(drawnTabletProject);
                    RightClickMenu.Items.Add(onHoldTabletProject);
                    RightClickMenu.Items.Add(cancelTabletProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("TabletProjectStartedDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void TabletProjectDrawnDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();
                    MenuItem finishTabletProject = new MenuItem
                    {
                        Header = "Submit"
                    };
                    finishTabletProject.Click += SubmitTabletProject_Click;

                    MenuItem onHoldTabletProject = new MenuItem
                    {
                        Header = "Set On Hold"
                    };
                    onHoldTabletProject.Click += OnHoldTabletProject_Click;

                    MenuItem cancelTabletProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelTabletProject.Click += CancelTabletProject_Click;

                    RightClickMenu.Items.Add(finishTabletProject);
                    RightClickMenu.Items.Add(onHoldTabletProject);
                    RightClickMenu.Items.Add(cancelTabletProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("TabletProjectDrawnDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void TabletProjectSubmittedDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem submitTabletProject = new MenuItem
                    {
                        Header = "Check"
                    };
                    submitTabletProject.Click += CheckTabletProject_Click;

                    MenuItem onHoldTabletProject = new MenuItem
                    {
                        Header = "Set On Hold"
                    };
                    onHoldTabletProject.Click += OnHoldTabletProject_Click;

                    MenuItem cancelTabletProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelTabletProject.Click += CancelTabletProject_Click;

                    RightClickMenu.Items.Add(submitTabletProject);
                    RightClickMenu.Items.Add(onHoldTabletProject);
                    RightClickMenu.Items.Add(cancelTabletProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("TabletProjectSubmittedDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void TabletProjectOnHoldDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem offHoldTabletProject = new MenuItem
                    {
                        Header = "Take Off Hold"
                    };
                    offHoldTabletProject.Click += OffHoldTabletProject_Click;

                    MenuItem cancelTabletProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelTabletProject.Click += CancelTabletProject_Click;

                    RightClickMenu.Items.Add(offHoldTabletProject);
                    RightClickMenu.Items.Add(cancelTabletProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("TabletProjectOnHoldDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void ToolProjectNotStartedDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem startToolProject = new MenuItem
                    {
                        Header = "Start"
                    };
                    startToolProject.Click += StartToolProject_Click;

                    MenuItem onHoldToolProject = new MenuItem
                    {
                        Header = "Put On Hold"
                    };
                    onHoldToolProject.Click += OnHoldToolProject_Click;

                    MenuItem cancelToolProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelToolProject.Click += CancelToolProject_Click;

                    RightClickMenu.Items.Add(startToolProject);
                    RightClickMenu.Items.Add(onHoldToolProject);
                    RightClickMenu.Items.Add(cancelToolProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("ToolProjectNotStartedDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void ToolProjectStartedDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {

                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem finishToolProject = new MenuItem
                    {
                        Header = "Finish"
                    };
                    finishToolProject.Click += FinishToolProject_Click;

                    MenuItem onHoldToolProject = new MenuItem
                    {
                        Header = "Put On Hold"
                    };
                    onHoldToolProject.Click += OnHoldToolProject_Click;

                    MenuItem cancelToolProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelToolProject.Click += CancelToolProject_Click;

                    RightClickMenu.Items.Add(finishToolProject);
                    RightClickMenu.Items.Add(onHoldToolProject);
                    RightClickMenu.Items.Add(cancelToolProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("ToolProjectNotStartedDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void ToolProjectDrawnDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem checkToolProject = new MenuItem
                    {
                        Header = "Check"
                    };
                    checkToolProject.Click += CheckToolProject_Click;

                    MenuItem onHoldToolProject = new MenuItem
                    {
                        Header = "Put On Hold"
                    };
                    onHoldToolProject.Click += OnHoldToolProject_Click;

                    MenuItem cancelToolProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelToolProject.Click += CancelToolProject_Click;

                    RightClickMenu.Items.Add(checkToolProject);
                    RightClickMenu.Items.Add(onHoldToolProject);
                    RightClickMenu.Items.Add(cancelToolProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("ToolProjectDrawnDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void ToolProjectOnHoldDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Engineering" || User.GetUserName().Contains("Phyllis"))
                {

                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem offHoldToolProject = new MenuItem
                    {
                        Header = "Off Hold"
                    };
                    offHoldToolProject.Click += OffHoldToolProject_Click;

                    MenuItem cancelToolProject = new MenuItem
                    {
                        Header = "Cancel",
                        IsEnabled = true
                    };
                    cancelToolProject.Click += CancelToolProject_Click;

                    RightClickMenu.Items.Add(offHoldToolProject);
                    RightClickMenu.Items.Add(cancelToolProject);

                    Expander expander = sender as Expander;
                    Grid grid = expander.Header as Grid;

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    _projectNumber = int.Parse(grid.Children[0].GetValue(ContentProperty).ToString());
                    _revNumber = int.Parse(grid.Children[1].GetValue(ContentProperty).ToString());

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("ToolProjectOnHoldDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void QuoteDataGrid_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Customer Service" || User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E4509")
                {
                    Expander expander = sender as Expander;
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem completedQuoteCheck = new MenuItem
                    {
                        Header = "Completed Follow-up"
                    };
                    MenuItem submitQuote = new MenuItem
                    {
                        Header = "Submit Quote"
                    };
                    completedQuoteCheck.Click += CompletedQuoteCheck_Click;
                    submitQuote.Click += SubmitQuote_Click;

                    RightClickMenu.Items.Add(completedQuoteCheck);
                    RightClickMenu.Items.Add(submitQuote);

                    expander.ContextMenu = RightClickMenu;
                    expander.ContextMenu.Tag = "RightClickMenu";
                    expander.ContextMenu.Closed += ContextMenu_Closed;
                    _quoteNumber = double.Parse((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString());
                    _quoteRevNumber = int.Parse((expander.Header as Grid).Children[1].GetValue(ContentProperty).ToString());
                }
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                WriteToErrorLog("QuoteDataGrid_MouseRightButtonUp", ex.Message);
            }
        }

        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            foreach (StackPanel stackPanel in MainGrid.Children.OfType<StackPanel>())
            {
                try
                {
                    DataGrid dataGrid = stackPanel.Children.OfType<DataGrid>().First();
                    if (dataGrid.ContextMenu != null && dataGrid.ContextMenu.Tag.ToString() == "RightClickMenu")
                    {
                        dataGrid.ContextMenu = null;
                    }
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    WriteToErrorLog("ContextMenu_Closed", ex.Message);
                }
            }
        }

        private void Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;
            Expander expander = (((checkBox.Parent as Grid).Parent as Border).TemplatedParent as ToggleButton).TemplatedParent as Expander;
            string header = (((expander.Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
            bool quote = headers.First(h => h.Value == header).Key.Contains("Quote");
            bool project = headers.First(h => h.Value == header).Key.Contains("Project");
            bool order = !headers.First(h => h.Value == header).Key.Contains("Queue") &&
                         !headers.First(h => h.Value == header).Key.Contains("List") &&
                         !headers.First(h => h.Value == header).Key.Contains("Quote") &&
                         !headers.First(h => h.Value == header).Key.Contains("Project");
            string col0val = (((checkBox.Parent as Grid).Children[1] as ContentPresenter).Content as Grid).Children[0].GetValue(ContentProperty).ToString();
            string col1val = (((checkBox.Parent as Grid).Children[1] as ContentPresenter).Content as Grid).Children[1].GetValue(ContentProperty).ToString();
            if (quote)
            {
                selectedQuotes.Add((col0val, col1val, checkBox));
            }
            else if (project)
            {
                selectedProjects.Add((col0val, col1val, checkBox));
            }
            else if (order)
            {
                selectedOrders.Add((col0val, checkBox));
                if (expander.IsExpanded)
                {
                    foreach (Grid grid in (expander.Content as StackPanel).Children)
                    {
                        (grid.Children[0] as CheckBox).IsChecked = true;
                    }
                }
            }
        }

        private void Checkbox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                Expander expander = (((checkBox.Parent as Grid).Parent as Border).TemplatedParent as ToggleButton).TemplatedParent as Expander;
                string header = (((expander.Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                bool quote = headers.First(h => h.Value == header).Key.Contains("Quote");
                bool project = headers.First(h => h.Value == header).Key.Contains("Project");
                bool order = !headers.First(h => h.Value == header).Key.Contains("Queue") &&
                             !headers.First(h => h.Value == header).Key.Contains("List") &&
                             !headers.First(h => h.Value == header).Key.Contains("Quote") &&
                             !headers.First(h => h.Value == header).Key.Contains("Project");
                string col0val = (((checkBox.Parent as Grid).Children[1] as ContentPresenter).Content as Grid).Children[0].GetValue(ContentProperty).ToString();
                string col1val = (((checkBox.Parent as Grid).Children[1] as ContentPresenter).Content as Grid).Children[1].GetValue(ContentProperty).ToString();
                if (quote)
                {
                    selectedQuotes.Remove((col0val, col1val, checkBox));
                }
                else if (project)
                {
                    selectedProjects.Remove((col0val, col1val, checkBox));
                }
                else if (order)
                {
                    selectedOrders.Remove((col0val, checkBox));
                    if (expander.IsExpanded)
                    {
                        foreach (Grid grid in (expander.Content as StackPanel).Children)
                        {
                            (grid.Children[0] as CheckBox).IsChecked = false;
                        }
                    }
                }
            }
            catch
            {

            }
        }
        #endregion

        #region ErrorHandling
        private void WriteToErrorLog(string errorLoc, string errorMessage)
        {
            try
            {
                string path = @"\\engserver\workstations\NatoliOrderInterfaceErrorLog\Error_Log.txt";
                System.IO.StreamReader sr = new System.IO.StreamReader(path);
                string existing = sr.ReadToEnd();
                existing = existing.TrimEnd();
                sr.Close();
                System.IO.StreamWriter sw = new System.IO.StreamWriter(path, false);
                sw.Write(DateTime.Now + "  " + User.GetUserName() + "  " + errorLoc + "\r\n" + errorMessage.PadLeft(20) + "\r\n" + existing);
                sw.Flush();
                sw.Close();
            }
            catch
            {
                MessageBox.Show("Error in the Error Handling of Errors");
            }
        }
        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    mainTimer.Dispose();
                    quoteTimer.Dispose();
                    oqTimer.Dispose();
                    _nat01context.Dispose();
                    NatoliOrderListTimer.Dispose();
                    // TODO: dispose managed state (managed objects).
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProjectWindow()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}