using DK.WshRuntime;
using NatoliOrderInterface.Models;
using NatoliOrderInterface.Models.DriveWorks;
using NatoliOrderInterface.Models.NAT01;
using NatoliOrderInterface.Models.NEC;
using NatoliOrderInterface.Models.Projects;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Navigation;

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
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.MouseDoubleClickEvent, new MouseButtonEventHandler(file_MouseDoubleClick)));
            UpperDrawingListBox.ItemContainerStyle = itemContainerStyle;
            LowerDrawingListBox.ItemContainerStyle = itemContainerStyle;
            ShortRejectDrawingListBox.ItemContainerStyle = itemContainerStyle;
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
                if(sender is ListBoxItem)
                {
                    ListBoxItem listBoxItem = sender as ListBoxItem;
                    Tuple<string, string, string> file = listBoxItem.Content as Tuple<string, string, string>;
                    string fullFilePath = file.Item2 + "\\" + file.Item1 + file.Item3;
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", fullFilePath);
                }
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
            List<Tuple<string, string, string>> existingFiles = new List<Tuple<string, string, string>>();
            List<Tuple<string, string, string>> transferedFiles = new List<Tuple<string, string, string>>();
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
                        transferedFiles.Add(tabletDrawing);
                    }
                    catch (Exception ex)
                    {
                        if(ex.Message.Contains("already exists"))
                        {
                            existingFiles.Add(tabletDrawing);
                        }
                        else
                        {
                            MessageBox.Show("Could not copy " + tabletDrawing.Item1 + " because:" + Environment.NewLine + ex.Message);
                        }
                        
                    }
                }
                if (existingFiles.Count > 0)
                {

                    if (existingFiles.Count == 1)
                    {
                        try
                        {
                            MessageBoxResult result = MessageBox.Show(existingFiles[0].Item1 + existingFiles[0].Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            string fullFilePath = existingFiles[0].Item2 + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3;
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3, true);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                case MessageBoxResult.No:
                                    InputBox inputBox = new InputBox("Please enter the new name for " + existingFiles[0].Item1 + existingFiles[0].Item3 + ". Do NOT include the file extension.", "Rename File", this);
                                    inputBox.ShowDialog();
                                    string newName = inputBox.ReturnString;
                                    newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + newName + existingFiles[0].Item3);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files == 1", ex.Message, App.user);
                        }
                    }
                    else
                    {
                        foreach (Tuple<string, string, string> existingFile in existingFiles)
                        {
                            try
                            {
                                MessageBoxResult result = MessageBox.Show(existingFile.Item1 + existingFile.Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                string fullFilePath = existingFile.Item2 + "\\" + existingFile.Item1 + existingFile.Item3;
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + existingFile.Item1 + existingFile.Item3, true);
                                        transferedFiles.Add(existingFile);
                                        break;
                                    case MessageBoxResult.No:
                                        InputBox inputBox = new InputBox("Please enter the new name for " + existingFile.Item1 + existingFile.Item3+"." +Environment.NewLine+  "Do NOT include the file extension.", "Rename File", this);
                                        inputBox.ShowDialog();
                                        string newName = inputBox.ReturnString;
                                        if (newName.Length > 0)
                                        {
                                            newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                            File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + newName + existingFile.Item3);
                                            transferedFiles.Add(existingFile);
                                        }
                                        else
                                        {
                                            MessageBox.Show("File not copied because the file name was empty.");
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files > 1", ex.Message, App.user);
                            }
                        }
                    }
                }
                if (transferedFiles.Count > 0)
                {
                    MessageBox.Show("Successfully transfered " + transferedFiles.Count + " files to '" + projectNumber + "'.");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click()", ex.Message, App.user);
            }
        }
        /// <summary>
        /// Copies all files out of all the list boxes and moves it to the project folder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyAllToProject_Click(object sender, RoutedEventArgs e)
        {
            List<Tuple<string, string, string>> existingFiles = new List<Tuple<string, string, string>>();
            List<Tuple<string, string, string>> transferedFiles = new List<Tuple<string, string, string>>();
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
                        transferedFiles.Add(tabletDrawing);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("already exists"))
                        {
                            existingFiles.Add(tabletDrawing);
                        }
                        else
                        {
                            MessageBox.Show("Could not copy " + tabletDrawing.Item1 + " because:" + Environment.NewLine + ex.Message);
                        }
                    }
                }
                if (existingFiles.Count > 0)
                {

                    if (existingFiles.Count == 1)
                    {
                        try
                        {
                            MessageBoxResult result = MessageBox.Show(existingFiles[0].Item1 + existingFiles[0].Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            string fullFilePath = existingFiles[0].Item2 + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3;
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3, true);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                case MessageBoxResult.No:
                                    InputBox inputBox = new InputBox("Please enter the new name for " + existingFiles[0].Item1 + existingFiles[0].Item3 + ". Do NOT include the file extension.", "Rename File", this);
                                    inputBox.ShowDialog();
                                    string newName = inputBox.ReturnString;
                                    newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + newName + existingFiles[0].Item3);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files == 1", ex.Message, App.user);
                        }
                    }
                    else
                    {
                        foreach (Tuple<string, string, string> existingFile in existingFiles)
                        {
                            try
                            {
                                MessageBoxResult result = MessageBox.Show(existingFile.Item1 + existingFile.Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                string fullFilePath = existingFile.Item2 + "\\" + existingFile.Item1 + existingFile.Item3;
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + existingFile.Item1 + existingFile.Item3, true);
                                        transferedFiles.Add(existingFile);
                                        break;
                                    case MessageBoxResult.No:
                                        InputBox inputBox = new InputBox("Please enter the new name for " + existingFile.Item1 + existingFile.Item3 + "." + Environment.NewLine + "Do NOT include the file extension.", "Rename File", this);
                                        inputBox.ShowDialog();
                                        string newName = inputBox.ReturnString;
                                        if (newName.Length > 0)
                                        {
                                            newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                            File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + newName + existingFile.Item3);
                                            transferedFiles.Add(existingFile);
                                        }
                                        else
                                        {
                                            MessageBox.Show("File not copied because the file name was empty.");
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files > 1", ex.Message, App.user);
                            }
                        }
                    }
                }
                if (transferedFiles.Count > 0)
                {
                    MessageBox.Show("Successfully transfered " + transferedFiles.Count + " files to '" + projectNumber + "'.");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopyAllToProject_Click()", ex.Message, App.user);
            }
        }
        /// <summary>
        /// Copies the selected files out of all the list boxes and moves it to the "FILES_FOR_CUSTOMER" folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyAllToFilesForCustomer_Click(object sender, RoutedEventArgs e)
        {
            List<Tuple<string, string, string>> existingFiles = new List<Tuple<string, string, string>>();
            List<Tuple<string, string, string>> transferedFiles = new List<Tuple<string, string, string>>();
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
                        transferedFiles.Add(tabletDrawing);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("already exists"))
                        {
                            existingFiles.Add(tabletDrawing);
                        }
                        else
                        {
                            MessageBox.Show("Could not copy " + tabletDrawing.Item1 + " because:" + Environment.NewLine + ex.Message);
                        }
                    }
                }
                if (existingFiles.Count > 0)
                {

                    if (existingFiles.Count == 1)
                    {
                        try
                        {
                            MessageBoxResult result = MessageBox.Show(existingFiles[0].Item1 + existingFiles[0].Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            string fullFilePath = existingFiles[0].Item2 + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3;
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3, true);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                case MessageBoxResult.No:
                                    InputBox inputBox = new InputBox("Please enter the new name for " + existingFiles[0].Item1 + existingFiles[0].Item3 + ". Do NOT include the file extension.", "Rename File", this);
                                    inputBox.ShowDialog();
                                    string newName = inputBox.ReturnString;
                                    newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + newName + existingFiles[0].Item3);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files == 1", ex.Message, App.user);
                        }
                    }
                    else
                    {
                        foreach (Tuple<string, string, string> existingFile in existingFiles)
                        {
                            try
                            {
                                MessageBoxResult result = MessageBox.Show(existingFile.Item1 + existingFile.Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                string fullFilePath = existingFile.Item2 + "\\" + existingFile.Item1 + existingFile.Item3;
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + existingFile.Item1 + existingFile.Item3, true);
                                        transferedFiles.Add(existingFile);
                                        break;
                                    case MessageBoxResult.No:
                                        InputBox inputBox = new InputBox("Please enter the new name for " + existingFile.Item1 + existingFile.Item3 + "." + Environment.NewLine + "Do NOT include the file extension.", "Rename File", this);
                                        inputBox.ShowDialog();
                                        string newName = inputBox.ReturnString;
                                        if (newName.Length > 0)
                                        {
                                            newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                            File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + newName + existingFile.Item3);
                                            transferedFiles.Add(existingFile);
                                        }
                                        else
                                        {
                                            MessageBox.Show("File not copied because the file name was empty.");
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files > 1", ex.Message, App.user);
                            }
                        }
                    }
                }
                if (transferedFiles.Count > 0)
                {
                    MessageBox.Show("Successfully transfered " + transferedFiles.Count + " files to 'FILES_FOR_CUSTOMER'.");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopyAllToFilesForCustomer_Click()", ex.Message, App.user);
            }
        }
        /// <summary>
        /// Copies all files out of all the list boxes and moves it to the "FILES_FOR_CUSTOMER" folder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopySelectedToFilesForCustomer_Click(object sender, RoutedEventArgs e)
        {
            List<Tuple<string, string, string>> existingFiles = new List<Tuple<string, string, string>>();
            List<Tuple<string, string, string>> transferedFiles = new List<Tuple<string, string, string>>();
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
                        transferedFiles.Add(tabletDrawing);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("already exists"))
                        {
                            existingFiles.Add(tabletDrawing);
                        }
                        else
                        {
                            MessageBox.Show("Could not copy " + tabletDrawing.Item1 + " because:" + Environment.NewLine + ex.Message);
                        }
                    }
                }
                if (existingFiles.Count > 0)
                {

                    if (existingFiles.Count == 1)
                    {
                        try
                        {
                            MessageBoxResult result = MessageBox.Show(existingFiles[0].Item1 + existingFiles[0].Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                            string fullFilePath = existingFiles[0].Item2 + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3;
                            switch (result)
                            {
                                case MessageBoxResult.Yes:
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + existingFiles[0].Item1 + existingFiles[0].Item3, true);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                case MessageBoxResult.No:
                                    InputBox inputBox = new InputBox("Please enter the new name for " + existingFiles[0].Item1 + existingFiles[0].Item3 + ". Do NOT include the file extension.", "Rename File", this);
                                    inputBox.ShowDialog();
                                    string newName = inputBox.ReturnString;
                                    newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                    File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + newName + existingFiles[0].Item3);
                                    transferedFiles.Add(existingFiles[0]);
                                    break;
                                default:
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files == 1", ex.Message, App.user);
                        }
                    }
                    else
                    {
                        foreach (Tuple<string, string, string> existingFile in existingFiles)
                        {
                            try
                            {
                                MessageBoxResult result = MessageBox.Show(existingFile.Item1 + existingFile.Item3 + " could not be copied because a file of that name already exists. Would you like to overwrite the file?" + Environment.NewLine + "Yes to overwrite." + Environment.NewLine + "No to rename." + Environment.NewLine + "Cancel to cancel copying this file.", "File Exists", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                                string fullFilePath = existingFile.Item2 + "\\" + existingFile.Item1 + existingFile.Item3;
                                switch (result)
                                {
                                    case MessageBoxResult.Yes:
                                        File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + existingFile.Item1 + existingFile.Item3, true);
                                        transferedFiles.Add(existingFile);
                                        break;
                                    case MessageBoxResult.No:
                                        InputBox inputBox = new InputBox("Please enter the new name for " + existingFile.Item1 + existingFile.Item3 + "." + Environment.NewLine + "Do NOT include the file extension.", "Rename File", this);
                                        inputBox.ShowDialog();
                                        string newName = inputBox.ReturnString;
                                        if (newName.Length > 0)
                                        {
                                            newName = IMethods.GetFileNameWOIllegalCharacters(newName);
                                            File.Copy(fullFilePath, projectDirectory + projectNumber + "\\" + "FILES_FOR_CUSTOMER" + "\\" + newName + existingFile.Item3);
                                            transferedFiles.Add(existingFile);
                                        }
                                        else
                                        {
                                            MessageBox.Show("File not copied because the file name was empty.");
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToProject_Click() => Existing files > 1", ex.Message, App.user);
                            }
                        }
                    }
                }
                if (transferedFiles.Count > 0)
                {
                    MessageBox.Show("Successfully transfered " + transferedFiles.Count + " files to 'FILES_FOR_CUSTOMER'.");
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("TabletDrawings => CopySelectedToFilesForCustomer_Click()", ex.Message, App.user);
            }
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
                IMethods.WriteToErrorLog("TabletDrawings.xaml.cs => ListBoxItem_PreviewMouseMove", ex.Message, App.user);
            }
        }
    }
}
