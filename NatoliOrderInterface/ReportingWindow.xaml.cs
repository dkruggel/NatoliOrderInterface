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
            BeginningDatePicker.SelectedDate = DateTime.Now.AddDays(-14);
            EndDatePicker.SelectedDate = DateTime.Now;

            BuildOrdersChart();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
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
    }
}
