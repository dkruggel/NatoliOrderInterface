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
using System.Windows.Controls;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.NAT01;
using System.Linq;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Window to create or view Customer Notes.
    /// </summary>
    public partial class CustomerNoteWindow : Window
    {
        private User user;
        private int ID = 0;
        /// <summary>
        /// Open existing Customer Note.
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="user"></param>
        public CustomerNoteWindow(int ID, User user)
        {
            this.user = user;
            this.ID = ID;
            InitializeComponent();
            using var _nat02Context = new NAT02Context();
            try
            {
                LinkType.IsEnabled = false;
                LinkDocumentNumber.IsEnabled = false;
                LinkAdd.IsEnabled = false;
                LinkAdd.Cursor = Cursors.Arrow;
                LinkRemove.IsEnabled = false;
                LinkRemove.Cursor = Cursors.Arrow;
                if (_nat02Context.EoiCustomerNotes.Any(cn => cn.ID == ID))
                {
                    EoiCustomerNotes eoiCustomerNote = _nat02Context.EoiCustomerNotes.First(cn => cn.ID == ID);
                    EnteredBy.Text = "Entered by: " + eoiCustomerNote.User;
                    EnteredDate.Text = "Date: " + eoiCustomerNote.Timestamp.ToLocalTime();
                    CustomerNumber.Text = eoiCustomerNote.CustomerNumber ?? "";
                    CustomerName.Text = eoiCustomerNote.CustomerName ?? "";
                    ShipToNumber.Text = eoiCustomerNote.ShipToNumber ?? "";
                    ShipToName.Text = eoiCustomerNote.ShipToName ?? "";
                    EndUserNumber.Text = eoiCustomerNote.EndUserNumber ?? "";
                    EndUserName.Text = eoiCustomerNote.EndUserName ?? "";
                    CategoryComboBox.Text = eoiCustomerNote.Category;
                    CommentTextBox.Text = eoiCustomerNote.Note;
                    if (eoiCustomerNote.QuoteNumbers != null && eoiCustomerNote.QuoteNumbers.Length > 0)
                    {
                        string[] quoteNumbers = eoiCustomerNote.QuoteNumbers.Split(',');
                        foreach (string quoteNumber in quoteNumbers)
                        {
                            ListBoxItem listBoxItem = new ListBoxItem { Content = quoteNumber, Style = (Style)Application.Current.Resources["ListBoxItem"] };
                            LinkListBox.Items.Add(listBoxItem);
                        }
                    }
                    if (eoiCustomerNote.OrderNumbers != null && eoiCustomerNote.OrderNumbers.Length > 0)
                    {
                        string[] orderNumbers = eoiCustomerNote.OrderNumbers.Split(',');
                        foreach (string orderNumber in orderNumbers)
                        {
                            ListBoxItem listBoxItem = new ListBoxItem { Content = orderNumber, Style = (Style)Application.Current.Resources["ListBoxItem"] };
                            LinkListBox.Items.Add(listBoxItem);
                        }
                    }
                    if (eoiCustomerNote.NotificationDate != null)
                    {
                        NotificationDate.SelectedDate = eoiCustomerNote.NotificationDate;
                        NotificationDate.IsEnabled = false;
                    }
                }
                CustomerNumber.IsReadOnly = true;
                CustomerName.IsReadOnly = true;
                ShipToNumber.IsReadOnly = true;
                ShipToName.IsReadOnly = true;
                EndUserNumber.IsReadOnly = true;
                EndUserName.IsReadOnly = true;
                CategoryComboBox.IsEnabled = false;
                CommentTextBox.IsReadOnly = true;
                OKButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => Existing Note: '" + ID + "'", ex.Message, user);
            }
            _nat02Context.Dispose();
        }
        /// <summary>
        /// Create new Customer Note. Prefilled with quote information and prelinked to that quote.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="quoteNo"></param>
        /// <param name="quoteRevNo"></param>
        public CustomerNoteWindow(User user, int quoteNo, short quoteRevNo)
        {
            this.user = user;
            InitializeComponent();
            try
            {
                using var _nat01Context = new NAT01Context();
                if (quoteNo != null && quoteRevNo != null)
                {
                    Quote quote = new Quote(Convert.ToInt32(quoteNo), Convert.ToInt16(quoteRevNo));
                    LinkListBox.Items.Add(quoteNo.ToString() + "-" + quoteRevNo.ToString());
                    CustomerNumber.Text = quote.CustomerNo.Trim();
                    CustomerName.Text = quote.BillToName.Trim();
                    ShipToNumber.Text = quote.ShipToAccountNo.Trim();
                    ShipToName.Text = quote.ShiptoName.Trim();
                    EndUserNumber.Text = quote.UserAcctNo.Trim();
                    quote.Dispose();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => New Note => DocumentNo: '" + quoteNo ?? "null" + "' QuoteRevNumber: '" + quoteRevNo ?? "null" + "'", ex.Message, user);
            }
        }
        /// <summary>
        /// Create New Customer Note. Prefilled with order information and prelinked to that order.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="orderNo"></param>
        public CustomerNoteWindow(User user, int orderNo)
        {
            this.user = user;
            InitializeComponent();
            try
            {
                if (orderNo != null)
                {
                    WorkOrder workOrder = new WorkOrder(Convert.ToInt32(orderNo), this);
                    LinkListBox.Items.Add(orderNo.ToString());
                    CustomerNumber.Text = workOrder.CustomerNumber.Trim();
                    CustomerName.Text = workOrder.SoldToCustomerName.Trim();
                    ShipToNumber.Text = workOrder.AccountNumber.Trim();
                    ShipToName.Text = workOrder.ShipToCustomerName.Trim();
                    EndUserNumber.Text = workOrder.UserNumber.Trim();
                    EndUserNumber.Text = workOrder.EndUserName.Trim();
                }

            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => New Note => DocumentNo: '" + orderNo ?? "null" + "'", ex.Message, user);
            }
        }
        /// <summary>
        /// Create New Customer Note.
        /// </summary>
        /// <param name="user"></param>
        public CustomerNoteWindow(User user)
        {
            this.user = user;
            InitializeComponent();
        }

        /// <summary>
        /// Adds document from the LinkListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkAdd_MouseUp(object sender, MouseButtonEventArgs e)
        {
            using var _nat01Context = new NAT01Context();
            string documentNumber = "";
            bool isQuoteType = false;
            try
            {
                documentNumber = LinkDocumentNumber.Text.Trim();

                switch (((ComboBoxItem)LinkType.SelectedItem).Content.ToString())
                {
                    case "Quote":
                        isQuoteType = true;
                        if (documentNumber.Contains("-"))
                        {
                            string[] quote = documentNumber.Split('-');
                            if (double.TryParse(quote[0], out double quoteNo) && short.TryParse(quote[1], out short quoteRevNo))
                            {
                                if (_nat01Context.QuoteHeader.Any(q => q.QuoteNo == quoteNo && q.QuoteRevNo == quoteRevNo))
                                {
                                    ListBoxItem listBoxItem = new ListBoxItem { Content = documentNumber, Style = (Style)Application.Current.Resources["ListBoxItem"]};
                                    //LinkListBox.Items.Add(documentNumber);
                                    LinkListBox.Items.Add(listBoxItem);
                                    LinkDocumentNumber.Clear();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Could not find an existing quote from " + documentNumber + ".");
                            }
                        }
                        break;
                    case "Order":
                        if (double.TryParse(documentNumber, out double orderNo))
                        {
                            if (_nat01Context.OrderHeader.Any(o => o.OrderNo == Convert.ToDouble(orderNo + "00")))
                            {
                                ListBoxItem listBoxItem = new ListBoxItem { Content = documentNumber, Style = (Style)Application.Current.Resources["ListBoxItem"] };
                                //LinkListBox.Items.Add(documentNumber);
                                LinkListBox.Items.Add(listBoxItem);
                                LinkDocumentNumber.Clear();
                            }
                            else
                            {
                                MessageBox.Show("Could not find an existing order from " + documentNumber + ".");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => LinkAdd_MouseUp() => Document Number: '" + documentNumber + "'", ex.Message, user);
                MessageBox.Show("Error converting " + documentNumber + " to " + (isQuoteType ? "a QuoteNumber-RevNumber." : "an Order Number."));
            }
            _nat01Context.Dispose();
        }
        /// <summary>
        /// Removes document from the LinkListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkRemove_MouseUp(object sender, MouseButtonEventArgs e)
        {
            string selectedItem = "";
            try
            {
                selectedItem = ((ListBoxItem)LinkListBox.SelectedItem).Content.ToString();
                LinkListBox.Items.Remove(LinkListBox.SelectedItem);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => LinkRemove_MouseUp() => Document Number: '" + selectedItem + "'", ex.Message, user);
            }
        }
        /// <summary>
        /// Submits the data to '[EOI_CustomerNotes]'.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            using var _nat02Context = new NAT02Context();
            string quoteNumbers = "";
            string orderNumbers = "";
            string userName = null;
            string customerNumber = null;
            string customerName = null;
            string shipToNumber = null;
            string shipToName = null;
            string endUserNumber = null;
            string endUserName = null;
            string category = null;
            string note = null;
            DateTime? notificationDate = null;
            try
            {
                foreach (ListBoxItem listBoxItem in LinkListBox.Items.OfType<ListBoxItem>())
                {
                    string s = listBoxItem.Content.ToString();
                    if (s.Contains("-"))
                    {
                        quoteNumbers += s + ",";
                    }
                    else
                    {
                        orderNumbers += s + ",";
                    }
                }
                quoteNumbers = quoteNumbers.Trim(',');
                orderNumbers = orderNumbers.Trim(',');
                quoteNumbers = string.IsNullOrEmpty(quoteNumbers) ? null : quoteNumbers;
                orderNumbers = string.IsNullOrEmpty(orderNumbers) ? null : orderNumbers;
                userName = string.IsNullOrEmpty(user.DomainName) ? null : user.DomainName;
                customerNumber = string.IsNullOrEmpty(CustomerNumber.Text) ? null : CustomerNumber.Text;
                customerName = string.IsNullOrEmpty(CustomerName.Text) ? null : CustomerName.Text;
                shipToNumber = string.IsNullOrEmpty(ShipToNumber.Text) ? null : ShipToNumber.Text;
                shipToName = string.IsNullOrEmpty(ShipToName.Text) ? null : ShipToName.Text;
                endUserNumber = string.IsNullOrEmpty(EndUserNumber.Text) ? null : EndUserNumber.Text;
                endUserName = string.IsNullOrEmpty(EndUserName.Text) ? null : EndUserName.Text;
                category = ((ComboBoxItem)CategoryComboBox.SelectedItem).Content.ToString();
                note = CommentTextBox.Text;
                notificationDate = NotificationDate.Text.ToString() == "" ? (DateTime?)null : NotificationDate.SelectedDate;
                EoiCustomerNotes customerNote = new EoiCustomerNotes {
                    Timestamp = DateTime.UtcNow,
                    User = userName,
                    CustomerNumber = customerNumber,
                    CustomerName = customerName,
                    ShipToNumber = shipToNumber,
                    ShipToName = shipToName,
                    EndUserNumber = endUserNumber,
                    EndUserName = endUserName,
                    Category = category,
                    Note = note,
                    QuoteNumbers = quoteNumbers,
                    OrderNumbers = orderNumbers,
                    NotificationDate = notificationDate,
                };
                if (ID > 0)
                {
                    customerNote.ID = ID;
                    _nat02Context.EoiCustomerNotes.Update(customerNote);
                }
                else
                {
                    _nat02Context.EoiCustomerNotes.Add(customerNote);
                }
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                Close();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => OKButton_Click() => User: '" + userName ?? "Null"
                    + "' CustomerNumber: '" + customerNumber ?? "Null"
                    + "' CustomerName: '" + customerName ?? "Null"
                    + "' ShipToNumber: '" + shipToNumber ?? "Null"
                    + "' ShipToName: '" + shipToName ?? "Null"
                    + "' EndUserNumber: '" + endUserNumber ?? "Null"
                    + "' EndUserName: '" + endUserName ?? "Null"
                    + "' Category: '" + category + "' Note: '"
                    + note + "' QuoteNumbers: '" + quoteNumbers ?? "Null"
                    + "' OrderNumbers: '" + orderNumbers ?? "Null"
                    + "' NotificationDate: '" + notificationDate ?? "Null" + "'", ex.Message + " ----Inner Exception: " + ex.InnerException.Message, user);
                MessageBox.Show(ex.Message + "\n" + ex.InnerException.Message);
            }
            _nat02Context.Dispose();
        }
        /// <summary>
        /// Closes the window without submitting form.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => CancelButton_Click()", ex.Message, user);
            }
        }
        /// <summary>
        /// Enables User to update the notification date by enabling the 'OK' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotificationDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EnteredBy.Text.ToString().Length > 0 && user.DomainName == EnteredBy.Text.Split(": ")[1])
            {
                OKButton.IsEnabled = true;
            }
            else if(EnteredBy.Text.ToString().Length > 0)
            {
                OKButton.IsEnabled = false;
            }
        }
    }
    
}
