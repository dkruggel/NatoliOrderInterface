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

namespace NatoliOrderInterface
{
    public partial class PDFOrderingWindow : Window, IMethods
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
        public PDFOrderingWindow(List<string> filePaths, OrderInfoWindow orderInfoWindow, WorkOrder workOrder, User user)
        {
            Owner = orderInfoWindow;
            InitializeComponent();
            directory = filePaths[0].Remove(filePaths[0].LastIndexOf("\\"));
            this.user = user;
            this.workOrder = workOrder;

            Dictionary<int,string> filesDict = new Dictionary<int, string>();
            foreach (string file in filePaths)
            {
                int metric = 0;
                string lineItemName = file.Substring(file.LastIndexOf("\\") + 1, file.IndexOf(".pdf") - file.LastIndexOf("\\") - 1);
                if (lineItemName.EndsWith("_M"))
                {
                    lineItemName = lineItemName.Remove(lineItemName.Length - 2);
                    metric++;
                }
                int lineItemNumber = Math.Max(99, filesDict.Count == 0 ? 0 : filesDict.Keys.Max()) + 1;
                if (workOrder.lineItems.Any(l => lineItemName.StartsWith(l.Value)))
                {
                    lineItemNumber = workOrder.lineItems.Where(l => lineItemName.StartsWith(l.Value)).First().Key;
                    lineItemNumber = lineItemNumber * 2 + metric;
                }
                filesDict.Add(lineItemNumber, lineItemName + (metric == 1 ? "_M" : ""));
            }
            foreach (KeyValuePair<int, string> keyValuePair in filesDict.OrderBy(kvp=>kvp.Key))
            {
                textBlockList.Add(new TextBlock { Text = keyValuePair.Value });
            }

            DragAndDrop dragAndDrop = new DragAndDrop(user,ListBox1, textBlockList);
            this.Show();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string tempFile = "";
                int file_count = 1;
                foreach (TextBlock textBlock in ListBox1.ItemsSource)
                {
                    string file = directory + "\\" + textBlock.Text.ToString() + ".pdf";
                    string woFolderName = directory.Remove(0, directory.LastIndexOf("\\") + 1);
                    if (tempFile == "")
                    {
                        string[] filesAlreadyInDirectory = Directory.GetFiles(@"C:\Users\" + user.DomainName + @"\Desktop\WorkOrdersToPrint\", "*" + woFolderName + "*");
                        foreach (string fileToDelete in filesAlreadyInDirectory)
                        {
                            File.Delete(fileToDelete);
                        }
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
                    string lineItemName = file.GetFileNameFromPath();
                    File.Copy(file, @"C:\Users\" + user.DomainName + @"\Desktop\WorkOrdersToPrint\" + woFolderName + "_" + file_count + ".pdf", true);
                    file_count++;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("PDFOrderingWindow => OK_Click", ex.Message, user);
            }
            this.Close();
        }
    }
}
