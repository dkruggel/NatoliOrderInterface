using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using NatoliOrderInterface.Models;

namespace NatoliOrderInterface
{
    public partial class OrderingWindow : Window, IMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        private readonly ObservableCollection<TextBlock> textBlockList = new ObservableCollection<TextBlock>();
        private readonly string directory = "";
        private readonly User user = null;
        private readonly WorkOrder workOrder = null;
        private Window _dragdropWindow = null;
        private List<Tuple<int, short>> quotesList = new List<Tuple<int, short>>();
        public List<Tuple<int, short>> QuotesList 
        {
            get 
            {
                return quotesList;
            }
        }
        /// <summary>
        /// Orders files and moves them into "C:\Users\[userName]\Desktop\WorkOrdersToPrint\" to be printed in order later.
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="user"></param>
        /// <param name="orderInfoWindow"></param>
        /// <param name="workOrder"></param>
        public OrderingWindow(List<string> filePaths, User user, OrderInfoWindow orderInfoWindow, WorkOrder workOrder)
        {
            this.workOrder = workOrder;

            InitializeComponent();
            directory = filePaths[0].Remove(filePaths[0].LastIndexOf("\\"));
            this.user = user;

            Dictionary<int, string> filesDict = new Dictionary<int, string>();

            foreach (string file in filePaths)
            {
                int metric = 0;
                string lineItemName = System.IO.Path.GetFileNameWithoutExtension(file);

                if (lineItemName.Contains("_M"))
                {
                    lineItemName = lineItemName.Remove(lineItemName.IndexOf("_M"), 2);
                    metric++;
                }
                if (lineItemName.Contains("_"))
                {
                    lineItemName = lineItemName.Remove(lineItemName.IndexOf("_"));
                }
                int lineItemNumber = Math.Max(99, filesDict.Count == 0 ? 0 : filesDict.Keys.Max()) + 1;
                if (workOrder.lineItems.Any(l => IMethods.lineItemTypeToDescription[l.Value].Contains(' ') ?
                IMethods.lineItemTypeToDescription[l.Value].Remove(IMethods.lineItemTypeToDescription[l.Value].IndexOf(' ')) == lineItemName :
                IMethods.lineItemTypeToDescription[l.Value] == lineItemName))
                {
                    lineItemNumber = workOrder.lineItems.First(l => IMethods.lineItemTypeToDescription[l.Value].Contains(' ') ?
                IMethods.lineItemTypeToDescription[l.Value].Remove(IMethods.lineItemTypeToDescription[l.Value].IndexOf(' ')) == lineItemName :
                IMethods.lineItemTypeToDescription[l.Value] == lineItemName).Key;
                    lineItemNumber = lineItemNumber * 2 + metric;
                }
                filesDict.Add(lineItemNumber, lineItemName + (metric == 1 ? "_M" : ""));
            }
            foreach (KeyValuePair<int, string> keyValuePair in filesDict.OrderBy(kvp => kvp.Key))
            {
                textBlockList.Add(new TextBlock { Text = keyValuePair.Value });
            }


            DragAndDrop dragAndDrop = new DragAndDrop(user, ListBox1, textBlockList);
            this.Show();


            ButtonGrid.Children.Clear();
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
            ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            Button signAndMoveButton = new Button { Name = "SignAndMove", Content = "Sign and Move", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
            Grid.SetColumn(signAndMoveButton, 0);
            signAndMoveButton.Click += SignAndMove_Click;
            Button moveOnlyButton = new Button { Name = "MoveOnly", Content = "Move Only", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
            Grid.SetColumn(moveOnlyButton, 1);
            moveOnlyButton.Click += MoveOnly_Click;
            Button cancelButton = new Button { Name = "Cancel", Content = "Cancel", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
            Grid.SetColumn(cancelButton, 2);
            cancelButton.Click += Cancel_Click;
            ButtonGrid.Children.Add(signAndMoveButton);
            ButtonGrid.Children.Add(moveOnlyButton);
            ButtonGrid.Children.Add(cancelButton);

        }
        /// <summary>
        /// Orders WorkOrderNumber file paths in a folder
        /// </summary>
        /// <param name="filePaths"></param>
        /// <param name="user"></param>
        /// <param name="mainWindow"></param>
        public OrderingWindow(List<string> filePaths, User user, MainWindow mainWindow)
        {
            InitializeComponent();
            this.user = user;

            directory = filePaths[0].Remove(filePaths[0].LastIndexOf("\\"));
            Dictionary<int, string> filesDict = new Dictionary<int, string>();

            foreach (string file in filePaths.OrderBy(t => t.ToString()))
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                textBlockList.Add(new TextBlock { Text = fileName });
            }

            DragAndDrop dragAndDrop = new DragAndDrop(user, ListBox1, textBlockList);
            this.Show();
            Header.Text = "Order Quotes";
            ButtonGrid.Children.Clear();
            ButtonGrid.ColumnDefinitions.Clear();
            ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
            Button reorderButton = new Button { Name = "Reorder", Content = "Reorder", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
            Grid.SetColumn(reorderButton, 0);
            reorderButton.Click += Reorder_Click;
            Button cancelButton = new Button { Name = "Cancel", Content = "Cancel", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
            Grid.SetColumn(cancelButton, 1);
            cancelButton.Click += Cancel_Click;
            ButtonGrid.Children.Add(reorderButton);
            ButtonGrid.Children.Add(cancelButton);
        }
        /// <summary>
        /// Returns ordered list of quotes. Should call ShowDialog() to get the ordered list back.
        /// </summary>
        /// <param name="quotes"></param>
        /// <param name="user"></param>
        /// <param name="mainWindow"></param>
        public OrderingWindow(List<Tuple<int, short>> quotes, User user)
        {
            InitializeComponent();
            try
            {
                this.user = user;
                quotesList = quotes;
                // Populate the textblocks for the listbox
                foreach (Tuple<int, short> quote in quotesList)
                {
                    using var _nat02Context = new NAT02Context();
                    string customer = _nat02Context.EoiQuotesNotConvertedView.First(q => q.QuoteNo == (double)quote.Item1 && q.QuoteRevNo == quote.Item2).CustomerName;
                    _nat02Context.Dispose();
                    string q = quote.Item1 + "-" + quote.Item2 + "  -  " + customer;
                    textBlockList.Add(new TextBlock { Text = q });
                }
                DragAndDrop dragAndDrop = new DragAndDrop(user, ListBox1, textBlockList);
                ButtonGrid.Children.Clear();
                ButtonGrid.ColumnDefinitions.Clear();
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100) });
                Button submitButton = new Button { Name = "Submit", Content = "Submit", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
                Grid.SetColumn(submitButton, 0);
                submitButton.Click += SubmitQuotes_Click;
                Button cancelButton = new Button { Name = "Cancel", Content = "Cancel", VerticalAlignment = VerticalAlignment.Center, MinHeight = 24, Style = (System.Windows.Style)Application.Current.Resources["Button"] };
                Grid.SetColumn(cancelButton, 1);
                cancelButton.Click += Cancel_Click;
                ButtonGrid.Children.Add(submitButton);
                ButtonGrid.Children.Add(cancelButton);
            }
            catch
            {

                this.Close();
            }
        }

        private void MovePDFsWOptionToSign(bool toBeSigned)
        {
            string tempFile = "";
            int file_count = 1;
            string[] oldFiles = Directory.GetFiles(directory);
            string userName;
            if (user.DomainName == "dsachuk") { userName = "dsachuk.NATOLI"; } else { userName = user.DomainName; }
            foreach (TextBlock textBlock in ListBox1.ItemsSource)
            {
                string file = directory + "\\" + textBlock.Text.ToString() + ".pdf";
                string woFolderName = directory.Remove(0, directory.LastIndexOf("\\") + 1);
                // Delete files already in the folder to move to on first loop
                if (tempFile == "")
                {
                    string[] filesAlreadyInDirectory = Directory.GetFiles(@"C:\Users\" + userName + @"\Desktop\WorkOrdersToPrint\", "*" + woFolderName + "*");
                    foreach (string fileToDelete in filesAlreadyInDirectory)
                    {
                        File.Delete(fileToDelete);
                    }
                }
                
                tempFile = System.IO.Path.GetTempFileName();
                if (toBeSigned)
                {
                    string oldFile = oldFiles.First(path => System.IO.Path.GetFileName(path).StartsWith(textBlock.Text.ToString()) && System.IO.Path.GetFileName(path).Contains("_M") == textBlock.Text.ToString().Contains("_M"));
                    PdfDocument pdfDocument = new PdfDocument(new PdfReader(oldFile), new PdfWriter(tempFile));
                    int page_count = pdfDocument.GetNumberOfPages();
                    Document document = new Document(pdfDocument);
                    for (int i = 1; i <= page_count; i++)
                    {
                        ImageData imageData = ImageDataFactory.Create(@"C:\Users\" + userName + @"\Desktop\John Hancock.png");
                        iText.Layout.Element.Image image = new iText.Layout.Element.Image(imageData).ScaleAbsolute(22, 22)
                                                                                                    .SetFixedPosition(i, user.SignatureLeft, user.SignatureBottom);
                        document.Add(image);
                    }
                    document.Close();
                    File.Move(tempFile, file, true);
                    File.Copy(file, @"C:\Users\" + userName + @"\Desktop\WorkOrdersToPrint\" + woFolderName + "_" + file_count + ".pdf", true);
                    if (file != oldFile)
                    {
                        File.Delete(oldFile);
                    }
                }
                else
                {
                    File.Copy(file, @"C:\Users\" + userName + @"\Desktop\WorkOrdersToPrint\" + woFolderName + "_" + file_count + ".pdf", true);
                }
                file_count++;
            }
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
        private void SignAndMove_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MovePDFsWOptionToSign(true);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("PDFOrderingWindow => SignAndMove_Click", ex.Message, user);
                MessageBox.Show(ex.Message);
            }
            this.Close();
        }
        private void Reorder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int i = 1;
                foreach (TextBlock textBlock in textBlockList)
                {
                    string from = directory + textBlock.Text + ".pdf";
                    string to = directory + i + "-" + textBlock.Text + ".pdf";
                    File.Move(from, to);
                    i++;
                }
                i = 1;
                foreach (string path in Directory.GetFiles(directory).OrderBy(s => s))
                {
                    string name = System.IO.Path.GetFileNameWithoutExtension(path);
                    string from = path;
                    string to = directory + name.Replace('-', '_') + ".pdf";
                    File.Move(from, to);
                    i++;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("PDFOrderingWindow => Reorder_Click", ex.Message, user);
                MessageBox.Show(ex.Message);
            }
            this.Close();
        }
        private void MoveOnly_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MovePDFsWOptionToSign(false);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("PDFOrderingWindow => SignAndMove_Click", ex.Message, user);
                MessageBox.Show(ex.Message);
            }
            this.Close();
        }
        private void SubmitQuotes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                quotesList.Clear();
                foreach (TextBlock textBlock in ListBox1.ItemsSource)
                {
                    string[] quote = textBlock.Text.Split('-');
                    quotesList.Add(new Tuple<int, short>(Convert.ToInt32(quote[0]), Convert.ToInt16(quote[1].Trim())));
                }
                this.DialogResult = true;
                this.Close();
            }
            catch(Exception ex)
            {
                IMethods.WriteToErrorLog("OrderingWindow.xaml.cs => SubmitQuotes_Click()", ex.Message, user);
            }
        }
    }
}