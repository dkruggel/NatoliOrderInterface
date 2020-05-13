using F23.StringSimilarity;
using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface;
using NatoliOrderInterface.FolderIntegrity;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.DriveWorks;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.Projects;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Windows.ApplicationModel;
using Windows.Management.Deployment;
using WpfAnimatedGif;
using Colors = System.Windows.Media.Colors;

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
        private bool restart = false;


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
        private readonly System.Timers.Timer moduleSearchTimer = new System.Timers.Timer();
        private readonly System.Timers.Timer updateTimer = new System.Timers.Timer();
        private string searchedFromModuleName = "";
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
        private List<EoiAllOrdersView> ordersBeingEntered = new List<EoiAllOrdersView>();
        private List<EoiAllOrdersView> _ordersBeingEntered = new List<EoiAllOrdersView>();
        public List<EoiAllOrdersView> OrdersBeingEntered
        {
            get
            {
                return ordersBeingEntered;
            }
            set
            {
                if (!ordersBeingEntered.SequenceEqual(value))
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
        private List<EoiAllOrdersView> ordersInTheOffice = new List<EoiAllOrdersView>();
        private List<EoiAllOrdersView> _ordersInTheOffice = new List<EoiAllOrdersView>();
        public List<EoiAllOrdersView> OrdersInTheOffice
        {
            get
            {
                return ordersInTheOffice;
            }
            set
            {
                if (!ordersInTheOffice.SequenceEqual(value))
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
        private List<EoiAllOrdersView> ordersEntered = new List<EoiAllOrdersView>();
        private List<EoiAllOrdersView> _ordersEntered = new List<EoiAllOrdersView>();
        public List<EoiAllOrdersView> OrdersEntered
        {
            get
            {
                return ordersEntered;
            }
            set
            {
                if (!ordersEntered.SequenceEqual(value)) //value.Except(ordersEntered).Count() > 0 || ordersEntered.Except(value).Count() > 0)
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
        private List<EoiAllOrdersView> ordersInEng = new List<EoiAllOrdersView>();
        private List<EoiAllOrdersView> _ordersInEng = new List<EoiAllOrdersView>();
        public List<EoiAllOrdersView> OrdersInEng
        {
            get
            {
                return ordersInEng;
            }
            set
            {
                if (!ordersInEng.SequenceEqual(value))
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
        private List<EoiAllOrdersView> ordersReadyToPrint = new List<EoiAllOrdersView>();
        private List<EoiAllOrdersView> _ordersReadyToPrint = new List<EoiAllOrdersView>();
        public List<EoiAllOrdersView> OrdersReadyToPrint
        {
            get
            {
                return ordersReadyToPrint;
            }
            set
            {
                try
                {
                    List<bool> incBack = new List<bool>();
                    List<bool> outBack = new List<bool>();
                    // Check if data differs
                    if (!ordersReadyToPrint.SequenceEqual(value))
                    {
                        ordersReadyToPrint = value;
                        OrdersReadyToPrintListBox.ItemsSource = null;
                        OrdersReadyToPrintListBox.ItemsSource = ordersReadyToPrint;
                    }
                    // Check if missing prints changed
                    else
                    {
                        // Get current bg colors bool
                        var kid = (VisualTreeHelper.GetChild(((
                               VisualTreeHelper.GetChild(OrdersReadyToPrintListBox, 0) as Border).Child as ScrollViewer).Content as ItemsPresenter, 0) as VirtualizingStackPanel).Children;
                        foreach (UIElement el in kid)
                        {
                            ListBoxItem listBoxItem = el as ListBoxItem;
                            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
                            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                            ToggleButton myToggleButton = (ToggleButton)myDataTemplate.FindName("ToggleButton", myContentPresenter);
                            myToggleButton.ApplyTemplate();
                            Color firstColor = ((VisualTreeHelper.GetChild(myToggleButton, 0) as Grid).Background as LinearGradientBrush).GradientStops[0].Color;
                            incBack.Add(firstColor == Colors.MediumPurple);
                        }

                        // Get future bg colors bool
                        foreach (EoiAllOrdersView order in value)
                        {
                            outBack.Add(CheckIfPrintsAreMissing(order));
                        }
                        if (!incBack.SequenceEqual(outBack))
                        {
                            ordersReadyToPrint = value;
                            OrdersReadyToPrintListBox.ItemsSource = null;
                            OrdersReadyToPrintListBox.ItemsSource = ordersReadyToPrint;
                        }
                    }

                }
                catch (Exception ex)
                {

                }

            }
        }
        #endregion
        #region Orders Printed In Engineering
        private ListBox OrdersPrintedListBox = new ListBox();
        private List<EoiAllOrdersView> ordersPrinted = new List<EoiAllOrdersView>();
        private List<EoiAllOrdersView> _ordersPrinted = new List<EoiAllOrdersView>();
        public List<EoiAllOrdersView> OrdersPrinted
        {
            get
            {
                return ordersPrinted;
            }
            set
            {
                try
                {
                    List<bool> incBack = new List<bool>();
                    List<bool> outBack = new List<bool>();

                    // Check if data differs
                    if (!ordersPrinted.SequenceEqual(value))
                    {
                        ordersPrinted = value;
                        OrdersPrintedListBox.ItemsSource = null;
                        OrdersPrintedListBox.ItemsSource = ordersPrinted;
                    }
                    // Check if missing prints changed
                    else
                    {
                        // Get current bg colors bool
                        var kid = (VisualTreeHelper.GetChild(((
                               VisualTreeHelper.GetChild(OrdersPrintedListBox, 0) as Border).Child as ScrollViewer).Content as ItemsPresenter, 0) as VirtualizingStackPanel).Children;
                        foreach (UIElement el in kid)
                        {
                            ListBoxItem listBoxItem = el as ListBoxItem;
                            ContentPresenter myContentPresenter = FindVisualChild<ContentPresenter>(listBoxItem);
                            DataTemplate myDataTemplate = myContentPresenter.ContentTemplate;
                            ToggleButton myToggleButton = (ToggleButton)myDataTemplate.FindName("ToggleButton", myContentPresenter);
                            myToggleButton.ApplyTemplate();
                            Color firstColor = ((VisualTreeHelper.GetChild(myToggleButton, 0) as Grid).Background as LinearGradientBrush).GradientStops[0].Color;
                            incBack.Add(firstColor == Colors.MediumPurple);
                        }

                        // Get future bg colors bool
                        foreach (EoiAllOrdersView order in value)
                        {
                            outBack.Add(CheckIfPrintsAreMissing(order));
                        }
                        if (!incBack.SequenceEqual(outBack))
                        {
                            ordersPrinted = value;
                            OrdersPrintedListBox.ItemsSource = null;
                            OrdersPrintedListBox.ItemsSource = ordersPrinted;
                        }
                    }




                }
                catch (Exception ex)
                { 

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
                if (!quotesNotConverted.SequenceEqual(value))
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
                if (!quotesToConvert.SequenceEqual(value))
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
                if (!allTabletProjects.SequenceEqual(value))
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
                if (!allToolProjects.SequenceEqual(value))
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
                if (!driveWorksQueue.SequenceEqual(value))
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
                if (!natoliOrderList.SequenceEqual(value))
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
                ChangeZoom(User.Zoom);
                ChangeModuleRows("", User.ModuleRows);
                //MainMenu.Background = SystemParameters.WindowGlassBrush; // Sets it to be the same color as the accent color in Windows
                InitializingMenuItem.Visibility = Visibility.Collapsed;
                InitializeTimers(User);

                if (isDebugMode)
                {
                    if (User.EmployeeCode == "E4754") // Tyler
                    {
                        //ProjectWindow projectWindow = new ProjectWindow("110012", "4", this, User, false);
                        //IMethods.SendProjectCompletedEmailToCSRAsync(new List<string> { "Tyler" }, "103267", "0", new User("twilliams"));
                        //(List<string> errantFolders, List<Tuple<string, string>> renamedFolders) = FolderCheck.CustomerFolderCheck();
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
        private childItem FindVisualChild<childItem>(DependencyObject obj)
    where childItem : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);
                if (child != null && child is childItem)
                {
                    return (childItem)child;
                }
                else
                {
                    childItem childOfChild = FindVisualChild<childItem>(child);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }
        public async void MainRefresh(string module = "")
        {
            if (string.IsNullOrEmpty(module))
            {
                List<Task> taskList = new List<Task>();
                foreach (string mod in User.VisiblePanels)
                {
                    switch (mod)
                    {
                        case "BeingEntered":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetBeingEntered());
                                Dispatcher.BeginInvoke((Action)(() => BindBeingEntered()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "InTheOffice":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetInTheOffice());
                                Dispatcher.BeginInvoke((Action)(() => BindInTheOffice()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "QuotesNotConverted":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetQuotesNotConverted());
                                Dispatcher.BeginInvoke((Action)(() => BindQuotesNotConverted()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "EnteredUnscanned":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetEnteredUnscanned());
                                Dispatcher.BeginInvoke((Action)(() => BindEnteredUnscanned()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "InEngineering":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetInEngineering());
                                Dispatcher.BeginInvoke((Action)(() => BindInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "QuotesToConvert":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetQuotesToConvert());
                                Dispatcher.BeginInvoke((Action)(() => BindQuotesToConvert()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "ReadyToPrint":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetReadyToPrint());
                                Dispatcher.BeginInvoke((Action)(() => BindReadyToPrint()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "PrintedInEngineering":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetPrintedInEngineering());
                                Dispatcher.BeginInvoke((Action)(() => BindPrintedInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "AllTabletProjects":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetAllTabletProjects());
                                Dispatcher.BeginInvoke((Action)(() => BindAllTabletProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "AllToolProjects":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetAllToolProjects());
                                Dispatcher.BeginInvoke((Action)(() => BindAllToolProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "DriveWorksQueue":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetDriveWorksQueue());
                                Dispatcher.BeginInvoke((Action)(() => BindDriveWorksQueue()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        case "NatoliOrderList":
                            taskList.Add(Task.Run(async () =>
                            {
                                await Task.Run(() => GetNatoliOrderList());
                                Dispatcher.BeginInvoke((Action)(() => BindNatoliOrderList()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            }));
                            break;
                        default:
                            break;
                    }
                }
                //await Task.WhenAll(taskList.ToArray());
                //Dispatcher.BeginInvoke((Action)(() =>
                //{
                //    RefreshButton.ApplyTemplate();
                //    var template = RefreshButton.Template;
                //    var image = (Image)template.FindName("Image", RefreshButton);
                //    System.Windows.Media.Animation.BeginStoryboard beginStoryboard = Application.Current.Resources["RotateIt"] as System.Windows.Media.Animation.BeginStoryboard;
                //    Storyboard sb = beginStoryboard.Storyboard;
                //    if (sb.RepeatBehavior == RepeatBehavior.Forever)
                //    {
                //        DoubleAnimation doubleAnimation = sb.Children.OfType<DoubleAnimation>().First() as DoubleAnimation;
                //        doubleAnimation.From = null;
                //        sb.RepeatBehavior = new RepeatBehavior(1.0);
                //        sb.BeginTime = sb.GetCurrentTime(image);
                //        sb.Begin(image, false);
                //    }
                //}),System.Windows.Threading.DispatcherPriority.Normal);
            }
            else
            {
                switch (module)
                {
                    case "BeingEntered":
                        await Task.Run(() => GetBeingEntered());
                        Dispatcher.BeginInvoke((Action)(() => BindBeingEntered()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "InTheOffice":
                        await Task.Run(() => GetInTheOffice());
                        Dispatcher.BeginInvoke((Action)(() => BindInTheOffice()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "QuotesNotConverted":
                        await Task.Run(() => GetQuotesNotConverted());
                        Dispatcher.BeginInvoke((Action)(() => BindQuotesNotConverted()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "EnteredUnscanned":
                        await Task.Run(() => GetEnteredUnscanned());
                        Dispatcher.BeginInvoke((Action)(() => BindEnteredUnscanned()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "InEngineering":
                        await Task.Run(() => GetInEngineering());
                        Dispatcher.BeginInvoke((Action)(() => BindInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "QuotesToConvert":
                        await Task.Run(() => GetQuotesToConvert());
                        Dispatcher.BeginInvoke((Action)(() => BindQuotesToConvert()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "ReadyToPrint":
                        await Task.Run(() => GetReadyToPrint());
                        Dispatcher.BeginInvoke((Action)(() => BindReadyToPrint()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "PrintedInEngineering":
                        await Task.Run(() => GetPrintedInEngineering());
                        Dispatcher.BeginInvoke((Action)(() => BindPrintedInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "AllTabletProjects":
                        await Task.Run(() => GetAllTabletProjects());
                        Dispatcher.BeginInvoke((Action)(() => BindAllTabletProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "AllToolProjects":
                        await Task.Run(() => GetAllToolProjects());
                        Dispatcher.BeginInvoke((Action)(() => BindAllToolProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "DriveWorksQueue":
                        await Task.Run(() => GetDriveWorksQueue());
                        Dispatcher.BeginInvoke((Action)(() => BindDriveWorksQueue()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                        break;
                    case "NatoliOrderList":
                        await Task.Run(() => GetNatoliOrderList());
                        Dispatcher.BeginInvoke((Action)(() => BindNatoliOrderList()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
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
                
                User = new User(Environment.UserName);
                if (isDebugMode)
                {
                    // User = new User("jwillis");
                    // User = new User("mbouzitoun");
                    // User = new User("billt");
                    // User = new User("rfaltus");
                    // User = new User("Pturner");
                    // User = new User("mmulaosmanovic");
                    // User = new User("hwillmuth");
                    // User = new User("kbergerdine");
                }
                App.user = User;
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
            moduleSearchTimer.Elapsed += ModuleSearchTimer_Elapsed;
            moduleSearchTimer.Interval = 0.3 * 1000; // x seconds
            updateTimer.Elapsed += UpdateTimer_Elapsed;
            updateTimer.Interval += (60 * (60 * (1000))); // 1 hour
            updateTimer.Enabled = true;
        }

        

        private void ChangeZoom(decimal? zoom = null)
        {
            try
            {
                // Pull from TextBox
                if (zoom == null)
                {
                    string newText = ZoomTextBox.Text;

                    newText = newText.EndsWith("%") ? newText.Remove(newText.LastIndexOf('%')) : newText;

                    if (double.TryParse(newText, out double scale))
                    {
                        ScaleTransform scaleTransform = (MainDock.Children.OfType<ScrollViewer>().First().Content as Border).LayoutTransform as ScaleTransform;
                        if (scaleTransform.ScaleX != scale / 100)
                        {
                            scaleTransform.ScaleX = scale / 100;
                        }
                        if (scaleTransform.ScaleY != scale / 100)
                        {
                            scaleTransform.ScaleY = scale / 100;
                        }
                        // Make sure there is a percent symbol at the end
                        if (!ZoomTextBox.Text.EndsWith("%"))
                        {
                            ZoomTextBox.Text += "%";
                        }
                        User.Zoom = Convert.ToDecimal(scale);
                    }
                }
                // Use Provided Number
                else
                {
                    double scale = Convert.ToDouble(zoom);
                    ScaleTransform scaleTransform = (MainDock.Children.OfType<ScrollViewer>().First().Content as Border).LayoutTransform as ScaleTransform;
                    if (scaleTransform.ScaleX != scale / 100)
                    {
                        scaleTransform.ScaleX = scale / 100;
                    }
                    if (scaleTransform.ScaleY != scale / 100)
                    {
                        scaleTransform.ScaleY = scale / 100;
                    }
                    ZoomTextBox.Text = scale + "%";
                    User.Zoom = Convert.ToDecimal(scale);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ChangeZoom", ex.Message, User);
            }
            
        }
        private void ChangeModuleRows(string from, int? delta = null)
        {
            int height;
            int maxRows;

            try
            {
                if (delta is null)
                {
                    height = int.Parse(ModuleHeightTextBox.Text);
                }
                else
                {
                    height = (from == "mouse" ? int.Parse(ModuleHeightTextBox.Text) : 0) + ((int)delta);
                }

                maxRows = (int)Math.Floor((MainWrapPanel.ActualHeight - 10 - 102) / 28);
                height = height > maxRows ? maxRows : height < 0 ? 0 : height;
                height = Math.Max(height, 0);
                ModuleHeightTextBox.Text = (height).ToString();
                foreach (Grid grid in MainWrapPanel.Children)
                {
                    // Get state of expand/collapse button to decide if to apply row change event
                    try
                    {
                        (grid.Children[0] as Label).MaxHeight = (28 * height) + 102;
                    }
                    catch (Exception ex)
                    {
                        IMethods.WriteToErrorLog("ChangeModuleRows => Setting MaxHeight //// height: " + height + " // maxRows: " + maxRows + " // delta: " + delta + " // from: " + from + " ////", ex.Message, User);
                    }
                }

                User.ModuleRows = (short)height;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ChangeModuleRows", ex.Message, User);
            }
        }
        #region Main Window Events
        private void GridWindow_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            
#endif
        }
        private void GridWindow_ContentRendered(object sender, EventArgs e)
        {
            // ConstructExpanders();
            List<string> timers = new List<string>();
            timers.Add("Main");
            if (User.VisiblePanels.Contains("QuotesNotConverted"))
            {
                timers.Add("QuotesNotConverted");
            }
            if (User.VisiblePanels.Contains("NatoliOrderList"))
            {
                timers.Add("NatoliOrderList");
            }
            Task t = new Task(()=>GetData(timers));
            t.Start();
            t.Wait();

            Dispatcher.BeginInvoke((Action)UpdateUI, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            Dispatcher.BeginInvoke((Action)SetNotificationPicture, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        
        private async void MainTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool active = false;
            bool applicationActive = true;
            await Dispatcher.BeginInvoke((Action)(()=>
            {
                active = IMethods.IsActive(this as Window);
                applicationActive = IMethods.IsApplicationActive();
            }));
            if (!applicationActive || active)
            {
                await Task.Run(() => GetData(new List<string> { "Main" }));
                Dispatcher.BeginInvoke((Action)UpdateUI, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }
        private async void QuoteTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool active = false;
            bool applicationActive = true;
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                active = IMethods.IsActive(this as Window);
                applicationActive = IMethods.IsApplicationActive();
            }));
            if (!applicationActive || active)
            {
                if (User.VisiblePanels.Contains("QuotesNotConverted"))
                {
                    await Task.Run(() => GetData(new List<string> { "QuotesNotConverted" }));
                    Dispatcher.BeginInvoke((Action)UpdateUI, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    Dispatcher.BeginInvoke((Action)SetNotificationPicture, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }
            }
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
        private async void NatoliOrderListTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            bool active = false;
            bool applicationActive = true;
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                active = IMethods.IsActive(this as Window);
                applicationActive = IMethods.IsApplicationActive();
            }));
            if (!applicationActive || active)
            {
                if (User.VisiblePanels.Contains("NatoliOrderList"))
                {
                    await Task.Run(() => GetData(new List<string> { "NatoliOrderList" }));
                    Dispatcher.BeginInvoke((Action)UpdateUI, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }
            }
            
        }
        private void OQTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754") { GetPercentages(); }
            if (User.Department == "Engineering")
            {
                QuotesAndOrders();
            }
        }
        private void UpdateTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
           updateTimer.Stop();
            try
            {
                System.IO.StreamReader streamReader = new System.IO.StreamReader(@"\\nshare\VB_Apps\NatoliOrderInterface\version.json");
                string version = "";
                while (!streamReader.ReadLine().Contains(':'))
                {
                    version = streamReader.ReadLine().Split(':')[1].Trim('"');
                    streamReader.Dispose();
                    break;
                }
                if (User.PackageVersion != version)
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBoxResult result = MessageBox.Show("There is a new update. Would you like to restart the application to apply?", "New Update for Natoli Order Interface", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.DefaultDesktopOnly);
                        if (result == MessageBoxResult.Yes)
                        {
                            restart = true;
                            Close();
                        }
                    });
                }
                updateTimer.Start();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("UpdateTimer_Elapsed()", ex.Message, User);
                updateTimer.Stop();
                updateTimer.Start();
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
                eoiSettings.Zoom = User.Zoom;
                eoiSettings.ModuleRows = User.ModuleRows;
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
            if(restart == true)
            try
            {
                Process.Start(@"\\nshare\VB_Apps\NatoliOrderInterface\NatoliOrderInterfaceLauncher.exe");
            }
            catch(Exception ex)
            {
                IMethods.WriteToErrorLog("GridWindow_Closing", ex.Message, User);
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
        private void GridWindow_Deactivated(object sender, EventArgs e)
        {
            //mainTimer.Stop();
            //quoteTimer.Stop();
            //NatoliOrderListTimer.Stop();
        }
        private void GridWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                mainTimer.Stop();
                quoteTimer.Stop();
                NatoliOrderListTimer.Stop();
            }
            else
            {
                if (!mainTimer.Enabled)
                {
                    mainTimer.Start();
                }
                if (!quoteTimer.Enabled)
                {
                    quoteTimer.Start();
                }
                if (!NatoliOrderListTimer.Enabled)
                {
                    NatoliOrderListTimer.Start();
                }
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
                ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).ScrollToHorizontalOffset(currPos - 655);
            }
            else if (e.Key == Key.Right)
            {

                double currPos = ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).HorizontalOffset;
                ((MainWrapPanel.Parent as Border).Parent as ScrollViewer).ScrollToHorizontalOffset(currPos + 655);
            }
            else if (e.Key == Key.Enter && ZoomTextBox.IsFocused)
            {
                ChangeZoom();
            }
            else if (e.Key == Key.Enter && ModuleHeightTextBox.IsFocused)
            {
                ChangeModuleRows("");
            }
        }
        private void ModuleHeightTextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ChangeModuleRows("mouse", e.Delta / 120);
        }
        private void ZoomTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            ChangeZoom();
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
            //if (User.EmployeeCode == "E4754") { fileMenu.Items.Add(createProject); }
            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.Department == "Customer Service")
            { 
                fileMenu.Items.Add(createProject); 
            }

            MenuItem projectSearch = new MenuItem()
            {
                Header = "Project Search",
                ToolTip = "Search for old engineering projects."
            };
            projectSearch.Click += ProjectSearch_Click;
            fileMenu.Items.Add(projectSearch);

            MenuItem forceRefresh = new MenuItem
            {
                Header = "Force Refresh",
                ToolTip = "Bypass the refresh timer."
            };
            forceRefresh.Click += ForceRefresh_Click;
            fileMenu.Items.Add(forceRefresh);

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
        public void SetNotificationPicture()
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                // Check for new notifications
                using var _nat02context = new NAT02Context();
                try
                {
                    int active = _nat02context.EoiNotificationsActive.Where(n => n.User == User.DomainName).Count();
                    if (active > 0)
                    {
                        NotificationButton.Tag = App.Current.Resources["bell_alt_ringDrawingImage"] as DrawingImage;
                    }
                    else
                    {
                        NotificationButton.Tag = App.Current.Resources["bellDrawingImage"] as DrawingImage;
                    }
                }
                catch (Exception ex)
                {

                }
                _nat02context.Dispose();
            }
            ), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }
        private void PrintDrawings_Click(object sender, RoutedEventArgs e)
        {
            // Opens folder with drawings
            string path = @"C:\Users\" + User.DomainName + @"\Desktop\WorkOrdersToPrint\";
            string newPath = @"\\nshare\users\" + User.DomainName + @"\WorkOrdersToPrint\";

            if (User.EmployeeCode == "E4408")
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", newPath);

                //MessageBoxResult res = MessageBox.Show("Delete pdfs?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                //switch (res)
                //{
                //    case MessageBoxResult.Yes:
                //        foreach (string file in System.IO.Directory.GetFiles(newPath).AsEnumerable().OrderBy(f => f))
                //        {
                //            System.IO.File.Delete(file);
                //        }
                //        break;
                //    case MessageBoxResult.No:
                //        break;
                //    default:
                //        break;
                //}
            }
            else
            {
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
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
                SizeToContent = SizeToContent.Height,
                Title = "Add New Module"
            };

            DockPanel dockPanel = new DockPanel();
            dockPanel.LastChildFill = false;
            ListBox listBox = new ListBox();
            DockPanel.SetDock(listBox, Dock.Top);
            listBox.SelectionChanged += ListBox_SelectionChanged;
            listBox.PreviewMouseDoubleClick += AddNewModule_Click;
            using var _ = new NAT02Context();
            List<string> visiblePanels = _.EoiSettings.Single(s => s.DomainName == User.DomainName).Panels.Split(',').ToList();
            _.Dispose();
            List<string> possiblePanels = IMethods.GetPossiblePanels(User);
            listBox.ItemsSource = possiblePanels.Except(visiblePanels).Where(p => !string.IsNullOrEmpty(p));

            dockPanel.Children.Add(listBox);
            addModuleWindow.Content = dockPanel;
            addModuleWindow.Show();
        }
        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DockPanel dockPanel = (sender as ListBox).Parent as DockPanel;
            Window w = dockPanel.Parent as Window;
            //dockPanel.Children.Remove(dockPanel.Children.OfType<Button>().First());
            if (!dockPanel.Children.OfType<Button>().Any())
            {

                Button button = new Button()
                {
                    Content = "Add Module",
                    Margin = new Thickness(10),
                    Style = Application.Current.Resources["Button"] as Style,
                    Width = 150
                };
                DockPanel.SetDock(button, Dock.Top);
                dockPanel.Children.Add(button);
                button.Click += AddNewModule_Click;
            }
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
        private void ForceRefresh_Click(object sender, RoutedEventArgs e)
        {
            MainRefresh();
        }
        private void CreateProject_Click(object sender, RoutedEventArgs e)
        {
            // Select max project number
            using var projectsContext = new ProjectsContext();
            int engProjMax = projectsContext.EngineeringProjects.Any() ? Convert.ToInt32(projectsContext.EngineeringProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber)).First().ProjectNumber) + 1 : 0;
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
                        w.WindowState = WindowState.Normal;
                        w.Show();
                        goto AlreadyOpen;
                    }
                }
                if (nat01context.QuoteHeader.Any(q => q.QuoteNo == double.Parse(quoteNumber) && q.QuoteRevNo == short.Parse(revNumber)))
                {
                    quote = new Quote(int.Parse(quoteNumber), short.Parse(revNumber));
                    QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, this, User)
                    {
                        Left = Left,
                        Top = Top
                    };
                    quoteInfoWindow.Show();
                }
                mainTimer.Stop();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                IMethods.WriteToErrorLog("QuoteSearchButton_Click", ex.Message, User);
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
                    string revNo = "0";
                    if (_projectscontext.ProjectSpecSheet.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)))
                    {
                        revNo = _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)).Max(p => p.RevisionNumber).ToString();
                    }
                    else if (_projectscontext.EngineeringProjects.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text).ToString()))
                    {
                        revNo = _projectscontext.EngineeringProjects.Where(p => Convert.ToInt32(p.ProjectNumber) == int.Parse(ProjectSearchTextBlock.Text)).Max(p => Convert.ToInt32(p.RevNumber)).ToString();
                    }
                    else if (_projectscontext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text).ToString()))
                    {
                        revNo = _projectscontext.EngineeringArchivedProjects.Where(p => Convert.ToInt32(p.ProjectNumber) == int.Parse(ProjectSearchTextBlock.Text)).Max(p => Convert.ToInt32(p.RevNumber)).ToString();
                    }


                    ProjectRevNoSearchTextBlock.Text = revNo;
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
                if (_projectesContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == revNumber) || _projectesContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == revNumber))
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
                    string revNo = "0";
                    if (ProjectSearchTextBlock.Text.Length > 0 && ProjectRevNoSearchTextBlock.Text.Length == 0)
                    {
                        using var _projectscontext = new ProjectsContext();
                        if (_projectscontext.ProjectSpecSheet.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)))
                        {
                            revNo = _projectscontext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text)).Max(p => p.RevisionNumber).ToString();
                        }
                        else if (_projectscontext.EngineeringProjects.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text).ToString()))
                        {
                            revNo = _projectscontext.EngineeringProjects.Where(p => Convert.ToInt32(p.ProjectNumber) == int.Parse(ProjectSearchTextBlock.Text)).Max(p => Convert.ToInt32(p.RevNumber)).ToString();
                        }
                        else if (_projectscontext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == int.Parse(ProjectSearchTextBlock.Text).ToString()))
                        {
                            revNo = _projectscontext.EngineeringArchivedProjects.Where(p => Convert.ToInt32(p.ProjectNumber) == int.Parse(ProjectSearchTextBlock.Text)).Max(p => Convert.ToInt32(p.RevNumber)).ToString();
                        }
                        ProjectRevNoSearchTextBlock.Text = revNo;
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

                    Task.Run(() => GetBeingEntered()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindBeingEntered()),System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetInTheOffice()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindInTheOffice()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetQuotesNotConverted()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindQuotesNotConverted()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetEnteredUnscanned()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindEnteredUnscanned()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetInEngineering()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetQuotesToConvert()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindQuotesToConvert()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetReadyToPrint()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindReadyToPrint()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetPrintedInEngineering()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindPrintedInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindAllTabletProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

                    AllTabletProjectsListBox.ItemsSource = null;
                    AllTabletProjectsListBox.ItemsSource = allTabletProjects;
                    return grid;
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

                    Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindAllToolProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

                    AllToolProjectsListBox.ItemsSource = null;
                    AllToolProjectsListBox.ItemsSource = allToolProjects;
                    return grid;
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

                    Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindDriveWorksQueue()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

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

                    Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindNatoliOrderList()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

                    NatoliOrderListListBox.ItemsSource = null;
                    NatoliOrderListListBox.ItemsSource = natoliOrderList;
                    return grid;
                default:
                    return grid;
            }
        }
        private void GetData(List<string> timers)
        {
            try
            {
                List<Task> tasks = new List<Task>();
                foreach (string panel in User.VisiblePanels)
                {
                    foreach (string timer in timers)
                    {
                        switch (panel, timer)
                        {
                            case ("BeingEntered", "Main"):
                                tasks.Add(new Task(() => GetBeingEntered()));
                                break;
                            case ("InTheOffice", "Main"):
                                tasks.Add(new Task(() => GetInTheOffice()));
                                break;
                            case ("QuotesNotConverted", "QuotesNotConverted"):
                                tasks.Add(new Task(() => GetQuotesNotConverted()));
                                break;
                            case ("EnteredUnscanned", "Main"):
                                tasks.Add(new Task(() => GetEnteredUnscanned()));
                                break;
                            case ("InEngineering", "Main"):
                                tasks.Add(new Task(() => GetInEngineering()));
                                break;
                            case ("QuotesToConvert", "Main"):
                                tasks.Add(new Task(() => GetQuotesToConvert()));
                                break;
                            case ("ReadyToPrint", "Main"):
                                tasks.Add(new Task(() => GetReadyToPrint()));
                                break;
                            case ("PrintedInEngineering", "Main"):
                                tasks.Add(new Task(() => GetPrintedInEngineering()));
                                break;
                            case ("AllTabletProjects", "Main"):
                                tasks.Add(new Task(() => GetAllTabletProjects()));
                                break;
                            case ("AllToolProjects", "Main"):
                                tasks.Add(new Task(() => GetAllToolProjects()));
                                break;
                            case ("DriveWorksQueue", "Main"):
                                tasks.Add(new Task(() => GetDriveWorksQueue()));
                                break;
                            case ("NatoliOrderList", "NatoliOrderList"):
                                tasks.Add(new Task(() => GetNatoliOrderList()));
                                break;
                            default:
                                break;
                        }
                    }
                }
                Task t = Task.WhenAll(tasks);
                Parallel.ForEach(tasks, task => task.Start());
                t.Wait();
            }
            catch
            {

            }
        }
        private void UpdateUI()
        {
            try
            {
                foreach (string mod in User.VisiblePanels)
                {
                    switch (mod)
                    {
                        case "BeingEntered":
                            Dispatcher.BeginInvoke((Action)(() => BindBeingEntered()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "InTheOffice":
                            Dispatcher.BeginInvoke((Action)(() => BindInTheOffice()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "QuotesNotConverted":
                            Dispatcher.BeginInvoke((Action)(() => BindQuotesNotConverted()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "EnteredUnscanned":
                            Dispatcher.BeginInvoke((Action)(() => BindEnteredUnscanned()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "InEngineering":
                            Dispatcher.BeginInvoke((Action)(() => BindInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "QuotesToConvert":
                            Dispatcher.BeginInvoke((Action)(() => BindQuotesToConvert()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "ReadyToPrint":
                            Dispatcher.BeginInvoke((Action)(() => BindReadyToPrint()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "PrintedInEngineering":
                            Dispatcher.BeginInvoke((Action)(() => BindPrintedInEngineering()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "AllTabletProjects":
                            Dispatcher.BeginInvoke((Action)(() => BindAllTabletProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "AllToolProjects":
                            Dispatcher.BeginInvoke((Action)(() => BindAllToolProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "DriveWorksQueue":
                            Dispatcher.BeginInvoke((Action)(() => BindDriveWorksQueue()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        case "NatoliOrderList":
                            Dispatcher.BeginInvoke((Action)(() => BindNatoliOrderList()), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                            break;
                        default:
                            break;
                    }
                }
                //Dispatcher.BeginInvoke((Action)BindQuotesNotConverted, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindQuotesToConvert, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindBeingEntered, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindInTheOffice, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindEnteredUnscanned, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindInEngineering, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindReadyToPrint, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindPrintedInEngineering, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindAllTabletProjects, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindAllToolProjects, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindDriveWorksQueue, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                //Dispatcher.BeginInvoke((Action)BindNatoliOrderList, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
            catch (Exception ex)
            {
                
            }
        }
        private bool CheckIfPrintsAreMissing(EoiAllOrdersView order)
        {
            if (order.Tablet == 1 || order.Tool == 1 || order.Tm2 == 1)
            {
                bool tm2 = System.Convert.ToBoolean(order.Tm2);
                bool tabletPrints = System.Convert.ToBoolean(order.Tablet);
                bool toolPrints = System.Convert.ToBoolean(order.Tool);
                List<string> hobNumbers = null;
                hobNumbers = !string.IsNullOrEmpty(order.HobNumbers) && !string.IsNullOrEmpty(order.HobNumbers) ? order.HobNumbers.Split(",").ToList() : null;
                if (tm2 || tabletPrints)
                {
                    foreach (string hobNumber in hobNumbers)
                    {
                        string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNumber + @"\" + hobNumber + ".pdf";
                        if (!System.IO.File.Exists(path))
                        {
                            return true;
                        }
                    }
                }

                if (tm2 || toolPrints)
                {
                    List<string> detailTypes = null;
                    detailTypes = !string.IsNullOrEmpty(order.DetailTypes) ? order.DetailTypes.Split(",").ToList() : null;
                    foreach (string detailTypeID in detailTypes)
                    {
                        if (detailTypeID == "U" || detailTypeID == "L" || detailTypeID == "D" || detailTypeID == "DS" || detailTypeID == "R")
                        {
                            string detailType = oeDetailTypes[detailTypeID];
                            detailType = detailType == "MISC" ? "REJECT" : detailType;
                            string international = order.UnitOfMeasure;
                            string path = @"\\engserver\workstations\tool_drawings\" + order.OrderNumber + @"\" + detailType + ".pdf";
                            if (!System.IO.File.Exists(path))
                            {
                                return true;
                            }
                            if (international == "M" && !System.IO.File.Exists(path.Replace(detailType, detailType + "_M")))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }
        private Grid GetHeaderGridFromListBox(ListBox listBox)
        {
            Grid displayGrid = listBox.Parent as Grid;
            return displayGrid.Children.OfType<Grid>().First(g => g.Name == "HeaderGrid");
        }
        #region Gets And Binds
        private void GetBeingEntered()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersBeingEntered = _.EoiAllOrdersView.Where(o => o.BeingEntered == 1).OrderBy(o => o.OrderNumber).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindBeingEntered()
        {
            string searchString = GetSearchString("BeingEntered");
            string column;
            var _filtered = _ordersBeingEntered;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":
                        _filtered =
                            _ordersBeingEntered.Where(o => o.OrderNumber.ToString().ToLower().Contains(searchString))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
                        break;
                    case "customer name":
                        _filtered =
                            _ordersBeingEntered.Where(o => !string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchString))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
                        break;
                    case "quote no":
                        _filtered =
                            _ordersBeingEntered.Where(o => !string.IsNullOrEmpty(o.QuoteNumber.ToString()) && o.QuoteNumber.ToString().ToLower().Contains(searchString))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
                        break;
                    default:
                        _filtered =
                            _ordersBeingEntered.Where(o => o.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                               o.OrderNumber.ToString().Contains(searchString) ||
                                               (!string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchString)))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _ordersBeingEntered.Where(o => o.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                               o.QuoteNumber.ToString().Contains(searchString) ||
                                               (!string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchString)))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
            }
            
            Grid headerGrid = GetHeaderGridFromListBox(OrdersBeingEnteredListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                
                if (tb.IsChecked == false)
                { 
                    switch(tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered.OrderBy(o => o.OrderNumber).ToList();
                            break;
                        case "Quote #":
                            _filtered = _filtered.OrderBy(o => o.QuoteNumber).ToList();
                            break;
                        case "Rev #":
                            _filtered = _filtered.OrderBy(o => o.QuoteRev).ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered.OrderBy(o => o.CustomerName).ToList();
                            break;
                        case "Ships In":
                            _filtered = _filtered.OrderBy(o => o.NumDaysToShip).ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if(tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered.OrderByDescending(o => o.OrderNumber).ToList();
                            break;
                        case "Quote #":
                            _filtered = _filtered.OrderByDescending(o => o.QuoteNumber).ToList();
                            break;
                        case "Rev #":
                            _filtered = _filtered.OrderByDescending(o => o.QuoteRev).ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered.OrderByDescending(o => o.CustomerName).ToList();
                            break;
                        case "Ships In":
                            _filtered = _filtered.OrderByDescending(o => o.NumDaysToShip).ToList();
                            break;
                        default:
                            break;
                    }
                }
                
            }
            OrdersBeingEntered = _filtered;
        }
        private void GetInTheOffice()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersInTheOffice = _.EoiAllOrdersView.Where(o => o.InTheOffice == 1).OrderBy(o => o.OrderNumber).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindInTheOffice()
        {
            string searchString = GetSearchString("InTheOffice");
            string column;
            var _filtered = _ordersInTheOffice;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":
                        _filtered =
                            _ordersInTheOffice.Where(o => o.OrderNumber.ToString().ToLower().Contains(searchString))
                                  .OrderBy(o => o.NumDaysToShip)
                                  .ThenBy(o => o.DaysInDept)
                                  .ThenBy(o => o.OrderNumber)
                                  .ToList();
                        break;
                    case "customer name":
                        _filtered =
                            _ordersInTheOffice.Where(o => !string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchString))
                                  .OrderBy(o => o.NumDaysToShip)
                                  .ThenBy(o => o.DaysInDept)
                                  .ThenBy(o => o.OrderNumber)
                                  .ToList();
                        break;
                    case "employee name":
                        _filtered =
                            _ordersInTheOffice.Where(o => !string.IsNullOrEmpty(o.EmployeeName) && o.EmployeeName.ToLower().Contains(searchString))
                                  .OrderBy(o => o.NumDaysToShip)
                                  .ThenBy(o => o.DaysInDept)
                                  .ThenBy(o => o.OrderNumber)
                                  .ToList();
                        break;
                    default:
                        _filtered =
                            _ordersInTheOffice.Where(o => o.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                              (!string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchString)) ||
                                              (!string.IsNullOrEmpty(o.EmployeeName) && o.EmployeeName.ToLower().Contains(searchString)) ||
                                              (!string.IsNullOrEmpty(o.Csr) && o.Csr.ToLower().Contains(searchString)))
                                  .OrderBy(o => o.NumDaysToShip)
                                  .ThenBy(o => o.DaysInDept)
                                  .ThenBy(o => o.OrderNumber)
                                  .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _ordersInTheOffice.Where(o => o.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                              (!string.IsNullOrEmpty(o.CustomerName) && o.CustomerName.ToLower().Contains(searchString)) ||
                                              (!string.IsNullOrEmpty(o.EmployeeName) && o.EmployeeName.ToLower().Contains(searchString)) ||
                                              (!string.IsNullOrEmpty(o.Csr) && o.Csr.ToLower().Contains(searchString)))
                                  .OrderBy(o => o.NumDaysToShip)
                                  .ThenBy(o => o.DaysInDept)
                                  .ThenBy(o => o.OrderNumber)
                                  .ToList();
            }
            Grid headerGrid = GetHeaderGridFromListBox(OrdersInTheOfficeListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderBy(o => o.OrderNumber)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList(); 
                            break;
                        case "Ships":
                            _filtered = _filtered
                                .OrderBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Office":
                            _filtered = _filtered
                                .OrderBy(o => o.DaysInDept)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderBy(o => o.EmployeeName)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "CSR":
                            _filtered = _filtered
                                .OrderBy(o => o.Csr)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.OrderNumber)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Ships":
                            _filtered = _filtered
                                .OrderByDescending(o => o.NumDaysToShip)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Office":
                            _filtered = _filtered
                                .OrderByDescending(o => o.DaysInDept)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderByDescending(o => o.EmployeeName)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "CSR":
                            _filtered = _filtered
                                .OrderByDescending(o => o.Csr)
                                .ThenBy(o => o.CustomerName)
                                .ThenBy(o => o.NumDaysToShip)
                                .ThenBy(o => o.DaysInDept)
                                .ThenBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            OrdersInTheOffice = _filtered;
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
        public void BindQuotesNotConverted()
        {
            string searchString = GetSearchString("QuotesNotConverted");

            var _filtered = _quotesNotConverted;
            if (searchString.ToLower().StartsWith("rep:"))
            {
                searchString = searchString.Substring(4);
                _filtered =
                _quotesNotConverted.Where(p => !string.IsNullOrEmpty(p.RepId) && p.RepId.ToLower().Trim() == searchString)
                                   .OrderByDescending(kvp => kvp.QuoteNo)
                                   .ToList();
            }
            else
            {
                _filtered =
                _quotesNotConverted.Where(p => p.QuoteNo.ToString().ToLower().Contains(searchString) ||
                                               p.QuoteRevNo.ToString().ToLower().Contains(searchString) ||
                                               (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                               (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)))
                                   .OrderByDescending(kvp => kvp.QuoteNo)
                                   .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(QuotesNotConvertedListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Quote #":
                            _filtered = _filtered
                                .OrderBy(o => o.QuoteNo)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        case "Rev #":
                            _filtered = _filtered
                                .OrderBy(o => o.QuoteRevNo)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderBy(o => o.Csr)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Quote #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.QuoteNo)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        case "Rev #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.QuoteRevNo)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderByDescending(o => o.Csr)
                                .ThenByDescending(kvp => kvp.QuoteNo)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }


            QuotesNotConverted = _filtered;
        }
        private void GetEnteredUnscanned()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersEntered = _.EoiAllOrdersView.Where(o => o.EnteredUnscanned == 1).OrderBy(o => o.OrderNumber).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindEnteredUnscanned()
        {
            string searchString = GetSearchString("EnteredUnscanned");

            string column;
            var _filtered = _ordersEntered;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":
                        _filtered =
                            _ordersEntered.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString))
                            .OrderBy(kvp => kvp.OrderNumber)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ToList();
                        break;
                    case "customer name":
                        _filtered =
                            _ordersEntered.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                            .OrderBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNumber)
                            .ToList();
                        break;
                    default:
                        _filtered =
                            _ordersEntered.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                          (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)))
                              .OrderBy(kvp => kvp.NumDaysToShip)
                              .ThenBy(kvp => kvp.OrderNumber)
                              .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _ordersEntered.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                          (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)))
                              .OrderBy(kvp => kvp.NumDaysToShip)
                              .ThenBy(kvp => kvp.OrderNumber)
                              .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(OrdersEnteredListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderBy(o => o.OrderNumber)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships In":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Days In":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.OrderNumber)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships In":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Days In":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            OrdersEntered = _filtered;
        }
        private void GetInEngineering()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersInEng = _.EoiAllOrdersView.Where(o => o.InEngineering == 1).OrderByDescending(o => o.DaysInDept).ThenBy(o => o.NumDaysToShip).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindInEngineering()
        {
            string searchString = GetSearchString("InEngineering");

            string column;
            var _filtered = _ordersInEng;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":

                        _filtered =
                            _ordersInEng.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString))
                            .OrderByDescending(kvp => kvp.DaysInDept)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNumber)
                            .ToList();
                        break;
                    case "customer name":

                        _filtered =
                            _ordersInEng.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                            .OrderByDescending(kvp => kvp.DaysInDept)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNumber)
                            .ToList();
                        break;
                    case "employee name":

                        _filtered =
                            _ordersInEng.Where(p => !string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString))
                            .OrderByDescending(kvp => kvp.DaysInDept)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNumber)
                            .ToList();
                        break;
                    default:

                        _filtered =
                            _ordersInEng.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                        (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                        (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString)))
                            .OrderByDescending(kvp => kvp.DaysInDept)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNumber)
                            .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _ordersInEng.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                        (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                        (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString)))
                            .OrderByDescending(kvp => kvp.DaysInDept)
                            .ThenBy(kvp => kvp.NumDaysToShip)
                            .ThenBy(kvp => kvp.OrderNumber)
                            .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(OrdersInEngListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderBy(o => o.OrderNumber)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ToList();
                                break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships In":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.NumDaysToShip)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "In Eng":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderBy(o => o.EmployeeName)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.OrderNumber)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships In":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.NumDaysToShip)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "In Eng":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderByDescending(o => o.EmployeeName)
                                .ThenByDescending(kvp => kvp.DaysInDept)
                                .ThenBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            OrdersInEng = _filtered;
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
        public void BindQuotesToConvert()
        {
            string searchString = GetSearchString("QuotesToConvert");

            string column;
            var _filtered = _quotesToConvert;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "quote no":

                        _filtered =
                            _quotesToConvert.Where(p => p.QuoteNo.ToString().ToLower().Contains(searchString))
                            .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                        break;
                    case "rev":

                        _filtered =
                            _quotesToConvert.Where(p => p.QuoteRevNo.ToString().ToLower().Contains(searchString))
                            .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                        break;
                    case "customer name":

                        _filtered =
                            _quotesToConvert.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                            .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                        break;
                    case "employee name":

                        _filtered =
                            _quotesToConvert.Where(p => !string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString))
                            .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                        break;
                    case "days in":

                        _filtered =
                            _quotesToConvert.Where(p => p.DaysMarked != null && p.DaysMarked.ToString()==searchString)
                            .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                        break;
                    default:

                        _filtered =
                            _quotesToConvert.Where(p => p.QuoteNo.ToString().ToLower().Contains(searchString) ||
                                            p.QuoteRevNo.ToString().ToLower().Contains(searchString) ||
                                            (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)))
                                .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _quotesToConvert.Where(p => p.QuoteNo.ToString().ToLower().Contains(searchString) ||
                                            p.QuoteRevNo.ToString().ToLower().Contains(searchString) ||
                                            (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)))
                                .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(QuotesToConvertListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Quote #":
                            _filtered = _filtered
                                .OrderBy(o => o.QuoteNo)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Rev":
                            _filtered = _filtered
                                .OrderBy(o => o.QuoteRevNo)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Employee Name":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.Csr)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Days In":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Quote #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.QuoteNo)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Rev":
                            _filtered = _filtered
                                .OrderByDescending(o => o.QuoteRevNo)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Employee Name":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.Csr)
                                .ThenBy(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        case "Days In":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.TimeSubmitted)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }
            QuotesToConvert = _filtered;
        }
        private void GetReadyToPrint()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersReadyToPrint = _.EoiAllOrdersView.Where(o => o.ReadyToPrint == 1).OrderBy(o => o.OrderNumber).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindReadyToPrint()
        {
            string searchString = GetSearchString("ReadyToPrint");

            string column;
            var _filtered = _ordersReadyToPrint;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":

                        _filtered =
                            _ordersReadyToPrint.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    case "customer name":

                        _filtered =
                            _ordersReadyToPrint.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    case "employee name":

                        _filtered =
                            _ordersReadyToPrint.Where(p => !string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    case "checker":

                        _filtered =
                            _ordersReadyToPrint.Where(p => (!string.IsNullOrEmpty(p.CheckedBy) && p.CheckedBy.ToLower().Contains(searchString)))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    default:

                        _filtered =
                            _ordersReadyToPrint.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                               (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                               (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString)) ||
                                               (!string.IsNullOrEmpty(p.CheckedBy) && p.CheckedBy.ToLower().Contains(searchString)))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _ordersReadyToPrint.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                               (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                               (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString)) ||
                                               (!string.IsNullOrEmpty(p.CheckedBy) && p.CheckedBy.ToLower().Contains(searchString)))
                                   .OrderBy(kvp => kvp.OrderNumber)
                                   .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(OrdersReadyToPrintListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.EmployeeName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Checker":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.CheckedBy)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.EmployeeName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Checker":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.CheckedBy)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            OrdersReadyToPrint = _filtered;
        }
        public void GetPrintedInEngineering()
        {
            try
            {
                using var _ = new NAT02Context();
                _ordersPrinted = _.EoiAllOrdersView.Where(o => o.Printed == 1).OrderBy(o => o.OrderNumber).ToList();
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
            var _filtered = _ordersPrinted;
            if (searchString.Contains(":"))
            {
                column = searchString.Split(':')[0];
                searchString = searchString.Split(':')[1].Trim();
                switch (column)
                {
                    case "order no":

                        _filtered =
                            _ordersPrinted.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    case "customer name":

                        _filtered =
                            _ordersPrinted.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    case "employee name":

                        _filtered =
                            _ordersPrinted.Where(p => !string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    case "checker":

                        _filtered =
                            _ordersPrinted.Where(p => !string.IsNullOrEmpty(p.CheckedBy) && p.CheckedBy.ToLower().Contains(searchString))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                    default:

                        _filtered =
                            _ordersPrinted.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                                      (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                      (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString)) ||
                                                      (!string.IsNullOrEmpty(p.CheckedBy) && p.CheckedBy.ToLower().Contains(searchString)))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
                        break;
                }
            }
            else
            {
                _filtered =
                            _ordersPrinted.Where(p => p.OrderNumber.ToString().ToLower().Contains(searchString) ||
                                                      (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                      (!string.IsNullOrEmpty(p.EmployeeName) && p.EmployeeName.ToLower().Contains(searchString)) ||
                                                      (!string.IsNullOrEmpty(p.CheckedBy) && p.CheckedBy.ToLower().Contains(searchString)))
                                          .OrderBy(kvp => kvp.OrderNumber)
                                          .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(OrdersPrintedListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderBy(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.EmployeeName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Checker":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.CheckedBy)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.OrderNumber)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Ships":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.NumDaysToShip)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Employee":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.EmployeeName)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        case "Checker":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.CheckedBy)
                                .ThenBy(kvp => kvp.OrderNumber)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            OrdersPrinted = _filtered;
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
                    if (User.DomainName == "mmulaosmanovic")
                    {
                        _allTabletProjects = eoiAllTabletProjects.Where(p => !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                               .OrderByDescending(p => p.Complete).ThenByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                    else
                    {
                        _allTabletProjects = eoiAllTabletProjects.Where(p =>!_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                               .OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                }
                else
                {
                    if (User.DomainName == "mmulaosmanovic")
                    {
                        _allTabletProjects = eoiAllTabletProjects.OrderByDescending(p => p.Complete).ThenByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                    else
                    {
                        _allTabletProjects = eoiAllTabletProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                }
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindAllTabletProjects()
        {
            string searchString = GetSearchString("AllTabletProjects");

            string column;
            var _filtered = _allTabletProjects;
            if (User.DomainName == "mmulaosmanovic")
            {
                if (searchString.Contains(":"))
                {
                    column = searchString.Split(':')[0];
                    searchString = searchString.Split(':')[1].Trim();
                    switch (column)
                    {
                        case "project number":

                            _filtered =
                                _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "revision number":

                            _filtered =
                                _allTabletProjects.Where(p => p.RevisionNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "customer name":

                            _filtered =
                                _allTabletProjects.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "csr":

                            _filtered =
                                _allTabletProjects.Where(p => !string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "drafter":

                            _filtered =
                                _allTabletProjects.Where(p => !string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "complete":
                            if (int.TryParse(searchString, out int cInt))
                            {
                                _filtered =
                                     _allTabletProjects.Where(p => p.Complete == cInt)
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            else
                            {
                                _filtered =
                                _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            break;
                        default:

                            _filtered =
                                _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                    }
                }
                else
                {
                    _filtered =
                               _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.Tools == true)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                }

                Grid headerGrid = GetHeaderGridFromListBox(AllTabletProjectsListBox);
                foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
                {
                    if (tb.IsChecked == false)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.CustomerName)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Csr)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Drafter)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.DueDate)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tb.IsChecked == true)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.CustomerName)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Csr)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Drafter)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.DueDate)
                                    .ThenByDescending(p => p.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.Tools == true)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else if (User.Department == "Engineering")
            {
                string name = User.GetDWPrincipalId();
                if (searchString.Contains(":"))
                {
                    column = searchString.Split(':')[0];
                    searchString = searchString.Split(':')[1].Trim();
                    switch (column)
                    {
                        case "project number":

                            _filtered =
                                _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "revision number":

                            _filtered =
                                _allTabletProjects.Where(p => p.RevisionNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "customer name":

                            _filtered =
                                _allTabletProjects.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "csr":

                            _filtered =
                                _allTabletProjects.Where(p => !string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "drafter":

                            _filtered =
                                _allTabletProjects.Where(p => !string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "complete":

                            if (int.TryParse(searchString, out int cInt))
                            {
                                _filtered =
                                    _allTabletProjects.Where(p => p.Complete == cInt)
                                          .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                          .ThenBy(kvp => kvp.Complete == 4)
                                          .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                          .ThenByDescending(kvp => kvp.Drafter == name)
                                          .ThenBy(kvp => kvp.Drafter)
                                          .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                          .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                          .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                          .ThenByDescending(kvp => kvp.MarkedPriority)
                                          .ThenBy(kvp => kvp.Tools ?? false)
                                          .ThenBy(kvp => kvp.DueDate)
                                          .ThenBy(kvp => kvp.ProjectNumber)
                                          .ToList();
                            }
                            else
                            {
                                _filtered =
                                 _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            break;

                        default:

                            _filtered =
                                 _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                    }
                }
                else
                {
                    _filtered =
                               _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                }

                Grid headerGrid = GetHeaderGridFromListBox(AllTabletProjectsListBox);
                foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
                {
                    if (tb.IsChecked == false)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                      .OrderBy(kvp => kvp.ProjectNumber)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                      .OrderBy(kvp => kvp.RevisionNumber)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                      .OrderBy(kvp => kvp.CustomerName)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                      .OrderBy(kvp => kvp.Csr)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                      .OrderBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                      .OrderBy(kvp => kvp.DueDate)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tb.IsChecked == true)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                      .OrderByDescending(kvp => kvp.ProjectNumber)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                      .OrderByDescending(kvp => kvp.RevisionNumber)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                      .OrderByDescending(kvp => kvp.CustomerName)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                      .OrderByDescending(kvp => kvp.Csr)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                      .OrderByDescending(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                      .OrderByDescending(kvp => kvp.DueDate)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 4)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTablet))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletDrawnBy))
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.TabletSubmittedBy))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.Tools ?? false)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }

            }
            else
            {
                _filtered =
                    _allTabletProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                  p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                  (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                  (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();

                Grid headerGrid = GetHeaderGridFromListBox(AllTabletProjectsListBox);
                foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
                {
                    if (tb.IsChecked == false)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.CustomerName)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Csr)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Drafter)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.DueDate)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tb.IsChecked == true)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.CustomerName)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.Csr)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.Drafter)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.DueDate)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            AllTabletProjects = _filtered;
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

                    if (User.DomainName == "kbergerdine")
                    {
                        _allToolProjects = eoiAllToolProjects.Where(p => !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                               .OrderByDescending(p => p.Complete).ThenByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                    else
                    {
                        _allToolProjects = eoiAllToolProjects.Where(p => !_nat02context.EoiProjectsFinished.Any(p2 => p2.ProjectNumber == p.ProjectNumber && p2.RevisionNumber == p.RevisionNumber))
                                               .OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                }
                else
                {
                    if (User.DomainName == "kbergerdine")
                    {
                        _allToolProjects = eoiAllToolProjects.OrderByDescending(p => p.Complete).ThenByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                    else
                    {
                        _allToolProjects = eoiAllToolProjects.OrderByDescending(p => p.MarkedPriority).ThenBy(p => p.DueDate).ThenBy(p => p.ProjectNumber).ToList();
                    }
                }
                _nat02context.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindAllToolProjects()
        {
            string searchString = GetSearchString("AllToolProjects");

            string column;
            var _filtered = _allToolProjects;
            if (User.DomainName == "kbergerdine")
            {
                if (searchString.Contains(":"))
                {
                    column = searchString.Split(':')[0];
                    searchString = searchString.Split(':')[1].Trim();
                    switch (column)
                    {
                        case "project number":

                            _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "revision number":

                            _filtered =
                                _allToolProjects.Where(p => p.RevisionNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "customer name":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "csr":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "drafter":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "complete":
                            if (int.TryParse(searchString, out int cInt))
                            {
                                _filtered =
                                     _allToolProjects.Where(p => p.Complete == cInt)
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            else
                            {
                                _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(p => p.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            break;
                        default:

                            _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                    .OrderByDescending(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                            break;
                    }
                }
                else
                {
                    _filtered =
                              _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                    .OrderByDescending(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();

                    Grid headerGrid = GetHeaderGridFromListBox(AllToolProjectsListBox);
                    foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
                    {
                        if (tb.IsChecked == false)
                        {
                            switch (tb.Tag.ToString())
                            {
                                case "Proj #":
                                    _filtered = _filtered
                                        .OrderBy(kvp => kvp.ProjectNumber)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ToList();
                                    break;
                                case "Rev":
                                    _filtered = _filtered
                                        .OrderBy(kvp => kvp.RevisionNumber)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "Customer Name":
                                    _filtered = _filtered
                                        .OrderBy(kvp => kvp.CustomerName)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "CSR":
                                    _filtered = _filtered
                                        .OrderBy(kvp => kvp.Csr)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "Drafter":
                                    _filtered = _filtered
                                        .OrderBy(kvp => kvp.Drafter)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "Due Date":
                                    _filtered = _filtered
                                        .OrderBy(kvp => kvp.DueDate)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                default:
                                    break;
                            }
                        }
                        else if (tb.IsChecked == true)
                        {
                            switch (tb.Tag.ToString())
                            {
                                case "Proj #":
                                    _filtered = _filtered
                                        .OrderByDescending(kvp => kvp.ProjectNumber)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ToList();
                                    break;
                                case "Rev":
                                    _filtered = _filtered
                                        .OrderByDescending(kvp => kvp.RevisionNumber)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "Customer Name":
                                    _filtered = _filtered
                                        .OrderByDescending(kvp => kvp.CustomerName)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "CSR":
                                    _filtered = _filtered
                                        .OrderByDescending(kvp => kvp.Csr)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "Drafter":
                                    _filtered = _filtered
                                        .OrderByDescending(kvp => kvp.Drafter)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.DueDate)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                case "Due Date":
                                    _filtered = _filtered
                                        .OrderByDescending(kvp => kvp.DueDate)
                                        .ThenByDescending(kvp => kvp.Complete)
                                        .ThenByDescending(kvp => kvp.MarkedPriority)
                                        .ThenBy(kvp => kvp.ProjectNumber)
                                        .ToList();
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
            else if (User.Department == "Engineering")
            {
                string name = User.GetDWPrincipalId();
                if (searchString.Contains(":"))
                {
                    column = searchString.Split(':')[0];
                    searchString = searchString.Split(':')[1].Trim();
                    switch (column)
                    {
                        case "project number":

                            _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "revision number":

                            _filtered =
                                _allToolProjects.Where(p => p.RevisionNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "customer name":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "csr":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "drafter":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "complete":
                            if (int.TryParse(searchString, out int cInt))
                            {
                                _filtered =
                                     _allToolProjects.Where(p => p.Complete == cInt)
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            else
                            {
                                _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            }
                            break;
                        default:

                            _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                    }
                }
                else
                {
                    _filtered =
                              _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenBy(kvp => kvp.Complete == 5)
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                      .ThenByDescending(kvp => kvp.Drafter == name)
                                      .ThenBy(kvp => kvp.Drafter)
                                      .ThenBy(kvp => kvp.Complete)
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                }
                Grid headerGrid = GetHeaderGridFromListBox(AllToolProjectsListBox);
                foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
                {
                    if (tb.IsChecked == false)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.CustomerName)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Csr)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Drafter)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.DueDate)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tb.IsChecked == true)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.CustomerName)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.Csr)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.Drafter)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.DueDate)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenBy(kvp => kvp.Complete == 5)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.Drafter))
                                    .ThenByDescending(kvp => kvp.Drafter == name)
                                    .ThenBy(kvp => kvp.Drafter)
                                    .ThenBy(kvp => kvp.Complete)
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            else
            {
                if (searchString.Contains(":"))
                {
                    column = searchString.Split(':')[0];
                    searchString = searchString.Split(':')[1].Trim();
                    switch (column)
                    {
                        case "project number":

                            _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "revision number":

                            _filtered =
                                _allToolProjects.Where(p => p.RevisionNumber.ToString().ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "customer name":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "csr":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        case "drafter":

                            _filtered =
                                _allToolProjects.Where(p => !string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                        default:

                            _filtered =
                                _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                            break;
                    }
                }
                else
                {
                    _filtered =
                              _allToolProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                                p.RevisionNumber.ToString().ToLower().Contains(searchString) ||
                                                (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Csr) && p.Csr.ToLower().Contains(searchString)) ||
                                                (!string.IsNullOrEmpty(p.Drafter) && p.Drafter.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                      .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                      .ThenByDescending(kvp => kvp.MarkedPriority)
                                      .ThenByDescending(kvp => kvp.MultiTipSketch)
                                      .ThenBy(kvp => kvp.DueDate)
                                      .ThenBy(kvp => kvp.ProjectNumber)
                                      .ToList();
                }
                Grid headerGrid = GetHeaderGridFromListBox(AllToolProjectsListBox);
                foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
                {
                    if (tb.IsChecked == false)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.CustomerName)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Csr)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.Drafter)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderBy(kvp => kvp.DueDate)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                    else if (tb.IsChecked == true)
                    {
                        switch (tb.Tag.ToString())
                        {
                            case "Proj #":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.ProjectNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ToList();
                                break;
                            case "Rev":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.RevisionNumber)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Customer Name":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.CustomerName)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "CSR":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.Csr)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Drafter":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.Drafter)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.DueDate)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            case "Due Date":
                                _filtered = _filtered
                                    .OrderByDescending(kvp => kvp.DueDate)
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.HoldStatus) || kvp.HoldStatus == "OFF HOLD")
                                    .ThenByDescending(kvp => string.IsNullOrEmpty(kvp.ProjectStartedTool))
                                    .ThenByDescending(kvp => kvp.MarkedPriority)
                                    .ThenByDescending(kvp => kvp.MultiTipSketch)
                                    .ThenBy(kvp => kvp.ProjectNumber)
                                    .ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            AllToolProjects = _filtered;
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
        public void BindDriveWorksQueue()
        {
            DriveWorksQueue = _driveWorksQueue;
        }
        private void GetNatoliOrderList()
        {
            using var _natbcContext = new NATBCContext();
            string username = Environment.UserDomainName.ToLower() + "\\" + Environment.UserName.ToLower();
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
        public void BindNatoliOrderList()
        {
            string searchString = GetSearchString("NatoliOrderList");

            var _filtered = _natoliOrderList;
            if (searchString.ToLower().StartsWith("rep:"))
            {
                searchString = searchString.Substring(4);
                _filtered =
                _natoliOrderList.Where(p => !string.IsNullOrEmpty(p.RepId) && p.RepId.ToLower().Trim() == searchString)
                                .OrderBy(kvp => kvp.ShipDate)
                                .ToList();
            }
            else
            {
                _filtered =
                _natoliOrderList.Where(p => p.OrderNo.ToString().ToLower().Contains(searchString) ||
                                            !string.IsNullOrEmpty(p.Customer) && p.Customer.ToLower().Contains(searchString))
                                .OrderBy(kvp => kvp.ShipDate)
                                .ToList();
            }

            Grid headerGrid = GetHeaderGridFromListBox(NatoliOrderListListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.OrderNo)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Customer":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.Customer)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Ship Date":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Rush":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.Rush)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "On Hold":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.OnHold)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                            case "Rep":
                            _filtered = _filtered
                                .OrderBy(kvp => kvp.RepId)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Order #":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.OrderNo)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Customer":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.Customer)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Ship Date":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Rush":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.Rush)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "On Hold":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.OnHold)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        case "Rep":
                            _filtered = _filtered
                                .OrderByDescending(kvp => kvp.RepId)
                                .ThenBy(kvp => kvp.ShipDate)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }

            NatoliOrderList = _filtered;
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
        private void ResetHeightWhenSearchIsOver(ListBox listBox)
        {
            Label label = (listBox.Parent as Grid).TemplatedParent as Label;
            label.ApplyTemplate();
            Grid templatedGrid = VisualTreeHelper.GetChild(label as DependencyObject, 0) as Grid;
            Grid templatedGrid1 = templatedGrid.Children.OfType<Grid>().First() as Grid;
            DockPanel templatedDockPanel = templatedGrid1.Children.OfType<DockPanel>().Last() as DockPanel;
            TextBox templatedTextBox = templatedDockPanel.Children.OfType<TextBox>().First() as TextBox;
            Border templatedBorder = VisualTreeHelper.GetChild(templatedTextBox as DependencyObject, 0) as Border;
            Grid templatedBorderGrid = templatedBorder.Child as Grid;
            TextBox templatedActualTextBox = (templatedBorderGrid.Children.OfType<TextBox>().First() as TextBox);
            if (templatedActualTextBox.Text == "")
            {
                // Resets the height so it can scale with number of items
                label.Height = Double.NaN;
            }
        }
        private void ModuleSearchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            moduleSearchTimer.Stop();
            switch (searchedFromModuleName)
            {
                case "BeingEntered":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindBeingEntered();
                        ResetHeightWhenSearchIsOver(OrdersBeingEnteredListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "InTheOffice":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindInTheOffice();
                        ResetHeightWhenSearchIsOver(OrdersInTheOfficeListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "QuotesNotConverted":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindQuotesNotConverted();
                        ResetHeightWhenSearchIsOver(QuotesNotConvertedListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "EnteredUnscanned":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindEnteredUnscanned();
                        ResetHeightWhenSearchIsOver(OrdersEnteredListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "InEngineering":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindInEngineering();
                        ResetHeightWhenSearchIsOver(OrdersInEngListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "QuotesToConvert":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindQuotesToConvert();
                        ResetHeightWhenSearchIsOver(QuotesToConvertListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "ReadyToPrint":
                    Dispatcher.BeginInvoke((Action)(() =>
                        {
                            BindReadyToPrint();
                            ResetHeightWhenSearchIsOver(OrdersReadyToPrintListBox);
                        }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "PrintedInEngineering":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindPrintedInEngineering();
                        ResetHeightWhenSearchIsOver(OrdersPrintedListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "AllTabletProjects":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindAllTabletProjects();
                        ResetHeightWhenSearchIsOver(AllTabletProjectsListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "AllToolProjects":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindAllToolProjects();
                        ResetHeightWhenSearchIsOver(AllToolProjectsListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "DriveWorksQueue":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindDriveWorksQueue();
                        ResetHeightWhenSearchIsOver(DriveWorksQueueListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                case "NatoliOrderList":
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        BindNatoliOrderList();
                        ResetHeightWhenSearchIsOver(NatoliOrderListListBox);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    break;
                default:
                    break;
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
                case "AllToolProjects":
                    AllToolProjectsSearchBox_TextChanged();
                    break;
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
            moduleSearchTimer.Stop();
            searchedFromModuleName = "BeingEntered";
            moduleSearchTimer.Start();
        }
        private void OrdersInTheOfficeSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "InTheOffice";
            moduleSearchTimer.Start();
        }
        private void QuotesNotConvertedSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "QuotesNotConverted";
            moduleSearchTimer.Start();
        }
        private void OrdersEnteredUnscannedSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "EnteredUnscanned";
            moduleSearchTimer.Start();
        }
        private void OrdersInEngineeringUnprintedSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "InEngineering";
            moduleSearchTimer.Start();
        }
        private void QuotesToConvertSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "QuotesToConvert";
            moduleSearchTimer.Start();
        }
        private void OrdersReadyToPrintSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "ReadyToPrint";
            moduleSearchTimer.Start();
        }
        private void OrdersPrintedInEngineeringSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "PrintedInEngineering";
            moduleSearchTimer.Start();
        }
        private void AllTabletProjectsSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "AllTabletProjects";
            moduleSearchTimer.Start();
        }
        private void AllToolProjectsSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "AllToolProjects";
            moduleSearchTimer.Start();
        }
        private void DriveWorksQueueSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "DriveWorksQueue";
            moduleSearchTimer.Start();
        }
        private void NatoliOrderListSearchBox_TextChanged()
        {
            moduleSearchTimer.Stop();
            searchedFromModuleName = "NatoliOrderList";
            moduleSearchTimer.Start();
        }
#endregion
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
        //private void GridWindow_Drop(object sender, DragEventArgs e)
        //{
        //    try
        //    {
        //        if (e.Data.GetData(DataFormats.FileDrop) != null)
        //        {
        //            string[] filePathArray = (string[])(e.Data.GetData(DataFormats.FileDrop));
        //            List<string> filePaths = filePathArray.ToList();
        //            if (filePaths[0].Contains("WorkOrdersToPrint"))
        //            {
        //                OrderingWindow pDFOrderingWindow = new OrderingWindow(filePaths, User, this);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        IMethods.WriteToErrorLog("MainWindow => GridWindow_Drop", ex.Message, User);
        //    }
        //}
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