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

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        private ListBox OrdersListBox = new ListBox();
        private List<EoiOrdersEnteredAndUnscannedView> orders = new List<EoiOrdersEnteredAndUnscannedView>();
        public List<EoiOrdersEnteredAndUnscannedView> Orders
        {
            get
            {
                return orders;
            }
            set
            {
                if (value.Except(orders).Count() > 0 || orders.Except(value).Count() > 0)
                {
                    orders = value;
                    OrdersListBox.ItemsSource = null;
                    OrdersListBox.ItemsSource = orders;
                }
            }
        }
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
        public TestWindow()
        {
            InitializeComponent();

            OrdersEnteredModule.ApplyTemplate();
            OrdersInEngineeringModule.ApplyTemplate();

            Grid grid = (VisualTreeHelper.GetChild(OrdersEnteredModule as DependencyObject, 0) as Grid);
            OrdersListBox = grid.Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
            grid = (VisualTreeHelper.GetChild(OrdersInEngineeringModule as DependencyObject, 0) as Grid);
            OrdersInEngListBox = grid.Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

            List<EoiOrdersEnteredAndUnscannedView> eoiOrdersEnteredAndUnscannedView = new List<EoiOrdersEnteredAndUnscannedView>();
            List<EoiOrdersInEngineeringUnprintedView> eoiOrdersInEngineeringUnprintedView = new List<EoiOrdersInEngineeringUnprintedView>();
            using var _ = new NAT02Context();
            eoiOrdersEnteredAndUnscannedView = _.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();
            eoiOrdersInEngineeringUnprintedView = _.EoiOrdersInEngineeringUnprintedView.OrderByDescending(o => o.DaysInEng).ThenBy(o => o.NumDaysToShip).ToList();
            _.Dispose();

            Orders = eoiOrdersEnteredAndUnscannedView;
            OrdersInEng = eoiOrdersInEngineeringUnprintedView;
        }
    }
}
