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
using System.Linq;
using System.Threading.Tasks;
using NatoliOrderInterface.Models;
using System.Windows.Media.Animation;

namespace NatoliOrderInterface
{
    class DragAndDrop : Window , IMethods
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
        private Label DraggedLabel = null;
        public ListBox ListBox = new ListBox();
        public Grid Grid = new Grid();
        public WrapPanel WrapPanel = new WrapPanel();
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

        public DragAndDrop(User _user, Grid _grid, double depthOfShadow = 4, double directionOfDropShadow = 315)
        {
            user = _user;
            try
            {

                DirectionOfDropShadow = directionOfDropShadow;
                DepthOfShadow = depthOfShadow;
                Grid = _grid;
                System.Windows.Style gridStyle = new System.Windows.Style(typeof(Grid));
                WrapPanel = Grid.Parent as WrapPanel;
                System.Windows.Style wrapPanelStyle = new System.Windows.Style(typeof(WrapPanel));
                System.Windows.Style mainWindowStyle = new System.Windows.Style(typeof(MainWindow));
                //wrapPanelStyle.Setters.Add(new EventSetter(WrapPanel.DropEvent, new DragEventHandler(Grid_Dropping)));
                mainWindowStyle.Setters.Add(new EventSetter(MainWindow.DropEvent, new DragEventHandler(Grid_Dropping)));
                gridStyle.Setters.Add(new Setter(Grid.AllowDropProperty, true));
                gridStyle.Setters.Add(new EventSetter(Grid.PreviewDragLeaveEvent, new DragEventHandler(Grid_PreviewDragLeave)));
                gridStyle.Setters.Add(new EventSetter(Grid.PreviewMouseMoveEvent, new MouseEventHandler(Grid_PreviewMouseMove)));
                //gridStyle.Setters.Add(new EventSetter(Grid.DropEvent, new DragEventHandler(Grid_Dropping)));
                gridStyle.Setters.Add(new EventSetter(Grid.GiveFeedbackEvent, new GiveFeedbackEventHandler(Grid_DraggingFeedback)));
                Grid.Style = gridStyle;
                WrapPanel.Style = wrapPanelStyle;
                Application.Current.MainWindow.Style = mainWindowStyle;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs", ex.Message, user);
            }
        }

