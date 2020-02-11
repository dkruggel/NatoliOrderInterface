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
            for (int i = 0; i < 20; i++)
            {
                ContentControl contentControl = new ContentControl()
                {
                    Style = FindResource("ActiveNotificationGrid") as Style
                };

                contentControl.ApplyTemplate();
                (VisualTreeHelper.GetChild(contentControl as DependencyObject, 0) as Grid).Children.OfType<TextBlock>().Single(tb => tb.Name == "OrderNumberTextBlock").Text = (276000 + i).ToString();

                NotificationDockPanel.Children.Add(contentControl);
            }
        }

        private void ArchiveNotification_Click(object sender, RoutedEventArgs e)
        {

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
