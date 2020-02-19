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
    /// Interaction logic for CustomerNoteWindow.xaml
    /// </summary>
    public partial class CustomerNoteWindow : Window
    {
        private User user;
        /// <summary>
        /// Open Existing Note
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="user"></param>
        public CustomerNoteWindow(int ID, User user)
        {
            this.user = user;
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
                    EnteredBy.Text = "Entered by:" + eoiCustomerNote.User;
                    CustomerNumber.Text = eoiCustomerNote.CustomerNumber ?? "";
                    CustomerName.Text = eoiCustomerNote.CustomerName ?? "";
                    ShipToNumber.Text = eoiCustomerNote.ShipToNumber ?? "";
                    ShipToName.Text = eoiCustomerNote.ShipToName ?? "";
                    EndUserNumber.Text = eoiCustomerNote.EndUserNumber ?? "";
                    EndUserName.Text = eoiCustomerNote.EndUserName ?? "";
                    CategoryComboBox.Text = eoiCustomerNote.Category;
                    CommentTextBox.Text = eoiCustomerNote.Note;
                    if (eoiCustomerNote.QuoteNumbers.Length > 0)
                    {
                        string[] quoteNumbers = eoiCustomerNote.QuoteNumbers.Split(',');
                        foreach (string quoteNumber in quoteNumbers)
                        {
                            ListBoxItem listBoxItem = new ListBoxItem { Content = quoteNumber, Style = (Style)Application.Current.Resources["ListBoxItem"] };
                            LinkListBox.Items.Add(listBoxItem);
                        }
                    }
                    if (eoiCustomerNote.OrderNumbers.Length > 0)
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
                CustomerNumber.IsEnabled = false;
                CustomerName.IsEnabled = false;
                ShipToNumber.IsEnabled = false;
                ShipToName.IsEnabled = false;
                EndUserNumber.IsEnabled = false;
                EndUserName.IsEnabled = false;
                CategoryComboBox.IsEnabled = false;
                CommentTextBox.IsEnabled = false;
                OKButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => Existing Note: '" + ID + "'", ex.Message, user);
            }
            _nat02Context.Dispose();
        }
        /// <summary>
        /// Create New Customer Note Window
        /// </summary>
        /// <param name="user"></param>
        /// <param name="quoteNo"></param>
        /// <param name="quoteRevNo"></param>
        public CustomerNoteWindow(User user, int? quoteNo = null, short? quoteRevNo = null)
        {
            this.user = user;
            InitializeComponent();
            try
            {
                if (quoteNo != null && quoteRevNo != null)
                {
                    LinkListBox.Items.Add(quoteNo.ToString() + "-" + quoteRevNo.ToString());
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => New Note => QuoteNumber: '" + quoteNo ?? "null" + "' QuoteRevNumber: '" + quoteRevNo ?? "null" + "'", ex.Message, user);
            }
        }

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

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            using var _nat02Context = new NAT02Context();
            string quoteNumbers = "";
            string orderNumbers = "";
            string userName = "";
            string customerNumber = "";
            string customerName = "";
            string shipToNumber = "";
            string shipToName = "";
            string endUserNumber = "";
            string endUserName = "";
            string category = "";
            string note = "";
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
                userName = user.DomainName;
                customerNumber = CustomerNumber.Text;
                customerName = CustomerName.Text;
                shipToNumber = ShipToNumber.Text;
                shipToName = ShipToName.Text;
                endUserNumber = EndUserNumber.Text;
                endUserName = EndUserName.Text;
                category = ((ComboBoxItem)CategoryComboBox.SelectedItem).Content.ToString();
                note = CommentTextBox.Text;
                notificationDate = NotificationDate.Text.ToString() == "" ? (DateTime?)null : NotificationDate.DisplayDate;
                EoiCustomerNotes customerNote = new EoiCustomerNotes {
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
                _nat02Context.EoiCustomerNotes.Add(customerNote);
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                Close();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CustomerNoteWindow.xaml.cs => OKButton_Click() => User: '" + userName + "' CustomerNumber: '" + customerNumber + "' CustomerName: '" + customerName + "' ShipToNumber: '" + shipToNumber + "' ShipToName: '" + shipToName + "' EndUserNumber: '" + endUserNumber + "' EndUserName: '" + endUserName + "' Category: '" + category + "' Note: '" + note + "' QuoteNumbers: '" + quoteNumbers + "' OrderNumbers: '" + orderNumbers + "' NotificationDate: '" + notificationDate + "'", ex.Message, user);
                MessageBox.Show(ex.Message);
            }
            _nat02Context.Dispose();
        }

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
    }
    
}