        public void CreateDragDropWindow(Visual dragElement)
        {
            try
            {
                DragDropWindow = new Window();
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
        public void Grid_PreviewDragLeave(object sender, DragEventArgs e)
        {
            try
            {
                DraggedLabel = ((sender as Grid).Children.OfType<Label>().First()) as Label;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => Grid_PreviewDragLeave", ex.Message, user);
            }
        }
        public void Grid_DraggingFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                DraggedLabel = ((sender as Grid).Children.OfType<Label>().First()) as Label;
                if (e.Effects == DragDropEffects.Move)
                {
                    e.UseDefaultCursors = false;
                    Mouse.SetCursor(Cursors.SizeAll);
                    if (DragDropWindow == null)
                    {
                        CreateDragDropWindow(DraggedLabel);
                        DraggedLabel.Effect = new DropShadowEffect
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
                        foreach (UIElement uIElement in Grid.Children)
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
                IMethods.WriteToErrorLog("DragAndDrop.cs => Grid_DraggingFeedback", ex.Message, user);
            }
        }
        public void Grid_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && sender is Grid)
                {
                    Grid draggedItem = sender as Grid;
                    Label label = draggedItem.Children[0] as Label;
                    label.Effect = new DropShadowEffect
                    {
                        Color = new Color { A = 70, R = 0, G = 0, B = 0 },
                        Direction = DirectionOfDropShadow,
                        ShadowDepth = DepthOfShadow,
                        Opacity = .75,
                    };
                    //draggedItem.IsSelected = true;
                    (sender as Grid).Visibility = Visibility.Collapsed;
                    List<Button> buttons = (Application.Current.MainWindow as MainWindow).MenuDock.Children.OfType<Button>().ToList();
                    Button button = buttons.Single(b => b.Name == "RemoveModuleButton");
                    button.Visibility = Visibility.Visible;
                    button.RenderTransform = new RotateTransform()
                    {
                        CenterX = 0,
                        CenterY = -5
                    };
                    button.RenderTransformOrigin = new Point(0.5, 0.5);
                    EventTrigger eventTrigger = new EventTrigger();
                    Storyboard storyboard = new Storyboard();
                    DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
                    doubleAnimationUsingKeyFrames.RepeatBehavior = new RepeatBehavior(50);
                    EasingDoubleKeyFrame easingDoubleKeyFrame_0 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 50)));
                    EasingDoubleKeyFrame easingDoubleKeyFrame_1 = new EasingDoubleKeyFrame(15, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 100)));
                    EasingDoubleKeyFrame easingDoubleKeyFrame_2 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 150)));
                    EasingDoubleKeyFrame easingDoubleKeyFrame_3 = new EasingDoubleKeyFrame(-15, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 200)));
                    EasingDoubleKeyFrame easingDoubleKeyFrame_4 = new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 250)));
                    doubleAnimationUsingKeyFrames.KeyFrames = new DoubleKeyFrameCollection() { easingDoubleKeyFrame_0,
                                                                                               easingDoubleKeyFrame_1,
                                                                                               easingDoubleKeyFrame_2,
                                                                                               easingDoubleKeyFrame_3,
                                                                                               easingDoubleKeyFrame_4};
                    Storyboard.SetTarget(doubleAnimationUsingKeyFrames, button);
                    Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
                    storyboard.Children.Add(doubleAnimationUsingKeyFrames);
                    storyboard.Begin(button);
                    CreateDragDropWindow(label);
                    DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => Grid_PreviewMouseMove", ex.Message, user);
            }
        }
        public void Grid_Dropping(object sender, DragEventArgs e)
        {
            try
            {
                var name = (VisualTreeHelper.GetChild(Grid.Children.OfType<Label>().First(), 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First().Name[0..^7];

                int oldIndex = user.VisiblePanels.IndexOf(name);

                List<(Point, Size)> locs = GetModuleLocations();
                int newIndex = 0;

                Button button = (Application.Current.MainWindow as MainWindow).RemoveModuleButton;

                HitTestResult hitTestResult = VisualTreeHelper.HitTest(Application.Current.MainWindow, e.GetPosition(Application.Current.MainWindow));

                if (hitTestResult.VisualHit == VisualTreeHelper.GetChild(button, 0))
                {

                    MessageBoxResult res = MessageBox.Show("Do you want to remove " + name + "?", "", MessageBoxButton.YesNo);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            (Application.Current.MainWindow as MainWindow).MainWrapPanel.Children.RemoveAt(oldIndex);
                            SaveSettings();
                            break;
                        case MessageBoxResult.No:
                            break;
                    }
                }
                else
                {
                    Point point = e.GetPosition(Application.Current.MainWindow as MainWindow);
                    foreach ((Point, Size) loc in locs)
                    {
                        if (point.X < (loc.Item1.X + loc.Item2.Width))
                        {
                            double nextY = locs[locs.IndexOf(loc) + 1].Item1.Y;
                            if (point.Y > loc.Item1.Y && point.Y < (loc.Item1.Y + (loc.Item2.Height / 2)))
                            {
                                // We have a winner!
                                newIndex = locs.IndexOf(loc) - 1;

                                break;
                            }
                            else if (point.Y > (loc.Item1.Y + (loc.Item2.Height / 2)) &&
                                (nextY > loc.Item1.Y ? point.Y < nextY : true))
                            {
                                // We have a winner!
                                newIndex = locs.IndexOf(loc);

                                break;
                            }
                        }
                    }

                    newIndex++;

                    

                    if (locs.Count == user.VisiblePanels.Count - 1)
                    {
                        // Remove module that's moving from old position
                        (Application.Current.MainWindow as MainWindow).MainWrapPanel.Children.RemoveAt(oldIndex);
                    }

                // Insert module that's moving into newIndex position
                (Application.Current.MainWindow as MainWindow).AddModule(name, newIndex);

                    SaveSettings();
                }
                CloseWindow();
                //droppedData.ClearValue(EffectProperty);
                Mouse.SetCursor(Cursors.Arrow);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("DragAndDrop.cs => Grid_Dropping", ex.Message, user);
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
        private List<(Point, Size)> GetModuleLocations()
        {
            List<(Point, Size)> loc = new List<(Point, Size)>();
            WrapPanel wrapPanel = (Application.Current.MainWindow as MainWindow).MainWrapPanel;
            var x = ((wrapPanel.Parent as Border).Parent as ScrollViewer).HorizontalOffset;
            foreach (Grid grid in wrapPanel.Children)
            {
                Point point = grid.TransformToAncestor(wrapPanel).Transform(new Point(-x, 0));

                if ((Application.Current.MainWindow.WindowState == WindowState.Maximized))
                {
                    var leftField = typeof(Window).GetField("_actualLeft", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var topField = typeof(Window).GetField("_actualTop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if ((double)leftField.GetValue(this) < -100) { point.X -= (int)System.Windows.SystemParameters.WorkArea.Width; }
                    else if ((double)leftField.GetValue(this) > 2100) { point.X += (int)System.Windows.SystemParameters.WorkArea.Width; }
                }
                else
                {
                    if ((Application.Current.MainWindow as MainWindow).Left > System.Windows.SystemParameters.WorkArea.Width)
                    {
                        point.X += (int)System.Windows.SystemParameters.WorkArea.Width;
                    }
                    else if ((Application.Current.MainWindow as MainWindow).Left < -10)
                    {
                        point.X -= (int)System.Windows.SystemParameters.WorkArea.Width;
                    }

                    point.Y += (Application.Current.MainWindow as MainWindow).Top;
                }

                Size size = new Size(grid.ActualWidth, grid.ActualHeight);
                if (size != new Size(0, 0)) { loc.Add((point, size)); }
            }

            return loc;
        }
        private void SaveSettings()
        {
            NAT02Context _nat02context = new NAT02Context();

            string newPanels = "";
            List<string> visiblePanels = new List<string>();
            foreach (Grid grid in WrapPanel.Children)
            {
                visiblePanels.Add((VisualTreeHelper.GetChild(grid.Children.OfType<Label>().First(), 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First().Name[0..^7]);
            }
            newPanels = String.Join(',', visiblePanels.ToArray());

            EoiSettings eoiSettings = _nat02context.EoiSettings.Single(s => s.EmployeeId == user.EmployeeCode);
            eoiSettings.Panels = newPanels;
            _nat02context.EoiSettings.Update(eoiSettings);
            _nat02context.SaveChanges();
            _nat02context.Dispose();

            user.VisiblePanels = visiblePanels;
        }
    }
}
