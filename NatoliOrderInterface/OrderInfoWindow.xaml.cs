using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.IO;
using System.Diagnostics;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using Microsoft.EntityFrameworkCore;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Forms;
using iText.Forms.Fields;
using iText.Signatures;
using iText.IO.Image;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.Crypto.Parameters;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Windows.Media;

namespace NatoliOrderInterface
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]

    public partial class OrderInfoWindow : Window, IDisposable
    {
        private readonly MainWindow parent;
        private readonly WorkOrder workOrder;
        private readonly int orderNumber;
        private readonly string orderLocation;
        private short lineItemNumber = 1;
        private readonly User user;
        private bool isReferenceWO = false;
        private Quote quote;

        private Dictionary<double, string> ordersDoNoProcess;
        private bool doNotProc = false;
        private bool validData;
        private List<OrderLineItem> orderLineItems = new List<OrderLineItem>();
        

        public OrderInfoWindow()
        {
            InitializeComponent();
        }
        public OrderInfoWindow(WorkOrder _workOrder, MainWindow _parent, string _orderLocation, User _user, bool _isReferenceWO = false)
        {
            // For centerowner startup
            Owner = _parent ?? new MainWindow();
            InitializeComponent();
            user = _user ?? new User("");
            workOrder = _workOrder ?? new WorkOrder();
            orderNumber = workOrder.OrderNumber;
            Title = "Work Order " + orderNumber;
            parent = _parent ?? new MainWindow();
            if (_parent.WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                //Top = parent.Top;
                //Left = parent.Left;
                Width = parent.Width;
                Height = parent.Height;
            }
            orderLocation = _orderLocation ?? "";
            isReferenceWO = _isReferenceWO;
            if (user.Department == "Customer Service")
            {
                DoNotProcessOrderButton.IsEnabled = false;
            }
            else
            {
                if (orderLocation.StartsWith("InTheOffice") || orderLocation.StartsWith("InEngineering") || orderLocation.StartsWith("EnteredUnscanned"))
                {
                    DoNotProcessOrderButton.IsEnabled = true;
                    using (var context = new NAT02Context())
                    {
                        ordersDoNoProcess = context.EoiOrdersDoNotProcess.ToDictionary(k => k.OrderNo, k => k.UserName);
                    }
                    if (ordersDoNoProcess.Keys.Contains((double)orderNumber))
                    {
                        doNotProc = true;
                        DoNotProcessOrderButton.Content = "Can Process";
                    }
                    else
                    {
                        doNotProc = false;
                        DoNotProcessOrderButton.Content = "Do Not Process";
                    }
                }
                else
                {
                    doNotProc = false;
                    DoNotProcessOrderButton.IsEnabled = false;
                }
            }
            for (short i = 1; i <= workOrder.LineItemCount; i++)
            {
                if (workOrder.lineItems[i].Trim().Length > 0)
                {
                    orderLineItems.Add(new OrderLineItem(workOrder, i));
                }
            }
            FillInfo();
            LineItemChange();
            if (user.Department == "Engineering") { CreateMachineVariablesDataGrid(); }
            CreateBarcodeDataGrid();
            Show();
        }
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string url = e.Uri.ToString();
            try
            {
                Process.Start(url);
            }
            catch
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
        private void FillInfo()
        {
            OrderNumberButton.Content = (workOrder.OrderNumber * 100).ToString().Insert(6, " ");
            OrderNumberButton.ToolTip = @"Opens L:\WorkOrders\'" + workOrder.OrderNumber.ToString() + @"' folder.";
            CustomerNumberLabel.Content = workOrder.CustomerNumber;
            SoldToCustomerNameButton.Content = workOrder.SoldToCustomerName;
            OrderDateContext.Content = workOrder.OrderDate.ToShortDateString();
            PONumberContext.Content = workOrder.PoNumber;
            ShipToCustomerNameButton.Content = workOrder.ShipToCustomerName;
            ShipDateContext.Content = workOrder.ShipDate.ToShortDateString();
            EndUserNameButton.Content = workOrder.EndUserName;
            ReferenceOrderButton.Content = workOrder.ReferenceOrder == 0 ? "N/A" : workOrder.ReferenceOrder + " 00";
            ReferenceOrderButton.IsEnabled = !(workOrder.ReferenceOrder is null) && workOrder.ReferenceOrder != 0;
            EngineeringNotesContext.Text = workOrder.EngineeringNotes;
            CSRTextBlock.Text = workOrder.Csr;
            AccountNumberContext.Content = workOrder.AccountNumber;
            TermsContext.Content = workOrder.Terms;
            SoldByContext.Content = workOrder.SoldBy;
            ShippedViaContext.Content = workOrder.ShippedVia;
            QuoteButton.Content = "Quote#: " + workOrder.QuoteNumber;
            Reference.Content = workOrder.Reference;
            OrderHobStatus.Content = workOrder.HobRequired;
            EndUserNumber.Content = workOrder.UserNumber;
            ProductName.Content = workOrder.ProductName;
            Project.Content = workOrder.ProjectNumber == 0 ? "N/A" : workOrder.ProjectNumber.ToString();
            Project.IsEnabled = workOrder.ProjectNumber != 0;
            if (workOrder.RushYOrN != "Y" && workOrder.PaidRushFee != "Y")
            {
                RushLabel.Content = "";
            }
            else
            {
                if (workOrder.RushYOrN == "Y" && workOrder.PaidRushFee != "Y")
                {
                    RushLabel.Content = "RUSH";
                }
                if (workOrder.RushYOrN == "Y" && workOrder.PaidRushFee == "Y")
                {
                    RushLabel.Content = "+RUSH+";
                }
            }
            CreateLineItemDataGrid();
        }
        private void CreateLineItemDataGrid()
        {
            DataTable lineItems = new DataTable();
            lineItems.Clear();
            lineItems.Columns.Add("LineNumber");
            lineItems.Columns.Add("Description");
            using var context = new NAT01Context();
            foreach (KeyValuePair<int, string> lineItem in workOrder.lineItems)
            {
                if (lineItem.Value.Trim().Length > 0)
                {
                    string description = context.OedetailType.Where(o => o.TypeId.Trim() == lineItem.Value.Trim()).FirstOrDefault().ShortDesc.Trim();
                    DataRow row = lineItems.NewRow();
                    row["LineNumber"] = lineItem.Key.ToString();
                    row["Description"] = description;
                    lineItems.Rows.Add(row);
                }
            }
            context.Dispose();
            LineItemsDataGrid.LoadingRow += LineItemsDataGrid_LoadingRow;
            LineItemsDataGrid.ItemsSource = lineItems.DefaultView;
            lineItems.Dispose();
        }
        private void LineItemsDataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;
            int index = e.Row.GetIndex();
            DataRowView row = dataGrid.Items[index] as DataRowView;
            string description = row[1].ToString();
            if (description.Contains("DIE") ||
                    description.Contains("KEY") ||
                    description.Contains("UPPER") ||
                    description.Contains("LOWER") ||
                    description.Contains("REJECT") ||
                    description.Contains("CORE") ||
                    description.Contains("COPPER") ||
                    (description.Contains("MISC") && !description.Contains("CHARGE")))

            {
                e.Row.Background = System.Windows.Media.Brushes.GreenYellow;
            }
            else
            {
                e.Row.Background = System.Windows.Media.Brushes.Gray;
            }
        }
        private void LineItemChange()
        {
            
            Order_Info_Window.Background = orderLineItems[lineItemNumber - 1].SheetColor;
            QTYLabel.Content = orderLineItems[lineItemNumber - 1].QTY;
            LineItemType.Content = orderLineItems[lineItemNumber - 1].LineItemType;
            HobOrDieNo.Content = orderLineItems[lineItemNumber - 1].HobNoShapeID;
            using var _nat01Context = new NAT01Context();
            if (!string.IsNullOrEmpty(orderLineItems[lineItemNumber - 1].HobNoShapeID) &&
                !string.IsNullOrWhiteSpace(orderLineItems[lineItemNumber - 1].HobNoShapeID) &&
                orderLineItems[lineItemNumber - 1].LineItemType != "D" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DS" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DH" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DI" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DA" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DP" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DS" &&
                orderLineItems[lineItemNumber - 1].LineItemType != "DC" &&
                _nat01Context.HobList.Any(h => h.HobNo.Trim() == orderLineItems[lineItemNumber - 1].HobNoShapeID.Trim()
                && h.TipQty == orderLineItems[lineItemNumber - 1].TipQTY))
            {
                HobList hob = _nat01Context.HobList.First(h => h.HobNo.Trim() == orderLineItems[lineItemNumber - 1].HobNoShapeID.Trim() && h.TipQty == orderLineItems[lineItemNumber - 1].TipQTY);
                Grid grid = new Grid();
                grid.ShowGridLines = false;
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                for (UInt16 ii = 0; ii < 22; ii++)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                Border headerBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };
                headerBorder.SetValue(Grid.ColumnSpanProperty, 100);
                grid.Children.Add(headerBorder);
                System.Reflection.PropertyInfo[] properties = typeof(HobList).GetProperties();
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    TextBlock textBlock0 = null;
                    TextBlock textBlock1 = null;
                    Border border0 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 0), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };
                    border0.SetValue(Grid.RowProperty, 0);
                    switch (property.Name)
                    {
                        case "HobNo":
                            textBlock0 = new TextBlock { Text = "Hob No", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 0);
                            border0.SetValue(Grid.ColumnProperty, 0);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "TipQty":
                            textBlock0 = new TextBlock { Text = "Tip", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "1" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 1);
                            border0.SetValue(Grid.ColumnProperty, 1);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "BoreCircle":
                            textBlock0 = new TextBlock { Text = "Bore Circle", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "0.0000" : ((float)property.GetValue(hob)).ToString("#0.0000"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 2);
                            border0.SetValue(Grid.ColumnProperty, 2);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "HobYorNorD":
                            textBlock0 = new TextBlock { Text = "Hob Status", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 3);
                            border0.SetValue(Grid.ColumnProperty, 3);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "DieId":
                            textBlock0 = new TextBlock { Text = "Die ID", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 4);
                            border0.SetValue(Grid.ColumnProperty, 4);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "Size":
                            textBlock0 = new TextBlock { Text = property.Name, Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 5);
                            border0.SetValue(Grid.ColumnProperty, 5);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "Shape":
                            textBlock0 = new TextBlock { Text = property.Name, Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 6);
                            border0.SetValue(Grid.ColumnProperty, 6);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "CupDepth":
                            textBlock0 = new TextBlock { Text = "Cup Depth", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "0.0000" : ((float)property.GetValue(hob)).ToString("#0.0000"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 7);
                            border0.SetValue(Grid.ColumnProperty, 7);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "Land":
                            textBlock0 = new TextBlock { Text = property.Name, Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "0.0000" : ((float)property.GetValue(hob)).ToString("#0.0000"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 8);
                            border0.SetValue(Grid.ColumnProperty, 8);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "LandRange":
                            textBlock0 = new TextBlock { Text = "Land Range", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "0.0000" : ((float)property.GetValue(hob)).ToString("#0.0000"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 9); 
                            border0.SetValue(Grid.ColumnProperty, 9);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "LandBlendedYorN":
                            textBlock0 = new TextBlock { Text = "Blended?", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 10);
                            border0.SetValue(Grid.ColumnProperty, 10);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "MeasurableCd":
                            textBlock0 = new TextBlock { Text = "Meas. Cup Depth", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "0.0000" : ((float)property.GetValue(hob)).ToString("#0.0000"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 11);
                            border0.SetValue(Grid.ColumnProperty, 11);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "CupCode":
                            textBlock0 = new TextBlock { Text = "Cup Code", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 12);
                            border0.SetValue(Grid.ColumnProperty, 12);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "BisectCode":
                            textBlock0 = new TextBlock { Text = "Bisect Code", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 13);
                            border0.SetValue(Grid.ColumnProperty, 13);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "OwnerReservedFor":
                            textBlock0 = new TextBlock { Text = "Owner" , Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 14);
                            border0.SetValue(Grid.ColumnProperty, 14);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "DateDesigned":
                            textBlock0 = new TextBlock { Text = "Date Designed", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : ((DateTime)property.GetValue(hob)).ToString("dd/mm/yy"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 15);
                            border0.SetValue(Grid.ColumnProperty, 15);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "Class":
                            textBlock0 = new TextBlock { Text = property.Name, Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 16);
                            border0.SetValue(Grid.ColumnProperty, 16);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "DrawingYorN":
                            textBlock0 = new TextBlock { Text = "Drawing?", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 17);
                            border0.SetValue(Grid.ColumnProperty, 17);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "DrawingType":
                            textBlock0 = new TextBlock { Text = "Drawing Type", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 18);
                            border0.SetValue(Grid.ColumnProperty, 18);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "ProgramNo":
                            textBlock0 = new TextBlock { Text = "Project No", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 19);
                            border0.SetValue(Grid.ColumnProperty, 19);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "CupRadius":
                            textBlock0 = new TextBlock { Text = "Cup Radius", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "0.0000" : ((decimal)property.GetValue(hob)).ToString("#0.0000"), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 20);
                            border0.SetValue(Grid.ColumnProperty, 20);
                            border0.Child = textBlock0;
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "Nnumber":
                            textBlock0 = new TextBlock { Text = "N-Number", Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "NULL" : property.GetValue(hob).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                            textBlock1.SetValue(Grid.RowProperty, 1);
                            textBlock1.SetValue(Grid.ColumnProperty, 21);
                            border0.SetValue(Grid.ColumnProperty, 21);
                            border0.Child = textBlock0;
                            border0.BorderThickness = new Thickness(0);
                            grid.Children.Add(border0);
                            grid.Children.Add(textBlock1);
                            break;
                        case "Note1":
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "" : property.GetValue(hob).ToString().Trim(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
                            Border border1 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Margin = new Thickness(4, 4, -20, 4), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                            border1.SetValue(Grid.RowProperty, 3);
                            border1.SetValue(Grid.ColumnProperty, 3);
                            border1.SetValue(Grid.ColumnSpanProperty, 3);
                            border1.Child = textBlock1;
                            grid.Children.Add(border1);
                            break;
                        case "Note2":
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "" : property.GetValue(hob).ToString().Trim(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
                            Border border2 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Margin = new Thickness(24, 4, 4, 4), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                            border2.SetValue(Grid.RowProperty, 3);
                            border2.SetValue(Grid.ColumnProperty, 6);
                            border2.SetValue(Grid.ColumnSpanProperty, 4);
                            border2.Child = textBlock1;
                            grid.Children.Add(border2);
                            break;
                        case "Note3":
                            textBlock1 = new TextBlock { Text = property.GetValue(hob) == null ? "" : property.GetValue(hob).ToString().Trim(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Stretch };
                            Border border3 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1), Margin = new Thickness(4), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                            border3.SetValue(Grid.RowProperty, 3);
                            border3.SetValue(Grid.ColumnProperty, 10);
                            border3.SetValue(Grid.ColumnSpanProperty, 5);
                            border3.Child = textBlock1;
                            grid.Children.Add(border3);
                            break;

                    }
                }
                HobOrDieNo.ToolTip = grid;
            }
            else
            {
                HobOrDieNo.ToolTip = null;
            }


            if (orderLineItems[lineItemNumber - 1].TipQTY > 1)
            {
                NoOfTips.Content = orderLineItems[lineItemNumber - 1].TipQTY + "-";
            }
            else
            {
                NoOfTips.Content = "-";
            }

            MachineNo.Content = orderLineItems[lineItemNumber - 1].MachineNo.ToString();

            
            if (_nat01Context.CustomerMachines.Any(m => m.MachineNo == orderLineItems[lineItemNumber - 1].MachineNo && m.CustomerNo.Trim() == workOrder.UserNumber && m.CustAddressCode.Trim() == workOrder.UserLoc))
            {
                CustomerMachines machine = _nat01Context.CustomerMachines.First(m => m.MachineNo == orderLineItems[lineItemNumber - 1].MachineNo && m.CustomerNo == workOrder.UserNumber);

                Grid grid = new Grid();
                grid.ShowGridLines = false;
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(24) });
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                Border headerBorder = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };
                headerBorder.SetValue(Grid.ColumnSpanProperty, 100);
                Border headerBorder1 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 0, 1), VerticalAlignment = VerticalAlignment.Stretch, HorizontalAlignment = HorizontalAlignment.Stretch };
                headerBorder1.SetValue(Grid.ColumnSpanProperty, 100);
                headerBorder1.SetValue(Grid.RowProperty, 3);
                grid.Children.Add(headerBorder);
                grid.Children.Add(headerBorder1);
                int x = 0;
                System.Reflection.PropertyInfo[] properties = typeof(CustomerMachines).GetProperties();
                foreach (System.Reflection.PropertyInfo property in properties)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                    TextBlock textBlock0 = new TextBlock { Text = property.Name.ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center};
                    TextBlock textBlock1 = new TextBlock { Text = property.GetValue(machine) == null ? "NULL" : property.GetValue(machine).ToString().Trim(), Margin = new Thickness(4), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    textBlock0.SetValue(Grid.RowProperty, x > 9 ? 3 : 0);
                    textBlock1.SetValue(Grid.RowProperty, x > 9 ? 4 : 1);
                    textBlock0.SetValue(Grid.ColumnProperty, x % 10 );
                    textBlock1.SetValue(Grid.ColumnProperty, x % 10);
                    grid.Children.Add(textBlock0);
                    grid.Children.Add(textBlock1);
                    x++;
                }
                MachineNo.ToolTip = grid;
            }
            else
            {
                MachineNo.ToolTip = null;
            }
            _nat01Context.Dispose();


            Material.Content = orderLineItems[lineItemNumber - 1].Material;
            StockSize.Content = orderLineItems[lineItemNumber - 1].StockSize;
            HobDescription1.Content = orderLineItems[lineItemNumber - 1].HobDescription1;
            HobDescription2.Content = orderLineItems[lineItemNumber - 1].HobDescription2;
            HobDescription3.Content = orderLineItems[lineItemNumber - 1].HobDescription3;
            MachineDescription.Content = orderLineItems[lineItemNumber - 1].MachineDescription;
            ShapeDescription.Content = orderLineItems[lineItemNumber - 1].Shape;
            DetailHobStatus.Content = orderLineItems[lineItemNumber - 1].HobYorNorD;
            OrderTitle.Content = orderLineItems[lineItemNumber - 1].Title;
            BoreCircle.Content = orderLineItems[lineItemNumber - 1].BoreCircle is null || orderLineItems[lineItemNumber - 1].BoreCircle.Length == 0 ? "" : "BORE CIRCLE: " + orderLineItems[lineItemNumber - 1].BoreCircle;
            


            //Options
            short i = 0;
            OptionsStackOne.Children.Clear();
            OptionsStackTwo.Children.Clear();
            foreach (KeyValuePair<short, string[]> option in orderLineItems[lineItemNumber - 1].Options)
            {
                string[] printables = option.Value;
                if (printables[0].Trim().Length != 0)
                {
                    Label label = new Label
                    {
                        Margin = new Thickness(0, 0, 0, 0),
                        Padding = new Thickness(0, 0, 0, 0),
                        VerticalContentAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeights.Bold,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Content = String.Concat(printables),
                        ToolTip = orderLineItems[lineItemNumber - 1].OptionNumbers[option.Key],
                        Height = 20
                    };
                    bool even = i % 2 == 0;
                    if (even)
                    {
                        OptionsStackOne.Children.Add(label);
                    }
                    else
                    {
                        OptionsStackTwo.Children.Add(label);
                    }
                    i++;
                }
            }
            ListHobDrawings();
            EnableOpenTabletButton();
        }
        private void ListHobDrawings()
        {
            Border borderOld = OrderPanel.Children.OfType<Border>().Where(sp => sp.Name == "HobDrawingsBorder").FirstOrDefault() as Border;
            if (borderOld != null)
            {
                OrderPanel.Children.Remove(borderOld);
            }
            if (orderLineItems[lineItemNumber - 1].LineItemType.Trim() == "H")
            {
                Border border = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(2, 2, 2, 2),
                    Margin = new Thickness(0, 2, 0, 2),
                    Name = "HobDrawingsBorder",
                    CornerRadius = new CornerRadius(2)
                };
                StackPanel stackPanel = new StackPanel { Margin = new Thickness(0,0,0,4) };
                string hobNo = orderLineItems[lineItemNumber - 1].HobNoShapeID;
                while (hobNo.First() == '0')
                {
                    hobNo = hobNo.Remove(0, 1);
                }
                string folderBeginning = hobNo.Length == 6 || hobNo.Length == 5 ? hobNo.Remove(hobNo.Length - 3) : hobNo.Length == 4 ? "0" + hobNo.First().ToString() : "00";
                string path = @"\\nsql03\data1\DRAW\E-DRAWINGS\" + folderBeginning + @"-E-DRAWINGS\";
                string[] files = Directory.GetFiles(path, hobNo + "*" + "*.pdf");
                List<string> doesContainCustomer = files.Where(s => s.Contains(workOrder.EndUserName.Split(' ')[0])).OrderBy(s => s).ToList();
                List<string> doesNotContainCustomer = files.Where(s => !s.Contains(workOrder.EndUserName.Split(' ')[0])).OrderBy(s => s).ToList();
                List<string> newFiles = new List<string>();
                newFiles.AddRange(doesContainCustomer);
                newFiles.AddRange(doesNotContainCustomer);
                foreach (string file in newFiles)
                {
                    string name = file.Remove(0, path.Length);
                    Border miniBorder = new Border
                    {
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 0, 0),
                        Margin = new Thickness(0, 4, 0, 0)
                    };
                    TextBlock textBox = new TextBlock
                    {
                        Cursor = Cursors.Hand,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(4,0,0,0)
                    };
                    Hyperlink hyperlink = new Hyperlink
                    {
                        NavigateUri = new Uri(file)
                    };
                    hyperlink.RequestNavigate += Hyperlink_RequestNavigate;
                    hyperlink.Inlines.Add(name);
                    textBox.Inlines.Add(hyperlink);
                    miniBorder.Child = textBox;
                    stackPanel.Children.Add(miniBorder);
                }
                border.Child = stackPanel;
                OrderPanel.Children.Insert(3, border);
            }
        }
        private void EnableOpenTabletButton()
        {
            string lineItemType = orderLineItems[lineItemNumber - 1].LineItemType;
            if (lineItemType == "U" ||
               lineItemType == "L" ||
               lineItemType == "R" ||
               lineItemType == "UT" ||
               lineItemType == "LT" ||
               lineItemType == "RT" ||
               lineItemType == "H" ||
               lineItemType == "LCRP"
               )
            {
                OpenTabletFolder.IsEnabled = true;
            }
            else
            {
                OpenTabletFolder.IsEnabled = false;
            }
        }
        private void CreateMachineVariablesDataGrid()
        {
            NAT02Context nAT02Context = new NAT02Context();

            try
            {
                DataGrid machineVariablesDataGridOld = OrderPanel.Children.OfType<DataGrid>().Where(dg => dg.Name == "MachineVariablesDataGrid").FirstOrDefault() as DataGrid;
                Border machineVariablesBorderOld = OrderPanel.Children.OfType<Border>().Where(dg => dg.Name == "MachineVariablesBorder").FirstOrDefault() as Border;
                OrderPanel.Children.Remove(machineVariablesBorderOld);
                OrderPanel.Children.Remove(machineVariablesDataGridOld);
                List<MaMachineVariables> machineVariables = nAT02Context.MaMachineVariables.Where(mv => mv.WorkOrderNumber == workOrder.OrderNumber.ToString()).OrderBy(mv => mv.LineNumber).ToList();
                if (machineVariables.Count != 0 && workOrder.CanRunOnAutocell)
                {
                    Border machineVariablesBorder = new Border
                    {
                        Name = "MachineVariablesBorder",
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        Margin = new Thickness(0, 40, 0, 0)
                    };
                    TextBlock machineVariablesTitle = new TextBlock
                    {
                        Name = "MachineVariablesTitle",
                        Text = "Machine Variables",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    machineVariablesBorder.Child = machineVariablesTitle;
                    OrderPanel.Children.Add(machineVariablesBorder);

                    DataGrid machineVariablesDataGrid = new DataGrid
                    {
                        Name = "MachineVariablesDataGrid",
                        ItemsSource = machineVariables,
                        IsReadOnly = true,
                        HeadersVisibility = DataGridHeadersVisibility.Column
                    };
                    OrderPanel.Children.Add(machineVariablesDataGrid);
                }
                else
                {
                    if (workOrder.CanRunOnAutocell)
                    {
                        Border machineVariablesBorder = new Border
                        {
                            Name = "MachineVariablesBorder",
                            BorderBrush = System.Windows.Media.Brushes.Gray,
                            BorderThickness = new Thickness(0, 1, 0, 0),
                            Margin = new Thickness(0, 40, 0, 0)
                        };
                        TextBlock machineVariablesTitle = new TextBlock
                        {
                            Name = "MachineVariablesTitle",
                            Text = "No Autocell Data For This Order",
                            FontSize = 20,
                            FontWeight = FontWeights.Bold,
                            Foreground = System.Windows.Media.Brushes.Crimson,
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin = new Thickness(0, 10, 0, 0)
                        };
                        machineVariablesBorder.Child = machineVariablesTitle;
                        OrderPanel.Children.Add(machineVariablesBorder);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            nAT02Context.Dispose();
        }
        private void CreateBarcodeDataGrid()
        {
            using var _natbcContext = new NATBCContext();

            try
            {
                DataGrid barcodeDataGridOld = OrderPanel.Children.OfType<DataGrid>().Where(dg => dg.Name == "BarcodeDataGrid").FirstOrDefault() as DataGrid;
                Border barcodeBorderOld = OrderPanel.Children.OfType<Border>().Where(dg => dg.Name == "BarcodeBorder").FirstOrDefault() as Border;
                OrderPanel.Children.Remove(barcodeBorderOld);
                OrderPanel.Children.Remove(barcodeDataGridOld);
                List<LineItemLastScan> lines = _natbcContext.LineItemLastScan.FromSqlRaw("SELECT DISTINCT OrderDetailTypeDescription, OrderLineNumber, (SELECT TOP 1 ScanTimeStamp FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber ORDER BY ScanTimeStamp DESC) AS 'ScanTimeStamp', (SELECT TOP 1 DepartmentDesc FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber ORDER BY ScanTimeStamp DESC) AS 'Department', (SELECT TOP 1 EmployeeName FROM NATBC.dbo.TravellerScansAudit WITH (NOLOCK) WHERE TravellerScansAudit.OrderNumber = TSA.OrderNumber AND TravellerScansAudit.OrderLineNumber = TSA.OrderLineNumber ORDER BY ScanTimeStamp DESC) AS 'Employee' FROM NATBC.dbo.TravellerScansAudit TSA WITH (NOLOCK) WHERE TSA.OrderNumber = {0} AND TSA.OrderDetailTypeID NOT IN('E','H','MC','RET','T','TM','Z') AND TSA.OrderDetailTypeDescription <> 'PARTS' ORDER BY OrderLineNumber", orderNumber * 100).ToList();
                _natbcContext.Dispose();
                if (lines.Count > 0)
                {
                    Border barcodeBorder = new Border
                    {
                        Name = "BarcodeBorder",
                        BorderBrush = System.Windows.Media.Brushes.Gray,
                        BorderThickness = new Thickness(0, 1, 0, 0),
                        Margin = new Thickness(0, 40, 0, 0)
                    };
                    TextBlock barcodeTitle = new TextBlock
                    {
                        Name = "BarcodeTitle",
                        Text = "Last Barcode Scan",
                        FontSize = 20,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    };
                    barcodeBorder.Child = barcodeTitle;
                    OrderPanel.Children.Add(barcodeBorder);

                    DataGrid barcodeDataGrid = new DataGrid
                    {
                        Name = "BarcodeDataGrid",
                        ItemsSource = lines,
                        IsReadOnly = true,
                        HeadersVisibility = DataGridHeadersVisibility.Column
                    };
                    OrderPanel.Children.Add(barcodeDataGrid);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void DeleteMachineVariables(string orderNo, int lineNumber)
        {
            using var _nat02Context = new NAT02Context();
            _nat02Context.MaMachineVariables.RemoveRange(_nat02Context.MaMachineVariables.Where(m => m.WorkOrderNumber.Trim() == orderNo && m.LineNumber == lineNumber));
            _nat02Context.SaveChanges();
            _nat02Context.Dispose();
        }
        private void InsertIntoDWAutoRun(int orderNo)
        {
            
            using var _nat02Context = new NAT02Context();
            try
            {
                if (_nat02Context.DwAutoRun.Any(dw => dw.WorkOrderNumber == orderNo * 100))
                {
                    DwAutoRun dwAutoRun = _nat02Context.DwAutoRun.First(dw => dw.WorkOrderNumber == orderNo * 100);
                    dwAutoRun.ProcessState = "New";
                    dwAutoRun.TransitionName = "AutoRelease";
                    _nat02Context.Update(dwAutoRun);
                }
                else
                {
                    DwAutoRun dwAutoRun = new DwAutoRun
                    {
                        WorkOrderNumber = orderNo * 100,
                        ProcessState = "New",
                        TransitionName = "AutoRelease"
                    };
                    _nat02Context.DwAutoRun.Add(dwAutoRun);
                }
                _nat02Context.SaveChanges();
            }
            catch
            {
                
            }
            _nat02Context.Dispose();
            
        }
        private void BarcodeTransfer(string employeeCode, string departmentCode, string travellerNumber, bool firstScan = false)
        {
            if (firstScan)
            {
                string lineNumber = travellerNumber.Substring(1, 2);
                string strSQL = "Update [NAT01].[dbo].[OrderDetails] set [TravellerNo] = '" + travellerNumber + "' where [OrderNo] = '" + workOrder.OrderNumber + "00' and [LineNumber] = '" + lineNumber + "'";
                SqlConnection updateConnection = new SqlConnection();
                SqlCommand updateCommand = new SqlCommand();
                try
                {
                    updateCommand.Connection = updateConnection;
                    updateCommand.CommandText = strSQL;
                    updateCommand.CommandType = CommandType.Text;
                    using (updateConnection)
                    {
                        updateConnection.ConnectionString = "Data Source=NSQL05;Initial Catalog=NATBC;Persist Security Info=True;User ID=BarcodeUser;Password=PrivateKey(0)";
                        updateConnection.Open();
                        DataTable TransferBatch = new DataTable();
                        TransferBatch.Load(updateCommand.ExecuteReader());
                        TransferBatch.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    updateCommand.Dispose();
                    updateConnection.Close();
                }
            }

            SqlConnection con = new SqlConnection();
            SqlCommand cmd = new SqlCommand();
            try
            {
                cmd.Connection = con;
                cmd.CommandText = "spValidateScanTransfer";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                // Terminal ID
                cmd.Parameters.Add("@TerminalID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@TerminalID"].Size = 50;
                cmd.Parameters["@TerminalID"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@TerminalID"].Value = Environment.MachineName;

                // NTUserID
                cmd.Parameters.Add("@NTUserID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@NTUserID"].Size = 35;
                cmd.Parameters["@NTUserID"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@NTUserID"].Value = Environment.MachineName;

                // ApplicationVersion
                cmd.Parameters.Add("@ApplicationVersion", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@ApplicationVersion"].Size = 25;
                cmd.Parameters["@ApplicationVersion"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@ApplicationVersion"].Value = "V3.4";

                // Browser Details
                cmd.Parameters.Add("@BrowserDetails", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@BrowserDetails"].Size = 75;
                cmd.Parameters["@BrowserDetails"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@BrowserDetails"].Value = "";

                // ActionPerformed
                cmd.Parameters.Add("@ActionPerformed", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@ActionPerformed"].Size = 75;
                cmd.Parameters["@ActionPerformed"].Direction = System.Data.ParameterDirection.Output;

                // EmployeeCode
                cmd.Parameters.Add("@EmployeeCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@EmployeeCode"].Size = 7;
                cmd.Parameters["@EmployeeCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@EmployeeCode"].Value = employeeCode;

                // CardCode
                cmd.Parameters.Add("@CardCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@CardCode"].Size = 7;
                cmd.Parameters["@CardCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@CardCode"].Value = "";


                // EmployeeValidationCode
                cmd.Parameters.Add("@EmployeeValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@EmployeeValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // EmployeeValidationText
                cmd.Parameters.Add("@EmployeeValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@EmployeeValidationText"].Size = 75;
                cmd.Parameters["@EmployeeValidationText"].Direction = System.Data.ParameterDirection.Output;

                // DepartmentCode
                cmd.Parameters.Add("@DepartmentCode", System.Data.SqlDbType.Char);
                cmd.Parameters["@DepartmentCode"].Size = 7;
                cmd.Parameters["@DepartmentCode"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@DepartmentCode"].Value = departmentCode;

                // DepartmentValidationCode
                cmd.Parameters.Add("@DepartmentValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@DepartmentValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // DepartmentValidationText
                cmd.Parameters.Add("@DepartmentValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@DepartmentValidationText"].Size = 75;
                cmd.Parameters["@DepartmentValidationText"].Direction = System.Data.ParameterDirection.Output;


                // TravellerNo
                cmd.Parameters.Add("@TravellerNo", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@TravellerNo"].Size = 11;
                cmd.Parameters["@TravellerNo"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@TravellerNo"].Value = travellerNumber;

                // WorkOrderNumber
                cmd.Parameters.Add("@WorkOrderNumber", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@WorkOrderNumber"].Size = 11;
                cmd.Parameters["@WorkOrderNumber"].Direction = System.Data.ParameterDirection.Input;
                cmd.Parameters["@WorkOrderNumber"].Value = "W" + travellerNumber.Substring(3, 6) + "S0" + travellerNumber.Substring(1, 2);

                // WorkOrderNumberValidated
                cmd.Parameters.Add("@WorkOrderNumberValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@WorkOrderNumberValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // WorkOrderNumberValidationText
                cmd.Parameters.Add("@WorkOrderNumberValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@WorkOrderNumberValidationText"].Size = 75;
                cmd.Parameters["@WorkOrderNumberValidationText"].Direction = System.Data.ParameterDirection.Output;

                // BatchID
                cmd.Parameters.Add("@BatchID", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@BatchID"].Size = 27;
                cmd.Parameters["@BatchID"].Direction = System.Data.ParameterDirection.InputOutput;
                cmd.Parameters["@BatchID"].Value = "none";


                // SaveValidationCode
                cmd.Parameters.Add("@SaveValidationCode", System.Data.SqlDbType.SmallInt);
                cmd.Parameters["@SaveValidationCode"].Direction = System.Data.ParameterDirection.Output;

                // SaveValidationText
                cmd.Parameters.Add("@SaveValidationText", System.Data.SqlDbType.VarChar);
                cmd.Parameters["@SaveValidationText"].Size = 75;
                cmd.Parameters["@SaveValidationText"].Direction = System.Data.ParameterDirection.Output;

                using (con)
                {
                    con.ConnectionString = "Data Source=NSQL05;Initial Catalog=NATBC;Persist Security Info=True;User ID=BarcodeUser;Password=PrivateKey(0)";
                    con.Open();
                    DataTable TransferBatch = new DataTable();
                    TransferBatch.Load(cmd.ExecuteReader());
                    TransferBatch.Dispose();
                }
            }

            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                cmd.Dispose();
                con.Close();
            }
        }
        private void ButtonRefresh(string button = "None")
        {
            if (isReferenceWO)
            {
                StartOrderButton.IsEnabled = false;
                SendToOfficeButton.IsEnabled = false;
                FinishOrderButton.IsEnabled = false;
                NotFinishedButton.IsEnabled = false;
                PrintOrderButton.IsEnabled = false;
                DoNotProcessOrderButton.IsEnabled = false;
            }
            else
            {
                if (user.Department != "Engineering")
                {
                    StartOrderButton.IsEnabled = false;
                    SendToOfficeButton.IsEnabled = false;
                    FinishOrderButton.IsEnabled = false;
                    NotFinishedButton.IsEnabled = false;
                    PrintOrderButton.IsEnabled = false;
                }
                else
                {
                    if (button == "None")
                    {
                        if (orderLocation.StartsWith("BeingEntered") || orderLocation.StartsWith("EnteredUnscanned") || orderLocation.StartsWith("InTheOffice"))
                        {
                            StartOrderButton.IsEnabled = true;
                        }
                        if (!orderLocation.StartsWith("EnteredUnscanned") && (orderLocation.StartsWith("BeingEntered") || orderLocation.StartsWith("InEngineering") || orderLocation.StartsWith("ReadyToPrint") || orderLocation.StartsWith("PrintedInEngineering")))
                        {
                            SendToOfficeButton.IsEnabled = true;
                        }
                        if (orderLocation.StartsWith("InEngineering") && workOrder.Finished == false)
                        {
                            FinishOrderButton.IsEnabled = true;
                            NotFinishedButton.IsEnabled = false;
                        }
                        if (orderLocation.StartsWith("InEngineering") && workOrder.Finished == true)
                        {
                            FinishOrderButton.IsEnabled = false;
                            NotFinishedButton.IsEnabled = true;
                            PrintOrderButton.IsEnabled = true;
                        }
                        if (orderLocation.StartsWith("ReadyToPrint"))
                        {
                            FinishOrderButton.IsEnabled = false;
                            NotFinishedButton.IsEnabled = true;
                            PrintOrderButton.IsEnabled = true;
                            PrintOrderButton.Content = "Remove From Print";
                        }
                    }
                    else
                    {
                        if (button == "Start")
                        {
                            StartOrderButton.IsEnabled = false;
                            SendToOfficeButton.IsEnabled = false;
                            FinishOrderButton.IsEnabled = true;
                            NotFinishedButton.IsEnabled = false;
                            PrintOrderButton.IsEnabled = false;
                        }
                        if (button == "Office")
                        {
                            StartOrderButton.IsEnabled = true;
                            SendToOfficeButton.IsEnabled = false;
                            FinishOrderButton.IsEnabled = false;
                            NotFinishedButton.IsEnabled = false;
                            PrintOrderButton.IsEnabled = false;
                        }
                        if (button == "Finish")
                        {
                            StartOrderButton.IsEnabled = false;
                            SendToOfficeButton.IsEnabled = false;
                            FinishOrderButton.IsEnabled = false;
                            NotFinishedButton.IsEnabled = true;
                            PrintOrderButton.IsEnabled = true;
                        }
                    }
                }
            }
            //Dispatcher.Invoke(() => StartOrderButton.UpdateLayout());
            //Dispatcher.Invoke(() => SendToOfficeButton.UpdateLayout());
            //Dispatcher.Invoke(() => FinishOrderButton.UpdateLayout());
            //Dispatcher.Invoke(() => NotFinishedButton.UpdateLayout());
            //Dispatcher.Invoke(() => PrintOrderButton.UpdateLayout());
        }

        #region Events
        private void Window_Closed(object sender, EventArgs e)
        {
            parent.Show();
        }
        private void Order_Info_Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //if (!isReferenceWO)
            //{
            //    using var nat02context = new NAT02Context();
            //    try
            //    {
            //        if (nat02context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User == user.GetUserName()).Any())
            //        {
            //            var orderBeingChecked = new EoiOrdersBeingChecked()
            //            {
            //                OrderNo = workOrder.OrderNumber,
            //                User = user.GetUserName()
            //            };
            //            nat02context.EoiOrdersBeingChecked.Remove(orderBeingChecked);

            //            nat02context.SaveChanges();
            //        }

            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.Message);
            //    }
            //    nat02context.Dispose();
            //}
            using var nat02context = new NAT02Context();
            try
            {
                if (nat02context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User == user.GetUserName()).Any())
                {
                    var orderBeingChecked = new EoiOrdersBeingChecked()
                    {
                        OrderNo = workOrder.OrderNumber,
                        User = user.GetUserName()
                    };
                    nat02context.EoiOrdersBeingChecked.Remove(orderBeingChecked);

                    nat02context.SaveChanges();
                }
                parent.ChildWindow = this.Title;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            nat02context.Dispose();
        }
        private void Order_Info_Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Environment.CurrentDirectory.Contains("Debug"))
            {
                using var _nat02context = new NAT02Context();
                var orderBeingChecked = new EoiOrdersBeingChecked()
                {
                    OrderNo = workOrder.OrderNumber,
                    User = user.GetUserName()
                };
                _nat02context.EoiOrdersBeingChecked.Add(orderBeingChecked);

                _nat02context.SaveChanges();
                _nat02context.Dispose();
            }

            //using var _nat02context = new NAT02Context();
            //var orderBeingChecked = new EoiOrdersBeingChecked()
            //{
            //    OrderNo = workOrder.OrderNumber,
            //    User = user.GetUserName()
            //};
            //_nat02context.EoiOrdersBeingChecked.Add(orderBeingChecked);

            //_nat02context.SaveChanges();
            //_nat02context.Dispose();
            Dispatcher.Invoke(() => { ButtonRefresh(); });
        }
        private void Order_Info_Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OrderPanel.Width = Order_Info_Window.ActualWidth - ButtonPanel.ActualWidth - ButtonPanel.Margin.Left - 35;
        }

        #region Clicks
        private void CSRTextBlock_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Cursor = Cursors.Wait;
                Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)
                    app.Application.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                mailItem.To = IMethods.GetEmailAddress(workOrder.Csr.Split(' ')[0]);
                mailItem.Body = "";
                mailItem.BCC = "intlcs6@natoli.com;customerservice5@natoli.com";
                mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceLow;
                mailItem.Display(true);
                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Cursor = Cursors.Arrow;
            }
        }
        #region Button Clicks

        #region Order Movement
        private void StartOrderButton_Click(object sender, RoutedEventArgs e)
        {
            int retVal = workOrder.TransferOrder(user, "D040", true);
            if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }
            
            Dispatcher.Invoke(() => { ButtonRefresh("Start"); });
            parent.BoolValue = true;
        }
        private void SendToOfficeButton_Click(object sender, RoutedEventArgs e)
        {
            int retVal = workOrder.TransferOrder(user, "D080", true);
            if (retVal == 1) { MessageBox.Show(workOrder.OrderNumber.ToString() + " was not transferred sucessfully."); }
            foreach (int lineNumber in workOrder.lineItems.Keys)
            {
                DeleteMachineVariables(orderNumber.ToString().Trim(), lineNumber);
            } 

            if (workOrder.Finished)
            {
                using var context = new NAT02Context();
                if (context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == workOrder.OrderNumber).Any())
                {
                    var orderMarkedForChecking = new EoiOrdersMarkedForChecking()
                    {
                        OrderNo = workOrder.OrderNumber
                    };
                    context.EoiOrdersMarkedForChecking.Remove(orderMarkedForChecking);

                    context.SaveChanges();
                }
            }

            try
            {
                Cursor = Cursors.Wait;
                Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
                Microsoft.Office.Interop.Outlook.MailItem mailItem = (Microsoft.Office.Interop.Outlook.MailItem)
                    app.Application.CreateItem(Microsoft.Office.Interop.Outlook.OlItemType.olMailItem);
                mailItem.Subject = "REQUEST FOR CHANGES WO# " + workOrder.OrderNumber.ToString();
                mailItem.To = IMethods.GetEmailAddress(workOrder.Csr.Split(' ')[0]);
                mailItem.Body = "";
                mailItem.BCC = "intlcs6@natoli.com;customerservice5@natoli.com";
                mailItem.Importance = Microsoft.Office.Interop.Outlook.OlImportance.olImportanceHigh;
                mailItem.Display(false);
                Cursor = Cursors.Arrow;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Cursor = Cursors.Arrow;
            }

        }
        private void FinishOrderButton_Click(object sender, RoutedEventArgs e)
        {
            string missingPrints = "";
            string path = @"\\engserver\workstations\tool_drawings\" + workOrder.OrderNumber + @"\";
            string file = "";
            foreach (OrderLineItem oli in orderLineItems)
            {
                switch (oli.LineItemType.Trim())
                {
                    case "U":
                    case "UT":
                    case "UA":
                    case "UH":
                    case "UC":
                    case "UHD":
                        file = "UPPER";
                        if (!File.Exists(path + file + ".pdf") && !missingPrints.Contains(file))
                        {
                            missingPrints += file + ",";
                        }
                        break;
                    case "L":
                    case "LT":
                    case "LA":
                    case "LH":
                    case "LC":
                    case "LHD":
                        file = "LOWER";
                        if (!File.Exists(path + file + ".pdf") && !missingPrints.Contains(file))
                        {
                            missingPrints += file + ",";
                        }
                        break;
                    case "R":
                    case "RT":
                    case "RA":
                    case "RH":
                    case "RC":
                    case "RHD":
                        file = "REJECT";
                        if (!File.Exists(path + file + ".pdf") && !missingPrints.Contains(file))
                        {
                            missingPrints += file + ",";
                        }
                        break;
                    case "A":
                        file = "ALIGNMENT";
                        if (!File.Exists(path + file + ".pdf") && !missingPrints.Contains(file))
                        {
                            missingPrints += file + ",";
                        }
                        break;
                    case "D":
                    case "DH":
                    case "DA":
                    case "DI":
                    case "DS":
                        file = "DIE";
                        if (!File.Exists(path + file + ".pdf") && !missingPrints.Contains(file))
                        {
                            missingPrints += file + ",";
                        }
                        break;
                    default:
                        break;
                }
            }
            MessageBoxResult result = MessageBoxResult.Yes;
            if (!string.IsNullOrEmpty(missingPrints))
            {
                result = MessageBox.Show(
                    "Are you sure you want to finish this order?" + "\n" +
                    "This order appears to be missing: " + missingPrints.Remove(missingPrints.Length - 1) + " print(s) in the work order folder."
                    , "Continue?", MessageBoxButton.YesNo, MessageBoxImage.Question);
            }
            if (result == MessageBoxResult.Yes)
            {
                using var context = new NAT02Context();

                int count = context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == workOrder.OrderNumber).Count();

                if (count < 1)
                {
                    var orderMarkedForChecking = new EoiOrdersMarkedForChecking()
                    {
                        OrderNo = workOrder.OrderNumber
                    };
                    context.EoiOrdersMarkedForChecking.Add(orderMarkedForChecking);

                    context.SaveChanges();
                    workOrder.Finished = true;
                    Dispatcher.Invoke(() => { ButtonRefresh("Finish"); });
                }
                parent.BoolValue = true;
            }
        }
        private void NotFinishedButton_Click(object sender, RoutedEventArgs e)
        {
            using var context = new NAT02Context();
            var orderMarkedForChecking = new EoiOrdersMarkedForChecking()
            {
                OrderNo = workOrder.OrderNumber
            };
            context.EoiOrdersMarkedForChecking.Remove(orderMarkedForChecking);

            context.SaveChanges();
            workOrder.Finished = false;
            parent.BoolValue = true;
            Close();
        }
        private void PrintOrderButton_Click(object sender, RoutedEventArgs e)
        {

            using var nat01Context = new NAT01Context();
            using var nat02Context = new NAT02Context();
            try
            {
                if (nat02Context.EoiOrdersDoNotProcess.Any(o => o.OrderNo == workOrder.OrderNumber))
                {
                    MessageBox.Show("This order is marked to not be processed at this time." + "\n" + "The order was marked by " + nat02Context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == workOrder.OrderNumber).FirstOrDefault().UserName + ".", "", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    bool mark_print = PrintOrderButton.Content.ToString() == "Print Order";
                    OrderHeader order = nat01Context.OrderHeader.Where(o => o.OrderNo == workOrder.OrderNumber * 100).First();
                    order.PrintOrderStatus = mark_print ? "Y" : "";
                    order.WorkOrderPrintQty = mark_print ? (short)(nat01Context.OrderHeader.Where(o => o.OrderNo == workOrder.OrderNumber * 100).FirstOrDefault().WorkOrderPrintQty + 1) :
                                                           (short)(nat01Context.OrderHeader.Where(o => o.OrderNo == workOrder.OrderNumber * 100).FirstOrDefault().WorkOrderPrintQty - 1);
                    nat01Context.OrderHeader.Update(order);
                    nat01Context.SaveChanges();

                    if (mark_print)
                    {
                        EoiOrdersMarkedForChecking orderMarked = nat02Context.EoiOrdersMarkedForChecking.Where(o => o.OrderNo == workOrder.OrderNumber).FirstOrDefault();
                        nat02Context.EoiOrdersMarkedForChecking.Remove(orderMarked);
                    }
                    else
                    {
                        EoiOrdersMarkedForChecking orderMarked = new EoiOrdersMarkedForChecking()
                        {
                            OrderNo = workOrder.OrderNumber
                        };
                        nat02Context.EoiOrdersMarkedForChecking.Add(orderMarked);
                    }

                    if (mark_print)
                    {
                        try
                        {
                            EoiOrdersCheckedBy orderCheckedBy = new EoiOrdersCheckedBy
                            {
                                CheckedBy = user.GetUserName(),
                                OrderNo = orderNumber
                            }; // nat02Context.EoiOrdersCheckedBy.Where(o => o.OrderNo == workOrder.OrderNumber).First();
                            nat02Context.EoiOrdersCheckedBy.Add(orderCheckedBy);
                        }
                        catch
                        {
                            EoiOrdersCheckedBy orderCheckedBy = new EoiOrdersCheckedBy
                            {
                                CheckedBy = user.GetUserName(),
                                OrderNo = orderNumber
                            }; // nat02Context.EoiOrdersCheckedBy.Where(o => o.OrderNo == workOrder.OrderNumber).First();
                            nat02Context.EoiOrdersCheckedBy.Add(orderCheckedBy);
                        }
                    }
                    else
                    {
                        EoiOrdersCheckedBy orderCheckedBy = nat02Context.EoiOrdersCheckedBy.Where(o => o.OrderNo == workOrder.OrderNumber).First();
                        nat02Context.EoiOrdersCheckedBy.Remove(orderCheckedBy);
                    }
                    nat02Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            nat01Context.Dispose();
            nat02Context.Dispose();
            parent.BoolValue = true;
            Close();
        }
        private void DoNotProcessOrderButton_Click(object sender, RoutedEventArgs e)
        {
            using var context = new NAT02Context();
            EoiOrdersDoNotProcess order;

            if (doNotProc)
            {
                order = context.EoiOrdersDoNotProcess.Where(o => o.OrderNo == workOrder.OrderNumber).FirstOrDefault();
                context.EoiOrdersDoNotProcess.Remove(order);
            }
            else
            {
                order = new EoiOrdersDoNotProcess { OrderNo = orderNumber, UserName = user.GetUserName() };
                context.EoiOrdersDoNotProcess.Add(order);
            }
            context.SaveChanges();
            context.Dispose();
            doNotProc = !doNotProc;
            parent.BoolValue = true;
            Close();
        }
        #endregion

        #region Open Folders
        private void OpenWOFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"\\engserver\workstations\tool_drawings\" + workOrder.OrderNumber + @"\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + path + @"""");
                IMethods.BringProcessToFront("explorer");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OpenCustomerMachineFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var nat01context = new NAT01Context();
                string mach = nat01context.MachineList.Where(m => m.MachineNo == orderLineItems[lineItemNumber - 1].MachineNo).Select(m => m.MachineNo.ToString().Trim() + "-" + m.Description.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim()).FirstOrDefault();
                nat01context.Dispose();
                string folderName = @"\\engserver\workstations\tools\Customers\" + workOrder.UserNumber + " - " + workOrder.EndUserName.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\"; // orderLineItems[lineItemNumber - 1].MachineNo + "-" + orderLineItems[lineItemNumber - 1].MachineDescription.Trim().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                if (System.IO.Directory.Exists(folderName))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                    IMethods.BringProcessToFront("explorer");
                }
                else
                {
                    folderName = @"\\engserver\workstations\tools\Customers\" + workOrder.UserNumber + " - " + workOrder.EndUserName.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                    if (System.IO.Directory.Exists(folderName))
                    {
                        System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                        IMethods.BringProcessToFront("explorer");
                    }
                    else
                    {
                        folderName = @"\\engserver\workstations\tools\Customers\" + workOrder.UserNumber + " - " + workOrder.EndUserName.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\";
                        Directory.CreateDirectory(folderName);
                    }
                }
            }
            catch
            {
                
            }
        }
        private void OpenTabletFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string lineItemType = orderLineItems[lineItemNumber - 1].LineItemType;
                if (lineItemType == "U" ||
               lineItemType == "L" ||
               lineItemType == "R" ||
               lineItemType == "UT" ||
               lineItemType == "LT" ||
               lineItemType == "RT" ||
               lineItemType == "H" ||
               lineItemType == "LCRP"
               )
                {
                    string folderBeginning = orderLineItems[lineItemNumber - 1].HobNoShapeID.Trim().Remove(3);
                    if (folderBeginning.First() == '0')
                    {
                        folderBeginning = folderBeginning.Remove(0, 1);
                    }
                    string path = @"\\nsql03\data1\DRAW\E-DRAWINGS\" + folderBeginning + @"-E-DRAWINGS\";
                    Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void OpenWorkOrdersFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string path = @"L:\WorkOrders\" + workOrder.OrderNumber+@"\";
                string quotePath = @"L:\Quotes\" + workOrder.QuoteNumber + @"\";
                if (!string.IsNullOrWhiteSpace(workOrder.QuoteNumber) && Directory.Exists(quotePath))
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    foreach (string file in Directory.GetFiles(quotePath))
                    {
                        try
                        {
                            File.Move(file, path + file.Remove(0, quotePath.Length));
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                    }
                    // I do not know if this works
                    //if (Directory.GetFiles(quotePath).Length == 0)
                    //{
                    //    Directory.Delete(quotePath);
                    //}
                }
                if (Directory.Exists(path))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + path + @"""");
                    IMethods.BringProcessToFront("explorer");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Project_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string projectPath = @"R:\TOOLING AUTOMATION\Project Specifications\" + workOrder.ProjectNumber.ToString().Trim() + @"\";
                string orderPath = @"L:\WorkOrders\" + workOrder.OrderNumber + @"\";
                if (Directory.Exists(projectPath))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + projectPath + @"""");
                    IMethods.BringProcessToFront("explorer");
                }
                else if (Directory.Exists(orderPath))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + orderPath + @"""");
                    IMethods.BringProcessToFront("explorer");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        #region SMI Buttons
        private void SoldToSMIs_Click(object sender, RoutedEventArgs e)
        {
            SMI SMIWindow = new SMI(workOrder.SoldToCustomerName, workOrder.CustomerNumber)
            {
                Owner = this,
                Width = Math.Max(ActualWidth - 50, 50),
                Height = Math.Max(ActualHeight - 50, 50)
            };
            SMIWindow.Show();
        }
        private void EndUserSMIs_Click(object sender, RoutedEventArgs e)
        {
            SMI SMIWindow = new SMI(workOrder.EndUserName, workOrder.UserNumber)
            {
                Owner = this,
                Width = Math.Max(ActualWidth - 50, 50),
                Height = Math.Max(ActualHeight - 50, 50)
            };
            SMIWindow.Show();
        }
        private void ShipToSMIs_Click(object sender, RoutedEventArgs e)
        {
            SMI SMIWindow = new SMI(workOrder.ShipToCustomerName, workOrder.AccountNumber)
            {
                Owner = this,
                Width = Math.Max(ActualWidth - 50, 50),
                Height = Math.Max(ActualHeight - 50, 50)
            };
            SMIWindow.Show();
        }
        #endregion

        private void ReferenceOrderButton_Click(object sender, RoutedEventArgs e)
        {
            using var _nat01context = new NAT01Context();
            try
            {
                WorkOrder refWorkOrder = new WorkOrder((int)workOrder.ReferenceOrder, this);
                OrderInfoWindow referenceOrderInfoWindow = new OrderInfoWindow(refWorkOrder, parent, orderLocation, user, true)
                {
                    Left = Left,
                    Top = Top
                };
                _nat01context.Dispose();
                referenceOrderInfoWindow.Show();
                referenceOrderInfoWindow.Dispose();
            }
            catch (Exception ex)
            {
                _nat01context.Dispose();
                MessageBox.Show(ex.Message);

            }

}
        private void QuoteButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            using var nat01context = new NAT01Context();
            try
            {
                string quoteNumber = workOrder.QuoteNumber.Split('-')[0];
                string revNumber = workOrder.QuoteNumber.Split('-')[1];
                WindowCollection collection = App.Current.Windows;
                foreach (Window w in collection)
                {
                    if (w.Title.Contains(quoteNumber))
                    {
                        nat01context.Dispose();
                        w.WindowState = WindowState.Normal;
                        w.Show();
                        goto AlreadyOpen;
                    }
                }
                quote = new Quote(int.Parse(quoteNumber), short.Parse(revNumber));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, parent, "", user)
                {
                    Owner = this,
                    Left = Left,
                    Top = Top
                };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        AlreadyOpen:
            nat01context.Dispose();
            Cursor = Cursors.Arrow;
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion
        #endregion

        #region Line Item Stuff
        private void LineItemsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            DataRowView row = (DataRowView)dataGrid.SelectedCells.FirstOrDefault().Item;
            try
            {
                lineItemNumber = short.Parse(row["LineNumber"].ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            LineItemChange();
        }
        private void Order_Info_Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta;
            short itemNo = lineItemNumber;
            if (e.Delta > 0)
                itemNo = (short)Math.Max(itemNo - e.Delta / 120, 1);
            if (e.Delta < 0)
                itemNo = (short)Math.Min(itemNo - e.Delta / 120, workOrder.LineItemCount);
            lineItemNumber = itemNo;
            LineItemsDataGrid.SelectedItem = LineItemsDataGrid.Items[itemNo - 1];
        }
        private void LineItemsDataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Dictionary<string, string> pdfDict = new Dictionary<string, string> {
                { "A", "ALIGNMENT"},
                { "CT", "TABLETS"},
                { "D", "DIE"},
                { "DA", "DIE"},
                { "DC", "DIE"},
                { "DH", "DIE"},
                { "DI", "DIE"},
                { "DP", "DIE"},
                { "DS", "DIE"},
                { "K", "KEY"},
                { "L", "LOWER"},
                { "LA", "LOWER"},
                { "LC", "LOWER"},
                { "LCR", "LOWER"},
                { "LCRK", "LOWER"},
                { "LCRP", "LOWER"},
                { "LH", "LOWER"},
                { "LHD", "LOWER"},
                { "LT", "LOWER"},
                { "R", "REJECT"},
                { "RA", "REJECT"},
                { "RC", "REJECT"},
                { "RH", "REJECT"},
                { "RHD", "REJECT"},
                { "RT", "REJECT"},
                { "U", "UPPER"},
                { "UA", "UPPER"},
                { "UC", "UPPER"},
                { "UH", "UPPER"},
                { "UHD", "UPPER"},
                { "UT", "UPPER"},
            };
            DataGrid dataGrid = (DataGrid)sender;
            DataGridCellInfo cell = dataGrid.SelectedCells[0];
            string lineNumber = ((TextBlock)cell.Column.GetCellContent(cell.Item)).Text;
            string detailTypeID = workOrder.lineItems[Convert.ToInt32(lineNumber)];
            string fileName = "";
            try
            {
                fileName = pdfDict[detailTypeID];
            }
            catch
            {
                MessageBox.Show("Detail Type ID '" + detailTypeID + "' does not have a standardized file name." + "\n" + "Try opening the folder to view the file.", "Nothing To Open", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            try
            {
                string path = @"\\engserver\workstations\tool_drawings\" + workOrder.OrderNumber + @"\" + fileName + ".pdf";
                Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                //MessageBox.Show("Could not locate file: '" + fileName + "' in '" + workOrder.OrderNumber + "'." + "\n" + "Please ensure the file is in the folder before trying to open.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        private void LineItemsDataGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                Dictionary<string, string> pdfDict = new Dictionary<string, string> {
                { "A", "ALIGNMENT_M"},
                { "CT", "TABLETS"},
                { "D", "DIE_M"},
                { "DA", "DIE_M"},
                { "DC", "DIE_M"},
                { "DH", "DIE_M"},
                { "DI", "DIE_M"},
                { "DP", "DIE_M"},
                { "DS", "DIE_M"},
                { "K", "KEY_M"},
                { "L", "LOWER_M"},
                { "LA", "LOWER_M"},
                { "LC", "LOWER_M"},
                { "LCR", "LOWER_M"},
                { "LCRK", "LOWER_M"},
                { "LCRP", "LOWER_M"},
                { "LH", "LOWER_M"},
                { "LHD", "LOWER_M"},
                { "LT", "LOWER_M"},
                { "R", "REJECT_M"},
                { "RA", "REJECT_M"},
                { "RC", "REJECT_M"},
                { "RH", "REJECT_M"},
                { "RHD", "REJECT_M"},
                { "RT", "REJECT_M"},
                { "U", "UPPER_M"},
                { "UA", "UPPER_M"},
                { "UC", "UPPER_M"},
                { "UH", "UPPER_M"},
                { "UHD", "UPPER_M"},
                { "UT", "UPPER_M"},
            };
                DataGrid dataGrid = (DataGrid)sender;
                DataGridCellInfo cell = dataGrid.SelectedCells[0];
                string lineNumber = ((TextBlock)cell.Column.GetCellContent(cell.Item)).Text;
                string detailTypeID = workOrder.lineItems[Convert.ToInt32(lineNumber)];
                string fileName = "";
                try
                {
                    fileName = pdfDict[detailTypeID];
                }
                catch
                {
                    MessageBox.Show("Detail Type ID '" + detailTypeID + "' does not have a standardized file name." + "\n" + "Try opening the folder to view the file.", "Nothing To Open", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                try
                {
                    string path = @"\\engserver\workstations\tool_drawings\" + workOrder.OrderNumber + @"\" + fileName + ".pdf";
                    Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    //MessageBox.Show("Could not locate file: '" + fileName + "' in '" + workOrder.OrderNumber + "'." + "\n" + "Please ensure the file is in the folder before trying to open.", "File Not Found", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        #endregion

        #endregion

        protected static bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;

            if (e != null && (e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
            {
                Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
                if (data != null)
                {
                    if ((data.Length == 1) && (data.GetValue(0) is String))
                    {
                        filename = ((string[])data)[0];
                        string ext = System.IO.Path.GetExtension(filename).ToLower();
                        if (ext == ".pdf")
                        {
                            ret = true;
                        }
                    }
                }
            }
            return ret;
        }

        private void OrderPanel_Drop(object sender, DragEventArgs e)
        {
            string tempFile;
            string[] filePaths = (string[])(e.Data.GetData(DataFormats.FileDrop));
            bool wrongWO = false;
            foreach (string file in filePaths)
            {
                string woFolderName = file.Remove(file.LastIndexOf('\\'));
                woFolderName = woFolderName.Remove(0, woFolderName.LastIndexOf('\\') + 1);
                if (woFolderName != workOrder.OrderNumber.ToString())
                {
                    wrongWO = true;
                    break;
                }
                tempFile = file.Replace(".pdf", "_TEMP.pdf");
                PdfDocument pdfDocument = new PdfDocument(new PdfReader(file), new PdfWriter(tempFile));
                int page_count = pdfDocument.GetNumberOfPages();
                Document document = new Document(pdfDocument);
                for (int i = 1; i <= page_count; i++)
                {
                    ImageData imageData = ImageDataFactory.Create(@"C:\Users\" + user.DomainName + @"\Desktop\John Hancock.png");
                    iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData).ScaleAbsolute(22, 22)
                                                                                                .SetFixedPosition(i, user.SignatureLeft, user.SignatureBottom);
                    document.Add(image);
                }
                document.Close();
                File.Move(tempFile, file, true);

                int file_count;
                string lineItemName = file.Substring(file.LastIndexOf("\\") + 1, file.IndexOf(".pdf") - file.LastIndexOf("\\") - 1);
                try
                {
                    int lineItemNumber = workOrder.lineItems.Where(l => lineItemName.StartsWith(l.Value)).First().Key;
                    file_count = lineItemName.EndsWith("_M") ? lineItemNumber * 2 : lineItemNumber * 2 - 1;
                }
                catch
                {
                    string max = Directory.GetFiles(@"C:\Users\" + user.DomainName + @"\Desktop\WorkOrdersToPrint\")
                                          .Where(f => f.Contains(workOrder.OrderNumber.ToString()))
                                          .OrderByDescending(f => f[(f.IndexOf("_") + 1)..(f.IndexOf(".pdf") - 1)])
                                          .FirstOrDefault();
                    int lineItemNumber = string.IsNullOrEmpty(max) ? 100 :
                                                       int.Parse(max[(max.IndexOf("_") + 1)..(max.IndexOf(".pdf"))]) < 100 ? 100 :
                                                                                       int.Parse(max[(max.IndexOf("_") + 1)..(max.IndexOf(".pdf"))]) + 1;
                    file_count = lineItemNumber;
                }
                try
                {
                    File.Copy(file, @"C:\Users\" + user.DomainName + @"\Desktop\WorkOrdersToPrint\" + workOrder.OrderNumber + "_" + file_count + ".pdf", true);
                }
                catch
                {

                }
            }
            if(wrongWO)
            {
                MessageBox.Show("This folder does not match the Work Order Number.\n" + "Nothing was done with the pdfs", "Wrong WO#", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private void PdfSignature(string[] filePaths)
        {
            string filename;
            string tempFile;

            var passCert = user.Password;
            foreach (string file in filePaths)
            {
                filename = file;
                tempFile = filename.Replace(".pdf", "_TEMP.pdf");
                PdfReader pdfReader = new PdfReader(filename);
                PdfWriter pdfWriter = new PdfWriter(tempFile);
                PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter);
                int page_count = pdfDocument.GetNumberOfPages();
                Document document = new Document(pdfDocument);
                PdfAcroForm acroForm = PdfAcroForm.GetAcroForm(pdfDocument, true);

                int count = acroForm.GetFormFields().Count;
                if (count > 0) { document.Close(); File.Delete(tempFile); goto end; }

                if (page_count > 1)
                {
                    for (int i = 2; i <= page_count; i++)
                    {
                        ImageData imageData = ImageDataFactory.Create(@"C:\Users\" + user.DomainName + @"\Desktop\John Hancock.png");
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData).ScaleAbsolute(22, 22).SetFixedPosition(i, 950, 20);
                        document.Add(image);
                    }
                }

                PdfFormField pdfSignatureFormField = PdfFormField.CreateSignature(pdfDocument, new Rectangle(950, 20, 35, 20))
                                                                .SetFieldName("Signature" + count)
                                                                .SetVisibility(PdfFormField.VISIBLE)
                                                                .SetPage(1);
                acroForm.AddField(pdfSignatureFormField);
                document.Close();

                PdfReader reader = new PdfReader(tempFile);
                PdfSigner signer = new PdfSigner(reader, new FileStream(filename, FileMode.Open), new StampingProperties());
                PdfSignatureAppearance appearance = signer.GetSignatureAppearance()
                                                        .SetImage(ImageDataFactory.Create(@"C:\Users\" + user.DomainName + @"\Desktop\John Hancock.png"))
                                                        .SetReuseAppearance(false)
                                                        .SetLayer2Text("")
                                                        .SetPageNumber(1);

                signer.SetFieldName("Signature" + count);
                signer.SetSignDate(System.DateTime.Now);
                var pathToCert = @"C:\Users\" + user.DomainName + @"\Desktop\cert.pfx";
                //var passCert = "614_Biltmore";
                var pass = passCert.ToCharArray();
                FileStream fs;
                fs = new FileStream(pathToCert, FileMode.Open);
                var store = new Pkcs12Store(fs, pass);
                fs.Close();
                var alias = "";
                // searching for private key
                foreach (string al in store.Aliases)
                    if (store.IsKeyEntry(al) && store.GetKey(al).Key.IsPrivate)
                    {
                        alias = al;
                        break;
                    }

                var pk = store.GetKey(alias);

                ICollection<X509Certificate> chain = store.GetCertificateChain(alias).Select(c => c.Certificate).ToList();
                Org.BouncyCastle.X509.X509Certificate[] x509Certificates = chain.ToArray();

                var parameters = pk.Key as RsaPrivateCrtKeyParameters;
                IExternalSignature pks = new PrivateKeySignature(parameters, DigestAlgorithms.SHA256);
                signer.SignDetached(pks, x509Certificates, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
                reader.Close();
                File.Delete(tempFile);
                int file_count;
                string lineItemName = file.Substring(file.LastIndexOf("\\") + 1, file.IndexOf(".pdf") - file.LastIndexOf("\\") - 1);
                int lineItemNumber = workOrder.lineItems.Where(l => lineItemName.StartsWith(l.Value)).First().Key;
                file_count = lineItemName.EndsWith("_M") ? lineItemNumber * 2 : lineItemNumber * 2 - 1;
                File.Copy(filename, @"C:\Users\" + user.DomainName + @"\Desktop\WorkOrdersToPrint\" + workOrder.OrderNumber + "_" + file_count + ".pdf", false);
            end:;
                pdfReader.Close();
                pdfWriter.Dispose();
                pdfDocument.Close();
            }
        }

        private void OrderPanel_DragEnter(object sender, DragEventArgs e)
        {
            string filename;
            validData = GetFilename(out filename, e);
            if (validData)
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~OrderInfoWindow()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
