using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using NatoliOrderInterface.Models;
using System.Linq;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
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

        private Dictionary<string, (string, IEnumerable<object>, ListBox)> panels = new Dictionary<string, (string, IEnumerable<object>, ListBox)>();
        List<(string, IEnumerable<object>, ListBox)> modules = new List<(string, IEnumerable<object>, ListBox)>();

        public System.Timers.Timer timer = new System.Timers.Timer(30000);

        private User user;


        public TestWindow()
        {
            InitializeComponent();

            user = new User("dkruggel");

            BuildPanels();

            BindData();
            UpdateUI();

            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void BuildPanels()
        {
            try
            {
                foreach (string panel in user.VisiblePanels)
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

                    switch (panel, "Main")
                    {
                        case ("BeingEntered", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["OrdersBeingEnteredModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            OrdersBeingEnteredListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            OrdersBeingEnteredListBox.ItemsSource = null;
                            OrdersBeingEnteredListBox.ItemsSource = ordersBeingEntered;
                            break;
                        case ("InTheOffice", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["OrdersInTheOfficeModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            OrdersInTheOfficeListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            OrdersInTheOfficeListBox.ItemsSource = null;
                            OrdersInTheOfficeListBox.ItemsSource = ordersInTheOffice;
                            break;
                        case ("QuotesNotConverted", "QuotesNotConverted"):
                            label = new Label
                            {
                                Style = App.Current.Resources["QuotesNotConvertedModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            QuotesNotConvertedListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            QuotesNotConvertedListBox.ItemsSource = null;
                            QuotesNotConvertedListBox.ItemsSource = quotesNotConverted;
                            break;
                        case ("EnteredUnscanned", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["OrdersEnteredModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            OrdersEnteredListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            OrdersEnteredListBox.ItemsSource = null;
                            OrdersEnteredListBox.ItemsSource = ordersEntered;
                            break;
                        case ("InEngineering", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["OrdersInEngineeringModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            OrdersInEngListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            OrdersInEngListBox.ItemsSource = null;
                            OrdersInEngListBox.ItemsSource = ordersInEng;
                            break;
                        case ("QuotesToConvert", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["QuotesToConvertModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            QuotesToConvertListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            QuotesToConvertListBox.ItemsSource = null;
                            QuotesToConvertListBox.ItemsSource = quotesToConvert;
                            break;
                        case ("ReadyToPrint", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["OrdersReadyToPrintModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            OrdersReadyToPrintListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            OrdersReadyToPrintListBox.ItemsSource = null;
                            OrdersReadyToPrintListBox.ItemsSource = ordersReadyToPrint;
                            break;
                        case ("PrintedInEngineering", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["OrdersPrintedInEngineeringModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            OrdersPrintedListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            OrdersPrintedListBox.ItemsSource = null;
                            OrdersPrintedListBox.ItemsSource = ordersPrinted;
                            break;
                        case ("AllTabletProjects", "Main"):
                            label = new Label
                            {
                                Style = App.Current.Resources["AllTabletProjectsModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            AllTabletProjectsListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            AllTabletProjectsListBox.ItemsSource = null;
                            AllTabletProjectsListBox.ItemsSource = allTabletProjects;
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
                            label = new Label
                            {
                                Style = App.Current.Resources["AllToolProjectsModule"] as Style
                            };

                            label.ApplyTemplate();

                            grid.Children.Add(label);

                            MainWrapPanel.Children.Add(grid);

                            AllToolProjectsListBox = (VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First() as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
                            AllToolProjectsListBox.ItemsSource = null;
                            AllToolProjectsListBox.ItemsSource = allToolProjects;
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
                        //case ("DriveWorksQueue", "Main"):
                        //    Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()), TaskScheduler.Current);
                        //    break;
                        //case ("NatoliOrderList", "NatoliOrderList"):
                        //    Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()), TaskScheduler.Current);
                        //    break;
                        default:
                            break;
                    }
                }
            }
            catch
            {

            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //DateTime dateTime = DateTime.Now;
            timer.Stop();
            BindData();
            UpdateUI();
            timer.Start();
            //TimeSpan timeSpan = DateTime.Now - dateTime;
            //MessageBox.Show(timeSpan.TotalMilliseconds.ToString());
        }

        private void BindData()
        {
            try
            {
                foreach (string panel in user.VisiblePanels)
                {
                    switch (panel, "Main")
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
                        //case ("DriveWorksQueue", "Main"):
                        //    Task.Run(() => GetDriveWorksQueue()).ContinueWith(t => Dispatcher.Invoke(() => BindDriveWorksQueue()), TaskScheduler.Current);
                        //    break;
                        //case ("NatoliOrderList", "NatoliOrderList"):
                        //    Task.Run(() => GetNatoliOrderList()).ContinueWith(t => Dispatcher.Invoke(() => BindNatoliOrderList()), TaskScheduler.Current);
                        //    break;
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
            Task.Run(() => Dispatcher.Invoke(() => BindBeingEntered()));
            Task.Run(() => Dispatcher.Invoke(() => BindInTheOffice()));
            Task.Run(() => Dispatcher.Invoke(() => BindEnteredUnscanned()));
            Task.Run(() => Dispatcher.Invoke(() => BindInEngineering()));
            Task.Run(() => Dispatcher.Invoke(() => BindReadyToPrint()));
            Task.Run(() => Dispatcher.Invoke(() => BindPrintedInEngineering()));
            Task.Run(() => Dispatcher.Invoke(() => BindAllTabletProjects()));
            Task.Run(() => Dispatcher.Invoke(() => BindAllToolProjects()));
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
            OrdersInTheOffice = _ordersInTheOffice;
        }
        private void GetQuotesNotConverted()
        {
            try
            {
                using var _ = new NAT02Context();
                IQueryable<string> subList = _.EoiSettings.Where(e => e.EmployeeId == user.EmployeeCode)
                                                         .Select(e => e.Subscribed);
                string[] subs = subList.First().Split(',');
                //quotesCompletedChanged = (quotesCompletedCount != _.EoiQuotesOneWeekCompleted.Count());
                //quotesCompletedCount = _.EoiQuotesOneWeekCompleted.Count();
                short quoteDays = user.QuoteDays;
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
            OrdersInEng = _ordersInEng;
        }
        private void GetQuotesToConvert()
        {
            try
            {
                using var _ = new NAT02Context();
                List<EoiQuotesMarkedForConversionView> eoiQuotesMarkedForConversion = new List<EoiQuotesMarkedForConversionView>();

                IQueryable<string> subList = _.EoiSettings.Where(e => e.EmployeeId == user.EmployeeCode)
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
            OrdersReadyToPrint = _ordersReadyToPrint;
        }
        private void GetPrintedInEngineering()
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
        private void BindPrintedInEngineering()
        {
            OrdersPrinted = _ordersPrinted;
        }
        private void GetAllTabletProjects()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiAllTabletProjectsView> eoiAllTabletProjects = new List<EoiAllTabletProjectsView>();
                IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == user.EmployeeCode)
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
                if (user.FilterActiveProjects)
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
            AllTabletProjects = _allTabletProjects;
        }
        private void GetAllToolProjects()
        {
            try
            {
                using var _nat02context = new NAT02Context();
                List<EoiAllToolProjectsView> eoiAllToolProjects = new List<EoiAllToolProjectsView>();
                IQueryable<string> subList = _nat02context.EoiSettings.Where(e => e.EmployeeId == user.EmployeeCode)
                                                                     .Select(e => e.Subscribed);
                eoiAllToolProjects = new List<EoiAllToolProjectsView>();
                string[] subs = subList.First().Split(',');
                List<EoiAllToolProjectsView> projects = new List<EoiAllToolProjectsView>();
                foreach (string sub in subs)
                {
                    string s = sub;
                    if (sub == "Gregory") { s = "Greg"; }
                    if (sub == "Nicholas") { s = "Nick"; }
                    eoiAllToolProjects.AddRange(_nat02context.EoiAllToolProjectsView.Where(q => q.Csr.Contains(s) || q.ReturnToCsr.Contains(s)).ToList());
                }
                if (user.FilterActiveProjects)
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
            AllToolProjects = _allToolProjects;
        }
        #endregion
    }
}
