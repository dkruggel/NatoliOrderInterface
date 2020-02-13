﻿using System;
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
            parent = _parent;
            Owner = _parent;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            InitializeComponent();
            FillNotifications();
        }

        private void FillNotifications()
        {
            using var _ = new NAT02Context();
            List<EoiNotificationsActive> active = _.EoiNotificationsActive.OrderBy(a => a.Timestamp).ToList();
            List<EoiNotificationsViewed> archived = _.EoiNotificationsViewed.OrderBy(a => a.Timestamp).ToList();

            List<(int, string, string, string, bool, string)> notifications = new List<(int, string, string, string, bool, string)>();

            foreach (EoiNotificationsActive a in active)
            {
                using var __ = new NAT01Context();
                using var ___ = new NECContext();
                string acctNo = __.OrderHeader.Single(o => o.OrderNo / 100 == double.Parse(a.Number)).UserAcctNo;
                string custName = ___.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Custname;
                notifications.Add((a.Id, a.Number, custName, a.Message, true, a.Type));
                __.Dispose();
                ___.Dispose();
            }
            foreach (EoiNotificationsViewed a in archived)
            {
                using var __ = new NAT01Context();
                using var ___ = new NECContext();
                string acctNo = __.OrderHeader.Single(o => o.OrderNo / 100 == double.Parse(a.Number)).UserAcctNo;
                string custName = ___.Rm00101.Single(r => r.Custnmbr.Trim() == acctNo.Trim()).Custname.Trim();
                notifications.Add((a.NotificationId, a.Number, custName, a.Message, false, a.Type));
                __.Dispose();
                ___.Dispose();
            }

            notifications.OrderBy(n => n.Item1);

            //foreach ((int, string, string, string, bool, string) notification in notifications)
            //{
            //    ContentControl contentControl = new ContentControl()
            //    {
            //        Style = notification.Item5 ? FindResource("ActiveNotificationGrid") as Style :
            //                                     FindResource("InactiveNotificationGrid") as Style
            //    };

            //    contentControl.ApplyTemplate();
            //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = notification.Item2;
            //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<TextBlock>().Single(tb => tb.Name == "CustomerNameTextBlock").Text = notification.Item3;
            //    (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<TextBlock>().Single(tb => tb.Name == "NotificationMessageTextBlock").Text =
            //        notification.Item4.Replace("Document", notification.Item6);

            //    NotificationDockPanel.Children.Add(contentControl);
            //}

            for (int i = 0; i < 10; i++)
            {
                ContentControl contentControl = new ContentControl()
                {
                    Style = FindResource("ActiveNotificationGrid") as Style
                };
                NotificationDockPanel.Children.Add(contentControl);
            }

            _.Dispose();
        }

        private void ArchiveNotification_Click(object sender, RoutedEventArgs e)
        {
            string orderNumber = (((sender as Image).Parent as Grid).Parent as Grid).Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text;
            using var _ = new NAT02Context();

            if (_.EoiNotificationsActive.Count(n => n.Number == orderNumber) > 0)
            {
                EoiNotificationsActive active = _.EoiNotificationsActive.Single(n => n.Number == orderNumber);

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
            else if (_.EoiNotificationsViewed.Count(n => n.Number == orderNumber) > 0)
            {
                EoiNotificationsViewed viewed = _.EoiNotificationsViewed.Single(n => n.Number == orderNumber);

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

            _.Dispose();
        }

        private void OpenOrder_Click(object sender, RoutedEventArgs e)
        {
            using var _context = new NAT02Context();
            using var _nat01context = new NAT01Context();
            Image image = sender as Image;
            Grid grid = (image.Parent as Grid).Parent as Grid;
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
    }
}
