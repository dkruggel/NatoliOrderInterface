using DK.WshRuntime;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.DriveWorks;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.Projects;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Windows.Navigation;
using System.Windows.Documents;
using System.Windows.Interop;
using System.Diagnostics;
using System.Text;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for TabletDrawings.xaml
    /// </summary>
    public partial class TabletDrawings : Window
    {
        private readonly string eDrawDirectory = @"\\nsql03\data1\DRAW\E-DRAWINGS\";
        private readonly string projectDirectory = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\";

        private List<Tuple<string,string,string>> upperTabletDrawings = new List<Tuple<string,string,string>>();
        private List<Tuple<string,string,string>> lowerTabletDrawings = new List<Tuple<string,string,string>>();
        private List<Tuple<string,string,string>> shortRejectTabletDrawings = new List<Tuple<string,string,string>>();
        private List<Tuple<string,string,string>> longRejectTabletDrawings = new List<Tuple<string,string,string>>();

        public List<Tuple<string,string,string>> UpperTabletDrawings
        {
            get { return upperTabletDrawings; }
            set 
            { 
                upperTabletDrawings = value;
                UpperDrawingListBox.ItemsSource = null;
                UpperDrawingListBox.ItemsSource = upperTabletDrawings;
            }
        }
        public List<Tuple<string,string,string>> LowerTabletDrawings
        {
            get { return lowerTabletDrawings; }
            set
            {
                lowerTabletDrawings = value;
                LowerDrawingListBox.ItemsSource = null;
                LowerDrawingListBox.ItemsSource = lowerTabletDrawings;
            }
        }
        public List<Tuple<string,string,string>> ShortRejectTabletDrawings
        {
            get { return shortRejectTabletDrawings; }
            set
            {
                shortRejectTabletDrawings = value;
                ShortRejectDrawingListBox.ItemsSource = null;
                ShortRejectDrawingListBox.ItemsSource = shortRejectTabletDrawings;
            }
        }
        public List<Tuple<string,string,string>> LongRejectTabletDrawings
        {
            get { return longRejectTabletDrawings; }
            set
            {
                longRejectTabletDrawings = value;
                LongRejectDrawingListBox.ItemsSource = null;
                LongRejectDrawingListBox.ItemsSource = longRejectTabletDrawings;
            }
        }

        private string projectNumber = "";
        private List<string> hobNumbers = new List<string>();


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
        /// <summary>
        /// Creates a new Window with listboxes containing the relevant tablet drawings.
        /// Hob Numbers must be in order of Upper, Lower, Short Reject, Long Reject.
        /// </summary>
        /// <param name="hobNumbers"></param>
        /// <param name="window"></param>
        public TabletDrawings(List<string> hobNumbers, Window window, string projectNumber ="", string projectRevNumber="0")
        {
            InitializeComponent();
            IntPtr hwnd = new WindowInteropHelper(window).Handle;
            Rect windowRect = new Rect();
            GetWindowRect(hwnd, ref windowRect);
            this.hobNumbers = hobNumbers;
            Top = windowRect.Top;
            Left = windowRect.Left + (((window).Width - this.Width) / 2);
            Header.Text = projectNumber == "" ? "Tablet Drawings" : "Tablet Drawings For Project# " + projectNumber + "-" + projectRevNumber;
            this.projectNumber = projectNumber;

            SetDragDropEvents();
            FillTabletDrawingsData();
            this.Show();
        }
        /// <summary>
        /// Populates the data for the listboxes
        /// </summary>
        private void FillTabletDrawingsData()
        {
            int i = 0;
            foreach (string hobNumberString in hobNumbers)
            {
                if (int.TryParse(hobNumberString, out int hobNumber))
                {
                    string folderPrefix = IMethods.GetEDrawingsFolderPrefix(hobNumber);
                    string folderLocation = eDrawDirectory + folderPrefix + @"-E-DRAWINGS\";
                    IEnumerable<string> files = Directory.EnumerateFiles(eDrawDirectory + folderPrefix + @"-E-DRAWINGS\", hobNumber.ToString() + "*", new EnumerationOptions { RecurseSubdirectories = false, MatchType = MatchType.Simple, MatchCasing = MatchCasing.CaseInsensitive });
                    if (files.Any())
                    {
                        switch (i)
                        {
                            case 0:
                                List<Tuple<string, string, string>> upperTabletDrawings = new List<Tuple<string, string, string>>();
                                foreach (string file in files)
                                {
                                    string directory = Path.GetDirectoryName(file);
                                    string fileName = Path.GetFileNameWithoutExtension(file);
                                    string ext = Path.GetExtension(file);
                                    upperTabletDrawings.Add(new Tuple<string, string, string>(fileName, directory, ext));
                                }
                                UpperTabletDrawings = upperTabletDrawings;
                                break;
                            case 1:
                                List<Tuple<string, string, string>> lowerTabletDrawings = new List<Tuple<string, string, string>>();
                                foreach (string file in files)
                                {
                                    string directory = Path.GetDirectoryName(file);
                                    string fileName = Path.GetFileNameWithoutExtension(file);
                                    string ext = Path.GetExtension(file);
                                    lowerTabletDrawings.Add(new Tuple<string, string, string>(fileName, directory, ext));
                                }
                                LowerTabletDrawings = lowerTabletDrawings;
                                break;
                            case 2:
                                List<Tuple<string, string, string>> shortRejectTabletDrawings = new List<Tuple<string, string, string>>();
                                foreach (string file in files)
                                {
                                    string directory = Path.GetDirectoryName(file);
                                    string fileName = Path.GetFileNameWithoutExtension(file);
                                    string ext = Path.GetExtension(file);
                                    shortRejectTabletDrawings.Add(new Tuple<string, string, string>(fileName, directory, ext));
                                }
                                ShortRejectTabletDrawings = shortRejectTabletDrawings;
                                break;
                            case 3:
                                List<Tuple<string, string, string>> longRejectTabletDrawings = new List<Tuple<string, string, string>>();
                                foreach (string file in files)
                                {
                                    string directory = Path.GetDirectoryName(file);
                                    string fileName = Path.GetFileNameWithoutExtension(file);
                                    string ext = Path.GetExtension(file);
                                    longRejectTabletDrawings.Add(new Tuple<string, string, string>(fileName, directory, ext));
                                }
                                LongRejectTabletDrawings = longRejectTabletDrawings;
                                break;
                            default:
                                break;
                        }
                    }
                }
                i++;
            }
        }
        private void SetDragDropEvents()
        {
            Style itemContainerStyle = new System.Windows.Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewDragLeaveEvent, new DragEventHandler(ListBoxItem_PreviewDragLeave)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Dropping)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.GiveFeedbackEvent, new GiveFeedbackEventHandler(ListBoxItem_DraggingFeedback)));
            UpperDrawingListBox.ItemContainerStyle = itemContainerStyle;

            //itemContainerStyle = LowerDrawingListBox.ItemContainerStyle;
            //itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewDragLeaveEvent, new DragEventHandler(ListBoxItem_PreviewDragLeave)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Dropping)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.GiveFeedbackEvent, new GiveFeedbackEventHandler(ListBoxItem_DraggingFeedback)));
            LowerDrawingListBox.ItemContainerStyle = itemContainerStyle;

            //itemContainerStyle = ShortRejectDrawingListBox.ItemContainerStyle;
            //itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewDragLeaveEvent, new DragEventHandler(ListBoxItem_PreviewDragLeave)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Dropping)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.GiveFeedbackEvent, new GiveFeedbackEventHandler(ListBoxItem_DraggingFeedback)));
            ShortRejectDrawingListBox.ItemContainerStyle = itemContainerStyle;

            //itemContainerStyle = LongRejectDrawingListBox.ItemContainerStyle;
            //itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewDragLeaveEvent, new DragEventHandler(ListBoxItem_PreviewDragLeave)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(ListBoxItem_PreviewMouseMove)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ListBoxItem_Dropping)));
            //itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.GiveFeedbackEvent, new GiveFeedbackEventHandler(ListBoxItem_DraggingFeedback)));
            LongRejectDrawingListBox.ItemContainerStyle = itemContainerStyle;
        }
        /// <summary>
        /// Handles the listbox doubleclick event to open the file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void file_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListBox listBox = sender as ListBox;
                Tuple<string, string, string> file = null;
                switch (listBox.Name)
                {
                    case "UpperDrawingListBox":
                        file = upperTabletDrawings[listBox.SelectedIndex];
                        break;
                    case "LowerDrawingListBox":
                        file = lowerTabletDrawings[listBox.SelectedIndex];
                        break;
                    case "ShortRejectDrawingsListBox":
                        file = ShortRejectTabletDrawings[listBox.SelectedIndex];
                        break;
                    case "LongRejectDrawingsListBox":
                        file = LongRejectTabletDrawings[listBox.SelectedIndex];
                        break;
                }
                string fullFilePath = file.Item2 + "\\" + file.Item1 + file.Item3;
                System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", fullFilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the file. See error below." + System.Environment.NewLine + System.Environment.NewLine + ex.Message);
            }
        }
        /// <summary>
        /// Copies the selected files out of all the list boxes and moves it to the project folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopySelectedToProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + "\\");
                List<Tuple<string, string, string>> tabletDrawings = new List<Tuple<string, string, string>>();
                foreach (Tuple<string, string, string> listBoxItem in UpperDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = upperTabletDrawings[UpperDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> listBoxItem in LowerDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = lowerTabletDrawings[LowerDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> listBoxItem in ShortRejectDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = shortRejectTabletDrawings[ShortRejectDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> listBoxItem in LongRejectDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = longRejectTabletDrawings[LongRejectDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }

                foreach (Tuple<string, string, string> tabletDrawing in tabletDrawings)
                {
                    string fullFilePath = tabletDrawing.Item2 + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3;
                    try
                    {
                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not copy " + tabletDrawing.Item1 + "because:" + Environment.NewLine + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click()", ex.Message, new User());
            }
        }
        /// <summary>
        /// Copies all files out of all the list boxes and moves it to the project folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyAllToProject_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + "\\");
                List<Tuple<string, string, string>> tabletDrawings = new List<Tuple<string, string, string>>();
                foreach (Tuple<string, string, string> tabletDrawing in upperTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> tabletDrawing in lowerTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> tabletDrawing in shortRejectTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> tabletDrawing in longRejectTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }

                foreach (Tuple<string, string, string> tabletDrawing in tabletDrawings)
                {
                    string fullFilePath = tabletDrawing.Item2 + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3;
                    try
                    {
                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not copy " + tabletDrawing.Item1 + "because:" + Environment.NewLine + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopyAllToProject_Click()", ex.Message, new User());
            }
        }
        /// <summary>
        /// Copies the selected files out of all the list boxes and moves it to the "FILES_FOR_CUSTOMER" folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyAllToFilesForCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\");
                List<Tuple<string, string, string>> tabletDrawings = new List<Tuple<string, string, string>>();
                foreach (Tuple<string, string, string> tabletDrawing in upperTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> tabletDrawing in lowerTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> tabletDrawing in shortRejectTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> tabletDrawing in longRejectTabletDrawings)
                {
                    tabletDrawings.Add(tabletDrawing);
                }

                foreach (Tuple<string, string, string> tabletDrawing in tabletDrawings)
                {
                    string fullFilePath = tabletDrawing.Item2 + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3;
                    try
                    {
                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not copy " + tabletDrawing.Item1 + "because:" + Environment.NewLine + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopyAllToFilesForCustomer_Click()", ex.Message, new User());
            }
        }
        /// <summary>
        /// Copies all files out of all the list boxes and moves it to the "FILES_FOR_CUSTOMER" folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopySelectedToFilesForCustomer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(@"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\");
                List<Tuple<string, string, string>> tabletDrawings = new List<Tuple<string, string, string>>();
                foreach (Tuple<string, string, string> listBoxItem in UpperDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = upperTabletDrawings[UpperDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> listBoxItem in LowerDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = lowerTabletDrawings[LowerDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> listBoxItem in ShortRejectDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = shortRejectTabletDrawings[ShortRejectDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }
                foreach (Tuple<string, string, string> listBoxItem in LongRejectDrawingListBox.SelectedItems)
                {
                    Tuple<string, string, string> tabletDrawing = longRejectTabletDrawings[LongRejectDrawingListBox.Items.IndexOf(listBoxItem)];
                    tabletDrawings.Add(tabletDrawing);
                }

                foreach (Tuple<string, string, string> tabletDrawing in tabletDrawings)
                {
                    string fullFilePath = tabletDrawing.Item2 + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3;
                    try
                    {
                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Could not copy " + tabletDrawing.Item1 + "because:" + Environment.NewLine + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToFilesForCustomer_Click()", ex.Message, new User());
            }
        }

        private void ListBox_DragEnter(object sender, DragEventArgs e)
        {

        }

        private void ListBox_DragLeave(object sender, DragEventArgs e)
        {

        }

        private void ListBox_DragOver(object sender, DragEventArgs e)
        {

        }

        private void ListBox_Drop(object sender, DragEventArgs e)
        {

        }
        public void ListBoxItem_PreviewDragLeave(object sender, DragEventArgs e)
        {

        }
        public void ListBoxItem_DraggingFeedback(object sender, GiveFeedbackEventArgs e)
        {
            
        }
        public void ListBoxItem_Dropping(object sender, DragEventArgs e)
        {
           
        }
        public void ListBoxItem_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (e.LeftButton == MouseButtonState.Pressed && sender is ListBoxItem)
                {
                    ListBoxItem draggedItem = sender as ListBoxItem;
                    draggedItem.IsSelected = true;
                    List<Tuple<string, string, string>> tabletDrawings = new List<Tuple<string, string, string>>();
                    foreach (Tuple<string, string, string> listBoxItem in UpperDrawingListBox.SelectedItems)
                    {
                        Tuple<string, string, string> tabletDrawing = upperTabletDrawings[UpperDrawingListBox.Items.IndexOf(listBoxItem)];
                        tabletDrawings.Add(tabletDrawing);
                    }
                    foreach (Tuple<string, string, string> listBoxItem in LowerDrawingListBox.SelectedItems)
                    {
                        Tuple<string, string, string> tabletDrawing = lowerTabletDrawings[LowerDrawingListBox.Items.IndexOf(listBoxItem)];
                        tabletDrawings.Add(tabletDrawing);
                    }
                    foreach (Tuple<string, string, string> listBoxItem in ShortRejectDrawingListBox.SelectedItems)
                    {
                        Tuple<string, string, string> tabletDrawing = shortRejectTabletDrawings[ShortRejectDrawingListBox.Items.IndexOf(listBoxItem)];
                        tabletDrawings.Add(tabletDrawing);
                    }
                    foreach (Tuple<string, string, string> listBoxItem in LongRejectDrawingListBox.SelectedItems)
                    {
                        Tuple<string, string, string> tabletDrawing = longRejectTabletDrawings[LongRejectDrawingListBox.Items.IndexOf(listBoxItem)];
                        tabletDrawings.Add(tabletDrawing);
                    }

                    List<string> paths = new List<string>();
                    foreach (Tuple<string, string, string> tabletDrawing in tabletDrawings)
                    {
                        paths.Add(tabletDrawing.Item2 + "\\" + tabletDrawing.Item1 + tabletDrawing.Item3);
                    }

                    DragDrop.DoDragDrop(this, new DataObject(DataFormats.FileDrop, paths.ToArray()), DragDropEffects.Copy);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings.xaml.cs => ListBoxItem_PreviewMouseMove", ex.Message, new User());
            }
        }
    }
}
