using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace NatoliOrderInterface
{
    public class WorkOrder
    {
        #region SettersAndGetters
        private int orderNumber;
        public int OrderNumber
        {
            get { return orderNumber; }
            set { this.orderNumber = value; }
        }

        private string customerNumber;
        public string CustomerNumber
        {
            get { return customerNumber; }
            set { this.customerNumber = value; }
        }

        private string newCustomer;
        public string NewCustomer
        {
            get { return newCustomer; }
            set { this.newCustomer = value; }
        }

        private string soldToCustomerName;
        public string SoldToCustomerName
        {
            get { return soldToCustomerName; }
            set { this.soldToCustomerName = value; }
        }

        private DateTime orderDate;
        public DateTime OrderDate
        {
            get { return orderDate; }
            set { this.orderDate = value; }
        }

        private string poNumber;
        public string PoNumber
        {
            get { return poNumber; }
            set { this.poNumber = value; }
        }

        private string shipToCustomerName;
        public string ShipToCustomerName
        {
            get { return shipToCustomerName; }
            set { this.shipToCustomerName = value; }
        }

        private DateTime shipDate;
        public DateTime ShipDate
        {
            get { return shipDate; }
            set { this.shipDate = value; }
        }

        private string endUserName;
        public string EndUserName
        {
            get { return endUserName; }
            set { this.endUserName = value; }
        }

        private int? referenceOrder;
        public int? ReferenceOrder
        {
            get { return referenceOrder; }
            set { this.referenceOrder = value; }
        }

        private string engineeringNotes;
        public string EngineeringNotes
        {
            get { return engineeringNotes; }
            set { this.engineeringNotes = value; }
        }

        private string csr;
        public string Csr
        {
            get { return csr; }
            set { this.csr = value; }
        }

        private string accountNumber;
        public string AccountNumber
        {
            get { return accountNumber; }
            set { this.accountNumber = value; }
        }

        private string terms;
        public string Terms
        {
            get { return terms; }
            set { this.terms = value; }
        }

        private string soldBy;
        public string SoldBy
        {
            get { return soldBy; }
            set { this.soldBy = value; }
        }

        private string shippedVia;
        public string ShippedVia
        {
            get { return shippedVia; }
            set { this.shippedVia = value; }
        }

        private string quoteNumber;
        public string QuoteNumber
        {
            get { return quoteNumber; }
            set { this.quoteNumber = value; }
        }

        private string reference;
        public string Reference
        {
            get { return reference; }
            set { this.reference = value; }
        }

        private int? projectNumber;
        public int? ProjectNumber
        {
            get { return projectNumber; }
            set { this.projectNumber = value; }
        }

        private string hobRequired;
        public string HobRequired
        {
            get { return hobRequired; }
            set { this.hobRequired = value; }
        }

        private string userNumber;
        public string UserNumber
        {
            get { return userNumber; }
            set { this.userNumber = value; }
        }

        private string productName;
        public string ProductName
        {
            get { return productName; }
            set { this.productName = value; }
        }

        private string rushYOrN;
        public string RushYOrN
        {
            get { return rushYOrN; }
            set { this.rushYOrN = value; }
        }

        private string paidRushFee;
        public string PaidRushFee
        {
            get { return paidRushFee; }
            set { this.paidRushFee = value; }
        }

        private List<OrderLineItem> lineItemsList;
        public List<OrderLineItem> LineItemsList
        {
            get { return lineItemsList; }
            set { this.lineItemsList = value; }
        }
        #endregion

        public int LineItemCount { get; set; }
        public Dictionary<int, string> lineItems = new Dictionary<int, string>();
        public bool Finished { get; set; }
        public bool CanRunOnAutocell { get; set; }
        private List<int> lineItemsToScan = new List<int>();
        private StackPanel stackPanelTemp;
        private Window parent;

        public WorkOrder() { }

        /// <summary>
        /// Instance of a work order, complete with all details about the work order
        /// </summary>
        /// <param name="orderNumber"></param>
        public WorkOrder(int orderNumber, Window parent)
        {
            OrderNumber = orderNumber;
            Finished = false;
            CanRunOnAutocell = false;
            this.parent = parent;

            // nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber).Load();
            using (var context = new NAT02Context())
            {
                Finished = context.EoiOrdersMarkedForChecking.Any(o => o.OrderNo == OrderNumber);
                context.Dispose();
            }
            using (var nat01context = new NAT01Context())
            {
                LineItemCount = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).Count();
                lineItems = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).ToDictionary(kvp => (int)kvp.LineNumber, kvp => kvp.DetailTypeId.Trim());
                OrderHeader orderHeader = nat01context.OrderHeader.Where(o => o.OrderNo == OrderNumber * 100).FirstOrDefault();
                List<OrderDetails> orderDetails = nat01context.OrderDetails.Where(o => o.OrderNo == OrderNumber * 100).ToList();
                string repId = "";
                string csr = "*NO CSR*";
                if (nat01context.QuoteHeader.Any(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo))
                {
                    repId = string.IsNullOrEmpty(nat01context.QuoteHeader.Where(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).First().QuoteRepId) ? "" : nat01context.QuoteHeader.Where(q => q.QuoteNo == orderHeader.QuoteNumber && q.QuoteRevNo == orderHeader.QuoteRevNo).First().QuoteRepId.Trim();
                    if (nat01context.QuoteRepresentative.Any(qr => qr.RepId == repId))
                    {
                        csr = string.IsNullOrEmpty(nat01context.QuoteRepresentative.Where(qr => qr.RepId == repId).First().Name) ? "*NO CSR*" : nat01context.QuoteRepresentative.Where(qr => qr.RepId == repId).First().Name.Trim();
                    }
                }
                string customerName; string endUserName;
                using (var ctx = new NECContext())
                {
                    customerName = ctx.Rm00101.Where(c => c.Custnmbr == orderHeader.CustomerNo).FirstOrDefault().Custname;
                    endUserName = ctx.Rm00101.Where(c => c.Custnmbr == orderHeader.UserAcctNo).FirstOrDefault().Custname;
                    ctx.Dispose();
                }
                nat01context.Dispose();
                SetInfo(orderHeader, csr, customerName, endUserName);
            }
        }

        public void SetInfo(OrderHeader orderHeader, string csr, string customerName, string endUserName)
        {
            customerNumber = string.IsNullOrEmpty(orderHeader.CustomerNo) ? "" : orderHeader.CustomerNo.Trim();
            newCustomer = string.IsNullOrEmpty(orderHeader.NewCustomer) ? "" : orderHeader.NewCustomer.Trim();
            soldToCustomerName = string.IsNullOrEmpty(customerName) ? "" : customerName.Trim();
            orderDate = (DateTime)orderHeader.OrderDate;
            poNumber = (string.IsNullOrEmpty(orderHeader.Ponumber) ? "" :  orderHeader.Ponumber.Trim()) + ' ' + (string.IsNullOrEmpty(orderHeader.Poextension) ? "" : orderHeader.Poextension.Trim());
            shipToCustomerName = string.IsNullOrEmpty(orderHeader.ShiptoName) ? "" : orderHeader.ShiptoName.Trim();
            shipDate = (DateTime)orderHeader.RequestedShipDateRev;
            if (shipDate.ToShortDateString() != "01/01/1901")
            {
                shipDate = (DateTime)orderHeader.RequestedShipDate;
            }
            this.endUserName = string.IsNullOrEmpty(endUserName) ? "" : endUserName.Trim();
            referenceOrder = orderHeader.RefWo;
            engineeringNotes = orderHeader.EngineeringNote1.Trim() + "\n" + orderHeader.EngineeringNote2.Trim() + "\n" + orderHeader.MiscNote.Trim();
            this.csr = string.IsNullOrEmpty(csr) ? "" : csr.Trim();
            accountNumber = string.IsNullOrEmpty(orderHeader.ShipToAccountNo) ? "" : orderHeader.ShipToAccountNo.Trim();
            terms = string.IsNullOrEmpty(orderHeader.TermsId) ? "" : orderHeader.TermsId.Trim();
            soldBy = string.IsNullOrEmpty(orderHeader.SalesRepId) ? "" : orderHeader.SalesRepId.Trim();
            shippedVia = string.IsNullOrEmpty(orderHeader.ShippedVia) ? "" : orderHeader.ShippedVia.Trim();
            quoteNumber = orderHeader.QuoteNumber + "-" + orderHeader.QuoteRevNo;
            reference = string.IsNullOrEmpty(orderHeader.Reference) ? "" : orderHeader.Reference.Trim();
            projectNumber = orderHeader.ProjectNo;
            hobRequired = string.IsNullOrEmpty(orderHeader.HobRequired) ? "" : orderHeader.HobRequired.Trim();
            userNumber = string.IsNullOrEmpty(orderHeader.UserAcctNo) ? "" : orderHeader.UserAcctNo.Trim();
            productName = string.IsNullOrEmpty(orderHeader.ProductName) ? "" : orderHeader.ProductName.Trim();
            rushYOrN = string.IsNullOrEmpty(orderHeader.RushYorN) ? "" : orderHeader.RushYorN.Trim();
            paidRushFee = string.IsNullOrEmpty(orderHeader.PaidRushFee) ? "" : orderHeader.PaidRushFee.Trim();
        }
        /// <summary>
        /// Transfer orders using given user object and department to transfer to as a string.
        /// Department MUST be in the format "D000[0]". First scan refers to whether the order
        /// has ever been scanned into the system for the first time.
        /// Returns 0 on sucessful transfer.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="transToDept"></param>
        /// <param name="firstScan"></param>
        /// <returns></returns>
        public int TransferOrder(User user, string transToDept, bool firstScan = false)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user), "User cannot be null");
            if (transToDept is null)
                throw new ArgumentNullException(nameof(transToDept), "User cannot be null");
            if (!transToDept.StartsWith('D'))
                throw new ArgumentException("Department must begin with a 'D'", nameof(transToDept));

            if (transToDept.EndsWith("080")) { GetLineItemsToScan(parent); }

            if (lineItemsToScan.Any())
            {
                foreach (KeyValuePair<int, string> kvp in lineItems)
                {
                    if (!lineItemsToScan.Contains(kvp.Key)) { continue; }
                    try
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + OrderNumber + "00";
                            BarcodeTransfer(user.EmployeeCode, transToDept, travellerNumber, firstScan);
                        }
                    }
                    catch
                    {
                        return 1;
                    }
                }
            }
            else
            {
                foreach (KeyValuePair<int, string> kvp in lineItems)
                {
                    try
                    {
                        string lineType = kvp.Value.Trim();
                        if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                        {
                            string travellerNumber = "1" + kvp.Key.ToString("00") + OrderNumber + "00";
                            BarcodeTransfer(user.EmployeeCode, transToDept, travellerNumber, firstScan);
                        }
                    }
                    catch
                    {
                        return 1;
                    }
                }
            }
            
            return 0;
        }

        private void BarcodeTransfer(string employeeCode, string departmentCode, string travellerNumber, bool firstScan)
        {
            if (firstScan)
            {
                string lineNumber = travellerNumber.Substring(1, 2);
                string strSQL = "Update [NAT01].[dbo].[OrderDetails] set [TravellerNo] = '" + travellerNumber + "' where [OrderNo] = '" + OrderNumber.ToString() + "00' and [LineNumber] = '" + lineNumber + "'";
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
                    updateCommand.Dispose();
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
        private void LineItemChoices_Closed(object sender, EventArgs e)
        {
            UIElementCollection collection = stackPanelTemp.Children;
            foreach (UIElement element in collection)
            {
                if (element.GetType().FullName.Contains("CheckBox"))
                {
                    CheckBox c = (CheckBox)element;
                    IEnumerable<KeyValuePair<int, string>> lineItem = lineItems.Where(x => c.Name[0..^8] == x.Value);
                    lineItemsToScan.Add(lineItem.ElementAt(0).Key);
                }
            }
        }
        private void GetLineItemsToScan(Window owner)
        {
            Window LineItemChoices = new Window();
            LineItemChoices.Owner = owner;
            LineItemChoices.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            LineItemChoices.Width = 250;
            //LineItemChoices.Height = 200;
            Grid grid = new Grid();
            StackPanel stackPanel = new StackPanel();
            stackPanel.Width = 225;
            stackPanel.Margin = new Thickness(0, 15, 0, 0);
            Label label = new Label();
            label.Content = "Check the items you want to send back.";
            label.Width = 225;
            label.HorizontalContentAlignment = HorizontalAlignment.Center;
            stackPanelTemp = stackPanel;
            stackPanel.Children.Add(label);
            grid.Children.Add(stackPanel);
            LineItemChoices.Content = grid;
            LineItemChoices.SizeToContent = SizeToContent.Height;

            foreach (KeyValuePair<int, string> kvp in lineItems)
            {
                string lineType = kvp.Value.Trim();
                if (lineType != "E" && lineType != "H" && lineType != "MC" && lineType != "RET" && lineType != "T" && lineType != "TM" && lineType != "Z")
                {
                    using var context = new NAT01Context();
                    string lineDesc = context.OedetailType.Where(x => lineType == x.TypeId.Trim()).FirstOrDefault().ShortDesc.Trim();
                    CheckBox checkBox = new CheckBox
                    {
                        Name = lineType + "CheckBox",
                        Content = lineDesc,
                        IsChecked = true
                    };
                    stackPanel.Children.Add(checkBox);
                    context.Dispose();
                }
            }

            LineItemChoices.Closed += LineItemChoices_Closed;
            LineItemChoices.ShowDialog();
        }
    }
}
