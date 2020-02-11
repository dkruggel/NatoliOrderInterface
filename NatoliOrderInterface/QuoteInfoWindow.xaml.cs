using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using DK.WshRuntime;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models;
using Outlook = Microsoft.Office.Interop.Outlook;
using DocumentFormat.OpenXml;
using System.Windows.Input;
using NatoliOrderInterface.Models.NEC;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Shapes;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Windows.Interop;

namespace NatoliOrderInterface
{

    //public class SymbolicLink
    //{
    //    [DllImport("Kernel32.dll", CharSet = CharSet.Unicode)]
    //    static extern bool CreateSymbolicLink(
    //        string lpSymlinkFileName,
    //        string lpTargetFileName,
    //        uint dwFlags
    //    );
    //}

    public partial class QuoteInfoWindow : Window, IMethods
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        enum SymbolicLink
        {
            File = 0,
            Directory = 1
        }

        private readonly MainWindow parent;
        private readonly Quote quote;
        private readonly int quoteNumber;
        //private readonly string quoteLocation;
        private List<QuoteLineItem> quoteLineItems = new List<QuoteLineItem>();
        private readonly User user;
        private bool? isChecked = false;
        private bool errorNotificationPopped = false;
        private bool validData;
        private bool isFirstTimeEntering;
        private WorkOrder workOrder;

        public QuoteInfoWindow()
        {
            this.InitializeComponent();
        }

        public QuoteInfoWindow(Quote quote, MainWindow parent, User user)
        {
            //Owner = parent;
            InitializeComponent();
            this.user = user ?? new User("");
            this.quote = quote ?? new Quote(0,0);
            quoteNumber = this.quote.QuoteNumber;
            this.parent = parent ?? new MainWindow();

            if (quote.OrderNo != 0)
            {
                OrderFolderButton1.IsEnabled = true;
                OrderFolderButton2.IsEnabled = true;
                OrderFolderButton3.IsEnabled = true;
                OrderFolderButton4.IsEnabled = true;
                SubmitButton1.IsEnabled = false;
                RecallButton1.IsEnabled = false;
            }
            else
            {
                OrderFolderButton1.IsEnabled = false;
                OrderFolderButton2.IsEnabled = false;
                OrderFolderButton3.IsEnabled = false;
                OrderFolderButton4.IsEnabled = false;

                if (user.EmployeeCode == "E4408" || user.EmployeeCode == "E4754" || user.EmployeeCode == "E4509")
                {
                    using var nat02Context = new NAT02Context();
                    if (nat02Context.EoiQuotesMarkedForConversion.Any(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo))
                    {
                        SubmitButton1.IsEnabled = false;
                        RecallButton1.IsEnabled = true;
                    }
                    else
                    {
                        SubmitButton1.IsEnabled = true;
                        RecallButton1.IsEnabled = false;
                    }
                }
                else if (user.Department == "Customer Service")
                {
                    using var nat02Context = new NAT02Context();
                    if (nat02Context.EoiQuotesMarkedForConversion.Any(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo))
                    {
                        SubmitButton1.IsEnabled = false;
                        RecallButton1.IsEnabled = true;
                    }
                    else
                    {
                        SubmitButton1.IsEnabled = true;
                        RecallButton1.IsEnabled = false;
                    }
                }
                else
                {
                    SubmitButton1.IsEnabled = false;
                    RecallButton1.IsEnabled = false;
                }
            }
            Title = "Quote#: " + quoteNumber + " Rev#: " + quote.QuoteRevNo;

            IntPtr hwnd = new WindowInteropHelper(parent).Handle;
            Rect windowRect = new Rect();
            GetWindowRect(hwnd, ref windowRect);
            Top = windowRect.Top + 8;
            Left = windowRect.Left + 8;

            //if (this.parent.WindowState == WindowState.Maximized)
            //{
            //    WindowState = WindowState.Maximized;
            //}
            //else
            //{
            //    Top = parent.Top;
            //    Left = parent.Left;
            //    Width = parent.Width;
            //    Height = parent.Height;
            //}
            //quoteLocation = quote_location;
            QuoteTopHeaderExpanderHeader.Text = "Quote: " + quote.QuoteNumber + " Rev: " + quote.QuoteRevNo;
            List<QuoteDetails> quoteDetails = quote.Nat01Context.QuoteDetails.Where(l => (int)l.QuoteNo == quote.QuoteNumber && l.Revision == quote.QuoteRevNo).OrderBy(q => q.LineNumber).ToList();
            foreach (QuoteDetails line in quoteDetails)
            {
                try
                {
                    quoteLineItems.Add(new QuoteLineItem(quote, line.LineNumber));
                }
                catch (Exception ex)
                {
                    // var X = 0;
                    // MessageBox.Show(ex.Message);
                    IMethods.WriteToErrorLog("QuoteInfoWindow constructor - Adding quote line items", ex.Message, user);
                }
            }
            Show();
        }

