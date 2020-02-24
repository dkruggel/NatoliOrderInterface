using NatoliOrderInterface.Models.NAT01;
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
using System.Linq;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.Projects;
using NatoliOrderInterface.Models;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for CustomerInfoWindow.xaml
    /// </summary>
    public partial class CustomerInfoWindow : Window
    {
        private string CustomerNumber { get; set; }
        private string CustomerName { get; set; }
        private User user;
        private MainWindow parent;

        public CustomerInfoWindow(User _user, MainWindow _parent, string _customerNumber)
        {
            CustomerNumber = _customerNumber;
            user = _user;
            parent = _parent;

            using var _ = new NECContext();
            CustomerName = _.Rm00101.First(r => r.Custnmbr.Trim().ToLower() == CustomerNumber.Trim().ToLower()).Custname.Trim();
            _.Dispose();

            InitializeComponent();

            FillQuoteList();
            FillOrderList();
            FillProjectList();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CustomerNameTextBlock.Text = CustomerNumber + " - " + CustomerName;

            Title = CustomerName;
        }

        private void FillQuoteList()
        {
            using var _ = new NAT01Context();
            List<QuoteHeader> quoteHeader = _.QuoteHeader.Where(q => (q.UserAcctNo.Trim() == CustomerNumber || q.ShipToAccountNo.Trim() == CustomerNumber || q.CustomerNo.Trim() == CustomerNumber) && q.OrderNo == 0 &&
                                                                !_.OrderHeader.Where(o => o.UserAcctNo == CustomerNumber).Select(o => o.QuoteNumber).Contains((double)q.QuoteNo))
                                                         .OrderByDescending(q => q.QuoteNo).ToList();

            _.Dispose();

            foreach (QuoteHeader quote in quoteHeader)
            {
                ContentControl contentControl = new ContentControl()
                {
                    Style = FindResource("QuoteGrid") as Style
                };

                using var __ = new NECContext();
                string customerName = __.Rm00101.First(r => r.Custnmbr == quote.UserAcctNo.Trim() || r.Custnmbr == quote.ShipToAccountNo.Trim() || r.Custnmbr == quote.CustomerNo.Trim()).Custname.Trim();
                __.Dispose();

                contentControl.ApplyTemplate();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteNumberTextBlock").Text = quote.QuoteNo.ToString();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteRevNumberTextBlock").Text = quote.QuoteRevNo.ToString();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = customerName;

                QuoteDockPanel.Children.Add(contentControl);
            }
        }

        private void FillOrderList()
        {
            using var _ = new NAT01Context();
            List<OrderHeader> orderHeader = _.OrderHeader.Where(o => o.UserAcctNo.Trim() == CustomerNumber || o.CustomerNo.Trim() == CustomerNumber || o.ShipToAccountNo == CustomerNumber)
                                                         .OrderByDescending(o => o.OrderNo).ToList();

            _.Dispose();

            foreach (OrderHeader order in orderHeader)
            {
                ContentControl contentControl = new ContentControl()
                {
                    Style = FindResource("OrderGrid") as Style
                };

                bool notShipped = order.ShippedYn.Trim() == "N";
                bool rush = order.RushYorN == "Y" || order.PaidRushFee == "Y";

                using var __ = new NECContext();
                string customerName = __.Rm00101.First(r => r.Custnmbr == order.UserAcctNo || r.Custnmbr == order.CustomerNo || r.Custnmbr == order.ShipToAccountNo).Custname.Trim();
                __.Dispose();

                contentControl.ApplyTemplate();

                foreach (TextBlock tb in (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>())
                {
                    if (notShipped) { tb.FontStyle = FontStyles.Oblique; }
                    if (rush) { tb.Foreground = new SolidColorBrush(Colors.DarkRed); }
                }

                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = (order.OrderNo / 100).ToString();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteNumberTextBlock").Text = order.QuoteNumber.ToString();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteRevNumberTextBlock").Text = order.QuoteRevNo.ToString();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = customerName;

                OrderDockPanel.Children.Add(contentControl);
            }
        }

        private void FillProjectList()
        {
            using var _ = new ProjectsContext();
            List<ProjectSpecSheet> projectSpecSheet = _.ProjectSpecSheet.Where(p => (p.InternationalId != "N/A" ? p.InternationalId == CustomerNumber : p.CustomerNumber == CustomerNumber) &&
                                                                                    // ((!(bool)p.Tools && string.IsNullOrEmpty(p.TabletCheckedBy)) || ((bool)p.Tools && string.IsNullOrEmpty(p.ToolCheckedBy))) &&
                                                                                    p.HoldStatus != "CANCELED" && p.HoldStatus != "CANCELLED" && p.HoldStatus != "ON HOLD")
                                                        .OrderByDescending(p => p.ProjectNumber).ToList();

            _.Dispose();

            foreach (ProjectSpecSheet project in projectSpecSheet)
            {
                ContentControl contentControl = new ContentControl()
                {
                    Style = FindResource("ProjectGrid") as Style
                };

                SolidColorBrush back;
                SolidColorBrush fore;
                FontWeight fontWeight;
                FontStyle fontStyle;
                bool priority = project.MarkedPriority is null ? false : project.MarkedPriority == "PRIORITY";
                using var nat02context = new NAT02Context();
                bool finished = nat02context.EoiProjectsFinished.Where(p => p.ProjectNumber == project.ProjectNumber && p.RevisionNumber == project.RevisionNumber).Any() ||
                                ((bool)project.Tools && project.ToolCheckedBy.Length > 0) ||
                                ((bool)project.Tablet && !(bool)project.Tools && project.TabletCheckedBy.Length > 0);
                nat02context.Dispose();
                bool onHold = project.HoldStatus == "On Hold";
                string stage = "";

                // Tablet
                //       Entered:   tablet=true, tabletstartedby=""
                //       Started:   tablet=true, tabletdrawnby=""
                //       Drawn:     tablet=true, tabletsubmittedby=""
                //       Submitted: tablet=true, tabletcheckedby=""
                //       Finished:  tablet=true, tool=false, projectfinished=true
                // Tool
                //       Entered:   tools=true, toolstartedby=""
                //       Started:   tools=true, tooldrawnby=""
                //       Drawn:     tools=true, toolcheckedby=""
                //       Finished:  tools=true, projectfinished=true

                bool tablet = (bool)project.Tablet;
                bool tool = (bool)project.Tools;
                bool tabletEntered = false;
                bool tabletStarted = false;
                bool tabletDrawn = false;
                bool tabletSubmitted = false;
                bool tabletFinished = false;
                bool toolEntered = false;
                bool toolStarted = false;
                bool toolDrawn = false;
                bool toolFinished = false;

                if (tablet && !tool)
                {
                    tabletFinished = finished;
                    if (tabletFinished)
                    {
                        stage = "Tablet Finished";
                        goto JumpPoint;
                    }
                    tabletSubmitted = project.TabletSubmittedBy is null ? false : project.TabletSubmittedBy.Length > 0;
                    if (tabletSubmitted)
                    {
                        stage = "Tablet Being Checked";
                        goto JumpPoint;
                    }
                    tabletDrawn = project.TabletDrawnBy.Length > 0;
                    if (tabletDrawn)
                    {
                        stage = "Tablet Drawn";
                        goto JumpPoint;
                    }
                    tabletStarted = project.ProjectStartedTablet.Length > 0;
                    if (tabletStarted)
                    {
                        stage = "Tablet Started";
                        goto JumpPoint;
                    }
                    tabletEntered = !tabletSubmitted && !tabletDrawn && !tabletStarted && !tabletFinished;
                    if (tabletEntered)
                    {
                        stage = "Tablet Entered";
                        goto JumpPoint;
                    }
                }
                else if (tablet && tool)
                {
                    toolFinished = finished;
                    if (toolFinished)
                    {
                        stage = "Tool Finished";
                        goto JumpPoint;
                    }
                    toolDrawn = project.ToolDrawnBy.Length > 0;
                    if (toolDrawn)
                    {
                        stage = "Tool Drawn";
                        goto JumpPoint;
                    }
                    toolStarted = project.ProjectStartedTool.Length > 0;
                    if (toolStarted)
                    {
                        stage = "Tool Started";
                        goto JumpPoint;
                    }
                    toolEntered = !toolDrawn && !toolStarted && !toolFinished;
                    if (tabletFinished)
                    {
                        stage = "Tablet Finished";
                        goto JumpPoint;
                    }
                    tabletSubmitted = project.TabletSubmittedBy is null ? false : project.TabletSubmittedBy.Length > 0;
                    if (tabletSubmitted)
                    {
                        stage = "Tablet Being Checked";
                        goto JumpPoint;
                    }
                    tabletDrawn = project.TabletDrawnBy.Length > 0;
                    if (tabletDrawn)
                    {
                        stage = "Tablet Drawn";
                        goto JumpPoint;
                    }
                    tabletStarted = project.ProjectStartedTablet.Length > 0;
                    if (tabletStarted)
                    {
                        stage = "Tablet Started";
                        goto JumpPoint;
                    }
                    tabletEntered = !tabletSubmitted && !tabletDrawn && !tabletStarted && !tabletFinished;
                    if (tabletEntered)
                    {
                        stage = "Tablet Entered";
                        goto JumpPoint;
                    }
                }
                else
                {
                    toolFinished = finished;
                    if (toolFinished)
                    {
                        stage = "Tool Finished";
                        goto JumpPoint;
                    }
                    toolDrawn = project.ToolDrawnBy.Length > 0;
                    if (toolDrawn)
                    {
                        stage = "Tool Drawn";
                        goto JumpPoint;
                    }
                    toolStarted = project.ProjectStartedTool.Length > 0;
                    if (toolStarted)
                    {
                        stage = "Tool Started";
                        goto JumpPoint;
                    }
                    toolEntered = !toolDrawn && !toolStarted && !toolFinished;
                    if (toolEntered)
                    {
                        stage = "Tool Entered";
                        goto JumpPoint;
                    }
                }

                JumpPoint:
                if (tool)
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
                    // back = new SolidColorBrush(Colors.MediumPurple);
                }
                else if (finished)
                {
                    // back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFADFF2F"));
                }
                else if (tabletSubmitted)
                {
                    // back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF0A7DFF"));
                }
                else if (tabletDrawn || toolDrawn)
                {
                    // back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF52A3FF"));
                }
                else if (tabletStarted || toolStarted)
                {
                    // back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFB2D6FF"));
                }
                else
                {
                    // back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00FFFFFF"));
                }

                back = (SolidColorBrush)(new BrushConverter().ConvertFrom("#00FFFFFF"));

                using var __ = new NECContext();
                string customerName = __.Rm00101.Single(r => r.Custnmbr == (project.InternationalId == "N/A" ? project.CustomerNumber : project.InternationalId)).Custname.Trim();
                __.Dispose();

                contentControl.ApplyTemplate();

                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Background = back;

                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "ProjectNumberTextBlock").Text = project.ProjectNumber.ToString();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "ProjectStageTextBlock").Text = stage;
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = customerName;

                ProjectDockPanel.Children.Add(contentControl);
            }
        }

        private void OpenQuote_Click(object sender, MouseButtonEventArgs e)
        {
            using var nat01context = new NAT01Context();
            Image image = sender as Image;
            Grid grid = (image.Parent as StackPanel).Parent as Grid;
            Quote quote = null;

            Cursor = Cursors.AppStarting;
            try
            {
                int quoteNumber = int.Parse((grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteNumberTextBlock") as TextBlock).Text);
                short revNumber = short.Parse((grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteRevNumberTextBlock") as TextBlock).Text);
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
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, parent, user)
                {
                    Owner = this,
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

        private void OpenOrder_Click(object sender, MouseButtonEventArgs e)
        {
            using var _context = new NAT02Context();
            using var _nat01context = new NAT01Context();
            Image image = sender as Image;
            Grid grid = (image.Parent as StackPanel).Parent as Grid;
            Cursor = Cursors.AppStarting;
            WorkOrder workOrder = null;

            try
            {
                TextBlock textBlock = grid.Children.OfType<TextBlock>().First() as TextBlock;
                string orderNumber = textBlock.Text;
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
                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, parent, null, user)
                {
                    //Owner = parent,
                    Left = parent.Left,
                    Top = parent.Top
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
            Cursor = Cursors.Arrow;
        }

        private void OpenProject_Click(object sender, MouseButtonEventArgs e)
        {
            Image image = sender as Image;
            Grid grid = (image.Parent as StackPanel).Parent as Grid;
            Cursor = Cursors.AppStarting;
            try
            {
                string projectNumber = (grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "ProjectNumberTextBlock") as TextBlock).Text;
                string revNumber = (grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "ProjectRevNumberTextBlock") as TextBlock).Text;
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
    }
}
