using iText.IO.Image;
using iText.Kernel.Pdf;
using iText.Layout;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Linq;

namespace NatoliOrderInterface
{
    public partial class PDFOrderingWindow : Window, IMethods
    {
        private readonly ObservableCollection<TextBlock> textBlockList = new ObservableCollection<TextBlock>();
        private readonly string directory = "";
        private readonly User user = null;
        private readonly WorkOrder workOrder = null;
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
                string lineItemName = file.Substring(file.LastIndexOf("\\") + 1, file.IndexOf(".pdf") - file.LastIndexOf("\\") - 1);
                int lineItemNumber = Math.Max(99, filesDict.Count == 0 ? 0 : filesDict.Keys.Max()) + 1;
                if (workOrder.lineItems.Any(l => lineItemName.StartsWith(l.Value)))
                {
                    lineItemNumber = workOrder.lineItems.Where(l => lineItemName.StartsWith(l.Value)).First().Key;
                }
                filesDict.Add(lineItemNumber, lineItemName);
            }
            foreach (KeyValuePair<int, string> keyValuePair in filesDict.OrderBy(kvp=>kvp.Key))
            {
                textBlockList.Add(new TextBlock { Text = keyValuePair.Value });
            }
            ListBox1.ItemsSource = textBlockList;
            System.Windows.Style itemContainerStyle = new System.Windows.Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Drop)));
            ListBox1.ItemContainerStyle = itemContainerStyle;
            this.Show();
        }

        private void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void ListBoxItem_Drop(object sender, DragEventArgs e)
        {
            // This needs to be "Data" to trigger "DataContext" changed and update the view from the changed itemsource.
            TextBlock droppedData = e.Data.GetData(typeof(TextBlock)) as TextBlock;
            TextBlock target = ((ListBoxItem)(sender)).DataContext as TextBlock;

            int removedIdx = ListBox1.Items.IndexOf(droppedData);
            int targetIdx = ListBox1.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                textBlockList.Insert(targetIdx + 1, droppedData);
                textBlockList.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (textBlockList.Count + 1 > remIdx)
                {
                    textBlockList.Insert(targetIdx, droppedData);
                    textBlockList.RemoveAt(remIdx);
                }
            }
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
                foreach (TextBlock textBlock in textBlockList)
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
