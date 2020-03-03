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

        private Dictionary<string, (string, IEnumerable<object>, ListBox)> panels = new Dictionary<string, (string, IEnumerable<object>, ListBox)>();
        List<(string, IEnumerable<object>, ListBox)> modules = new List<(string, IEnumerable<object>, ListBox)>();

        public System.Timers.Timer timer = new System.Timers.Timer(15000);

        private User user;


        public TestWindow()
        {
            InitializeComponent();

            user = new User("dkruggel");

            BuildPanels();

            BindData();

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
                        //case ("AllTabletProjects", "Main"):
                        //    Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()), TaskScheduler.Current);
                        //    break;
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
                        //case ("AllToolProjects", "Main"):
                        //    Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()), TaskScheduler.Current);
                        //    break;
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
            Task.Run(() => BindData());
        }

        private void GetData()
        {
            using var _ = new NAT02Context();
            try
            {
                // List<EoiOrdersBeingEnteredView> eoiOrdersBeingEnteredView = _.EoiOrdersBeingEnteredView.OrderBy(o => o.OrderNo).ToList();

                //if (user.Department == "Customer Service" && !(user.GetUserName().StartsWith("Tiffany") || user.GetUserName().StartsWith("James W")))
                //{
                //    string usrName = user.GetUserName().Split(' ')[0];
                //    eoiOrdersInOfficeView = _.EoiOrdersInOfficeView.Where(o => o.Csr.StartsWith(usrName)).OrderBy(o => o.NumDaysToShip).ThenBy(o => o.DaysInOffice).ToList();
                //}
                //else
                //{
                // List<EoiOrdersInOfficeView> eoiOrdersInOfficeView = _.EoiOrdersInOfficeView.OrderBy(o => o.NumDaysToShip).ThenBy(o => o.DaysInOffice).ToList();
                //}
                //List<EoiOrdersEnteredAndUnscannedView> eoiOrdersEnteredAndUnscannedView = _.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();
                //List<EoiOrdersInEngineeringUnprintedView> eoiOrdersInEngineeringUnprintedView = _.EoiOrdersInEngineeringUnprintedView.OrderByDescending(o => o.DaysInEng).ThenBy(o => o.NumDaysToShip).ToList();
                List<EoiOrdersReadyToPrintView> eoiOrdersReadyToPrintView = _.EoiOrdersReadyToPrintView.OrderBy(o => o.OrderNo).ToList();
                List<EoiOrdersPrintedInEngineeringView> eoiOrdersPrintedInEngineeringView = _.EoiOrdersPrintedInEngineeringView.OrderBy(o => o.OrderNo).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _.Dispose();
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
                            Dispatcher.Invoke(() => GetBeingEntered());
                            break;
                        case ("InTheOffice", "Main"):
                            Dispatcher.Invoke(() => GetInTheOffice());
                            break;
                        case ("QuotesNotConverted", "QuotesNotConverted"):
                            Dispatcher.Invoke(() => GetQuotesNotConverted());
                            break;
                        case ("EnteredUnscanned", "Main"):
                            Dispatcher.Invoke(() => GetEnteredUnscanned());
                            break;
                        case ("InEngineering", "Main"):
                            Dispatcher.Invoke(() => GetInEngineering());
                            break;
                        case ("QuotesToConvert", "Main"):
                            Dispatcher.Invoke(() => GetQuotesToConvert());
                            break;
                        case ("ReadyToPrint", "Main"):
                            Dispatcher.Invoke(() => GetReadyToPrint());
                            break;
                        case ("PrintedInEngineering", "Main"):
                            Dispatcher.Invoke(() => GetPrintedInEngineering());
                            break;
                        //case ("AllTabletProjects", "Main"):
                        //    Task.Run(() => GetAllTabletProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllTabletProjects()), TaskScheduler.Current);
                        //    break;
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
                        //case ("AllToolProjects", "Main"):
                        //    Task.Run(() => GetAllToolProjects()).ContinueWith(t => Dispatcher.Invoke(() => BindAllToolProjects()), TaskScheduler.Current);
                        //    break;
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

        #region Gets And Binds
        private void GetBeingEntered()
        {
            try
            {
                using var _ = new NAT02Context();
                OrdersBeingEntered = _.EoiOrdersBeingEnteredView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetInTheOffice()
        {
            try
            {
                using var _ = new NAT02Context();
                OrdersInTheOffice = _.EoiOrdersInOfficeView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                QuotesNotConverted = _eoiQuotesNotConvertedView.Where(q => q.QuoteDate >= DateTime.Now.AddDays(-quoteDays)).OrderByDescending(q => q.QuoteNo).ThenByDescending(q => q.QuoteRevNo).ToList();
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
                OrdersEntered = _.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetInEngineering()
        {
            try
            {
                using var _ = new NAT02Context();
                OrdersInEng = _.EoiOrdersInEngineeringUnprintedView.OrderByDescending(o => o.DaysInEng).ThenBy(o => o.NumDaysToShip).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
                QuotesToConvert = _eoiQuotesMarkedForConversion;
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
                OrdersReadyToPrint = _.EoiOrdersReadyToPrintView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void GetPrintedInEngineering()
        {
            try
            {
                using var _ = new NAT02Context();
                OrdersPrinted = _.EoiOrdersPrintedInEngineeringView.OrderBy(o => o.OrderNo).ToList();
                _.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion
    }
}
