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
using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using System.Linq;
using NatoliOrderInterface.Models.Projects;
using LiveCharts;
using LiveCharts.Wpf;
using LiveCharts.Definitions.Series;
using NatoliOrderInterface.Models.NAT01;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for ReportingWindow.xaml
    /// </summary>
    public partial class ReportingWindow : Window
    {
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        private readonly MainWindow parent;

        public ReportingWindow(MainWindow _parent)
        {
            InitializeComponent();
            parent = _parent ?? new MainWindow();
            IntPtr hwnd = new WindowInteropHelper(parent).Handle;
            Rect windowRect = new Rect();
            GetWindowRect(hwnd, ref windowRect);
            Top = windowRect.Top + 8;
            Left = windowRect.Left + 8;
            Width = parent.Width;
            Height = parent.Height;
            BeginningDatePicker.SelectedDate = DateTime.Now.AddDays(-14);
            EndDatePicker.SelectedDate = DateTime.Now;

            if (Environment.UserName != "dkruggel")
            {
                Quotes.Visibility = Visibility.Hidden;
            }
            BuildOrdersChart((bool)Orders.IsChecked, (bool)Tablets.IsChecked, (bool)Tools.IsChecked, BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
        }

        //private void UpdateButton_Click(object sender, RoutedEventArgs e)
        //{
        //    BuildOrdersChart();
        //}

        private async void BuildOrdersChart(bool ordersChecked, bool tabletsChecked, bool toolsChecked, DateTime? beginningDateTime, DateTime? endDateTime)
        {

            SetLoadingAnimationVisibility(Visibility.Visible);
            SeriesCollection sc = new SeriesCollection();
            using var _ = new ProjectsContext();
            if (ordersChecked && tabletsChecked && toolsChecked)
            {
                string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction @StartDate = {0}, @EndDate = {1}";

                string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

                List<OrdersAndProjectsReport> ordersReport = new List<OrdersAndProjectsReport>();
                await Task.Run((Action)(() =>
                {
                    ordersReport = _.OrdersAndProjectsReport.FromSqlRaw(orderReportQuery, dates).ToList();
                }));

                sc.Add(new RowSeries
                {
                    Title = "Orders Scanned In",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.OrdersIn))
                });

                sc.Add(new RowSeries
                {
                    Title = "Orders Scanned Out",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.OrdersOut))
                });

                sc.Add(new RowSeries
                {
                    Title = "Orders To Office",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.OrdersToOffice))
                });

                sc.Add(new RowSeries
                {
                    Title = "Tablet Projects",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.TabletProjects))
                });

                sc.Add(new RowSeries
                {
                    Title = "Tool Projects",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.ToolProjects))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();
                YAxis.FontSize = 25;
                YAxis.Foreground = new SolidColorBrush(Colors.Black);

                string[] Labels = ordersReport.Select(or => or.Employee).ToArray();
            }
            else if (ordersChecked && !tabletsChecked && !toolsChecked)
            {
                string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_Orders @StartDate = {0}, @EndDate = {1}";

                string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

                List<OrdersReport> ordersReport = new List<OrdersReport>();
                await Task.Run((Action)(() =>
                {
                    ordersReport = _.OrdersReport.FromSqlRaw(orderReportQuery, dates).ToList();
                }));


                sc.Add(new RowSeries
                {
                    Title = "Orders Scanned In",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.OrdersIn))
                });

                sc.Add(new RowSeries
                {
                    Title = "Orders Scanned Out",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.OrdersOut))
                });

                sc.Add(new RowSeries
                {
                    Title = "Orders To Office",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.OrdersToOffice))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();
                YAxis.FontSize = 25;
                YAxis.Foreground = new SolidColorBrush(Colors.Black);

                string[] Labels = ordersReport.Select(or => or.Employee).ToArray();
            }
            else if (!ordersChecked && tabletsChecked && toolsChecked)
            {
                string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_Projects @StartDate = {0}, @EndDate = {1}";

                string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

                List<ProjectsReport> ordersReport = new List<ProjectsReport>();
                await Task.Run((Action)(() =>
                {
                    ordersReport = _.ProjectsReport.FromSqlRaw(orderReportQuery, dates).ToList();
                }));


                sc.Add(new RowSeries
                {
                    Title = "Tablet Projects",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.TabletProjects))
                });

                sc.Add(new RowSeries
                {
                    Title = "Tool Projects",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.ToolProjects))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();
                YAxis.FontSize = 25;
                YAxis.Foreground = new SolidColorBrush(Colors.Black);

                string[] Labels = ordersReport.Select(or => or.Employee).ToArray();
            }
            else if (!ordersChecked && tabletsChecked && !toolsChecked)
            {
                string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_Tablets @StartDate = {0}, @EndDate = {1}";

                string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

                List<TabletProjectsReport> ordersReport = new List<TabletProjectsReport>();
                await Task.Run((Action)(() =>
                {
                    ordersReport = _.TabletProjectsReport.FromSqlRaw(orderReportQuery, dates).ToList();
                }));


                sc.Add(new RowSeries
                {
                    Title = "Tablet Projects",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.TabletProjects))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee.Trim() + ':' + or.AverageHours.ToString()).ToList();
                YAxis.FontSize = 25;
                YAxis.Foreground = new SolidColorBrush(Colors.Black);

                string[] Labels = ordersReport.Select(or => or.Employee.Trim() + ':' + or.AverageHours.ToString()).ToArray();
            }
            else if (!ordersChecked && !tabletsChecked && toolsChecked)
            {
                string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_Tools @StartDate = {0}, @EndDate = {1}";

                string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

                List<ToolProjectsReport> ordersReport = new List<ToolProjectsReport>();
                await Task.Run((Action)(() =>
                {
                    ordersReport = _.ToolProjectsReport.FromSqlRaw(orderReportQuery, dates).ToList();
                }));

                sc.Add(new RowSeries
                {
                    Title = "Tool Projects Drawn",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.ToolProjectsDrawn))
                });
                sc.Add(new RowSeries
                {
                    Title = "Tool Projects Checked",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.ToolProjectsChecked))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee.Trim() + ':' + or.AverageHours.ToString()).ToList();
                YAxis.FontSize = 25;
                YAxis.Foreground = new SolidColorBrush(Colors.Black);

                string[] Labels = ordersReport.Select(or => or.Employee + ':' + or.AverageHours.ToString()).ToArray();
            }
            _.Dispose();
            SetLoadingAnimationVisibility(Visibility.Hidden);
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                ProductionChart.Series = sc;
                ProductionChart.Visibility = ProductionChart.Series.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            }));
        }
        private async void BuildCSCharts(DateTime? beginningDateTime, DateTime? endDateTime)
        {
            SetLoadingAnimationVisibility(Visibility.Visible);
            string[] reps = { "Alex Heimberger", "Miral Bouzitoun", "Gregory Lyle", "Humberto Zamora", "Nick Tarte" };
            for (int i = 0; i < reps.Length; i++)
            {
                SeriesCollection sc = new SeriesCollection();
                PieChart pieChart = new PieChart()
                {
                    LegendLocation = LegendLocation.Top,
                    Hoverable = true,
                    Width = 250,
                    Height = 250
                };
                using var _ = new NAT02Context();

                string csReportQuery = "EXECUTE NAT02.dbo.sp_EOI_QuoteConversionByCSR @CSR = {0}, @Beginning = {1}, @End = {2}, @Rep = ''";

                string[] p = new string[] { reps[i], beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

                List<VwQuoteConversion> qc = new List<VwQuoteConversion>();
                await Task.Run((Action)(() =>
                {
                    qc = _.VwQuoteConversion.FromSqlRaw(csReportQuery, p).ToList();
                }));

                sc.Add(new PieSeries
                {
                    Title = "Converted",
                    Values = new ChartValues<int>(qc.Select(q => (int)q.Converted))
                });

                sc.Add(new PieSeries
                {
                    Title = "Not Converted",
                    Values = new ChartValues<int>(qc.Select(q => (int)q.NotConverted))
                });

                await Dispatcher.BeginInvoke((Action)(() =>
                {
                    pieChart.Series = sc;
                    StackPanel _sp = new StackPanel()
                    {
                        Orientation = Orientation.Vertical
                    };
                    _sp.Children.Add(new Label() { Content = reps[i] });
                    _sp.Children.Add(pieChart);
                    ChartStack.Children.Add(_sp);
                }));
            }
            SetLoadingAnimationVisibility(Visibility.Hidden);
            ChartStack.Visibility = Visibility.Visible;
        }
        private async void SetLoadingAnimationVisibility(Visibility visibility)
        {
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                LoadingAnimation.Visibility = visibility;
            }));
        }
        private void QuoteConversionDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string csr = ((sender as DataGrid).SelectedCells[0].Item as VwQuoteConversion).Rep;
            using var _ = new NAT02Context();
            List<double> pct = new List<double>();
            
            _.Dispose();
            // CreateLineGraph(csr, pct);
            // CreateBarGraph(csr);
        }
        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            ChartStack.Children.Clear();
            if ((bool)Orders.IsChecked || (bool)Tablets.IsChecked || (bool)Tools.IsChecked)
            {
                BuildOrdersChart((bool)Orders.IsChecked, (bool)Tablets.IsChecked, (bool)Tools.IsChecked, BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
            }
            else if ((bool)Quotes.IsChecked)
            {
                BuildCSCharts(BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
            }
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndDatePicker.SelectedDate == null || BeginningDatePicker.SelectedDate == null || BeginningDatePicker.SelectedDate.Value >= EndDatePicker.SelectedDate.Value)
            {
                ProductionChart.Visibility = Visibility.Hidden;
            }
            else
            {
                ChartStack.Children.Clear();
                BuildOrdersChart((bool)Orders.IsChecked, (bool)Tablets.IsChecked, (bool)Tools.IsChecked, BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
                if ((bool)Quotes.IsChecked)
                {
                    ChartStack.Children.Clear();
                    BuildCSCharts(BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
                }
            }
        }
    }
}
