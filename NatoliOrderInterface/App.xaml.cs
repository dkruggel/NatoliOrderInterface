using System.Windows;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.FileExtensions;
using System.IO;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NatoliOrderInterface.Models;
using System.Windows.Input;
using NatoliOrderInterface.Models.NAT01;
using System.Windows.Controls.Primitives;
using NatoliOrderInterface.Models.Projects;
using System.Timers;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.DriveWorks;
using System.Threading.Tasks;
using System.Reflection;
using System.ComponentModel;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Declaratoins
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        public static string Server;
        public static string PersistSecurityInfo;
        public static string UserID;
        public static string Password;
        public static string SmtpServer;
        public static int? SmtpPort;
        public static List<string> StandardKeys = new List<string> { "N-6600-32M", "N-6600-01M", "N-6600-02M", "N-6600-03M", "N-7080-02M", "N-6010", "N-6441", "N-6441M", "N-6653", "N-6652", "N-6445", "N-6444" };
        public static User user { get; set; }
        private DependencyObject grid = null;

        public List<(string, CheckBox, string)> selectedOrders = new List<(string, CheckBox, string)>();
        public List<(string, string, CheckBox, string, string)> selectedProjects = new List<(string, string, CheckBox, string, string)>();
        public List<(string, string, CheckBox, string, string)> projectsToMove = new List<(string, string, CheckBox, string, string)>();
        public List<(string, string, CheckBox, string)> selectedQuotes = new List<(string, string, CheckBox, string)>();

        private DispatcherTimer double_click_timer = new DispatcherTimer()
        {
            Interval = new TimeSpan(0, 0, 0, 0, 250)
        };
        private int click_count = 0;
        private CheckBox checkBox;
        private string docNumber;
        #endregion

        public static void GetConnectionString()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(@"\\nshare\VB_Apps\NatoliOrderInterface\Resources")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                var emailConfiguration = configuration.GetSection("ConnectionStrings");
                Server = emailConfiguration.GetSection("Server").Value;
                PersistSecurityInfo = emailConfiguration.GetSection("PersistSecurityInfo").Value;
                UserID = emailConfiguration.GetSection("UserID").Value;
                Password = emailConfiguration.GetSection("Password").Value;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("App.Xaml.cs => GetConnectionString()", ex.Message, null);
            }
        }
        public static void GetEmailSettings()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(@"\\nshare\VB_Apps\NatoliOrderInterface\Resources")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                var emailConfiguration = configuration.GetSection("EmailConfiguration");
                SmtpServer = emailConfiguration.GetSection("SmtpServer").Value;
                SmtpPort = Int32.Parse(emailConfiguration.GetSection("SmtpPort").Value);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("App.Xaml.cs => GetEmailSettings()", ex.Message, null);
            }
        }
        public void InitializeTimers()
        {
            double_click_timer.Tick += Double_Click_Timer_Elapsed;
        }
        private void OpenNotesButtonButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Grid childGrid = (Grid)button.Parent;
            Grid grid = (Grid)childGrid.Parent;

            int id = Convert.ToInt32(((TextBlock)grid.Children.OfType<TextBlock>().First(tb => tb.Tag.ToString() == "ID")).Text);
            CustomerNoteWindow customerNoteWindow = new CustomerNoteWindow(id, new User(Environment.UserName));
            customerNoteWindow.Show();
        }
        private void SearchTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            Grid grid = textBox.Parent as Grid;
            Image xImage = grid.Children.OfType<Image>().First(i => i.Name.ToString() == "xImage") as Image;
            if (e.Key == System.Windows.Input.Key.Escape && textBox.IsFocused)
            {
                textBox.Text = "";
                xImage.Visibility = Visibility.Collapsed;
            }
            else
            {
                xImage.Visibility = Visibility.Visible;
            }
        }
        private void xImage_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image xImage = sender as Image;
            Grid grid = xImage.Parent as Grid;
            TextBox searchTextBox = grid.Children.OfType<TextBox>().First(i => i.Name.ToString() == "SearchTextBox") as TextBox;
            xImage.Visibility = Visibility.Collapsed;
            searchTextBox.Text = "";
        }
        private void MagImage_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image magImage = sender as Image;
            Grid grid = magImage.Parent as Grid;
            Border border = grid.Parent as Border;
            if (border.ActualWidth < 140)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(150, TimeSpan.FromSeconds(.5));
                border.BeginAnimation(Border.WidthProperty, doubleAnimation);
            }
            else
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(24, TimeSpan.FromSeconds(.5));
                border.BeginAnimation(Border.WidthProperty, doubleAnimation);
            }
        }
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var x = new TextBox();
            try
            {
                x = VisualTreeHelper.GetParent(((sender as TextBox).Parent as Grid).Parent as DockPanel) as TextBox;
            }
            catch
            {
                x = sender as TextBox;
            }
            string type = (((VisualTreeHelper.GetParent((x.Parent as Grid).Parent as Border) as TextBox).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
            (Window.GetWindow(sender as DependencyObject) as MainWindow).TextChanged(type);
        }
        private void OpenQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            OpenQuote(selectedQuotes.Last().Item1 + "-" + selectedQuotes.Last().Item2);
        }
        private void OpenProjectButton_Click(object sender, RoutedEventArgs e)
        {
            OpenProject(selectedProjects.Last().Item1);
        }
        private void OpenOrderButton_Click(object sender, RoutedEventArgs e)
        {
            OpenWorkOrder(selectedOrders.Last().Item1);
        }
        private void ExpandModule_Click(object sender, RoutedEventArgs e)
        {
            Button expandButton = sender as Button;
            Button collapseButton = (expandButton.Parent as StackPanel).Children.OfType<Button>().Single(b => b.Name == "CollapseButton");
            int count = (((expandButton.Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Items.Count;

            expandButton.Visibility = Visibility.Collapsed;

            collapseButton.Visibility = Visibility.Visible;

            if (count == 0)
            {
                (expandButton.TemplatedParent as Label).MinHeight = 100;
                //(expandButton.TemplatedParent as Label).Height = 205;
                (expandButton.TemplatedParent as Label).Height = 10;
            }
            else
            {
                if (count < 11)
                {
                    (collapseButton.TemplatedParent as Label).Height = 35 * count + 100;
                }
                else
                {
                    (collapseButton.TemplatedParent as Label).Height = 500;
                }
                (expandButton.TemplatedParent as Label).MinHeight = 200;
            }
        }
        private void CollapseModule_Click(object sender, RoutedEventArgs e)
        {
            Button collapseButton = sender as Button;
            Button expandButton = (collapseButton.Parent as StackPanel).Children.OfType<Button>().Single(b => b.Name == "ExpandButton");
            int count = (((collapseButton.Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Items.Count;

            collapseButton.Visibility = Visibility.Collapsed;

            expandButton.Visibility = Visibility.Visible;

            if (count == 0)
            {
                (collapseButton.TemplatedParent as Label).MinHeight = 100;
                (collapseButton.TemplatedParent as Label).Height = 100;
            }
            else
            {
                if (count < 3)
                {
                    (collapseButton.TemplatedParent as Label).Height = 35 * count + 100;
                }
                else
                {
                    (collapseButton.TemplatedParent as Label).Height = 205;
                }
                (expandButton.TemplatedParent as Label).MinHeight = 200;
            }
        }
        private void DockPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Window parent = Window.GetWindow((sender as DockPanel));
            GetAncestorWithoutName(sender as DependencyObject, typeof(Label));
            Label label = grid as Label;
            grid = null;

            DragAndDrop dragAndDrop = new DragAndDrop(user, label.Parent as Grid);

            //parent.Cursor = Cursors.Hand;
        }
        private void DockPanel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var name = ((sender as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            int oldIndex = user.VisiblePanels.IndexOf(name);

            Window parent = Window.GetWindow((sender as DockPanel));
            
            Win32Point w32Mouse = new Win32Point();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GetCursorPos(ref w32Mouse);
            }
            else
            {
                // How to do on Linux and OSX?
            }

            if (w32Mouse.X < parent.Left || w32Mouse.X > (parent.Left + parent.Width) || w32Mouse.Y < parent.Top || w32Mouse.Y > (parent.Top + parent.Height))
            {
                (Application.Current.MainWindow as MainWindow).MainWrapPanel.Children.RemoveAt(oldIndex);
                SaveSettings();
            }
        }
        private void SaveSettings()
        {
            NAT02Context _nat02context = new NAT02Context();

            string newPanels = "";
            List<string> visiblePanels = new List<string>();
            foreach (Grid grid in (Application.Current.MainWindow as MainWindow).MainWrapPanel.Children)
            {
                visiblePanels.Add((VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First(), 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First().Name[0..^7]);
            }
            newPanels = String.Join(',', visiblePanels.ToArray());

            EoiSettings eoiSettings = _nat02context.EoiSettings.Single(s => s.EmployeeId == user.EmployeeCode);
            eoiSettings.Panels = newPanels;
            _nat02context.EoiSettings.Update(eoiSettings);
            _nat02context.SaveChanges();
            _nat02context.Dispose();

            user.VisiblePanels = visiblePanels;
        }
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            ListBox listBox = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First();
            var items = listBox.Items;

            if (listBox.Name.Contains("Quote"))
            {
                string filePath = @"C:\Users\" + user.DomainName + @"\Desktop\QuoteList.csv";
                using var stream = new System.IO.StreamWriter(filePath, false);

                // Quote Number, Rev Number, Customer Name, Quote Date
                // Get info from currently filtered list in QuotesNotConverted
                var expanders = ((((sender as Button).Parent as Grid).Parent as DockPanel).Children.OfType<ScrollViewer>().First().Content as StackPanel).Children;

                // Write headers
                stream.Write("Sales Rep ID,Quote Number,Rev Number,Customer Name,Quote Date\n");

                foreach (EoiQuotesNotConvertedView quote in items)
                {
                    double quoteNumber = quote.QuoteNo;
                    short revNumber = (short)quote.QuoteRevNo;
                    string customerName = quote.CustomerName.Replace(',', '\0');
                    using var _ = new NAT01Context();
                    string acctNo = _.QuoteHeader.Single(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == revNumber).UserAcctNo;
                    using var __ = new NECContext();
                    string repId = __.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Slprsnid;
                    __.Dispose();
                    string quoteDate = _.QuoteHeader.Single(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == revNumber).QuoteDate.ToShortDateString();
                    _.Dispose();
                    stream.Write("{0},{1},{2},{3},{4}\n", repId, quoteNumber, revNumber, customerName, quoteDate);
                }

                stream.Flush();
                stream.Dispose();
            }
            else
            {
                string filePath = @"C:\Users\" + user.DomainName + @"\Desktop\OrderList.csv";
                using var stream = new System.IO.StreamWriter(filePath, false);

                // Order Number, Quote Number, Rev Number, Customer Name, Order Date, Ship Date, PO Number
                // Get info from currently filtered list in NatoliOrderList
                
                // Write headers
                stream.Write("Order Number,Quote Number,Rev Number,Customer Name,Order Date,Ship Date,PO Number\n");

                foreach (NatoliOrderListFinal order in items)
                {
                    double orderNumber = order.OrderNo;
                    using var _ = new NAT01Context();
                    OrderHeader orderHeader = _.OrderHeader.Single(q => q.OrderNo == orderNumber * 100);
                    _.Dispose();
                    double? quoteNumber = orderHeader.QuoteNumber;
                    short? revNumber = orderHeader.QuoteRevNo;
                    string customerName = order.Customer.Replace(',', '\0');
                    string orderDate = ((DateTime)orderHeader.OrderDate).ToShortDateString();
                    string shipDate = order.ShipDate.ToShortDateString();
                    string poNumber = orderHeader.Ponumber.Trim() + (string.IsNullOrEmpty(orderHeader.Poextension.Trim()) ? "" : '-' + orderHeader.Poextension);
                    stream.Write("{0},{1},{2},{3},{4},{5},{6}\n", orderNumber, quoteNumber, revNumber, customerName, orderDate, shipDate, poNumber);
                }

                stream.Flush();
                stream.Dispose();
            }

            MessageBox.Show("Your file is ready.");
        }
        private void HeaderCheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
        private void HeaderCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                var type = (((sender as CheckBox).Parent as Grid).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

                if (type.Contains("Quote"))
                {
                    selectedQuotes.Clear();
                }
                else if (type.Contains("Project"))
                {
                    selectedProjects.Clear();
                }
                else
                {
                    selectedOrders.Clear();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("HeaderCheckBox_Unchecked", ex.Message, user);
            }
        }
        private DependencyObject GetAncestor(DependencyObject curr, Type type, string name)
        {
            DependencyObject newObj = VisualTreeHelper.GetParent(curr);

            if (newObj.GetType() != type || (newObj.GetType() == type && !(newObj as FrameworkElement).Name.Contains(name)))
            {
                GetAncestor(newObj, type, name);
            }

            if (grid is null) { grid = newObj; }
            return newObj;
        }
        private DependencyObject GetAncestorWithoutName(DependencyObject curr, Type type)
        {
            DependencyObject newObj = VisualTreeHelper.GetParent(curr);

            if (newObj.GetType() != type)
            {
                GetAncestorWithoutName(newObj, type);
            }

            if (grid is null) { grid = newObj; }
            return newObj;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            using var _ = new ProjectsContext();

            try
            {
                CheckBox checkBox = sender as CheckBox;
                var x = (checkBox.Parent as Grid).Children;

                // Get ListBox
                GetAncestorWithoutName(checkBox, typeof(ListBox));
                var type = (grid as ListBox).Name[0..^7];
                grid = null;

                // Get DockPanel for buttons
                GetAncestor(checkBox, typeof(Grid), "DisplayGrid");
                DockPanel dockPanel = (grid as Grid).Children.OfType<DockPanel>().Single(dp => dp.Name == "TitleDockPanel");
                grid = null;

                bool quote = type.Contains("Quote");
                bool project = type.Contains("Project");
                bool order = (!type.Contains("Quote") &&
                              !type.Contains("Project") &&
                              !type.Contains("Queue") &&
                              !type.Contains("NatoliOrderList"));

                // Get Buttons
                var buttons = dockPanel.Children.OfType<StackPanel>().First().Children.OfType<Button>();
                string col0val = (x[1] as TextBlock).Text;
                string col1val = (x[2] as TextBlock).Text;

                // Quote
                if (quote)
                {
                    selectedQuotes.Add((col0val, col1val, checkBox, type));

                    var uniqueTypes = selectedQuotes.Select(q => q.Item4).Distinct();

                    if (type == "QuotesNotConverted")
                    {
                        Button submitQuoteButton = buttons.Single(b => b.Name == "SubmitQuoteButton");
                        Button followUpButton = buttons.Single(b => b.Name == "FollowUpButton");

                        if (uniqueTypes.Count() == 1)
                        {
                            submitQuoteButton.Visibility = Visibility.Visible;
                            followUpButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            submitQuoteButton.Visibility = Visibility.Collapsed;
                            followUpButton.Visibility = Visibility.Collapsed;
                        }
                    }
                    else
                    {
                        Button recallQuoteButton = buttons.Single(b => b.Name == "RecallQuoteButton");

                        if (uniqueTypes.Count() == 1)
                        {
                            recallQuoteButton.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            recallQuoteButton.Visibility = Visibility.Collapsed;
                        }
                    }
                }
                // Project
                else if (project)
                {
                    Button projectOnHoldButton = buttons.Single(b => b.Name == "ProjectOnHoldButton");
                    Button projectOffHoldButton = buttons.Single(b => b.Name == "ProjectOffHoldButton");
                    Button projectNextStepButton = buttons.Single(b => b.Name == "ProjectNextStepButton");
                    Button projectCompleteButton = buttons.Single(b => b.Name == "ProjectCompleteButton");
                    Button projectCancelButton = buttons.Single(b => b.Name == "ProjectCancelButton");
                    Button projectOpenButton = buttons.Single(b => b.Name == "WindowButton");

                    projectOnHoldButton.Visibility = Visibility.Visible;
                    projectOffHoldButton.Visibility = Visibility.Visible;
                    projectNextStepButton.Visibility = Visibility.Visible;
                    projectCompleteButton.Visibility = Visibility.Visible;
                    projectCancelButton.Visibility = Visibility.Visible;
                    projectOpenButton.Visibility = Visibility.Visible;

                    projectOnHoldButton.IsEnabled = false;
                    projectOffHoldButton.IsEnabled = false;
                    projectNextStepButton.IsEnabled = false;
                    projectCompleteButton.IsEnabled = false;
                    projectCancelButton.IsEnabled = false;

                    // Get previous and next steps to reset tooltip of back and forward buttons
                    ProjectSpecSheet pss = _.ProjectSpecSheet.Single(p => p.ProjectNumber == int.Parse(col0val) && p.RevisionNumber == int.Parse(col1val));
                    List<ProjectSpecSheet> projectSpecSheets = new List<ProjectSpecSheet>();

                    string nextStep = "";

                    if (pss.HoldStatus == "ON HOLD")
                    {
                        ProjectOnHoldButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                    }
                    else if ((bool)pss.Tablet && !(bool)pss.Tools)
                    {
                        if (pss.TabletCheckedBy.Length > 0)
                        {
                            ProjectCheckedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                        }
                        else if (!(pss.TabletSubmittedBy is null))
                        {
                            ProjectSubmittedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Check";
                        }
                        else if (pss.TabletDrawnBy.Length > 0)
                        {
                            ProjectDrawnButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Submit";
                        }
                        else if (pss.ProjectStartedTablet.Length > 0)
                        {
                            ProjectStartedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Finish";
                        }
                        else
                        {
                            ProjectEnteredButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Start";
                        }
                    }
                    else if ((bool)pss.Tablet && (bool)pss.Tools)
                    {
                        if (pss.ToolCheckedBy.Length > 0)
                        {
                            ProjectCheckedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                        }
                        else if (pss.ToolDrawnBy.Length > 0)
                        {
                            ProjectDrawnButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Check";
                        }
                        else if (pss.ProjectStartedTool.Length > 0)
                        {
                            ProjectStartedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Finish";
                        }
                        else if (pss.TabletCheckedBy.Length > 0)
                        {
                            ProjectEnteredButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Start";
                        }
                        else if (!(pss.TabletSubmittedBy is null))
                        {
                            ProjectSubmittedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Check";
                        }
                        else if (pss.TabletDrawnBy.Length > 0)
                        {
                            ProjectDrawnButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Submit";
                        }
                        else if (pss.ProjectStartedTablet.Length > 0)
                        {
                            ProjectStartedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Finish";
                        }
                        else
                        {
                            ProjectEnteredButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Start";
                        }
                    }
                    if (!(bool)pss.Tablet && (bool)pss.Tools)
                    {
                        if (pss.ToolCheckedBy.Length > 0)
                        {
                            ProjectCheckedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                        }
                        else if (pss.ToolDrawnBy.Length > 0)
                        {
                            ProjectSubmittedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Check";
                        }
                        else if (pss.ProjectStartedTool.Length > 0)
                        {
                            ProjectStartedButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Finish";
                        }
                        else
                        {
                            ProjectEnteredButtons(projectOnHoldButton, projectOffHoldButton, projectNextStepButton, projectCompleteButton, projectCancelButton);
                            nextStep = "Start";
                        }
                    }

                    selectedProjects.Add((col0val, col1val, checkBox, type, nextStep));

                    // Cancel: Enabled, "Cancel Project"
                    projectCancelButton.IsEnabled = true;
                    projectCancelButton.ToolTip = "Cancel Project";
                }
                // Order
                else if (order)
                {
                    Button orderToOfficeButton = buttons.SingleOrDefault(b => b.Name == "OrderToOfficeButton");
                    Button orderDoNotProcessButton = buttons.SingleOrDefault(b => b.Name == "OrderDoNotProcessButton");
                    Button orderCanProcessButton = buttons.SingleOrDefault(b => b.Name == "OrderCanProcessButton");
                    Button orderStartButton = buttons.SingleOrDefault(b => b.Name == "OrderStartButton");
                    Button orderFinishButton = buttons.SingleOrDefault(b => b.Name == "OrderFinishButton");
                    Button orderCheckButton = buttons.SingleOrDefault(b => b.Name == "OrderCheckButton");
                    Button orderNotFinishedButton = buttons.SingleOrDefault(b => b.Name == "OrderNotFinishedButton");
                    Button orderToProductionButton = buttons.SingleOrDefault(b => b.Name == "OrderToProductionButton");

                    foreach (Button button in buttons)
                    {
                        if (button.Name != "ExpandButton" && button.Name != "CollapseButton")
                        {
                            button.Visibility = Visibility.Visible;
                            if (button.Name != "WindowButton" && button.Name != "ExpandButton" && button.Name != "CollapseButton")
                            {
                                button.IsEnabled = false;
                            }
                        }
                    }

                    using var __ = new NAT02Context();
                    EoiAllOrdersView _order = __.EoiAllOrdersView.Single(o => o.OrderNumber == double.Parse(col0val));
                    __.Dispose();

                    // Get location to determine button functions
                    switch (type)
                    {
                        case "BeingEntered":
                            // To Office: Enabled, "Send To Office"
                            orderToOfficeButton.IsEnabled = true;
                            orderToOfficeButton.ToolTip = "Send To Office";
                            break;
                        case "InTheOffice":
                            // Start Order: Enabled, "Start Order"
                            orderStartButton.IsEnabled = true;
                            orderStartButton.ToolTip = "Start Order";
                            break;
                        case "EnteredUnscanned":
                            // To Office: Enabled, "Send To Office"
                            orderToOfficeButton.IsEnabled = true;
                            orderToOfficeButton.ToolTip = "Send To Office";

                            // Start Order: Enabled, "Start Order"
                            orderStartButton.IsEnabled = true;
                            orderStartButton.ToolTip = "Start Order";
                            break;
                        case "InEngineering":
                            // To Office: Enabled, "Send To Office"
                            orderToOfficeButton.IsEnabled = true;
                            orderToOfficeButton.ToolTip = "Send To Office";

                            if (_order.MarkedForChecking == 0)
                            {
                                // Finish Order: Enabled, "Mark As Finished"
                                orderFinishButton.IsEnabled = true;
                                orderFinishButton.ToolTip = "Mark As Finished";
                            }
                            else
                            {
                                // Check Order: Enabled, "Mark As Checked"
                                //orderCheckButton.IsEnabled = true;
                                //orderCheckButton.ToolTip = "Mark As Checked";
                            }
                            break;
                        case "ReadyToPrint":
                            // To Office: Enabled, "Send To Office"
                            orderToOfficeButton.IsEnabled = true;
                            orderToOfficeButton.ToolTip = "Send To Office";

                            // Not Finish Order: Enabled, "Order Not Finished"
                            orderNotFinishedButton.IsEnabled = true;
                            orderNotFinishedButton.ToolTip = "Order Not Finished";
                            break;
                        case "PrintedInEngineering":
                            // To Office: Enabled, "Send To Office"
                            orderToOfficeButton.IsEnabled = true;
                            orderToOfficeButton.ToolTip = "Send To Office";

                            // Send To Production: Enabled, "Send To Production"
                            orderToProductionButton.IsEnabled = true;
                            orderToProductionButton.ToolTip = "Send To Production";
                            break;
                        default:
                            break;
                    }

                    if (_order.DoNotProcess == 0)
                    {
                        orderDoNotProcessButton.IsEnabled = true;
                        orderCanProcessButton.IsEnabled = false;
                    }
                    else
                    {

                        orderDoNotProcessButton.IsEnabled = false;
                        orderCanProcessButton.IsEnabled = true;
                    }

                    selectedOrders.Add((col0val, checkBox, type));
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CheckBox_Checked App.xaml.cs", ex.Message, user);
            }
            finally
            {
                _.Dispose();
            }
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                var x = (checkBox.Parent as Grid).Children;

                GetAncestorWithoutName(checkBox, typeof(ListBox));
                var type = (grid as ListBox).Name[0..^7];
                grid = null;

                // Get DockPanel for buttons
                GetAncestor(checkBox, typeof(Grid), "DisplayGrid");
                DockPanel dockPanel = (grid as Grid).Children.OfType<DockPanel>().Single(dp => dp.Name == "TitleDockPanel");
                grid = null;

                bool quote = type.Contains("Quote");
                bool project = type.Contains("Project");
                bool order = (!type.Contains("Quote") &&
                              !type.Contains("Project") &&
                              !type.Contains("Queue") &&
                              !type.Contains("NatoliOrderList"));

                // Get Buttons
                var buttons = dockPanel.Children.OfType<StackPanel>().First().Children.OfType<Button>();

                string col0val = (x[1] as TextBlock).Text;
                string col1val = (x[2] as TextBlock).Text;
                if (quote)
                {
                    selectedQuotes.Remove((col0val, col1val, checkBox, type));
                }
                else if (project)
                {
                    string nextStep = selectedProjects.Single(p => p.Item1 == col0val).Item5;
                    selectedProjects.Remove((col0val, col1val, checkBox, type, nextStep));
                }
                else if (order)
                {
                    selectedOrders.Remove((col0val, checkBox, type));
                }

                foreach (Button button in buttons)
                {
                    if (button.Name != "ExpandButton" && button.Name != "CollapseButton")
                    {
                        button.Visibility = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CheckBox_Unchecked App.xaml.cs", ex.Message, user);
            }
        }
        private void ProjectOnHoldButtons(Button projectOnHoldButton, Button projectOffHoldButton, Button projectNextStepButton,
                                          Button projectCompleteButton, Button projectCancelButton)
        {
            // On Hold: Disabled, No Tooltip
            projectOnHoldButton.IsEnabled = false;
            projectOnHoldButton.ToolTip = "";

            // Off Hold: Enabled, "Take Off Hold"
            projectOffHoldButton.IsEnabled = true;
            projectOffHoldButton.ToolTip = "Take Off Hold";

            // Next Step: Disabled, No Tooltip
            projectNextStepButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Collapsed;
            projectNextStepButton.IsEnabled = false;
            projectNextStepButton.ToolTip = "";

            // Complete: Disabled, No Tooltip
            projectCompleteButton.Visibility = user.Department == "Customer Service" ? Visibility.Visible : Visibility.Collapsed;
            projectCompleteButton.IsEnabled = false;
            projectCompleteButton.ToolTip = "";
        }
        private void ProjectCheckedButtons(Button projectOnHoldButton, Button projectOffHoldButton, Button projectNextStepButton,
                                           Button projectCompleteButton, Button projectCancelButton)
        {
            // On Hold: Enabled, "Put On Hold"
            projectOnHoldButton.IsEnabled = true;
            projectOnHoldButton.ToolTip = "On Hold";

            // Off Hold: Disabled, No Tooltip
            projectOffHoldButton.IsEnabled = false;
            projectOffHoldButton.ToolTip = "";

            // Next Step: Disabled, No Tooltip
            projectNextStepButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Collapsed;
            projectNextStepButton.IsEnabled = false;
            projectNextStepButton.ToolTip = "";

            // Complete: Enabled, "Mark As Complete"
            projectCompleteButton.Visibility = (user.Department == "Customer Service" || user.EmployeeCode == "E4408" || user.EmployeeCode == "E4754") ? Visibility.Visible : Visibility.Collapsed;
            projectCompleteButton.IsEnabled = true;
            projectCompleteButton.ToolTip = "Mark As Complete";
        }
        private void ProjectSubmittedButtons(Button projectOnHoldButton, Button projectOffHoldButton, Button projectNextStepButton,
                                             Button projectCompleteButton, Button projectCancelButton)
        {
            // On Hold: Enabled, "Put On Hold"
            projectOnHoldButton.IsEnabled = true;
            projectOnHoldButton.ToolTip = "On Hold";

            // Off Hold: Disabled, No Tooltip
            projectOffHoldButton.IsEnabled = false;
            projectOffHoldButton.ToolTip = "";

            // Next Step: Enabled, "Mark As Checked"
            projectNextStepButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Collapsed;
            projectNextStepButton.IsEnabled = true;
            projectNextStepButton.ToolTip = "Mark As Checked";
            //RemoveEventHandlers(projectNextStepButton);
            projectNextStepButton.Click += CheckProject_Click;

            // Complete: Disabled, No Tooltip
            projectCompleteButton.Visibility = user.Department == "Customer Service" ? Visibility.Visible : Visibility.Collapsed;
            projectCompleteButton.IsEnabled = false;
            projectCompleteButton.ToolTip = "";
        }
        private void ProjectDrawnButtons(Button projectOnHoldButton, Button projectOffHoldButton, Button projectNextStepButton,
                                         Button projectCompleteButton, Button projectCancelButton)
        {
            // On Hold: Enabled, "Put On Hold"
            projectOnHoldButton.IsEnabled = true;
            projectOnHoldButton.ToolTip = "On Hold";

            // Off Hold: Disabled, No Tooltip
            projectOffHoldButton.IsEnabled = false;
            projectOffHoldButton.ToolTip = "";

            // Next Step: Enabled, "Mark As Submitted"
            projectNextStepButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Collapsed;
            projectNextStepButton.IsEnabled = true;
            projectNextStepButton.ToolTip = "Mark As Submitted";
            //RemoveEventHandlers(projectNextStepButton);
            projectNextStepButton.Click += SubmitProject_Click;

            // Complete: Disabled, No Tooltip
            projectCompleteButton.Visibility = user.Department == "Customer Service" ? Visibility.Visible : Visibility.Collapsed;
            projectCompleteButton.IsEnabled = false;
            projectCompleteButton.ToolTip = "";
        }
        private void ProjectStartedButtons(Button projectOnHoldButton, Button projectOffHoldButton, Button projectNextStepButton,
                                           Button projectCompleteButton, Button projectCancelButton)
        {
            // On Hold: Enabled, "Put On Hold"
            projectOnHoldButton.IsEnabled = true;
            projectOnHoldButton.ToolTip = "On Hold";

            // Off Hold: Disabled, No Tooltip
            projectOffHoldButton.IsEnabled = false;
            projectOffHoldButton.ToolTip = "";

            // Next Step: Enabled, "Mark As Drawn"
            projectNextStepButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Collapsed;
            projectNextStepButton.IsEnabled = true;
            projectNextStepButton.ToolTip = "Mark As Drawn";
            //RemoveEventHandlers(projectNextStepButton);
            projectNextStepButton.Click += FinishProject_Click;

            // Complete: Disabled, No Tooltip
            projectCompleteButton.Visibility = user.Department == "Customer Service" ? Visibility.Visible : Visibility.Collapsed;
            projectCompleteButton.IsEnabled = false;
            projectCompleteButton.ToolTip = "";
        }
        private void ProjectEnteredButtons(Button projectOnHoldButton, Button projectOffHoldButton, Button projectNextStepButton,
                                           Button projectCompleteButton, Button projectCancelButton)
        {
            // On Hold: Enabled, "Put On Hold"
            projectOnHoldButton.IsEnabled = true;
            projectOnHoldButton.ToolTip = "On Hold";

            // Off Hold: Disabled, No Tooltip
            projectOffHoldButton.IsEnabled = false;
            projectOffHoldButton.ToolTip = "";

            // Next Step: Enabled, "Mark As Started"
            projectNextStepButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Collapsed;
            projectNextStepButton.IsEnabled = true;
            projectNextStepButton.ToolTip = "Mark As Started";
            //RemoveEventHandlers(projectNextStepButton);
            projectNextStepButton.Click += StartProject_Click;

            // Complete: Disabled, No Tooltip
            projectCompleteButton.Visibility = user.Department == "Customer Service" ? Visibility.Visible : Visibility.Collapsed;
            projectCompleteButton.IsEnabled = false;
            projectCompleteButton.ToolTip = "";
        }
        private void RemoveEventHandlers(Control el)
        {
            FieldInfo fi = typeof(Control).GetField("EventClick",
                BindingFlags.Static | BindingFlags.NonPublic);
            object obj = fi.GetValue(el);
            PropertyInfo pi = el.GetType().GetProperty("Events",
                BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)pi.GetValue(el, null);
            list.RemoveHandler(obj, list[obj]);
        }
        private void ToggleButton_SingleClick(object sender, MouseButtonEventArgs e)
        {
            double_click_timer.Stop();
            click_count++;
            checkBox = (VisualTreeHelper.GetChild(sender as ToggleButton, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<CheckBox>().First();
            
            GetAncestorWithoutName(checkBox, typeof(ListBox));
            var type = (grid as ListBox).Name[0..^7];
            grid = null;

            if (type.Contains("Quote"))
            {
                docNumber = "Q" + (VisualTreeHelper.GetChild(sender as ToggleButton, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteNumber").Text;
                docNumber += "-" + (VisualTreeHelper.GetChild(sender as ToggleButton, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteRev").Text;
            }
            else if (type.Contains("Project"))
            {
                docNumber = "P" + (VisualTreeHelper.GetChild(sender as ToggleButton, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "ProjectNumber").Text;
            }
            else
            {
                docNumber = "O" + (VisualTreeHelper.GetChild(sender as ToggleButton, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumber").Text;
            }
            double_click_timer.Start();
        }
        public void Double_Click_Timer_Elapsed(object sender, EventArgs e)
        {
            double_click_timer.Stop();

            if (click_count == 2)
            {
                // Set cursor to waiting
                (Application.Current.MainWindow as MainWindow).Cursor = Cursors.AppStarting;

                // Open Quote, Order, or Project folder
                if (docNumber[0] == 'P')
                {
                    OpenProject(docNumber[1..]);
                }
                else if (docNumber[0] == 'Q')
                {
                    OpenQuote(docNumber[1..]);
                }
                else if (docNumber[0] == 'O')
                {
                    OpenWorkOrder(docNumber[1..]);
                }

                // Set cursor to pointer
                (Application.Current.MainWindow as MainWindow).Cursor = Cursors.Arrow;
            }
            else
            {
                checkBox.IsChecked = !checkBox.IsChecked;
            }

            click_count = 0;
        }
        #region Open Documents
        private void OpenProject(string projectNumber)
        {
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
        private void OpenQuote(string qNumber)
        {
            using var nat01context = new NAT01Context();
            Quote quote = null;
            MainWindow window = null;
            try
            {
                int quoteNumber = int.Parse(qNumber[0..^2]);
                short revNumber = qNumber[^1..] == "-" ? (short)0 : short.Parse(qNumber[^1..]);
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
                    if (w.Title.StartsWith("Natoli Order Interface"))
                    {
                        window = w as MainWindow;
                    }
                }
                quote = new Quote(quoteNumber, revNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, window, user)
                {
                    //Owner = parent,
                    Left = MainWindow.Left,
                    Top = MainWindow.Top
                };
                quoteInfoWindow.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        AlreadyOpen:
            nat01context.Dispose();
        }
        private void OpenWorkOrder(string orderNumber)
        {
            using var _context = new NAT02Context();
            using var _nat01context = new NAT01Context();
            Window parent = Window.GetWindow(checkBox);
            MainWindow mainWindow = null;
            WorkOrder workOrder = null;

            try
            {
                workOrder = new WorkOrder(int.Parse(orderNumber), parent);
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
                    else if (w.GetType().FullName.Contains("MainWindow"))
                    {
                        mainWindow = w as MainWindow;
                    }
                }
                if (_context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User != user.GetUserName()))
                {
                    MessageBox.Show("BEWARE!!\n" + _context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User != user.GetUserName()).FirstOrDefault().User + " is in this order at the moment.");
                }
                else if (_context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User == user.GetUserName()))
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
                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, mainWindow, null, user)
                {
                    //Owner = parent,
                    Left = MainWindow.Left,
                    Top = MainWindow.Top
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
        }
        #endregion
        #region Document Movements
        #region Orders
        private void SendToOfficeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            WorkOrder workOrder = null;
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count > 0)
            {
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    var item = (((((((order.Item2.Parent as Grid).Parent as Border).TemplatedParent as ToggleButton).Parent as Grid).Parent as Grid).Parent as DockPanel).Parent as Border);
                    var item2 = ((((item.TemplatedParent as Expander).Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First();
                    workOrder = new WorkOrder(int.Parse(order.Item1), this.MainWindow);
                    int retVal = workOrder.TransferOrder(user, "D080", currModule == "EnteredUnscanned");
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

                    (Window.GetWindow(sender as DependencyObject) as MainWindow).DeleteMachineVariables(workOrder.OrderNumber.ToString());
                }

                try
                {
                    this.MainWindow.Cursor = Cursors.Wait;
                    Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                    Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)
                        app.Application.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                    mailItem.Subject = "REQUEST FOR CHANGES WO# " + string.Join(",", validOrders.Select(o => o.Item1));
                    mailItem.To = IMethods.GetEmailAddress(workOrder.Csr);
                    mailItem.Body = "";
                    mailItem.BCC = "intlcs6@natoli.com;customerservice5@natoli.com";
                    mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
                    mailItem.Display(false);
                    this.MainWindow.Cursor = Cursors.Arrow;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    this.MainWindow.Cursor = Cursors.Arrow;
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

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void StartWorkOrder_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            WorkOrder workOrder = null;
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    var item = (((((((order.Item2.Parent as Grid).Parent as Border).TemplatedParent as ToggleButton).Parent as Grid).Parent as Grid).Parent as DockPanel).Parent as Border);
                    var item2 = ((((item.TemplatedParent as Expander).Parent as StackPanel).Parent as ScrollViewer).Parent as DockPanel).Children.OfType<Grid>().First().Children.OfType<Label>().First();
                    workOrder = new WorkOrder(int.Parse(order.Item1), this.MainWindow);
                    int retVal = workOrder.TransferOrder(user, "D040", currModule == "EnteredUnscanned");
                    if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }

                    // Uncheck order expander
                    order.Item2.IsChecked = false;
                }
            }

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void ToProdManOrder_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            WorkOrder workOrder = null;
            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
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
                    workOrder = new WorkOrder(int.Parse(order.Item1), this.MainWindow);
                    int retVal = workOrder.TransferOrder(user, "D921");
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
            }

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void DoNotProcess_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    using var nat02context = new NAT02Context();
                    EoiOrdersDoNotProcess _ = new EoiOrdersDoNotProcess()
                    {
                        OrderNo = double.Parse(order.Item1),
                        UserName = user.GetUserName()
                    };
                    nat02context.EoiOrdersDoNotProcess.Add(_);
                    nat02context.SaveChanges();
                    nat02context.Dispose();
                }
            }

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void CanProcess_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    using var nat02context = new NAT02Context();
                    if (nat02context.EoiOrdersDoNotProcess.Any(o => o.OrderNo == double.Parse(order.Item1)))
                    {
                        EoiOrdersDoNotProcess _ = nat02context.EoiOrdersDoNotProcess.Single(o => o.OrderNo == double.Parse(order.Item1));
                        nat02context.EoiOrdersDoNotProcess.Remove(_);
                    }
                    nat02context.SaveChanges();
                    nat02context.Dispose();
                }
            }

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void NotFinished_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    using var nat02context = new NAT02Context();
                    if (nat02context.EoiOrdersMarkedForChecking.Any(o => o.OrderNo == double.Parse(order.Item1)))
                    {
                        EoiOrdersMarkedForChecking _ = nat02context.EoiOrdersMarkedForChecking.Single(o => o.OrderNo == double.Parse(order.Item1));
                        nat02context.EoiOrdersMarkedForChecking.Remove(_);
                    }
                    nat02context.SaveChanges();
                    nat02context.Dispose();
                }
            }

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void FinishOrder_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            // Scan selected orders if there are any and then clear the list
            if (selectedOrders.Count != 0)
            {
                List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

                int count = validOrders.Count;
                for (int i = 0; i < count; i++)
                {
                    (string, CheckBox, string) order = validOrders[i];
                    (VisualTreeHelper.GetParent((order.Item2.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    using var nat02context = new NAT02Context();
                    if (!nat02context.EoiOrdersMarkedForChecking.Any(o => o.OrderNo == double.Parse(order.Item1)))
                    {
                        EoiOrdersMarkedForChecking _ = new EoiOrdersMarkedForChecking()
                        {
                            OrderNo = double.Parse(order.Item1)
                        };
                        nat02context.EoiOrdersMarkedForChecking.Add(_);
                    }
                    nat02context.SaveChanges();
                    nat02context.Dispose();
                }
            }

            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        //private void CheckOrder_Click(object sender, RoutedEventArgs e)
        //{
        //    // New list of projects that are in the same module that was right clicked inside of
        //    string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

        //    // Scan selected orders if there are any and then clear the list
        //    if (selectedOrders.Count != 0)
        //    {
        //        List<(string, CheckBox, string)> validOrders = selectedOrders.Where(p => p.Item3 == currModule).ToList();

        //        int count = validOrders.Count;
        //        for (int i = 0; i < count; i++)
        //        {

        //        }
        //    }

        //    (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        //}
        #endregion
        #region Quotes
        private void CompletedQuoteCheck_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            using var _nat02context = new NAT02Context();

            if (selectedProjects.Count > 0)
            {
                List<(string, string, CheckBox, string)> validQuotes = selectedQuotes.Where(p => p.Item4 == currModule).ToList();

                try
                {
                    if (validQuotes.Any())
                    {
                        for (int i = 0; i < validQuotes.Count; i++)
                        {
                            (string, string, CheckBox, string) quote = validQuotes[i];
                            quote.Item3.IsChecked = false;
                            (VisualTreeHelper.GetParent((quote.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

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
            }
            
            _nat02context.SaveChanges();
            _nat02context.Dispose();
            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void SubmitQuote_Click(object sender, RoutedEventArgs e)
        {
            this.MainWindow.Cursor = Cursors.AppStarting;
            using var context = new NAT01Context();
            using var nat02Context = new NAT02Context();
            using var necContext = new NECContext();
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
            List<(string, string, CheckBox, string)> validQuotes = selectedQuotes.Where(p => p.Item4 == currModule).ToList();
            List<Tuple<int, short>> quotes = new List<Tuple<int, short>>();
            List<Quote> quoteItems = new List<Quote>();
            List<string> quoteErrorNumbers = new List<string>();
            if (validQuotes.Any())
            {

                for (int i = 0; i < validQuotes.Count; i++)
                {
                    quotes.Add(new Tuple<int, short>(Convert.ToInt32(validQuotes[i].Item1), Convert.ToInt16(validQuotes[i].Item2)));
                }
                OrderingWindow orderingWindow = new OrderingWindow(quotes, user);
                if (orderingWindow.ShowDialog() == true)
                {
                    foreach (Tuple<int, short> quote in quotes)
                    {
                        if (IMethods.QuoteErrors(quote.Item1.ToString(), quote.Item2.ToString(), user).Count > 0)
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
                                CsrMarked = user.GetUserName(),
                                TimeSubmitted = DateTime.Now,
                                Rush = r.RushYorN
                            };
                            nat02Context.EoiQuotesMarkedForConversion.Add(q);
                            quote.Dispose();
                        }
                    }
                }
            }

            nat02Context.SaveChanges();
            nat02Context.Dispose();
            necContext.Dispose();
            context.Dispose();
            this.MainWindow.Cursor = Cursors.Arrow;
            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        private void RecallQuote_Click(object sender, RoutedEventArgs e)
        {
            Quote quote = null;
            this.MainWindow.Cursor = Cursors.AppStarting;
            using var context = new NAT01Context();
            using var nat02Context = new NAT02Context();
            using var necContext = new NECContext();
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
            List<(string, string, CheckBox, string)> validQuotes = selectedQuotes.Where(p => p.Item4 == currModule).ToList();

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
                        CsrMarked = user.GetUserName(),
                        TimeSubmitted = DateTime.Now,
                        Rush = r.RushYorN
                    };
                    nat02Context.EoiQuotesMarkedForConversion.Remove(q);
                }
            }
            nat02Context.SaveChanges();
            nat02Context.Dispose();
            necContext.Dispose();
            context.Dispose();
            this.MainWindow.Cursor = Cursors.Arrow;
            (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
        }
        #endregion
        #region Projects
        private void NextStepProject_Click(object sender, RoutedEventArgs e)
        {
            // New list of projects that are in the same module that was right clicked inside of
            string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];

            projectsToMove = selectedProjects.Where(p => p.Item5 == selectedProjects.Last().Item5).ToList();
        }
        private void StartProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects" && validProjects[0].Item5 == "Start")
                {
                    StartTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects" && validProjects[0].Item5 == "Start")
                {
                    StartToolProject(validProjects);

                    selectedProjects.Clear();
                    projectsToMove.Clear();
                }

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void FinishProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (projectsToMove.Count > 0)
                {
                    // New list of projects that are in the same module that was right clicked inside of
                    string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                    List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                    if (currModule == "AllTabletProjects" && validProjects[0].Item5 == "Finish")
                    {
                        FinishTabletProject(validProjects);
                    }
                    else if (currModule == "AllToolProjects" && validProjects[0].Item5 == "Finish")
                    {
                        FinishToolProject(validProjects);
                        selectedProjects.Clear();
                        projectsToMove.Clear();

                        (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
                    }
                }
            }
            catch (Exception ex) { 
            }
            
        }
        private void SubmitProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects" && validProjects[0].Item5 == "Submit")
                {
                    SubmitTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects")
                {
                    SubmitToolProject(validProjects);

                    selectedProjects.Clear();
                    projectsToMove.Clear();
                }

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void CheckProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects" && validProjects[0].Item5 == "Check")
                {
                    CheckTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects" && validProjects[0].Item5 == "Check")
                {
                    CheckToolProject(validProjects);
                    selectedProjects.Clear();
                    projectsToMove.Clear();
                }

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void OnHoldProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects")
                {
                    OnHoldTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects")
                {
                    OnHoldToolProject(validProjects);
                }
                else
                {
                    throw new NotImplementedException();
                }

                selectedProjects.Clear();
                projectsToMove.Clear();

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void OffHoldProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects")
                {
                    OffHoldTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects")
                {
                    OffHoldToolProject(validProjects);
                }
                else
                {
                    throw new NotImplementedException();
                }

                selectedProjects.Clear();
                projectsToMove.Clear();

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void CompleteProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects")
                {
                    CompleteTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects")
                {
                    CompleteToolProject(validProjects);
                }
                else
                {
                    throw new NotImplementedException();
                }

                selectedProjects.Clear();
                projectsToMove.Clear();

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void CancelProject_Click(object sender, RoutedEventArgs e)
        {
            if (projectsToMove.Count > 0)
            {
                // New list of projects that are in the same module that was right clicked inside of
                string currModule = ((((sender as Button).Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Name[0..^7];
                List<(string, string, CheckBox, string, string)> validProjects = projectsToMove.Where(p => p.Item4 == currModule).ToList();

                if (currModule == "AllTabletProjects")
                {
                    CancelTabletProject(validProjects);
                }
                else if (currModule == "AllToolProjects")
                {
                    CancelToolProject(validProjects);
                }
                else
                {
                    throw new NotImplementedException();
                }

                selectedProjects.Clear();
                projectsToMove.Clear();

                (Window.GetWindow(sender as DependencyObject) as MainWindow).MainRefresh(currModule);
            }
        }
        private void StartTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();

                    // Get project revision number
                    // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                    string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;

                    // Insert into StartedBy

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.StartProject(project.Item1, project.Item2, "TABLETS", user);
                    }
                    else
                    {
                        ProjectStartedTablet tabletProjectStarted = new ProjectStartedTablet();
                        tabletProjectStarted.ProjectNumber = int.Parse(project.Item1);
                        tabletProjectStarted.RevisionNumber = int.Parse(project.Item2);
                        tabletProjectStarted.TimeSubmitted = DateTime.Now;
                        tabletProjectStarted.ProjectStartedTablet1 = user.GetUserName().Split(' ')[0] == "Floyd" ? "Joe" :
                                                                     user.GetUserName().Split(' ')[0] == "Ronald" ? "Ron" :
                                                                     user.GetUserName().Split(' ')[0] == "Phyllis" ? new InputBox("Drafter?", "Whom?", this.MainWindow).ReturnString : user.GetUserName().Split(' ')[0];
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
                    IMethods.WriteToErrorLog("StartTabletProject_CLick", ex.Message, user);
                }
            }
        }
        private void StartToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.StartProject(project.Item1, project.Item2, "TOOLS", user);
                    }
                    else
                    {
                        // Get project revision number
                        // int? _revNo = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == _projectNumber).First().RevisionNumber;
                        string _csr = _projectsContext.ProjectSpecSheet.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First().Csr;
                        string usrName = user.GetUserName().Split(" ")[0];
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
                            toolProjectStarted.ProjectStartedTool1 = user.GetUserName().Split(' ')[0];
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
                    IMethods.WriteToErrorLog("StartToolProject_Click", ex.Message, user);
                }
            }
        }
        private void FinishTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.DrawProject(project.Item1, project.Item2, "TABLETS", user);
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
                        tabletDrawnBy.TabletDrawnBy1 = user.GetUserName().Split(' ')[0] == "Floyd" ? "Joe" :
                                                       user.GetUserName().Split(' ')[0] == "Ronald" ? "Ron" :
                                                       user.GetUserName().Split(' ')[0] == "Phyllis" ? new InputBox("Drafter?", "Whom?", this.MainWindow).ReturnString : user.GetUserName().Split(' ')[0];
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
                    IMethods.WriteToErrorLog("FinishTabletProject_Click", ex.Message, user);
                }
            }
        }
        private void FinishToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();


                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.DrawProject(project.Item1, project.Item2, "TOOLS", user);
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
                        toolDrawnBy.ToolDrawnBy1 = user.GetUserName().Split(' ')[0];
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
                    IMethods.WriteToErrorLog("FinishToolProject_Click", ex.Message, user);
                }
            }
        }
        private void SubmitTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.SubmitProject(project.Item1, project.Item2, "TABLETS", user);
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
                        tabletSubmittedBy.TabletSubmittedBy1 = user.GetUserName().Split(' ')[0] == "Floyd" ? "Joe" :
                                                               user.GetUserName().Split(' ')[0] == "Ronald" ? "Ron" : user.GetUserName().Split(' ')[0];
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
                    IMethods.WriteToErrorLog("SubmitTabletProject_Click", ex.Message, user);
                }
            }
        }
        private void SubmitToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            
        }
        private void CheckTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _projectsContext = new ProjectsContext();
                    using var _driveworksContext = new DriveWorksContext();
                    using var _nat02Context = new NAT02Context();

                    if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                    {
                        IMethods.CheckProject(project.Item1, project.Item2, "TABLETS", user);
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
                        tabletCheckedBy.TabletCheckedBy1 = user.GetUserName().Split(' ')[0];
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
                                IMethods.SendProjectCompletedEmailToCSRAsync(_CSRs, project.Item1, project.Item2, user);
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
                    IMethods.WriteToErrorLog("CheckTabletProject_Click", ex.Message, user);
                }
            }
        }
        private void CheckToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                        (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                        using var _projectsContext = new ProjectsContext();
                        using var _driveworksContext = new DriveWorksContext();


                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                        {
                            IMethods.CheckProject(project.Item1, project.Item2, "TOOLS", user);
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
                            toolCheckedBy.ToolCheckedBy1 = user.GetUserName().Split(' ')[0];
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
                            IMethods.SendProjectCompletedEmailToCSRAsync(_CSRs, int.Parse(project.Item1).ToString(), int.Parse(project.Item2).ToString(), user);

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
                        IMethods.WriteToErrorLog("CheckToolProject_Click", ex.Message, user);
                    }
                }
            }
        }
        private void OnHoldTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            try
            {
                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string, string) project = validProjects[i];

                    // Uncheck project expander
                    project.Item3.IsChecked = false;
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    OnHoldCommentWindow onHoldCommentWindow = new OnHoldCommentWindow("Tablets", int.Parse(project.Item1), int.Parse(project.Item2), this.MainWindow as MainWindow, user)
                    {
                        Left = this.MainWindow.Left,
                        Top = this.MainWindow.Top
                    };
                    onHoldCommentWindow.Show();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("OnHoldTabletProject_Click", ex.Message, user);
            }
        }
        private void OnHoldToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            try
            {
                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string, string) project = validProjects[i];

                    // Uncheck project expander
                    project.Item3.IsChecked = false;
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    OnHoldCommentWindow onHoldCommentWindow = new OnHoldCommentWindow("Tools", int.Parse(project.Item1), int.Parse(project.Item2), this.MainWindow as MainWindow, user)
                    {
                        Left = this.MainWindow.Left,
                        Top = this.MainWindow.Top
                    };
                    onHoldCommentWindow.Show();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("OnHoldToolProject_Click", ex.Message, user);
            }
        }
        private void OffHoldTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            try
            {
                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string, string) project = validProjects[i];

                    // Uncheck project expander
                    project.Item3.IsChecked = false;
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    int _projectNumber = int.Parse(project.Item1);
                    int _revNumber = int.Parse(project.Item2);

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
                }

                _projectsContext.SaveChanges();
                _driveworksContext.SaveChanges();
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                IMethods.WriteToErrorLog("OffHoldTabletProject_Click", ex.Message, user);
            }
        }
        private void OffHoldToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            try
            {
                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();

                for (int i = 0; i < validProjects.Count; i++)
                {
                    (string, string, CheckBox, string, string) project = validProjects[i];

                    // Uncheck project expander
                    project.Item3.IsChecked = false;
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;
                    int _projectNumber = int.Parse(project.Item1);
                    int _revNumber = int.Parse(project.Item2);

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

                        // Drive specification transition name to "Off Hold - Tools"
                        string _name = _projectNumber.ToString() + (_revNumber > 0 ? "_" + _revNumber : "");
                        Specifications spec = _driveworksContext.Specifications.Where(s => s.Name == _name).First();
                        spec.StateName = "Off Hold - Tools";
                        _driveworksContext.Specifications.Update(spec);
                    }
                }

                _projectsContext.SaveChanges();
                _driveworksContext.SaveChanges();
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
            }
            catch (Exception ex)
            {
                // MessageBox.Show(ex.Message);
                IMethods.WriteToErrorLog("OffHoldToolProject_Click", ex.Message, user);
            }
        }
        private void CompleteTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _nat02Context = new NAT02Context();
                    EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Where(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2)).First();
                    _nat02Context.EoiProjectsFinished.Remove(projectsFinished);
                    _nat02Context.Dispose();
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    IMethods.WriteToErrorLog("CompleteTabletProject_Click", ex.Message, user);
                }
            }
        }
        private void CompleteToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            for (int i = 0; i < validProjects.Count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
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
                    (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

                    using var _nat02Context = new NAT02Context();
                    EoiProjectsFinished projectsFinished = _nat02Context.EoiProjectsFinished.Single(p => p.ProjectNumber == int.Parse(project.Item1) && p.RevisionNumber == int.Parse(project.Item2));
                    _nat02Context.EoiProjectsFinished.Remove(projectsFinished);
                    _nat02Context.Dispose();
                }
                catch (Exception ex)
                {
                    // MessageBox.Show(ex.Message);
                    IMethods.WriteToErrorLog("CompleteToolProject_Click", ex.Message, user);
                }
            }
        }
        private void CancelTabletProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            int count = validProjects.Count;
            for (int i = 0; i < count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();
                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                {
                    IMethods.CancelProject(project.Item1, project.Item2, user);
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
                            (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

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
                            IMethods.WriteToErrorLog("SetOnHold", ex.Message, user);
                        }
                    }
                }
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
            }
        }
        private void CancelToolProject(List<(string, string, CheckBox, string, string)> validProjects)
        {
            int count = validProjects.Count;
            for (int i = 0; i < count; i++)
            {
                (string, string, CheckBox, string, string) project = validProjects[i];
                using var _projectsContext = new ProjectsContext();
                using var _driveworksContext = new DriveWorksContext();

                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == project.Item1 && p.RevNumber == project.Item2))
                {
                    IMethods.CancelProject(project.Item1, project.Item2, user);
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
                            (VisualTreeHelper.GetParent((project.Item3.Parent as Grid).Parent as Grid) as ToggleButton).IsChecked = false;

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
                            IMethods.WriteToErrorLog("SetOnHold", ex.Message, user);
                        }
                    }
                }
                _projectsContext.Dispose();
                _driveworksContext.Dispose();
            }
        }
        #endregion

        #endregion
    }
}