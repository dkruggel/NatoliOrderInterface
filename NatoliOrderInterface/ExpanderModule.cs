using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NatoliOrderInterface
{
    class ExpanderModule : StackPanel
    {
        public string SharedName { get; set; }
        public string ModuleTitle { get; set; }
        public bool SearchBox { get; set; }
        public int RowCount { get; set; }
        public double ModuleHeight { get; set; }
        public Brush ColumnHeaderBackground { get; set; }

        public ExpanderModule()
        {
            // Header grid
            // This houses the title of the module and any extra features i.e.
            // search boxes, filter combo boxes, etc.
            Grid headerLabelGrid = new Grid()
            {
                Name = SharedName + "HeaderLabelGrid",
                Height = 31,
                Background = new SolidColorBrush(SystemColors.GradientActiveCaptionColor),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Header label
            // This contains the title of the module
            Label headerLabel = new Label()
            {
                Name = SharedName + "Label",
                Content = ModuleTitle,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1, 3, 1, 1),
                Height = 31,
                FontWeight = FontWeights.Bold,
                FontSize = 14,
                Padding = new Thickness(0),
                Background = new SolidColorBrush(SystemColors.GradientActiveCaptionColor),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Search box
            // This allows searching of the expanders inside the module if the
            // SearchBox bool is set to true
            TextBox searchBox = new TextBox()
            {
                Name = SharedName + "SearchBox",
                BorderBrush = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(1, 3, 1, 1),
                Padding = new Thickness(0, 5, 0, 5)
            };
            searchBox.TextChanged += SearchBox_TextChanged;

            ColumnDefinition cDef;
            cDef = new ColumnDefinition();
            cDef.Width = new GridLength(1, GridUnitType.Star); // Sets title position
            headerLabelGrid.ColumnDefinitions.Add(cDef);
            cDef = new ColumnDefinition();
            cDef.Width = new GridLength(150); // Sets search box position
            headerLabelGrid.ColumnDefinitions.Add(cDef);

            Grid.SetColumn(headerLabel, 0);
            headerLabelGrid.Children.Add(headerLabel);
            Grid.SetColumn(searchBox, 1);
            headerLabelGrid.Children.Add(searchBox);

            // Scroll viewer for stack panel
            // This presents a scrollbar on the right side of the stack panel
            ScrollViewer scrollViewer = new ScrollViewer()
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
                CanContentScroll = true,
                BorderBrush = new SolidColorBrush(SystemColors.WindowFrameColor),
                Height = (ModuleHeight / RowCount),
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            // Stack panel header for expanders
            // This grid is where the column headers are placed
            Grid headerGrid = new Grid()
            {
                Background = ColumnHeaderBackground,
                Height = 30,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            ColumnDefinition colDef;

            colDef = new ColumnDefinition();
            colDef.Width = new GridLength(25); // Blank space to account for expander arrow
            headerGrid.ColumnDefinitions.Add(colDef);

            //foreach (var column in Columns)
            //{
            //    colDef = new ColumnDefinition();
            //    colDef.Width = column.Item2;
            //    headerGrid.ColumnDefinitions.Add(colDef);
            //    Label label = new Label() { Content = column.Item1 };
            //    Grid.SetRow(label, 0);
            //    Grid.SetColumn(label, column.Item3 + 1);
            //    headerGrid.Children.Add(label);
            //}

            colDef = new ColumnDefinition();
            colDef.Width = new GridLength(22); // Blank space to account for scrollbar
            headerGrid.ColumnDefinitions.Add(colDef);

            // Stack panel for expanders
            StackPanel interiorStackPanel = new StackPanel()
            {
                Name = "InteriorStackPanel",
                CanVerticallyScroll = true,
                ScrollOwner = scrollViewer,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            using var _nat02context = new NAT02Context();
            _nat02context.EoiOrdersInEngineeringUnprintedView.OrderBy(o => o.OrderNo).Load();
            var binding = new Binding
            {
                Source = _nat02context.EoiOrdersInEngineeringUnprintedView.Local.ToObservableCollection(),
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            _nat02context.Dispose();
            interiorStackPanel.SetBinding(ItemsControl.ItemsSourceProperty, binding);

            // Expanders
            //foreach (var item in binding)
            //{
            //    Grid grid = new Grid();
            //    grid.HorizontalAlignment = HorizontalAlignment.Stretch;

            //    colDef = new ColumnDefinition();
            //    colDef.Width = new GridLength(60);
            //    grid.ColumnDefinitions.Add(colDef);
            //    colDef = new ColumnDefinition();
            //    colDef.Width = new GridLength(this.Width / 3 - 327);
            //    colDef.Width = new GridLength(1, GridUnitType.Star);
            //    grid.ColumnDefinitions.Add(colDef);
            //    colDef = new ColumnDefinition();
            //    colDef.Width = new GridLength(80);
            //    grid.ColumnDefinitions.Add(colDef);
            //    colDef = new ColumnDefinition();
            //    colDef.Width = new GridLength(40);
            //    grid.ColumnDefinitions.Add(colDef);
            //    colDef = new ColumnDefinition();
            //    colDef.Width = new GridLength(55);
            //    grid.ColumnDefinitions.Add(colDef);
            //    colDef = new ColumnDefinition();
            //    colDef.Width = new GridLength(40);
            //    grid.ColumnDefinitions.Add(colDef);

            //    Label orderNoLabel = new Label();
            //    orderNoLabel.Content = (item.OrderNo / 100).ToString();
            //    Label customerLabel = new Label();
            //    customerLabel.Content = item.Customer.Trim();
            //    Label shipDateLabel = new Label();
            //    shipDateLabel.Content = item.ShipDate.ToShortDateString();
            //    Label rushLabel = new Label();
            //    rushLabel.Content = item.Rush.Trim();
            //    Label onHoldLabel = new Label();
            //    onHoldLabel.Content = item.OnHold;
            //    Label repInitialsLabel = new Label();
            //    repInitialsLabel.Content = item.RepInitials;

            //    Grid.SetRow(orderNoLabel, 0);
            //    Grid.SetColumn(orderNoLabel, 0);
            //    grid.Children.Add(orderNoLabel);
            //    Grid.SetRow(customerLabel, 0);
            //    Grid.SetColumn(customerLabel, 1);
            //    grid.Children.Add(customerLabel);
            //    Grid.SetRow(shipDateLabel, 0);
            //    Grid.SetColumn(shipDateLabel, 2);
            //    grid.Children.Add(shipDateLabel);
            //    Grid.SetRow(rushLabel, 0);
            //    Grid.SetColumn(rushLabel, 3);
            //    grid.Children.Add(rushLabel);
            //    Grid.SetRow(onHoldLabel, 0);
            //    Grid.SetColumn(onHoldLabel, 4);
            //    grid.Children.Add(onHoldLabel);
            //    Grid.SetRow(repInitialsLabel, 0);
            //    Grid.SetColumn(repInitialsLabel, 5);
            //    grid.Children.Add(repInitialsLabel);

            //    int daysToShip = (order.ShipDate.Date - DateTime.Now.Date).Days;

            //    Expander expander = new Expander()
            //    {
            //        IsExpanded = false,
            //        Header = grid,
            //        HorizontalAlignment = HorizontalAlignment.Stretch
            //    };
            //    if (daysToShip < 0)
            //    {
            //        expander.Background = new SolidColorBrush(Colors.Red);
            //        expander.Foreground = new SolidColorBrush(Colors.Black);
            //    }
            //    else if (daysToShip == 0)
            //    {
            //        expander.Background = new SolidColorBrush(Colors.Orange);
            //        expander.Foreground = new SolidColorBrush(Colors.Black);
            //    }
            //    else if (daysToShip > 0 && daysToShip < 4)
            //    {
            //        expander.Background = new SolidColorBrush(Colors.Yellow);
            //        expander.Foreground = new SolidColorBrush(Colors.Black);
            //    }
            //    else
            //    {
            //        expander.Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FFFFFFFF"));
            //    }
            //    // expander.Expanded += Expander_Expanded;
            //    interiorStackPanel.Children.Add(expander);
            //}

            scrollViewer.Content = interiorStackPanel;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        //private void Expander_Expanded(object sender, RoutedEventArgs e)
        //{
        //    Expander expander = (Expander)sender;
        //    Grid grid = (Grid)expander.Header;
        //    UIElementCollection collection = grid.Children;
        //    string orderNumber = collection[0].GetValue(ContentProperty).ToString() + "00";
        //    using var _natbcContext = new NATBCContext();

        //    List<LineItemLastScan> lines = _natbcContext.LineItemLastScan.FromSqlRaw("SELECT DISTINCT OrderDetailTypeDescription, OrderLineNumber, (SELECT TOP 1 ScanTimeStamp FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'ScanTimeStamp', (SELECT TOP 1 DepartmentDesc FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Department', (SELECT TOP 1 EmployeeName FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber AND TravellerScansAudit.DepartmentDesc <> 'Production Mgmnt' ORDER BY ScanTimeStamp DESC) AS 'Employee' FROM NATBC.dbo.TravellerScansAudit TSA WITH (NOLOCK) WHERE TSA.OrderNumber = {0} AND TSA.OrderDetailTypeID NOT IN('E','H','MC','RET','T','TM','Z') AND TSA.OrderDetailTypeDescription <> 'PARTS' AND TSA.DepartmentDesc <> 'Production Mgmnt' ORDER BY OrderLineNumber", orderNumber).ToList();
        //    _natbcContext.Dispose();

        //    StackPanel lineItemsStackPanel = new StackPanel()
        //    {
        //        Orientation = Orientation.Vertical
        //    };

        //    foreach (LineItemLastScan lineItem in lines)
        //    {
        //        Grid lineItemGrid = new Grid();
        //        // lineItemGrid.Width = expander.Width - 30 - 22;
        //        lineItemGrid.HorizontalAlignment = HorizontalAlignment.Stretch;
        //        ColumnDefinition colDef;

        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        lineItemGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        lineItemGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        lineItemGrid.ColumnDefinitions.Add(colDef);
        //        colDef = new ColumnDefinition();
        //        colDef.Width = new GridLength(1, GridUnitType.Star);
        //        lineItemGrid.ColumnDefinitions.Add(colDef);

        //        Label detailTypeLabel = new Label();
        //        detailTypeLabel.Content = lineItem.OrderDetailTypeDescription;
        //        Label scanTimeStampLabel = new Label();
        //        scanTimeStampLabel.Content = string.Format("{0:d} {0:t}", lineItem.ScanTimeStamp);
        //        Label departmentLabel = new Label();
        //        departmentLabel.Content = lineItem.Department;
        //        Label employeeLabel = new Label();
        //        employeeLabel.Content = lineItem.Employee;

        //        Grid.SetRow(detailTypeLabel, 0);
        //        Grid.SetColumn(detailTypeLabel, 0);
        //        lineItemGrid.Children.Add(detailTypeLabel);
        //        Grid.SetRow(scanTimeStampLabel, 0);
        //        Grid.SetColumn(scanTimeStampLabel, 1);
        //        lineItemGrid.Children.Add(scanTimeStampLabel);
        //        Grid.SetRow(departmentLabel, 0);
        //        Grid.SetColumn(departmentLabel, 2);
        //        lineItemGrid.Children.Add(departmentLabel);
        //        Grid.SetRow(employeeLabel, 0);
        //        Grid.SetColumn(employeeLabel, 3);
        //        lineItemGrid.Children.Add(employeeLabel);

        //        lineItemsStackPanel.Children.Add(lineItemGrid);
        //    }

        //    expander.Content = lineItemsStackPanel;
        //}
    }
}
