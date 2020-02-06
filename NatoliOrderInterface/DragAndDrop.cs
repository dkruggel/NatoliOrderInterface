using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
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
    class DragAndDrop : Window
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
        public double DepthOfShadow;
        public double DirectionOfDropShadow;
        public Window DragDropWindow = null;
        public ObservableCollection<TextBlock> TextBlocks = new ObservableCollection<TextBlock>();
        private TextBlock DraggedTextBlock = null;
        public ListBox ListBox = new ListBox();


        public DragAndDrop(ListBox listBox, ObservableCollection<TextBlock> textBlocks, double depthOfShadow = 4, double directionOfDropShadow = 315)
        {
            DirectionOfDropShadow = directionOfDropShadow;
            DepthOfShadow = depthOfShadow;
            TextBlocks = textBlocks;
            ListBox = listBox;
            System.Windows.Style itemContainerStyle = new System.Windows.Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewDragLeaveEvent, new DragEventHandler(ListBoxItem_PreviewDragLeave)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Dropping)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.GiveFeedbackEvent, new GiveFeedbackEventHandler(ListBoxItem_DraggingFeedback)));
            ListBox.ItemContainerStyle = itemContainerStyle;
            ListBox.ItemsSource = TextBlocks;
        }

        public void CreateDragDropWindow(Visual dragElement)
        {
            this.DragDropWindow = new Window();
            DragDropWindow.WindowStyle = WindowStyle.None;
            DragDropWindow.AllowsTransparency = true;
            DragDropWindow.AllowDrop = false;
            DragDropWindow.Background = null;
            DragDropWindow.IsHitTestVisible = false;
            DragDropWindow.SizeToContent = SizeToContent.WidthAndHeight;
            DragDropWindow.Topmost = true;
            DragDropWindow.ShowInTaskbar = false;

            Rectangle r = new Rectangle();
            r.Width = ((FrameworkElement)dragElement).ActualWidth + Math.Abs(DepthOfShadow * Math.Cos(DirectionOfDropShadow));
            r.Height = ((FrameworkElement)dragElement).ActualHeight + Math.Abs(DepthOfShadow * Math.Sin(DirectionOfDropShadow));
            r.Fill = new VisualBrush(dragElement);
            this.DragDropWindow.Content = r;


            Win32Point w32Mouse = new Win32Point();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GetCursorPos(ref w32Mouse);
            }
            else
            {
                // How to do on Linux and OSX?
            }


            this.DragDropWindow.Left = w32Mouse.X;
            this.DragDropWindow.Top = w32Mouse.Y;
            this.DragDropWindow.Show();
        }
        public void ListBoxItem_PreviewDragLeave(object sender, DragEventArgs e)
        {
            DraggedTextBlock = ((sender as ListBoxItem).Content) as TextBlock;
        }

        public void ListBoxItem_DraggingFeedback(object sender, GiveFeedbackEventArgs e)
        {
            DraggedTextBlock = ((sender as ListBoxItem).Content) as TextBlock;
            if (e.Effects == DragDropEffects.Move)
            {
                e.UseDefaultCursors = false;
                Mouse.SetCursor(Cursors.SizeAll);
                if (DragDropWindow == null)
                {
                    CreateDragDropWindow(DraggedTextBlock);
                    DraggedTextBlock.Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 70, R = 0, G = 0, B = 0 },
                        Direction = DirectionOfDropShadow,
                        ShadowDepth = DepthOfShadow,
                        Opacity = .75,
                    };
                }
                else
                {
                    Win32Point w32Mouse = new Win32Point();
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        GetCursorPos(ref w32Mouse);
                    }
                    else
                    {
                        // How to do on Linux and OSX?
                    }

                    this.DragDropWindow.Left = w32Mouse.X;
                    this.DragDropWindow.Top = w32Mouse.Y;
                }
            }
            else if (e.Effects == DragDropEffects.None)
            {
                if (CloseWindow())
                {
                    foreach (UIElement uIElement in ListBox.Items)
                    {
                        uIElement.ClearValue(EffectProperty);
                    }
                    return;
                }
            }
            else
            {
                e.UseDefaultCursors = true;
            }
            e.Handled = true;
        }

        public void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                TextBlock textBlock = draggedItem.Content as TextBlock;
                textBlock.Effect = new DropShadowEffect
                {
                    Color = new Color { A = 70, R = 0, G = 0, B = 0 },
                    Direction = DirectionOfDropShadow,
                    ShadowDepth = DepthOfShadow,
                    Opacity = .75,
                };
                draggedItem.IsSelected = true;
                CreateDragDropWindow(textBlock);
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
            }
        }

        public void ListBoxItem_Dropping(object sender, DragEventArgs e)
        {
            TextBlock droppedData = e.Data.GetData(typeof(TextBlock)) as TextBlock;
            TextBlock target = ((ListBoxItem)(sender)).DataContext as TextBlock;

            int removedIdx = ListBox.Items.IndexOf(droppedData);
            int targetIdx = ListBox.Items.IndexOf(target);

            if (removedIdx < targetIdx)
            {
                TextBlocks.Insert(targetIdx + 1, droppedData);
                TextBlocks.RemoveAt(removedIdx);
            }
            else
            {
                int remIdx = removedIdx + 1;
                if (TextBlocks.Count + 1 > remIdx)
                {

                    TextBlocks.Insert(targetIdx, droppedData);
                    TextBlocks.RemoveAt(remIdx);
                }
            }


            CloseWindow();
            droppedData.ClearValue(EffectProperty);
            Mouse.SetCursor(Cursors.Arrow);
        }
        public bool CloseWindow()
        {
            if (this.DragDropWindow != null)
            {
                this.DragDropWindow.Close();
                this.DragDropWindow = null;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
