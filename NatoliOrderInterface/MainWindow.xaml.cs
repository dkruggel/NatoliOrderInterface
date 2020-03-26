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
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using Colors = System.Windows.Media.Colors;
using Windows.Management.Deployment;
using Windows.ApplicationModel;
using WpfAnimatedGif;
using NatoliOrderInterface.FolderIntegrity;
using F23.StringSimilarity;
using NatoliOrderInterface;

namespace NatoliOrderInterface
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]

    public partial class MainWindow : Window, IDisposable , IMethods
    {
        #region Declarations
        public string connectionString;
        private bool _panelLoading;
        private string _panelMainMessage = "Main Loading Message";
        private string _panelSubMessage = "Sub Loading Message";
        public bool isDebugMode = false;


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
        private readonly System.Timers.Timer foldersTimer = new System.Timers.Timer();
        private int _projectNumber = 0;
        private int? _revNumber = 0;
        private double _quoteNumber = 0;
        private int? _quoteRevNumber = 0;
        private bool quotesCompletedChanged = false;
        private int quotesCompletedCount = 0;
        private double _orderNumber = 0;
        private bool _filterProjects = false;
        private string rClickModule;
        private List<(string, CheckBox, string)> selectedOrders = new List<(string, CheckBox, string)>();
        private List<string> selectedLineItems = new List<string>();
        private List<(string, string, CheckBox, string)> selectedProjects = new List<(string, string, CheckBox, string)>();
        private List<(string, string, CheckBox, string)> selectedQuotes = new List<(string, string, CheckBox, string)>();

        NAT01Context _nat01context = new NAT01Context();
        public string ChildWindow { get; set; }
        public event EventHandler RemovedFromSelectedOrders;
        protected virtual void OnRemovedFromSelectedOrders(EventArgs e)
        {
            EventHandler handler = RemovedFromSelectedOrders;
            handler?.Invoke(this, e);
        }
        public delegate void SomeBoolChangedEvent(string module = "");
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

        //public static readonly DependencyProperty notificationDotProperty =
        //    DependencyProperty.Register("NotificationDot", typeof(double), typeof(MainWindow), new UIPropertyMetadata(string.Empty));
        //private double NotificationDot
        //{
        //    get { return (double)GetValue(notificationDotProperty); }
        //    set { SetValue(notificationDotProperty, value); }
        //}
        private double NotificationDot = 0.0;
        private double notificationNumber1 = 0.0;
        private double notificationNumber2 = 0.0;
        private double notificationNumber3 = 0.0;
        private double notificationNumber4 = 0.0;
        private double notificationNumber5 = 0.0;
        private double notificationNumber6 = 0.0;
        private double notificationNumber7 = 0.0;
        private double notificationNumber8 = 0.0;
        private double notificationNumber9 = 0.0;
        private double notificationNumber10 = 0.0;
        private double notificationNumber11 = 0.0;


        #region View Dictionaries
        //Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, string repId, string background, string foreground, string fontWeight)> quotesNotConvertedDict;
        //Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> quotesToConvertDict;
        //Dictionary<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)> ordersBeingEnteredDict;
        //Dictionary<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)> ordersInTheOfficeDict;
        //Dictionary<double, (string customerName, int daysToShip, int daysIn, string background, string foreground, string fontWeight)> ordersEnteredUnscannedDict;
        //Dictionary<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)> ordersInEngineeringUnprintedDict;
        //Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> ordersReadyToPrintDict;
        //Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> ordersPrintedInEngineeringDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsNotStartedDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsStartedDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsDrawnDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsSubmittedDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> tabletProjectsOnHoldDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> allTabletProjectsDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsNotStartedDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsStartedDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsDrawnDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> toolProjectsOnHoldDict;
        //Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> allToolProjectsDict;
        //Dictionary<string, (string releasedBy, string tag, string releaseTime, int priority)> driveWorksQueueDict;
        //Dictionary<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string repId, string background)> natoliOrderListDict;
        List<object> dictList;
        #endregion

        #region New Style ListBox
        #region Orders Being Entered
        private ListBox OrdersBeingEnteredListBox { get; set; }
        private List<EoiOrdersBeingEnteredView> ordersBeingEntered = new List<EoiOrdersBeingEnteredView>();
        private List<EoiOrdersBeingEnteredView> _ordersBeingEntered = new List<EoiOrdersBeingEnteredView>();
        public List<EoiOrdersBeingEnteredView> OrdersBeingEntered
        {
            get
            {
                return ordersBeingEntered;
            }
            set
            {
                if (value.Except(ordersBeingEntered).Count() > 0 || ordersBeingEntered.Except(value).Count() > 0)
                {
                    ordersBeingEntered = value;
                    OrdersBeingEnteredListBox.ItemsSource = null;
                    OrdersBeingEnteredListBox.ItemsSource = ordersBeingEntered;
                }
            }
        }
        #endregion
        #region Orders In The Office
        private ListBox OrdersInTheOfficeListBox = new ListBox();
        private List<EoiOrdersInOfficeView> ordersInTheOffice = new List<EoiOrdersInOfficeView>();
        private List<EoiOrdersInOfficeView> _ordersInTheOffice = new List<EoiOrdersInOfficeView>();
        public List<EoiOrdersInOfficeView> OrdersInTheOffice
        {
            get
            {
                return ordersInTheOffice;
            }
            set
            {
                if (value.Except(ordersInTheOffice).Count() > 0 || ordersInTheOffice.Except(value).Count() > 0)
                {
                    ordersInTheOffice = value;
                    OrdersInTheOfficeListBox.ItemsSource = null;
                    OrdersInTheOfficeListBox.ItemsSource = ordersInTheOffice;
                }
            }
        }
        #endregion
        #region Orders Entered Unscanned
        private ListBox OrdersEnteredListBox = new ListBox();
        private List<EoiOrdersEnteredAndUnscannedView> ordersEntered = new List<EoiOrdersEnteredAndUnscannedView>();
        private List<EoiOrdersEnteredAndUnscannedView> _ordersEntered = new List<EoiOrdersEnteredAndUnscannedView>();
        public List<EoiOrdersEnteredAndUnscannedView> OrdersEntered
        {
            get
            {
                return ordersEntered;
            }
            set
            {
                if (value.Except(ordersEntered).Count() > 0 || ordersEntered.Except(value).Count() > 0)
                {
                    ordersEntered = value;
                    OrdersEnteredListBox.ItemsSource = null;
                    OrdersEnteredListBox.ItemsSource = ordersEntered;
                }
            }
        }
        #endregion
        #region Orders In Engineering
        private ListBox OrdersInEngListBox = new ListBox();
        private List<EoiOrdersInEngineeringUnprintedView> ordersInEng = new List<EoiOrdersInEngineeringUnprintedView>();
        private List<EoiOrdersInEngineeringUnprintedView> _ordersInEng = new List<EoiOrdersInEngineeringUnprintedView>();
        public List<EoiOrdersInEngineeringUnprintedView> OrdersInEng
        {
            get
            {
                return ordersInEng;
            }
            set
            {
                if (value.Except(ordersInEng).Count() > 0 || ordersInEng.Except(value).Count() > 0)
                {
                    ordersInEng = value;
                    OrdersInEngListBox.ItemsSource = null;
                    OrdersInEngListBox.ItemsSource = ordersInEng;
                }
            }
        }
        #endregion
        #region Orders Ready To Print
        private ListBox OrdersReadyToPrintListBox = new ListBox();
        private List<EoiOrdersReadyToPrintView> ordersReadyToPrint = new List<EoiOrdersReadyToPrintView>();
        private List<EoiOrdersReadyToPrintView> _ordersReadyToPrint = new List<EoiOrdersReadyToPrintView>();
        public List<EoiOrdersReadyToPrintView> OrdersReadyToPrint
        {
            get
            {
                return ordersReadyToPrint;
            }
            set
            {
                if (value.Except(ordersReadyToPrint).Count() > 0 || ordersReadyToPrint.Except(value).Count() > 0)
                {
                    ordersReadyToPrint = value;
                    OrdersReadyToPrintListBox.ItemsSource = null;
                    OrdersReadyToPrintListBox.ItemsSource = ordersReadyToPrint;
                }
            }
        }
        #endregion
        #region Orders Printed In Engineering
        private ListBox OrdersPrintedListBox = new ListBox();
        private List<EoiOrdersPrintedInEngineeringView> ordersPrinted = new List<EoiOrdersPrintedInEngineeringView>();
        private List<EoiOrdersPrintedInEngineeringView> _ordersPrinted = new List<EoiOrdersPrintedInEngineeringView>();
        public List<EoiOrdersPrintedInEngineeringView> OrdersPrinted
        {
            get
            {
                return ordersPrinted;
            }
            set
            {
                if (value.Except(ordersPrinted).Count() > 0 || ordersPrinted.Except(value).Count() > 0)
                {
                    ordersPrinted = value;
                    OrdersPrintedListBox.ItemsSource = null;
                    OrdersPrintedListBox.ItemsSource = ordersPrinted;
                }
            }
        }
        #endregion
        #region Quotes Not Converted
        private ListBox QuotesNotConvertedListBox = new ListBox();
        private List<EoiQuotesNotConvertedView> quotesNotConverted = new List<EoiQuotesNotConvertedView>();
        private List<EoiQuotesNotConvertedView> _quotesNotConverted = new List<EoiQuotesNotConvertedView>();
        public List<EoiQuotesNotConvertedView> QuotesNotConverted
        {
            get
            {
                return quotesNotConverted;
            }
            set
            {
                if (value.Except(quotesNotConverted).Count() > 0 || quotesNotConverted.Except(value).Count() > 0)
                {
                    quotesNotConverted = value;
                    QuotesNotConvertedListBox.ItemsSource = null;
                    QuotesNotConvertedListBox.ItemsSource = quotesNotConverted;
                }
            }
        }
        #endregion
        #region Quotes To Convert
        private ListBox QuotesToConvertListBox = new ListBox();
        private List<EoiQuotesMarkedForConversionView> quotesToConvert = new List<EoiQuotesMarkedForConversionView>();
        private List<EoiQuotesMarkedForConversionView> _quotesToConvert = new List<EoiQuotesMarkedForConversionView>();
        public List<EoiQuotesMarkedForConversionView> QuotesToConvert
        {
            get
            {
                return quotesToConvert;
            }
            set
            {
                if (value.Except(quotesToConvert).Count() > 0 || quotesToConvert.Except(value).Count() > 0)
                {
                    quotesToConvert = value;
                    QuotesToConvertListBox.ItemsSource = null;
                    QuotesToConvertListBox.ItemsSource = quotesToConvert;
                }
            }
        }
        #endregion
        #region All Tablet Projects
        private ListBox AllTabletProjectsListBox = new ListBox();
        private List<EoiAllTabletProjectsView> allTabletProjects = new List<EoiAllTabletProjectsView>();
        private List<EoiAllTabletProjectsView> _allTabletProjects = new List<EoiAllTabletProjectsView>();
        public List<EoiAllTabletProjectsView> AllTabletProjects
        {
            get
            {
                return allTabletProjects;
            }
            set
            {
                if (value.Except(allTabletProjects).Count() > 0 || allTabletProjects.Except(value).Count() > 0)
                {
                    allTabletProjects = value;
                    AllTabletProjectsListBox.ItemsSource = null;
                    AllTabletProjectsListBox.ItemsSource = allTabletProjects;
                }
            }
        }
        #endregion
        #region All Tool Projects
        private ListBox AllToolProjectsListBox = new ListBox();
        private List<EoiAllToolProjectsView> allToolProjects = new List<EoiAllToolProjectsView>();
        private List<EoiAllToolProjectsView> _allToolProjects = new List<EoiAllToolProjectsView>();
        public List<EoiAllToolProjectsView> AllToolProjects
        {
            get
            {
                return allToolProjects;
            }
            set
            {
                if (value.Except(allToolProjects).Count() > 0 || allToolProjects.Except(value).Count() > 0)
                {
                    allToolProjects = value;
                    AllToolProjectsListBox.ItemsSource = null;
                    AllToolProjectsListBox.ItemsSource = allToolProjects;
                }
            }
        }
        #endregion
        #region DriveWorks Queue
        private ListBox DriveWorksQueueListBox = new ListBox();
        private List<QueueView> driveWorksQueue = new List<QueueView>();
        private List<QueueView> _driveWorksQueue = new List<QueueView>();
        public List<QueueView> DriveWorksQueue
        {
            get
            {
                return driveWorksQueue;
            }
            set
            {
                if (value.Except(driveWorksQueue).Count() > 0 || driveWorksQueue.Except(value).Count() > 0)
                {
                    driveWorksQueue = value;
                    DriveWorksQueueListBox.ItemsSource = null;
                    DriveWorksQueueListBox.ItemsSource = driveWorksQueue;
                }
            }
        }
        #endregion
        #region Natoli Order List
        private ListBox NatoliOrderListListBox = new ListBox();
        private List<NatoliOrderListFinal> natoliOrderList = new List<NatoliOrderListFinal>();
        private List<NatoliOrderListFinal> _natoliOrderList = new List<NatoliOrderListFinal>();
        public List<NatoliOrderListFinal> NatoliOrderList
        {
            get
            {
                return natoliOrderList;
            }
            set
            {
                if (value.Except(natoliOrderList).Count() > 0 || natoliOrderList.Except(value).Count() > 0)
                {
                    natoliOrderList = value;
                    NatoliOrderListListBox.ItemsSource = null;
                    NatoliOrderListListBox.ItemsSource = natoliOrderList;
                }
            }
        }
        #endregion
        #endregion

        Dictionary<string, string> oeDetailTypes = new Dictionary<string, string>() { { "U", "Upper" }, { "L", "Lower" }, { "D", "Die" }, { "DS", "Die" }, { "R", "Reject" }, { "A", "Alignment" } };
        #endregion

        public MainWindow()
        {
            try
            {
#if DEBUG
                isDebugMode = true;
#endif
                ShowSplashScreen("Natoli_Logo_Color.png");
                IfAppIsRunningSwitchToItAndShutdown();
                InitializeComponent();
                App.GetConnectionString();
                App.GetEmailSettings();
                ((App)Application.Current as App).InitializeTimers();
                UpdatedFromChild = MainRefresh;
                SetUser();
                Width = (double)User.Width;
                Height = (double)User.Height;
                Top = (double)User.Top;
                Left = (double)User.Left;
                if (isDebugMode)
                {
                    Title = "Natoli Order Interface *DEBUG*";
                }
                else
                {
                    Title = "Natoli Order Interface " + "v" + User.PackageVersion;
                }
                _filterProjects = User.FilterActiveProjects;
                if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { GetPercentages(); }
                RemoveUserFromOrdersBeingCheckedBy(User);
                this.Show();
                if (User.Maximized == true)
                {
                    Dispatcher.Invoke(new Action(() => this.WindowState = WindowState.Maximized));
                }
                originalProps = new List<string>();
                dictList = new List<object>();
                foreach (string s in User.VisiblePanels)
                {
                    originalProps.Add(s);
                }
                // ConstructModules();
                BuildPanels();
                BuildMenus();
                //MainMenu.Background = SystemParameters.WindowGlassBrush; // Sets it to be the same color as the accent color in Windows
                InitializingMenuItem.Visibility = Visibility.Collapsed;
                InitializeTimers(User);

                if (isDebugMode)
                {
                    if (User.EmployeeCode == "E4754")
                    {
                        //CustomerNoteWindow customerNoteWindow = new CustomerNoteWindow(User, "2000002");
                        //customerNoteWindow.Show();
                        //IMethods.SendProjectCompletedEmailToCSRAsync(new List<string> { "Miral","Tyler" }, "102881", "0", new User("mbouzitoun"));
                        //ProjectWindow projectWindow = new ProjectWindow("110012", "4", this, User, false);
                    }
                    else if (User.EmployeeCode == "E4408")
                    {
                        //NotificationManagementWindow notificationManagementWindow = new NotificationManagementWindow(User, this);
                        //notificationManagementWindow.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("MainWindow Entry", ex.Message, User);
            }
        }
        public void MainRefresh(string module = "")
        {
            //BindData("Main");
            //BindData("QuotesNotConverted");
            //BindData("NatoliOrderList");

            //UpdateUI();

            if (string.IsNullOrEmpty(module))
            {
                Task.Run(() =>
                {
                    BindData("Main");
                    BindData("QuotesNotConverted");
                    BindData("NatoliOrderList");
                }).ContinueWith(t => Dispatcher.Invoke(() => UpdateUI()));
            }
            else
            {
                switch (module)
                {
                    case "BeingEntered":
                        GetBeingEntered();
                        Dispatcher.Invoke(() => BindBeingEntered());
                        break;
                    case "InTheOffice":
                        GetInTheOffice();
                        Dispatcher.Invoke(() => BindInTheOffice());
                        break;
                    case "QuotesNotConverted":
                        GetQuotesNotConverted();
                        Dispatcher.Invoke(() => BindQuotesNotConverted());
                        break;
                    case "EnteredUnscanned":
                        GetEnteredUnscanned();
                        Dispatcher.Invoke(() => BindEnteredUnscanned());
                        break;
                    case "InEngineering":
                        GetInEngineering();
                        Dispatcher.Invoke(() => BindInEngineering());
                        break;
                    case "QuotesToConvert":
                        GetQuotesToConvert();
                        Dispatcher.Invoke(() => BindQuotesToConvert());
                        break;
                    case "ReadyToPrint":
                        GetReadyToPrint();
                        Dispatcher.Invoke(() => BindReadyToPrint());
                        break;
                    case "PrintedInEngineering":
                        GetPrintedInEngineering();
                        Dispatcher.Invoke(() => BindPrintedInEngineering());
                        break;
                    case "AllTabletProjects":
                        GetAllTabletProjects();
                        Dispatcher.Invoke(() => BindAllTabletProjects());
                        break;
                    case "AllToolProjects":
                        GetAllToolProjects();
                        Dispatcher.Invoke(() => BindAllToolProjects());
                        break;
                    case "DriveWorksQueue":
                        GetDriveWorksQueue();
                        Dispatcher.Invoke(() => BindDriveWorksQueue());
                        break;
                    case "NatoliOrderList":
                        GetNatoliOrderList();
                        Dispatcher.Invoke(() => BindNatoliOrderList());
                        break;
                    default:
                        break;
                }
            }

            ResetTimers(new List<Timer> { mainTimer, quoteTimer, NatoliOrderListTimer });
        }
        /// <summary>
        /// Resets timers from a list in the order they are provided.
        /// </summary>
        /// <param name="timers"></param>
        private void ResetTimers(List<Timer> timers)
        {
            foreach (Timer timer in timers)
            {
                timer.Stop();
                timer.Start();
            }
            
        }
        /// <summary>
        /// Shows startup splash image
        /// </summary>
        /// <param name="image"></param>
        private void ShowSplashScreen(string image)
        {
            SplashScreen splashScreen = new SplashScreen(image);
            splashScreen.Show(true);
        }
        /// <summary>
        /// Checks for an app of this name running. If it is running, this app shuts down.
        /// </summary>
        private void IfAppIsRunningSwitchToItAndShutdown()
        {
            var currentlyRunningProcesses = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Where(p => p.Id != Process.GetCurrentProcess().Id);
            if (currentlyRunningProcesses.Any())
            {
                var process = currentlyRunningProcesses.First();
                var id = process.Id;
                IMethods.BringProcessToFront(currentlyRunningProcesses.First());
                Application.Current.Shutdown();
            }
        }
        /// <summary>
        /// Binds a user to User.
        /// </summary>
        private void SetUser()
        {
            try
            {
                // User = new User("jwillis");
                User = new User(Environment.UserName);
                App.user = User;
                // User = new User("jwillis");
                // User = new User("mbouzitoun");
                // User = new User("billt");
            }
            catch (Exception ex)
            {
                User = new User("");
            }
        }
        /// <summary>
        /// Removes all instances of this user in EOI_OrdersBeingCheckedBy
        /// </summary>
        /// <param name="user"></param>
        private void RemoveUserFromOrdersBeingCheckedBy(User user)
        {
            using var _nat02Context = new NAT02Context();
            _nat02Context.EoiOrdersBeingChecked.RemoveRange(_nat02Context.EoiOrdersBeingChecked.Where(o => o.User == user.GetUserName()));
            _nat02Context.SaveChanges();
            _nat02Context.Dispose();
        }
        /// <summary>
        /// Starts all the times with their desired interval
        /// </summary>
        /// <param name="user"></param>
        private void InitializeTimers(User user)
        {
            mainTimer.Elapsed += MainTimer_Elapsed;
            mainTimer.Interval = user.Department == "Engineering" ? 0.5 * (60 * 1000) : 5 * (60 * 1000); // 0.5 or 5 minutes
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
            if (user.EmployeeCode == "E4408" || user.EmployeeCode == "E4754")
            {
                foldersTimer.Elapsed += FoldersTimer_Elapsed;
                foldersTimer.Interval = 2 * (60 * 60 * 1000); // 2 hours
                foldersTimer.Enabled = true;
            }
        }
        
        #region Main Window Events
        private void GridWindow_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            //TestWindow testWindow = new TestWindow();
            //testWindow.Show();
#endif
        }
        private void GridWindow_ContentRendered(object sender, EventArgs e)
        {
            // ConstructExpanders();
            BindData("Main");
            BindData("QuotesNotConverted");
            BindData("NatoliOrderList");
            UpdateUI();

            SetNotificationPicture();
        }
        private void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BindData("Main");
            UpdateUI();
        }
        private void QuoteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BindData("QuotesNotConverted");
            UpdateUI();

            SetNotificationPicture();
        }
        private async void FoldersTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                (List<string> errantFolders, List<Tuple<string, string>> renamedFolders) = FolderCheck.CustomerFolderCheck();
                Task.Factory.StartNew(() => IMethods.WriteToErrantFoldersLog(errantFolders, User));
                Task.Factory.StartNew(() => IMethods.WriteToFoldersRenamedLog(renamedFolders, User));
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("FoldersTimer_Elapsed()", ex.Message, User);
            }
        }
        private void NatoliOrderListTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            BindData("NatoliOrderList");
            UpdateUI();
        }
        private void OQTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { GetPercentages(); }
            if (User.Department == "Engineering")
            {
                QuotesAndOrders();
            }
        }
        private void GridWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using var context = new NAT02Context();
            try
            {
                EoiSettings eoiSettings = context.EoiSettings.Single(s => s.EmployeeId == User.EmployeeCode);
                eoiSettings.Maximized = WindowState == WindowState.Maximized;
                eoiSettings.Width = (short?)Width;
                eoiSettings.Height = (short?)Height;
                eoiSettings.Top = (short?)Top;
                eoiSettings.Left = (short?)Left;
                eoiSettings.FilterActiveProjects = _filterProjects;
                context.EoiSettings.Update(eoiSettings);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                IMethods.WriteToErrorLog("GridWindow_Closing - Save Settings", ex.Message, User);
            }
            context.Dispose();
            try
            {
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
                        IMethods.WriteToErrorLog("GridWindow_Closing - Remove from OrdersBeingChecked", ex.Message, User);
                    }
                    nat02context.Dispose();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("GridWindow_Closing - Remove from Checking", ex.Message, User);
            }
            Dispose();
            System.Environment.Exit(0);
        }
        private void GridWindow_Activated(object sender, EventArgs e)
        {
            if (Height < 200) { Height = 250; }
            if (Width < 150) { Width = 200; }
            if (mainTimer.Enabled == false) { mainTimer.Start(); }
            if (quoteTimer.Enabled == false) { quoteTimer.Start(); }
            if (NatoliOrderListTimer.Enabled == false) { NatoliOrderListTimer.Start(); }
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
                if (WindowState != WindowState.Maximized)
                {
                    MainRefresh();
                }
                mainTimer.Start();
                quoteTimer.Start();
                NatoliOrderListTimer.Start();
            }
        }
        private void GridWindow_LayoutUpdated(object sender, EventArgs e)
        {
            try
            {
                //foreach (string VisiblePanel in User.VisiblePanels)
                //{
                //    int i = User.VisiblePanels.IndexOf(VisiblePanel);
                //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
                //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
                //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

                //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
                //    {
                //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Last().Width != new GridLength(22))
                //        {
                //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
                //        }
                //    }
                //    else
                //    {
                //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Last().Width == new GridLength(22))
                //        {
                //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
                //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
                //        }
                //    }
                //}
            }
            catch
            {

            }
        }
        private void GridWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                double currPos = ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).HorizontalOffset;
                ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).ScrollToHorizontalOffset(currPos - 755);
            }
            else if (e.Key == Key.Right)
            {

                double currPos = ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).HorizontalOffset;
                ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).ScrollToHorizontalOffset(currPos + 755);
            }
        }
        #endregion

        #region MenuStuff
        public void BuildMenus()
        {
            //MainMenu.Items.Clear();
            MainMenu.Items.RemoveAt(0);
            #region FileMenuRegion
            MenuItem fileMenu = new MenuItem
            {
                Header = "File",
                Height = MainMenu.Height
            };

            MenuItem updateApp = new MenuItem
            {
                Header = "Update App",
                ToolTip = "Updates the app to the most current version (if available)."
            };
            updateApp.Click += UpdateApp_Click;


            MenuItem customerNote = new MenuItem
            {
                Header = "Customer Note",
                ToolTip = "Opens a New Customer Note"
            };
            customerNote.Click += CustomerNote_Click;
            if (User.ViewReports) { fileMenu.Items.Add(customerNote); }

            MenuItem createProject = new MenuItem()
            {
                Header = "Create Project",
                ToolTip = "Creates a new Tablet or Tool Project. It will become active on form submission."
            };
            createProject.Click += CreateProject_Click;
            if (User.EmployeeCode == "E4754") { fileMenu.Items.Add(createProject); }
            // if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.Department == "Customer Service") { fileMenu.Items.Add(createProject); }

            MenuItem projectSearch = new MenuItem()
            {
                Header = "Project Search",
                ToolTip = "Search for old engineering projects."
            };
            projectSearch.Click += ProjectSearch_Click;
            fileMenu.Items.Add(projectSearch);

            //MenuItem forceRefresh = new MenuItem
            //{
            //    Header = "Force Refresh",
            //    ToolTip = "Bypass the refresh timer."
            //};
            //forceRefresh.Click += ForceRefresh_Click;
            //fileMenu.Items.Add(forceRefresh);

            //MenuItem editLayout = new MenuItem
            //{
            //    Header = "Edit Layout",
            //    ToolTip = "Change which views are shown in the main window."
            //};
            //editLayout.Click += EditLayout_Click;
            //fileMenu.Items.Add(editLayout);

            MenuItem reports = new MenuItem
            {
                Header = "Reports",
                ToolTip = "Opens reporting window."
            };
            reports.Click += Reports_Click;
            if (User.ViewReports) { fileMenu.Items.Add(reports); }

            MenuItem checkMissingVariables = new MenuItem
            {
                Header = "Missing Automation Info",
                ToolTip = "Checks for orders missing automation information."
            };
            checkMissingVariables.Click += CheckMissingVariables_Click;
            if (User.Department == "Engineering") { fileMenu.Items.Add(checkMissingVariables); }

            MenuItem filterProjects = new MenuItem
            {
                Header = "Filter Active Projects",
                IsChecked = User.FilterActiveProjects,
                ToolTip = "Filters All Tablet Projects and All Tool Projects to just active projects (in engineering)."
            };
            filterProjects.Click += FilterProjects_Click;
            //if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { fileMenu.Items.Add(filterProjects); }
            fileMenu.Items.Add(filterProjects);

            MenuItem printDrawings = new MenuItem
            {
                Header = "Print Drawings",
                ToolTip = "Prints pdfs from your Desktop\\WorkOrdersToPrint."
            };
            printDrawings.Click += PrintDrawings_Click;
            if (User.Department == "Engineering" && !User.GetUserName().StartsWith("Phyllis")) { fileMenu.Items.Add(printDrawings); }
            //if (User.EmployeeCode == "E4408") { fileMenu.Items.Add(updateApp); }
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
            if (User.Department != "Hob Programming") { MainMenu.Items.Add(subsMenu); }
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
                if (User.Department != "Hob Programming")
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
            }

            #endregion
            #region RightClickRegion
            MenuItem startOrder = new MenuItem
            {
                Header = "Start"
            };
            #endregion
        }
        private void CustomerNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CustomerNoteWindow customerNoteWindow = new CustomerNoteWindow(User);
                customerNoteWindow.Show();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("MainWindow.xaml.cs => CustomerNote_Click()", ex.Message, User);
            }
        }
        private void NotificationsMenu_Click(object sender, RoutedEventArgs e)
        {
            NotificationManagementWindow notificationManagementWindow = new NotificationManagementWindow(User, this);
            notificationManagementWindow.Show();
        }
        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            ReportingWindow reportingWindow = new ReportingWindow(this);
            reportingWindow.Show();
        }
        private void UpdateApp_Click(object sender, RoutedEventArgs e)
        {
            // CheckForAvailableUpdatesAndLaunchAsync()
            // Process.Start(@"\\nshare\VB_Apps\NatoliOrderInterface\NatoliOrderInterface.Package.appinstaller");
            // System.IO.File.Open(@"\\nshare\VB_Apps\NatoliOrderInterface\NatoliOrderInterface.Package.appinstaller", System.IO.FileMode.Open);
        }
        public void CheckForAvailableUpdatesAndLaunch(User user)
        {
            //try
            //{                
            //    string currentVersion = user.PackageVersion;
            //    using var _nat02Context = new NAT02Context();
            //    string minimumVersion = _nat02Context.EoiSettings.First(s => s.EmployeeId == "EPACKG").PackageVersion;
            //    _nat02Context.Dispose();
            //    string[] currentVersionNumbers = currentVersion.Split('.');
            //    string[] minimumVersionNumbers = minimumVersion.Split('.');
            //    for (int i = 0; i < 4; i++)
            //    {
            //        if (Convert.ToInt32(minimumVersionNumbers[i]) > Convert.ToInt32(currentVersionNumbers[i]))
            //        {
            //            Process _process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"\\nshare\VB_Apps\NatoliOrderInterface\NatoliOrderInterface.Package_" + minimumVersionNumbers[0] + "." + minimumVersionNumbers[1] + "." + minimumVersionNumbers[2] + "." + minimumVersionNumbers[3] + "_" + "Test\\" + "NatoliOrderInterface.Package_" + minimumVersionNumbers[0] + "." + minimumVersionNumbers[1] + "." + minimumVersionNumbers[2] + "." + minimumVersionNumbers[3] + "_" + "x64.appxbundle");
            //            break;
            //        }
            //    }
                
            //}
            //catch (Exception ex)
            //{
            //    IMethods.WriteToErrorLog("CheckForAvailableUpdatesAndLaunchAsync", ex.Message, User);
            //}
        }
        public void SetNotificationPicture()
        {
            Dispatcher.Invoke(() =>
            {
                //    // Check for new notifications
                //    using var _nat02context = new NAT02Context();
                //    try
                //    {
                //        int active = _nat02context.EoiNotificationsActive.Where(n => n.User == User.DomainName).Count();
                //        if (active > 0)
                //        {
                //            //MenuItem notificationMenu = MainMenu.Items.GetItemAt(3) as MenuItem;
                //            var bell = App.Current.Resources["bell_alt_ringDrawingImage"] as DrawingImage;
                //            var image = ((notificationMenu.Template.FindName("Border", notificationMenu) as Border).Child as Grid).Children.OfType<Image>().First();
                //            image.Source = bell as ImageSource;
                //        }
                //        else
                //        {
                //            //MenuItem notificationMenu = MainMenu.Items.GetItemAt(3) as MenuItem;
                //            var bell = App.Current.Resources["bellDrawingImage"] as DrawingImage;
                //            var image = ((notificationMenu.Template.FindName("Border", notificationMenu) as Border).Child as Grid).Children.OfType<Image>().First();
                //            image.Source = bell as ImageSource;
                //        }
                //    }
                //    catch (Exception ex)
                //    {

                //    }
                //    _nat02context.Dispose();
            }
            );
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
        public void DeleteMachineVariables(string orderNo, int lineNumber = 0)
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
        private void AddModule_Click(object sender, RoutedEventArgs e)
        {
            // Open Add Module Window
            Window addModuleWindow = new Window()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Width = 250,
                Height = 350
            };

            DockPanel dockPanel = new DockPanel();

            ListBox listBox = new ListBox();
            DockPanel.SetDock(listBox, Dock.Top);
            listBox.SelectionChanged += ListBox_SelectionChanged;
            listBox.PreviewMouseDoubleClick += AddNewModule_Click;
            using var _ = new NAT02Context();
            List<string> visiblePanels = _.EoiSettings.Single(s => s.DomainName == User.DomainName).Panels.Split(',').ToList();
            _.Dispose();
            List<string> possiblePanels = IMethods.GetPossiblePanels(User);
            listBox.ItemsSource = possiblePanels.Except(visiblePanels);

            dockPanel.Children.Add(listBox);
            addModuleWindow.Content = dockPanel;
            addModuleWindow.Show();
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DockPanel dockPanel = (sender as ListBox).Parent as DockPanel;
            Window w = dockPanel.Parent as Window;

            if (dockPanel.Children.OfType<Button>().Any())
                dockPanel.Children.Remove(dockPanel.Children.OfType<Button>().First());

            Button button = new Button()
            {
                Content = "Add Module"
            };
            DockPanel.SetDock(button, Dock.Top);
            dockPanel.Children.Add(button);
            button.Click += AddNewModule_Click;
        }
        private void AddNewModule_Click(object sender, RoutedEventArgs e)
        {
            string name = sender is Button ? ((sender as Button).Parent as DockPanel).Children.OfType<ListBox>().First().SelectedItem.ToString() : (sender as ListBox).SelectedItem.ToString();
            Window.GetWindow(sender as DependencyObject).Close();
            AddModule(name, 0);
            SaveSettings();
        }
        private void SaveSettings()
        {
            NAT02Context _nat02context = new NAT02Context();

            string newPanels = "";
            List<string> visiblePanels = new List<string>();
            foreach (Grid grid in MainWrapPanel.Children)
            {
                visiblePanels.Add((VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First(), 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First().Name[0..^7]);
            }
            newPanels = String.Join(',', visiblePanels.ToArray());

            EoiSettings eoiSettings = _nat02context.EoiSettings.Single(s => s.EmployeeId == User.EmployeeCode);
            eoiSettings.Panels = newPanels;
            _nat02context.EoiSettings.Update(eoiSettings);
            _nat02context.SaveChanges();
            _nat02context.Dispose();

            User.VisiblePanels = visiblePanels;
        }
        #region Clicks
        private void StartTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllTabletProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTablet))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

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
                            IMethods.StartProject(project.Item1, project.Item2, "TABLETS", User);
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
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        IMethods.WriteToErrorLog("StartTabletProject_CLick", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                MainRefresh();
            }
        }
        private void FinishTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllTabletProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletDrawnBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTablet))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            IMethods.DrawProject(project.Item1, project.Item2, "TABLETS", User);
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
                        IMethods.WriteToErrorLog("FinishTabletProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                MainRefresh();
            }
        }
        private void SubmitTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllTabletProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletSubmittedBy) || 
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletDrawnBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTablet))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            IMethods.SubmitProject(project.Item1, project.Item2, "TABLETS", User);
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
                        IMethods.WriteToErrorLog("SubmitTabletProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

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

                OnHoldCommentWindow onHoldCommentWindow = new OnHoldCommentWindow("Tablets", _projectNumber, _revNumber, this, User)
                {
                    Left = Left,
                    Top = Top
                };
                onHoldCommentWindow.Show();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("OnHoldTabletProject_Click", ex.Message, User);
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
                    IMethods.TakeProjectOffHold(_projectNumber.ToString(), _revNumber.ToString());
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
                IMethods.WriteToErrorLog("OffHoldTabletProject_Click", ex.Message, User);
            }
        }
        private void CompleteTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                using var _nat02Context = new NAT02Context();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllTabletProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletCheckedBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletSubmittedBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletDrawnBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTablet))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First();
                        _nat02Context.EoiProjectsFinished.Remove(projectsFinished);
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        IMethods.WriteToErrorLog("CompleteTabletProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                selectedProjects.Clear();

                MainRefresh();
            }
        }
        private void CheckTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right-clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllTabletProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletCheckedBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletSubmittedBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).TabletDrawnBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTablet))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();
                        using var _nat02Context = new NAT02Context();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            IMethods.CheckProject(project.Item1, project.Item2, "TABLETS", User);
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

                            // Drive specification transition name to "Completed"
                            // Auto archive project specification
                            string _name = project.Item1 + (int.Parse(project.Item2) > 0 ? "_" + project.Item2 : "");
                            bool? _tools = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Tools;
                            Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                            spec.StateName = _tools == true ? "Sent to Tools" : "Completed";
                            spec.IsArchived = (bool)!_tools;
                            _driveworksContext.Specifications.Update(spec);


                            try
                            {
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
                                    IMethods.SendProjectCompletedEmailToCSRAsync(_CSRs, project.Item1, project.Item2, User);
                                }
                            }
                            catch
                            {

                            }
                            finally
                            {
                                // Save pending changes
                                _projectsContext.SaveChanges();
                                _driveworksContext.SaveChanges();
                            }
                        }
                        // Dispose of contexts
                        _projectsContext.Dispose();
                        _driveworksContext.Dispose();
                        _nat02Context.Dispose();
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        IMethods.WriteToErrorLog("CheckTabletProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                MainRefresh();
            }
        }
        private void CancelTabletProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = validProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();
                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.CancelProject(project.Item1, project.Item2, User);
                    }
                    else
                    {

                        MessageBoxResult res = MessageBox.Show("Are you sure you want to cancel project# " + int.Parse(project.Item1) + "_" + int.Parse(project.Item2) + "?", "Are You Sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                                IMethods.WriteToErrorLog("SetOnHold", ex.Message, User);
                            }
                        }
                    }
                    _projectsContext.Dispose();
                    _driveworksContext.Dispose();
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                MainRefresh();
            }
        }
        private void StartToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllToolProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTool))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();

                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            IMethods.StartProject(project.Item1, project.Item2, "TOOLS", User);
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
                        IMethods.WriteToErrorLog("StartToolProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                selectedProjects.Clear();
                MainRefresh();
            }

        }
        private void FinishToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllToolProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ToolDrawnBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTool))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();


                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            IMethods.DrawProject(project.Item1, project.Item2, "TOOLS", User);
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
                        IMethods.WriteToErrorLog("FinishToolProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                selectedProjects.Clear();
                MainRefresh();
            }
        }
        private void CheckToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    using var _nat02Context = new NAT02Context();
                    bool alreadyThere = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).Any();
                    _nat02Context.Dispose();

                    if (!alreadyThere)
                    {
                        try
                        {
                            // Check to see if the project is in the correct module
                            if (project.Item4 == "AllToolProjects")
                            {
                                using var _ = new ProjectsContext();
                                if (!string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ToolCheckedBy) ||
                                    string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ToolDrawnBy) ||
                                    string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTool))
                                {
                                    _.Dispose();
                                    continue;
                                }
                                _.Dispose();
                            }

                            // Uncheck project expander
                            project.Item3.IsChecked = false;

                            using var _projectsContext = new ProjectsContext();
                            using var _driveworksContext = new DriveWorksContext();


                            if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                            {
                                IMethods.CheckProject(project.Item1, project.Item2, "TOOLS", User);
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
                                IMethods.SendProjectCompletedEmailToCSRAsync(_CSRs, int.Parse(project.Item1).ToString(), int.Parse(project.Item2).ToString(), User);

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
                            IMethods.WriteToErrorLog("CheckToolProject_Click", ex.Message, User);
                        }
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                selectedProjects.Clear();
                MainRefresh();
            }
        }
        private void OnHoldToolProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OnHoldCommentWindow onHoldCommentWindow = new OnHoldCommentWindow("Tools", _projectNumber, _revNumber, this, User)
                {
                    Left = Left,
                    Top = Top
                };
                onHoldCommentWindow.Show();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("OnHoldToolProject_Click", ex.Message, User);
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
                    IMethods.TakeProjectOffHold(_projectNumber.ToString(), _revNumber.ToString());
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
                IMethods.WriteToErrorLog("OffHoldToolProject_Click", ex.Message, User);
            }
        }
        private void CompleteToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                using var _nat02Context = new NAT02Context();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    try
                    {
                        // Check to see if the project is in the correct module
                        if (project.Item4 == "AllToolProjects")
                        {
                            using var _ = new ProjectsContext();
                            if (string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ToolCheckedBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ToolDrawnBy) ||
                                string.IsNullOrEmpty(_.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).ProjectStartedTool))
                            {
                                _.Dispose();
                                continue;
                            }
                            _.Dispose();
                        }

                        // Uncheck project expander
                        project.Item3.IsChecked = false;

                        EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2));
                        _nat02Context.EoiProjectsFinished.Remove(projectsFinished);
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        IMethods.WriteToErrorLog("CompleteToolProject_Click", ex.Message, User);
                    }
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                selectedProjects.Clear();

                MainRefresh();
            }
        }
        private void CancelToolProject_Click(object sender, RoutedEventArgs e)
        {
            if (selectedProjects.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validProjects = selectedProjects.Where(p => p.Item4 == rClickModule).ToList();

                //foreach ((string, string, CheckBox) project in selectedProjects)
                int count = validProjects.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, string, CheckBox, string) project = validProjects[i];
                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.CancelProject(project.Item1, project.Item2, User);
                    }
                    else
                    {
                        MessageBoxResult res = MessageBox.Show("Are you sure you want to cancel project# " + project.Item1 + "_" + project.Item2 + "?", "Are You Sure?", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                                IMethods.WriteToErrorLog("SetOnHold", ex.Message, User);
                            }
                        }
                    }
                    _projectsContext.Dispose();
                    _driveworksContext.Dispose();
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

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
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == rClickModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    var item = (((((((order.Item2.Parent as Grid).Parent as Border).TemplatedParent as ToggleButton).Parent as Grid).Parent as Grid).Parent as DockPanel).Parent as Border);
                    var item2 = ((((item.TemplatedParent as Expander).Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First();
                    workOrder = new WorkOrder(int.Parse(order.Item1), this);
                    string module = headers.First(kvp => kvp.Value == item2.Content.ToString()).Key;
                    int retVal = workOrder.TransferOrder(User, "D080", module == "EnteredUnscanned");
                    if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }

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

                    // Uncheck order expander
                    order.Item2.IsChecked = false;

                    DeleteMachineVariables(workOrder.OrderNumber.ToString());
                }

                try
                {
                    Cursor = Cursors.Wait;
                    Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                    Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)
                        app.Application.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                    mailItem.Subject = "REQUEST FOR CHANGES WO# " + string.Join(",", validOrders.Select(o => o.Item1));
                    mailItem.To = IMethods.GetEmailAddress(workOrder.Csr);
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

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}
            }
            // Scan just the order that was right clicked if nothing else has been selected
            else
            {
                workOrder = new WorkOrder((int)_orderNumber, this);
                int retVal = workOrder.TransferOrder(User, "D080");
                if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }

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
                    mailItem.To = IMethods.GetEmailAddress(workOrder.Csr);
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

            MainRefresh();
        }
        private void StartWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == rClickModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    var item = (((((((order.Item2.Parent as Grid).Parent as Border).TemplatedParent as ToggleButton).Parent as Grid).Parent as Grid).Parent as DockPanel).Parent as Border);
                    var item2 = ((((item.TemplatedParent as Expander).Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First();
                    string module = headers.First(kvp => kvp.Value == item2.Content.ToString()).Key;
                    workOrder = new WorkOrder(int.Parse(order.Item1), this);
                    int retVal = workOrder.TransferOrder(User, "D040", module == "EnteredUnscanned");
                    if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }

                    // Uncheck order expander
                    order.Item2.IsChecked = false;
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}
            }

            MainRefresh();
        }
        private void ToProdManOrder_Click(object sender, RoutedEventArgs e)
        {
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == rClickModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
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
                    workOrder = new WorkOrder(int.Parse(order.Item1), this);
                    int retVal = workOrder.TransferOrder(User, "D921");
                    if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }

                    // Uncheck order expander
                    order.Item2.IsChecked = false;

                    // Check EOI_TrackedDocuments table to see if this order needs a notification
                    using var _nat02context = new NAT02Context();
                    bool tracked = _nat02context.EoiTrackedDocuments.Any(d => d.Number == order.Item1 && d.MovementId == 3);
                    if (tracked)
                    {
                        // Retrieve tracked document
                        EoiTrackedDocuments trackedDoc = _nat02context.EoiTrackedDocuments.Single(d => d.Number == order.Item1 && d.MovementId == 3);

                        // Insert into EOI_Notifications_Active
                        EoiNotificationsActive _active = new EoiNotificationsActive()
                        {
                            Type = trackedDoc.Type,
                            Number = trackedDoc.Number,
                            Message = "Document has moved to production",
                            User = trackedDoc.User,
                            Timestamp = DateTime.Now
                        };
                        _nat02context.EoiNotificationsActive.Add(_active);

                        // Delete from EOI_TrackedDocuments
                        _nat02context.EoiTrackedDocuments.Remove(trackedDoc);

                        // Save context transactions
                        _nat02context.SaveChanges();
                    }
                    _nat02context.Dispose();
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}
            }

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

            projectWindow.Show();

            // Dispose of project window THIS STOPS THE TIMER.
            //projectWindow.Dispose();
        }
        private void ProjectSearch_Click(object sender, RoutedEventArgs e)
        {
            ProjectSearchWindow projectSearchWindow = new ProjectSearchWindow()
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            projectSearchWindow.Show();
            projectSearchWindow.Dispose();
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

            try
            {
                // New list of projects that are in the same module that was right clicked inside of
                List<(string, string, CheckBox, string)> validQuotes = selectedQuotes.Where(p => p.Item4 == rClickModule).ToList();

                if (validQuotes.Any())
                {
                    for (int i = 0; i < validQuotes.Count; i++)
                    {
                        (string, string, CheckBox, string) quote = validQuotes[i];
                        quote.Item3.IsChecked = false;

                        EoiQuotesOneWeekCompleted q = new EoiQuotesOneWeekCompleted()
                        {
                            QuoteNo = double.Parse(quote.Item1),
                            QuoteRevNo = int.Parse(quote.Item2),
                            TimeSubmitted = DateTime.Now,
                            FollowUpsCompleted = _nat02context.EoiQuotesOneWeekCompleted.Count(m => m.QuoteNo == double.Parse(quote.Item1) && m.QuoteRevNo == int.Parse(quote.Item2)) + 1
                        };
                        _nat02context.Add(q);
                    }
                }
            }
            catch
            {

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
            // New list of projects that are in the same module that was right clicked inside of
            List<(string, string, CheckBox, string)> validQuotes = selectedQuotes.Where(p => p.Item4 == rClickModule).ToList();
            List<Tuple<int, short>> quotes = new List<Tuple<int, short>>();
            List<Quote> quoteItems = new List<Quote>();
            List<string> quoteErrorNumbers = new List<string>();
            if (validQuotes.Any())
            {

                for (int i = 0; i < validQuotes.Count; i++)
                {
                    quotes.Add(new Tuple<int, short>(Convert.ToInt32(validQuotes[i].Item1), Convert.ToInt16(validQuotes[i].Item2)));
                }
                OrderingWindow orderingWindow = new OrderingWindow(quotes, User);
                if (orderingWindow.ShowDialog() == true)
                {
                    foreach (Tuple<int, short> quote in quotes)
                    {
                        if (IMethods.QuoteErrors(quote.Item1.ToString(), quote.Item2.ToString(), User).Count > 0)
                        {
                            quoteErrorNumbers.Add(quote.Item1.ToString() + "-" + quote.Item2.ToString());
                        }
                        quoteItems.Add(new Quote(quote.Item1, quote.Item2));
                    }
                    if (quoteErrorNumbers.Any() && MessageBoxResult.Yes != MessageBox.Show((quoteErrorNumbers.Count == 1 ? "Quote" : "Quotes") + string.Concat(quoteErrorNumbers.Select(q => q + ", ")).TrimEnd().TrimEnd(',') + (quoteErrorNumbers.Count == 1 ? " has" : " have") + " quote check errors.\n\nWould you still like to submit these quotes?", "ERRORS", MessageBoxButton.YesNo, MessageBoxImage.Question))
                    {
                        // Do nothing
                    }
                    else
                    {
                        foreach (Quote quote in quoteItems)
                        {
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
                            quote.Dispose();
                        }
                    }
                }

                //for (int i = 0; i < validQuotes.Count; i++)
                //{
                //    (string, string, CheckBox, string) selectedQuote = validQuotes[i];
                //    selectedQuote.Item3.IsChecked = false;

                //    Quote quote = new Quote(int.Parse(selectedQuote.Item1), short.Parse(selectedQuote.Item2));

                //    if (IMethods.QuoteErrors(quote.QuoteNumber.ToString(), quote.QuoteRevNo.ToString(), User).Count > 0 && MessageBoxResult.Yes != MessageBox.Show("Quote " + quote.QuoteNumber.ToString() + "-" + quote.QuoteRevNo.ToString() + " has quote check errors.\n Would you still like to submit this quote?", "ERRORS", MessageBoxButton.YesNo, MessageBoxImage.Question))
                //    {
                //        // Do nothing
                //    }
                //    else
                //    {
                //        QuoteHeader r = context.QuoteHeader.Where(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo).FirstOrDefault();
                //        string customerName = necContext.Rm00101.Where(c => c.Custnmbr == r.UserAcctNo).First().Custname;
                //        string csr = context.QuoteRepresentative.Where(r => r.RepId == quote.QuoteRepID).First().Name;
                //        EoiQuotesMarkedForConversion q = new EoiQuotesMarkedForConversion()
                //        {
                //            QuoteNo = quote.QuoteNumber,
                //            QuoteRevNo = quote.QuoteRevNo,
                //            CustomerName = customerName,
                //            Csr = csr,
                //            CsrMarked = User.GetUserName(),
                //            TimeSubmitted = DateTime.Now,
                //            Rush = r.RushYorN
                //        };
                //        nat02Context.EoiQuotesMarkedForConversion.Add(q);
                //    }
                //}


                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}

            }

            nat02Context.SaveChanges();
            nat02Context.Dispose();
            necContext.Dispose();
            context.Dispose();
            Cursor = Cursors.Arrow;
            MainRefresh();
        }
        private void RecallQuote_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            using var context = new NAT01Context();
            using var nat02Context = new NAT02Context();
            using var necContext = new NECContext();
            // New list of projects that are in the same module that was right clicked inside of
            List<(string, string, CheckBox, string)> validQuotes = selectedQuotes.Where(p => p.Item4 == rClickModule).ToList();

            if (validQuotes.Any())
            {
                for (int i = 0; i < validQuotes.Count; i++)
                {
                    (string, string, CheckBox, string) selectedQuote = validQuotes[i];
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
                    nat02Context.EoiQuotesMarkedForConversion.Remove(q);
                }

                // Uncheck Check All CheckBox
                //var x = MainGrid.Children;
                //foreach (Border border in x.OfType<Border>())
                //{
                //    string header = (border.Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First().Content.ToString();
                //    if (headers.Single(h => h.Value == header).Key == rClickModule)
                //    {
                //        ((border.Child as DockPanel).Children.OfType<Border>().First().Child as Grid).Children.OfType<CheckBox>().First().IsChecked = false;
                //    }
                //}
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
                nat02Context.EoiQuotesMarkedForConversion.Remove(q);
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
            using var _ = new NAT02Context();
            _.EoiSettings.Single(u => u.DomainName == User.DomainName).FilterActiveProjects = _filterProjects;
            _.SaveChanges();
            _.Dispose();
            User.FilterActiveProjects = _filterProjects;
            MainRefresh("AllTabletProjects");
            MainRefresh("AllToolProjects");
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
        private void Subscriptions_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem topMenu = (MenuItem)sender;
                using var nat02context = new NAT02Context();
                string subs = "";
                foreach (MenuItem item in topMenu.Items)
                {
                    if (item.IsChecked) { subs += item.Header.ToString() + ','; }
                }
                if (subs.Length > 0)
                {
                    subs = subs.Substring(0, subs.Length - 1);
                }
                EoiSettings sub = nat02context.EoiSettings.First(u => u.EmployeeId == User.EmployeeCode);
                if (sub.Subscribed != subs)
                {
                    sub.Subscribed = subs;
                    nat02context.Update(sub);
                    nat02context.SaveChanges();
                    MainRefresh();
                }
                nat02context.Dispose();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("MainWindow.xaml.cs => Subscriptions_SubmenuClosed()", ex.Message, User);
            }
        }
        #endregion
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
                IMethods.WriteToErrorLog("QuoteSearchButton_Click - Before new window instance", ex.Message, User);
            }
            try
            {
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, User)
                {
                    Left = Left,
                    Top = Top
                };
                quoteInfoWindow.Show();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
                IMethods.WriteToErrorLog("QuoteSearchButton_Click - After new window instance, quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, User);
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
        private void QuoterevNoSearchTextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.CaptureMouse();
        }
        private void QuoterevNoSearchTextBlock_GotMouseCapture(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.SelectAll();

            textBox.SelectionStart = 0;

            textBox.SelectionLength = textBox.Text.Length;
        }
        private void QuoterevNoSearchTextBlock_IsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.SelectAll();

            textBox.SelectionStart = 0;

            textBox.SelectionLength = textBox.Text.Length;
        }
        private void ProjectSearchButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            using var _projectesContext = new ProjectsContext();
            try
            {
                string projectNumber = ProjectSearchTextBlock.Text;
                string revNumber = ProjectRevNoSearchTextBlock.Text;
                if (User.EmployeeCode == "E4754" && (_projectesContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == revNumber) || _projectesContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == revNumber)))
                {
                    try
                    {
                        ProjectWindow projectWindow = new ProjectWindow(projectNumber, revNumber, this, User, false);
                        projectWindow.Show();
                        // projectWindow.Dispose(); THIS STOPS THE TIMERS FROM WORKING
                    }
                    catch (Exception ex)
                    {
                        // MessageBox.Show(ex.Message);
                        IMethods.WriteToErrorLog("ProjectSearchButton_Click - After new window instance", ex.Message, User);
                    }
                }
                else
                {
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
                        IMethods.WriteToErrorLog("ProjectSearchButton_Click - Before new window instance", ex.Message, User);
                    }
                }
                
            }
            catch (Exception ex)
            {

                // MessageBox.Show(ex.Message);
                IMethods.WriteToErrorLog("ProjectSearchButton_Click - After new window instance", ex.Message, User);
            }
            _projectesContext.Dispose();
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
                        workOrder = new WorkOrder(int.Parse(orderNumber), this);
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
                    IMethods.WriteToErrorLog("OrderSearchButton_Click - Before new window instance", ex.Message, User);
                }
                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, this, "", User)
                {
                    Left = Left,
                    Top = Top
                };
                orderInfoWindow.Show();
                orderInfoWindow.Dispose();
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
                    int revNo = 0;
                    if (ProjectSearchTextBlock.Text.Length > 0 && ProjectRevNoSearchTextBlock.Text.Length == 0)
                    {
                        using var _projectscontext = new ProjectsContext();
                        if (_projectscontext.EngineeringProjects.Any(p => p.ProjectNumber == ProjectSearchTextBlock.Text))
                        {
                            revNo = _projectscontext.EngineeringProjects.Where(p => p.ProjectNumber == ProjectSearchTextBlock.Text).Max(p => Convert.ToInt32(p.RevNumber));
                        }
                        else if (_projectscontext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == ProjectSearchTextBlock.Text))
                        {
                            revNo = _projectscontext.EngineeringArchivedProjects.Where(p => p.ProjectNumber == ProjectSearchTextBlock.Text).Max(p => Convert.ToInt32(p.RevNumber));
                        }
                        else if(_projectscontext.ProjectSpecSheet.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)))
                        {
                            revNo = _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)).Max(p => (int)p.RevisionNumber);
                        }
                        ProjectRevNoSearchTextBlock.Text = revNo.ToString();
                        _projectscontext.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("MainWindow.cs => ProjectSearchTextBlock_PreviewKeyDown", ex.Message, User);
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
        private void FuzzySearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ContentControl contentControl = MainMenu.FindName("CustomerSearchListBox") as ContentControl;
            Canvas canvas = (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
                                                                                                      .Children.OfType<Canvas>().First();
            ListBox listBox = canvas.Children.OfType<ListBox>().First();
            string searchText = (sender as TextBox).Text;

            if (searchText.Length > 0)
            {
                int i = 0;
                while (!char.IsLetter(searchText[i]) && i < searchText.Length - 1) { i++; }
                if ((i == 0 || i < searchText.Length - 1) && searchText.Length > 1)
                {
                    using var _ = new NECContext();

                    List<Rm00101> customers = _.Rm00101.Where(r => r.Custname.Trim().ToLower().Contains(searchText.ToLower())).ToList();
                    List<(string, double)> scores = new List<(string, double)>();

                    var l = new NormalizedLevenshtein();
                    double dist = 0.0;

                    foreach (Rm00101 customer in customers)
                    {
                        dist = 1 - l.Distance(searchText.ToLower(), customer.Custname.ToLower());

                        // If a customer is user's customer, rank it higher
                        if (User.Department == "Customer Service")
                        {
                            using var __ = new NAT01Context();
                            QuoteRepresentative qr = __.QuoteRepresentative.Single(r => r.Name.ToLower().Trim() == User.GetDWDisplayName().ToLower().Trim());

                            if (!string.IsNullOrEmpty(qr.RepId))
                            {
                                var info = __.QuoteHeader.Where(q => q.UserAcctNo == customer.Custnmbr.ToLower() && q.QuoteDate > DateTime.Now.AddYears(-1).Date)
                                                         .GroupBy(q => q.QuoteRepId)
                                                         .Select(x => new { RepId = x.Key, Count = x.Count() })
                                                         .OrderByDescending(a => a.Count);
                                if (info.Count() > 0)
                                {
                                    if (info.First().RepId.Trim().ToLower() == qr.RepId.Trim().ToLower())
                                    {
                                        dist = dist * 25;
                                    }
                                }
                            }
                            __.Dispose();
                        }

                        scores.Add((customer.Custname.Trim(), dist));
                    }

                    List<string> res = scores.OrderByDescending(s => s.Item2).Take(10).Select(s => s.Item1).ToList();

                    listBox.ItemsSource = res;
                    _.Dispose();
                    listBox.SelectedIndex = 0;
                    listBox.Visibility = Visibility.Visible;
                }
                else
                {

                }
            }
            else
            {
                listBox.Visibility = Visibility.Collapsed;
            }
        }
        private void FuzzySearchTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            bool isNumeric = true;
            foreach (char c in textBox.Text)
            {
                if (char.IsLetter(c)) { isNumeric = false; break; }
            }
            if (e.Key == Key.Enter)
            {
                if (textBox.Text.Length > 0)
                {
                    if ((textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().Visibility == Visibility.Visible)
                    {
                        // Get customer number for top name
                        int i = (textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().SelectedIndex;
                        string customerName = (textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().Items[i].ToString();
                        using var _ = new NECContext();
                        string customerNumber = _.Rm00101.First(r => r.Custname.Trim().ToLower() == customerName.Trim().ToLower()).Custnmbr.Trim();
                        _.Dispose();

                        // Open customer window
                        CustomerInfoWindow customerInfoWindow = new CustomerInfoWindow(User, this, customerNumber);
                        customerInfoWindow.Show();

                        (textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().Visibility = Visibility.Collapsed;
                    }
                    else if (isNumeric)
                    {
                        // Check if the text is a valid customer number
                        using var _ = new NECContext();
                        if (_.Rm00101.Any(r => r.Custnmbr.Trim() == textBox.Text))
                        {
                            // Open customer window
                            CustomerInfoWindow customerInfoWindow = new CustomerInfoWindow(User, this, textBox.Text);
                            customerInfoWindow.Show();

                            (textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().Visibility = Visibility.Collapsed;
                        }
                        _.Dispose();
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                (textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().SelectedIndex++;
            }
            else if (e.Key == Key.Up)
            {
                (textBox.Parent as Grid).Children.OfType<Canvas>().First().Children.OfType<ListBox>().First().SelectedIndex--;
            }
            else if (e.Key == Key.Escape)
            {
                textBox.Text = "";
            }
        }
        private void FuzzySearchListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            // Get customer number selected name
            string customerName = listBox.SelectedItem.ToString();
            using var _ = new NECContext();
            string customerNumber = _.Rm00101.First(r => r.Custname.Trim().ToLower() == customerName.Trim().ToLower()).Custnmbr.Trim();
            _.Dispose();

            // Open customer window
            CustomerInfoWindow customerInfoWindow = new CustomerInfoWindow(User, this, customerNumber);
            customerInfoWindow.Show();

            listBox.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region New Modules
        private void BuildPanels()
        {
            try
            {
                foreach (string panel in User.VisiblePanels)
                {
                    AddModule(panel);
                }
            }
            catch
            {

            }
        }
        public Grid AddModule(string name, int index = -1)
        {
            ColumnDefinition columnDefinition = new ColumnDefinition
            {
                SharedSizeGroup = "WrapWidth",
                Width = GridLength.Auto
            };

            Grid grid = new Grid
            {
                Margin = new Thickness(10)
            };

            grid.ColumnDefinitions.Add(columnDefinition);

            Label label = null;

            switch (name, "Main")
            {
                case ("BeingEntered", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["OrdersBeingEnteredModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    OrdersBeingEnteredListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.Invoke(() => BindBeingEntered()));

                    OrdersBeingEnteredListBox.ItemsSource = null;
                    OrdersBeingEnteredListBox.ItemsSource = ordersBeingEntered;
                    return grid;
                case ("InTheOffice", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["OrdersInTheOfficeModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    OrdersInTheOfficeListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.Invoke(() => BindInTheOffice()));

                    OrdersInTheOfficeListBox.ItemsSource = null;
                    OrdersInTheOfficeListBox.ItemsSource = ordersInTheOffice;
                    return grid;
                case ("QuotesNotConverted", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["QuotesNotConvertedModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    QuotesNotConvertedListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetQuotesNotConverted()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesNotConverted()));

                    QuotesNotConvertedListBox.ItemsSource = null;
                    QuotesNotConvertedListBox.ItemsSource = quotesNotConverted;
                    return grid;
                case ("EnteredUnscanned", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["OrdersEnteredModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    OrdersEnteredListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.Invoke(() => BindEnteredUnscanned()));

                    OrdersEnteredListBox.ItemsSource = null;
                    OrdersEnteredListBox.ItemsSource = ordersEntered;
                    return grid;
                case ("InEngineering", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["OrdersInEngineeringModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    OrdersInEngListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindInEngineering()));

                    OrdersInEngListBox.ItemsSource = null;
                    OrdersInEngListBox.ItemsSource = ordersInEng;
                    return grid;
                case ("QuotesToConvert", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["QuotesToConvertModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    QuotesToConvertListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetQuotesToConvert()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesToConvert()));

                    QuotesToConvertListBox.ItemsSource = null;
                    QuotesToConvertListBox.ItemsSource = quotesToConvert;
                    return grid;
                case ("ReadyToPrint", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["OrdersReadyToPrintModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    OrdersReadyToPrintListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.Invoke(() => BindReadyToPrint()));

                    OrdersReadyToPrintListBox.ItemsSource = null;
                    OrdersReadyToPrintListBox.ItemsSource = ordersReadyToPrint;
                    return grid;
                case ("PrintedInEngineering", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["OrdersPrintedInEngineeringModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    OrdersPrintedListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindPrintedInEngineering()));

                    OrdersPrintedListBox.ItemsSource = null;
                    OrdersPrintedListBox.ItemsSource = ordersPrinted;
                    return grid;
                case ("AllTabletProjects", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["AllTabletProjectsModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    AllTabletProjectsListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()));

                    AllTabletProjectsListBox.ItemsSource = null;
                    AllTabletProjectsListBox.ItemsSource = allTabletProjects;
                    return grid;
                //case ("TabletProjectsNotStarted", "Main"):
                //    Task.Run(() => GetTabletProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsNotStarted()), TaskScheduler.Current);
                //    break;
                //case ("TabletProjectsStarted", "Main"):
                //    Task.Run(() => GetTabletProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsStarted()), TaskScheduler.Current);
                //    break;
                //case ("TabletProjectsDrawn", "Main"):
                //    Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
                //    break;
                //case ("TabletProjectsSubmitted", "Main"):
                //    Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
                //    break;
                //case ("TabletProjectsOnHold", "Main"):
                //    Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
                //    break;
                case ("AllToolProjects", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["AllToolProjectsModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    AllToolProjectsListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()));

                    AllToolProjectsListBox.ItemsSource = null;
                    AllToolProjectsListBox.ItemsSource = allToolProjects;
                    return grid;
                //case ("ToolProjectsNotStarted", "Main"):
                //    Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
                //    break;
                //case ("ToolProjectsStarted", "Main"):
                //    Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
                //    break;
                //case ("ToolProjectsDrawn", "Main"):
                //    Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
                //    break;
                //case ("ToolProjectsOnHold", "Main"):
                //    Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
                //    break;
                case ("DriveWorksQueue", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["DriveWorksQueueModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    DriveWorksQueueListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()));

                    DriveWorksQueueListBox.ItemsSource = null;
                    DriveWorksQueueListBox.ItemsSource = driveWorksQueue;
                    return grid;
                case ("NatoliOrderList", "Main"):
                    label = new Label
                    {
                        Style = App.Current.Resources["NatoliOrderListModule"] as Style
                    };

                    label.ApplyTemplate();

                    grid.Children.Add(label);

                    if (index == -1)
                    {
                        MainWrapPanel.Children.Add(grid);
                    }
                    else
                    {
                        MainWrapPanel.Children.Insert(index, grid);
                    }

                    NatoliOrderListListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

                    Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()));

                    NatoliOrderListListBox.ItemsSource = null;
                    NatoliOrderListListBox.ItemsSource = natoliOrderList;
                    return grid;
                default:
                    return grid;
            }
        }
        private void BindData(string timer)
        {
            try
            {
                foreach (string panel in User.VisiblePanels)
                {
                    switch (panel, timer)
                    {
                        case ("BeingEntered", "Main"):
                            //Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.Invoke(() => BindBeingEntered()), TaskScheduler.Current);
                            Task.Run(() => GetBeingEntered());
                            break;
                        case ("InTheOffice", "Main"):
                            //Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.Invoke(() => BindInTheOffice()), TaskScheduler.Current);
                            Task.Run(() => GetInTheOffice());
                            break;
                        case ("QuotesNotConverted", "QuotesNotConverted"):
                            Dispatcher.Invoke(() => GetQuotesNotConverted());
                            break;
                        case ("EnteredUnscanned", "Main"):
                            //Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.Invoke(() => BindEnteredUnscanned()), TaskScheduler.Current);
                            Task.Run(() => GetEnteredUnscanned());
                            break;
                        case ("InEngineering", "Main"):
                            //Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindInEngineering()), TaskScheduler.Current);
                            Task.Run(() => GetInEngineering());
                            break;
                        case ("QuotesToConvert", "Main"):
                            Dispatcher.Invoke(() => GetQuotesToConvert());
                            break;
                        case ("ReadyToPrint", "Main"):
                            //Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.Invoke(() => BindReadyToPrint()), TaskScheduler.Current);
                            Task.Run(() => GetReadyToPrint());
                            break;
                        case ("PrintedInEngineering", "Main"):
                            //Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindPrintedInEngineering()), TaskScheduler.Current);
                            Task.Run(() => GetPrintedInEngineering());
                            break;
                        case ("AllTabletProjects", "Main"):
                            Task.Run(() => GetAllTabletProjects());
                            break;
                        //case ("TabletProjectsNotStarted", "Main"):
                        //    Task.Run(() => GetTabletProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsNotStarted()), TaskScheduler.Current);
                        //    break;
                        //case ("TabletProjectsStarted", "Main"):
                        //    Task.Run(() => GetTabletProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsStarted()), TaskScheduler.Current);
                        //    break;
                        //case ("TabletProjectsDrawn", "Main"):
                        //    Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
                        //    break;
                        //case ("TabletProjectsSubmitted", "Main"):
                        //    Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
                        //    break;
                        //case ("TabletProjectsOnHold", "Main"):
                        //    Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
                        //    break;
                        case ("AllToolProjects", "Main"):
                            Task.Run(() => GetAllToolProjects());
                            break;
                        //case ("ToolProjectsNotStarted", "Main"):
                        //    Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
                        //    break;
                        //case ("ToolProjectsStarted", "Main"):
                        //    Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
                        //    break;
                        //case ("ToolProjectsDrawn", "Main"):
                        //    Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
                        //    break;
                        //case ("ToolProjectsOnHold", "Main"):
                        //    Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
                        //    break;
                        case ("DriveWorksQueue", "Main"):
                            Task.Run(() => GetDriveWorksQueue());
                            break;
                        case ("NatoliOrderList", "NatoliOrderList"):
                            Task.Run(() => GetNatoliOrderList());
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
        private void UpdateUI()
        {
            Task.Run(() => Dispatcher.Invoke(() => BindQuotesNotConverted()));
            Task.Run(() => Dispatcher.Invoke(() => BindQuotesToConvert()));
            Task.Run(() => Dispatcher.Invoke(() => BindBeingEntered()));
            Task.Run(() => Dispatcher.Invoke(() => BindInTheOffice()));
            Task.Run(() => Dispatcher.Invoke(() => BindEnteredUnscanned()));
            Task.Run(() => Dispatcher.Invoke(() => BindInEngineering()));
            Task.Run(() => Dispatcher.Invoke(() => BindReadyToPrint()));
            Task.Run(() => Dispatcher.Invoke(() => BindPrintedInEngineering()));
            Task.Run(() => Dispatcher.Invoke(() => BindAllTabletProjects()));
            Task.Run(() => Dispatcher.Invoke(() => BindAllToolProjects()));
            Task.Run(() => Dispatcher.Invoke(() => BindDriveWorksQueue()));
            Task.Run(() => Dispatcher.Invoke(() => BindNatoliOrderList()));
        }

        #region Gets And Binds
        private void GetBeingEntered()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersBeingEntered = _.EoiOrdersBeingEnteredView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindBeingEntered()
        {
            string searchString = GetSearchString("BeingEntered");

            _ordersBeingEntered =
                _ordersBeingEntered.Where(o => o.OrderNo.ToString().ToLower().Contains(searchString) ||
                                               o.QuoteNo.ToString().Contains(searchString) ||
                                               o.CustomerName.ToLower().Contains(searchString))
                                   .OrderBy(kvp => kvp.OrderNo)
                                   .ToList();

            OrdersBeingEntered = _ordersBeingEntered;
        }
        private void GetInTheOffice()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersInTheOffice = _.EoiOrdersInOfficeView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindInTheOffice()
        {
            string searchString = GetSearchString("InTheOffice");

            _ordersInTheOffice =
                _ordersInTheOffice.Where(o => o.OrderNo.ToString().ToLower().Contains(searchString) ||
                                              o.CustomerName.ToLower().ToString().Contains(searchString) ||
                                              o.EmployeeName.ToLower().Contains(searchString) ||
                                              o.Csr.ToLower().Contains(searchString))
                                  .OrderBy(o => o.NumDaysToShip)
                                  .ThenBy(o => o.DaysInOffice)
                                  .ThenBy(o => o.OrderNo)
                                  .ToList();

            OrdersInTheOffice = _ordersInTheOffice;
        }
        private void GetQuotesNotConverted()
        {
            try
            {
                using var _ = new NAT02Context();
                IQueryable<string> subList = _.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                         .Select(e => e.Subscribed);
                string[] subs = subList.First().Split(',');
                //quotesCompletedChanged = (quotesCompletedCount != _.EoiQuotesOneWeekCompleted.Count());
                //quotesCompletedCount = _.EoiQuotesOneWeekCompleted.Count();
                short quoteDays = User.QuoteDays;
                List<EoiQuotesNotConvertedView> _eoiQuotesNotConvertedView = new List<EoiQuotesNotConvertedView>();
                foreach (string sub in subs)
                {
                    string s = sub;
                    if (sub == "Nicholas")
                    {
                        s = "Nick";
                    }
                    _eoiQuotesNotConvertedView.AddRange(_.EoiQuotesNotConvertedView.Where(q => q.Csr.Contains(s) && q.QuoteDate >= DateTime.Now.AddDays(-quoteDays)).ToList());
                }
                _quotesNotConverted = _eoiQuotesNotConvertedView.Where(q => q.QuoteDate >= DateTime.Now.AddDays(-quoteDays)).OrderByDescending(q => q.QuoteNo).ThenByDescending(q => q.QuoteRevNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindQuotesNotConverted()
        {
            string searchString = GetSearchString("QuotesNotConverted");

            if (searchString.ToLower().StartsWith("rep:"))
            {
                searchString = searchString.Substring(4);
                var _filtered =
                _quotesNotConverted.Where(p => p.RepId.ToLower().Trim() == searchString)
                                   .OrderByDescending(kvp => kvp.QuoteNo)
                                   .ToList();
            }
            else
            {
                var _filtered =
                _quotesNotConverted.Where(p => p.QuoteNo.ToString().ToLower().Contains(searchString) ||
                                               p.QuoteRevNo.ToString().ToLower().Contains(searchString) ||
                                               p.CustomerName.ToLower().Contains(searchString) ||
                                               p.Csr.ToLower().Contains(searchString))
                                   .OrderByDescending(kvp => kvp.QuoteNo)
                                   .ToList();
            }

            QuotesNotConverted = _quotesNotConverted;
        }
        private void GetEnteredUnscanned()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersEntered = _.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindEnteredUnscanned()
        {
            string searchString = GetSearchString("EnteredUnscanned");

            _ordersEntered =
                _ordersEntered.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                          p.CustomerName.ToLower().Contains(searchString))
                              .OrderBy(kvp => kvp.OrderNo)
                              .ToList();

            OrdersEntered = _ordersEntered;
        }
        private void GetInEngineering()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersInEng = _.EoiOrdersInEngineeringUnprintedView.OrderByDescending(o => o.DaysInEng).ThenBy(o => o.NumDaysToShip).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindInEngineering()
        {
            string searchString = GetSearchString("InEngineering");

            _ordersInEng =
                _ordersInEng.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                        p.CustomerName.ToLower().Contains(searchString) ||
                                        p.EmployeeName.ToLower().Contains(searchString))
                            .OrderByDescending(kvp => kvp.DaysInEng)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNo)
                            .ToList();

            OrdersInEng = _ordersInEng;
        }
        private void GetQuotesToConvert()
        {
            try
            {
                using var _ = new NAT02Context();
                List<EoiQuotesMarkedForConversionView> eoiQuotesMarkedForConversion = new List<EoiQuotesMarkedForConversionView>();

                IQueryable<string> subList = _.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
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
                    _eoiQuotesMarkedForConversion.AddRange(_.EoiQuotesMarkedForConversionView.Where(q => q.Csr.Contains(s)).OrderBy(q => q.TimeSubmitted).ToList());
                }
                _quotesToConvert = _eoiQuotesMarkedForConversion;
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindQuotesToConvert()
        {
            string searchString = GetSearchString("QuotesToConvert");

            _quotesToConvert =
                _quotesToConvert.Where(p => p.QuoteNo.ToString().ToLower().Contains(searchString) ||
                                            p.QuoteRevNo.ToString().ToLower().Contains(searchString) ||
                                            p.CustomerName.ToLower().Contains(searchString) ||
                                            p.Csr.ToLower().Contains(searchString))
                                .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();

            QuotesToConvert = _quotesToConvert;
        }
        private void GetReadyToPrint()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersReadyToPrint = _.EoiOrdersReadyToPrintView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindReadyToPrint()
        {
            string searchString = GetSearchString("ReadyToPrint");

            _ordersReadyToPrint =
                _ordersReadyToPrint.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                               p.CustomerName.ToLower().Contains(searchString) ||
                                               p.EmployeeName.ToLower().Contains(searchString) ||
                                               p.CheckedBy.ToLower().Contains(searchString))
                                   .OrderBy(kvp => kvp.OrderNo)
                                   .ToList();

            OrdersReadyToPrint = _ordersReadyToPrint;
        }
        public void GetPrintedInEngineering()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersPrinted = _.EoiOrdersPrintedInEngineeringView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindPrintedInEngineering()
        {
            string searchString = GetSearchString("PrintedInEngineering");

            string column;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":

                        _ordersPrinted =
                            _ordersPrinted.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNo)
                                          .ToList();
                        break;
                    case "customer name":

                        _ordersPrinted =
                            _ordersPrinted.Where(p => p.CustomerName.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNo)
                                          .ToList();
                        break;
                    case "employee name":

                        _ordersPrinted =
                            _ordersPrinted.Where(p => p.EmployeeName.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNo)
                                          .ToList();
                        break;
                    case "checker":

                        _ordersPrinted =
                            _ordersPrinted.Where(p => p.CheckedBy.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNo)
                                          .ToList();
                        break;
                    default:

                        _ordersPrinted =
                            _ordersPrinted.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                                      p.CustomerName.ToLower().Contains(searchString) ||
                                                      p.EmployeeName.ToLower().Contains(searchString) ||
                                                      p.CheckedBy.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNo)
                                          .ToList();
                        break;
                }
            }
            else
            {
                _ordersPrinted =
                    _ordersPrinted.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                              p.CustomerName.ToLower().Contains(searchString) ||
                                              p.EmployeeName.ToLower().Contains(searchString) ||
                                              p.CheckedBy.ToLower().Contains(searchString))
                                  .OrderBy(kvp => kvp.OrderNo)
                                  .ToList();
            }

            OrdersPrinted = _ordersPrinted;
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
                if (User.FilterActiveProjects)
                {
                    _allTabletProjects = eoiAllTabletProjects.Where(p => p.HoldStatus != "On Hold" &&
                                           !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                           .OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                }
                else
                {
                    _allTabletProjects = eoiAllTabletProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                }
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindAllTabletProjects()
        {
            string searchString = GetSearchString("AllTabletProjects");

            _allTabletProjects =
                _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                              p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                              p.CustomerName.ToLower().Contains(searchString) ||
                                              p.Csr.ToLower().Contains(searchString) ||
                                              p.Drafter.ToLower().Contains(searchString))
                                  .OrderByDescending(kvp => kvp.MarkedPriority)
                                  .ThenBy(kvp => kvp.DueDate)
                                  .ThenBy(kvp => kvp.ProjectNumber)
                                  .ToList();

            AllTabletProjects = _allTabletProjects;
        }
        private void GetAllToolProjects()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiAllToolProjectsView> eoiAllToolProjects = new List<EoiAllToolProjectsView>();
                IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
                                                                     .Select(e => e.Subscribed);

                string[] subs = subList.First().Split(',');
                foreach (string sub in subs)
                {
                    string s = sub;
                    if (sub == "Gregory") { s = "Greg"; }
                    if (sub == "Nicholas") { s = "Nick"; }
                    eoiAllToolProjects.AddRange(_nat02context.EoiAllToolProjectsView.Where(q => q.Csr.Contains(s) || q.ReturnToCsr.Contains(s)).ToList());
                }
                if (User.FilterActiveProjects)
                {

                    _allToolProjects = eoiAllToolProjects.Where(p => p.HoldStatus != "On Hold" &&
                                           !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                           .OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                }
                else
                {
                    _allToolProjects = eoiAllToolProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                }
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindAllToolProjects()
        {
            string searchString = GetSearchString("AllToolProjects");

            _allToolProjects =
                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                            p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                            p.CustomerName.ToLower().Contains(searchString) ||
                                            p.Csr.ToLower().Contains(searchString) ||
                                            p.Drafter.ToLower().Contains(searchString))
                                .OrderByDescending(kvp => kvp.MarkedPriority)
                                .ThenBy(kvp => kvp.DueDate)
                                .ThenBy(kvp => kvp.ProjectNumber)
                                .ToList();

            AllToolProjects = _allToolProjects;
        }
        private void GetDriveWorksQueue()
        {
            try
            {
                using var _driveworkscontext = new DriveWorksContext();
                _driveWorksQueue = _driveworkscontext.QueueView.OrderBy(t => t.Priority).ThenBy(t => t.DateReleased).ToList();
                _driveworkscontext.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void BindDriveWorksQueue()
        {
            DriveWorksQueue = _driveWorksQueue;
        }
        private void GetNatoliOrderList()
        {
            using var _natbcContext = new NATBCContext();
            string username = Environment.UserDomainName + "\\" + Environment.UserName;
            List<NatoliOrderList> _nol = new List<NatoliOrderList>();
            if (User.Department == "D1133")
            {
                _nol = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
            }
            else if (User.EmployeeCode == "E4408")
            {
                _nol = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", @"NATOLI\dnelson").ToList();
                _nol = _nol.OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
            }
            else
            {
                _nol = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).ToList();
                _nol = _nol.OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
            }

            using var _ = new NAT01Context();
            _natoliOrderList.Clear();
            foreach (NatoliOrderList nol in _nol)
            {
                // Get repId from quote header
                OrderHeader orderHeader = _nat01context.OrderHeader.Single(o => o.OrderNo == nol.OrderNo);
                
                string acctNo = _.QuoteHeader.Single(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).UserAcctNo;
                using var __ = new NECContext();
                string repId = __.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Slprsnid;

                NatoliOrderListFinal nolf = new NatoliOrderListFinal()
                {
                    OrderNo = nol.OrderNo / 100,
                    Customer = nol.Customer,
                    ShipDate = nol.ShipDate,
                    Rush = nol.Rush,
                    OnHold = nol.OnHold,
                    RepInitials = nol.RepInitials,
                    RepId = repId
                };
                _natoliOrderList.Add(nolf);
            }

            _.Dispose();
            _natbcContext.Dispose();
        }
        private void BindNatoliOrderList()
        {
            string searchString = GetSearchString("NatoliOrderList");

            if (searchString.ToLower().StartsWith("rep:"))
            {
                searchString = searchString.Substring(4);
                var _filtered =
                _natoliOrderList.Where(p => p.RepId.ToLower().Trim() == searchString)
                                .OrderBy(kvp => kvp.ShipDate)
                                .ToList();
            }
            else
            {
                var _filtered =
                _natoliOrderList.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                            p.Customer.ToLower().Contains(searchString))
                                .OrderBy(kvp => kvp.ShipDate)
                                .ToList();
            }

            NatoliOrderList = _natoliOrderList;
        }
        #endregion
        #region Module Search Box Text Changed Events
        private string GetSearchString(string moduleName)
        {
            int i = User.VisiblePanels.IndexOf(moduleName);
            if(i<0)
            {
                return "";
            }
            else
            {

                var _textBox = (VisualTreeHelper.GetChild((MainWrapPanel.Children[i] as Grid).Children[0] as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<DockPanel>().Last().Children.OfType<TextBox>().First();
                //var x = VisualTreeHelper.GetChild(_textBox as DependencyObject, 0); //_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox;
                var text = _textBox == null ? new TextBox { Text = "" } : _textBox.Template.FindName("SearchTextBox", _textBox) as TextBox;
                string _text = text == null ? "" : (text.Text ?? "");
                //return _textBox.Text.ToLower(); //x.Text.ToLower();
                return _text.ToLower();
            }
        }
        public void TextChanged(string module)
        {
            switch (module)
            {
                case "BeingEntered":
                    OrdersBeingEnteredSearchBox_TextChanged();
                    break;
                case "InTheOffice":
                    OrdersInTheOfficeSearchBox_TextChanged();
                    break;
                case "QuotesNotConverted":
                    QuotesNotConvertedSearchBox_TextChanged();
                    break;
                case "EnteredUnscanned":
                    OrdersEnteredUnscannedSearchBox_TextChanged();
                    break;
                case "InEngineering":
                    OrdersInEngineeringUnprintedSearchBox_TextChanged();
                    break;
                case "QuotesToConvert":
                    QuotesToConvertSearchBox_TextChanged();
                    break;
                case "ReadyToPrint":
                    OrdersReadyToPrintSearchBox_TextChanged();
                    break;
                case "PrintedInEngineering":
                    OrdersPrintedInEngineeringSearchBox_TextChanged();
                    break;
                case "AllTabletProjects":
                    AllTabletProjectsSearchBox_TextChanged();
                    break;
                //case ("TabletProjectsNotStarted", "Main"):
                //    break;
                //case ("TabletProjectsStarted", "Main"):
                //    break;
                //case ("TabletProjectsDrawn", "Main"):
                //    Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
                //    break;
                //case ("TabletProjectsSubmitted", "Main"):
                //    Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
                //    break;
                //case ("TabletProjectsOnHold", "Main"):
                //    Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
                //    break;
                case "AllToolProjects":
                    AllToolProjectsSearchBox_TextChanged();
                    break;
                //case ("ToolProjectsNotStarted", "Main"):
                //    Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
                //    break;
                //case ("ToolProjectsStarted", "Main"):
                //    Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
                //    break;
                //case ("ToolProjectsDrawn", "Main"):
                //    Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
                //    break;
                //case ("ToolProjectsOnHold", "Main"):
                //    Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
                //    break;
                case "DriveWorksQueue":
                    DriveWorksQueueSearchBox_TextChanged();
                    break;
                case "NatoliOrderList":
                    NatoliOrderListSearchBox_TextChanged();
                    break;
                default:
                    break;
            }
        }
        private void OrdersBeingEnteredSearchBox_TextChanged()
        {
            Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.Invoke(() => BindBeingEntered()), TaskScheduler.Current);
        }
        private void OrdersInTheOfficeSearchBox_TextChanged()
        {
            Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.Invoke(() => BindInTheOffice()), TaskScheduler.Current);
        }
        private void QuotesNotConvertedSearchBox_TextChanged()
        {
            Task.Run(() => GetQuotesNotConverted()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesNotConverted()), TaskScheduler.Current);
        }
        private void OrdersEnteredUnscannedSearchBox_TextChanged()
        {
            Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.Invoke(() => BindEnteredUnscanned()), TaskScheduler.Current);
        }
        private void OrdersInEngineeringUnprintedSearchBox_TextChanged()
        {
            Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindInEngineering()), TaskScheduler.Current);
        }
        private void QuotesToConvertSearchBox_TextChanged()
        {
            Task.Run(() => GetQuotesToConvert()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesToConvert()), TaskScheduler.Current);
        }
        private void OrdersReadyToPrintSearchBox_TextChanged()
        {
            Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.Invoke(() => BindReadyToPrint()), TaskScheduler.Current);
        }
        private void OrdersPrintedInEngineeringSearchBox_TextChanged()
        {
            Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindPrintedInEngineering()), TaskScheduler.Current);
        }
        private void AllTabletProjectsSearchBox_TextChanged()
        {
            Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()), TaskScheduler.Current);
        }
        //private void TabletProjectsNotStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsNotStarted()), TaskScheduler.Current);
        //}
        //private void TabletProjectsStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsStarted()), TaskScheduler.Current);
        //}
        //private void TabletProjectsDrawnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
        //}
        //private void TabletProjectsSubmittedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
        //}
        //private void TabletProjectsOnHoldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
        //}
        private void AllToolProjectsSearchBox_TextChanged()
        {
            Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()), TaskScheduler.Current);
        }
        //private void ToolProjectsNotStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
        //}
        //private void ToolProjectsStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
        //}
        //private void ToolProjectsDrawnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
        //}
        //private void ToolProjectsOnHoldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
        //}
        private void DriveWorksQueueSearchBox_TextChanged()
        {
            Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()), TaskScheduler.Current);
        }
        private void NatoliOrderListSearchBox_TextChanged()
        {
            Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()), TaskScheduler.Current);
        }
        #endregion
        #endregion

        //#region ModuleBuilding

        //#region Panel Construction
        //private void ConstructModules()
        //{
        //    int panel_count = User.VisiblePanels.Count;
        //    ColumnDefinition colDef;
        //    RowDefinition rowDef;
        //    int colCount = 0;
        //    int rowCount = 0;

        //    if (User.VisiblePanels.Count == 1)
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }
        //    else if (User.VisiblePanels.Count == 2)
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }
        //    else if (User.VisiblePanels.Count == 3)
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }
        //    else if (User.VisiblePanels.Count == 4)
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }
        //    else if (User.VisiblePanels.Count == 5)
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }
        //    else if (User.VisiblePanels.Count == 6)
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }
        //    else
        //    {
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        MainGrid.ColumnDefinitions.Add(colDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //        rowDef = new RowDefinition();
        //        rowDef.Height = new GridLength(1, GridUnitType.Star);
        //        MainGrid.RowDefinitions.Add(rowDef);
        //    }

        //    int i = 0;
        //    colCount = MainGrid.ColumnDefinitions.Count;
        //    int j = 0;
        //    rowCount = MainGrid.RowDefinitions.Count;
        //    int k = 0;
        //    for (j = 0; j < colCount && k < User.VisiblePanels.Count; j++)
        //    {
        //        for (i = 0; i < rowCount && k < User.VisiblePanels.Count; i++)
        //        {
        //            Border border = new Border();
        //            //ContentControl mainContentControl = new ContentControl();
        //            //mainContentControl.Style = App.Current.Resources["Orders" + User.VisiblePanels[k] + "Grid"] as Style;
        //            if (User.VisiblePanels.Count == 5 && k == 4)
        //            {
        //                border = ConstructBorder();
        //                Grid.SetRow(border, i);
        //                Grid.SetColumn(border, j);
        //                border.SetValue(Grid.RowSpanProperty, 2);
        //            }
        //            else
        //            {
        //                border = ConstructBorder();
        //                Grid.SetRow(border, i);
        //                Grid.SetColumn(border, j);
        //            }
        //            try
        //            {
        //                MainGrid.Children.Add(border);
        //                // MainGrid.Children.Add(mainContentControl);
        //            }
        //            catch (Exception ex)
        //            {
        //                MessageBox.Show(ex.Message);
        //            }
        //            k++;
        //        }
        //    }
        //}

        //private Border ConstructBorder()
        //{
        //    Border border = new Border()
        //    {
        //        Name = "Border_" + MainGrid.Children.Count.ToString(),
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        BorderThickness = new Thickness(1)
        //    };

        //    // Count label
        //    Label countLabel = new Label()
        //    {
        //        Name = "TotalLabel",
        //        Content = "Total: ", // + interiorStackPanel.Children.Count,
        //        HorizontalContentAlignment = HorizontalAlignment.Right,
        //        VerticalAlignment = VerticalAlignment.Bottom,
        //        BorderThickness = new Thickness(0, 1, 0, 0),
        //        Height = 20,
        //        Padding = new Thickness(0, 0, 5, 0),
        //        Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFF0F0F0")),
        //        BorderBrush = new SolidColorBrush(SystemColors.ControlDarkDarkColor)
        //    };

        //    DockPanel dockPanel = ConstructDockPanel("DockPanel_" + MainGrid.Children.Count.ToString(),
        //                                             CreateHeaderGrid("A_" + MainGrid.Children.Count.ToString() + "_", "Test"),
        //                                             ConstructScrollViewer(),
        //                                             countLabel);
        //    border.Child = dockPanel;

        //    return border;
        //}

        //private static DockPanel ConstructDockPanel(string name, Grid headerGrid, ScrollViewer scrollViewer, Label countLabel)
        //{
        //    DockPanel dockPanel = new DockPanel()
        //    {
        //        Name = name,
        //        LastChildFill = true
        //    };

        //    DockPanel.SetDock(headerGrid, Dock.Top);
        //    dockPanel.Children.Add(headerGrid);

        //    DockPanel.SetDock(countLabel, Dock.Bottom);
        //    dockPanel.Children.Add(countLabel);

        //    dockPanel.Children.Add(scrollViewer);
        //    Image image = new Image()
        //    {
        //        VerticalAlignment = VerticalAlignment.Center,
        //        HorizontalAlignment = HorizontalAlignment.Center,
        //        MaxWidth = 200,
        //        MinWidth = 100
        //    };
        //    var bitImage = new BitmapImage();
        //    bitImage.BeginInit();
        //    bitImage.UriSource = new Uri("NATOLI_ANIMATION.gif", UriKind.Relative);
        //    bitImage.EndInit();
        //    //AnimationBehavior.SetSourceUri(image, new Uri("NATOLI_ANIMATION.gif", UriKind.Relative));
        //    ImageBehavior.SetAnimatedSource(image, bitImage);
        //    dockPanel.Children.Add(image);

        //    return dockPanel;
        //}

        //private Grid CreateHeaderGrid(string name, string title)
        //{
        //    Grid headerLabelGrid = new Grid()
        //    {
        //        Name = name + "HeaderLabelGrid",
        //        Height = 31,
        //        Background = new SolidColorBrush(SystemColors.GradientActiveCaptionColor),
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    TextBox daysTextBox = new TextBox()
        //    {
        //        Name = name + "DateTextBox",
        //        VerticalAlignment = VerticalAlignment.Center,
        //        VerticalContentAlignment = VerticalAlignment.Center,
        //        Margin = new Thickness(1, 2, 0, 2),
        //        Text = User.QuoteDays.ToString(),
        //        Visibility = Visibility.Collapsed
        //    };
        //    daysTextBox.TextChanged += DaysTextBox_TextChanged;
        //    daysTextBox.PreviewKeyUp += DaysTextBox_PreviewKeyUp;

        //    Button csvCreationButton = new Button()
        //    {
        //        Name = name + "CSVButton",
        //        Content = "Export to CSV",
        //        VerticalAlignment = VerticalAlignment.Center,
        //        VerticalContentAlignment = VerticalAlignment.Center,
        //        Margin = new Thickness(2, 0, 0, 0),
        //        Visibility = Visibility.Collapsed,
        //        Style = App.Current.Resources["Button"] as Style
        //    };
        //    csvCreationButton.Click += CsvCreationButton_Click;

        //    if (User.VisiblePanels[int.Parse(name.Substring(2, 1))].ToLower().Contains("notconverted"))
        //    {
        //        daysTextBox.Visibility = Visibility.Visible;
        //        csvCreationButton.Visibility = Visibility.Visible;
        //    }

        //    if (User.VisiblePanels[int.Parse(name.Substring(2, 1))].ToLower() == "natoliorderlist")
        //    {
        //        csvCreationButton.Visibility = Visibility.Visible;
        //    }

        //    // Header label
        //    Label headerLabel = new Label()
        //    {
        //        Name = name + "Label",
        //        Content = title,
        //        HorizontalContentAlignment = HorizontalAlignment.Center,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        BorderThickness = new Thickness(0, 3, 0, 1),
        //        Height = 31,
        //        FontWeight = FontWeights.Bold,
        //        FontSize = 14,
        //        Padding = new Thickness(0),
        //        Background = new SolidColorBrush(SystemColors.GradientActiveCaptionColor),
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    // Header search filter
        //    TextBox searchBox = new TextBox()
        //    {
        //        Name = name + "SearchBox",
        //        Style = App.Current.Resources["SearchBox"] as Style
        //    };
        //    //searchBox.PreviewKeyUp += SearchBox_PreviewKeyUp;
        //    //searchBox.TextChanged += SearchBox_TextChanged;

        //    Grid.SetColumn(daysTextBox, 0);
        //    Grid.SetColumn(csvCreationButton, 1);
        //    Grid.SetColumn(headerLabel, 0);
        //    Grid.SetColumnSpan(headerLabel, 4);
        //    Grid.SetColumn(searchBox, 3);
        //    AddColumn(headerLabelGrid, CreateColumnDefinition(new GridLength(30)));
        //    AddColumn(headerLabelGrid, CreateColumnDefinition(new GridLength(120)));
        //    AddColumn(headerLabelGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), headerLabel);
        //    headerLabelGrid.Children.Add(daysTextBox);
        //    headerLabelGrid.Children.Add(csvCreationButton);
        //    AddColumn(headerLabelGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Auto)), searchBox);

        //    return headerLabelGrid;
        //}

        //private void CsvCreationButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (((sender as Button).Parent as Grid).Children.OfType<Label>().First().Content.ToString().Contains("Quote"))
        //    {
        //        string filePath = @"C:\Users\" + User.DomainName + @"\Desktop\QuoteList.csv";
        //        using var stream = new System.IO.StreamWriter(filePath, false);

        //        // Quote Number, Rev Number, Customer Name, Quote Date
        //        // Get info from currently filtered list in QuotesNotConverted
        //        var expanders = ((((sender as Button).Parent as Grid).Parent as DockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel).Children;

        //        // Write headers
        //        stream.Write("Sales Rep ID,Quote Number,Rev Number,Customer Name,Quote Date\n");

        //        foreach (Expander expander in expanders)
        //        {
        //            int quoteNumber = int.Parse((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString());
        //            short revNumber = short.Parse((expander.Header as Grid).Children[1].GetValue(ContentProperty).ToString());
        //            string customerName = (expander.Header as Grid).Children[2].GetValue(ContentProperty).ToString().Replace(',', '\0');
        //            using var _ = new NAT01Context();
        //            string acctNo = _.QuoteHeader.Single(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == revNumber).UserAcctNo;
        //            using var __ = new NECContext();
        //            string repId = __.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Slprsnid;
        //            __.Dispose();
        //            DateTime quoteDate = _.QuoteHeader.Single(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == revNumber).QuoteDate;
        //            _.Dispose();
        //            stream.Write("{0},{1},{2},{3},{4}\n", repId, quoteNumber, revNumber, customerName, quoteDate.ToShortDateString());
        //        }

        //        stream.Flush();
        //        stream.Dispose();
        //    }
        //    else
        //    {
        //        string filePath = @"C:\Users\" + User.DomainName + @"\Desktop\OrderList.csv";
        //        using var stream = new System.IO.StreamWriter(filePath, false);

        //        // Order Number, Quote Number, Rev Number, Customer Name, Order Date, Ship Date, PO Number
        //        // Get info from currently filtered list in NatoliOrderList
        //        var expanders = ((((sender as Button).Parent as Grid).Parent as DockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel).Children;

        //        // Write headers
        //        stream.Write("Order Number,Quote Number,Rev Number,Customer Name,Order Date,Ship Date,PO Number\n");

        //        foreach (Expander expander in expanders)
        //        {
        //            int orderNumber = int.Parse((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString());
        //            using var _ = new NAT01Context();
        //            OrderHeader orderHeader = _.OrderHeader.Single(q => q.OrderNo == orderNumber * 100);
        //            _.Dispose();
        //            double? quoteNumber = orderHeader.QuoteNumber;
        //            short? revNumber = orderHeader.QuoteRevNo;
        //            string customerName = (expander.Header as Grid).Children[1].GetValue(ContentProperty).ToString().Replace(',', '\0');
        //            DateTime orderDate = (DateTime)orderHeader.OrderDate;
        //            string shipDate = (expander.Header as Grid).Children[2].GetValue(ContentProperty).ToString();
        //            string poNumber = orderHeader.Ponumber.Trim() + (string.IsNullOrEmpty(orderHeader.Poextension.Trim()) ? "" : '-' + orderHeader.Poextension);
        //            stream.Write("{0},{1},{2},{3},{4},{5},{6}\n", orderNumber, quoteNumber, revNumber, customerName, orderDate.ToShortDateString(), shipDate, poNumber);
        //        }

        //        stream.Flush();
        //        stream.Dispose();
        //    }

        //    MessageBox.Show("Your file is ready.");
        //}

        //private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
        //    TextBlock textBlock = (sender as TextBox).Template.FindName("SearchTextBlock", sender as TextBox) as TextBlock;
        //    Image image = (sender as TextBox).Template.FindName("MagImage", (sender as TextBox)) as Image;

        //    if (textBox.Text.Length > 0)
        //    {
        //        image.Source = ((DrawingImage)App.Current.Resources["closeDrawingImage"]);
        //        image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
        //        textBlock.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        image.Source = ((DrawingImage)App.Current.Resources["searchDrawingImage"]);
        //        textBlock.Visibility = Visibility.Visible;
        //    }
        //}
        //private void DaysTextBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    TextBox textBox = sender as TextBox;
        //    using var _ = new NAT02Context();
        //    try
        //    {
        //        EoiSettings eoiSettings = _.EoiSettings.Single(s => s.EmployeeId == User.EmployeeCode);
        //        eoiSettings.QuoteDays = short.Parse(textBox.Text);
        //        _.EoiSettings.Update(eoiSettings);
        //        User.QuoteDays = eoiSettings.QuoteDays;
        //    }
        //    catch
        //    {

        //    }
        //    finally
        //    {
        //        _.SaveChanges();
        //        _.Dispose();
        //    }
        //}
        //private void DaysTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    MainRefresh();
        //}

        //private void SearchBox_PreviewKeyUp(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Escape)
        //    {
        //        TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
        //        textBox.Text = "";
        //    }
        //}

        //private void Image_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    Image image = sender as Image;
        //    TextBox textBox = (image.Parent as Grid).Children.OfType<TextBox>().First();
        //    textBox.Text = "";
        //}

        //private ScrollViewer ConstructScrollViewer()
        //{
        //    ScrollViewer scrollViewer = new ScrollViewer()
        //    {
        //        Style = FindResource("ScrollViewerStyle") as Style,
        //        Visibility = Visibility.Collapsed
        //    };

        //    StackPanel stackPanel = new StackPanel();
        //    scrollViewer.Content = stackPanel;

        //    return scrollViewer;
        //}

        //// Build expanders
        //private void ConstructExpanders()
        //{
        //    for (int i = 0; i < User.VisiblePanels.Count; i++)
        //    {
        //        DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel;

        //        Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();

        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //        string panel = User.VisiblePanels[i];

        //        BuildPanel(dockPanel, sp, panel);

        //        Label countLabel = dockPanel.Children.OfType<Label>().First();

        //        countLabel.Content = "Total: " + sp.Children.OfType<Expander>().Count();
        //    }
        //}

        //private void BuildPanel(DockPanel dockPanel, StackPanel sp, string panel)
        //{
        //    Border headerBorder = new Border()
        //    {
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        BorderThickness = new Thickness(0, 1, 0, 1)
        //    };

        //    Grid headerGrid = new Grid()
        //    {
        //        Background = MainMenu.Background,
        //        Height = 30,
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    // Check all checkbox
        //    CheckBox checkBox = new CheckBox()
        //    {
        //        IsChecked = false,
        //        VerticalAlignment = VerticalAlignment.Center,
        //        Margin = new Thickness(20, 0, 0, 0),
        //        LayoutTransform = new ScaleTransform(0.65, 0.65)
        //    };
        //    checkBox.Checked += CheckBox_Checked;
        //    checkBox.Unchecked += CheckBox_Checked;

        //    switch (panel)
        //    {
        //        case "BeingEntered":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(65)), CreateLabel("Quote No", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(30)), CreateLabel("Rev", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Ships", 0, 5, FontWeights.Normal));
        //            // if (ordersBeingEnteredDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += OrdersBeingEnteredSearchBox_TextChanged;

        //            break;
        //        case "InTheOffice":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(45)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("In Office", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(100)), CreateLabel("Employee Name", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("CSR", 0, 6, FontWeights.Normal));
        //            // if (ordersInTheOfficeDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += OrdersInTheOfficeSearchBox_TextChanged;

        //            break;
        //        case "QuotesNotConverted":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(65)), CreateLabel("Quote No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Rev No", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
        //            //if (quotesNotConvertedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += QuotesNotConvertedSearchBox_TextChanged;

        //            break;
        //        case "EnteredUnscanned":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Days In", 0, 4, FontWeights.Normal));
        //            // if (ordersEnteredUnscannedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += OrdersEnteredUnscannedSearchBox_TextChanged;

        //            break;
        //        case "InEngineering":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(45)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("In Eng", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(125)), CreateLabel("Employee Name", 0, 5, FontWeights.Normal));
        //            // if (ordersInEngineeringUnprintedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += OrdersInEngineeringUnprintedSearchBox_TextChanged;

        //            break;
        //        case "QuotesToConvert":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(65)), CreateLabel("Quote No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Rev No", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(148)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(50)), CreateLabel("Days In", 0, 5, FontWeights.Normal));
        //            // if (quotesToConvertDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += QuotesToConvertSearchBox_TextChanged;

        //            break;
        //        case "ReadyToPrint":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(110)), CreateLabel("Checker", 0, 5, FontWeights.Normal));
        //            // if (ordersReadyToPrintDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += OrdersReadyToPrintSearchBox_TextChanged;

        //            break;
        //        case "PrintedInEngineering":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order No", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(45)), CreateLabel("Ships", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Employee Name", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Checker", 0, 5, FontWeights.Normal));
        //            // if (ordersPrintedInEngineeringDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += OrdersPrintedInEngineeringSearchBox_TextChanged;

        //            break;
        //        case "AllTabletProjects":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (allTabletProjectsDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += AllTabletProjectsSearchBox_TextChanged;

        //            break;
        //        case "TabletProjectsNotStarted":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 5, FontWeights.Normal));
        //            // if (tabletProjectsNotStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += TabletProjectsNotStartedSearchBox_TextChanged;

        //            break;
        //        case "TabletProjectsStarted":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (tabletProjectsStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += TabletProjectsStartedSearchBox_TextChanged;

        //            break;
        //        case "TabletProjectsDrawn":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (tabletProjectsDrawnDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += TabletProjectsDrawnSearchBox_TextChanged;

        //            break;
        //        case "TabletProjectsSubmitted":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (tabletProjectsSubmittedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += TabletProjectsSubmittedSearchBox_TextChanged;

        //            break;
        //        case "TabletProjectsOnHold":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (tabletProjectsOnHoldDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += TabletProjectsOnHoldSearchBox_TextChanged;

        //            break;
        //        case "AllToolProjects":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (allToolProjectsDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += AllToolProjectsSearchBox_TextChanged;

        //            break;
        //        case "ToolProjectsNotStarted":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 5, FontWeights.Normal));
        //            // if (toolProjectsNotStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += ToolProjectsNotStartedSearchBox_TextChanged;

        //            break;
        //        case "ToolProjectsStarted":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (toolProjectsStartedDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += ToolProjectsStartedSearchBox_TextChanged;

        //            break;
        //        case "ToolProjectsDrawn":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (toolProjectsDrawnDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += ToolProjectsDrawnSearchBox_TextChanged;

        //            break;
        //        case "ToolProjectsOnHold":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("Proj #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rev", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer Name", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("CSR", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(70)), CreateLabel("Drafter", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Due Date", 0, 6, FontWeights.Normal));
        //            // if (toolProjectsOnHoldDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += ToolProjectsOnHoldSearchBox_TextChanged;

        //            break;
        //        case "DriveWorksQueue":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Model Name", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(170)), CreateLabel("Released By", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel("Tag", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(85)), CreateLabel("Release Time", 0, 4, FontWeights.Normal));
        //            // if (driveWorksQueueDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += DriveWorksQueueSearchBox_TextChanged;

        //            break;
        //        case "NatoliOrderList":
        //            // AddColumn(headerGrid, CreateColumnDefinition(new GridLength(18))); // Blank space to account for expander arrow
        //            Grid.SetColumn(checkBox, 0);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(36)), checkBox);
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(60)), CreateLabel("Order #", 0, 1, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Customer", 0, 2, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(80)), CreateLabel("Ship Date", 0, 3, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rush", 0, 4, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(55)), CreateLabel("On Hold", 0, 5, FontWeights.Normal));
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(40)), CreateLabel("Rep", 0, 6, FontWeights.Normal));
        //            // if (natoliOrderListDict.Keys.Count > 16) { AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22))); } // Blank space to account for scrollbar

        //            headerBorder.Child = headerGrid;
        //            DockPanel.SetDock(headerBorder, Dock.Top);
        //            dockPanel.Children.Insert(1, headerBorder);

        //            dockPanel.Children.OfType<Grid>().First().Children.OfType<TextBox>().Single(t => t.Name.Contains("Search")).TextChanged += NatoliOrderListSearchBox_TextChanged;

        //            break;
        //        default:
        //            break;
        //    }
        //}

        //private void CheckBox_Checked(object sender, RoutedEventArgs e)
        //{
        //    CheckBox checkBox = sender as CheckBox;
        //    var expanders = ((((checkBox.Parent as Grid).Parent as Border).Parent as DockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.OfType<Expander>();
        //    foreach (Expander expander in expanders)
        //    {
        //        var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
        //        ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = checkBox.IsChecked;
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="width"></param>
        ///// <returns></returns>
        //private ColumnDefinition CreateColumnDefinition(GridLength width)
        //{
        //    ColumnDefinition colDef = new ColumnDefinition();
        //    try
        //    {
        //        colDef.Width = width;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    return colDef;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="content"></param>
        ///// <param name="row"></param>
        ///// <param name="column"></param>
        ///// <returns></returns>
        //private Label CreateLabel(string content, int row, int column, FontWeight weight, SolidColorBrush textColor = null, FontStyle? fontStyle = null, double fontSize = 12, bool addPadding = false)
        //{
        //    Label label = new Label();
        //    try
        //    {
        //        label.Content = content;
        //        label.HorizontalAlignment = HorizontalAlignment.Stretch;
        //        label.HorizontalContentAlignment = HorizontalAlignment.Left;
        //        label.VerticalAlignment = VerticalAlignment.Top;
        //        label.VerticalContentAlignment = VerticalAlignment.Center;
        //        label.FontSize = fontSize;
        //        if (addPadding) { label.Padding = new Thickness(0, 0, 0, 0); }
        //        if (!(textColor is null))
        //        {
        //            label.Foreground = textColor;
        //        }
        //        label.FontWeight = weight;
        //        label.FontStyle = !(fontStyle is null) ? (FontStyle)fontStyle : FontStyles.Normal;
        //        Grid.SetRow(label, row);
        //        Grid.SetColumn(label, column);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //    return label;
        //}

        //private CheckBox CreateCheckBox(int row, int column, bool isChecked)
        //{
        //    CheckBox checkBox = new CheckBox();
        //    try
        //    {
        //        checkBox.IsChecked = isChecked;
        //        checkBox.VerticalAlignment = VerticalAlignment.Center;
        //        checkBox.LayoutTransform = new ScaleTransform(0.7, 0.7, 0, 0);
        //        checkBox.Checked += LineItemCheckBox_Checked;
        //        checkBox.Unchecked += LineItemCheckBox_Unchecked;
        //        Grid.SetRow(checkBox, row);
        //        Grid.SetColumn(checkBox, column);
        //    }
        //    catch
        //    {
        //        return null;
        //    }
        //    return checkBox;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="grid"></param>
        ///// <param name="columnDefinition"></param>
        ///// <param name="label"></param>
        //private static void AddColumn(Grid grid, ColumnDefinition columnDefinition = null, UIElement element = null)
        //{
        //    try
        //    {
        //        grid.ColumnDefinitions.Add(columnDefinition);
        //        if (!(element is null)) { grid.Children.Add(element); };
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        //#endregion

        ////Handle refreshes
        //private void BindData(string timer)
        //{
        //    try
        //    {
        //        foreach (string panel in User.VisiblePanels)
        //        {
        //            switch (panel, timer)
        //            {
        //                case ("BeingEntered", "Main"):
        //                    Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.Invoke(() => BindBeingEntered()), TaskScheduler.Current);
        //                    break;
        //                case ("InTheOffice", "Main"):
        //                    Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.Invoke(() => BindInTheOffice()), TaskScheduler.Current);
        //                    break;
        //                case ("QuotesNotConverted", "QuotesNotConverted"):
        //                    Task.Run(() => GetQuotesNotConverted()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesNotConverted()), TaskScheduler.Current);
        //                    break;
        //                case ("EnteredUnscanned", "Main"):
        //                    Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.Invoke(() => BindEnteredUnscanned()), TaskScheduler.Current);
        //                    break;
        //                case ("InEngineering", "Main"):
        //                    Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindInEngineering()), TaskScheduler.Current);
        //                    break;
        //                case ("QuotesToConvert", "Main"):
        //                    Task.Run(() => GetQuotesToConvert()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesToConvert()), TaskScheduler.Current);
        //                    break;
        //                case ("ReadyToPrint", "Main"):
        //                    Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.Invoke(() => BindReadyToPrint()), TaskScheduler.Current);
        //                    break;
        //                case ("PrintedInEngineering", "Main"):
        //                    Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindPrintedInEngineering()), TaskScheduler.Current);
        //                    break;
        //                case ("AllTabletProjects", "Main"):
        //                    Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()), TaskScheduler.Current);
        //                    break;
        //                case ("TabletProjectsNotStarted", "Main"):
        //                    Task.Run(() => GetTabletProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsNotStarted()), TaskScheduler.Current);
        //                    break;
        //                case ("TabletProjectsStarted", "Main"):
        //                    Task.Run(() => GetTabletProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsStarted()), TaskScheduler.Current);
        //                    break;
        //                case ("TabletProjectsDrawn", "Main"):
        //                    Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
        //                    break;
        //                case ("TabletProjectsSubmitted", "Main"):
        //                    Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
        //                    break;
        //                case ("TabletProjectsOnHold", "Main"):
        //                    Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
        //                    break;
        //                case ("AllToolProjects", "Main"):
        //                    Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()), TaskScheduler.Current);
        //                    break;
        //                case ("ToolProjectsNotStarted", "Main"):
        //                    Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
        //                    break;
        //                case ("ToolProjectsStarted", "Main"):
        //                    Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
        //                    break;
        //                case ("ToolProjectsDrawn", "Main"):
        //                    Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
        //                    break;
        //                case ("ToolProjectsOnHold", "Main"):
        //                    Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
        //                    break;
        //                case ("DriveWorksQueue", "Main"):
        //                    Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()), TaskScheduler.Current);
        //                    break;
        //                case ("NatoliOrderList", "NatoliOrderList"):
        //                    Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()), TaskScheduler.Current);
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}
        //private static int GetNumberOfDays(string csr)
        //{
        //    switch (csr)
        //    {
        //        case "Alex Heimberger":
        //            return 14;
        //        case "Anna King":
        //            return 7;
        //        case "Bryan Foy":
        //            return 7;
        //        case "David Nelson":
        //            return 7;
        //        case "Gregory Lyle":
        //            return 14;
        //        case "Heather Lane":
        //            return 7;
        //        case "Humberto Zamora":
        //            return 14;
        //        case "James Willis":
        //            return 14;
        //        case "Miral Bouzitoun":
        //            return 14;
        //        case "Nicholas Tarte":
        //            return 14;
        //        case "Samantha Bowman":
        //            return 7;
        //        case "Tiffany Simonpietri":
        //            return 7;
        //        default:
        //            return 14;
        //    }
        //}

        //private static void OpenOrder_Click(object sender, RoutedEventArgs e)
        //{

        //}

        //#region GetsAndBinds
        //private void GetBeingEntered()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiOrdersBeingEnteredView> eoiOrdersBeingEnteredView = _nat02context.EoiOrdersBeingEnteredView.OrderBy(o => o.OrderNo).ToList();

        //        ordersBeingEnteredDict = new Dictionary<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)>();

        //        foreach (EoiOrdersBeingEnteredView order in eoiOrdersBeingEnteredView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else
        //            {
        //                if (_nat02context.EoiOrdersBeingChecked.Where(o => o.OrderNo == order.OrderNo).Any())
        //                {
        //                    back = new SolidColorBrush(Colors.DodgerBlue);
        //                }
        //                else
        //                {
        //                    back = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFFFF");
        //                }
        //            }
        //            ordersBeingEnteredDict.Add(order.OrderNo, ((double)order.QuoteNo, (short)order.Rev, order.CustomerName, (int)order.NumDaysToShip, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        dictList.Add(ordersBeingEnteredDict);
        //        eoiOrdersBeingEnteredView.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindBeingEntered()
        //{
        //    int i = User.VisiblePanels.IndexOf("BeingEntered");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += OrdersBeingEnteredSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "BeingEntered");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    ordersBeingEnteredDict =
        //        ordersBeingEnteredDict.Where(o => o.Key.ToString().ToLower().Contains(searchString) ||
        //                                          o.Value.quoteNumber.ToString().Contains(searchString) ||
        //                                          o.Value.customerName.ToLower().Contains(searchString))
        //                              .OrderBy(kvp => kvp.Key)
        //                              .ToDictionary(x => x.Key, x => x.Value);

        //    BeingEnteredExpanders(ordersBeingEnteredDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetInTheOffice()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiOrdersInOfficeView> eoiOrdersInOfficeView = new List<EoiOrdersInOfficeView>();
        //        if (User.Department == "Customer Service" && !(User.GetUserName().StartsWith("Tiffany") || User.GetUserName().StartsWith("James W")))
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiOrdersInOfficeView = _nat02context.EoiOrdersInOfficeView.Where(o => o.Csr.StartsWith(usrName)).OrderBy(o => o.NumDaysToShip).ThenBy(o => o.DaysInOffice).ToList();
        //        }
        //        else
        //        {
        //            eoiOrdersInOfficeView = _nat02context.EoiOrdersInOfficeView.OrderBy(o => o.NumDaysToShip).ThenBy(o => o.DaysInOffice).ToList();
        //        }

        //        ordersInTheOfficeDict = new Dictionary<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)>();

        //        foreach (EoiOrdersInOfficeView order in eoiOrdersInOfficeView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            ordersInTheOfficeDict.Add((double)order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, (int)order.DaysInOffice, order.EmployeeName, order.Csr, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }

        //        eoiOrdersInOfficeView.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        //private void BindInTheOffice()
        //{
        //    int i = User.VisiblePanels.IndexOf("InTheOffice");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += OrdersInTheOfficeSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "InTheOffice");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    ordersInTheOfficeDict =
        //        ordersInTheOfficeDict.Where(o => o.Key.ToString().ToLower().Contains(searchString) ||
        //                                         o.Value.customerName.ToString().Contains(searchString) ||
        //                                         o.Value.employeeName.ToLower().Contains(searchString) ||
        //                                         o.Value.csr.ToLower().Contains(searchString))
        //                             .OrderBy(kvp => kvp.Value.daysToShip)
        //                             .ThenBy(kvp => kvp.Value.daysInOffice)
        //                             .ThenBy(kvp => kvp.Key)
        //                             .ToDictionary(x => x.Key, x => x.Value);

        //    InTheOfficeExpanders(ordersInTheOfficeDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetEnteredUnscanned()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiOrdersEnteredAndUnscannedView> eoiOrdersEnteredAndUnscannedView = _nat02context.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();

        //        ordersEnteredUnscannedDict = new Dictionary<double, (string customerName, int daysToShip, int daysIn, string background, string foreground, string fontWeight)>();

        //        foreach (EoiOrdersEnteredAndUnscannedView order in eoiOrdersEnteredAndUnscannedView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            bool doNotProcess = Convert.ToBoolean(order.DoNotProcess);
        //            string[] errRes;
        //            errRes = new string[2] { order.ProcessState,
        //                                 order.TransitionName };

        //            if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            //else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
        //            //{
        //            //    fore = new SolidColorBrush(Colors.White);
        //            //    weight = FontWeights.Normal;
        //            //}
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.DarkGray);
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            ordersEnteredUnscannedDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, (int)order.NumDaysIn, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        eoiOrdersEnteredAndUnscannedView.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindEnteredUnscanned()
        //{
        //    int i = User.VisiblePanels.IndexOf("EnteredUnscanned");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += OrdersEnteredUnscannedSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "EnteredUnscanned");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    ordersEnteredUnscannedDict =
        //        ordersEnteredUnscannedDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                              p.Value.customerName.ToLower().Contains(searchString))
        //                                  .OrderBy(kvp => kvp.Key)
        //                                  .ToDictionary(x => x.Key, x => x.Value);

        //    OrdersEnteredUnscannedExpanders(ordersEnteredUnscannedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetInEngineering()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        using var nat01context = new NAT01Context();
        //        List<EoiOrdersInEngineeringUnprintedView> eoiOrdersInEngineeringUnprintedView = _nat02context.EoiOrdersInEngineeringUnprintedView.OrderByDescending(o => o.DaysInEng).ThenBy(o => o.NumDaysToShip).ToList();

        //        ordersInEngineeringUnprintedDict = new Dictionary<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)>();

        //        foreach (EoiOrdersInEngineeringUnprintedView order in eoiOrdersInEngineeringUnprintedView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else
        //            {
        //                int count = _nat02context.MaMachineVariables.Where(o => o.WorkOrderNumber == order.OrderNo.ToString()).Count();
        //                string machineType = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100).FirstOrDefault().MachinePriceCode.Trim();
        //                short machineNo = (short)nat01context.OrderDetails.First(o => o.OrderNo == order.OrderNo * 100).MachineNo;
        //                string stockSize = machineNo == 0 ? "" : nat01context.MachineList.Single(m => m.MachineNo == machineNo).UpperSize;
        //                bool rework = nat01context.OrderDetails.Any(o => o.Desc1.Contains("<REWORK>"));
        //                var lineType = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100 && (o.DetailTypeId == "U" || o.DetailTypeId == "L" || o.DetailTypeId == "R")).ToList();
        //                if (_nat02context.EoiOrdersBeingChecked.Where(o => o.OrderNo == order.OrderNo).Any())
        //                {
        //                    back = new SolidColorBrush(Colors.DodgerBlue);
        //                }
        //                else if (count == 0 && (machineType == "BB" ||
        //                                       (machineType == "B" && !stockSize.StartsWith("3/4") && !stockSize.StartsWith("1-1/4")) ||
        //                                        machineType == "D" && !stockSize.StartsWith("1-1/2")) && lineType.Count != 0)
        //                {
        //                    back = new SolidColorBrush(Colors.Red);
        //                }
        //                else if (_nat02context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == order.OrderNo).Any())
        //                {
        //                    back = new SolidColorBrush(Colors.GreenYellow);
        //                }
        //                else
        //                {
        //                    back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //                }
        //            }
        //            ordersInEngineeringUnprintedDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, (int)order.DaysInEng, order.EmployeeName, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        eoiOrdersInEngineeringUnprintedView.Clear();
        //        _nat02context.Dispose();
        //        nat01context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindInEngineering()
        //{
        //    int i = User.VisiblePanels.IndexOf("InEngineering");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += OrdersInEngineeringUnprintedSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "InEngineering");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    //ContentControl footerContentControl = (VisualTreeHelper.GetChild((MainGrid.Children[i] as ContentControl) as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                                                              .Children.OfType<ContentControl>().Last();
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    ordersInEngineeringUnprintedDict =
        //        ordersInEngineeringUnprintedDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                                    p.Value.customerName.ToLower().Contains(searchString) ||
        //                                                    p.Value.employeeName.ToLower().Contains(searchString))
        //                                        .OrderByDescending(kvp => kvp.Value.daysInEng)
        //                                        .ThenBy(kvp => kvp.Value.daysToShip)
        //                                        .ThenBy(kvp => kvp.Key)
        //                                        .ToDictionary(x => x.Key, x => x.Value);

        //    OrdersInEngineeringUnprintedExpanders(ordersInEngineeringUnprintedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetReadyToPrint()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        using var nat01context = new NAT01Context();
        //        List<EoiOrdersReadyToPrintView> eoiOrdersReadyToPrintView = _nat02context.EoiOrdersReadyToPrintView.OrderBy(o => o.OrderNo).ToList();

        //        ordersReadyToPrintDict = new Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)>();

        //        foreach (EoiOrdersReadyToPrintView order in eoiOrdersReadyToPrintView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else
        //            {
        //                bool tm2 = Convert.ToBoolean(order.TM2);
        //                bool tabletPrints = Convert.ToBoolean(order.Tablet);
        //                bool toolPrints = Convert.ToBoolean(order.Tool);
        //                List<OrderDetails> orderDetails;
        //                OrderHeader orderHeader;
        //                orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100).ToList();
        //                orderHeader = nat01context.OrderHeader.Single(o => o.OrderNo == order.OrderNo * 100);

        //                if (tm2 || tabletPrints)
        //                {
        //                    foreach (OrderDetails od in orderDetails)
        //                    {
        //                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
        //                        {
        //                            string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + od.HobNoShapeId.Trim() + ".pdf";
        //                            if (!System.IO.File.Exists(path))
        //                            {
        //                                goto Missing;
        //                            }
        //                        }
        //                    }
        //                }

        //                if (tm2 || toolPrints)
        //                {
        //                    foreach (OrderDetails od in orderDetails)
        //                    {
        //                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
        //                        {
        //                            string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
        //                            detailType = detailType == "MISC" ? "REJECT" : detailType;
        //                            string international = orderHeader.UnitOfMeasure;
        //                            string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + detailType + ".pdf";
        //                            if (!System.IO.File.Exists(path))
        //                            {
        //                                goto Missing;
        //                            }
        //                            if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
        //                            {
        //                                goto Missing;
        //                            }
        //                        }
        //                    }
        //                }

        //                goto NotMissing;

        //            Missing:;
        //                if (User.Department == "Engineering")
        //                {
        //                    back = new SolidColorBrush(Colors.MediumPurple);
        //                }
        //                else
        //                {
        //                    back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //                }
        //                goto Finished;

        //            NotMissing:;
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

        //            Finished:;
        //            }
        //            ordersReadyToPrintDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, order.EmployeeName, order.CheckedBy, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        eoiOrdersReadyToPrintView.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindReadyToPrint()
        //{
        //    int i = User.VisiblePanels.IndexOf("ReadyToPrint");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "ReadyToPrint");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    ordersReadyToPrintDict =
        //        ordersReadyToPrintDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                          p.Value.customerName.ToLower().Contains(searchString) ||
        //                                          p.Value.employeeName.ToLower().Contains(searchString) ||
        //                                          p.Value.checkedBy.ToLower().Contains(searchString))
        //                              .OrderBy(kvp => kvp.Key)
        //                              .ToDictionary(x => x.Key, x => x.Value);

        //    OrdersReadyToPrintExpanders(ordersReadyToPrintDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetPrintedInEngineering()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        using var nat01context = new NAT01Context();
        //        List<EoiOrdersPrintedInEngineeringView> eoiOrdersPrintedInEngineeringView = _nat02context.EoiOrdersPrintedInEngineeringView.OrderBy(o => o.OrderNo).ToList();

        //        ordersPrintedInEngineeringDict = new Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)>();

        //        foreach (EoiOrdersPrintedInEngineeringView order in eoiOrdersPrintedInEngineeringView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            if (_nat02context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == order.OrderNo).Any())
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else
        //            {
        //                bool tm2 = Convert.ToBoolean(order.TM2);
        //                bool tabletPrints = Convert.ToBoolean(order.Tablet);
        //                bool toolPrints = Convert.ToBoolean(order.Tool);
        //                List<OrderDetails> orderDetails;
        //                List<OrderHeader> orderHeader;
        //                orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == order.OrderNo * 100).ToList();
        //                orderHeader = nat01context.OrderHeader.Where(o => o.OrderNo == order.OrderNo * 100).ToList();

        //                if (tm2 || tabletPrints)
        //                {
        //                    foreach (OrderDetails od in orderDetails)
        //                    {
        //                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
        //                        {
        //                            string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + od.HobNoShapeId.Trim() + ".pdf";
        //                            if (!System.IO.File.Exists(path))
        //                            {
        //                                goto Missing;
        //                            }
        //                        }
        //                    }
        //                }

        //                if (tm2 || toolPrints)
        //                {
        //                    foreach (OrderDetails od in orderDetails)
        //                    {
        //                        if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
        //                        {
        //                            string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
        //                            detailType = detailType == "MISC" ? "REJECT" : detailType;
        //                            string international = orderHeader.FirstOrDefault().UnitOfMeasure;
        //                            string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNo + @"\" + detailType + ".pdf";
        //                            if (!System.IO.File.Exists(path))
        //                            {
        //                                goto Missing;
        //                            }
        //                            if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
        //                            {
        //                                goto Missing;
        //                            }
        //                        }
        //                    }
        //                }

        //                goto NotMissing;

        //            Missing:;
        //                if (User.Department == "Engineering")
        //                {
        //                    back = new SolidColorBrush(Colors.MediumPurple);
        //                }
        //                else
        //                {
        //                    back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //                }
        //                goto Finished;

        //            NotMissing:;
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

        //            Finished:;
        //            }
        //            ordersPrintedInEngineeringDict.Add(order.OrderNo, (order.CustomerName, (int)order.NumDaysToShip, order.EmployeeName, order.CheckedBy, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        eoiOrdersPrintedInEngineeringView.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindPrintedInEngineering()
        //{
        //    int i = User.VisiblePanels.IndexOf("PrintedInEngineering");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "PrintedInEngineering");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    string column;
        //    if (searchString.Contains(":"))
        //    {
        //        column = searchString.Split(':')[0];
        //        searchString = searchString.Split(':')[1];
        //        switch (column)
        //        {
        //            case "Order No":

        //                ordersPrintedInEngineeringDict =
        //                    ordersPrintedInEngineeringDict.Where(p => p.Key.ToString().ToLower().Contains(searchString))
        //                                                  .OrderBy(kvp => kvp.Key)
        //                                                  .ToDictionary(x => x.Key, x => x.Value);
        //                break;
        //            case "Customer Name":

        //                ordersPrintedInEngineeringDict =
        //                    ordersPrintedInEngineeringDict.Where(p => p.Value.customerName.ToLower().Contains(searchString))
        //                                                  .OrderBy(kvp => kvp.Key)
        //                                                  .ToDictionary(x => x.Key, x => x.Value);
        //                break;
        //            case "Employee Name":

        //                ordersPrintedInEngineeringDict =
        //                    ordersPrintedInEngineeringDict.Where(p => p.Value.employeeName.ToLower().Contains(searchString))
        //                                                  .OrderBy(kvp => kvp.Key)
        //                                                  .ToDictionary(x => x.Key, x => x.Value);
        //                break;
        //            case "Checker":

        //                ordersPrintedInEngineeringDict =
        //                    ordersPrintedInEngineeringDict.Where(p => p.Value.checkedBy.ToLower().Contains(searchString))
        //                                                  .OrderBy(kvp => kvp.Key)
        //                                                  .ToDictionary(x => x.Key, x => x.Value);
        //                break;
        //            default:

        //                ordersPrintedInEngineeringDict =
        //                    ordersPrintedInEngineeringDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                                              p.Value.customerName.ToLower().Contains(searchString) ||
        //                                                              p.Value.employeeName.ToLower().Contains(searchString) ||
        //                                                              p.Value.checkedBy.ToLower().Contains(searchString))
        //                                                  .OrderBy(kvp => kvp.Key)
        //                                                  .ToDictionary(x => x.Key, x => x.Value);
        //                break;
        //        }
        //    }
        //    else
        //    {
        //        ordersPrintedInEngineeringDict =
        //            ordersPrintedInEngineeringDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                                      p.Value.customerName.ToLower().Contains(searchString) ||
        //                                                      p.Value.employeeName.ToLower().Contains(searchString) ||
        //                                                      p.Value.checkedBy.ToLower().Contains(searchString))
        //                                          .OrderBy(kvp => kvp.Key)
        //                                          .ToDictionary(x => x.Key, x => x.Value);
        //    }

        //    OrdersPrintedInEngineeringExpanders(ordersPrintedInEngineeringDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetQuotesNotConverted()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
        //                                                 .Select(e => e.Subscribed);
        //        string[] subs = subList.First().Split(',');
        //        quotesCompletedChanged = (quotesCompletedCount != _nat02context.EoiQuotesOneWeekCompleted.Count());
        //        quotesCompletedCount = _nat02context.EoiQuotesOneWeekCompleted.Count();
        //        short quoteDays = User.QuoteDays;
        //        List<EoiQuotesNotConvertedView> _eoiQuotesNotConvertedView = new List<EoiQuotesNotConvertedView>();
        //        foreach (string sub in subs)
        //        {
        //            string s = sub;
        //            if (sub == "Nicholas")
        //            {
        //                s = "Nick";
        //            }
        //            _eoiQuotesNotConvertedView.AddRange(_nat02context.EoiQuotesNotConvertedView.Where(q => q.Csr.Contains(s) && q.QuoteDate >= DateTime.Now.AddDays(-quoteDays)).ToList());
        //        }
        //        List<EoiQuotesNotConvertedView> eoiQuotesNotConvertedView = _eoiQuotesNotConvertedView.Where(q => q.QuoteDate >= DateTime.Now.AddDays(-quoteDays)).OrderByDescending(q => q.QuoteNo).ThenByDescending(q => q.QuoteRevNo).ToList();

        //        quotesNotConvertedDict = new Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, string repId, string background, string foreground, string fontWeight)>();

        //        foreach (EoiQuotesNotConvertedView quote in eoiQuotesNotConvertedView)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (quote.RushYorN == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            using var _ = new NAT01Context();
        //            string acctNo = _.QuoteHeader.Single(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).UserAcctNo;
        //            _.Dispose();
        //            using var __ = new NECContext();
        //            string repId = __.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Slprsnid;
        //            __.Dispose();

        //            int days = GetNumberOfDays(quote.Csr);

        //            bool needs_followup_4 = DateTime.Today.Subtract(_nat02context.EoiQuotesNotConvertedView.First(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).QuoteDate).Days > 28 &&
        //                                    GetNumberOfDays(quote.Csr) == 14;
        //            bool needs_followup = !_nat02context.EoiQuotesOneWeekCompleted.Where(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).Any() &&
        //                                  DateTime.Today.Subtract(_nat02context.EoiQuotesNotConvertedView.First(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).QuoteDate).Days > days &&
        //                                  !needs_followup_4;

        //            if (needs_followup)
        //            {
        //                back = new SolidColorBrush(Colors.Pink);
        //            }
        //            else if (needs_followup_4)
        //            {
        //                back = new SolidColorBrush(Colors.OrangeRed);
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }

        //            ExpanderAttributes expanderAttributes = new ExpanderAttributes(back, fore, weight);
        //            quotesNotConvertedDict.Add((quote.QuoteNo, quote.QuoteRevNo), (quote.CustomerName, quote.Csr, repId, back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        eoiQuotesNotConvertedView.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindQuotesNotConverted()
        //{
        //    int i = User.VisiblePanels.IndexOf("QuotesNotConverted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Visible;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Visible;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += QuotesNotConvertedSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "QuotesNotConverted");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    if (searchString.ToLower().StartsWith("rep:"))
        //    {
        //        searchString = searchString.Substring(4);
        //        var _filtered =
        //        quotesNotConvertedDict.Where(p => p.Value.repId.ToLower().Trim() == searchString)
        //                              .OrderByDescending(kvp => kvp.Key.quoteNumber)
        //                              .ToDictionary(x => x.Key, x => x.Value);

        //        // Remove/Add expanders based on filtering
        //        QuotesNotConvertedExpanders(_filtered);
        //    }
        //    else
        //    {
        //        var _filtered =
        //        quotesNotConvertedDict.Where(p => p.Key.quoteNumber.ToString().ToLower().Contains(searchString) ||
        //                                          p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                          p.Value.customerName.ToLower().Contains(searchString) ||
        //                                          p.Value.csr.ToLower().Contains(searchString))
        //                              .OrderByDescending(kvp => kvp.Key.quoteNumber)
        //                              .ToDictionary(x => x.Key, x => x.Value);

        //        // Remove/Add expanders based on filtering
        //        QuotesNotConvertedExpanders(_filtered);
        //    }

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetQuotesToConvert()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiQuotesMarkedForConversionView> eoiQuotesMarkedForConversion = new List<EoiQuotesMarkedForConversionView>();

        //        IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
        //                                                .Select(e => e.Subscribed);
        //        string[] subs = subList.First().Split(',');
        //        List<EoiQuotesMarkedForConversionView> _eoiQuotesMarkedForConversion = new List<EoiQuotesMarkedForConversionView>();
        //        foreach (string sub in subs)
        //        {
        //            string s = sub;
        //            if (sub == "Nicholas")
        //            {
        //                s = "Nick";
        //            }
        //            _eoiQuotesMarkedForConversion.AddRange(_nat02context.EoiQuotesMarkedForConversionView.Where(q => q.Csr.Contains(s)).OrderBy(q => q.TimeSubmitted).ToList());
        //        }
        //        eoiQuotesMarkedForConversion = _eoiQuotesMarkedForConversion;

        //        quotesToConvertDict = new Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)>();

        //        foreach (EoiQuotesMarkedForConversionView quote in eoiQuotesMarkedForConversion)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight weight;
        //            if (quote.Rush.Trim() == "Y")
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                weight = FontWeights.ExtraBold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                weight = FontWeights.Normal;
        //            }

        //            back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            using var nAT01Context = new NAT01Context();
        //            string shipment = nAT01Context.QuoteHeader.First(q => q.QuoteNo == quote.QuoteNo && q.QuoteRevNo == quote.QuoteRevNo).Shipment ?? "";
        //            nAT01Context.Dispose();
        //            quotesToConvertDict.Add((quote.QuoteNo, quote.QuoteRevNo), (quote.CustomerName, quote.Csr, (int)quote.DaysMarked, (DateTime)quote.TimeSubmitted, shipment.Trim(), back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
        //        }
        //        eoiQuotesMarkedForConversion.Clear();
        //        _nat02context.Dispose();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindQuotesToConvert()
        //{
        //    int i = User.VisiblePanels.IndexOf("QuotesToConvert");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "QuotesToConvert");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    quotesToConvertDict =
        //        quotesToConvertDict.Where(p => p.Key.quoteNumber.ToString().ToLower().Contains(searchString) ||
        //                                       p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                       p.Value.customerName.ToLower().Contains(searchString) ||
        //                                       p.Value.csr.ToLower().Contains(searchString))
        //                           .OrderBy(kvp => kvp.Key.quoteNumber)
        //                           .ToDictionary(x => x.Key, x => x.Value);

        //    QuotesToConvertExpanders(quotesToConvertDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetAllTabletProjects()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiAllTabletProjectsView> eoiAllTabletProjects = new List<EoiAllTabletProjectsView>();
        //        IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
        //                                                             .Select(e => e.Subscribed);
        //        eoiAllTabletProjects = new List<EoiAllTabletProjectsView>();
        //        string[] subs = subList.First().Split(',');
        //        List<EoiAllTabletProjectsView> projects = new List<EoiAllTabletProjectsView>();
        //        foreach (string sub in subs)
        //        {
        //            string s = sub;
        //            if (sub == "Gregory") { s = "Greg"; }
        //            if (sub == "Nicholas") { s = "Nick"; }
        //            eoiAllTabletProjects.AddRange(_nat02context.EoiAllTabletProjectsView.Where(q => q.Csr.Contains(s) || q.ReturnToCsr.Contains(s)).ToList());
        //        }
        //        if (_filterProjects)
        //        {
        //            eoiAllTabletProjects = eoiAllTabletProjects.Where(p => p.HoldStatus != "On Hold" &&
        //                                   !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
        //                                   .OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
        //        }
        //        else
        //        {
        //            eoiAllTabletProjects = eoiAllTabletProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
        //        }
        //        _nat02context.Dispose();

        //        allTabletProjectsDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiAllTabletProjectsView project in eoiAllTabletProjects)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            using var nat02context = new NAT02Context();
        //            bool finished = nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any();
        //            nat02context.Dispose();
        //            bool onHold = project.HoldStatus == "On Hold";
        //            bool submitted = project.TabletSubmittedBy is null ? false : project.TabletSubmittedBy.Length > 0;
        //            bool drawn = project.TabletDrawnBy.Length > 0;
        //            bool started = project.ProjectStartedTablet.Length > 0;

        //            if ((bool)project.Tools)
        //            {
        //                fontStyle = FontStyles.Oblique;
        //            }
        //            else
        //            {
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (onHold)
        //            {
        //                back = new SolidColorBrush(Colors.MediumPurple);
        //            }
        //            else if (finished)
        //            {
        //                back = new SolidColorBrush(Colors.GreenYellow);
        //            }
        //            else if (submitted)
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0A7DFF"));
        //            }
        //            else if (drawn)
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#52A3FF"));
        //            }
        //            else if (started)
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            allTabletProjectsDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.Drafter, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        dictList.Add(allTabletProjectsDict);
        //        eoiAllTabletProjects.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindAllTabletProjects()
        //{
        //    int i = User.VisiblePanels.IndexOf("AllTabletProjects");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += AllTabletProjectsSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "AllTabletProjects");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    allTabletProjectsDict =
        //        allTabletProjectsDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                         p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                         p.Value.customerName.ToLower().Contains(searchString) ||
        //                                         p.Value.csr.ToLower().Contains(searchString) ||
        //                                         p.Value.drafter.ToLower().Contains(searchString))
        //                             .OrderByDescending(kvp => kvp.Value.priority)
        //                             .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                             .ThenBy(kvp => kvp.Key.projectNumber)
        //                             .ToDictionary(x => x.Key, x => x.Value);

        //    AllTabletProjectsExpanders(allTabletProjectsDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetTabletProjectsNotStarted()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiTabletProjectsNotStarted> eoiTabletProjectsNotStarted = new List<EoiTabletProjectsNotStarted>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiTabletProjectsNotStarted = _nat02context.EoiTabletProjectsNotStarted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiTabletProjectsNotStarted = _nat02context.EoiTabletProjectsNotStarted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        tabletProjectsNotStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiTabletProjectsNotStarted project in eoiTabletProjectsNotStarted)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            if ((bool)project.Tools)
        //            {
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Oblique;
        //            }
        //            else
        //            {
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }
        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            tabletProjectsNotStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindTabletProjectsNotStarted()
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsNotStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsNotStarted");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    tabletProjectsNotStartedDict =
        //        tabletProjectsNotStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                                p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                                p.Value.customerName.ToLower().Contains(searchString) ||
        //                                                p.Value.csr.ToLower().Contains(searchString))
        //                                    .OrderByDescending(kvp => kvp.Value.priority)
        //                                    .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                                    .ThenBy(kvp => kvp.Key.projectNumber)
        //                                    .ToDictionary(x => x.Key, x => x.Value);

        //    TabletProjectsNotStartedExpanders(tabletProjectsNotStartedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetTabletProjectsStarted()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiTabletProjectsStarted> eoiTabletProjectsStarted = new List<EoiTabletProjectsStarted>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiTabletProjectsStarted = _nat02context.EoiTabletProjectsStarted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiTabletProjectsStarted = _nat02context.EoiTabletProjectsStarted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        tabletProjectsStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiTabletProjectsStarted project in eoiTabletProjectsStarted)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            if ((bool)project.Tools)
        //            {
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Oblique;
        //            }
        //            else
        //            {
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }
        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            tabletProjectsStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.ProjectStartedTablet, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiTabletProjectsStarted.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindTabletProjectsStarted()
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsStarted");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    tabletProjectsStartedDict =
        //        tabletProjectsStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                             p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                             p.Value.customerName.ToLower().Contains(searchString) ||
        //                                             p.Value.csr.ToLower().Contains(searchString) ||
        //                                             p.Value.drafter.ToLower().Contains(searchString))
        //                                 .OrderByDescending(kvp => kvp.Value.priority)
        //                                 .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                                 .ThenBy(kvp => kvp.Key.projectNumber)
        //                                 .ToDictionary(x => x.Key, x => x.Value);

        //    TabletProjectsStartedExpanders(tabletProjectsStartedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetTabletProjectsDrawn()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiTabletProjectsDrawn> eoiTabletProjectsDrawn = new List<EoiTabletProjectsDrawn>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiTabletProjectsDrawn = _nat02context.EoiTabletProjectsDrawn.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiTabletProjectsDrawn = _nat02context.EoiTabletProjectsDrawn.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        tabletProjectsDrawnDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiTabletProjectsDrawn project in eoiTabletProjectsDrawn)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            if ((bool)project.Tools)
        //            {
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Oblique;
        //            }
        //            else
        //            {
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            tabletProjectsDrawnDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.TabletDrawnBy, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiTabletProjectsDrawn.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindTabletProjectsDrawn()
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsDrawn");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsDrawn");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    tabletProjectsDrawnDict =
        //        tabletProjectsDrawnDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                           p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                           p.Value.customerName.ToLower().Contains(searchString) ||
        //                                           p.Value.csr.ToLower().Contains(searchString) ||
        //                                           p.Value.drafter.ToLower().Contains(searchString))
        //                               .OrderByDescending(kvp => kvp.Value.priority)
        //                               .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                               .ThenBy(kvp => kvp.Key.projectNumber)
        //                               .ToDictionary(x => x.Key, x => x.Value);

        //    TabletProjectsDrawnExpanders(tabletProjectsDrawnDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetTabletProjectsSubmitted()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiTabletProjectsSubmitted> eoiTabletProjectsSubmitted = new List<EoiTabletProjectsSubmitted>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiTabletProjectsSubmitted = _nat02context.EoiTabletProjectsSubmitted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiTabletProjectsSubmitted = _nat02context.EoiTabletProjectsSubmitted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        tabletProjectsSubmittedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiTabletProjectsSubmitted project in eoiTabletProjectsSubmitted)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            if ((bool)project.Tools)
        //            {
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Oblique;
        //            }
        //            else
        //            {
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            tabletProjectsSubmittedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.TabletDrawnBy ?? project.ProjectStartedTablet ?? project.TabletSubmittedBy, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiTabletProjectsSubmitted.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindTabletProjectsSubmitted()
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsSubmitted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsSubmitted");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    tabletProjectsSubmittedDict =
        //        tabletProjectsSubmittedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                              p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                              p.Value.customerName.ToLower().Contains(searchString) ||
        //                                              p.Value.csr.ToLower().Contains(searchString) ||
        //                                              p.Value.drafter.ToLower().Contains(searchString))
        //                                  .OrderByDescending(kvp => kvp.Value.priority)
        //                                  .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                                  .ThenBy(kvp => kvp.Key.projectNumber)
        //                                  .ToDictionary(x => x.Key, x => x.Value);

        //    TabletProjectsSubmittedExpanders(tabletProjectsSubmittedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetTabletProjectsOnHold()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiProjectsOnHold> eoiTabletProjectsOnHold = new List<EoiProjectsOnHold>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            if (usrName == "Gregory") { usrName = "Greg"; }
        //            if (usrName == "Nicholas") { usrName = "Nick"; }
        //            eoiTabletProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Csr.StartsWith(usrName) && p.Tablet == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiTabletProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Tablet == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        tabletProjectsOnHoldDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiProjectsOnHold project in eoiTabletProjectsOnHold)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            if ((bool)project.Tools)
        //            {
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Oblique;
        //            }
        //            else
        //            {
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            tabletProjectsOnHoldDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiTabletProjectsOnHold.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindTabletProjectsOnHold()
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsOnHold");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "TabletProjectsOnHold");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    tabletProjectsOnHoldDict =
        //        tabletProjectsOnHoldDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                          p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                          p.Value.customerName.ToLower().Contains(searchString) ||
        //                                          p.Value.csr.ToLower().Contains(searchString))
        //                              .OrderByDescending(kvp => kvp.Value.priority)
        //                              .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                              .ThenBy(kvp => kvp.Key.projectNumber)
        //                              .ToDictionary(x => x.Key, x => x.Value);

        //    TabletProjectsOnHoldExpanders(tabletProjectsOnHoldDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetAllToolProjects()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiAllToolProjectsView> eoiAllToolProjects = new List<EoiAllToolProjectsView>();
        //        try
        //        {
        //            IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == User.EmployeeCode)
        //                                                             .Select(e => e.Subscribed);
        //            string[] subs = subList.First().Split(',');
        //            if (string.IsNullOrEmpty(subs[0]))
        //            {
        //                eoiAllToolProjects.AddRange(_nat02context.EoiAllToolProjectsView.ToList());
        //            }
        //            else
        //            {
        //                foreach (string sub in subs)
        //                {
        //                    string s = sub;
        //                    if (sub == "Gregory") { s = "Greg"; }
        //                    if (sub == "Nicholas") { s = "Nick"; }
        //                    eoiAllToolProjects.AddRange(_nat02context.EoiAllToolProjectsView.Where(q => q.Csr.Contains(s) || q.ReturnToCsr.Contains(s)).ToList());
        //                }
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }
        //        if (_filterProjects)
        //        {
        //            eoiAllToolProjects = eoiAllToolProjects.Where(p => p.HoldStatus != "On Hold" &&
        //                                                                   !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber)).ToList();
        //        }
        //        _nat02context.Dispose();

        //        eoiAllToolProjects = eoiAllToolProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();

        //        allToolProjectsDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiAllToolProjectsView project in eoiAllToolProjects)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            using var nat02context = new NAT02Context();
        //            bool finished = nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any();
        //            nat02context.Dispose();
        //            bool onHold = project.HoldStatus == "On Hold";
        //            using var projectsContext = new ProjectsContext();
        //            bool tablets = (bool)projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Tablet &&
        //                           string.IsNullOrEmpty(projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).TabletCheckedBy);
        //            bool multitip = (bool)projectsContext.ProjectSpecSheet.First(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).MultiTipSketch;
        //            projectsContext.Dispose();
        //            bool drawn = project.ToolDrawnBy.Length > 0;
        //            bool started = project.ProjectStartedTool.Length > 0;

        //            fontStyle = FontStyles.Normal;

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //            }

        //            if (onHold)
        //            {
        //                back = new SolidColorBrush(Colors.MediumPurple);
        //            }
        //            else if (finished)
        //            {
        //                back = new SolidColorBrush(Colors.GreenYellow);
        //            }
        //            else if (drawn)
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3594FF"));
        //            }
        //            else if (started)
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
        //            }
        //            else if (multitip)
        //            {
        //                back = new SolidColorBrush(Colors.Gray);
        //            }
        //            else if (tablets)
        //            {
        //                back = new SolidColorBrush(Colors.Yellow);
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            allToolProjectsDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.Drafter, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiAllToolProjects.Clear();
        //    }
        //    catch //(Exception ex)
        //    {

        //    }
        //}
        //private void BindAllToolProjects()
        //{
        //    int i = User.VisiblePanels.IndexOf("AllToolProjects");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "AllToolProjects");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    allToolProjectsDict = allToolProjectsDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                                         p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                                         p.Value.customerName.ToLower().Contains(searchString) ||
        //                                                         p.Value.csr.ToLower().Contains(searchString) ||
        //                                                         p.Value.drafter.ToLower().Contains(searchString))
        //                                             .OrderByDescending(kvp => kvp.Value.priority)
        //                                             .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                                             .ThenBy(kvp => kvp.Key.projectNumber)
        //                                             .ToDictionary(x => x.Key, x => x.Value);

        //    AllToolProjectsExpanders(allToolProjectsDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetToolProjectsNotStarted()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiToolProjectsNotStarted> eoiToolProjectsNotStarted = new List<EoiToolProjectsNotStarted>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiToolProjectsNotStarted = _nat02context.EoiToolProjectsNotStarted.Where(p => p.Csr.StartsWith(usrName) && (p.Tablet == false || (p.Tablet == true && p.TabletCheckedBy.Length > 0))).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiToolProjectsNotStarted = _nat02context.EoiToolProjectsNotStarted.Where(p => p.Tablet == false || (p.Tablet == true && p.TabletCheckedBy.Length > 0)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        toolProjectsNotStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiToolProjectsNotStarted project in eoiToolProjectsNotStarted)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            fontWeight = FontWeights.Normal;
        //            fontStyle = FontStyles.Normal;

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Normal;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            toolProjectsNotStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiToolProjectsNotStarted.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindToolProjectsNotStarted()
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsNotStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsNotStarted");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    toolProjectsNotStartedDict =
        //        toolProjectsNotStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                              p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                              p.Value.customerName.ToLower().Contains(searchString) ||
        //                                              p.Value.csr.ToLower().Contains(searchString))
        //                                  .OrderByDescending(kvp => kvp.Value.priority)
        //                                  .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                                  .ThenBy(kvp => kvp.Key.projectNumber)
        //                                  .ToDictionary(x => x.Key, x => x.Value);

        //    ToolProjectsNotStartedExpanders(toolProjectsNotStartedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetToolProjectsStarted()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiToolProjectsStarted> eoiToolProjectsStarted = new List<EoiToolProjectsStarted>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiToolProjectsStarted = _nat02context.EoiToolProjectsStarted.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiToolProjectsStarted = _nat02context.EoiToolProjectsStarted.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        toolProjectsStartedDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiToolProjectsStarted project in eoiToolProjectsStarted)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            fontWeight = FontWeights.Normal;
        //            fontStyle = FontStyles.Normal;

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Normal;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            toolProjectsStartedDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.ProjectStartedTool, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiToolProjectsStarted.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindToolProjectsStarted()
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsStarted");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    toolProjectsStartedDict =
        //        toolProjectsStartedDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                           p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                           p.Value.customerName.ToLower().Contains(searchString) ||
        //                                           p.Value.csr.ToLower().Contains(searchString) ||
        //                                           p.Value.drafter.ToLower().Contains(searchString))
        //                               .OrderByDescending(kvp => kvp.Value.priority)
        //                               .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                               .ThenBy(kvp => kvp.Key.projectNumber)
        //                               .ToDictionary(x => x.Key, x => x.Value);

        //    ToolProjectsStartedExpanders(toolProjectsStartedDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetToolProjectsDrawn()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiToolProjectsDrawn> eoiToolProjectsDrawn = new List<EoiToolProjectsDrawn>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            eoiToolProjectsDrawn = _nat02context.EoiToolProjectsDrawn.Where(p => p.Csr.StartsWith(usrName)).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiToolProjectsDrawn = _nat02context.EoiToolProjectsDrawn.OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        toolProjectsDrawnDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiToolProjectsDrawn project in eoiToolProjectsDrawn)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            fontWeight = FontWeights.Normal;
        //            fontStyle = FontStyles.Normal;

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Normal;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            toolProjectsDrawnDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.ToolDrawnBy, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiToolProjectsDrawn.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindToolProjectsDrawn()
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsDrawn");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsDrawn");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    toolProjectsDrawnDict =
        //        toolProjectsDrawnDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                         p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                         p.Value.customerName.ToLower().Contains(searchString) ||
        //                                         p.Value.csr.ToLower().Contains(searchString) ||
        //                                         p.Value.drafter.ToLower().Contains(searchString))
        //                             .OrderByDescending(kvp => kvp.Value.priority)
        //                             .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                             .ThenBy(kvp => kvp.Key.projectNumber)
        //                             .ToDictionary(x => x.Key, x => x.Value);

        //    ToolProjectsDrawnExpanders(toolProjectsDrawnDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetToolProjectsOnHold()
        //{
        //    try
        //    {
        //        using var _nat02context = new NAT02Context();
        //        List<EoiProjectsOnHold> eoiToolProjectsOnHold = new List<EoiProjectsOnHold>();
        //        if (User.Department == "Customer Service")
        //        {
        //            string usrName = User.GetUserName().Split(' ')[0];
        //            if (usrName == "Gregory") { usrName = "Greg"; }
        //            if (usrName == "Nicholas") { usrName = "Nick"; }
        //            eoiToolProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Csr.StartsWith(usrName) && p.Tools == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        else
        //        {
        //            eoiToolProjectsOnHold = _nat02context.EoiProjectsOnHold.Where(p => p.Tools == true).OrderByDescending(t => t.MarkedPriority).ThenBy(t => t.DueDate).ToList();
        //        }
        //        _nat02context.Dispose();

        //        toolProjectsOnHoldDict = new Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)>();

        //        foreach (EoiProjectsOnHold project in eoiToolProjectsOnHold)
        //        {
        //            SolidColorBrush back;
        //            SolidColorBrush fore;
        //            FontWeight fontWeight;
        //            FontStyle fontStyle;
        //            bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
        //            bool late = project.DueDate < DateTime.Now.Date;
        //            using var nat02context = new NAT02Context();
        //            nat02context.Dispose();

        //            fontWeight = FontWeights.Normal;
        //            fontStyle = FontStyles.Normal;

        //            if (priority)
        //            {
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //                fontStyle = FontStyles.Normal;
        //            }
        //            else
        //            {
        //                fore = new SolidColorBrush(Colors.Black);
        //                fontWeight = FontWeights.Normal;
        //                fontStyle = FontStyles.Normal;
        //            }

        //            if (late && User.Department == "Engineering")
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //                fore = new SolidColorBrush(Colors.DarkRed);
        //                fontWeight = FontWeights.Bold;
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            toolProjectsOnHoldDict.Add((project.ProjectNumber, project.RevisionNumber), (project.CustomerName, project.Csr, project.MarkedPriority, project.DueDate.Value.ToShortDateString(), back.Color.ToString(), fore.Color.ToString(), fontWeight.ToString(), fontStyle.ToString()));
        //        }
        //        eoiToolProjectsOnHold.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindToolProjectsOnHold()
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsOnHold");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "ToolProjectsOnHold");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    toolProjectsOnHoldDict =
        //        toolProjectsOnHoldDict.Where(p => p.Key.projectNumber.ToString().ToLower().Contains(searchString) ||
        //                                          p.Key.revNumber.ToString().ToLower().Contains(searchString) ||
        //                                          p.Value.customerName.ToLower().Contains(searchString) ||
        //                                          p.Value.csr.ToLower().Contains(searchString))
        //                              .OrderByDescending(kvp => kvp.Value.priority)
        //                              .ThenBy(kvp => DateTime.Parse(kvp.Value.dueDate))
        //                              .ThenBy(kvp => kvp.Key.projectNumber)
        //                              .ToDictionary(x => x.Key, x => x.Value);

        //    ToolProjectsOnHoldExpanders(toolProjectsOnHoldDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetDriveWorksQueue()
        //{
        //    try
        //    {
        //        using var _driveworkscontext = new DriveWorksContext();
        //        List<QueueView> queueView = _driveworkscontext.QueueView.OrderBy(t => t.Priority).ThenBy(t => t.DateReleased).ToList();
        //        _driveworkscontext.Dispose();

        //        driveWorksQueueDict = new Dictionary<string, (string releasedBy, string tag, string releaseTime, int priority)>();

        //        foreach (QueueView queueItem in queueView)
        //        {
        //            driveWorksQueueDict.Add(queueItem.TargetName, (queueItem.DisplayName, queueItem.Tags, queueItem.DateReleased.Value.ToShortTimeString(), queueItem.Priority));
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindDriveWorksQueue()
        //{
        //    int i = User.VisiblePanels.IndexOf("DriveWorksQueue");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Collapsed;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "DriveWorksQueue");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (textBox.Template.FindName("SearchTextBox", textBox) as TextBox).Text.ToLower();
        //    driveWorksQueueDict = driveWorksQueueDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                                         p.Value.releasedBy.ToLower().Contains(searchString) ||
        //                                                         p.Value.tag.ToLower().Contains(searchString))
        //                                             .OrderBy(kvp => kvp.Value.priority)
        //                                             .ThenBy(kvp => kvp.Value.releaseTime)
        //                                             .ToDictionary(x => x.Key, x => x.Value);

        //    DriveWorksQueueExpanders(driveWorksQueueDict);

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}
        //private void GetNatoliOrderList()
        //{
        //    try
        //    {
        //        using var _natbcContext = new NATBCContext();
        //        List<NatoliOrderList> natoliOrderList = new List<NatoliOrderList>();
        //        string username = Environment.UserDomainName + "\\" + Environment.UserName;
        //        if (User.Department == "D1133")
        //        {
        //            natoliOrderList = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
        //        }
        //        else if (User.EmployeeCode == "E4408")
        //        {
        //            natoliOrderList = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", @"NATOLI\dnelson").ToList();
        //            natoliOrderList = natoliOrderList.OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
        //        }
        //        else
        //        {
        //            natoliOrderList = _natbcContext.Set<NatoliOrderList>().FromSqlRaw("dbo.spNOL_Get_OrderList_ByUserID @NTUserID = {0}", username).ToList();
        //            natoliOrderList = natoliOrderList.OrderBy(o => o.ShipDate).ThenBy(o => o.OrderNo).ToList();
        //        }
        //        _natbcContext.Dispose();

        //        natoliOrderListDict = new Dictionary<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string repId, string background)>();

        //        foreach (NatoliOrderList order in natoliOrderList)
        //        {
        //            int daysToShip = (order.ShipDate.Date - DateTime.Now.Date).Days;
        //            SolidColorBrush back;

        //            if (daysToShip < 0)
        //            {
        //                back = new SolidColorBrush(Colors.Red);
        //            }
        //            else if (daysToShip == 0)
        //            {
        //                back = new SolidColorBrush(Colors.Orange);
        //            }
        //            else if (daysToShip > 0 && daysToShip < 4)
        //            {
        //                back = new SolidColorBrush(Colors.Yellow);
        //            }
        //            else
        //            {
        //                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }

        //            // Get repId from quote header
        //            OrderHeader orderHeader = _nat01context.OrderHeader.Single(o => o.OrderNo == order.OrderNo);
        //            using var _ = new NAT01Context();
        //            string acctNo = _.QuoteHeader.Single(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).UserAcctNo;
        //            _.Dispose();
        //            using var __ = new NECContext();
        //            string repId = __.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Slprsnid;
        //            __.Dispose();

        //            natoliOrderListDict.Add((order.OrderNo / 100).ToString(), (order.Customer, order.ShipDate, order.Rush, order.OnHold, order.RepInitials, repId, back.Color.ToString()));
        //        }
        //        natoliOrderList.Clear();
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
        //private void BindNatoliOrderList()
        //{
        //    int i = User.VisiblePanels.IndexOf("NatoliOrderList");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    if (moduleHeader.Children.OfType<Label>().First().Content.ToString() != headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value)
        //    {
        //        moduleHeader.Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == User.VisiblePanels[i]).First().Value;

        //        Button button = moduleHeader.Children.OfType<Button>().First();

        //        button.Visibility = Visibility.Visible;

        //        TextBox textBox0 = moduleHeader.Children.OfType<TextBox>().Single(t => !t.Name.Contains("Search"));

        //        textBox0.Visibility = Visibility.Collapsed;

        //        TextBox textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.Contains("Search"));

        //        RemoveRoutedEventHandlers(textBox, TextBox.TextChangedEvent);

        //        textBox.TextChanged += NatoliOrderListSearchBox_TextChanged;

        //        dockPanel.Children.Remove(dockPanel.Children.OfType<Border>().First() as Border);

        //        (dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel).Children.Clear();

        //        BuildPanel(dockPanel, interiorStackPanel, "NatoliOrderList");
        //    }

        //    // Filter using search box so they don't lose a search just because of a refresh
        //    var _textBox = moduleHeader.Children.OfType<TextBox>().Single(t => t.Name.EndsWith("SearchBox"));
        //    string searchString = (_textBox.Template.FindName("SearchTextBox", _textBox) as TextBox).Text.ToLower();
        //    if (searchString.ToLower().StartsWith("rep:"))
        //    {
        //        searchString = searchString.Substring(4);
        //        var _filtered =
        //        natoliOrderListDict.Where(p => p.Value.repId.ToLower().Trim() == searchString)
        //                           .OrderBy(kvp => kvp.Value.shipDate)
        //                           .ToDictionary(x => x.Key, x => x.Value);

        //        // Remove/Add expanders based on filtering
        //        NatoliOrderListExpanders(_filtered);
        //    }
        //    else
        //    {
        //        var _filtered =
        //        natoliOrderListDict.Where(p => p.Key.ToString().ToLower().Contains(searchString) ||
        //                                       p.Value.customerName.ToLower().Contains(searchString))
        //                           .OrderBy(kvp => kvp.Value.shipDate)
        //                           .ToDictionary(x => x.Key, x => x.Value);

        //        // Remove/Add expanders based on filtering
        //        NatoliOrderListExpanders(_filtered);
        //    }

        //    StackPanel sp = (dockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    ScrollViewer sv = sp.Parent as ScrollViewer;
        //    if (sv.Visibility != Visibility.Visible)
        //    {
        //        sv.Visibility = Visibility.Visible;
        //        Image image = (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.OfType<Image>().First();
        //        (MainGrid.Children.OfType<Border>().First(c => c.Name == "Border_" + i).Child as DockPanel).Children.Remove(image);
        //    }
        //}

        ///// <summary>
        ///// Removes all event handlers subscribed to the specified routed event from the specified element.
        ///// </summary>
        ///// <param name="element">The UI element on which the routed event is defined.</param>
        ///// <param name="routedEvent">The routed event for which to remove the event handlers.</param>
        //public static void RemoveRoutedEventHandlers(UIElement element, RoutedEvent routedEvent)
        //{
        //    // Get the EventHandlersStore instance which holds event handlers for the specified element.
        //    // The EventHandlersStore class is declared as internal.
        //    var eventHandlersStoreProperty = typeof(UIElement).GetProperty(
        //        "EventHandlersStore", BindingFlags.Instance | BindingFlags.NonPublic);
        //    object eventHandlersStore = eventHandlersStoreProperty.GetValue(element, null);

        //    if (eventHandlersStore == null) return;

        //    // Invoke the GetRoutedEventHandlers method on the EventHandlersStore instance 
        //    // for getting an array of the subscribed event handlers.
        //    var getRoutedEventHandlers = eventHandlersStore.GetType().GetMethod(
        //        "GetRoutedEventHandlers", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //    var routedEventHandlers = (RoutedEventHandlerInfo[])getRoutedEventHandlers.Invoke(
        //        eventHandlersStore, new object[] { routedEvent });

        //    // Iteratively remove all routed event handlers from the element.
        //    try
        //    {
        //        foreach (var routedEventHandler in routedEventHandlers)
        //            element.RemoveHandler(routedEvent, routedEventHandler.Handler);
        //    }
        //    catch
        //    {

        //    }
        //}
        //#endregion

        //#region Expander Addition/Subtraction
        //private void BeingEnteredExpanders(Dictionary<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("BeingEntered");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

        //    IEnumerable<double> newOrders = dict.Keys.Except(orders);
        //    foreach (double order in newOrders)
        //    {
        //        int index = dict.Keys.ToList().IndexOf(dict.First(kvp => kvp.Key == order).Key);
        //        Expander expander = CreateBeingEnteredExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
        //        if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(kvp => kvp.Key == double.Parse(_order)).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese) { interiorStackPanel.Children.Remove(expander1); }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "BeingEntered").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
        //}
        //private void InTheOfficeExpanders(Dictionary<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("InTheOffice");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

        //    IEnumerable<double> newOrders = dict.Keys.Except(orders);
        //    foreach (double order in newOrders)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //        Expander expander = CreateInTheOfficeExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
        //        if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese) { interiorStackPanel.Children.Remove(expander1); }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "InTheOffice").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
        //}
        //private void QuotesNotConvertedExpanders(Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, string repId, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("QuotesNotConverted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(double, short)> quotes = interiorStackPanel.Children.OfType<Expander>().Select(e => (double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                       , short.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(double, short)> newQuotes = dict.Keys.AsEnumerable().Select(q => (q.quoteNumber, (short)q.revNumber)).Except(quotes);
        //    foreach ((double, short?) quote in newQuotes)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => (o.Key.quoteNumber, (short)o.Key.revNumber) == (quote.Item1, quote.Item2)));
        //        Expander expander = CreateQuotesNotConvertedExpander(dict.First(x => (x.Key.quoteNumber, (short)x.Key.revNumber) == (quote.Item1, quote.Item2)));
        //        Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        double _quote = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        short _rev = short.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.quoteNumber, (short)kvp.Key.revNumber) == (_quote, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_quote, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 5)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "QuotesNotConverted").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void OrdersEnteredUnscannedExpanders(Dictionary<double, (string customerName, int daysToShip, int daysIn, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("EnteredUnscanned");

        //    // New style
        //    //// Get main content control that houses all the rows
        //    //ContentControl mainContentControl = MainGrid.Children[i] as ContentControl;
        //    //ContentControl footerContentControl = (VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ContentControl>().Last();

        //    //// Get main content control children
        //    //List <ContentControl> rows = ((VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                                            .Children.OfType<ScrollViewer>().First().Content as DockPanel)
        //    //                                                                                                            .Children.OfType<ContentControl>().ToList();

        //    //// See if there are any that need to be removed
        //    //List<ContentControl> removeThese = new List<ContentControl>();
        //    //foreach (ContentControl row in rows)
        //    //{
        //    //    string _order = (VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                   .Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text;
        //    //    if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //    //    {
        //    //        removeThese.Add(row);
        //    //        continue;
        //    //    }
        //    //    Dispatcher.Invoke(() =>
        //    //    {
        //    //        (VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.Single(kvp => kvp.Key == double.Parse(_order)).Value.background));
        //    //        foreach (TextBlock textBlock in (VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>())
        //    //        {
        //    //            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.foreground));
        //    //        }
        //    //    });

        //    //}
        //    //foreach (ContentControl row1 in removeThese) {
        //    //    ((VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                   .Children.OfType<ScrollViewer>().First().Content as DockPanel)
        //    //                                                                                   .Children.Remove(row1); }

        //    //// See if there are any that need to be added
        //    //List<double> orders = new List<double>();
        //    //foreach (ContentControl row in rows)
        //    //{
        //    //    orders.Add(double.Parse((VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                           .Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text));
        //    //}

        //    //IEnumerable<double> newOrders = dict.Keys.Except(orders);

        //    //foreach (double order in newOrders)
        //    //{
        //    //    int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //    //    ContentControl contentControl = new ContentControl()
        //    //    {
        //    //        Style = App.Current.Resources["OrdersEnteredUnscannedRows"] as Style
        //    //    };

        //    //    contentControl.ApplyTemplate();

        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.Single(kvp => kvp.Key == order).Value.background));
        //    //    foreach (TextBlock textBlock in (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>())
        //    //    {
        //    //        textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == order).Value.foreground));
        //    //    }

        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = (order).ToString();
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "ShipDaysTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.daysToShip.ToString();
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "DaysInEngTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.daysIn.ToString();
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.customerName;
        //    //    Dispatcher.Invoke(() =>
        //    //    ((VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                   .Children.OfType<ScrollViewer>().First().Content as DockPanel)
        //    //                                                                                   .Children.Insert(index, contentControl));
        //    //}

        //    //(VisualTreeHelper.GetChild(footerContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                .Children.OfType<TextBlock>().First().Text = "Count: " + dict.Count.ToString();








        //    // Old style
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

        //    IEnumerable<double> newOrders = dict.Keys.Except(orders);
        //    foreach (double order in newOrders)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //        Expander expander = CreateEnteredUnscannedExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
        //        if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        {
        //            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background));
        //            foreach (Label label in (expander.Header as Grid).Children.OfType<Label>())
        //            {
        //                label.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.foreground));
        //            }
        //        });

        //    }
        //    foreach (Expander expander1 in removeThese) { interiorStackPanel.Children.Remove(expander1); }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "EnteredUnscanned").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
        //}
        //private void OrdersInEngineeringUnprintedExpanders(Dictionary<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("InEngineering");
        //    // New style
        //    //// Get main content control that houses all the rows
        //    //ContentControl mainContentControl = MainGrid.Children[i] as ContentControl;
        //    //ContentControl footerContentControl = (VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ContentControl>().Last();

        //    //// Get main content control children
        //    //List<ContentControl> rows = ((VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                                            .Children.OfType<ScrollViewer>().First().Content as DockPanel)
        //    //                                                                                                            .Children.OfType<ContentControl>().ToList();

        //    //// See if there are any that need to be removed
        //    //List<ContentControl> removeThese = new List<ContentControl>();
        //    //foreach (ContentControl row in rows)
        //    //{
        //    //    string _order = (VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                   .Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text;
        //    //    if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //    //    {
        //    //        removeThese.Add(row);
        //    //        continue;
        //    //    }
        //    //    Dispatcher.Invoke(() =>
        //    //    {
        //    //        (VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.Single(kvp => kvp.Key == double.Parse(_order)).Value.background));
        //    //        foreach (TextBlock textBlock in (VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>())
        //    //        {
        //    //            textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.foreground));
        //    //        }
        //    //    });

        //    //}
        //    //foreach (ContentControl row1 in removeThese)
        //    //{
        //    //    ((VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                   .Children.OfType<ScrollViewer>().First().Content as DockPanel)
        //    //                                                                                   .Children.Remove(row1);
        //    //}

        //    //// See if there are any that need to be added
        //    //List<double> orders = new List<double>();
        //    //foreach (ContentControl row in rows)
        //    //{
        //    //    orders.Add(double.Parse((VisualTreeHelper.GetChild(row as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                           .Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text));
        //    //}

        //    //IEnumerable<double> newOrders = dict.Keys.Except(orders);

        //    //foreach (double order in newOrders)
        //    //{
        //    //    int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //    //    ContentControl contentControl = new ContentControl()
        //    //    {
        //    //        Style = App.Current.Resources["OrdersInEngineeringRows"] as Style
        //    //    };

        //    //    contentControl.ApplyTemplate();

        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.Single(kvp => kvp.Key == order).Value.background));
        //    //    foreach (TextBlock textBlock in (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>())
        //    //    {
        //    //        textBlock.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == order).Value.foreground));
        //    //    }

        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = (order).ToString();
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "ShipDaysTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.daysToShip.ToString();
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.customerName;
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "EmployeeTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.employeeName;
        //    //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "DaysInEngTextBlock").Text = dict.Single(kvp => kvp.Key == order).Value.daysInEng.ToString();
        //    //    Dispatcher.Invoke(() =>
        //    //    ((VisualTreeHelper.GetChild(mainContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                   .Children.OfType<ScrollViewer>().First().Content as DockPanel)
        //    //                                                                                   .Children.Insert(index, contentControl));
        //    //}

        //    //(VisualTreeHelper.GetChild(footerContentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First()
        //    //                                                                                .Children.OfType<TextBlock>().First().Text = "Count: " + dict.Count.ToString();








        //    // Old style
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

        //    IEnumerable<double> newOrders = dict.Keys.Except(orders);
        //    foreach (double order in newOrders)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //        Expander expander = CreateInEngineeringUnprintedExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();

        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
        //        if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "InEngineering").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void QuotesToConvertExpanders(Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> dict)
        //{
        //    try
        //    {
        //        int i = User.VisiblePanels.IndexOf("QuotesToConvert");
        //        DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //        Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //        StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //        IEnumerable<(double, short)> quotes = interiorStackPanel.Children.OfType<Expander>().Select(e => (double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()),
        //                                                                                                          short.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //        Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> newQuotes = new Dictionary<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)>();

        //        foreach (var quote in dict)
        //        {
        //            if (!quotes.Any(q => q.Item1 == quote.Key.quoteNumber && q.Item2 == (short)quote.Key.revNumber))
        //            {
        //                // Add to newQuotes
        //                newQuotes.Add(quote.Key, quote.Value);
        //            }
        //        }

        //        newQuotes = newQuotes.OrderBy(kvp => kvp.Value.timeSubmitted).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //        //IEnumerable<(double, short)> newQuotes = dict.AsEnumerable().Select(kvp => (kvp.Key.quoteNumber, (short)kvp.Key.revNumber)).Except(quotes)
        //        //                                             .OrderBy(kvp => dict.First(q => q.Key.quoteNumber == kvp.Item1 && q.Key.revNumber == kvp.Item2).Value.timeSubmitted);
        //        foreach (var quote in newQuotes)
        //        {
        //            int index = dict.OrderBy(d => d.Value.timeSubmitted).ToList().IndexOf(dict.First(o => (o.Key.quoteNumber, (short)o.Key.revNumber) == (quote.Key.quoteNumber, quote.Key.revNumber)));
        //            Expander expander = CreateQuotesToConvertExpander(dict.First(q => (q.Key.quoteNumber, (short)q.Key.revNumber) == (quote.Key.quoteNumber, quote.Key.revNumber)));
        //            Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
        //        }

        //        List<Expander> removeThese = new List<Expander>();
        //        foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //        {
        //            double _quote = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //            short _rev = short.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //            if (!dict.Any(kvp => (kvp.Key.quoteNumber, (short)kvp.Key.revNumber) == (_quote, _rev)))
        //            {
        //                removeThese.Add(expander);
        //                continue;
        //            }
        //            Dispatcher.Invoke(() =>
        //            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_quote, _rev))).Value.background)));
        //        }
        //        foreach (Expander expander1 in removeThese)
        //        {
        //            interiorStackPanel.Children.Remove(expander1);
        //        }

        //        if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //        {
        //            if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //            {
        //                Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //                AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //            }
        //        }
        //        else
        //        {
        //            if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //            {
        //                Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //                headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //            }
        //        }

        //        dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "QuotesToConvert").First().Value;
        //        dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }
        //}
        //private void OrdersReadyToPrintExpanders(Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("ReadyToPrint");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

        //    IEnumerable<double> newOrders = dict.Keys.Except(orders);
        //    foreach (double order in newOrders)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //        Expander expander = CreateReadyToPrintExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
        //        if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ReadyToPrint").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void OrdersPrintedInEngineeringExpanders(Dictionary<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("PrintedInEngineering");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<double> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => double.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()));

        //    IEnumerable<double> newOrders = dict.Keys.Except(orders);
        //    foreach (double order in newOrders)
        //    {
        //        int index = dict.OrderBy(o => o.Key).ToList().IndexOf(dict.First(o => o.Key == order));
        //        Expander expander = CreatePrintedInEngineeringExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string _order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
        //        if (!dict.Any(kvp => kvp.Key == double.Parse(_order)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key == double.Parse(_order)).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "PrintedInEngineering").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void AllTabletProjectsExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("AllTabletProjects");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);
        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateAllTabletProjectsExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        try
        //        {
        //            Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
        //        }
        //        catch
        //        {

        //        }
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        {
        //            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict
        //                                                                           .First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.background));
        //            (expander.Header as Grid).Children[4].SetValue(ContentProperty, dict.First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.drafter);
        //        });
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "AllTabletProjects").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void TabletProjectsNotStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsNotStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateTabletProjectsNotStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsNotStarted").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void TabletProjectsStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateTabletProjectsStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsStarted").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void TabletProjectsDrawnExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsDrawn");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateTabletProjectsDrawnExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsDrawn").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void TabletProjectsSubmittedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsSubmitted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateTabletProjectsSubmittedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsSubmitted").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void TabletProjectsOnHoldExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("TabletProjectsOnHold");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateTabletProjectsOnHoldExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "TabletProjectsOnHold").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void AllToolProjectsExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("AllToolProjects");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);
        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateAllToolProjectsExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        try
        //        {
        //            Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        {
        //            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict
        //                                                                           .First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.background));
        //            (expander.Header as Grid).Children[4].SetValue(ContentProperty, dict.First(o => (o.Key.projectNumber, o.Key.revNumber) == (_projectNumber, _rev)).Value.drafter);
        //        });
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "AllToolProjects").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void ToolProjectsNotStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsNotStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateToolProjectsNotStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsNotStarted").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void ToolProjectsStartedExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsStarted");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateToolProjectsStartedExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }
        //    if ((interiorStackPanel.Parent as ScrollViewer).ComputedVerticalScrollBarVisibility == Visibility.Visible)
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
        //        }
        //    }
        //    else
        //    {
        //        if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 8)
        //        {
        //            Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
        //            headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
        //        }
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsStarted").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void ToolProjectsDrawnExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsDrawn");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateToolProjectsDrawnExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsDrawn").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void ToolProjectsOnHoldExpanders(Dictionary<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("ToolProjectsOnHold");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<(int, int)> projects = interiorStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString())
        //                                                                                                  , int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

        //    IEnumerable<(int, int)> newProjects = dict.Keys.AsEnumerable().Select(o => (o.projectNumber, (int)o.revNumber)).Except(projects);

        //    foreach ((int, int?) project in newProjects)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Expander expander = CreateToolProjectsOnHoldExpander(dict.First(p => (p.Key.projectNumber, (int)p.Key.revNumber) == (project.Item1, project.Item2)));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        int _projectNumber = int.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        //        int _rev = int.Parse(((Grid)expander.Header).Children[1].GetValue(ContentProperty).ToString());
        //        if (!dict.Any(kvp => (kvp.Key.projectNumber, (int)kvp.Key.revNumber) == (_projectNumber, _rev)))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //        Dispatcher.Invoke(() =>
        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(o => o.Key.Equals((_projectNumber, _rev))).Value.background)));
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "ToolProjectsOnHold").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void DriveWorksQueueExpanders(Dictionary<string, (string releasedBy, string tag, string releaseTime, int priority)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("DriveWorksQueue");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<string> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => (e.Header as Grid).Children[0].GetValue(ContentProperty).ToString());

        //    IEnumerable<string> newModels = dict.Keys.Except(orders);
        //    foreach (string model in newModels)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => o.Key == model));
        //        Expander expander = CreateDriveWorksQueueExpander(dict.First(x => x.Key == model));
        //        Dispatcher.Invoke(() =>
        //        interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string modelName = ((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString();
        //        if (!dict.Any(kvp => kvp.Key == modelName))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "DriveWorksQueue").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //private void NatoliOrderListExpanders(Dictionary<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string repId, string background)> dict)
        //{
        //    int i = User.VisiblePanels.IndexOf("NatoliOrderList");
        //    DockPanel dockPanel = MainGrid.Children.OfType<Border>().First(p => p.Name.StartsWith("Border_" + i)).Child as DockPanel;
        //    Grid moduleHeader = dockPanel.Children.OfType<Grid>().First();
        //    StackPanel interiorStackPanel = dockPanel.Children.OfType<ScrollViewer>().First().Content as StackPanel;

        //    IEnumerable<string> orders = interiorStackPanel.Children.OfType<Expander>().Select(e => (e.Header as Grid).Children[0].GetValue(ContentProperty).ToString());

        //    IEnumerable<string> newOrders = dict.Keys.Except(orders).OrderBy(o => dict.First(ol => ol.Key == o).Value.shipDate);
        //    foreach (string order in newOrders)
        //    {
        //        int index = dict.ToList().IndexOf(dict.First(o => o.Key == order));
        //        Expander expander = CreateNatoliOrderListExpander(dict.First(x => x.Key == order));
        //        Dispatcher.Invoke(() => interiorStackPanel.Children.Insert(index, expander));
        //    }

        //    List<Expander> removeThese = new List<Expander>();
        //    foreach (Expander expander in interiorStackPanel.Children.OfType<Expander>())
        //    {
        //        string order = ((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString();
        //        if (!dict.Any(kvp => kvp.Key == order))
        //        {
        //            removeThese.Add(expander);
        //            continue;
        //        }
        //    }
        //    foreach (Expander expander1 in removeThese)
        //    {
        //        interiorStackPanel.Children.Remove(expander1);
        //    }

        //    dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "NatoliOrderList").First().Value;
        //    dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Keys.Count;
        //}
        //#endregion

        //#region Expander Creation
        //private Expander CreateBeingEnteredExpander(KeyValuePair<double, (double quoteNumber, int revNumber, string customerName, int numDaysToShip, string background, string foreground, string fontWeight)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(65)), CreateLabel(kvp.Value.quoteNumber.ToString(), 0, 1, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(30)), CreateLabel(kvp.Value.revNumber.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 3, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.numDaysToShip.ToString(), 0, 4, fontWeight, foreground, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black)
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += OrdersBeingEnteredExpander_MouseRightButtonUp;
        //    return expander;
        //}
        //private Expander CreateInTheOfficeExpander(KeyValuePair<double, (string customerName, int daysToShip, int daysInOffice, string employeeName, string csr, string background, string foreground, string fontWeight)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        VerticalAlignment = VerticalAlignment.Top
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Value.daysInOffice.ToString(), 0, 3, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(100)), CreateLabel(kvp.Value.employeeName, 0, 4, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.csr, 0, 5, fontWeight, foreground, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black)
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += OrdersInTheOfficeExpander_MouseRightButtonUp;

        //    // expander.Expanded += InTheOfficeExpander_Expanded;
        //    return expander;
        //}
        //private Expander CreateEnteredUnscannedExpander(KeyValuePair<double, (string customerName, int daysToShip, int daysIn, string background, string foreground, string fontWeight)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysIn.ToString(), 0, 3, fontWeight, foreground, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black)
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += OrdersEnteredUnscannedDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += EnteredUnscannedExpander_Expanded;
        //    return expander;
        //}
        //private Expander CreateInEngineeringUnprintedExpander(KeyValuePair<double, (string customerName, int daysToShip, int daysInEng, string employeeName, string background, string foreground, string fontWeight)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        VerticalAlignment = VerticalAlignment.Top
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysInEng.ToString(), 0, 3, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.employeeName, 0, 4, fontWeight, foreground, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += OrdersInEngineeringUnprintedExpander_MouseRightButtonUp;

        //    expander.Expanded += InEngineeringExpander_Expanded;
        //    return expander;
        //}
        //private Expander CreateReadyToPrintExpander(KeyValuePair<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    try
        //    {
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.employeeName, 0, 3, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(105)), CreateLabel(kvp.Value.checkedBy, 0, 4, fontWeight, foreground, null, 14, true));
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //    }

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += OrdersReadyToPrintExpander_MouseRightButtonUp;

        //    //expander.Expanded += ReadyToPrintExpander_Expanded;
        //    return expander;
        //}
        //private Expander CreatePrintedInEngineeringExpander(KeyValuePair<double, (string customerName, int daysToShip, string employeeName, string checkedBy, string background, string foreground, string fontWeight)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Value.daysToShip.ToString(), 0, 2, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.employeeName, 0, 3, fontWeight, foreground, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.checkedBy, 0, 4, fontWeight, foreground, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += OrderPrintedInEngineeringDataGrid_MouseRightButtonUp;

        //    //expander.Expanded += PrintedInEngineeringExpander_Expanded;
        //    return expander;
        //}
        //private Expander CreateQuotesNotConvertedExpander(KeyValuePair<(double quoteNumber, short? revNumber), (string customerName, string csr, string repId, string background, string foreground, string fontWeight)> kvp)
        //{
        //    try
        //    {
        //        Grid grid = new Grid
        //        {
        //            HorizontalAlignment = HorizontalAlignment.Stretch
        //        };

        //        SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //        FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(65)), CreateLabel(kvp.Key.quoteNumber.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName, 0, 2, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.csr, 0, 3, fontWeight, foreground, null, 14, true));

        //        Expander expander = new Expander()
        //        {
        //            IsExpanded = false,
        //            Header = grid,
        //            HorizontalAlignment = HorizontalAlignment.Stretch
        //        };

        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //        expander.MouseDoubleClick += QuoteDataGrid_MouseDoubleClick;
        //        expander.PreviewKeyDown += QuoteDataGrid_PreviewKeyDown;
        //        expander.PreviewMouseDown += QuoteDataGrid_PreviewMouseDown;
        //        expander.MouseRightButtonUp += QuotesNotConverted_MouseRightButtonUp;

        //        expander.Expanded += QuotesNotConvertedExpander_Expanded;
        //        return expander;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return new Expander();
        //    }
        //}
        //private Expander CreateQuotesToConvertExpander(KeyValuePair<(double quoteNumber, short? revNumber), (string customerName, string csr, int daysIn, DateTime timeSubmitted, string shipment, string background, string foreground, string fontWeight)> kvp)
        //{
        //    try
        //    {
        //        Grid grid = new Grid
        //        {
        //            HorizontalAlignment = HorizontalAlignment.Stretch
        //        };

        //        SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //        FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(65)), CreateLabel(kvp.Key.quoteNumber.ToString(), 0, 0, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName, 0, 2, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(150)), CreateLabel(kvp.Value.csr, 0, 3, fontWeight, foreground, null, 14, true));
        //        AddColumn(grid, CreateColumnDefinition(new GridLength(50)), CreateLabel(kvp.Value.daysIn.ToString(), 0, 4, fontWeight, foreground, null, 14, true));

        //        Expander expander = new Expander()
        //        {
        //            IsExpanded = false,
        //            Header = grid,
        //            HorizontalAlignment = HorizontalAlignment.Stretch,
        //            ToolTip = kvp.Value.shipment
        //        };

        //        expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //        expander.MouseDoubleClick += QuoteDataGrid_MouseDoubleClick;
        //        expander.PreviewKeyDown += QuoteDataGrid_PreviewKeyDown;
        //        expander.PreviewMouseDown += QuoteDataGrid_PreviewMouseDown;
        //        expander.MouseRightButtonUp += QuotesToConvert_MouseRightButtonUp;

        //        //expander.Expanded += QuotesNotConvertedExpander_Expanded;
        //        return expander;
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(ex.Message);
        //        return new Expander();
        //    }
        //}
        //private Expander CreateAllTabletProjectsExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate.Trim(), 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += AllTabletProjectsDataGrid_MouseRightButtonUp;

        //    expander.Expanded += AllTabletProjectsExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateTabletProjectsNotStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 4, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += TabletProjectNotStartedDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += TabletProjectsNotStartedExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateTabletProjectsStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += TabletProjectStartedDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += TabletProjectsStartedExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateTabletProjectsDrawnExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += TabletProjectDrawnDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += TabletProjectsDrawnExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateTabletProjectsSubmittedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += TabletProjectSubmittedDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += TabletProjectsSubmittedExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateTabletProjectsOnHoldExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.priority.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += TabletProjectOnHoldDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += TabletProjectsOnHoldExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateAllToolProjectsExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate.Trim(), 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += AllToolProjectsDataGrid_MouseRightButtonUp;

        //    expander.Expanded += AllToolProjectsExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateToolProjectsNotStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 4, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += ToolProjectNotStartedDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += ToolProjectsNotStartedExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateToolProjectsStartedExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += ToolProjectStartedDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += ToolProjectsStartedExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateToolProjectsDrawnExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string drafter, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.drafter, 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += ToolProjectDrawnDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += ToolProjectsDrawnExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateToolProjectsOnHoldExpander(KeyValuePair<(int projectNumber, int? revNumber), (string customerName, string csr, string priority, string dueDate, string background, string foreground, string fontWeight, string fontStyle)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
        //    FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
        //    FontStyle fontStyle = (FontStyle)new FontStyleConverter().ConvertFromString(kvp.Value.fontStyle);
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Key.projectNumber.ToString(), 0, 0, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Key.revNumber.ToString(), 0, 1, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 2, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.csr.Trim(), 0, 3, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(70)), CreateLabel(kvp.Value.priority.Trim(), 0, 4, fontWeight, foreground, fontStyle, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.dueDate, 0, 5, fontWeight, foreground, fontStyle, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    expander.MouseRightButtonUp += ToolProjectOnHoldDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += ToolProjectsOnHoldExpander_Expanded;
        //    using var __nat02context = new NAT02Context();
        //    if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber))
        //    {
        //        expander.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == kvp.Key.projectNumber && p.RevisionNumber == kvp.Key.revNumber).OnHoldComment.Trim();
        //    }
        //    __nat02context.Dispose();
        //    return expander;
        //}
        //private Expander CreateDriveWorksQueueExpander(KeyValuePair<string, (string releasedBy, string tag, string releaseTime, int priority)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Key.Trim(), 0, 0, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(170)), CreateLabel(kvp.Value.releasedBy.Trim(), 0, 1, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(120)), CreateLabel(kvp.Value.tag.Trim(), 0, 2, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.releaseTime, 0, 3, FontWeights.Normal, null, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null,
        //        Background = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFFFF")
        //    };

        //    // expander.MouseDoubleClick += ProjectDataGrid_MouseDoubleClick;
        //    // expander.PreviewKeyDown += ProjectDataGrid_PreviewKeyDown;
        //    // expander.PreviewMouseDown += ProjectDataGrid_PreviewMouseDown;
        //    // expander.MouseRightButtonUp += ToolProjectOnHoldDataGrid_MouseRightButtonUp;

        //    // expander.Expanded += DriveWorksQueueExpander_Expanded;
        //    return expander;
        //}
        //private Expander CreateNatoliOrderListExpander(KeyValuePair<string, (string customerName, DateTime shipDate, string rush, string onHold, string rep, string repId, string background)> kvp)
        //{
        //    Grid grid = new Grid
        //    {
        //        HorizontalAlignment = HorizontalAlignment.Stretch
        //    };

        //    AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ToString(), 0, 0, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(kvp.Value.customerName.Trim(), 0, 1, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(80)), CreateLabel(kvp.Value.shipDate.ToShortDateString(), 0, 2, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Value.rush.Trim(), 0, 3, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(55)), CreateLabel(kvp.Value.onHold, 0, 4, FontWeights.Normal, null, null, 14, true));
        //    AddColumn(grid, CreateColumnDefinition(new GridLength(40)), CreateLabel(kvp.Value.rep, 0, 5, FontWeights.Normal, null, null, 14, true));

        //    Expander expander = new Expander()
        //    {
        //        IsExpanded = false,
        //        Header = grid,
        //        HorizontalAlignment = HorizontalAlignment.Stretch,
        //        BorderBrush = new SolidColorBrush(Colors.Black),
        //        ToolTip = null
        //    };

        //    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
        //    expander.Expanded += OrderListExpander_Expanded;
        //    expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;

        //    return expander;
        //}
        //#endregion

        //#region Module Search Box Text Changed Events
        //private void OrdersBeingEnteredSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.Invoke(() => BindBeingEntered()), TaskScheduler.Current);
        //}
        //private void OrdersInTheOfficeSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.Invoke(() => BindInTheOffice()), TaskScheduler.Current);
        //}
        //private void QuotesNotConvertedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetQuotesNotConverted()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesNotConverted()), TaskScheduler.Current);
        //}
        //private void OrdersEnteredUnscannedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.Invoke(() => BindEnteredUnscanned()), TaskScheduler.Current);
        //}
        //private void OrdersInEngineeringUnprintedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindInEngineering()), TaskScheduler.Current);
        //}
        //private void QuotesToConvertSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetQuotesToConvert()).ContinueWith(t => Dispatcher.Invoke(() => BindQuotesToConvert()), TaskScheduler.Current);
        //}
        //private void OrdersReadyToPrintSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.Invoke(() => BindReadyToPrint()), TaskScheduler.Current);
        //}
        //private void OrdersPrintedInEngineeringSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.Invoke(() => BindPrintedInEngineering()), TaskScheduler.Current);
        //}
        //private void AllTabletProjectsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()), TaskScheduler.Current);
        //}
        //private void TabletProjectsNotStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsNotStarted()), TaskScheduler.Current);
        //}
        //private void TabletProjectsStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsStarted()), TaskScheduler.Current);
        //}
        //private void TabletProjectsDrawnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsDrawn()), TaskScheduler.Current);
        //}
        //private void TabletProjectsSubmittedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsSubmitted()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsSubmitted()), TaskScheduler.Current);
        //}
        //private void TabletProjectsOnHoldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetTabletProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindTabletProjectsOnHold()), TaskScheduler.Current);
        //}
        //private void AllToolProjectsSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()), TaskScheduler.Current);
        //}
        //private void ToolProjectsNotStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsNotStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsNotStarted()), TaskScheduler.Current);
        //}
        //private void ToolProjectsStartedSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsStarted()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsStarted()), TaskScheduler.Current);
        //}
        //private void ToolProjectsDrawnSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsDrawn()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsDrawn()), TaskScheduler.Current);
        //}
        //private void ToolProjectsOnHoldSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetToolProjectsOnHold()).ContinueWith(t => Dispatcher.Invoke(() => BindToolProjectsOnHold()), TaskScheduler.Current);
        //}
        //private void DriveWorksQueueSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()), TaskScheduler.Current);
        //}
        //private void NatoliOrderListSearchBox_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()), TaskScheduler.Current);
        //}
        //#endregion

        //#region Expanders Expanding Events
        //private void OrderListExpander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    Expander expander = (Expander)sender;
        //    Grid grid = (Grid)expander.Header;
        //    UIElementCollection collection = grid.Children;
        //    string orderNumber = collection[0].GetValue(ContentProperty).ToString() + "00";
        //    using var _natbcContext = new NATBCContext();

        //    List<LineItemLastScan> lines = _natbcContext.LineItemLastScan.FromSqlRaw("SELECT DISTINCT OrderDetailTypeDescription, OrderLineNumber, (SELECT TOP 1 ScanTimeStamp FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'ScanTimeStamp', (SELECT TOP 1 DepartmentDesc FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Department', (SELECT TOP 1 EmployeeName FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Employee' FROM NATBC.dbo.TravellerScansAudit TSA WITH (NOLOCK) WHERE TSA.OrderNumber = {0} AND TSA.OrderDetailTypeID NOT IN('E','H','MC','RET','T','TM','Z') AND TSA.OrderDetailTypeDescription <> 'PARTS' AND TSA.DepartmentDesc <> 'Production Mgmnt' ORDER BY OrderLineNumber", orderNumber).ToList();
        //    _natbcContext.Dispose();

        //    StackPanel lineItemsStackPanel = new StackPanel()
        //    {
        //        Orientation = Orientation.Vertical
        //    };

        //    foreach (LineItemLastScan lineItem in lines)
        //    {
        //        Grid lineItemGrid = new Grid();
        //        // lineItemGrid.Width = expander.Width - 30 - 22;
        //        lineItemGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.OrderDetailTypeDescription, 0, 0, FontWeights.Normal));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(string.Format("{0:d} {0:t}", lineItem.ScanTimeStamp), 0, 1, FontWeights.Normal));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.Department, 0, 2, FontWeights.Normal));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.Employee, 0, 3, FontWeights.Normal));

        //        lineItemsStackPanel.Children.Add(lineItemGrid);
        //    }

        //    expander.Content = lineItemsStackPanel;
        //}
        //private void InEngineeringExpander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    Expander expander = sender as Expander;
        //    string orderNumber = (expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString() + "00";
        //    using var _natbcContext = new NATBCContext();

        //    List<LineItemLastScan> lines = _natbcContext.LineItemLastScan.FromSqlRaw("SELECT DISTINCT OrderDetailTypeDescription, OrderLineNumber, (SELECT TOP 1 ScanTimeStamp FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'ScanTimeStamp', (SELECT TOP 1 DepartmentDesc FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Department', (SELECT TOP 1 EmployeeName FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Employee' FROM NATBC.dbo.TravellerScansAudit TSA WITH (NOLOCK) WHERE TSA.OrderNumber = {0} AND TSA.OrderDetailTypeID NOT IN('E','H','MC','RET','T','TM','Z') AND TSA.OrderDetailTypeDescription <> 'PARTS' AND TSA.DepartmentDesc <> 'Production Mgmnt' ORDER BY OrderLineNumber", orderNumber).ToList();
        //    _natbcContext.Dispose();

        //    StackPanel lineItemsStackPanel = new StackPanel()
        //    {
        //        Orientation = Orientation.Vertical
        //    };

        //    foreach (LineItemLastScan lineItem in lines)
        //    {
        //        Grid lineItemGrid = new Grid();
        //        // lineItemGrid.Width = expander.Width - 30 - 22;
        //        lineItemGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

        //        bool isChecked = selectedOrders.Any(o => o.Item1.Contains((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString())) ||
        //                         selectedLineItems.Any(o => o.Contains(orderNumber) && o.Substring(1, 2) == lineItem.OrderLineNumber.ToString("00"));

        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(36)));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(18)), CreateCheckBox(0, 1, isChecked));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(lineItem.OrderDetailTypeDescription, 0, 2, FontWeights.Normal));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(string.Format("{0:d} {0:t}", lineItem.ScanTimeStamp), 0, 3, FontWeights.Normal));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(lineItem.Department, 0, 4, FontWeights.Normal));
        //        AddColumn(lineItemGrid, CreateColumnDefinition(new GridLength(150)), CreateLabel(lineItem.Employee, 0, 5, FontWeights.Normal));

        //        lineItemGrid.Tag = lineItem.OrderLineNumber;

        //        lineItemsStackPanel.Children.Add(lineItemGrid);
        //    }

        //    expander.Content = lineItemsStackPanel;
        //}
        //private void AllTabletProjectsExpander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    Expander expander = sender as Expander;
        //    int projectNumber = int.Parse((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString());
        //    int revNumber = int.Parse((expander.Header as Grid).Children[1].GetValue(ContentProperty).ToString());
        //    using var _projectsContext = new ProjectsContext();
        //    ProjectSpecSheet eoiAllTabletProjectsView = _projectsContext.ProjectSpecSheet.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber);

        //    StackPanel stagesStackPanel = new StackPanel()
        //    {
        //        Orientation = Orientation.Vertical
        //    };

        //    List<(string, string, DateTime?)> stages = new List<(string, string, DateTime?)>();
        //    if (!string.IsNullOrEmpty(eoiAllTabletProjectsView.ProjectStartedTablet)) { stages.Add(("Started", eoiAllTabletProjectsView.ProjectStartedTablet,
        //        _projectsContext.ProjectStartedTablet.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted)); }
        //    if (!string.IsNullOrEmpty(eoiAllTabletProjectsView.TabletDrawnBy)) { stages.Add(("Drawn", eoiAllTabletProjectsView.TabletDrawnBy,
        //        _projectsContext.TabletDrawnBy.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    if (!string.IsNullOrEmpty(eoiAllTabletProjectsView.TabletSubmittedBy)) { stages.Add(("Submitted", eoiAllTabletProjectsView.TabletSubmittedBy,
        //        _projectsContext.TabletSubmittedBy.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    if (!string.IsNullOrEmpty(eoiAllTabletProjectsView.TabletCheckedBy)) { stages.Add(("Checked", eoiAllTabletProjectsView.TabletCheckedBy,
        //        _projectsContext.TabletCheckedBy.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    _projectsContext.Dispose();

        //    foreach ((string, string, DateTime?) stage in stages)
        //    {
        //        Grid stagesGrid = new Grid();
        //        stagesGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(36)));
        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(stage.Item1, 0, 1, FontWeights.Normal));
        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(stage.Item2, 0, 2, FontWeights.Normal));
        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(string.Format("{0:d} {0:t}", stage.Item3), 0, 3, FontWeights.Normal));

        //        stagesStackPanel.Children.Add(stagesGrid);
        //    }

        //    expander.Content = stagesStackPanel;
        //}
        //private void AllToolProjectsExpander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    Expander expander = sender as Expander;
        //    int projectNumber = int.Parse((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString());
        //    int revNumber = int.Parse((expander.Header as Grid).Children[1].GetValue(ContentProperty).ToString());
        //    using var _projectsContext = new ProjectsContext();
        //    ProjectSpecSheet eoiAllToolProjectsView = _projectsContext.ProjectSpecSheet.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber);

        //    StackPanel stagesStackPanel = new StackPanel()
        //    {
        //        Orientation = Orientation.Vertical
        //    };

        //    List<(string, string, DateTime?)> stages = new List<(string, string, DateTime?)>();
        //    if (!string.IsNullOrEmpty(eoiAllToolProjectsView.ProjectStartedTool))
        //    {
        //        stages.Add(("Started", eoiAllToolProjectsView.ProjectStartedTool,
        //            _projectsContext.ProjectStartedTool.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    if (!string.IsNullOrEmpty(eoiAllToolProjectsView.ToolDrawnBy))
        //    {
        //        stages.Add(("Drawn", eoiAllToolProjectsView.ToolDrawnBy,
        //            _projectsContext.ToolDrawnBy.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    if (!string.IsNullOrEmpty(eoiAllToolProjectsView.ToolSubmittedBy))
        //    {
        //        stages.Add(("Submitted", eoiAllToolProjectsView.ToolSubmittedBy,
        //            _projectsContext.ToolSubmittedBy.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    if (!string.IsNullOrEmpty(eoiAllToolProjectsView.ToolCheckedBy))
        //    {
        //        stages.Add(("Checked", eoiAllToolProjectsView.ToolCheckedBy,
        //            _projectsContext.ToolCheckedBy.Single(p => p.ProjectNumber == projectNumber && p.RevisionNumber == revNumber).TimeSubmitted));
        //    }
        //    _projectsContext.Dispose();

        //    foreach ((string, string, DateTime?) stage in stages)
        //    {
        //        Grid stagesGrid = new Grid();
        //        stagesGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(36)));
        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel(stage.Item1, 0, 1, FontWeights.Normal));
        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(stage.Item2, 0, 2, FontWeights.Normal));
        //        AddColumn(stagesGrid, CreateColumnDefinition(new GridLength(120)), CreateLabel(string.Format("{0:d} {0:t}", stage.Item3), 0, 3, FontWeights.Normal));

        //        stagesStackPanel.Children.Add(stagesGrid);
        //    }

        //    expander.Content = stagesStackPanel;
        //}
        //private void QuotesNotConvertedExpander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    Expander expander = sender as Expander;
        //    int quoteNumber = int.Parse((expander.Header as Grid).Children[0].GetValue(ContentProperty).ToString());
        //    int revNumber = int.Parse((expander.Header as Grid).Children[1].GetValue(ContentProperty).ToString());

        //    // Get the quote date/revision date
        //    using var _nat01Context = new NAT01Context();
        //    DateTime quoteDate = _nat01Context.QuoteHeader.Single(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == revNumber).QuoteDate;
        //    _nat01Context.Dispose();

        //    // Get the follow-up date(s)
        //    using var _nat02Context = new NAT02Context();
        //    List<DateTime?> followUps = _nat02Context.EoiQuotesOneWeekCompleted.Where(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == revNumber)
        //                                                                       .OrderBy(q => q.TimeSubmitted)
        //                                                                       .Select(q => q.TimeSubmitted).ToList();
        //    _nat02Context.Dispose();

        //    StackPanel infoStackPanel = new StackPanel()
        //    {
        //        Orientation = Orientation.Vertical
        //    };

        //    Grid infoGrid = new Grid();
        //    infoGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

        //    AddColumn(infoGrid, CreateColumnDefinition(new GridLength(36)));
        //    AddColumn(infoGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("The last saved date of this quote is: " + quoteDate.ToShortDateString(), 0, 1, FontWeights.Normal));
        //    infoStackPanel.Children.Add(infoGrid);

        //    if (followUps.Any())
        //    {
        //        foreach (DateTime date in followUps)
        //        {
        //            infoGrid = new Grid();
        //            infoGrid.HorizontalAlignment = HorizontalAlignment.Stretch;

        //            AddColumn(infoGrid, CreateColumnDefinition(new GridLength(36)));
        //            AddColumn(infoGrid, CreateColumnDefinition(new GridLength(1, GridUnitType.Star)), CreateLabel("Follow-up was completed on: " + date.ToShortDateString(), 0, 1, FontWeights.Normal));
        //            infoStackPanel.Children.Add(infoGrid);
        //        }
        //    }

        //    expander.Content = infoStackPanel;
        //}
        //#endregion
        //#endregion

        //#region DataGridLoadingRow
        //private void InEngineeringDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiOrdersInEngineeringUnprintedView rowView = dataGrid.Items[index] as EoiOrdersInEngineeringUnprintedView;
        //        bool rush = rowView.RushYorN.ToString().Trim() == "Y" || rowView.PaidRushFee.ToString().Trim() == "Y";
        //        bool doNotProcess = Convert.ToBoolean(rowView.DoNotProcess);
        //        bool beingChecked = Convert.ToBoolean(rowView.BeingChecked);
        //        bool markedForChecking = Convert.ToBoolean(rowView.MarkedForChecking);
        //        if (rush)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (doNotProcess)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Pink);
        //        }
        //        else
        //        {
        //            if (beingChecked && User.Department == "Engineering")
        //            {
        //                e.Row.Background = new SolidColorBrush(Colors.DodgerBlue);
        //            }
        //            //else if (count == 0 && (machineType == "BB" || machineType == "B" || machineType == "D") && lineType.Count != 0)
        //            //{
        //            //    e.Row.Background = new SolidColorBrush(Colors.Red);
        //            //}
        //            else if (markedForChecking)
        //            {
        //                e.Row.Background = new SolidColorBrush(Colors.GreenYellow);
        //            }
        //            else
        //            {
        //                e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //        }
        //    }
        //    catch //(Exception ex)
        //    {
        //        // MessageBox.Show(ex.Message);
        //        // IMethods.WriteToErrorLog("InEngineeringDataGrid_LoadingRow", ex.Message, User);
        //    }
        //}

        //private void ReadyToPrintDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiOrdersReadyToPrintView rowView = dataGrid.Items[index] as EoiOrdersReadyToPrintView;
        //        double orderNumber = rowView.OrderNo;
        //        bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
        //        bool tm2 = Convert.ToBoolean(rowView.TM2);
        //        bool tabletPrints = Convert.ToBoolean(rowView.Tablet);
        //        bool toolPrints = Convert.ToBoolean(rowView.Tool);
        //        List<OrderDetails> orderDetails;
        //        List<OrderHeader> orderHeader;
        //        orderDetails = _nat01context.OrderDetails.Where(o => o.OrderNo == orderNumber * 100).ToList();
        //        orderHeader = _nat01context.OrderHeader.Where(o => o.OrderNo == orderNumber * 100).ToList();
        //        if (rush)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }

        //        if (tm2 || tabletPrints)
        //        {
        //            foreach (OrderDetails od in orderDetails)
        //            {
        //                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
        //                {
        //                    string path = @"\\engserver\workstations\tool_drawings\" + orderNumber + @"\" + od.HobNoShapeId.Trim() + ".pdf";
        //                    if (!System.IO.File.Exists(path))
        //                    {
        //                        goto Missing;
        //                    }
        //                }
        //            }
        //        }

        //        if (tm2 || toolPrints)
        //        {
        //            foreach (OrderDetails od in orderDetails)
        //            {
        //                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
        //                {
        //                    string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
        //                    detailType = detailType == "MISC" ? "REJECT" : detailType;
        //                    string international = orderHeader.FirstOrDefault().UnitOfMeasure;
        //                    string path = @"\\engserver\workstations\tool_drawings\" + orderNumber + @"\" + detailType + ".pdf";
        //                    if (!System.IO.File.Exists(path))
        //                    {
        //                        goto Missing;
        //                    }
        //                    if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
        //                    {
        //                        goto Missing;
        //                    }
        //                }
        //            }
        //        }

        //        goto NotMissing;

        //    Missing:;
        //        if (User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //        goto Finished;

        //    NotMissing:;
        //        e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

        //    Finished:;
        //    }
        //    catch
        //    {

        //    }
        //}

        //private void InTheOfficeDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiOrdersInOfficeView rowView = dataGrid.Items[index] as EoiOrdersInOfficeView;
        //        int? orderNumber = rowView.OrderNo;
        //        bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
        //        bool doNotProcess = Convert.ToBoolean(rowView.DoNotProcess);
        //        if (rush)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (doNotProcess)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Pink);
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}

        //private void EnteredUnscannedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiOrdersEnteredAndUnscannedView rowView = dataGrid.Items[index] as EoiOrdersEnteredAndUnscannedView;
        //        bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
        //        bool doNotProcess = Convert.ToBoolean(rowView.DoNotProcess);
        //        string[] errRes;
        //        errRes = new string[2] { rowView.ProcessState,
        //                  rowView.TransitionName };
        //        if (rush)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.White);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (doNotProcess)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Pink);
        //        }
        //        else if (((errRes[0] == "Failed" && errRes[0] != "Complete") || errRes[1] == "NeedInfo") && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Black);
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //private void BeingEnteredDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiOrdersBeingEnteredView rowView = dataGrid.Items[index] as EoiOrdersBeingEnteredView;
        //        bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
        //        if (rush)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //private void QuotesNotConvertedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiQuotesNotConvertedView rowView = dataGrid.Items[index] as EoiQuotesNotConvertedView;
        //        int daysOld = (DateTime.Now - _nat01context.QuoteHeader.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).Select(q => q.QuoteDate).First()).Days;
        //        string rush = rowView.RushYorN;
        //        if (rush == "Y")
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (daysOld > 6)
        //        {
        //            using var _nat02context = new NAT02Context();
        //            if (!_nat02context.EoiQuotesOneWeekCompleted.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).Any())
        //            {
        //                e.Row.Background = new SolidColorBrush(Colors.Pink);
        //            }
        //            else
        //            {
        //                e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //            }
        //            _nat02context.Dispose();
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //private void QuotesToConvertDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiQuotesMarkedForConversionView rowView = dataGrid.Items[index] as EoiQuotesMarkedForConversionView;
        //        string rush = rowView.Rush.Trim();
        //        e.Row.ToolTip = null;
        //        if (rush == "Y")
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        using var _nat01context = new NAT01Context();
        //        e.Row.ToolTip = string.IsNullOrEmpty(_nat01context.QuoteHeader.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).First().Shipment.Trim()) ? "No Comment" : _nat01context.QuoteHeader.Where(q => q.QuoteNo == rowView.QuoteNo && q.QuoteRevNo == rowView.QuoteRevNo).First().Shipment.Trim();
        //    }
        //    catch
        //    {
        //    }
        //}

        //private void PrintedInEngineeringDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiOrdersPrintedInEngineeringView rowView = dataGrid.Items[index] as EoiOrdersPrintedInEngineeringView;
        //        bool rush = rowView.RushYorN == "Y" || rowView.PaidRushFee == "Y";
        //        bool tm2 = Convert.ToBoolean(rowView.TM2);
        //        bool tabletPrints = Convert.ToBoolean(rowView.Tablet);
        //        bool toolPrints = Convert.ToBoolean(rowView.Tool);
        //        List<OrderDetails> orderDetails;
        //        List<OrderHeader> orderHeader;
        //        orderDetails = _nat01context.OrderDetails.Where(o => o.OrderNo == rowView.OrderNo * 100).ToList();
        //        orderHeader = _nat01context.OrderHeader.Where(o => o.OrderNo == rowView.OrderNo * 100).ToList();
        //        if (rush)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }

        //        if (tm2 || tabletPrints)
        //        {
        //            foreach (OrderDetails od in orderDetails)
        //            {
        //                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "R")
        //                {
        //                    string path = @"\\engserver\workstations\tool_drawings\" + rowView.OrderNo + @"\" + od.HobNoShapeId.Trim() + ".pdf";
        //                    if (!System.IO.File.Exists(path))
        //                    {
        //                        goto Missing;
        //                    }
        //                }
        //            }
        //        }

        //        if (tm2 || toolPrints)
        //        {
        //            foreach (OrderDetails od in orderDetails)
        //            {
        //                if (od.DetailTypeId.Trim() == "U" || od.DetailTypeId.Trim() == "L" || od.DetailTypeId.Trim() == "D" || od.DetailTypeId.Trim() == "DS" || od.DetailTypeId.Trim() == "R")
        //                {
        //                    string detailType = oeDetailTypes[od.DetailTypeId.Trim()];
        //                    detailType = detailType == "MISC" ? "REJECT" : detailType;
        //                    string international = orderHeader.FirstOrDefault().UnitOfMeasure;
        //                    string path = @"\\engserver\workstations\tool_drawings\" + rowView.OrderNo + @"\" + detailType + ".pdf";
        //                    if (!System.IO.File.Exists(path))
        //                    {
        //                        goto Missing;
        //                    }
        //                    if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
        //                    {
        //                        goto Missing;
        //                    }
        //                }
        //            }
        //        }

        //        goto NotMissing;

        //    Missing:;
        //        if (User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //        goto Finished;

        //    NotMissing:;
        //        e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));

        //    Finished:;
        //    }
        //    catch
        //    {

        //    }
        //}

        //private void AllTabletProjectsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiAllTabletProjectsView rowView = dataGrid.Items[index] as EoiAllTabletProjectsView;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        using var _nat02context = new NAT02Context();
        //        bool finished = _nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).Any();
        //        _nat02context.Dispose();
        //        bool onHold = rowView.HoldStatus == "On Hold";
        //        bool submitted = rowView.TabletSubmittedBy is null ? false : rowView.TabletSubmittedBy.Length > 0;
        //        bool drawn = rowView.TabletDrawnBy.Length > 0;
        //        bool started = rowView.ProjectStartedTablet.Length > 0;
        //        e.Row.ToolTip = null;
        //        if ((bool)rowView.Tools)
        //        {
        //            e.Row.FontWeight = FontWeights.Bold;
        //            e.Row.FontStyle = FontStyles.Oblique;
        //        }
        //        else
        //        {
        //            e.Row.FontWeight = FontWeights.Normal;
        //            e.Row.FontStyle = FontStyles.Normal;
        //        }
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (onHold)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
        //            using var __nat02context = new NAT02Context();
        //            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber))
        //            {
        //                e.Row.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim();
        //            }
        //            __nat02context.Dispose();
        //        }
        //        else if (finished)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.GreenYellow);
        //        }
        //        else if (submitted)
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#0A7DFF"));
        //        }
        //        else if (drawn)
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#52A3FF"));
        //        }
        //        else if (started)
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    {
        //    }
        //}

        //private void TabletProjectsNotStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiTabletProjectsNotStarted rowView = dataGrid.Items[index] as EoiTabletProjectsNotStarted;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if ((bool)rowView.Tools)
        //        {
        //            e.Row.FontWeight = FontWeights.Bold;
        //            e.Row.FontStyle = FontStyles.Oblique;
        //        }
        //        else
        //        {
        //            e.Row.FontWeight = FontWeights.Normal;
        //            e.Row.FontStyle = FontStyles.Normal;
        //        }
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void TabletProjectsStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiTabletProjectsStarted rowView = dataGrid.Items[index] as EoiTabletProjectsStarted;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if ((bool)rowView.Tools)
        //        {
        //            e.Row.FontWeight = FontWeights.Bold;
        //            e.Row.FontStyle = FontStyles.Oblique;
        //        }
        //        else
        //        {
        //            e.Row.FontWeight = FontWeights.Normal;
        //            e.Row.FontStyle = FontStyles.Normal;
        //        }
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void TabletProjectsDrawnDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiTabletProjectsDrawn rowView = dataGrid.Items[index] as EoiTabletProjectsDrawn;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if ((bool)rowView.Tools)
        //        {
        //            e.Row.FontWeight = FontWeights.Bold;
        //            e.Row.FontStyle = FontStyles.Oblique;
        //        }
        //        else
        //        {
        //            e.Row.FontWeight = FontWeights.Normal;
        //            e.Row.FontStyle = FontStyles.Normal;
        //        }
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void TabletProjectsSubmittedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiTabletProjectsSubmitted rowView = dataGrid.Items[index] as EoiTabletProjectsSubmitted;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if ((bool)rowView.Tools)
        //        {
        //            e.Row.FontWeight = FontWeights.Bold;
        //            e.Row.FontStyle = FontStyles.Oblique;
        //        }
        //        else
        //        {
        //            e.Row.FontWeight = FontWeights.Normal;
        //            e.Row.FontStyle = FontStyles.Normal;
        //        }
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void TabletProjectsOnHoldDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiProjectsOnHold rowView = dataGrid.Items[index] as EoiProjectsOnHold;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if ((bool)rowView.Tools)
        //        {
        //            e.Row.FontWeight = FontWeights.Bold;
        //            e.Row.FontStyle = FontStyles.Oblique;
        //        }
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //        e.Row.ToolTip = string.IsNullOrEmpty(rowView.OnHoldComment.Trim()) ? "No Comment" : rowView.OnHoldComment.Trim();
        //    }
        //    catch
        //    { }
        //}

        //private void AllToolProjectsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiAllToolProjectsView rowView = dataGrid.Items[index] as EoiAllToolProjectsView;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        using var _nat02context = new NAT02Context();
        //        bool finished = _nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).Any();
        //        _nat02context.Dispose();
        //        using var _projectscontext = new ProjectsContext();
        //        bool tablet = (bool)_projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().Tablet &&
        //                      _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().TabletCheckedBy.Trim().Length == 0;
        //        bool multi_tip = (bool)_projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().MultiTipSketch;
        //        _projectscontext.Dispose();
        //        bool onHold = rowView.HoldStatus == "On Hold";
        //        bool drawn = rowView.ToolDrawnBy.Trim().Length > 0;
        //        bool started = rowView.ProjectStartedTool.Trim().Length > 0;
        //        e.Row.ToolTip = null;
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (onHold)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.MediumPurple);
        //            using var __nat02context = new NAT02Context();
        //            if (__nat02context.EoiProjectsOnHold.Any(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber))
        //            {
        //                e.Row.ToolTip = string.IsNullOrEmpty(__nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim()) ? "No Comment" : __nat02context.EoiProjectsOnHold.First(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).OnHoldComment.Trim();
        //            }
        //            __nat02context.Dispose();
        //        }
        //        else if (finished)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.GreenYellow);
        //        }
        //        else if (drawn)
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#3594FF"));
        //        }
        //        else if (started)
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#B2D6FF"));
        //        }
        //        else if (multi_tip)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Gray);
        //        }
        //        else if (tablet)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Yellow);
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void ToolProjectsNotStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiToolProjectsNotStarted rowView = dataGrid.Items[index] as EoiToolProjectsNotStarted;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        using var _projectscontext = new ProjectsContext();
        //        bool multi_tip = (bool)_projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == rowView.ProjectNumber && p.RevisionNumber == rowView.RevisionNumber).First().MultiTipSketch;
        //        _projectscontext.Dispose();
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (multi_tip)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Gray);
        //        }
        //        else if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void ToolProjectsStartedDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiToolProjectsStarted rowView = dataGrid.Items[index] as EoiToolProjectsStarted;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void ToolProjectsDrawnDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiToolProjectsDrawn rowView = dataGrid.Items[index] as EoiToolProjectsDrawn;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}

        //private void ToolProjectsOnHoldDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        EoiProjectsOnHold rowView = dataGrid.Items[index] as EoiProjectsOnHold;
        //        bool priority = rowView.MarkedPriority is null ? false : rowView.MarkedPriority == "PRIORITY";
        //        bool late = rowView.DueDate < DateTime.Now.Date;
        //        if (priority)
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //            e.Row.FontWeight = FontWeights.Normal;
        //        }
        //        if (late && User.Department == "Engineering")
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.DarkRed);
        //            e.Row.FontWeight = FontWeights.Bold;
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //        e.Row.ToolTip = string.IsNullOrEmpty(rowView.OnHoldComment.Trim()) ? "No Comment" : rowView.OnHoldComment.Trim();
        //    }
        //    catch
        //    { }
        //}

        //private void NatoliOrderListDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        //{
        //    try
        //    {
        //        DataGrid dataGrid = sender as DataGrid;
        //        int index = e.Row.GetIndex();
        //        NatoliOrderList rowView = dataGrid.Items[index] as NatoliOrderList;
        //        int daysToShip = (rowView.ShipDate.Date - DateTime.Now.Date).Days;
        //        if (daysToShip < 0)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Red);
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //        }
        //        else if (daysToShip == 0)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Orange);
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //        }
        //        else if (daysToShip > 0 && daysToShip < 4)
        //        {
        //            e.Row.Background = new SolidColorBrush(Colors.Yellow);
        //            e.Row.Foreground = new SolidColorBrush(Colors.Black);
        //        }
        //        else
        //        {
        //            e.Row.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
        //        }
        //    }
        //    catch
        //    { }
        //}
        //#endregion276138

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
            string connectionString = @"Data Source=" + App.Server + ";Initial Catalog=Projects;Persist Security Info=True; User ID=" + App.UserID+";Password="+App.Password+"";
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
            string connectionString = @"Data Source=" + App.Server + ";Initial Catalog=Projects;Persist Security Info=True; User ID=" + App.UserID+";Password="+App.Password+"";
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
                                                    if(!System.IO.File.Exists(file.Replace(@"Quotes\" + quote, @"WorkOrders\" + order)))
                                                    {
                                                        System.IO.File.Move(file, file.Replace(@"Quotes\" + quote, @"WorkOrders\" + order));
                                                    }
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
            string connectionString = @"Data Source="+App.Server+";Initial Catalog=NAT01;Persist Security Info=True; User ID="+App.UserID+";Password="+App.Password+"";
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
                Title += " " + string.Format("{0:P2}", (double)devs[1] / devs[0] - 1);
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
                workOrder = new WorkOrder(int.Parse(orderNumber), this);
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
                orderInfoWindow.Show();
                orderInfoWindow.Dispose();
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
                    workOrder = new WorkOrder(int.Parse(orderNumber), this);
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
                    orderInfoWindow.Show();
                    orderInfoWindow.Dispose();
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
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, User)
                {
                    Left = Left,
                    Top = Top
                };
                quoteInfoWindow.Show();
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
                    QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, User)
                    {
                        Left = Left,
                        Top = Top
                    };
                    quoteInfoWindow.Show();
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
        private void OrdersBeingEnteredExpander_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem toOfficeOrder = new MenuItem
            {
                Header = "Send to Office"
            };

            Expander expander = sender as Expander;
            toOfficeOrder.Click += SendToOfficeMenuItem_Click;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            // Set the right click module variable
            rClickModule = "BeingEntered";

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

            // Set the right click module variable
            rClickModule = "EnteredUnscanned";

            _orderNumber = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        }
        private void OrdersInEngineeringUnprintedExpander_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem toOfficeOrder = new MenuItem
            {
                Header = "Send to Office"
            };

            Expander expander = sender as Expander;
            toOfficeOrder.Click += SendToOfficeMenuItem_Click;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            // Set the right click module variable
            rClickModule = "InEngineering";

            RightClickMenu.Items.Add(toOfficeOrder);
            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;
            _orderNumber = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        }
        private void OrdersReadyToPrintExpander_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem toOfficeOrder = new MenuItem
            {
                Header = "Send to Office"
            };

            Expander expander = sender as Expander;
            toOfficeOrder.Click += SendToOfficeMenuItem_Click;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            // Set the right click module variable
            rClickModule = "ReadyToPrint";

            RightClickMenu.Items.Add(toOfficeOrder);
            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;
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
            if (User.Department == "Engineering" || User.Department == "Order Entry")
            {
                RightClickMenu.Items.Add(toProdManOrder);
            }
            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            // Set the right click module variable
            rClickModule = "PrintedInEngineering";

            _orderNumber = double.Parse(((Grid)expander.Header).Children[0].GetValue(ContentProperty).ToString());
        }
        private void OrdersInTheOfficeExpander_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu RightClickMenu = new ContextMenu();

            MenuItem startOrder = new MenuItem
            {
                Header = "Start Order"
            };

            Expander expander = sender as Expander;
            startOrder.Click += StartWorkOrder_Click;
            
            RightClickMenu.Items.Add(startOrder);

            expander.ContextMenu = RightClickMenu;
            expander.ContextMenu.Tag = "RightClickMenu";
            expander.ContextMenu.Closed += ContextMenu_Closed;

            // Check the checkbox for the right-clicked expander
            var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
            ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

            // Set the right click module variable
            rClickModule = "InTheOffice";

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
                    //projectWindow.Show();
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

                // Set the right click module variable
                rClickModule = "AllTabletProjects";

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
                    completedTabletProject.IsEnabled = false;
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
                IMethods.WriteToErrorLog("AllTabletProjectsDataGrid_MouseRightButtonUp", ex.Message, User);
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

                // Set the right click module variable
                rClickModule = "AllToolProjects";

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
                    completedToolProject.IsEnabled = true;
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
                IMethods.WriteToErrorLog("AllToolProjectsDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "TabletProjectsNotStarted";

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
                IMethods.WriteToErrorLog("TabletProjectNotStartedDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "TabletProjectsStarted";

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
                IMethods.WriteToErrorLog("TabletProjectStartedDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "TabletProjectsDrawn";

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
                IMethods.WriteToErrorLog("TabletProjectDrawnDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "TabletProjectsSubmitted";

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
                IMethods.WriteToErrorLog("TabletProjectSubmittedDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "TabletProjectsOnHold";

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
                IMethods.WriteToErrorLog("TabletProjectOnHoldDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "ToolProjectsNotStarted";

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
                IMethods.WriteToErrorLog("ToolProjectNotStartedDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "ToolProjectsStarted";

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
                IMethods.WriteToErrorLog("ToolProjectNotStartedDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "ToolProjectsDrawn";

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
                IMethods.WriteToErrorLog("ToolProjectDrawnDataGrid_MouseRightButtonUp", ex.Message, User);
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

                    // Set the right click module variable
                    rClickModule = "ToolProjectsOnHold";

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
                IMethods.WriteToErrorLog("ToolProjectOnHoldDataGrid_MouseRightButtonUp", ex.Message, User);
            }
        }
        private void QuotesNotConverted_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
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

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    // Set the right click module variable
                    rClickModule = "QuotesNotConverted";

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
                IMethods.WriteToErrorLog("QuoteDataGrid_MouseRightButtonUp", ex.Message, User);
            }
        }
        private void QuotesToConvert_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (User.Department == "Customer Service" || User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E4509")
                {
                    Expander expander = sender as Expander;
                    ContextMenu RightClickMenu = new ContextMenu();

                    MenuItem recallQuote = new MenuItem
                    {
                        Header = "Recall Quote"
                    };
                    recallQuote.Click += RecallQuote_Click;

                    RightClickMenu.Items.Add(recallQuote);

                    // Check the checkbox for the right-clicked expander
                    var x = ((VisualTreeHelper.GetChild(expander as DependencyObject, 0) as Border).Child as DockPanel).Children.OfType<Grid>().First().Children.OfType<Grid>().First();
                    ((VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(x, 0), 0) as Border).Child as Grid).Children.OfType<CheckBox>().First().IsChecked = true;

                    // Set the right click module variable
                    rClickModule = "QuotesToConvert";

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
                IMethods.WriteToErrorLog("QuoteDataGrid_MouseRightButtonUp", ex.Message, User);
            }
        }
        private void ContextMenu_Closed(object sender, RoutedEventArgs e)
        {
            //foreach (StackPanel stackPanel in MainGrid.Children.OfType<StackPanel>())
            //{
            //    try
            //    {
            //        DataGrid dataGrid = stackPanel.Children.OfType<DataGrid>().First();
            //        if (dataGrid.ContextMenu != null && dataGrid.ContextMenu.Tag.ToString() == "RightClickMenu")
            //        {
            //            dataGrid.ContextMenu = null;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        // MessageBox.Show(ex.Message);
            //        IMethods.WriteToErrorLog("ContextMenu_Closed", ex.Message, User);
            //    }
            //}
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
                selectedQuotes.Add((col0val, col1val, checkBox, headers.Single(h => h.Value == header).Key));
            }
            else if (project)
            {
                selectedProjects.Add((col0val, col1val, checkBox, headers.Single(h => h.Value == header).Key));
            }
            else if (order)
            {
                selectedOrders.Add((col0val, checkBox, headers.Single(h => h.Value == header).Key));
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
                    selectedQuotes.Remove((col0val, col1val, checkBox, headers.Single(h => h.Value == header).Key));
                }
                else if (project)
                {
                    selectedProjects.Remove((col0val, col1val, checkBox, headers.Single(h => h.Value == header).Key));
                }
                else if (order)
                {
                    selectedOrders.Remove((col0val, checkBox, headers.Single(h => h.Value == header).Key));
                    foreach (Grid grid in (expander.Content as StackPanel).Children)
                    {
                        (grid.Children[0] as CheckBox).IsChecked = false;
                    }
                }
            }
            catch
            {

            }
        }
        private void GridWindow_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string[] filePathArray = (string[])(e.Data.GetData(DataFormats.FileDrop));
                List<string> filePaths = filePathArray.ToList();
                if (filePaths[0].Contains("WorkOrdersToPrint"))
                {
                    OrderingWindow pDFOrderingWindow = new OrderingWindow(filePaths, User, this);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("MainWindow => GridWindow_Drop", ex.Message, User);
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