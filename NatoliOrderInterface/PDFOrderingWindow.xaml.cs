using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NatoliOrderInterface
{
    public partial class PDFOrderingWindow : Window, IMethods
    {
        ObservableCollection<TextBlock> textBlockList = new ObservableCollection<TextBlock>();
        public PDFOrderingWindow(List<string> names)
        {
            InitializeComponent();
            this.Show();
            foreach (string name in names)
            {
                textBlockList.Add(new TextBlock { Text = name });
            }
            ListBox1.ItemsSource = textBlockList;
            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Drop)));
            ListBox1.ItemContainerStyle = itemContainerStyle;
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
    }
}
