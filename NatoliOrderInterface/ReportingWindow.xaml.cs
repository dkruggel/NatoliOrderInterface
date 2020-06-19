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
            
            SeriesCollection sc = new SeriesCollection();
            if (ordersChecked)
            {
                SetLoadingAnimationVisibility(Visibility.Visible);
                sc = await BuildOrdersChart(beginningDateTime, endDateTime);
            }
            else if (tabletsChecked)
            {
                SetLoadingAnimationVisibility(Visibility.Visible);
                sc = await BuildTabletsChart(beginningDateTime, endDateTime);
            }
            else if(toolsChecked)
            {
                SetLoadingAnimationVisibility(Visibility.Visible);
                sc = await BuildToolsChart(beginningDateTime, endDateTime);
            }
            SetLoadingAnimationVisibility(Visibility.Hidden);
            await Dispatcher.BeginInvoke((Action)(() =>
            {
                ProductionChart.Series = sc;
                ProductionChart.Visibility = ProductionChart.Series.Count > 0 ? Visibility.Visible : Visibility.Hidden;
            }));
        }
        private async Task<SeriesCollection> BuildOrdersChart(DateTime? beginningDateTime, DateTime? endDateTime)
        {
            SeriesCollection sc = new SeriesCollection();
            string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_Orders @StartDate = {0}, @EndDate = {1}";

            string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };

            List<OrdersReport> ordersReport = new List<OrdersReport>();
            using var _ = new ProjectsContext();
            await Task.Run((Action)(() =>
            {
                ordersReport = _.OrdersReport.FromSqlRaw(orderReportQuery, dates).ToList();
            }));
            _.Dispose();


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
            return sc;
        }
        private async Task<SeriesCollection> BuildTabletsChart( DateTime? beginningDateTime, DateTime? endDateTime)
        {
            SeriesCollection sc = new SeriesCollection();
            using var _ = new ProjectsContext();
            string tabletProjectsReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_TabletProject_Start_End @StartDate = {0}, @EndDate = {1}";
            string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };
            List<TabletProjectsReportStartEnd> tabletProjectsReport = new List<TabletProjectsReportStartEnd>();
            await Task.Run((Action)(() =>
            {
                tabletProjectsReport = _.TabletProjectsReportStartEnd.FromSqlRaw(tabletProjectsReportQuery, dates).ToList();
            }));
            _.Dispose();
            List<(string Drafter, decimal Hours, int Projects)> drafters = new List<(string Drafter, decimal Hours, int Projects)>();
            foreach (string drafter in tabletProjectsReport.Select(o => o.Drafter).Distinct())
            {
                List<TabletProjectsReportStartEnd> draftersProjects = tabletProjectsReport.Where(o => o.Drafter == drafter).ToList();
                List<Interval> intervals = new List<Interval>();
                foreach (TabletProjectsReportStartEnd project in draftersProjects.Where(p => p.Minutes > 1))
                {
                    intervals.Add(Interval.CreateInterval(project.TabletStartedDateTime, project.TabletDrawnDateTime));
                }
                intervals = Interval.MergeOverlappingIntervals(intervals.AsEnumerable()).ToList();
                TimeSpan totalTime = Interval.GetTimeSpanOfIntervals(intervals.AsEnumerable());
                int totalProjects = draftersProjects.Count;
                decimal hours = intervals.Count > 0 ? Convert.ToDecimal(totalTime.TotalSeconds / 3600) / Convert.ToDecimal(draftersProjects.Count(p => p.Minutes > 1)) : -1;
                drafters.Add((drafter, hours, totalProjects));
            }
            drafters = drafters.OrderBy(d => d.Projects).ToList();
            sc.Add(new RowSeries
            {
                Title = "Tablet Projects",
                Values = new ChartValues<int>(drafters.Select(or => or.Projects))
            });
            YAxis.Labels = drafters.Select(or => or.Drafter + ": " + Math.Round(or.Hours, 2)).ToList();
            YAxis.FontSize = 25;
            YAxis.Foreground = new SolidColorBrush(Colors.Black);
            return sc;
        }
        private async Task<SeriesCollection> BuildToolsChart(DateTime? beginningDateTime, DateTime? endDateTime)
        {
            SeriesCollection sc = new SeriesCollection();
            string toolProjectsReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction_ToolProject_Start_End @StartDate = {0}, @EndDate = {1}";
            string[] dates = new string[] { beginningDateTime.Value.ToShortDateString(), endDateTime.Value.ToShortDateString() };
            List<ToolProjectsReportStartEnd> toolProjectsReport = new List<ToolProjectsReportStartEnd>();
            using var _ = new ProjectsContext();
            await Task.Run((Action)(() =>
            {
                toolProjectsReport = _.ToolProjectsReportStartEnd.FromSqlRaw(toolProjectsReportQuery, dates).ToList();
            }));
            _.Dispose();
            List<(string Drafter, decimal Hours, int Projects)> drafters = new List<(string Drafter, decimal Hours, int Projects)>();
            foreach (string drafter in toolProjectsReport.Select(o => o.Drafter).Distinct())
            {
                List<ToolProjectsReportStartEnd> draftersProjects = toolProjectsReport.Where(o => o.Drafter == drafter).ToList();
                List<Interval> intervals = new List<Interval>();
                foreach (ToolProjectsReportStartEnd project in draftersProjects.Where(p => p.Minutes > 1))
                {
                    intervals.Add(Interval.CreateInterval(project.ToolStartedDateTime, project.ToolDrawnDateTime));
                }
                intervals = Interval.MergeOverlappingIntervals(intervals.AsEnumerable()).ToList();
                TimeSpan totalTime = Interval.GetTimeSpanOfIntervals(intervals.AsEnumerable());
                int totalProjects = draftersProjects.Count;
                decimal hours = intervals.Count > 0 ? Convert.ToDecimal(totalTime.TotalSeconds / 3600) / Convert.ToDecimal(draftersProjects.Count(p => p.Minutes > 1)) : -1;
                drafters.Add((drafter, hours, totalProjects));
            }
            drafters = drafters.OrderBy(d => d.Projects).ToList();
            sc.Add(new RowSeries
            {
                Title = "Tool Projects",
                Values = new ChartValues<int>(drafters.Select(or => or.Projects))
            });
            YAxis.Labels = drafters.Select(or => or.Drafter + ": " + Math.Round(or.Hours, 2)).ToList();
            YAxis.FontSize = 25;
            YAxis.Foreground = new SolidColorBrush(Colors.Black);
            return sc;
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
