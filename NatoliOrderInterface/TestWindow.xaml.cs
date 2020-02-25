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
        public ObservableCollection<EoiOrdersEnteredAndUnscannedView> ordersCollection;
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
        public TestWindow()
        {
            InitializeComponent();

            AllOrdersModule.ApplyTemplate();
            DockPanel dockPanel = (VisualTreeHelper.GetChild(AllOrdersModule as DependencyObject, 0) as DockPanel);
            OrdersListBox = dockPanel.Children.OfType<ListBox>().First();

            List<EoiOrdersEnteredAndUnscannedView> EoiOrdersEnteredAndUnscannedView = new List<EoiOrdersEnteredAndUnscannedView>();
            using var _ = new NAT02Context();
            EoiOrdersEnteredAndUnscannedView = _.EoiOrdersEnteredAndUnscannedView.OrderBy(o => o.OrderNo).ToList();
            ordersCollection = new ObservableCollection<EoiOrdersEnteredAndUnscannedView>(EoiOrdersEnteredAndUnscannedView);
            _.Dispose();

            Orders = EoiOrdersEnteredAndUnscannedView;
        }
    }
}
