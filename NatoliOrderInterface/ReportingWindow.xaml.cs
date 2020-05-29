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

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for ReportingWindow.xaml
    /// </summary>
    public partial class ReportingWindow : Window
    {
        public ReportingWindow(Window _parent)
        {
            InitializeComponent();
            BeginningDatePicker.SelectedDate = DateTime.Now.AddDays(-14);
            EndDatePicker.SelectedDate = DateTime.Now;

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
                    Values = new ChartValues<int>(ordersReport.Where(or => or.TabletProjects > 0).Select(or => or.TabletProjects))
                });

                sc.Add(new RowSeries
                {
                    Title = "Tool Projects",
                    Values = new ChartValues<int>(ordersReport.Where(or => or.ToolProjects > 0).Select(or => or.ToolProjects))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();

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
                YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();

                string[] Labels = ordersReport.Select(or => or.Employee).ToArray();
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
                    Title = "Tool Projects",
                    Values = new ChartValues<int>(ordersReport.Select(or => or.ToolProjects))
                });
                YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();

                string[] Labels = ordersReport.Select(or => or.Employee).ToArray();
            }
            _.Dispose();
            SetLoadingAnimationVisibility(Visibility.Hidden);
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                ProductionChart.Series = sc;
                ProductionChart.Visibility = ProductionChart.Series.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            }));
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
            BuildOrdersChart((bool)Orders.IsChecked, (bool)Tablets.IsChecked, (bool)Tools.IsChecked, BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
        }

        private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EndDatePicker.SelectedDate == null || BeginningDatePicker.SelectedDate == null || BeginningDatePicker.SelectedDate.Value >= EndDatePicker.SelectedDate.Value)
            {
                ProductionChart.Visibility = Visibility.Hidden;
            }
            else
            {
                BuildOrdersChart((bool)Orders.IsChecked, (bool)Tablets.IsChecked, (bool)Tools.IsChecked, BeginningDatePicker.SelectedDate, EndDatePicker.SelectedDate);
            }
        }
    }
}