        /// <summary>
        /// Gets the End User Name of the quote
        /// </summary>
        /// <returns></returns>
        private string GetEndUserName()
        {
            using var _nat01Context = new NAT01Context();
            using var _necContext = new NECContext();
            if (_nat01Context.QuoteHeader.Any(x => x.QuoteNo == quote.QuoteNumber && x.QuoteRevNo == quote.QuoteRevNo))
            {
                QuoteHeader quoteHeader = _nat01Context.QuoteHeader.First(x => x.QuoteNo == quote.QuoteNumber && x.QuoteRevNo == quote.QuoteRevNo);
                if (_necContext.Rm00101.Any(c => c.Custnmbr == quoteHeader.UserAcctNo))
                {
                    string user = !string.IsNullOrEmpty(_necContext.Rm00101.First(c => c.Custnmbr == quoteHeader.UserAcctNo).Custname) ? _necContext.Rm00101.First(c => c.Custnmbr == quoteHeader.UserAcctNo).Custname.ToString().Trim() : "";
                    _nat01Context.Dispose();
                    _necContext.Dispose();
                    return user;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                _nat01Context.Dispose();
                _necContext.Dispose();
                return "";
            }
        }

        #region QuoteInfoPage
        private void FillQuoteInfoPage()
        {
            Date.Text = " " + quote.QuoteDate.ToShortDateString();
            QuoteNo.Text = "QUOTE#: " + quoteNumber.ToString();
            RevisionNo.Text = "REV#: " + quote.QuoteRevNo.ToString();
            CustEmail.Text = "EMail: " + quote.Email;
            Attention.Text = "ATTN: " + quote.ContactPerson;
            if (quote.CustomerNo.Length == 0) { UserButton.Visibility = Visibility.Collapsed; } else { UserButton.Visibility = Visibility.Visible; }
            BillToButton.Content = "Bill To: " + quote.CustomerNo;
            if (quote.ShipToAccountNo.Length == 0) { UserButton.Visibility = Visibility.Collapsed; } else { UserButton.Visibility = Visibility.Visible; }
            ShipToButton.Content = "Ship To: " + quote.ShipToAccountNo + " - " + quote.ShipToNo;
            #region Addresses
            List<string> billToAddresses = new List<string> {
                quote.BillToName.ToString(),
                quote.BillToAddr1.ToString(),
                quote.BillToAddr2.ToString(),
                quote.BillToAddr3.ToString(),
                quote.BillToCity.ToString() + ", " + quote.BillToState.ToString() +" "+ quote.BillToZip.ToString(),
                quote.BillToCountry.ToString()
            };
            BillToAddress.Children.Clear();
            foreach (string line in billToAddresses)
            {
                if (line.Length != 0 && line != ",  ")
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = line;
                    textBlock.FontSize = 14;
                    textBlock.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    BillToAddress.Children.Add(textBlock);
                }
            }

            List<string> shipToAddresses = new List<string> {
                quote.ShiptoName.ToString(),
                quote.ShiptoAddr1.ToString(),
                quote.ShiptoAddr2.ToString(),
                quote.ShiptoAddr3.ToString(),
                quote.ShiptoCity.ToString() + ", " + quote.ShiptoState.ToString() +" "+ quote.ShiptoState.ToString(),
                quote.ShiptoCountry.ToString()
            };
            ShipToAddress.Children.Clear();
            foreach (string line in shipToAddresses)
            {
                if (line.Length != 0 && line != ",  ")
                {
                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = line;
                    textBlock.FontSize = 14;
                    textBlock.FontFamily = new System.Windows.Media.FontFamily("Arial");
                    ShipToAddress.Children.Add(textBlock);
                }
            }
            #endregion
            if (GetEndUserName().Length == 0) { UserButton.Visibility = Visibility.Collapsed; } else { UserButton.Visibility = Visibility.Visible; }
            UserButton.Content = GetEndUserName();
            ProductName.Text = quote.ProductName;
            Reference.Text = quote.Reference;
            PaymentTerms.Text = quote.TermsID;
            MfgTime.Text = quote.Shipment + " - AFTER RECEIPT OF P.O. AND APPROVED DRAWINGS (IF REQUIRED).";
            ShipMethod.Text = quote.ShippedVia;
            Incoterms.Text = quote.Incoterms;
            DutiesTaxesFees.Text = quote.DutiesTaxesBilling;
            foreach (QuoteLineItem quoteLineItem in quoteLineItems)
            {
                if (quoteLineItem.MachineNo != 0 && quoteLineItem.MachineNo != null)
                {
                    MachineDescription.Text = string.IsNullOrEmpty(quoteLineItem.MachineDescription) ? "" : quoteLineItem.MachineDescription.Trim() + " // (" + quoteLineItem.MachineNo + ")";
                    break;
                }
            }
            QtyStackPanel.Children.Clear();
            ItemDescriptionStackPanel.Children.Clear();
            ItemDescription2StackPanel.Children.Clear();
            UnitValueStackPanel.Children.Clear();
            ExtendedValueStackPanel.Children.Clear();
            EngineeringNote1.Text = quote.EngineeringNote1;
            EngineeringNote2.Text = quote.EngineeringNote2;
            TM2DATA.Text = quote.TM2Data ? "*TM - II DATA*" : "";
            MiscNote.Text = quote.MiscNote;
            EtchingNote.Text = quote.EtchingNote;
            ShippingNote.Text = quote.ShippingNote;
            InspectionNote.Text = quote.InspectionNote;
            DrawingSetNo.Text = quote.DrawingSetNo;
            ProjectNo.Text = quote.ProjectNo.ToString();
            RefWONo.Text = quote.RefWO.ToString();
            Subtotal.Text = quote.QuoteSubtotal == 0 ? "$0.00" : "$" + String.Format("{0:#,###.00}", quote.QuoteSubtotal);
            Freight.Text = quote.QuoteFreightCharge == 0 ? "$0.00" : "$" + String.Format("{0:#,###.00}", quote.QuoteFreightCharge);
            Markdown.Text = quote.QuoteMarkdown == 0 ? "" : "$" + String.Format("{0:#,###.00}", quote.QuoteMarkdown);
            MarkdownCallout.Text = quote.QuoteMarkdown == 0 ? "" : "DISCOUNT:";
            try
            {
                AddedToInvoice.Text = quote.Nat01Context.QuoteFreightDesc.Where(q => q.FreightId == quote.FreightDescID.ToString()).FirstOrDefault().Description.Trim();
            }
            catch
            {
                MessageBox.Show("Error getting Freight Description from FreightID (QuoteFreightDesc)", "Error", MessageBoxButton.OK);
            }
            Total.Text = quote.QuoteTotal == 0 ? "$0.00" : "$" + String.Format("{0:#,###.00}", quote.QuoteTotal);
            ReferToQuote.Text = "* * * PLEASE REFER TO THIS QUOTE #: " + quote.QuoteNumber + ", REV. " + quote.QuoteRevNo + " ON YOUR PURCHASE ORDER * * *";
            QuoteRepresentative representativeRow = quote.Nat01Context.QuoteRepresentative.Where(q => q.RepId.Trim() == quote.QuoteRepID.ToString().Trim()).FirstOrDefault();
            try
            {
                BitmapImage image = new BitmapImage(new Uri(representativeRow.SignatureFile.ToString().Trim(), UriKind.Absolute));
                Signature.Source = image;
                SignaturePlainText.Text = representativeRow.Name.ToString().Trim() + " / NATOLI ENGINEERING";
            }
            catch
            {
                MessageBox.Show("\"Signature File\" path in \"QuoteRepresentative\" database is not valid.", "Signature.jpg", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            foreach (QuoteLineItem lineItem in quoteLineItems)
            {
                if (-1 != lineItem.LineItemNumber)
                {
                    TextBlock qty = new TextBlock();
                    qty.Text = lineItem.QtyOrdered.ToString();
                    qty.HorizontalAlignment = HorizontalAlignment.Center;
                    qty.Height = 16;
                    QtyStackPanel.Children.Add(qty);

                    TextBlock unitPrice = new TextBlock();
                    unitPrice.Text = lineItem.UnitPrice == 0 ? "$0.00" : "$" + String.Format("{0:#,###.00}", lineItem.UnitPrice);
                    unitPrice.HorizontalAlignment = HorizontalAlignment.Center;
                    unitPrice.Height = 16;
                    UnitValueStackPanel.Children.Add(unitPrice);

                    TextBlock extendedPrice = new TextBlock();
                    extendedPrice.Text = lineItem.ExtendedPrice == 0 ? "$0.00" : "$" + String.Format("{0:#,###.00}", lineItem.ExtendedPrice);
                    extendedPrice.HorizontalAlignment = HorizontalAlignment.Center;
                    extendedPrice.Height = 16;
                    ExtendedValueStackPanel.Children.Add(extendedPrice);

                    TextBlock desc1 = new TextBlock();
                    desc1.Text = lineItem.Desc1;
                    desc1.Height = 16;
                    TextBlock desc2 = new TextBlock();
                    desc2.Text = lineItem.Desc2;
                    desc2.Height = 16;
                    TextBlock desc3 = new TextBlock();
                    desc3.Text = lineItem.Desc3;
                    desc3.Height = 16;
                    TextBlock desc4 = new TextBlock();
                    desc4.Text = lineItem.Desc4;
                    desc4.Height = 16;
                    TextBlock desc5 = new TextBlock();
                    desc5.Text = lineItem.Desc5;
                    desc5.Height = 16;
                    TextBlock desc6 = new TextBlock();
                    desc6.Text = lineItem.Desc6;
                    desc6.Height = 16;
                    TextBlock desc7 = new TextBlock();
                    desc7.Text = lineItem.Desc7;
                    desc7.Height = 16;

                    ItemDescriptionStackPanel.Children.Add(desc1);
                    ItemDescriptionStackPanel.Children.Add(desc3);
                    ItemDescriptionStackPanel.Children.Add(desc5);
                    ItemDescriptionStackPanel.Children.Add(desc7);
                    ItemDescription2StackPanel.Children.Add(desc2);
                    ItemDescription2StackPanel.Children.Add(desc4);
                    ItemDescription2StackPanel.Children.Add(desc6);

                    for (short j = 0; j < lineItem.Options.Count; j++)
                    {
                        string[] printables = lineItem.Options[j];
                        if (printables[0].Trim().Length != 0)
                        {
                            TextBlock option = new TextBlock();
                            option.Text = String.Concat(printables);
                            option.Height = 16;
                            option.Margin = new Thickness(20, 0, 0, 0);
                            option.ToolTip = lineItem.OptionNumbers[j];
                            ItemDescriptionStackPanel.Children.Add(option);
                        }
                    }
                    TextBlock blank = new TextBlock();
                    blank.Height = 16;
                    blank.Text = "";
                    ItemDescriptionStackPanel.Children.Add(blank);
                    AddBlankTextBlocks(QtyStackPanel, ItemDescriptionStackPanel);
                    AddBlankTextBlocks(UnitValueStackPanel, ItemDescriptionStackPanel);
                    AddBlankTextBlocks(ExtendedValueStackPanel, ItemDescriptionStackPanel);
                    AddBlankTextBlocks(ItemDescription2StackPanel, ItemDescriptionStackPanel);
                }
            }
            UEtching1.Text = quote.UEtching1;
            UEtching2.Text = quote.UEtching2;
            UEtching3.Text = quote.UEtching3;
            UEtching4.Text = quote.UEtching4;
            UEtching5.Text = quote.UEtching5;
            UEtching6.Text = quote.UEtching6;
            UEtching7.Text = quote.UEtching7;

            LEtching1.Text = quote.LEtching1;
            LEtching2.Text = quote.LEtching2;
            LEtching3.Text = quote.LEtching3;
            LEtching4.Text = quote.LEtching4;
            LEtching5.Text = quote.LEtching5;
            LEtching6.Text = quote.LEtching6;
            LEtching7.Text = quote.LEtching7;

            DEtching1.Text = quote.DEtching1;
            DEtching2.Text = quote.DEtching2;
            DEtching3.Text = quote.DEtching3;
            DEtching4.Text = quote.DEtching4;
            DEtching5.Text = quote.DEtching5;
            DEtching6.Text = quote.DEtching6;
            DEtching7.Text = quote.DEtching7;

            REtching1.Text = quote.REtching1;
            REtching2.Text = quote.REtching2;
            REtching3.Text = quote.REtching3;
            REtching4.Text = quote.REtching4;
            REtching5.Text = quote.REtching5;
            REtching6.Text = quote.REtching6;
            REtching7.Text = quote.REtching7;

            AEtching1.Text = quote.AEtching1;
            AEtching2.Text = quote.AEtching2;
            AEtching3.Text = quote.AEtching3;
            AEtching4.Text = quote.AEtching4;
            AEtching5.Text = quote.AEtching5;
            AEtching6.Text = quote.AEtching6;
            //AEtching7.Text = quote.AEtching7;   --- Alingment Etching is all on one side of the tool, quotes print all alignment etching in one chunk on the front --
            //AEtching1B.Text = quote.AEtching1B;
            //AEtching2B.Text = quote.AEtching2B;
            //AEtching3B.Text = quote.AEtching3B;
            //AEtching4B.Text = quote.AEtching4B;
            //AEtching5B.Text = quote.AEtching5B;
            //AEtching6B.Text = quote.AEtching6B;
            //AEtching7B.Text = quote.AEtching7B;
            AEtching7.Text = quote.AEtching7B;
            AEtching8.Text = quote.AEtching8;
            AEtching9.Text = quote.AEtching9;
            AEtching10.Text = quote.AEtching10;

            UEtching1B.Text = quote.UEtching1B;
            UEtching2B.Text = quote.UEtching2B;
            UEtching3B.Text = quote.UEtching3B;
            UEtching4B.Text = quote.UEtching4B;
            UEtching5B.Text = quote.UEtching5B;
            UEtching6B.Text = quote.UEtching6B;
            UEtching7B.Text = quote.UEtching7B;

            LEtching1B.Text = quote.LEtching1B;
            LEtching2B.Text = quote.LEtching2B;
            LEtching3B.Text = quote.LEtching3B;
            LEtching4B.Text = quote.LEtching4B;
            LEtching5B.Text = quote.LEtching5B;
            LEtching6B.Text = quote.LEtching6B;
            LEtching7B.Text = quote.LEtching7B;

            DEtching1B.Text = quote.DEtching1B;
            DEtching2B.Text = quote.DEtching2B;
            DEtching3B.Text = quote.DEtching3B;
            DEtching4B.Text = quote.DEtching4B;
            DEtching5B.Text = quote.DEtching5B;
            DEtching6B.Text = quote.DEtching6B;
            DEtching7B.Text = quote.DEtching7B;

            REtching1B.Text = quote.REtching1B;
            REtching2B.Text = quote.REtching2B;
            REtching3B.Text = quote.REtching3B;
            REtching4B.Text = quote.REtching4B;
            REtching5B.Text = quote.REtching5B;
            REtching6B.Text = quote.REtching6B;
            REtching7B.Text = quote.REtching7B;
        }
        private void ChangeLineItemScrollerHeight()
        {
            QuoteTabItem.UpdateLayout();
            try
            {
                double expanderHeight = QuoteTopHeaderExpander.IsExpanded ? 473 : 23;
                if (LineItemScroller != null)
                {
                    LineItemScroller.Height = Math.Max((Quote_Info_Window == null ? Quote_Info_Window.Height : Quote_Info_Window.ActualHeight) - expanderHeight - (Quote_Info_Window == null ? ButtonBorder1.Height : ButtonBorder1.ActualHeight) - (Quote_Info_Window == null ? QuoteTabItem.Height : QuoteTabItem.ActualHeight) - 60, 30);
                }

            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => ChangeLineItemScrollerHeight; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        static void AddBlankTextBlocks(StackPanel stackPanel, StackPanel bigStackPanel)
        {
            for (short k = (short)stackPanel.Children.Count; k < bigStackPanel.Children.Count; k++)
            {
                TextBlock blank = new TextBlock();
                blank.Height = 16;
                blank.Text = "";
                stackPanel.Children.Add(blank);
            }
        }

        #region Events
        private void Quote_Info_Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowState = parent.WindowState;
        }
        private async void Quote_Info_Window_ContentRendered_Async(object sender, EventArgs e)
        {
            try
            {
                await Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        FillSMIAndScratchPadPage();
                        ChangeSMIScrollHeights();
                        SMITabItem.Header = "Price/SMI Check";
                        SMITabItem.IsEnabled = true;
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }, System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ConfigureAwait(false);
                await Task.Factory.StartNew(() =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        ResetInstructionEntering();
                        FillOrderEntryInstructions();
                        OrderEntryTabItem.Header = "Order Entry Instructions";
                        OrderEntryTabItem.IsEnabled = true;
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }, System.Threading.CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default).ConfigureAwait(false);

                List<string> errors = await Task<List<string>>.Factory.StartNew(() => IMethods.QuoteErrors(quoteNumber.ToString(), quote.QuoteRevNo.ToString(), user), System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
                if (errors.Count > 0)
                {
                    Dispatcher.Invoke(() =>
                    {
                        QuoteErrorsTabItem.Header = "Errors";
                        QuoteErrorsTabItem.IsEnabled = true;
                        ErrorsDockPanel.Children.Clear();
                        foreach (string error in errors)
                        {
                            BulletDecorator bulletDecorator = new BulletDecorator { HorizontalAlignment = HorizontalAlignment.Left };
                            bulletDecorator.SetValue(DockPanel.DockProperty, Dock.Top);
                            Ellipse ellipse = new Ellipse { Width = 8, Height = 8, Margin = new Thickness(0, 4, 0, 4), Fill = (Brush)Application.Current.Resources["ForeGround.AccentBrush"] };
                            TextBlock textBlock = new TextBlock { Text = error, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(8, 2, 8, 2), FontSize = 20, Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                            bulletDecorator.Bullet = ellipse;
                            bulletDecorator.Child = textBlock;
                            ErrorsDockPanel.Children.Add(bulletDecorator);
                        }
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);

                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        QuoteErrorsTabItem.Header = "No Errors Found";
                        QuoteErrorsTabItem.IsEnabled = false;
                    }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }

                bool first = true;
                int percent = 0;
                int j = 1;
                if (quote.lineItems.Keys.Any(k => quote.lineItems[k] != "Z" && quote.lineItems[k] != "E" && quote.lineItems[k] != "H" && quote.lineItems[k] != "K" && quote.lineItems[k] != "MC" && quote.lineItems[k] != "TM"))
                {
                    foreach (int i in quote.lineItems.Keys.Where(k => quote.lineItems[k] != "Z" && quote.lineItems[k] != "E" && quote.lineItems[k] != "H" && quote.lineItems[k] != "K" && quote.lineItems[k] != "MC" && quote.lineItems[k] != "TM"))
                    {
                        percent = (j * 100) / quote.lineItems.Keys.Where(k => quote.lineItems[k] != "Z" && quote.lineItems[k] != "E" && quote.lineItems[k] != "H" && quote.lineItems[k] != "K" && quote.lineItems[k] != "MC" && quote.lineItems[k] != "TM").Count();
                        j++;
                        //(string LineItemDescription, List<string> Suggestions) firstOptionRecommendations = await Task<(string, List<string>)>.Factory.StartNew(() => IMethods.QuoteLineItemOptionSuggestions(quoteNumber.ToString(), quote.QuoteRevNo.ToString(), quote.lineItems[i], user), System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
                        (string LineItemDescription, List<string> Suggestions) firstOptionRecommendations = await Task<(string, List<string>)>.Factory.StartNew(() => IMethods.GetLineItemSuggestionsFromUserAndMachine(quote, new QuoteLineItem(quote, (short)i), user), System.Threading.CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).ConfigureAwait(false);
                        Dispatcher.Invoke(() =>
                        {
                            if (first)
                            {
                                QuoteOptionSuggestions.IsEnabled = true;
                                first = !first;
                            }
                            QuoteOptionSuggestions.Header = "Suggested Options - " + percent + "%";
                            DockPanel dockPanel = new DockPanel();
                            dockPanel.SetValue(DockPanel.DockProperty, Dock.Top);
                            TextBlock headerTextBlock = new TextBlock
                            {
                                Text = firstOptionRecommendations.LineItemDescription,
                                Style = (Style)Application.Current.Resources["BoldTextBlock"],
                                HorizontalAlignment = HorizontalAlignment.Left,
                                FontSize = 28,
                                Margin = new Thickness(40, 0, 0, 0)
                            };
                            headerTextBlock.SetValue(DockPanel.DockProperty, Dock.Top);
                            dockPanel.Children.Add(headerTextBlock);
                            foreach (string suggestion in firstOptionRecommendations.Suggestions)
                            {

                                BulletDecorator bulletDecorator = new BulletDecorator { HorizontalAlignment = HorizontalAlignment.Left };
                                if (suggestion == firstOptionRecommendations.Suggestions.Last())
                                {
                                    bulletDecorator.Margin = new Thickness(0, 0, 0, 20);
                                }
                                bulletDecorator.SetValue(DockPanel.DockProperty, Dock.Top);
                                Ellipse ellipse = new Ellipse { Width = 8, Height = 8, Margin = new Thickness(0, 4, 0, 4), Fill = (Brush)Application.Current.Resources["ForeGround.AccentBrush"] };
                                TextBlock textBlock = new TextBlock { Text = suggestion, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(8, 2, 8, 2), FontSize = 20, Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                                bulletDecorator.Bullet = ellipse;
                                bulletDecorator.Child = textBlock;
                                dockPanel.Children.Add(bulletDecorator);
                            }
                            OptionSuggestionsDockPanel.Children.Add(dockPanel);
                        }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                    }
                    Dispatcher.Invoke(() => QuoteOptionSuggestions.Header = "Suggested Options", System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }
            }
                
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfowWindow.cs => Quote_Info_Window_ContentRendered; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void QuoteTopHeaderExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ChangeLineItemScrollerHeight();
            QuoteTopHeaderExpander.Background = Brushes.Silver;
        }
        private void QuoteTopHeaderExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ChangeLineItemScrollerHeight();
            QuoteTopHeaderExpander.Background = Brushes.White;
        }
        private void Signature_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            IMethods.WriteToErrorLog("QuoteInfoWindow.cs => Signature_ImageFailed; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, e.ErrorException.Message, user);
        }
        #endregion
        #endregion
        
        #region SMI/Scratchpad
        private void FillSMIAndScratchPadPage()
        {
            using NAT01Context _nat01Context = new NAT01Context();
            using NAT02Context _nat02Context = new NAT02Context();
            using NATBCContext _natbcContext = new NATBCContext();
            bool isConvertedToOrder = quote.ConvertedToOrder == 'Y';
            #region SMIs
            Dictionary<short, IQueryable<CustomerInstructionTable>> smiDict = new Dictionary<short, IQueryable<CustomerInstructionTable>>();
            Dictionary<short, string> custNamesDict = new Dictionary<short, string>();
            IQueryable<CustomerInstructionTable> userSMI = _nat01Context.CustomerInstructionTable.Where(q => q.CustomerId == quote.UserAcctNo && q.Inactive == false).OrderBy(q => q.Sequence);
            string userName = GetEndUserName();
            smiDict.Add(0, userSMI);
            custNamesDict.Add(0, userName);
            string shipToName = null;
            IQueryable<CustomerInstructionTable> shipToSMI = null;
            if (quote.UserAcctNo != quote.ShipToAccountNo)
            {
                shipToName = quote.ShiptoName;
                shipToSMI = _nat01Context.CustomerInstructionTable.Where(q => q.CustomerId == quote.ShipToAccountNo && q.Inactive == false).OrderBy(q => q.Sequence);
                smiDict.Add(1, shipToSMI);
                custNamesDict.Add(1, shipToName);
            }
            IQueryable<CustomerInstructionTable> customerSMI = null;
            string customerName = null;
            if (quote.UserAcctNo != quote.CustomerNo && quote.ShipToAccountNo != quote.CustomerNo)
            {
                customerName = quote.BillToName;
                customerSMI = _nat01Context.CustomerInstructionTable.Where(q => q.CustomerId == quote.CustomerNo && q.Inactive == false).OrderBy(q => q.Sequence);
                smiDict.Add(2, customerSMI);
                custNamesDict.Add(2, customerName);
            }

            SMIStackPanel.Children.Clear();


            foreach(KeyValuePair<short,string> custNameKVP in custNamesDict)
            {
                short i = custNameKVP.Key;
                try
                {
                    if (smiDict.ContainsKey(i) && smiDict[i] != null && smiDict[i].Any())
                    {
                        bool? applies = null;
                        Grid grid = new Grid();
                        grid.Tag = "SMIHeaderLine";
                        grid.Margin = new Thickness(0, 5, 0, 0);
                        TextBlock header = new TextBlock();
                        header.Text = custNamesDict[i] + " SMIs";
                        header.Tag = "SMIHeader";
                        header.Name = "Cust_" + smiDict[i].FirstOrDefault().CustomerId.Trim().Replace("-", "_");
                        header.FontSize = 16;
                        header.FontWeight = FontWeights.Bold;
                        header.HorizontalAlignment = HorizontalAlignment.Center;
                        header.VerticalAlignment = VerticalAlignment.Bottom;

                        TextBlock applied = new TextBlock();
                        applied.Text = "Y";
                        applied.HorizontalAlignment = HorizontalAlignment.Left;
                        applied.Margin = new Thickness(3, 0, 0, 0);
                        applied.VerticalAlignment = VerticalAlignment.Bottom;

                        TextBlock notApplied = new TextBlock();
                        notApplied.Text = "N";
                        notApplied.HorizontalAlignment = HorizontalAlignment.Left;
                        notApplied.Margin = new Thickness(23, 0, 0, 0);
                        notApplied.VerticalAlignment = VerticalAlignment.Bottom;

                        grid.Children.Add(header);
                        grid.Children.Add(applied);
                        grid.Children.Add(notApplied);
                        Border headerBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 0, 1) };
                        headerBorder.Child = grid;

                        SMIStackPanel.Children.Add(headerBorder);
                        short j = 1;
                        foreach (CustomerInstructionTable row in smiDict[i])
                        {
                            bool exists = _nat02Context.EoiQuoteSMICheck.Any(i => i.QuoteNo == quote.QuoteNumber && i.RevNo == quote.QuoteRevNo && i.CustomerID.Trim() == row.CustomerId.Trim() && i.Sequence == row.Sequence);
                            Grid grid2 = new Grid();
                            ColumnDefinition checkBoxColumn0 = new ColumnDefinition { Width = new GridLength(20,GridUnitType.Pixel) };
                            ColumnDefinition checkBoxColumn1 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };
                            ColumnDefinition instructionColumn2 = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
                            ColumnDefinition categoryColumn3 = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) };
                            ColumnDefinition userStampColumn4 = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) };
                            ColumnDefinition dateStampColumn5 = new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) };
                            grid2.ColumnDefinitions.Add(checkBoxColumn0);
                            grid2.ColumnDefinitions.Add(checkBoxColumn1);
                            grid2.ColumnDefinitions.Add(instructionColumn2);
                            grid2.ColumnDefinitions.Add(categoryColumn3);
                            grid2.ColumnDefinitions.Add(userStampColumn4);
                            grid2.ColumnDefinitions.Add(dateStampColumn5);
                            grid2.Tag = "SMILine";
                            grid2.Name = "SMILine_" + row.Sequence + "_" + row.CustomerId.Trim().Replace("-", "_") + "_" + i.ToString() + j.ToString();
                            if (row.Instruction.ToString().Take(5).Contains('.') && int.TryParse(row.Instruction.ToString().Trim().First().ToString(), out int temp))
                            {
                                applies = exists ? _nat02Context.EoiQuoteSMICheck.Where(i => i.QuoteNo == quote.QuoteNumber && i.RevNo == quote.QuoteRevNo && i.CustomerID.Trim() == row.CustomerId.Trim() && i.Sequence == row.Sequence).First().AppliesToQuote : null;
                                RadioButton radioButton = new RadioButton();
                                radioButton.HorizontalAlignment = HorizontalAlignment.Center;
                                radioButton.SetValue(Grid.ColumnProperty, 0);
                                radioButton.GroupName = "RadioButton" + i.ToString() + j.ToString();
                                radioButton.Tag = "Y";
                                if (exists && applies  == true)
                                {
                                    radioButton.IsChecked = true;
                                }
                                radioButton.IsEnabled = !isConvertedToOrder;

                                radioButton.PreviewMouseDown += SMIIsRadioChecked;
                                radioButton.Click += SMIUncheckRadioButtons;

                                RadioButton radioButton1 = new RadioButton();
                                radioButton1.HorizontalAlignment = HorizontalAlignment.Center;
                                radioButton1.SetValue(Grid.ColumnProperty, 1);
                                radioButton1.GroupName = "RadioButton" + i.ToString() + j.ToString();
                                radioButton1.Tag = "N";
                                if (exists && applies == false)
                                {
                                    radioButton1.IsChecked = true;
                                }
                                radioButton1.IsEnabled = !isConvertedToOrder;

                                radioButton1.PreviewMouseDown += SMIIsRadioChecked;
                                radioButton1.Click += SMIUncheckRadioButtons;
                                grid2.Children.Add(radioButton);
                                grid2.Children.Add(radioButton1);
                            }

                            TextBlock textBlock = new TextBlock();
                            textBlock.Text = row.Instruction.ToString().TrimEnd();
                            textBlock.Tag = "SMIText";
                            textBlock.TextWrapping = TextWrapping.Wrap;
                            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
                            textBlock.Margin = new Thickness(5, 0, 0, 0);
                            if (exists)
                            {
                                switch (applies) 
                                {
                                    case true:
                                        textBlock.Background = Brushes.Yellow;
                                        break;
                                    case false:
                                        textBlock.TextDecorations = TextDecorations.Strikethrough;
                                        break;
                                    case null:
                                        break;
                                }
                            }
                            Border border = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1,0,0,0) };
                            border.Child = textBlock;
                            border.Tag = "InstructionBorder";
                            border.SetValue(Grid.ColumnProperty, 2);
                            grid2.Children.Add(border);

                            TextBlock categoryTextBlock = new TextBlock
                            {
                                Text = row.Category.ToString(),
                                Tag = "SMICategory",
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(5, 0, 0, 0)
                            };
                            
                            Border border1 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1, 0, 0, 0) };
                            border1.Child = categoryTextBlock;
                            border1.SetValue(Grid.ColumnProperty, 3);
                            grid2.Children.Add(border1);

                            TextBlock userTextBlock = new TextBlock {
                                Text = row.UserStamp.ToString(),
                                Tag = "SMIUserStamp",
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(5, 0, 0, 0)
                            };
                            Border border2 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1, 0, 0, 0) };
                            border2.SetValue(Grid.ColumnProperty, 4);
                            border2.Child = userTextBlock;
                            grid2.Children.Add(border2);

                            TextBlock dateTextBlock = new TextBlock
                            {
                                Text = row.DateStamp.Date.ToString("MM/dd/yyyy"),
                                Tag = "SMIDateStamp",
                                HorizontalAlignment = HorizontalAlignment.Left,
                                Margin = new Thickness(5, 0, 0, 0)
                            };
                            Border border3 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1, 0, 0, 0) };
                            border3.SetValue(Grid.ColumnProperty, 5);
                            border3.Child = dateTextBlock;
                            grid2.Children.Add(border3);

                            SMIStackPanel.Children.Add(grid2);
                            j++;
                        }
                        Border footerBorder = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 0, 0, 1) };
                        SMIStackPanel.Children.Add(footerBorder);
                    }
                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("QuoteInfoWindow => FillSMIAndScratchPadPage; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                }
            }
            Button saveSMIButton = new Button
            {
                Content = "Save SMI Check",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 20),
                MinHeight = 24,
                Width = 130,
                Name = "SaveSMICheck",
                IsEnabled = !isConvertedToOrder,
                Style =  FindResource("Button") as Style
            };
            saveSMIButton.Click += SaveSMIButton_Click;
            SMIStackPanel.Children.Add(saveSMIButton);
            #endregion
            #region ScratchPad
            if (ScratchPadTabs.Items.Count == 0)
            {
                foreach (var lineItem in quote.lineItems)
                {
                    QuoteLineItem quoteLineItem = quoteLineItems.Where(li => li.LineItemNumber == (short)lineItem.Key).FirstOrDefault();
                    QuoteDetails quoteDetails = _nat01Context.QuoteDetails.Where(q => q.QuoteNo == quote.QuoteNumber && q.Revision == quote.QuoteRevNo && q.LineNumber == lineItem.Key).First();
                    //--------------------------------------------------------------------------------------------
                    bool exists = _nat02Context.EoiQuoteScratchPad.Any(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == Convert.ToByte(quote.QuoteRevNo) && q.LineNo == lineItem.Key && q.LineType == lineItem.Value);
                    //--------------------------------------------------------------------------------------------
                    TabItem tab = new TabItem();
                    tab.Name = lineItem.Value.ToString().ToUpper() + "_" + lineItem.Key.ToString();
                    tab.Header = quoteLineItem.Title;
                    tab.Style = FindResource("NormalTabItem") as Style;
                    //--------------------------------------------------------------------------------------------
                    ScrollViewer scrollViewer = new ScrollViewer();
                    scrollViewer.Name = "ScrollViewer" + lineItem.Value.ToString().ToUpper() + lineItem.Key.ToString();
                    scrollViewer.IsEnabled = true;
                    scrollViewer.CanContentScroll = true;
                    //--------------------------------------------------------------------------------------------
                    Grid grid = new Grid();
                    grid.Name = "Grid" + lineItem.Value.ToString().ToUpper() + lineItem.Key.ToString();
                    ColumnDefinition column1_ = new ColumnDefinition();
                    //column1_500.Width = new GridLength(500, GridUnitType.Pixel);
                    ColumnDefinition column2_300 = new ColumnDefinition();
                    column2_300.Width = new GridLength(250, GridUnitType.Pixel);
                    RowDefinition row2_ = new RowDefinition();
                    RowDefinition row4_ = new RowDefinition();
                    RowDefinition row5_ = new RowDefinition();
                    RowDefinition row6_ = new RowDefinition();
                    RowDefinition row7_ = new RowDefinition();
                    RowDefinition row3_30 = new RowDefinition();
                    row3_30.Height = new GridLength(30, GridUnitType.Pixel);
                    RowDefinition row1_30 = new RowDefinition();
                    row1_30.Height = new GridLength(30, GridUnitType.Pixel);
                    grid.ColumnDefinitions.Add(column1_);
                    grid.ColumnDefinitions.Add(column2_300);
                    grid.RowDefinitions.Add(row1_30);
                    grid.RowDefinitions.Add(row2_);
                    grid.RowDefinitions.Add(row3_30);
                    grid.RowDefinitions.Add(row4_);
                    grid.RowDefinitions.Add(row5_);
                    grid.RowDefinitions.Add(row6_);
                    grid.RowDefinitions.Add(row7_);
                    //--------------------------------------------------------------------------------------------
                    Grid basePriceGrid = new Grid();
                    basePriceGrid.Tag = "BasePriceGrid";
                    ColumnDefinition columnBase10 = new ColumnDefinition();
                    columnBase10.Width = new GridLength(10, GridUnitType.Pixel);
                    ColumnDefinition columnBase60 = new ColumnDefinition();
                    columnBase60.Width = new GridLength(60, GridUnitType.Pixel);
                    ColumnDefinition columnBase45 = new ColumnDefinition();
                    columnBase45.Width = new GridLength(45, GridUnitType.Pixel);
                    ColumnDefinition columnBase_ = new ColumnDefinition();
                    basePriceGrid.ColumnDefinitions.Add(columnBase10);
                    basePriceGrid.ColumnDefinitions.Add(columnBase60);
                    basePriceGrid.ColumnDefinitions.Add(columnBase45);
                    basePriceGrid.ColumnDefinitions.Add(columnBase_);

                    TextBlock dollarSignBase = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    dollarSignBase.Text = "$";
                    dollarSignBase.SetValue(Grid.ColumnProperty, 0);
                    dollarSignBase.SetValue(Grid.ColumnSpanProperty, 1);

                    bool priceChanged = false;
                    TextBox priceBase = new TextBox();
                    priceBase.TabIndex = 0;
                    priceBase.GotFocus += Price_GotFocus;
                    priceBase.GotMouseCapture += Price_GotMouseCapture;
                    priceBase.IsMouseCaptureWithinChanged += Price_IsMouseCaptureWithinChanged;
                    priceBase.IsEnabled = !isConvertedToOrder;
                    double basePrice = 0;
                    try
                    {
                        string category = quote.InternationalYorN == 'Y' ? "I" : "D";
                        string repName = _nat01Context.QuoteRepresentative.Where(qr => qr.RepId == quote.QuoteRepID).First().Name;
                        bool international = _natbcContext.MoeEmployees.Where(e => e.MoeEmployeeName == repName).First().MoeDepartmentCode == "D1149";
                        if (exists)
                        {
                            if (_nat02Context.EoiQuoteScratchPad.Any(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == Convert.ToByte(quote.QuoteRevNo) && q.LineNo == lineItem.Key && q.LineType == lineItem.Value))
                            {
                                basePrice = (double)_nat02Context.EoiQuoteScratchPad.First(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == Convert.ToByte(quote.QuoteRevNo) && q.LineNo == lineItem.Key && q.LineType == lineItem.Value).BasePrice;
                                if (_nat02Context.EoiBasePriceList.Any(x => x.Category == category &&
                                                                                 x.MachineType == quoteLineItem.MachinePriceCode &&
                                                                                 x.SteelPriceCode == quoteLineItem.SteelPriceCode &&
                                                                                 x.Shape == quoteLineItem.ShapePriceCode &&
                                                                                 x.PunchType == lineItem.Value &&
                                                                                 x.QuantityOrdered >= x.OrderQty &&
                                                                                 x.QuoteNo == quote.QuoteNumber &&
                                                                                 x.Revision == quote.QuoteRevNo))
                                {
                                    if (Math.Round(basePrice, 2, MidpointRounding.ToPositiveInfinity) != Math.Round(_nat02Context.EoiBasePriceList.Where(x => x.Category == category &&
                                                                                    x.MachineType == quoteLineItem.MachinePriceCode &&
                                                                                    x.SteelPriceCode == quoteLineItem.SteelPriceCode &&
                                                                                    x.Shape == quoteLineItem.ShapePriceCode &&
                                                                                    x.PunchType == lineItem.Value &&
                                                                                    x.QuantityOrdered >= x.OrderQty &&
                                                                                    x.QuoteNo == quote.QuoteNumber &&
                                                                                    x.Revision == quote.QuoteRevNo).OrderByDescending(x => x.OrderQty).First().BasePrice, 2, MidpointRounding.ToPositiveInfinity))
                                    {
                                        priceChanged = true;
                                    }
                                }
                            }
                        }
                        else
                        {

                            if (international)
                            {
                                basePrice = 0.0;
                            }
                            else
                            {
                                if (_nat02Context.EoiBasePriceList.Any(x => x.Category == category &&
                                                                         x.MachineType == quoteLineItem.MachinePriceCode &&
                                                                         x.SteelPriceCode == quoteLineItem.SteelPriceCode &&
                                                                         x.Shape == quoteLineItem.ShapePriceCode &&
                                                                         x.PunchType == lineItem.Value &&
                                                                         x.QuantityOrdered >= x.OrderQty &&
                                                                         x.QuoteNo == quote.QuoteNumber &&
                                                                         x.Revision == quote.QuoteRevNo))
                                {
                                    basePrice = _nat02Context.EoiBasePriceList.Where(x => x.Category == category &&
                                                                            x.MachineType == quoteLineItem.MachinePriceCode &&
                                                                            x.SteelPriceCode == quoteLineItem.SteelPriceCode &&
                                                                            x.Shape == quoteLineItem.ShapePriceCode &&
                                                                            x.PunchType == lineItem.Value &&
                                                                            x.QuantityOrdered >= x.OrderQty &&
                                                                            x.QuoteNo == quote.QuoteNumber &&
                                                                            x.Revision == quote.QuoteRevNo).OrderByDescending(x => x.OrderQty).First().BasePrice;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IMethods.WriteToErrorLog("QuoteInfoWindow => ScratchPad Base Price / Price Changed; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                    }
                    priceBase.Text = string.Format("{0:0.00}", basePrice);
                    priceBase.HorizontalAlignment = HorizontalAlignment.Stretch;
                    priceBase.VerticalAlignment = VerticalAlignment.Top;
                    priceBase.HorizontalContentAlignment = HorizontalAlignment.Right;
                    priceBase.Name = "BasePrice" + lineItem.Key + lineItem.Value;
                    priceBase.Tag = "$" + lineItem.Key + lineItem.Value;
                    priceBase.SetValue(Grid.ColumnProperty, 1);
                    priceBase.SetValue(Grid.ColumnSpanProperty, 1);
                    priceBase.TextChanged += BasePriceChanged;
                    TextBlock basePriceTextBlock = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    basePriceTextBlock.Text = "BASE PRICE";
                    if (priceChanged)
                    {
                        basePriceTextBlock.Background = Brushes.GreenYellow;
                        priceChanged = false;
                    }
                    else
                    {
                        basePriceTextBlock.Background = Brushes.White;
                        priceChanged = false;
                    }
                    basePriceTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    basePriceTextBlock.Margin = new Thickness(9, 0, 0, 0);
                    basePriceTextBlock.SetValue(Grid.ColumnProperty, 2);
                    basePriceTextBlock.SetValue(Grid.ColumnSpanProperty, 2);
                    basePriceGrid.Children.Add(dollarSignBase);
                    basePriceGrid.Children.Add(priceBase);
                    basePriceGrid.Children.Add(basePriceTextBlock);
                    //--------------------------------------------------------------------------------------------
                    StackPanel optionsStackPanel = new StackPanel();
                    optionsStackPanel.HorizontalAlignment = HorizontalAlignment.Stretch;
                    optionsStackPanel.SetValue(Grid.ColumnProperty, 0);
                    optionsStackPanel.SetValue(Grid.ColumnSpanProperty, 1);
                    optionsStackPanel.SetValue(Grid.RowProperty, 1);
                    optionsStackPanel.SetValue(Grid.RowSpanProperty, 1);
                    //--------------------------------------------------------------------------------------------
                    TextBlock commentHeader = new TextBlock { Style = (Style)Application.Current.Resources["BoldTextBlock"] };
                    commentHeader.Text = "Line Item Comment";
                    commentHeader.FontWeight = FontWeights.Bold;
                    commentHeader.HorizontalAlignment = HorizontalAlignment.Center;
                    commentHeader.VerticalAlignment = VerticalAlignment.Center;
                    commentHeader.Margin = new Thickness(0, 0, 40, 0);
                    commentHeader.SetValue(Grid.ColumnProperty, 1);
                    commentHeader.SetValue(Grid.ColumnSpanProperty, 1);
                    commentHeader.SetValue(Grid.RowProperty, 0);
                    commentHeader.SetValue(Grid.RowSpanProperty, 1);

                    TextBox commentBox = new TextBox();
                    commentBox.IsTabStop = false;
                    commentBox.TextWrapping = TextWrapping.Wrap;
                    commentBox.IsEnabled = !isConvertedToOrder;
                    commentBox.Name = lineItem.Value.ToString().ToUpper() + lineItem.Key.ToString() + "SMIScratchPadComment";
                    commentBox.Tag = "CommentBox";
                    if (exists)
                    {
                        commentBox.Text = _nat02Context.EoiQuoteScratchPad.Where(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == Convert.ToByte(quote.QuoteRevNo) && q.LineNo == lineItem.Key && q.LineType == lineItem.Value).First().Comment;
                    }
                    commentBox.VerticalAlignment = VerticalAlignment.Stretch;
                    commentBox.Margin = new Thickness(0, 0, 40, 0);
                    commentBox.SetValue(Grid.ColumnProperty, 1);
                    commentBox.SetValue(Grid.ColumnSpanProperty, 1);
                    commentBox.SetValue(Grid.RowProperty, 1);
                    commentBox.SetValue(Grid.RowSpanProperty, 1);

                    Button saveLineItemButton = new Button();
                    saveLineItemButton.IsEnabled = !isConvertedToOrder;
                    saveLineItemButton.Name = lineItem.Value.ToString().ToUpper() + lineItem.Key.ToString() + "SMIScratchPadButton";
                    saveLineItemButton.Content = "Save Line Item";
                    saveLineItemButton.Width = 1400;
                    saveLineItemButton.MinHeight = 24;
                    saveLineItemButton.HorizontalAlignment = HorizontalAlignment.Center;
                    saveLineItemButton.VerticalAlignment = VerticalAlignment.Center;
                    saveLineItemButton.Margin = new Thickness(0, 0, 40, 0);
                    saveLineItemButton.Click += SaveLineItemButton_Click;
                    saveLineItemButton.Style = FindResource("Button") as Style;
                    saveLineItemButton.SetValue(Grid.ColumnProperty, 1);
                    saveLineItemButton.SetValue(Grid.ColumnSpanProperty, 1);
                    saveLineItemButton.SetValue(Grid.RowProperty, 2);
                    saveLineItemButton.SetValue(Grid.RowSpanProperty, 1);

                    DockPanel checkBoxDockPanel = new DockPanel();
                    checkBoxDockPanel.Tag = "CheckBoxGrid";
                    checkBoxDockPanel.Name = lineItem.Value.ToString().ToUpper() + lineItem.Key.ToString() + "CheckBoxGrid";
                    checkBoxDockPanel.VerticalAlignment = VerticalAlignment.Center;
                    checkBoxDockPanel.HorizontalAlignment = HorizontalAlignment.Center;
                    checkBoxDockPanel.SetValue(Grid.ColumnProperty, 1);
                    checkBoxDockPanel.SetValue(Grid.ColumnSpanProperty, 1);
                    checkBoxDockPanel.SetValue(Grid.RowProperty, 4);
                    checkBoxDockPanel.SetValue(Grid.RowSpanProperty, 1);
                    CheckBox checkBox = new CheckBox();
                    checkBox.Name = "OverrideUnitPriceCheckBox" + lineItem.Key + lineItem.Value;
                    checkBox.IsChecked = quoteDetails.UnitPriceOverride == true ? true : false;
                    checkBox.Checked += OverrideUnitPriceCheckBox_Checked;
                    checkBox.Unchecked += OverrideUnitPriceCheckBox_Unchecked;

                    checkBoxDockPanel.Children.Add(checkBox);

                    TextBlock unitPriceOverrideTextBlock = new TextBlock { Text = "Override Unit Price", Tag = "UnitPriceOverrideTextBlock", Name = lineItem.Value.ToString().ToUpper() + lineItem.Key.ToString() + "UnitPriceOverrideTextBlock", Margin = new Thickness(5, 0, 0, 0) };

                    checkBoxDockPanel.Children.Add(unitPriceOverrideTextBlock);
                    //--------------------------------------------------------------------------------------------
                    short q = 0;
                    foreach (string optionCode in quoteLineItem.OptionNumbers)
                    {
                        Grid optionLinePriceGrid = new Grid();
                        ColumnDefinition columnOLPG1_10 = new ColumnDefinition();
                        columnOLPG1_10.Width = new GridLength(10, GridUnitType.Pixel);
                        ColumnDefinition columnOLPG2_60 = new ColumnDefinition();
                        columnOLPG2_60.Width = new GridLength(60, GridUnitType.Pixel);
                        ColumnDefinition columnOLPG3_45 = new ColumnDefinition();
                        ColumnDefinition columnOLPG4_ = new ColumnDefinition();
                        columnOLPG3_45.Width = new GridLength(45, GridUnitType.Pixel);
                        optionLinePriceGrid.ColumnDefinitions.Add(columnOLPG1_10);
                        optionLinePriceGrid.ColumnDefinitions.Add(columnOLPG2_60);
                        optionLinePriceGrid.ColumnDefinitions.Add(columnOLPG3_45);
                        optionLinePriceGrid.ColumnDefinitions.Add(columnOLPG4_);


                        string[] printables = quoteLineItem.Options[q];
                        if (printables[0].Trim().Length != 0)
                        {
                            string optionType = quoteLineItem.OptionType[optionCode] == "P" ? "%" : "$";
                            TextBlock dollarSign = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                            dollarSign.Text = optionType;
                            dollarSign.Tag = optionType + lineItem.Key + lineItem.Value;
                            dollarSign.SetValue(Grid.ColumnProperty, 0);
                            dollarSign.SetValue(Grid.ColumnSpanProperty, 1);

                            TextBox price = new TextBox();
                            price.TabIndex = q + 1;
                            price.GotFocus += Price_GotFocus;
                            price.GotMouseCapture += Price_GotMouseCapture;
                            price.IsMouseCaptureWithinChanged += Price_IsMouseCaptureWithinChanged;
                            price.IsEnabled = !isConvertedToOrder;
                            price.HorizontalAlignment = HorizontalAlignment.Stretch;
                            price.VerticalAlignment = VerticalAlignment.Stretch;
                            price.HorizontalContentAlignment = HorizontalAlignment.Right;
                            price.Text = string.Format("{0:0.00}", quoteLineItem.OptionPrice[optionCode]);
                            price.Name = "Price_" + optionCode;
                            price.Tag = optionType + lineItem.Key + lineItem.Value;
                            price.SetValue(Grid.ColumnProperty, 1);
                            price.SetValue(Grid.ColumnSpanProperty, 1);
                            price.TextChanged += OptionPriceChanged;

                            TextBlock optionCodeTextBlock = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                            optionCodeTextBlock.Name = lineItem.Value.ToString().ToUpper() + "_" + lineItem.Key.ToString() + "_" + optionCode;
                            optionCodeTextBlock.Tag = optionCode;
                            optionCodeTextBlock.Text = "(" + optionCode + ")";
                            optionCodeTextBlock.HorizontalAlignment = HorizontalAlignment.Center;
                            optionCodeTextBlock.SetValue(Grid.ColumnProperty, 2);
                            optionCodeTextBlock.SetValue(Grid.ColumnSpanProperty, 1);

                            TextBlock optionText = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                            optionText.Text = String.Concat(printables);
                            optionText.SetValue(Grid.ColumnProperty, 3);
                            optionText.SetValue(Grid.ColumnSpanProperty, 1);

                            optionLinePriceGrid.Children.Add(dollarSign);
                            optionLinePriceGrid.Children.Add(price);
                            optionLinePriceGrid.Children.Add(optionCodeTextBlock);
                            optionLinePriceGrid.Children.Add(optionText);
                            optionsStackPanel.Children.Add(optionLinePriceGrid);
                            q++;
                        }
                    }
                    //--------------------------------------------------------------------------------------------
                    Grid gridPercentMark = new Grid();
                    ColumnDefinition columnPM1_10 = new ColumnDefinition();
                    columnPM1_10.Width = new GridLength(10, GridUnitType.Pixel);
                    ColumnDefinition columnPM2_60 = new ColumnDefinition();
                    columnPM2_60.Width = new GridLength(60, GridUnitType.Pixel);
                    ColumnDefinition columnPM3_45 = new ColumnDefinition();
                    columnPM3_45.Width = new GridLength(45, GridUnitType.Pixel);
                    ColumnDefinition columnPM4_45 = new ColumnDefinition();
                    gridPercentMark.Tag = "PercentMark";
                    gridPercentMark.ColumnDefinitions.Add(columnPM1_10);
                    gridPercentMark.ColumnDefinitions.Add(columnPM2_60);
                    gridPercentMark.ColumnDefinitions.Add(columnPM3_45);
                    gridPercentMark.ColumnDefinitions.Add(columnPM4_45);
                    gridPercentMark.SetValue(Grid.ColumnProperty, 0);
                    gridPercentMark.SetValue(Grid.ColumnSpanProperty, 1);
                    gridPercentMark.SetValue(Grid.RowProperty, 3);
                    gridPercentMark.SetValue(Grid.RowSpanProperty, 1);

                    TextBlock percentSignPM = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    percentSignPM.Text = "%";
                    percentSignPM.VerticalAlignment = VerticalAlignment.Bottom;
                    percentSignPM.SetValue(Grid.ColumnProperty, 0);
                    percentSignPM.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBox pricePM = new TextBox();
                    pricePM.GotFocus += Price_GotFocus;
                    pricePM.GotMouseCapture += Price_GotMouseCapture;
                    pricePM.IsMouseCaptureWithinChanged += Price_IsMouseCaptureWithinChanged;
                    pricePM.IsEnabled = !isConvertedToOrder;
                    pricePM.Text = exists ? string.Format("{0:0.00}", _nat02Context.EoiQuoteScratchPad.First(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == Convert.ToByte(quote.QuoteRevNo) && q.LineNo == lineItem.Key && q.LineType == lineItem.Value).PercentMark) : "0.00";
                    pricePM.Name = "PercentMark" + lineItem.Key + lineItem.Value;
                    pricePM.Tag = "%" + lineItem.Key + lineItem.Value;
                    pricePM.TextChanged += UnitPercentMarkChanged;
                    pricePM.HorizontalAlignment = HorizontalAlignment.Stretch;
                    pricePM.VerticalAlignment = VerticalAlignment.Bottom;
                    pricePM.HorizontalContentAlignment = HorizontalAlignment.Right;
                    pricePM.SetValue(Grid.ColumnProperty, 1);
                    pricePM.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBlock PMTextBlock = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    PMTextBlock.Text = "PERCENT MARK";
                    PMTextBlock.ToolTip = "Positive for markup" + Environment.NewLine + "Negative for discount";
                    PMTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    PMTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    PMTextBlock.Margin = new Thickness(9, 0, 0, 0);
                    PMTextBlock.SetValue(Grid.ColumnProperty, 2);
                    PMTextBlock.SetValue(Grid.ColumnSpanProperty, 2);

                    gridPercentMark.Children.Add(percentSignPM);
                    gridPercentMark.Children.Add(pricePM);
                    gridPercentMark.Children.Add(PMTextBlock);
                    //--------------------------------------------------------------------------------------------
                    Grid unitPriceGrid = new Grid();
                    unitPriceGrid.Tag = "UnitPrice";
                    ColumnDefinition columnPartSub1_10 = new ColumnDefinition();
                    columnPartSub1_10.Width = new GridLength(10, GridUnitType.Pixel);
                    ColumnDefinition columnPartSub2_60 = new ColumnDefinition();
                    columnPartSub2_60.Width = new GridLength(60, GridUnitType.Pixel);
                    ColumnDefinition columnPartSub3_45 = new ColumnDefinition();
                    columnPartSub3_45.Width = new GridLength(45, GridUnitType.Pixel);
                    ColumnDefinition columnPartSub4_45 = new ColumnDefinition();
                    unitPriceGrid.ColumnDefinitions.Add(columnPartSub1_10);
                    unitPriceGrid.ColumnDefinitions.Add(columnPartSub2_60);
                    unitPriceGrid.ColumnDefinitions.Add(columnPartSub3_45);
                    unitPriceGrid.ColumnDefinitions.Add(columnPartSub4_45);
                    unitPriceGrid.SetValue(Grid.ColumnProperty, 0);
                    unitPriceGrid.SetValue(Grid.ColumnSpanProperty, 1);
                    unitPriceGrid.SetValue(Grid.RowProperty, 4);
                    unitPriceGrid.SetValue(Grid.RowSpanProperty, 1);

                    TextBlock dollarSignUnitPrice = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    dollarSignUnitPrice.Text = "$";
                    dollarSignUnitPrice.VerticalAlignment = VerticalAlignment.Bottom;
                    dollarSignUnitPrice.SetValue(Grid.ColumnProperty, 0);
                    dollarSignUnitPrice.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBox unitPriceTextBox = new TextBox();
                    unitPriceTextBox.GotFocus += Price_GotFocus;
                    unitPriceTextBox.GotMouseCapture += Price_GotMouseCapture;
                    unitPriceTextBox.IsMouseCaptureWithinChanged += Price_IsMouseCaptureWithinChanged;
                    unitPriceTextBox.IsEnabled = !(quote.OrderNo > 0) && quoteDetails.UnitPriceOverride == true ? true : false;
                    unitPriceTextBox.Text = string.Format("{0:0.00}", quoteLineItem.UnitPrice);
                    unitPriceTextBox.TextChanged += UnitPriceTextBox_TextChanged;
                    unitPriceTextBox.Name = "UnitPrice" + lineItem.Key + lineItem.Value;
                    unitPriceTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    unitPriceTextBox.VerticalAlignment = VerticalAlignment.Bottom;
                    unitPriceTextBox.HorizontalContentAlignment = HorizontalAlignment.Right;
                    unitPriceTextBox.SetValue(Grid.ColumnProperty, 1);
                    unitPriceTextBox.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBlock unitPriceTextBlock = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    unitPriceTextBlock.Text = "UNIT PRICE";
                    unitPriceTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    unitPriceTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    unitPriceTextBlock.Margin = new Thickness(9, 0, 0, 0);
                    unitPriceTextBlock.SetValue(Grid.ColumnProperty, 2);
                    unitPriceTextBlock.SetValue(Grid.ColumnSpanProperty, 2);
                    unitPriceGrid.Children.Add(dollarSignUnitPrice);
                    unitPriceGrid.Children.Add(unitPriceTextBox);
                    unitPriceGrid.Children.Add(unitPriceTextBlock);
                    //--------------------------------------------------------------------------------------------
                    Grid gridQTY = new Grid();
                    ColumnDefinition columnQTY1_10 = new ColumnDefinition();
                    columnQTY1_10.Width = new GridLength(10, GridUnitType.Pixel);
                    ColumnDefinition columnQTY2_60 = new ColumnDefinition();
                    columnQTY2_60.Width = new GridLength(60, GridUnitType.Pixel);
                    ColumnDefinition columnQTY3_45 = new ColumnDefinition();
                    columnQTY3_45.Width = new GridLength(45, GridUnitType.Pixel);
                    ColumnDefinition columnQTY4_45 = new ColumnDefinition();
                    gridQTY.Tag = "QTY";
                    gridQTY.ColumnDefinitions.Add(columnQTY1_10);
                    gridQTY.ColumnDefinitions.Add(columnQTY2_60);
                    gridQTY.ColumnDefinitions.Add(columnQTY3_45);
                    gridQTY.ColumnDefinitions.Add(columnQTY4_45);
                    gridQTY.SetValue(Grid.ColumnProperty, 0);
                    gridQTY.SetValue(Grid.ColumnSpanProperty, 1);
                    gridQTY.SetValue(Grid.RowProperty, 5);
                    gridQTY.SetValue(Grid.RowSpanProperty, 1);

                    TextBlock actualQTY = new TextBlock();
                    actualQTY.Text = quoteLineItem.QtyOrdered.ToString();
                    actualQTY.HorizontalAlignment = HorizontalAlignment.Right;
                    actualQTY.VerticalAlignment = VerticalAlignment.Bottom;
                    actualQTY.Tag = "QTY";
                    actualQTY.SetValue(Grid.ColumnProperty, 1);
                    actualQTY.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBlock QTYTextBlock = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    QTYTextBlock.Text = "QTY";
                    QTYTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    QTYTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    QTYTextBlock.Margin = new Thickness(9, 0, 0, 0);
                    QTYTextBlock.SetValue(Grid.ColumnProperty, 2);
                    QTYTextBlock.SetValue(Grid.ColumnSpanProperty, 2);
                    gridQTY.Children.Add(actualQTY);
                    gridQTY.Children.Add(QTYTextBlock);
                    //--------------------------------------------------------------------------------------------
                    Grid extendedPriceGrid = new Grid();
                    extendedPriceGrid.Tag = "ExtendedPrice";
                    ColumnDefinition columnSub1_10 = new ColumnDefinition();
                    columnSub1_10.Width = new GridLength(10, GridUnitType.Pixel);
                    ColumnDefinition columnSub2_60 = new ColumnDefinition();
                    columnSub2_60.Width = new GridLength(60, GridUnitType.Pixel);
                    ColumnDefinition columnSub3_45 = new ColumnDefinition();
                    columnSub3_45.Width = new GridLength(45, GridUnitType.Pixel);
                    ColumnDefinition columnSub4_45 = new ColumnDefinition();
                    extendedPriceGrid.ColumnDefinitions.Add(columnSub1_10);
                    extendedPriceGrid.ColumnDefinitions.Add(columnSub2_60);
                    extendedPriceGrid.ColumnDefinitions.Add(columnSub3_45);
                    extendedPriceGrid.ColumnDefinitions.Add(columnSub4_45);
                    extendedPriceGrid.SetValue(Grid.ColumnProperty, 0);
                    extendedPriceGrid.SetValue(Grid.ColumnSpanProperty, 1);
                    extendedPriceGrid.SetValue(Grid.RowProperty, 6);
                    extendedPriceGrid.SetValue(Grid.RowSpanProperty, 1);

                    TextBlock dollarSignExtendedPrice = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    dollarSignExtendedPrice.Text = "$";
                    dollarSignExtendedPrice.VerticalAlignment = VerticalAlignment.Bottom;
                    dollarSignExtendedPrice.SetValue(Grid.ColumnProperty, 0);
                    dollarSignExtendedPrice.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBox extendedPriceTextBox = new TextBox();
                    extendedPriceTextBox.GotFocus += Price_GotFocus;
                    extendedPriceTextBox.GotMouseCapture += Price_GotMouseCapture;
                    extendedPriceTextBox.IsMouseCaptureWithinChanged += Price_IsMouseCaptureWithinChanged;
                    extendedPriceTextBox.IsEnabled = !isConvertedToOrder;
                    extendedPriceTextBox.Text = string.Format("{0:0.00}", quoteLineItem.ExtendedPrice);
                    extendedPriceTextBox.Name = "ExtendedPrice" + lineItem.Key + lineItem.Value;
                    extendedPriceTextBox.Tag = "ExtendedPrice";
                    extendedPriceTextBox.TextChanged += ExtendedPriceTextBox_TextChanged;
                    extendedPriceTextBox.IsEnabled = false;
                    extendedPriceTextBox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    extendedPriceTextBox.VerticalAlignment = VerticalAlignment.Bottom;
                    extendedPriceTextBox.HorizontalContentAlignment = HorizontalAlignment.Right;
                    extendedPriceTextBox.SetValue(Grid.ColumnProperty, 1);
                    extendedPriceTextBox.SetValue(Grid.ColumnSpanProperty, 1);

                    TextBlock extendedPriceTextBlock = new TextBlock { Style = (Style)Application.Current.Resources["NormalTextBlock"] };
                    extendedPriceTextBlock.Text = "EXTENDED PRICE";
                    extendedPriceTextBlock.HorizontalAlignment = HorizontalAlignment.Left;
                    extendedPriceTextBlock.VerticalAlignment = VerticalAlignment.Bottom;
                    extendedPriceTextBlock.Margin = new Thickness(9, 0, 0, 0);
                    extendedPriceTextBlock.SetValue(Grid.ColumnProperty, 2);
                    extendedPriceTextBlock.SetValue(Grid.ColumnSpanProperty, 2);
                    extendedPriceGrid.Children.Add(dollarSignExtendedPrice);
                    extendedPriceGrid.Children.Add(extendedPriceTextBox);
                    extendedPriceGrid.Children.Add(extendedPriceTextBlock);
                    //--------------------------------------------------------------------------------------------
                    grid.Children.Add(gridQTY);
                    grid.Children.Add(basePriceGrid);
                    grid.Children.Add(gridPercentMark);
                    grid.Children.Add(optionsStackPanel);
                    grid.Children.Add(commentHeader);
                    grid.Children.Add(commentBox);
                    grid.Children.Add(unitPriceGrid);
                    grid.Children.Add(extendedPriceGrid);
                    grid.Children.Add(saveLineItemButton);
                    grid.Children.Add(checkBoxDockPanel);
                    scrollViewer.Content = grid;
                    tab.Content = scrollViewer;
                    ScratchPadTabs.Items.Add(tab);
                }
                FlatMark.Text = string.Format("{0:0.00}", quote.QuoteMarkdown);
                QuoteSubTotal.Text = string.Format("{0:0.00}", quote.QuoteSubtotal);
                FreightTotal.Text = string.Format("{0:0.00}", quote.QuoteFreightCharge);
                QuoteTotal.Text = string.Format("{0:0.00}", quote.QuoteTotal);
                SaveAllLineItemsButton.IsEnabled = !isConvertedToOrder;
            }
            #endregion
            _nat02Context.Dispose();
            _nat01Context.Dispose();
            _natbcContext.Dispose();
        }

        //private void Price_GotMouseCapture(object sender, EventArgs e)
        //{
        //    TextBox textBox = (TextBox)sender;
        //    if (!textBox.IsSelectionActive)
        //    {
        //        textBox.SelectAll();
        //        textBox.SelectionStart = 0;
        //        textBox.SelectionLength = textBox.Text.Length;
        //    }            
        //}
        //private void Price_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        //{
        //    TextBox textBox = (TextBox)sender;
        //    textBox.SelectAll();
        //    textBox.SelectionStart = 0;
        //    textBox.SelectionLength = textBox.Text.Length;
        //}

        private void Price_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.CaptureMouse();
        }

        private void Price_GotMouseCapture(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.SelectAll();
        }

        private void Price_IsMouseCaptureWithinChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            textBox.SelectAll();
        }

        private void ChangeSMIScrollHeights()
        {
            if (!(ScratchPadTabs is null))
            {
                SMITabItem.UpdateLayout();
                foreach (TabItem tab in ScratchPadTabs.Items)
                {
                    ScrollViewer scrollViewer = tab.Content as ScrollViewer;
                    TabItem tabItem = ScratchPadTabs.Items[0] as TabItem;
                    SMIStackPanel.MaxHeight = (tabControl.ActualHeight - 40 - ButtonBorder2.ActualHeight - QuoteTabItem.ActualHeight - tabItem.ActualHeight - QuoteSubtotalGrid.ActualHeight - QuoteFreightTotalGrid.ActualHeight - QuoteFlatMarkGrid.ActualHeight - QuoteTotalGrid.ActualHeight) / 2;
                    if (SMIListExpander.IsExpanded)
                    {
                        SMIStackPanel.UpdateLayout();
                        scrollViewer.MaxHeight = (tabControl.ActualHeight - 40 - ButtonBorder2.ActualHeight - QuoteTabItem.ActualHeight - tabItem.ActualHeight - QuoteSubtotalGrid.ActualHeight - QuoteFreightTotalGrid.ActualHeight - QuoteFlatMarkGrid.ActualHeight - QuoteTotalGrid.ActualHeight) - SMIStackPanel.ActualHeight;
                    }
                    else
                    {
                        scrollViewer.MaxHeight = tabControl.ActualHeight - 40 - ButtonBorder2.ActualHeight - QuoteTabItem.ActualHeight - tabItem.ActualHeight - QuoteSubtotalGrid.ActualHeight - QuoteFreightTotalGrid.ActualHeight - QuoteFlatMarkGrid.ActualHeight - QuoteTotalGrid.ActualHeight - 23;
                    }
                }
            }
        }

        #region SMI Events
        private void SMIIsRadioChecked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            isChecked = radioButton.IsChecked;
        }
        private void SMIUncheckRadioButtons(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            Grid grid = radioButton.Parent as Grid;
            Border border = grid.Children.OfType<Border>().First(b => b.Tag.ToString() == "InstructionBorder") as Border;
            TextBlock textBlock = border.Child as TextBlock;
            if (isChecked == true)
            {
                radioButton.IsChecked = false;
                textBlock.Background = Brushes.Transparent;
                textBlock.TextDecorations = null;
            }
            else
            {
                if (radioButton.Tag.ToString() == "Y")
                {
                    textBlock.Background = Brushes.Yellow;
                    textBlock.TextDecorations = null;
                }
                else
                {
                    textBlock.TextDecorations = TextDecorations.Strikethrough;
                    textBlock.Background = Brushes.Transparent;
                }
            }
        }
        private void SMIListExpander_Expanded(object sender, RoutedEventArgs e)
        {
            ChangeSMIScrollHeights();
        }
        private void SMIListExpander_Collapsed(object sender, RoutedEventArgs e)
        {
            ChangeSMIScrollHeights();
        }
        private void SaveSMIButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (Grid grid in SMIStackPanel.Children.OfType<Grid>().Where(g => g.Tag.ToString() == "SMILine"))
                {
                    string s = grid.Name.ToString().Remove(0, 8);
                    short sequence = Convert.ToInt16(s.Remove(s.IndexOf("_")));
                    s = s.Substring(s.IndexOf("_") + 1);
                    string customerID = s.Remove(s.IndexOf("_"));
                    bool? applies = null;
                    foreach (RadioButton radioButton in grid.Children.OfType<RadioButton>())
                    {
                        if (radioButton.IsChecked == true)
                        {
                            if (radioButton.Tag.ToString() == "Y")
                            {
                                applies = true;
                            }
                            else
                            {
                                applies = false;
                            }
                        }
                    }
                    
                    TextBlock instructionTextBlock = grid.Children.OfType<Border>().First(b => b.Tag.ToString() == "InstructionBorder").Child as TextBlock;
                    string instruction = instructionTextBlock.Text.ToString().Trim();
                    using (NAT02Context _nat02Context = new NAT02Context())
                    {
                        if (_nat02Context.EoiQuoteSMICheck.Any(i => i.QuoteNo == quote.QuoteNumber && i.RevNo == (byte)quote.QuoteRevNo && i.CustomerID.Trim() == customerID && i.Sequence == sequence))
                        {
                            EoiQuoteSMICheck eoiQuoteSMICheck = _nat02Context.EoiQuoteSMICheck.Where(i => i.QuoteNo == quote.QuoteNumber && i.RevNo == (byte)quote.QuoteRevNo && i.CustomerID.Trim() == customerID && i.Sequence == sequence).First();
                            eoiQuoteSMICheck.AppliesToQuote = applies;
                            eoiQuoteSMICheck.Instruction = instruction;
                            eoiQuoteSMICheck.User = Environment.UserName;
                            eoiQuoteSMICheck.DateTimeStamp = DateTime.Now;
                        }
                        else
                        {
                            EoiQuoteSMICheck eoiQuoteSMICheck = new EoiQuoteSMICheck
                            {
                                QuoteNo = quote.QuoteNumber,
                                RevNo = (byte)quote.QuoteRevNo,
                                CustomerID = customerID,
                                Sequence = sequence,
                                AppliesToQuote = applies,
                                Instruction = instruction,
                                User = Environment.UserName,
                                DateTimeStamp = DateTime.Now
                            };
                            _nat02Context.EoiQuoteSMICheck.Add(eoiQuoteSMICheck);
                        }
                        _nat02Context.SaveChanges();
                        _nat02Context.Dispose();
                    }
                }
                MessageBox.Show("Saved SMI Check Successfully", "Success!", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => SaveSMIButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }

        }
        #endregion

        #region ScratchPad Events
        private void BasePriceChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox box = sender as TextBox;
                Grid parentGrid = box.Parent as Grid;
                Grid parentGridparentGrid = parentGrid.Parent as Grid;
                double unitPrice = 0;
                try
                {
                    if (double.TryParse(box.Text, out double p1))
                    {
                        unitPrice = p1;
                    }
                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("QuoteInfoWindow => BasePriceChanged => unitPrice Conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                }
                double unitPercent = 0;
                int QTY = 0;
                string keyValue = box.Tag.ToString().Remove(0, 1);
                foreach (StackPanel stackPanel in parentGridparentGrid.Children.OfType<StackPanel>())
                {
                    foreach (Grid grid in stackPanel.Children.OfType<Grid>())
                    {
                        foreach (TextBox textBox in grid.Children.OfType<TextBox>())
                        {
                            if (textBox.Tag.ToString().Contains("$"))
                            {
                                try
                                {
                                    unitPrice += Convert.ToDouble(textBox.Text);
                                }
                                catch
                                {

                                }
                            }
                            if (textBox.Tag.ToString().Contains("%"))
                            {
                                try
                                {
                                    unitPercent += Convert.ToDouble(textBox.Text);
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
                foreach (Grid grid in parentGridparentGrid.Children.OfType<Grid>().Where(x => !(x.Tag is null) && (x.Tag.ToString() == "UnitPrice" || x.Tag.ToString() == "ExtendedPrice" || x.Tag.ToString() == "QTY" || x.Tag.ToString() == "PercentMark")))
                {
                    foreach (TextBlock textBlock in grid.Children.OfType<TextBlock>().Where(x => !(x.Tag is null) && x.Tag.ToString() == "QTY"))
                    {
                        try
                        {
                            QTY = Convert.ToInt32(textBlock.Text);
                        }
                        catch
                        {

                        }
                    }
                    foreach (TextBox textBox in grid.Children.OfType<TextBox>())
                    {
                        if (textBox.Name == "PercentMark" + keyValue)
                        {
                            try
                            {
                                unitPercent += Convert.ToDouble(textBox.Text);
                            }
                            catch
                            {

                            }
                        }
                        if (textBox.Name == "UnitPrice" + keyValue)
                        {
                            textBox.Text = string.Format("{0:0.00}", Math.Round(unitPrice * (1.0 + (unitPercent / 100)), 2, MidpointRounding.ToPositiveInfinity));
                        }
                        //if (textBox.Name == "ExtendedPrice" + keyValue)
                        //{
                        //    textBox.Text = string.Format("{0:0.00}", QTY * Math.Round(unitPrice * (1.0 + (unitPercent / 100)), 2, MidpointRounding.ToPositiveInfinity));
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => BasePriceChanged; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void OptionPriceChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox box = sender as TextBox;
                Grid parentGrid = box.Parent as Grid;
                StackPanel parentStack = parentGrid.Parent as StackPanel;
                Grid parentStackparentGrid = parentStack.Parent as Grid;

                double unitPrice = 0;
                double unitPercent = 0;

                int QTY = 0;
                string keyValue = box.Tag.ToString().Remove(0, 1);
                foreach (Grid grid in parentStack.Children.OfType<Grid>())
                {
                    foreach (TextBox textBox in grid.Children.OfType<TextBox>())
                    {
                        if (textBox.Tag.ToString().Contains("$"))
                        {
                            try
                            {
                                if (double.TryParse(textBox.Text, out double p1))
                                {
                                    unitPrice += p1;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged => unitPrice conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                            }
                        }
                        if (textBox.Tag.ToString().Contains("%"))
                        {
                            try
                            {
                                if (double.TryParse(textBox.Text, out double p1))
                                {
                                    unitPercent += p1;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged => unitPercent conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                            }
                        }
                        if (textBox.Tag.ToString() == "QTY")
                        {
                            try
                            {
                                if (int.TryParse(textBox.Text, out int p1))
                                {
                                    QTY = p1;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged => QTY conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                            }
                        }
                    }
                }
                foreach (Grid grid in parentStackparentGrid.Children.OfType<Grid>().Where(x => !(x.Tag is null) && (x.Tag.ToString() == "UnitPrice" || x.Tag.ToString() == "ExtendedPrice" || x.Tag.ToString() == "QTY" || x.Tag.ToString() == "BasePriceGrid" || x.Tag.ToString() == "PercentMark")))
                {
                    foreach (TextBlock textBlock in grid.Children.OfType<TextBlock>().Where(x => !(x.Tag is null) && x.Tag.ToString() == "QTY"))
                    {
                        try
                        {
                            if (int.TryParse(textBlock.Text, out int p1))
                            {
                                QTY = p1;
                            }
                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged => QTY conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                        }
                    }
                    foreach (TextBox textBox in grid.Children.OfType<TextBox>())
                    {
                        if (textBox.Name == "BasePrice" + keyValue)
                        {
                            try
                            {
                                if (double.TryParse(textBox.Text, out double p1))
                                {
                                    unitPrice += p1;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged => unitPrice conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                            }
                        }
                        if (textBox.Name == "PercentMark" + keyValue)
                        {
                            try
                            {
                                if (double.TryParse(textBox.Text, out double p1))
                                {
                                    unitPercent += p1;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged => unitPercent conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                            }
                        }
                        if (textBox.Name == "UnitPrice" + keyValue)
                        {
                            textBox.Text = string.Format("{0:0.00}", Math.Round(unitPrice * (1.0 + (unitPercent / 100)), 2, MidpointRounding.ToPositiveInfinity));
                        }
                        //if (textBox.Name == "ExtendedPrice" + keyValue)
                        //{

                        //    textBox.Text = string.Format("{0:0.00}", QTY * Math.Round(unitPrice * (1.0 + (unitPercent / 100)), 2, MidpointRounding.ToPositiveInfinity));
                        //}
                    }
                }

                //double quoteSubtotal = 0;
                //foreach (TabItem tabItem in ScratchPadTabs.Items)
                //{
                //    ScrollViewer scrollViewer = tabItem.Content as ScrollViewer;
                //    Grid masterGrid = scrollViewer.Content as Grid;
                //    foreach (Grid grid in masterGrid.Children.OfType<Grid>().Where(x => !(x.Tag is null) && x.Tag.ToString() == "ExtendedPrice"))
                //    {
                //        foreach (TextBox textBox in grid.Children.OfType<TextBox>().Where(x => x.Tag.ToString() == "ExtendedPrice"))
                //        {
                //            quoteSubtotal += Convert.ToDouble(textBox.Text);
                //        }
                //    }
                //}
                //QuoteSubTotal.Text = string.Format("{0:0.00}", quoteSubtotal);
                //QuoteTotal.Text = string.Format("{0:0.00}", quoteSubtotal + Convert.ToDouble(FlatMark.Text) + Convert.ToDouble(FreightTotal.Text));
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => OptionPriceChanged; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void UnitPercentMarkChanged(object sender, RoutedEventArgs e)
        {
            try
            {
                TextBox box = sender as TextBox;
                Grid parentGrid = box.Parent as Grid;
                Grid parentGridparentGrid = parentGrid.Parent as Grid;
                double unitPrice = 0;
                double unitPercent = 0;
                try
                {
                    if (double.TryParse(box.Text, out double p1))
                    {
                        unitPercent += p1;
                    }
                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("QuoteInfoWindow => UnitPercentMarkChanged => unitPercent conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                }

                int QTY = 0;
                string keyValue = box.Tag.ToString().Remove(0, 1);
                foreach (StackPanel stackPanel in parentGridparentGrid.Children.OfType<StackPanel>())
                {
                    foreach (Grid grid in stackPanel.Children.OfType<Grid>())
                    {
                        foreach (TextBox textBox in grid.Children.OfType<TextBox>())
                        {
                            if (textBox.Tag.ToString().Contains("$"))
                            {
                                try
                                {
                                    if (double.TryParse(textBox.Text, out double p1))
                                    {
                                        unitPrice += p1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    IMethods.WriteToErrorLog("QuoteInfoWindow => UnitPercentMarkChanged => unitPrice conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                                }
                            }
                            if (textBox.Tag.ToString().Contains("%"))
                            {
                                try
                                {
                                    if (double.TryParse(textBox.Text, out double p1))
                                    {
                                        unitPercent += p1;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    IMethods.WriteToErrorLog("QuoteInfoWindow => UnitPercentMarkChanged => unitPercent conversion; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                                }
                            }
                        }
                    }
                }
                foreach (Grid grid in parentGridparentGrid.Children.OfType<Grid>().Where(x => !(x.Tag is null) && (x.Tag.ToString() == "UnitPrice" || x.Tag.ToString() == "ExtendedPrice" || x.Tag.ToString() == "BasePriceGrid" || x.Tag.ToString() == "QTY")))
                {
                    foreach (TextBlock textBlock in grid.Children.OfType<TextBlock>().Where(x => !(x.Tag is null) && x.Tag.ToString() == "QTY"))
                    {
                        try
                        {
                            QTY = Convert.ToInt32(textBlock.Text);
                        }
                        catch
                        {

                        }
                    }
                    foreach (TextBox textBox in grid.Children.OfType<TextBox>())
                    {
                        if (textBox.Name == "BasePrice" + keyValue)
                        {
                            try
                            {
                                unitPrice += Convert.ToDouble(textBox.Text);
                            }
                            catch
                            {

                            }
                        }
                        if (textBox.Name == "UnitPrice" + keyValue)
                        {
                            textBox.Text = string.Format("{0:0.00}", Math.Round(unitPrice * (1.0 + (unitPercent / 100)), 2, MidpointRounding.ToPositiveInfinity));
                        }
                        //if (textBox.Name == "ExtendedPrice" + keyValue)
                        //{
                        //    textBox.Text = string.Format("{0:0.00}", QTY * Math.Round(unitPrice * (1.0 + (unitPercent / 100)), 2, MidpointRounding.ToPositiveInfinity));
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => UnitPercentMarkChanged; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void UnitPriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox;
                Grid unitPriceGrid = (Grid)textBox.Parent as Grid;
                Grid grid = unitPriceGrid.Parent as Grid;
                foreach (Grid subGrid in grid.Children.OfType<Grid>().Where(g => g.Tag.ToString() == "QTY"))
                {
                    TextBlock qtyTextBlock = subGrid.Children.OfType<TextBlock>().First(t => t.Tag.ToString() == "QTY");
                    if (int.TryParse(qtyTextBlock.Text == null ? "0" : qtyTextBlock.Text.ToString(), out int qty))
                    {
                        if (double.TryParse(textBox.Text == null ? "0" : textBox.Text.ToString(), out double unitPrice))
                        {
                            double extendedPrice = Math.Round(unitPrice * (double)qty, 2, MidpointRounding.ToPositiveInfinity);
                            foreach (Grid grid1 in grid.Children.OfType<Grid>().Where(g => g.Tag.ToString() == "ExtendedPrice"))
                            {
                                TextBox extendedPriceTextBox = grid1.Children.OfType<TextBox>().First(t => t.Tag.ToString() == "ExtendedPrice");
                                Dispatcher.Invoke(new Action(() => extendedPriceTextBox.Text = string.Format("{0:0.00}", extendedPrice)));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => UnitPriceTextBox_TextChanged; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void ExtendedPriceTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                double quoteSubtotal = 0;
                foreach (TabItem tabItem in ScratchPadTabs.Items)
                {
                    ScrollViewer scrollViewer = tabItem.Content as ScrollViewer;
                    Grid masterGrid = scrollViewer.Content as Grid;
                    foreach (Grid grid in masterGrid.Children.OfType<Grid>().Where(x => !(x.Tag is null) && x.Tag.ToString() == "ExtendedPrice"))
                    {
                        foreach (TextBox textBox in grid.Children.OfType<TextBox>().Where(x => x.Tag.ToString() == "ExtendedPrice"))
                        {
                            if (double.TryParse(textBox.Text == null ? "0" : textBox.Text.ToString(), out double extendedPrice))
                                quoteSubtotal += extendedPrice;
                        }
                    }
                }
                QuoteSubTotal.Text = string.Format("{0:0.00}", quoteSubtotal);
                QuoteTotal.Text = string.Format("{0:0.00}", quoteSubtotal + Convert.ToDouble(FlatMark.Text) + Convert.ToDouble(FreightTotal.Text));
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => ExtendedPriceTextBox_TextChanged; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }

        }
        private void OverrideUnitPriceCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                string keyValue = checkBox.Name.Remove(0, 25);
                DockPanel dockPanel = checkBox.Parent as DockPanel;
                Grid grid = dockPanel.Parent as Grid;
                TextBox unitPrice = grid.Children.OfType<Grid>().Where(g => g.Tag.ToString() == "UnitPrice").First().Children.OfType<TextBox>().First();
                unitPrice.IsEnabled = false;
                Grid basePriceGrid = grid.Children.OfType<Grid>().Where(g => g.Tag.ToString() == "BasePriceGrid").First();
                TextBox basePriceTextBox = basePriceGrid.Children.OfType<TextBox>().First();
                BasePriceChanged(basePriceTextBox, new RoutedEventArgs());
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => OverrideUnitPriceCheckBox_Unchecked; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void OverrideUnitPriceCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                CheckBox checkBox = sender as CheckBox;
                DockPanel dockPanel = checkBox.Parent as DockPanel;
                Grid grid = dockPanel.Parent as Grid;
                TextBox unitPrice = grid.Children.OfType<Grid>().Where(g => g.Tag.ToString() == "UnitPrice").First().Children.OfType<TextBox>().First();
                unitPrice.IsEnabled = true;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => OverrideUnitPriceCheckBox_Checked; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void SaveLineItemButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TabItem tab = ScratchPadTabs.SelectedItem as TabItem;
                string tabLineItemType = tab.Name.ToString().Remove(tab.Name.ToString().IndexOf('_'));
                short tabLineItemNumber = Convert.ToInt16(tab.Name.ToString().Remove(0, tab.Name.ToString().IndexOf('_') + 1));
                Dictionary<string, Tuple<bool, decimal>> optionPrices = new Dictionary<string, Tuple<bool, decimal>>();
                decimal basePrice = 0;
                decimal percentMark = 0;
                decimal unitPrice = 0;
                //bool unitPriceOverride = false;
                decimal extendedPrice = 0;
                string comment = "";
                decimal optionsIncrements = 0;
                decimal optionsPercentage = 0;
                ScrollViewer scroll = tab.Content as ScrollViewer;
                Grid lineItemMasterGrid = scroll.Content as Grid;

                //BasePrice
                //PercentMark
                //UnitPrice
                //ExtendedPrice
                foreach (Grid grid in lineItemMasterGrid.Children.OfType<Grid>())
                {
                    if (grid.Tag.ToString() == "BasePriceGrid")
                    {
                        try
                        {
                            basePrice = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Base Price is not a number in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString() + "'.", "Error Converting To Decimal\n"+ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    if (grid.Tag.ToString() == "PercentMark")
                    {

                        try
                        {
                            percentMark = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                        }
                        catch
                        {
                            MessageBox.Show("Percent Mark is not a number." + "\n" + "Prices were not saved.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    if (grid.Tag.ToString() == "UnitPrice")
                    {
                        TextBox unitPriceTextBox = grid.Children.OfType<TextBox>().First();
                        try
                        {
                            unitPrice = Convert.ToDecimal(unitPriceTextBox.Text);
                        }
                        catch
                        {
                            MessageBox.Show("Unit Price is not a number." + "\n" + "Prices were not saved.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        //unitPriceOverride = unitPriceTextBox.IsEnabled;
                    }
                    if (grid.Tag.ToString() == "ExtendedPrice")
                    {
                        try
                        {
                            extendedPrice = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                        }
                        catch
                        {
                            MessageBox.Show("Extended Price is not a number." + "\n" + "Prices were not saved.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                    }
                }

                //Comment
                try
                {
                    comment = lineItemMasterGrid.Children.OfType<TextBox>().Where(t => t.Tag.ToString() == "CommentBox").First().Text.ToString();
                }
                catch
                {
                    MessageBox.Show("Could not convert comment to string." + "\n" + "Prices were not saved.", "Error Converting To String", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Options
                StackPanel stack = lineItemMasterGrid.Children.OfType<StackPanel>().First();
                foreach (Grid grid in stack.Children.OfType<Grid>())
                {
                    string option = grid.Children.OfType<TextBlock>().Where(t => t.Text.First() == '(').First().Tag.ToString();
                    bool additive = grid.Children.OfType<TextBlock>().Any(t => t.Text == "$");
                    decimal price = 0;
                    try
                    {
                        price = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                    }
                    catch
                    {
                        MessageBox.Show("Price/Percent for option " + option + " is not a number." + "\n" + "Prices were not saved.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    optionPrices.Add(option, new Tuple<bool, decimal>(additive, price));
                    if (additive)
                    {
                        optionsIncrements += price;
                    }
                    else
                    {
                        optionsPercentage += price;
                    }
                }

                // Enter Data into NAT01
                using (NAT01Context _nat01Context = new NAT01Context())
                {
                    List<QuoteDetailOptions> quoteDetailOptions = _nat01Context.QuoteDetailOptions.Where(q => q.QuoteNumber == quote.QuoteNumber && q.RevisionNo == quote.QuoteRevNo && q.QuoteDetailLineNo == tabLineItemNumber && !string.IsNullOrEmpty(q.OptionCode.Trim())).ToList();
                    foreach (QuoteDetailOptions quoteDetailOption in quoteDetailOptions)
                    {
                        string optionCode = quoteDetailOption.OptionCode.ToString();
                        if (optionPrices.ContainsKey(optionCode))
                        {
                            if (optionPrices[optionCode].Item1)
                            {
                                quoteDetailOption.OrdDetOptPrice = (float)optionPrices[optionCode].Item2;
                            }
                            else
                            {
                                quoteDetailOption.OrdDetOptPercnt = (float)optionPrices[optionCode].Item2;
                            }
                        }
                        else
                        {
                            IMethods.WriteToErrorLog("SaveAllLineItemsButton_Click => Enter Data into NAT01 => { option is not listed on scratchpad but is in [NAT01].[dbo].[QuoteDetailOptions] }", "", user);
                            MessageBox.Show("Failed on Line Item '" + tab.Header.ToString() + "'. Option (" + optionCode + ") is not found on the scratchpad but is in [NAT01].[dbo].[QuoteDetailOptions].", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    QuoteDetails quoteDetails = _nat01Context.QuoteDetails.Where(q => q.QuoteNo == quote.QuoteNumber && q.Revision == quote.QuoteRevNo && q.LineNumber == tabLineItemNumber).First();
                    quoteDetails.UnitPrice = (double)unitPrice;
                    quoteDetails.UnitPriceOverride = true;
                    quoteDetails.ExtendedPrice = (double)extendedPrice;
                    quoteDetails.OptionsIncrements = (float)optionsIncrements;
                    quoteDetails.OptionsPercentage = (float)optionsPercentage;
                    _nat01Context.SaveChanges();
                    _nat01Context.Dispose();
                }

                // Enter Data tnto NAT02
                using (NAT02Context _nat02Context = new NAT02Context())
                {
                    if (_nat02Context.EoiQuoteScratchPad.Any(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == quote.QuoteRevNo && q.LineNo == tabLineItemNumber))
                    {
                        EoiQuoteScratchPad quoteScratchPad = _nat02Context.EoiQuoteScratchPad.Where(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == quote.QuoteRevNo && q.LineNo == tabLineItemNumber && q.LineType.Trim() == tabLineItemType).First();
                        quoteScratchPad.BasePrice = basePrice;
                        quoteScratchPad.PercentMark = percentMark;
                        quoteScratchPad.Comment = comment;
                        quoteScratchPad.User = Environment.UserName;
                        quoteScratchPad.DateTimeStamp = DateTime.Now;
                    }
                    else
                    {
                        EoiQuoteScratchPad quoteScratchPad = new EoiQuoteScratchPad
                        {
                            QuoteNo = quote.QuoteNumber,
                            RevNo = Convert.ToByte(quote.QuoteRevNo),
                            LineNo = tabLineItemNumber,
                            LineType = tabLineItemType,
                            BasePrice = basePrice,
                            PercentMark = percentMark,
                            Comment = comment,
                            User = Environment.UserName,
                            DateTimeStamp = DateTime.Now
                        };
                        _nat02Context.EoiQuoteScratchPad.Add(quoteScratchPad);
                    }
                    _nat02Context.SaveChanges();
                    _nat02Context.Dispose();
                }
                MessageBox.Show("Line Item Prices Updated Successfully", "Success!", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => SaveLineItemButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                MessageBox.Show("Prices Failed to Update.", "Oops!", MessageBoxButton.OK);
            }
        }
        private void SaveAllLineItemsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                foreach (TabItem tab in ScratchPadTabs.Items)
                {
                    try
                    {
                        string tabLineItemType = tab.Name.ToString().Remove(tab.Name.ToString().IndexOf('_'));
                        short tabLineItemNumber = Convert.ToInt16(tab.Name.ToString().Remove(0, tab.Name.ToString().IndexOf('_') + 1));
                        Dictionary<string, Tuple<bool, decimal>> optionPrices = new Dictionary<string, Tuple<bool, decimal>>();
                        decimal basePrice = 0;
                        decimal percentMark = 0;
                        decimal unitPrice = 0;
                        // bool unitPriceOverride = false;
                        decimal extendedPrice = 0;
                        string comment = "";
                        decimal optionsIncrements = 0;
                        decimal optionsPercentage = 0;
                        ScrollViewer scroll = tab.Content as ScrollViewer;
                        Grid lineItemMasterGrid = scroll.Content as Grid;

                        //BasePrice
                        //PercentMark
                        //UnitPrice
                        //ExtendedPrice
                        foreach (Grid grid in lineItemMasterGrid.Children.OfType<Grid>())
                        {
                            if (grid.Tag.ToString() == "BasePriceGrid")
                            {
                                try
                                {
                                    basePrice = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                                }
                                catch
                                {
                                    MessageBox.Show("Base Price is not a number in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString() + "'.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                            if (grid.Tag.ToString() == "PercentMark")
                            {

                                try
                                {
                                    percentMark = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                                }
                                catch
                                {
                                    MessageBox.Show("Percent Mark is not a number in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString() + "'.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                            if (grid.Tag.ToString() == "UnitPrice")
                            {
                                TextBox unitPriceTextBox = grid.Children.OfType<TextBox>().First();
                                try
                                {
                                    unitPrice = Convert.ToDecimal(unitPriceTextBox.Text);
                                }
                                catch
                                {
                                    MessageBox.Show("Unit Price is not a number in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString() + "'.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                // unitPriceOverride = unitPriceTextBox.IsEnabled;
                            }
                            if (grid.Tag.ToString() == "ExtendedPrice")
                            {
                                try
                                {
                                    extendedPrice = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                                }
                                catch
                                {
                                    MessageBox.Show("Extended Price is not a number in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString() + "'.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }

                            }
                        }

                        //Comment
                        try
                        {
                            comment = lineItemMasterGrid.Children.OfType<TextBox>().Where(t => t.Tag.ToString() == "CommentBox").First().Text.ToString();
                        }
                        catch
                        {
                            MessageBox.Show("Could not convert comment to string in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString(), "Error Converting To String", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        //Options
                        StackPanel stack = lineItemMasterGrid.Children.OfType<StackPanel>().First();
                        foreach (Grid grid in stack.Children.OfType<Grid>())
                        {
                            string option = grid.Children.OfType<TextBlock>().Where(t => t.Text.First() == '(').First().Tag.ToString();
                            bool additive = grid.Children.OfType<TextBlock>().Any(t => t.Text == "$");
                            decimal price = 0;
                            try
                            {
                                price = Convert.ToDecimal(grid.Children.OfType<TextBox>().First().Text);
                            }
                            catch
                            {
                                MessageBox.Show("Price/Percent for option " + option + " is not a number in tab '" + tab.Header.ToString() + "'." + "\n" + "Prices were saved in tabs before '" + tab.Header.ToString() + "'.", "Error Converting To Decimal", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            optionPrices.Add(option, new Tuple<bool, decimal>(additive, price));
                            if (additive)
                            {
                                optionsIncrements += price;
                            }
                            else
                            {
                                optionsPercentage += price;
                            }
                        }

                        // Enter Data into NAT01
                        using (NAT01Context _nat01Context = new NAT01Context())
                        {
                            List<QuoteDetailOptions> quoteDetailOptions = _nat01Context.QuoteDetailOptions.Where(q => q.QuoteNumber == quote.QuoteNumber && q.RevisionNo == quote.QuoteRevNo && q.QuoteDetailLineNo == tabLineItemNumber).ToList();
                            foreach (QuoteDetailOptions quoteDetailOption in quoteDetailOptions)
                            {
                                
                                string optionCode = quoteDetailOption.OptionCode.ToString();
                                if (optionPrices.ContainsKey(optionCode))
                                {
                                    if (optionPrices[optionCode].Item1)
                                    {
                                        quoteDetailOption.OrdDetOptPrice = (float)optionPrices[optionCode].Item2;
                                    }
                                    else
                                    {
                                        quoteDetailOption.OrdDetOptPercnt = (float)optionPrices[optionCode].Item2;
                                    }
                                }
                                else
                                {
                                    IMethods.WriteToErrorLog("SaveAllLineItemsButton_Click => Enter Data into NAT01 => { option is not listed on scratchpad but is in [NAT01].[dbo].[QuoteDetailOptions] }", "", user);
                                    MessageBox.Show("Failed on Line Item '" + tab.Header.ToString() + "'. Option ("+ optionCode + ") is not found on the scratchpad but is in [NAT01].[dbo].[QuoteDetailOptions].", "Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }
                            QuoteDetails quoteDetails = _nat01Context.QuoteDetails.Where(q => q.QuoteNo == quote.QuoteNumber && q.Revision == quote.QuoteRevNo && q.LineNumber == tabLineItemNumber).First();
                            quoteDetails.UnitPrice = (double)unitPrice;
                            quoteDetails.UnitPriceOverride = true;
                            quoteDetails.ExtendedPrice = (double)extendedPrice;
                            quoteDetails.OptionsIncrements = (float)optionsIncrements;
                            quoteDetails.OptionsPercentage = (float)optionsPercentage;
                            _nat01Context.Update(quoteDetails);
                            _nat01Context.SaveChanges();
                            _nat01Context.Dispose();
                        }

                        // Enter Data into NAT02
                        using (NAT02Context _nat02Context = new NAT02Context())
                        {
                            if (_nat02Context.EoiQuoteScratchPad.Any(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == quote.QuoteRevNo && q.LineNo == tabLineItemNumber))
                            {
                                EoiQuoteScratchPad quoteScratchPad = _nat02Context.EoiQuoteScratchPad.Where(q => q.QuoteNo == quote.QuoteNumber && q.RevNo == quote.QuoteRevNo && q.LineNo == tabLineItemNumber && q.LineType.Trim() == tabLineItemType).First();
                                quoteScratchPad.BasePrice = basePrice;
                                quoteScratchPad.PercentMark = percentMark;
                                quoteScratchPad.Comment = comment;
                                quoteScratchPad.User = Environment.UserName;
                                quoteScratchPad.DateTimeStamp = DateTime.Now;
                            }
                            else
                            {
                                EoiQuoteScratchPad quoteScratchPad = new EoiQuoteScratchPad
                                {
                                    QuoteNo = quote.QuoteNumber,
                                    RevNo = Convert.ToByte(quote.QuoteRevNo),
                                    LineNo = tabLineItemNumber,
                                    LineType = tabLineItemType,
                                    BasePrice = basePrice,
                                    PercentMark = percentMark,
                                    Comment = comment,
                                    User = Environment.UserName,
                                    DateTimeStamp = DateTime.Now
                                };
                                _nat02Context.EoiQuoteScratchPad.Add(quoteScratchPad);
                            }
                            _nat02Context.SaveChanges();
                            _nat02Context.Dispose();
                        }
                    }
                    catch (Exception ex)
                    {
                        IMethods.WriteToErrorLog("QuoteInfoWindow => SaveAllLineItemsButton_Click (in the foreach); Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                        MessageBox.Show("Prices Failed to Update for '" + tab.Header.ToString() + "'.", "Oops!", MessageBoxButton.OK);
                        return;
                    }
                }
                // Enter Data into NAT01 Quote Header for subtotal and total.
                using (NAT01Context _nat01Context = new NAT01Context())
                {
                    QuoteHeader quoteHeader = _nat01Context.QuoteHeader.First(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo);
                    quoteHeader.QuoteSubtotal = Convert.ToDouble(QuoteSubTotal.Text);
                    quoteHeader.QuoteTotal = Convert.ToDouble(QuoteTotal.Text);
                    _nat01Context.Update(quoteHeader);
                    _nat01Context.SaveChanges();
                    _nat01Context.Dispose();
                }
                MessageBox.Show("Prices Updated Successfully.", "Success!", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => SaveAllLineItemsButton_Click (outside the foreach); Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                MessageBox.Show("Prices Failed to Update.", "Oops!", MessageBoxButton.OK);
            }
        }
        #endregion
        #endregion

        #region Files
        protected static bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;
            if (e != null)
            {
                if ((e.AllowedEffects & DragDropEffects.Copy) == DragDropEffects.Copy)
                {
                    Array data = ((IDataObject)e.Data).GetData("FileName") as Array;
                    if (data != null)
                    {
                        if ((data.Length == 1) && (data.GetValue(0) is String))
                        {
                            filename = ((string[])data)[0];
                            string ext = System.IO.Path.GetExtension(filename).ToLower();
                            if ((ext == ".jpg") || (ext == ".png") || (ext == ".bmp") || (ext == ".pdf") || (ext == ".msg"))
                            {
                                ret = true;
                            }
                        }
                    }
                    else
                    {
                        if (e.Data.GetDataPresent("FileGroupDescriptor"))
                        {
                            Stream theStream = (Stream)e.Data.GetData("FileGroupDescriptor");
                            byte[] fileGroupDescriptor = new byte[512];
                            theStream.Read(fileGroupDescriptor, 0, 512);
                            // used to build the filename from the FileGroupDescriptor block
                            StringBuilder fileName = new StringBuilder("");
                            // this trick gets the filename of the passed attached file
                            for (int i = 76; fileGroupDescriptor[i] != 0; i++)
                            { fileName.Append(Convert.ToChar(fileGroupDescriptor[i])); }
                            theStream.Close();
                            theStream.Dispose();
                            string path = System.IO.Path.GetTempPath();
                            // put the zip file into the temp directory
                            filename = path + fileName.ToString();

                            if (filename.EndsWith(".msg"))
                            {
                                try
                                {
                                    Outlook.Application OL = new Outlook.Application();
                                    for (int i = 1; i <= OL.ActiveExplorer().Selection.Count; i++)
                                    {
                                        Object temp = OL.ActiveExplorer().Selection[i];
                                        if (temp is Outlook.MailItem)
                                        {
                                            Outlook.MailItem mailitem = (temp as Outlook.MailItem);
                                            Outlook.ItemProperties props = mailitem.ItemProperties;
                                            filename = ".msg";
                                        }
                                    }
                                    ret = true;
                                }
                                catch
                                {

                                }
                            }
                            else
                            {
                                // get the actual raw file into memory
                                MemoryStream ms = (MemoryStream)e.Data.GetData(
                                    "FileContents", true);
                                // allocate enough bytes to hold the raw data
                                byte[] fileBytes = new byte[ms.Length];
                                // set starting position at first byte and read in the raw data
                                ms.Position = 0;
                                ms.Read(fileBytes, 0, (int)ms.Length);
                                // create a file and save the raw zip file to it
                                FileStream fs = new FileStream(filename, FileMode.Create);
                                fs.Write(fileBytes, 0, (int)fileBytes.Length);

                                fs.Close();  // close the file

                                FileInfo tempFile = new FileInfo(filename);

                                // always good to make sure we actually created the file
                                if (tempFile.Exists == true)
                                {
                                    // for now, just delete what we created

                                }
                                else
                                { Trace.WriteLine("File was not created!"); }
                            }
                        }
                    }
                }
            }
            return ret;
        }

        #region Events
        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string filename;
            string nameOfFile = ((Grid)sender).Name.Substring(0, ((Grid)sender).Name.Length - 4);

            switch (((Grid)sender).Name)
            {
                case "QuoteReqGrid":
                    nameOfFile = "Quote_Requested";
                    break;
                case "QuoteSentGrid":
                    nameOfFile = "Quote_Sent";
                    break;
                case "POGrid":
                    nameOfFile = "PO";
                    break;
                case "ScratchSheetGrid":
                    nameOfFile = "Scratch_Sheet";
                    break;
                case "MiscGrid":
                    InputBox dialog = new InputBox("Enter name of file:", "File Name", this);
                    dialog.ShowDialog();
                    nameOfFile = dialog.ReturnString.Length > 0 ? dialog.ReturnString : "Misc";
                    break;
                default:
                    break;
            }

            validData = GetFilename(out filename, e);
            string path = @"\\nsql03\data1\Quotes\" + quote.QuoteNumber + @"\";
            if (filename == ".msg")
            {
                Outlook.Application OL = new Outlook.Application();
                for (int i = 1; i <= OL.ActiveExplorer().Selection.Count; i++)
                {
                    Object temp = OL.ActiveExplorer().Selection[i];
                    if (temp is Outlook.MailItem)
                    {
                        Outlook.MailItem mailitem = (temp as Outlook.MailItem);
                        while (System.IO.File.Exists(path + nameOfFile + ".msg"))
                        {
                            InputBox dialog = new InputBox("Enter name of file:", "File Name", this);
                            dialog.ShowDialog();
                            nameOfFile = dialog.ReturnString.Length > 0 ? dialog.ReturnString : nameOfFile;
                        }
                        mailitem.SaveAs(path + nameOfFile + ".msg");
                        MessageBox.Show(this, "Successful drop.");
                    }
                }
            }
            else
            {
                string fp = path + nameOfFile + (System.IO.Path.GetExtension(filename).ToLower().StartsWith(".xl") ? ".xlsx" : System.IO.Path.GetExtension(filename));
                while (System.IO.File.Exists(fp))
                {
                    InputBox dialog = new InputBox("Enter name of file:", "File Exists!", this);
                    dialog.ShowDialog();
                    nameOfFile = dialog.ReturnString.Length > 0 ? dialog.ReturnString : nameOfFile;
                    fp = path + nameOfFile + System.IO.Path.GetExtension(filename);
                }
                System.IO.File.Copy(filename, fp);
                if (filename.Contains(@"\Local\Temp")) { File.Delete(filename); }
                MessageBox.Show(this, "Successful drop.");
            }
        }
        private void Grid_DragEnter(object sender, DragEventArgs e)
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
        private void Grid_DragLeave(object sender, DragEventArgs e)
        {

        }
        private void Grid_DragOver(object sender, DragEventArgs e)
        {

        }
        private void EuropeanTolerancesButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(@"\\engserver\workstations\EuropeanStyleTolerances\EuropeanStyleTolerances.exe");
        }
        private void LinkProjectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (System.IO.Directory.Exists(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + ProjectNumberTextBox.Text))
                {
                    WshInterop.CreateShortcut(@"\\nsql03\data1\Quotes\" + quote.QuoteNumber + @"\" + ProjectNumberTextBox.Text + ".lnk",
                                              "",
                                              @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + ProjectNumberTextBox.Text,
                                              "",
                                              "");
                }

                if (System.IO.Directory.Exists(@"\\nsql03\data1\Quotes\" + quote.QuoteNumber))
                {
                    WshInterop.CreateShortcut(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + ProjectNumberTextBox.Text + @"\" + quote.QuoteNumber + ".lnk",
                                              "",
                                              @"\\nsql03\data1\Quotes\" + quote.QuoteNumber,
                                              "",
                                              "");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => LinkProjectButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
            MessageBox.Show("Successful link.");
        }
        private void LinkOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            try
            {
                if (System.IO.Directory.Exists(@"\\nsql03\data1\WorkOrders\" + OrderNumberTextBox.Text))
                {
                    WshInterop.CreateShortcut(@"\\nsql03\data1\Quotes\" + quote.QuoteNumber + @"\" + OrderNumberTextBox.Text + ".lnk",
                                              "",
                                              @"\\nsql03\data1\WorkOrders\" + OrderNumberTextBox.Text,
                                              "",
                                              "");
                }

                if (System.IO.Directory.Exists(@"\\nsql03\data1\Quotes\" + quote.QuoteNumber))
                {
                    WshInterop.CreateShortcut(@"\\nsql03\data1\WorkOrders\" + OrderNumberTextBox.Text + @"\" + quote.QuoteNumber + ".lnk",
                                              "",
                                              @"\\nsql03\data1\Quotes\" + quote.QuoteNumber,
                                              "",
                                              "");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => LinkOrderButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
            MessageBox.Show("Successful link.");
        }
        #endregion
        #endregion

        #region Order Entry Instructions
        private void ResetInstructionEntering()
        {
            try
            {
                InstructionEnteringTextBox.Text = "";
                InstructionEnteringUserTextBlock.Text = Environment.UserName;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => ResetInstructionEntering; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void FillOrderEntryInstructions(bool isAdding=false, string instruction = null, DateTime? timeEntered = null)
        {
            if (isAdding == false)
            {
                using NAT02Context _nat02Context = new NAT02Context();
                try
                {
                    InstructionsStackPanel.Children.Clear();
                    List<EoiOrderEntryInstructions> eoiOrderEntryInstructions = _nat02Context.EoiOrderEntryInstructions.Where(i => i.QuoteNo == quote.QuoteNumber && i.RevNo == (short)quote.QuoteRevNo).OrderByDescending(instruction => instruction.TimeEntered).ToList();
                    foreach (EoiOrderEntryInstructions eoiOrderEntryInstruction in eoiOrderEntryInstructions)
                    {
                        Grid grid = new Grid();
                        ColumnDefinition col1 = new ColumnDefinition();
                        ColumnDefinition col2 = new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) };
                        ColumnDefinition col3 = new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) };
                        ColumnDefinition col4 = new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) };
                        ColumnDefinition col5 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };
                        grid.ColumnDefinitions.Add(col1);
                        grid.ColumnDefinitions.Add(col2);
                        grid.ColumnDefinitions.Add(col3);
                        grid.ColumnDefinitions.Add(col4);
                        grid.ColumnDefinitions.Add(col5);
                        TextBlock instructionTextBlock = new TextBlock
                        {
                            Margin = new Thickness(2, 0, 0, 0),
                            TextWrapping = TextWrapping.Wrap,
                            Text = eoiOrderEntryInstruction.Instruction,
                            Style = (Style)Application.Current.Resources["NormalTextBlock"]
                        };
                        CheckBox checkBox = new CheckBox
                        {
                            Margin = new Thickness(4, 4, 0, 4),
                            IsChecked = eoiOrderEntryInstruction.Checked,
                            Content = instructionTextBlock,
                            IsEnabled = user.Department == "Order Entry",
                        };
                        checkBox.Click += CheckOrderEntryInstruction;
                        TextBlock userTextBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Text = eoiOrderEntryInstruction.User,
                            Style = (Style)Application.Current.Resources["NormalTextBlock"]
                        };
                        TextBlock enteredTextBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Text = eoiOrderEntryInstruction.TimeEntered.ToString(),
                            Style = (Style)Application.Current.Resources["NormalTextBlock"]
                        };
                        TextBlock checkedTextBlock = new TextBlock
                        {
                            VerticalAlignment = VerticalAlignment.Center,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Text = eoiOrderEntryInstruction.TimeChecked is null ? "" : eoiOrderEntryInstruction.TimeChecked.ToString(),
                            Style = (Style)Application.Current.Resources["NormalTextBlock"]
                        };

                        Border border1 = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 0, 0),
                            Child = userTextBlock
                        };
                        border1.SetValue(Grid.ColumnProperty, 1);

                        Border border2 = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 0, 0),
                            Child = enteredTextBlock
                        };
                        border2.SetValue(Grid.ColumnProperty, 2);

                        Border border3 = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(1, 0, 0, 0),
                            Child = checkedTextBlock
                        };
                        border3.SetValue(Grid.ColumnProperty, 3);

                        Border border4 = new Border
                        {
                            BorderBrush = Brushes.Black,
                            BorderThickness = new Thickness(0, 0, 0, 1)
                        };
                        border4.SetValue(Grid.ColumnSpanProperty, 5);
                        grid.Children.Add(checkBox);
                        grid.Children.Add(border1);
                        grid.Children.Add(border2);
                        grid.Children.Add(border3);
                        if (Environment.UserName == eoiOrderEntryInstruction.User && checkBox.IsChecked!=true)
                        {
                            BitmapImage _bitmapImage = new BitmapImage(new Uri(@"\\engserver\workstations\TOOLING AUTOMATION\redxdeleteicon.png", UriKind.Absolute));
                            Image image = new Image
                            {
                                Source = _bitmapImage,
                                Width = 20,
                                Height = 20,
                                Cursor = Cursors.Hand
                            };
                            image.SetValue(Grid.ColumnProperty, 4);
                            image.MouseUp += RemoveOrderEntryInstructionImage_MouseUp;
                            grid.Children.Add(image);
                        }
                        grid.Children.Add(border4);
                        InstructionsStackPanel.Children.Add(grid);
                    }
                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("QuoteInfoWindow => FillOrderEntryInstructions; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                }
                _nat02Context.Dispose();
            }
            else
            {
                try
                {
                    var children = InstructionsStackPanel.Children.Cast<UIElement>().ToArray();
                    #region Creating Grid To Add
                    Grid grid = new Grid();
                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) };
                    ColumnDefinition col3 = new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) };
                    ColumnDefinition col4 = new ColumnDefinition { Width = new GridLength(150, GridUnitType.Pixel) };
                    ColumnDefinition col5 = new ColumnDefinition { Width = new GridLength(20, GridUnitType.Pixel) };
                    grid.ColumnDefinitions.Add(col1);
                    grid.ColumnDefinitions.Add(col2);
                    grid.ColumnDefinitions.Add(col3);
                    grid.ColumnDefinitions.Add(col4);
                    grid.ColumnDefinitions.Add(col5);
                    TextBlock instructionTextBlock = new TextBlock
                    {
                        Margin = new Thickness(2, 0, 0, 0),
                        TextWrapping = TextWrapping.Wrap,
                        Text = instruction
                    };
                    CheckBox checkBox = new CheckBox
                    {
                        Margin = new Thickness(4, 4, 0, 4),
                        IsChecked = false,
                        IsEnabled = user.Department == "Order Entry",
                        Content = instructionTextBlock
                    };
                    checkBox.Click += CheckOrderEntryInstruction;
                    TextBlock userTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = Environment.UserName,
                        Style = (Style)Application.Current.Resources["NormalTextBlock"]
                    };
                    TextBlock enteredTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = timeEntered.ToString(),
                        Style = (Style)Application.Current.Resources["NormalTextBlock"]
                    };
                    TextBlock checkedTextBlock = new TextBlock
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Text = "",
                        Style = (Style)Application.Current.Resources["NormalTextBlock"]
                    };

                    Border border1 = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        Child = userTextBlock
                    };
                    border1.SetValue(Grid.ColumnProperty, 1);

                    Border border2 = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        Child = enteredTextBlock
                    };
                    border2.SetValue(Grid.ColumnProperty, 2);

                    Border border3 = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(1, 0, 0, 0),
                        Child = checkedTextBlock
                    };
                    border3.SetValue(Grid.ColumnProperty, 3);

                    Border border4 = new Border
                    {
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0, 0, 0, 1)
                    };
                    border4.SetValue(Grid.ColumnSpanProperty, 5);
                    grid.Children.Add(checkBox);
                    grid.Children.Add(border1);
                    grid.Children.Add(border2);
                    grid.Children.Add(border3);
                    BitmapImage _bitmapImage = new BitmapImage(new Uri(@"\\engserver\workstations\TOOLING AUTOMATION\redxdeleteicon.png", UriKind.Absolute));
                    Image image = new Image
                    {
                        Source = _bitmapImage,
                        Width = 20,
                        Height = 20,
                        Cursor = Cursors.Hand
                    };
                    image.SetValue(Grid.ColumnProperty, 4);
                    image.MouseUp += RemoveOrderEntryInstructionImage_MouseUp;
                    grid.Children.Add(image);
                    grid.Children.Add(border4);
                    #endregion

                    InstructionsStackPanel.Children.Clear();
                    InstructionsStackPanel.Children.Add(grid);
                    if (children != null)
                    {
                        foreach (var child in children)
                        {
                            InstructionsStackPanel.Children.Add(child);
                        }
                    }

                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("QuoteInfoWindow => FillOrderEntryInstructions; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
                }
            }
        }
        #region Events
        private void AddOrderEntryInstructionImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DateTime timeEntered = DateTime.Now;
                using NAT02Context _nat02Context = new NAT02Context();
                EoiOrderEntryInstructions eoiOrderEntryInstruction = new EoiOrderEntryInstructions
                {
                    QuoteNo = quote.QuoteNumber,
                    RevNo = (short)quote.QuoteRevNo,
                    Checked = false,
                    Instruction = InstructionEnteringTextBox.Text.ToString().Trim(),
                    User = Environment.UserName,
                    TimeEntered = timeEntered,
                    TimeChecked = null
                };
                _nat02Context.EoiOrderEntryInstructions.Add(eoiOrderEntryInstruction);
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                FillOrderEntryInstructions(true, InstructionEnteringTextBox.Text.ToString().Trim(), timeEntered);
                ResetInstructionEntering();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => AddOrderEntryInstructionImage_MouseUp; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }

        }
        private void RemoveOrderEntryInstructionImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image image = sender as Image;
                Grid grid = image.Parent as Grid;
                CheckBox checkBox = grid.Children.OfType<CheckBox>().First();
                TextBlock instructionTextBlock = checkBox.Content as TextBlock;
                string instruction = instructionTextBlock.Text.ToString();
                using NAT02Context _nat02Context = new NAT02Context();
                EoiOrderEntryInstructions eoiOrderEntryInstruction = _nat02Context.EoiOrderEntryInstructions.OrderBy(i=>i.TimeEntered).First(i => i.QuoteNo == quote.QuoteNumber && i.RevNo == quote.QuoteRevNo && i.Instruction == instruction && i.User==Environment.UserName);
                _nat02Context.EoiOrderEntryInstructions.Remove(eoiOrderEntryInstruction);
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
                FillOrderEntryInstructions();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => RemoveOrderEntryInstructionImage_MouseUp; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void CheckOrderEntryInstruction(object sender, RoutedEventArgs e)
        {
            DateTime timeChecked = DateTime.Now;
            CheckBox checkBox = sender as CheckBox;
            TextBlock textBlock = checkBox.Content as TextBlock;
            string instruction = textBlock.Text.ToString();
            using NAT02Context _nat02Context = new NAT02Context();
            if (checkBox.IsChecked == true)
            {
                EoiOrderEntryInstructions eoiOrderEntryInstruction = _nat02Context.EoiOrderEntryInstructions.First(i => i.Instruction == instruction && i.QuoteNo == quote.QuoteNumber && i.RevNo == quote.QuoteRevNo);
                eoiOrderEntryInstruction.Checked = true;
                eoiOrderEntryInstruction.TimeChecked = timeChecked;
                _nat02Context.Update(eoiOrderEntryInstruction);
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
            }
            else
            {
                EoiOrderEntryInstructions eoiOrderEntryInstruction = _nat02Context.EoiOrderEntryInstructions.First(i => i.Instruction == instruction && i.QuoteNo == quote.QuoteNumber && i.RevNo == quote.QuoteRevNo);
                eoiOrderEntryInstruction.Checked = false;
                eoiOrderEntryInstruction.TimeChecked = null;
                _nat02Context.Update(eoiOrderEntryInstruction);
                _nat02Context.SaveChanges();
                _nat02Context.Dispose();
            }
            FillOrderEntryInstructions();
        }
        #endregion
        #endregion

        #region Events
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            try
            {
                if (QuoteErrorsTabItem.Header.ToString() == "Errors" && errorNotificationPopped == false)
                {
                    QuoteErrorsTabItem.IsSelected = true;
                    Cursor = Cursors.Arrow;
                    MessageBox.Show("This quote has errors, please ensure that they have been reviewed.", "ERRORS", MessageBoxButton.OK, MessageBoxImage.Information);
                    errorNotificationPopped = true;
                }
                else
                {
                    SubmitButton1.IsEnabled = false;
                    RecallButton1.IsEnabled = true;
                    using var context = new NAT01Context();
                    using var nat02Context = new NAT02Context();
                    using var necContext = new NECContext();
                    QuoteHeader r = context.QuoteHeader.Where(q => q.QuoteNo == quote.QuoteNumber && q.QuoteRevNo == quote.QuoteRevNo).FirstOrDefault();
                    string customerName = necContext.Rm00101.Where(c => c.Custnmbr == r.UserAcctNo).First().Custname;
                    string csr = context.QuoteRepresentative.Where(r => r.RepId == quote.QuoteRepID).First().Name;
                    EoiQuotesMarkedForConversion q = new EoiQuotesMarkedForConversion()
                    {
                        QuoteNo = quote.QuoteNumber,
                        QuoteRevNo = quote.QuoteRevNo,
                        CustomerName = customerName,
                        Csr = csr,
                        CsrMarked = user.GetUserName(),
                        TimeSubmitted = DateTime.Now,
                        Rush = r.RushYorN
                    };
                    nat02Context.EoiQuotesMarkedForConversion.Add(q);
                    nat02Context.SaveChanges();
                    nat02Context.Dispose();
                    necContext.Dispose();
                    context.Dispose();
                    parent.BoolValue = true;
                    Cursor = Cursors.Arrow;
                    Close();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => SubmitButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
            Cursor = Cursors.Arrow;
        }
        private void RecallButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            SubmitButton1.IsEnabled = true;
            RecallButton1.IsEnabled = false;
            using var context = new NAT02Context();
            EoiQuotesMarkedForConversion q = new EoiQuotesMarkedForConversion()
            {
                QuoteNo = quote.QuoteNumber,
                QuoteRevNo = quote.QuoteRevNo
            };
            context.EoiQuotesMarkedForConversion.Remove(q);
            context.SaveChanges();
            context.Dispose();
            parent.BoolValue = true;
            Cursor = Cursors.Arrow;
            Close();
        }
        private void QuoteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            if (quote.OrderNo != 0)
            {
                string pathToOrder = @"\\nsql03\data1\WorkOrders\" + quote.OrderNo.ToString().Remove(6);
                if (System.IO.Directory.Exists(pathToOrder))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + pathToOrder + @"""");
                }
                else
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show("This work order folder does not exist.");
                }
            }
            else
            {
                string pathToQuote = @"\\nsql03\data1\Quotes\" + quote.QuoteNumber;
                if (System.IO.Directory.Exists(pathToQuote))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + pathToQuote + @"""");
                }
                else
                {
                    Cursor = Cursors.Arrow;
                    MessageBox.Show("This quote folder does not exist.");
                }
            }
            Cursor = Cursors.Arrow;
        }
        private void WorkOrderButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            using var context = new NAT02Context();
            using NAT01Context nat01context = new NAT01Context();
            try
            {
                string orderNumber = quote.OrderNo.ToString().Substring(0, 6);
                workOrder = new WorkOrder(int.Parse(orderNumber), this);
                WindowCollection collection = App.Current.Windows;
                foreach (Window w in collection)
                {
                    if (w.Title.Contains(workOrder.OrderNumber.ToString()))
                    {
                        context.Dispose();
                        nat01context.Dispose();
                        w.WindowState = WindowState.Normal;
                        this.Close();
                        w.Show();
                        goto AlreadyOpen;
                    }
                }
                if (context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User != user.GetUserName()))
                {
                    MessageBox.Show("BEWARE!!\n" + context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User != user.GetUserName()).FirstOrDefault().User + " is in this order at the moment.");
                }
                else if (context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User == user.GetUserName()))
                {
                    MessageBox.Show("You already have this order open.");
                    context.Dispose();
                    nat01context.Dispose();
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => WorkOrderButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
            try
            {
                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, parent, "", user)
                {
                    Left = Left,
                    Top = Top
                };
                orderInfoWindow.Dispose();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => WorkOrderButton_Click => OrderInfoWindow; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }

        AlreadyOpen:
            context.Dispose();
            nat01context.Dispose();
            Cursor = Cursors.Arrow;
        }
        private void ChecklistCustomerServiceButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pathToMasterPDF = @"\\engserver\workstations\TOOLING AUTOMATION\PO_CHECKLIST.pdf";
                string pathToQuoteFolder = quote.OrderNo != 0 ? @"\\nsql03\data1\WorkOrders\" + quote.OrderNo.ToString().Remove(6) : @"\\nsql03\data1\Quotes\" + quote.QuoteNumber;
                string pathToPOChecklistPDF = pathToQuoteFolder + "\\" + quote.QuoteNumber + "_" + quote.QuoteRevNo + "_PO_CHECKLIST.pdf";
                if (System.IO.File.Exists(pathToPOChecklistPDF))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", pathToPOChecklistPDF);
                }
                else
                {
                    System.IO.File.Copy(pathToMasterPDF, pathToPOChecklistPDF, false);
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", pathToPOChecklistPDF);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => ChecklistCustomerServiceButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void ChecklistOrderEntryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string pathToMasterPDF = @"\\engserver\workstations\TOOLING AUTOMATION\ORDER_ENTRY_PO_CHECKLIST.pdf";
                string pathToQuoteFolder = quote.OrderNo != 0 ? @"\\nsql03\data1\WorkOrders\" + quote.OrderNo.ToString().Remove(6) : @"\\nsql03\data1\Quotes\" + quote.QuoteNumber;
                string pathToPOChecklistPDF = pathToQuoteFolder + "\\" + quote.QuoteNumber + "_" + quote.QuoteRevNo + "_ORDER_ENTRY_PO_CHECKLIST.pdf";
                if (System.IO.File.Exists(pathToPOChecklistPDF))
                {
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + pathToPOChecklistPDF + @"""");
                }
                else
                {
                    System.IO.File.Copy(pathToMasterPDF, pathToPOChecklistPDF, false);
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + pathToPOChecklistPDF + @"""");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => ChecklistOrderEntryButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void MasterGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeLineItemScrollerHeight();
            ChangeSMIScrollHeights();
        }
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedTab = tabControl.SelectedItem as TabItem;
            string header = selectedTab.Header.ToString();
            if (header == "Quote")
            {
                FillQuoteInfoPage();
                ChangeLineItemScrollerHeight();
            }
            if (header == "Price/SMI Check")
            {
                //FillSMIAndScratchPadPage();
                ChangeSMIScrollHeights();
            }
            if (header == "Order Entry Instructions")
            {
                //ResetInstructionEntering();
                //FillOrderEntryInstructions();
            }
        }
        private void BillToButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var nat01context = new NAT01Context();
                string mach = nat01context.MachineList.Where(m => m.MachineNo == quoteLineItems[0].MachineNo).Select(m => m.MachineNo.ToString().Trim() + "-" + m.Description.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim()).FirstOrDefault();
                nat01context.Dispose();
                string folderName = @"\\engserver\workstations\tools\Customers\" + quote.CustomerNo + " - " + quote.BillToName.ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\"; // orderLineItems[lineItemNumber - 1].MachineNo + "-" + orderLineItems[lineItemNumber - 1].MachineDescription.Trim().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                if (System.IO.Directory.Exists(folderName))
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                    IMethods.BringProcessToFront(process);
                }
                else
                {
                    folderName = @"\\engserver\workstations\tools\Customers\" + quote.CustomerNo + " - " + quote.BillToName.ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                    if (System.IO.Directory.Exists(folderName))
                    {
                        System.Diagnostics.Process process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                        IMethods.BringProcessToFront(process);
                    }
                    else
                    {
                        folderName = @"\\engserver\workstations\tools\Customers\" + quote.CustomerNo + " - " + quote.BillToName.ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\";
                        System.IO.Directory.CreateDirectory(folderName);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => BillToButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void ShipToButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var nat01context = new NAT01Context();
                string mach = nat01context.MachineList.Where(m => m.MachineNo == quoteLineItems[0].MachineNo).Select(m => m.MachineNo.ToString().Trim() + "-" + m.Description.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim()).FirstOrDefault();
                nat01context.Dispose();
                string folderName = @"\\engserver\workstations\tools\Customers\" + quote.ShipToAccountNo + " - " + quote.ShiptoName.ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\"; // orderLineItems[lineItemNumber - 1].MachineNo + "-" + orderLineItems[lineItemNumber - 1].MachineDescription.Trim().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                if (System.IO.Directory.Exists(folderName))
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                    IMethods.BringProcessToFront(process); 
                }
                else
                {
                    folderName = @"\\engserver\workstations\tools\Customers\" + quote.ShipToAccountNo + " - " + quote.ShiptoName.ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                    if (System.IO.Directory.Exists(folderName))
                    {
                        System.Diagnostics.Process process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                        IMethods.BringProcessToFront(process);
                    }
                    else
                    {
                        folderName = @"\\engserver\workstations\tools\Customers\" + quote.ShipToAccountNo + " - " + quote.ShiptoName.ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\";
                        System.IO.Directory.CreateDirectory(folderName);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => ShipToButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var nat01context = new NAT01Context();
                string mach = nat01context.MachineList.Where(m => m.MachineNo == quoteLineItems[0].MachineNo).Select(m => m.MachineNo.ToString().Trim() + "-" + m.Description.Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim()).FirstOrDefault();
                nat01context.Dispose();
                string folderName = @"\\engserver\workstations\tools\Customers\" + quote.UserAcctNo + " - " + GetEndUserName().ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\"; // orderLineItems[lineItemNumber - 1].MachineNo + "-" + orderLineItems[lineItemNumber - 1].MachineDescription.Trim().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                if (System.IO.Directory.Exists(folderName))
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                    IMethods.BringProcessToFront(process);
                }
                else
                {
                    folderName = @"\\engserver\workstations\tools\Customers\" + quote.UserAcctNo + " - " + GetEndUserName().ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim();
                    if (System.IO.Directory.Exists(folderName))
                    {
                        System.Diagnostics.Process process = System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + folderName + @"""");
                        IMethods.BringProcessToFront(process);
                    }
                    else
                    {
                        folderName = @"\\engserver\workstations\tools\Customers\" + quote.UserAcctNo + " - " + GetEndUserName().ToString().Replace("/", "-").Replace("*", "").Replace(":", " ").Trim('.').Trim() + "\\" + mach + "\\";
                        System.IO.Directory.CreateDirectory(folderName);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("QuoteInfoWindow => UserButton_Click; Quote: " + quote.QuoteNumber + "-" + quote.QuoteRevNo, ex.Message, user);
            }
        }
        private void TrackButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentTrackingWindow documentTrackingWindow = new DocumentTrackingWindow(quote, user);
            documentTrackingWindow.ShowDialog();
        }
        #endregion
    }
}
