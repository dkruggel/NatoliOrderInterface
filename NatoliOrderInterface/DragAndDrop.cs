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
    class DragAndDrop : Window , IMethods
    {
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
        private User user;


        public DragAndDrop(User _user, ListBox _listBox, ObservableCollection<TextBlock> _textBlocks, double depthOfShadow = 4, double directionOfDropShadow = 315)
        {
            user = _user;
            try
            {

                DirectionOfDropShadow = directionOfDropShadow;
                DepthOfShadow = depthOfShadow;
                TextBlocks = _textBlocks;
                ListBox = _listBox;
                System.Windows.Style itemContainerStyle = new System.Windows.Style(typeof(ListBoxItem));
                itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewDragLeaveEvent, new DragEventHandler(ListBoxItem_PreviewDragLeave)));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Dropping)));
                itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.GiveFeedbackEvent, new GiveFeedbackEventHandler(ListBoxItem_DraggingFeedback)));
                ListBox.ItemContainerStyle = itemContainerStyle;
                ListBox.ItemsSource = TextBlocks;
            }
            catch(Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs", ex.Message, user);
            }
        }

        public void CreateDragDropWindow(Visual dragElement)
        {
            try
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
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => CreateDragDropWindow", ex.Message, user);
            }
        }
        public void ListBoxItem_PreviewDragLeave(object sender, DragEventArgs e)
        {
            try
            {
                DraggedTextBlock = ((sender as ListBoxItem).Content) as TextBlock;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => ListBoxItem_PreviewDragLeave", ex.Message, user);
            }
        }

        public void ListBoxItem_DraggingFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => ListBoxItem_DraggingFeedback", ex.Message, user);
            }
        }

        public void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => ListBoxItem_PreviewMouseMove", ex.Message, user);
            }
        }

        public void ListBoxItem_Dropping(object sender, DragEventArgs e)
        {
            try
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
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => ListBoxItem_Dropping", ex.Message, user);
            }
        }
        public bool CloseWindow()
        {
            try
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
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => CloseWindow", ex.Message, user);
                return false;
            }
        }
    }
}
