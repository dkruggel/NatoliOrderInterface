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
using NatoliOrderInterface.Models.NAT01;
using System.Linq;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.Projects;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for NotificationManagementWindow.xaml
    /// </summary>
    public partial class NotificationManagementWindow : Window
    {
        private User user;
        private MainWindow parent;

        public NotificationManagementWindow(User _user, MainWindow _parent)
        {
            user = _user;
            parent = _parent;;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            FillNotifications();
            this.Focus();
        }

        private void FillNotifications()
        {
            NM_DockPanel.Children.Clear();

            using var _ = new NAT02Context();
            List<EoiNotificationsActive> active = _.EoiNotificationsActive.Where(n => n.User == user.DomainName).OrderBy(a => a.Timestamp).ToList();
            List<EoiNotificationsViewed> viewed = _.EoiNotificationsViewed.Where(n => n.User == user.DomainName).OrderBy(a => a.Timestamp).ToList();

            List<(int, string, string, string, bool, string)> notifications = new List<(int, string, string, string, bool, string)>();

            foreach (EoiNotificationsActive a in active)
            {
                using var __ = new NAT01Context();
                using var ___ = new NECContext();
                using var _projectsContext = new ProjectsContext();
                string acctNo = __.OrderHeader.Single(o => o.OrderNo / 100 == double.Parse(a.Number)).UserAcctNo;
                string custName = "";
                if (a.Type == "Project")
                {
                    if (_projectsContext.EngineeringProjects.Any(ep => ep.ProjectNumber == a.Number))
                    {
                        string endUserName = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == a.Number).EndUserName;
                        string customerName = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == a.Number).CustomerName;
                        string shipToName = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == a.Number).ShipToName;
                        custName =
                            !string.IsNullOrEmpty(endUserName) && !string.IsNullOrWhiteSpace(endUserName) ? endUserName :
                            !string.IsNullOrEmpty(customerName) && !string.IsNullOrWhiteSpace(customerName) ? customerName :
                            !string.IsNullOrEmpty(shipToName) && !string.IsNullOrWhiteSpace(shipToName) ? shipToName : "";
                    }
                    else if(_projectsContext.EngineeringArchivedProjects.Any(eap => eap.ProjectNumber == a.Number))
                    {
                        string rev = _projectsContext.EngineeringArchivedProjects.Where(eap => eap.ProjectNumber == a.Number).Max(p => p.RevNumber);

                        string endUserName = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == a.Number && eap.RevNumber == rev).EndUserName;
                        string customerName = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == a.Number && eap.RevNumber == rev).CustomerName;
                        string shipToName = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == a.Number && eap.RevNumber == rev).ShipToName;
                        custName =
                            !string.IsNullOrEmpty(endUserName) && !string.IsNullOrWhiteSpace(endUserName) ? endUserName :
                            !string.IsNullOrEmpty(customerName) && !string.IsNullOrWhiteSpace(customerName) ? customerName :
                            !string.IsNullOrEmpty(shipToName) && !string.IsNullOrWhiteSpace(shipToName) ? shipToName : "";
                    }
                }
                else
                {
                    custName = ___.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Custname;
                }

                notifications.Add((a.Id, a.Number, custName, a.Message, true, a.Type));
                __.Dispose();
                ___.Dispose();
                _projectsContext.Dispose();
            }
            foreach (EoiNotificationsViewed v in viewed)
            {
                using var __ = new NAT01Context();
                using var ___ = new NECContext();
                using var _projectsContext = new ProjectsContext();
                string acctNo = __.OrderHeader.Single(o => o.OrderNo / 100 == double.Parse(v.Number)).UserAcctNo;
                string custName = "";
                if (v.Type == "Project")
                {
                    if (_projectsContext.EngineeringProjects.Any(ep => ep.ProjectNumber == v.Number))
                    {
                        string endUserName = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == v.Number).EndUserName;
                        string customerName = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == v.Number).CustomerName;
                        string shipToName = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == v.Number).ShipToName;
                        custName =
                            !string.IsNullOrEmpty(endUserName) && !string.IsNullOrWhiteSpace(endUserName) ? endUserName :
                            !string.IsNullOrEmpty(customerName) && !string.IsNullOrWhiteSpace(customerName) ? customerName :
                            !string.IsNullOrEmpty(shipToName) && !string.IsNullOrWhiteSpace(shipToName) ? shipToName : "";
                    }
                    else if (_projectsContext.EngineeringArchivedProjects.Any(eap => eap.ProjectNumber == v.Number))
                    {
                        string rev = _projectsContext.EngineeringArchivedProjects.Where(eap => eap.ProjectNumber == v.Number).Max(p => p.RevNumber);

                        string endUserName = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == v.Number && eap.RevNumber == rev).EndUserName;
                        string customerName = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == v.Number && eap.RevNumber == rev).CustomerName;
                        string shipToName = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == v.Number && eap.RevNumber == rev).ShipToName;
                        custName =
                            !string.IsNullOrEmpty(endUserName) && !string.IsNullOrWhiteSpace(endUserName) ? endUserName :
                            !string.IsNullOrEmpty(customerName) && !string.IsNullOrWhiteSpace(customerName) ? customerName :
                            !string.IsNullOrEmpty(shipToName) && !string.IsNullOrWhiteSpace(shipToName) ? shipToName : "";
                    }
                }
                else
                {
                    custName = ___.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Custname;
                }
                notifications.Add((v.NotificationId, v.Number, custName, v.Message, false, v.Type));
                __.Dispose();
                ___.Dispose();
                _projectsContext.Dispose();
            }

            notifications.OrderBy(n => n.Item1);

            foreach ((int, string, string, string, bool, string) notification in notifications)
            {
                ContentControl contentControl = new ContentControl()
                {
                    Style = notification.Item5 ? FindResource("ActiveNotificationGrid") as Style :
                                                 FindResource("InactiveNotificationGrid") as Style
                };

                contentControl.ApplyTemplate();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = notification.Item2;
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = notification.Item3;
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<TextBlock>().Single(tb => tb.Name == "NotificationMessageTextBlock").Text =
                    notification.Item4.Replace("Document", notification.Item6);

                NM_DockPanel.Children.Add(contentControl);
            }

            //List<EoiAllOrdersView> orders = _.EoiAllOrdersView.OrderBy(o => o.OrderNumber).ToList();

            //foreach (EoiAllOrdersView order in orders)
            //{
            //    ContentControl contentControl = new ContentControl()
            //    {
            //        Style = FindResource("OrderGrid") as Style
            //    };

            //    contentControl.ApplyTemplate();
            //    Grid grid = (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<Grid>().First();

            //    string location = "";
            //    string state = "";

            //    if (order.BeingEntered == 1)
            //    {
            //        location = "Order Entry";
            //        state = "Being Converted to Order";
            //    }
            //    else if (order.EnteredUnscanned == 1)
            //    {
            //        location = "Order Entry/Eng.";
            //        state = "Ready for Engineering";
            //    }
            //    else if (order.InEngineering == 1)
            //    {
            //        location = "Engineering";
            //        if (order.BeingChecked == 1)
            //        {
            //            state = "Being Checked";
            //        }
            //        else if (order.MarkedForChecking == 1)
            //        {
            //            state = "Ready to be Checked";
            //        }
            //        else
            //        {
            //            state = "Being Drawn";
            //        }
            //    }
            //    else if (order.ReadyToPrint == 1)
            //    {
            //        location = "Engineering";
            //        state = "Ready to Print";
            //    }
            //    else if (order.Printed == 1)
            //    {
            //        location = "Engineering";
            //        state = "Printed/Ready for Production";
            //    }
            //    else if (order.InTheOffice == 1)
            //    {
            //        location = "Office";
            //        state = "Sent to Office";
            //    }

            //    if (order.DoNotProcess == 1)
            //    {
            //        grid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#55FFC0CB"));
            //    }
            //    else if (order.DoNotProcess == 1)
            //    {
            //        grid.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#55FFC0CB"));
            //    }

            //    if (order.RushYorN == "Y" || order.PaidRushFee == "Y")
            //    {
            //        foreach (TextBlock textBlock in grid.Children.OfType<TextBlock>()) { textBlock.Foreground = FindResource("Tertiary.Dark") as SolidColorBrush; }
            //    }

            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = order.OrderNumber.ToString();
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteNumberTextBlock").Text = order.QuoteNumber.ToString();
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "QuoteRevNumberTextBlock").Text = order.QuoteRev.ToString();
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = order.CustomerName.Trim();
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "NumDaysToShipTextBlock").Text = order.NumDaysToShip.ToString();
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "NumDaysInDeptTextBlock").Text = order.DaysInDept.ToString();
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "EmployeeTextBlock").Text = order.EmployeeName;
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "LocationTextBlock").Text = location;
            //    grid.Children.OfType<TextBlock>().Single(tb => tb.Name == "StateTextBlock").Text = state;

            //    NM_DockPanel.Children.Add(contentControl);
            //}

            //for (int i = 0; i < 10; i++)
            //{
            //    ContentControl contentControl = new ContentControl()
            //    {
            //        Style = FindResource("ActiveNotificationGrid") as Style
            //    };
            //    NM_DockPanel.Children.Add(contentControl);
            //}

            _.Dispose();
        }

        private void ArchiveNotification_Click(object sender, RoutedEventArgs e)
        {
            string orderNumber = (((sender as Image).Parent as StackPanel).Parent as Grid).Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text;
            using var _ = new NAT02Context();

            if (_.EoiNotificationsActive.Count(n => n.Number == orderNumber) > 0)
            {
                List<EoiNotificationsActive> actives = _.EoiNotificationsActive.Where(n => n.Number == orderNumber).ToList();

                foreach (EoiNotificationsActive active in actives)
                {
                    EoiNotificationsArchived archived = new EoiNotificationsArchived()
                    {
                        NotificationId = active.Id,
                        Number = active.Number,
                        Type = active.Type,
                        Message = active.Message,
                        User = active.User,
                        Timestamp = DateTime.Now
                    };

                    _.EoiNotificationsActive.Remove(active);
                    _.EoiNotificationsArchived.Add(archived);
                }
            }
            else if (_.EoiNotificationsViewed.Count(n => n.Number == orderNumber) > 0)
            {
                List<EoiNotificationsViewed> vieweds = _.EoiNotificationsViewed.Where(n => n.Number == orderNumber).ToList();

                foreach (EoiNotificationsViewed viewed in vieweds)
                {
                    EoiNotificationsArchived archived = new EoiNotificationsArchived()
                    {
                        NotificationId = viewed.NotificationId,
                        Number = viewed.Number,
                        Type = viewed.Type,
                        Message = viewed.Message,
                        User = viewed.User,
                        Timestamp = DateTime.Now
                    };

                    _.EoiNotificationsViewed.Remove(viewed);
                    _.EoiNotificationsArchived.Add(archived);
                }
            }

            _.SaveChanges();
            _.Dispose();

            FillNotifications();
        }

        private void OpenOrder_Click(object sender, RoutedEventArgs e)
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using var _ = new NAT02Context();
            List<EoiNotificationsActive> active = _.EoiNotificationsActive.Where(n => n.User == user.DomainName).ToList();

            foreach (EoiNotificationsActive a in active)
            {
                EoiNotificationsViewed viewed = new EoiNotificationsViewed()
                {
                    NotificationId = a.Id,
                    Type = a.Type,
                    Number = a.Number,
                    Message = a.Message,
                    User = a.User,
                    Timestamp = DateTime.Now
                };

                _.EoiNotificationsViewed.Add(viewed);

                _.SaveChanges();

                _.EoiNotificationsActive.Remove(a);

                _.SaveChanges();
            }

            _.Dispose();

            parent.SetNotificationPicture();
        }

        private void NotificationMAnageMentWindow_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
