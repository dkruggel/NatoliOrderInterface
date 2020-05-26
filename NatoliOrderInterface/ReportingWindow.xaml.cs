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
            //using var _ = new NAT02Context();
            BeginningDatePicker.SelectedDate = DateTime.Parse("2020-01-01");
            EndDatePicker.SelectedDate = DateTime.Parse("2020-02-01");

            BuildOrdersChart();

            //RangePeriodComboBox.SelectedIndex = 0;
            //RangePeriodComboBox.ItemsSource = new List<string>() { "Day(s)", "Week(s)", "Month(s)", "Year(s)" };
            //var vwQuoteConversion = _.VwQuoteConversion.FromSqlRaw("EXEC dbo.sp_EOI_QuoteConversion {0}, {1}", BeginningDatePicker.SelectedDate.Value,
            //                                                                                                   EndDatePicker.SelectedDate.Value);
            //vwQuoteConversion.Load();
            //QuoteConversionDataGrid_Domestic.ItemsSource = _.VwQuoteConversion.Local.ToObservableCollection();
            //_.Dispose();

            //BeginningTextBox.Text = "1";
            //EndTextBox.Text = "0";
        }

        public Func<ChartPoint, string> PointLabel { get; set; }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            //using var _ = new NAT02Context();
            //var vwQuoteConversion = _.VwQuoteConversion.FromSqlRaw("EXEC dbo.sp_EOI_QuoteConversion {0}, {1}", BeginningDatePicker.SelectedDate.Value,
            //                                                                                                   EndDatePicker.SelectedDate.Value);
            //vwQuoteConversion.Load();
            //QuoteConversionDataGrid_Domestic.ItemsSource = _.VwQuoteConversion.Local.ToObservableCollection();
            //_.Dispose();

            BuildOrdersChart();
        }

        private void BuildOrdersChart()
        {
            using var _ = new ProjectsContext();

            string orderReportQuery = "EXECUTE NAT02.dbo.sp_EOI_EngineeringProduction @StartDate = {0}, @EndDate = {1}";

            string[] dates = new string[] { BeginningDatePicker.SelectedDate.Value.ToShortDateString(), EndDatePicker.SelectedDate.Value.ToShortDateString() };

            List<OrdersReport> ordersReport = _.OrdersReport.FromSqlRaw(orderReportQuery, dates).ToList();
            

            _.Dispose();

            SeriesCollection sc = new SeriesCollection();

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

            string[] Labels = ordersReport.Select(or => or.Employee).ToArray();

            YAxis.Labels = ordersReport.Select(or => or.Employee).ToList();
            ProductionChart.Series = sc;
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

        //private void CreateLineGraph(string csr, List<double> pct)
        //{
        //    var height = LineGraph.ActualHeight - 20;
        //    var width = LineGraph.ActualWidth;
        //    Line1.X1 = 0;
        //    Line1.X2 = width / 5;
        //    Line1.Y1 = (height) - ((height) / 100 * pct[0]);
        //    Line1.Y2 = (height) - ((height) / 100 * pct[1]);

        //    Line2.X1 = width / 5;
        //    Line2.X2 = width / 5 * 2;
        //    Line2.Y1 = (height) - (height / 100 * pct[1]);
        //    Line2.Y2 = (height) - (height / 100 * pct[2]);

        //    Line3.X1 = width / 5 * 2;
        //    Line3.X2 = width / 5 * 3;
        //    Line3.Y1 = height - (height / 100 * pct[2]);
        //    Line3.Y2 = height - (height / 100 * pct[3]);

        //    Line4.X1 = width / 5 * 3;
        //    Line4.X2 = width / 5 * 4;
        //    Line4.Y1 = height - (height / 100 * pct[3]);
        //    Line4.Y2 = height - (height / 100 * pct[4]);

        //    Line5.X1 = width / 5 * 4;
        //    Line5.X2 = width / 5 * 5;
        //    Line5.Y1 = height - (height / 100 * pct[4]);
        //    Line5.Y2 = height - (height / 100 * pct[5]);
        //}

        //private void CreateBarGraph(string csr)
        //{
        //    var height = BarGraph.ActualHeight - 20;
        //    var width = BarGraph.ActualWidth;
        //    BarGraph.Children.Clear();
        //    using var _ = new NAT02Context();
        //    int start = int.Parse(RangeIntTextBox.Text);
        //    for (int i = start; i > 0; i--)
        //    {
        //        DateTime begin;
        //        DateTime end;
        //        switch (RangePeriodComboBox.SelectedIndex)
        //        {
        //            case 0:
        //                begin = DateTime.Now.AddDays(-1 * i);
        //                end = begin.AddDays(1);
        //                break;
        //            case 1:
        //                begin = DateTime.Now.AddDays(-1 * i * 7);
        //                end = begin.AddDays(7);
        //                break;
        //            case 2:
        //                begin = DateTime.Now.AddMonths(-1 * i);
        //                end = begin.AddMonths(1);
        //                break;
        //            case 3:
        //                begin = DateTime.Now.AddYears(-1 * i);
        //                end = begin.AddYears(1);
        //                break;
        //            default:
        //                begin = DateTime.Now.AddMonths(-1 * i);
        //                end = begin.AddMonths(1);
        //                break;
        //        }

        //        var quotePercentage = _.QuotePercentage.FromSqlRaw("EXEC dbo.sp_EOI_QuoteConversionByCsr {0}, {1}, {2}", csr.Trim(), begin, end).AsEnumerable();
        //        var quotePercentage_lastyear = _.QuotePercentage.FromSqlRaw("EXEC dbo.sp_EOI_QuoteConversionByCsr {0}, {1}, {2}", csr.Trim(), begin.AddYears(-1), end.AddYears(-1)).AsEnumerable();

        //        Line line_lastyear = new Line();
        //        Line line_thisyear = new Line();

        //        try
        //        {
        //            line_lastyear = new Line()
        //            {
        //                X1 = (width / start * (start - i)) + 50,
        //                X2 = (width / start * (start - i)) + 50,
        //                Y1 = height,
        //                Y2 = (height) - ((height) / 100 * (double)quotePercentage_lastyear.Single().Rate),
        //                Stroke = new SolidColorBrush(Colors.LightBlue),
        //                StrokeThickness = 25
        //            };
        //            Grid.SetColumnSpan(line_lastyear, 6);
        //        }
        //        catch
        //        {

        //        }

        //        try
        //        {
        //            line_thisyear = new Line()
        //            {
        //                X1 = (width / start * (start - i)) + 75,
        //                X2 = (width / start * (start - i)) + 75,
        //                Y1 = height,
        //                Y2 = (height) - ((height) / 100 * (double)quotePercentage.Single().Rate),
        //                Stroke = new SolidColorBrush(Colors.LightCoral),
        //                StrokeThickness = 25
        //            };
        //            Grid.SetColumnSpan(line_thisyear, 6);
        //        }
        //        catch
        //        {

        //        }

        //        BarGraph.Children.Add(line_thisyear);
        //        BarGraph.Children.Add(line_lastyear);
        //    }
        //}
    }
}
