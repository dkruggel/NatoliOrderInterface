﻿using DK.WshRuntime;
using Microsoft.Win32;
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
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace NatoliOrderInterface
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]

    public partial class ProjectWindow : Window, IDisposable, IMethods
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

        private readonly string projectNumber;
        private string projectRevNumber;
        private readonly MainWindow mainWindow = null;
        private readonly User user = null;
        private readonly Timer EditedTimer = new Timer(1500);
        string editedText = "";
        string editedTextBoxName = "";
        string upperCupType = "";
        string upperCupDepth = "";
        string lowerCupType = "";
        string lowerCupDepth = "";
        string shortRejectCupType = "";
        string shortRejectCupDepth = "";
        string longRejectCupType = "";
        string longRejectCupDepth = "";
        string upperLand = "";
        string lowerLand = "";
        string shortRejectLand = "";
        string longRejectLand = "";
        bool projectLinkedToQuote = false;
        private bool NewDrawing = false;
        private bool UpdateExistingDrawing = false;
        private bool UpdateTextOnDrawing = false;
        private bool PerSampleTablet = false;
        private bool RefTabletDrawing = false;
        private bool PerSampleTool = false;
        private bool RefToolDrawing = false;
        private bool PerSuppliedPicture = false;
        private bool RefNatoliDrawing = false;
        private bool RefNonNatoliDrawing = false;
        private string BinLocation = "";
        private bool CoreRod = false;
        private string CoreRodSteelID = null;
        private bool CoreRodKey = false;
        private string CoreRodKeySteelID = null;
        private bool CoreRodKeyCollar = false;
        private string CoreRodKeyCollarSteelID = null;
        private bool CoreRodPunch = false;
        private string CoreRodPunchSteelID = null;
        public string emptyString = "";
        private List<Tuple<string, string, string>> projectFiles = new List<Tuple<string, string, string>>();
        public List<Tuple<string, string, string>> ProjectFiles
        {
            get { return projectFiles; }
            set
            {
                projectFiles = value;
                FilesListBox.ItemsSource = null;
                FilesListBox.ItemsSource = projectFiles;
            }
        }
        private readonly string projectsDirectory = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\";
        Quote quote = null;
        private bool validData = false;
        private bool timersEnabled = false;
        private bool saved = false;


        // Standard constructor
        public ProjectWindow(string projectNumber, string projectRevNumber, MainWindow parent, User user, bool isCreating)
        {
            InitializeComponent();
            this.projectNumber = projectNumber;
            this.projectRevNumber = projectRevNumber;
            this.user = user ?? new User();
            mainWindow = parent ?? new MainWindow();
            WindowSetup();
            Show();
            if (isCreating)
            {
                PopulateBlankWindow();
            }
            else
            {
                LoadEngineeringProject();
            }
            //if (mainWindow.WindowState == WindowState.Maximized)
            //{
            //    this.WindowState = WindowState.Maximized;
            //}
            timersEnabled = true;
        }

        // From quote constructor
        public ProjectWindow(string projectNumber, string projectRevNumber, MainWindow parent, User user, bool isCreating, Quote quote)
        {
            InitializeComponent();
            this.projectNumber = projectNumber;
            this.projectRevNumber = projectRevNumber;
            this.user = user ?? new User();
            mainWindow = parent ?? new MainWindow();
            WindowSetup();
            Show();
            this.quote = quote;
            if (isCreating)
            {
                saved = false;
                PopulateBlankWindow();
            }
            else
            {
                LoadEngineeringProject();
            }
            QuoteLink();
            //if (mainWindow.WindowState == WindowState.Maximized)
            //{
            //    this.WindowState = WindowState.Maximized;
            //}
            timersEnabled = true;
        }

        // Copied constructor
        public ProjectWindow(string projectNumber, string projectRevNumber, MainWindow parent, User user, string copiedProjectNumber, string copiedProjectRevNumber)
        {
            InitializeComponent();
            this.projectNumber = projectNumber;
            this.projectRevNumber = projectRevNumber;
            this.user = user ?? new User();
            mainWindow = parent ?? new MainWindow();
            WindowSetup();
            Show();
            LoadCopiedProject(copiedProjectNumber, copiedProjectRevNumber);
            //if (mainWindow.WindowState == WindowState.Maximized)
            //{
            //    this.WindowState = WindowState.Maximized;
            //}
            timersEnabled = true;
        }

        /// <summary>
        /// Checks the form for possible errors.
        /// Will throw MessageBoxes when it finds an error.
        /// Returns true if no errors are found.
        /// </summary>
        /// <returns></returns>
        private bool FormCheck()
        {
            bool isTabletProject = TabletsRequired.IsChecked ?? false;
            bool isToolProject = ToolsRequired.IsChecked ?? false;
            if (!isTabletProject && !isToolProject)
            {
                MessageBox.Show("Please select a project Type (Tablet or Tools).", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false; ;
            }
            if (string.IsNullOrWhiteSpace(UnitOfMeasure.Text))
            {
                MessageBox.Show("Please enter a Unit Of Measure.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false; ;
            }
            if (string.IsNullOrWhiteSpace(CustomerName.Text) && string.IsNullOrWhiteSpace(ShipToName.Text) && string.IsNullOrWhiteSpace(EndUserName.Text))
            {
                MessageBox.Show("Please enter a Customer/Ship To/End User name.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false; ;
            }
            if (string.IsNullOrWhiteSpace(DueDate.Text))
            {
                MessageBox.Show("Please enter a due date for the project.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false; ;
            }
            if ((CreateButton.Content == "Create" || user.Department != "Engineering") && 
                Priority.IsChecked == false && 
                ((DueDate.Text.StartsWith('0') || 
                    (DueDate.Text.StartsWith('1') && DateTime.Now.TimeOfDay > new TimeSpan(14, 0, 0))) || 
                (DueDate.Text.StartsWith('2') && DateTime.Now.DayOfWeek == DayOfWeek.Saturday) || 
                    (ToolsRequired.IsChecked == true && TabletsRequired.IsChecked == true && 
                    (DueDate.Text.StartsWith('1') || 
                    (DueDate.Text.StartsWith('2') && DateTime.Now.TimeOfDay > new TimeSpan(14, 0, 0)))))
                )
            {
                MessageBox.Show("Please mark Priority for this rush.", "Rush Needs Priority", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (Notes.GetText().Length<=2)
            {
                MessageBox.Show("Please enter notes for this project.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (Notes.GetText().Length > 1999)
            {
                MessageBox.Show("Please limit notes to less than 2000 characters. You have (" + Notes.GetText().Length + ") currently", "Reduce Notes", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (string.IsNullOrWhiteSpace(TabletWidth.Text))
            {
                MessageBox.Show("Please enter a tablet size.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (!string.IsNullOrWhiteSpace(TabletWidth.Text) && !DieShape.Text.Trim().ToUpper().Contains("DIAMETER") && !DieShape.Text.Trim().ToUpper().Contains("ROUND") && string.IsNullOrWhiteSpace(TabletLength.Text))
            {
                MessageBox.Show("Please enter a tablet length or indicate Die Shape as Diameter.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (System.IO.Directory.GetFiles(projectsDirectory + projectNumber).Where(f => !f.EndsWith(".lnk")).Count() == 0)
            {
                MessageBox.Show("You may have forgotten to attach supporting documents.", "Need Supporting Docs", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            if (isTabletProject)
            {
                if (string.IsNullOrWhiteSpace(DieTolerances.Text))
                {
                    MessageBox.Show("Please enter Tolerance for Die.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (UpperTabletDrawing.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(UpperHobDescription.Text))
                    {
                        MessageBox.Show("Please enter a Hob Description for Upper.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                    if (string.IsNullOrWhiteSpace(UpperTolerances.Text))
                    {
                        MessageBox.Show("Please enter Tolerance for Upper.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                }
                if (LowerTabletDrawing.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(LowerHobDescription.Text))
                    {
                        MessageBox.Show("Please enter a Hob Description for Lower.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                    if (string.IsNullOrWhiteSpace(LowerTolerances.Text))
                    {
                        MessageBox.Show("Please enter Tolerance for Lower.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                }
                if (ShortRejectTabletDrawing.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(ShortRejectHobDescription.Text))
                    {
                        MessageBox.Show("Please enter a Hob Description for Short Reject.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                    if (string.IsNullOrWhiteSpace(ShortRejectTolerances.Text))
                    {
                        MessageBox.Show("Please enter Tolerance for Short Reject.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                }
                if (LongRejectTabletDrawing.IsChecked == true)
                {
                    if (string.IsNullOrWhiteSpace(LongRejectHobDescription.Text))
                    {
                        MessageBox.Show("Please enter a Hob Description for Long Reject.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                    if (string.IsNullOrWhiteSpace(LongRejectTolerances.Text))
                    {
                        MessageBox.Show("Please enter Tolerance for Long Reject.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                        return false; ;
                    }
                }
            }
            if (isToolProject)
            {
                if (UpperPunch.IsChecked == true && string.IsNullOrWhiteSpace(UpperPunchSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Upper Punch.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (UpperCap.IsChecked == true && string.IsNullOrWhiteSpace(UpperCapSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Upper Cap.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (UpperHolder.IsChecked == true && string.IsNullOrWhiteSpace(UpperHolderSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Upper Holder.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (UpperHead.IsChecked == true && string.IsNullOrWhiteSpace(UpperHeadSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Upper Head.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (UpperTip.IsChecked == true && string.IsNullOrWhiteSpace(UpperTipSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Upper Tip.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LowerPunch.IsChecked == true && string.IsNullOrWhiteSpace(LowerPunchSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Lower Punch.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LowerCap.IsChecked == true && string.IsNullOrWhiteSpace(LowerCapSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Lower Cap.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LowerHolder.IsChecked == true && string.IsNullOrWhiteSpace(LowerHolderSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Lower Holder.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LowerHead.IsChecked == true && string.IsNullOrWhiteSpace(LowerHeadSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Lower Head.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LowerTip.IsChecked == true && string.IsNullOrWhiteSpace(LowerTipSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Lower Tip.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (CoreRod && string.IsNullOrWhiteSpace(CoreRodSteelID))
                {
                    MessageBox.Show("Please enter SteelID for Lower Core Rod.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (CoreRodPunch && string.IsNullOrWhiteSpace(CoreRodPunchSteelID))
                {
                    MessageBox.Show("Please enter SteelID for Lower Core Rod Punch.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (CoreRodKey && string.IsNullOrWhiteSpace(CoreRodKeySteelID))
                {
                    MessageBox.Show("Please enter SteelID for Lower Core Rod Key.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (CoreRodKeyCollar && string.IsNullOrWhiteSpace(CoreRodKeyCollarSteelID))
                {
                    MessageBox.Show("Please enter SteelID for Lower Core Rod Key Collar.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (ShortRejectPunch.IsChecked == true && string.IsNullOrWhiteSpace(ShortRejectPunchSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for ShortReject Punch.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (ShortRejectCap.IsChecked == true && string.IsNullOrWhiteSpace(ShortRejectCapSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Short Reject Cap.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (ShortRejectHolder.IsChecked == true && string.IsNullOrWhiteSpace(ShortRejectHolderSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Short Reject Holder.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (ShortRejectHead.IsChecked == true && string.IsNullOrWhiteSpace(ShortRejectHeadSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Short Reject Head.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (ShortRejectTip.IsChecked == true && string.IsNullOrWhiteSpace(ShortRejectTipSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Short Reject Tip.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LongRejectPunch.IsChecked == true && string.IsNullOrWhiteSpace(LongRejectPunchSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for LongR eject Punch.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LongRejectCap.IsChecked == true && string.IsNullOrWhiteSpace(LongRejectCapSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Long Reject Cap.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LongRejectHolder.IsChecked == true && string.IsNullOrWhiteSpace(LongRejectHolderSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Long Reject Holder.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LongRejectHead.IsChecked == true && string.IsNullOrWhiteSpace(LongRejectHeadSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Long Reject Head.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (LongRejectTip.IsChecked == true && string.IsNullOrWhiteSpace(LongRejectTipSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Long Reject Tip.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (Die.IsChecked == true && string.IsNullOrWhiteSpace(DieSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Die.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (DieComponent.IsChecked == true && string.IsNullOrWhiteSpace(DieComponentSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Die Component.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (DieHolder.IsChecked == true && string.IsNullOrWhiteSpace(DieHolderSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Die Holder.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (DieInsert.IsChecked == true && string.IsNullOrWhiteSpace(DieInsertSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Die Insert.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (DiePlate.IsChecked == true && string.IsNullOrWhiteSpace(DiePlateSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Die Plate.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (DieSegment.IsChecked == true && string.IsNullOrWhiteSpace(DieSegmentSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Die Segment.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (Alignment.IsChecked == true && string.IsNullOrWhiteSpace(AlignmentSteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Alignment Tool.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (Key.IsChecked == true && string.IsNullOrWhiteSpace(KeySteelID.Text))
                {
                    MessageBox.Show("Please enter SteelID for Key.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (Misc.IsChecked == true && string.IsNullOrWhiteSpace(MiscSteelID.Text))
                {
                    MessageBox.Show("Please enter material for Misc Item.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (string.IsNullOrWhiteSpace(KeyType.Text) && (UpperKeyed.IsChecked == true || LowerKeyed.IsChecked == true || ShortRejectKeyed.IsChecked == true || LongRejectKeyed.IsChecked == true))
                {
                    MessageBox.Show("Please enter Key Type.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (string.IsNullOrWhiteSpace(UpperGrooveType.Text) && (UpperGroove.IsChecked == true || ShortRejectGroove.IsChecked == true || LongRejectGroove.IsChecked == true))
                {
                    MessageBox.Show("Please enter an Upper Groove Type.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (string.IsNullOrWhiteSpace(LowerGrooveType.Text) && LowerGroove.IsChecked == true)
                {
                    MessageBox.Show("Please enter a Lower Groove Type.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
                if (string.IsNullOrWhiteSpace(HeadType.Text) && (UpperPunch.IsChecked == true || UpperHolder.IsChecked == true || UpperHead.IsChecked == true || UpperHolder.IsChecked == true ||
                    LowerPunch.IsChecked == true || LowerHolder.IsChecked == true || LowerHead.IsChecked == true || LowerHolder.IsChecked == true ||
                    ShortRejectPunch.IsChecked == true || ShortRejectHolder.IsChecked == true || ShortRejectHead.IsChecked == true || ShortRejectHolder.IsChecked == true ||
                    LongRejectPunch.IsChecked == true || LongRejectHolder.IsChecked == true || LongRejectHead.IsChecked == true || LongRejectHolder.IsChecked == true))
                {
                    MessageBox.Show("Please enter a Head Type.", "Need Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return false; ;
                }
            }
            return true;
        }
        /// <summary>
        /// Sets up the windows position, size, and item sources.
        /// </summary>
        private void WindowSetup()
        {
            IntPtr hwnd = new WindowInteropHelper(mainWindow).Handle;
            Rect windowRect = new Rect();
            GetWindowRect(hwnd, ref windowRect);

            Top = windowRect.Top;
            //Top = mainWindow.Top;
            Left = windowRect.Left;
            //Left = mainWindow.Left;
            Width = mainWindow.Width;
            Height = mainWindow.Height;


            EditedTimer.Elapsed += EditedTimer_Elapsed;
            Title = "Project# " + projectNumber + "-" + projectRevNumber;

            System.Collections.IEnumerable items = IMethods.GetSteelIDItemsSource();
            List<ComboBox> steelComboBoxes = new List<ComboBox>()
                {
                    UpperPunchSteelID,
                    UpperCapSteelID,
                    UpperHolderSteelID,
                    UpperHeadSteelID,
                    UpperTipSteelID,
                    LowerPunchSteelID,
                    LowerCapSteelID,
                    LowerHolderSteelID,
                    LowerHeadSteelID,
                    LowerTipSteelID,
                    ShortRejectPunchSteelID,
                    ShortRejectCapSteelID,
                    ShortRejectHolderSteelID,
                    ShortRejectHeadSteelID,
                    ShortRejectTipSteelID,
                    LongRejectPunchSteelID,
                    LongRejectCapSteelID,
                    LongRejectHolderSteelID,
                    LongRejectHeadSteelID,
                    LongRejectTipSteelID,
                    AlignmentSteelID,
                    KeySteelID,
                    DieSteelID,
                    DieComponentSteelID,
                    DieHolderSteelID,
                    DieInsertSteelID,
                    DiePlateSteelID,
                    DieSegmentSteelID
                };
            foreach (ComboBox comboBox in steelComboBoxes)
            {
                comboBox.Items.Clear();
                comboBox.ItemsSource = null;
                comboBox.ItemsSource = items;
            }
            DieShape.Items.Clear();
            DieShape.ItemsSource = null;
            DieShape.ItemsSource = IMethods.GetShapeDescriptionsItemsSource();
            DueDate.Items.Clear();
            DueDate.ItemsSource = null;
            DueDate.ItemsSource = IMethods.GetDueDatesItemsSource();
            UpperCupType.Items.Clear();
            UpperCupType.ItemsSource = null;
            UpperCupType.ItemsSource = IMethods.GetCupTypeItemsSource();
            LowerCupType.Items.Clear();
            LowerCupType.ItemsSource = null;
            LowerCupType.ItemsSource = IMethods.GetCupTypeItemsSource();
            ShortRejectCupType.Items.Clear();
            ShortRejectCupType.ItemsSource = null;
            ShortRejectCupType.ItemsSource = IMethods.GetCupTypeItemsSource();
            LongRejectCupType.Items.Clear();
            LongRejectCupType.ItemsSource = null;
            LongRejectCupType.ItemsSource = IMethods.GetCupTypeItemsSource();
            ProjectFiles = GetProjectFiles(projectNumber);
            EngineeringCommentButton.Visibility = user.Department == "Engineering" ? Visibility.Visible : Visibility.Hidden;
            HobAndDieInfoButton.Visibility = user.EmployeeCode == "E4408" ? Visibility.Visible : Visibility.Hidden;
        }

        private List<Tuple<string, string, string>> GetProjectFiles(string projectNumber)
        {
            try
            {
                string rootDir = projectsDirectory + projectNumber + "\\";
                if (!Directory.Exists(rootDir))
                {
                    Directory.CreateDirectory(rootDir);
                }
                string[] filePaths = Directory.GetFiles(rootDir, "*.*", SearchOption.AllDirectories);
                List<Tuple<string, string, string>> files = new List<Tuple<string, string, string>>();
                foreach (string file in filePaths)
                {
                    string directory = Path.GetDirectoryName(file);
                    string fileName = Path.GetFileNameWithoutExtension(file);
                    string ext = Path.GetExtension(file);
                    files.Add(new Tuple<string, string, string>(fileName, directory, ext));
                }
                return files;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => GetProjectFiles", ex.Message, user);
            }
            return new List<Tuple<string, string, string>>();
        }
        /// <summary>
        /// Fills in the controls for a blank project ready to be created.
        /// </summary>
        private void PopulateBlankWindow()
        {
            try
            {
                if (!Directory.Exists(projectsDirectory + projectNumber + "\\"))
                {
                    Directory.CreateDirectory(projectsDirectory + projectNumber + "\\");
                }
                ProjectNavigation.Visibility = Visibility.Hidden;
                CreationBorder.Visibility = Visibility.Visible;
                ArchivedOrInactive.Visibility = Visibility.Collapsed;
                using var _projectsContext = new ProjectsContext();
                using var _nat01Context = new NAT01Context();
                EngineeringProjects engineeringProject = IMethods.GetBlankEngineeringProject(user, projectNumber, projectRevNumber);
                StartButton.IsEnabled = false;
                FinishButton.IsEnabled = false;
                SubmitButton.IsEnabled = false;
                CheckButton.IsEnabled = false;
                PutOnHoldButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                QuoteFolderButton.IsEnabled = false;
                ReturnToCSR.ItemsSource = null;
                ReturnToCSR.ItemsSource = IMethods.GetDWCSRs();
                CSR.Text = user.GetDWPrincipalId();
                UnitOfMeasure.SelectedItem = engineeringProject.UnitOfMeasure;


                _projectsContext.Add(engineeringProject);
                _projectsContext.SaveChanges();
                _projectsContext.Dispose();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => PopulateBlankWindow", ex.Message, user);
            }
        }
        /// <summary>
        /// Fills in all controls/variables from the this.projectNumber and this.projectRevNumber.
        /// Provides the correct routing buttons.
        /// Disables all controls.
        /// </summary>
        private void LoadEngineeringProject()
        {
            try
            {
                saved = true;
                using var _projectsContext = new ProjectsContext();
                using var _nat01Context = new NAT01Context();
                CreationBorder.Visibility = Visibility.Hidden;
                ProjectNavigation.Visibility = Visibility.Visible;
                decimal conversion = (string)Resources["UnitsText"] == "in" ? 1 : 1 / (decimal)25.4;
                // Is there actually a project
                if (_projectsContext.EngineeringProjects.Any(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber) || _projectsContext.EngineeringArchivedProjects.Any(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber))
                {
                    if (_projectsContext.EngineeringProjects.Any(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber))
                    {
                        EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber);

                        if (!engineeringProject.ActiveProject)
                        {
                            ArchivedOrInactive.Text = "Inactive";
                            ArchivedOrInactive.Visibility = Visibility.Visible;
                        }
                        else
                        {
                            ArchivedOrInactive.Visibility = Visibility.Collapsed;
                        }
                        RefreshRoutingButtons();

                        projectLinkedToQuote = engineeringProject.QuoteNumber.Length > 0;
                        if (projectLinkedToQuote)
                        {
                            LinkedToQuoteBorder.Background = Brushes.PaleGoldenrod;
                            LinkQuoteButton.Tag = Application.Current.Resources["unlinkDrawingImage"] as DrawingImage;
                            LinkQuoteButton.ToolTip = "Unlink Quote";
                            QuoteFolderButton.IsEnabled = true;
                            try
                            {
                                quote = new Quote(Convert.ToInt32(engineeringProject.QuoteNumber), string.IsNullOrEmpty(engineeringProject.QuoteRevNumber) ? (short)0 : Convert.ToInt16(engineeringProject.QuoteRevNumber));
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            LinkedToQuoteBorder.ClearValue(BackgroundProperty);
                            LinkQuoteButton.Tag = Application.Current.Resources["linkDrawingImage"] as DrawingImage;
                            LinkQuoteButton.ToolTip = "Link Quote";
                            QuoteFolderButton.IsEnabled = false;
                            quote = null;
                        }
                        LinkQuoteButton.IsEnabled = false;

                        NewDrawing = engineeringProject.NewDrawing;
                        UpdateExistingDrawing = engineeringProject.UpdateExistingDrawing;
                        UpdateTextOnDrawing = engineeringProject.UpdateTextOnDrawing;
                        PerSampleTablet = engineeringProject.PerSampleTablet;
                        RefTabletDrawing = engineeringProject.RefTabletDrawing;
                        PerSampleTool = engineeringProject.PerSampleTool;
                        RefToolDrawing = engineeringProject.RefToolDrawing;
                        PerSuppliedPicture = engineeringProject.PerSuppliedPicture;
                        RefNatoliDrawing = engineeringProject.RefNatoliDrawing;
                        RefNonNatoliDrawing = engineeringProject.RefNonNatoliDrawing;
                        BinLocation = engineeringProject.BinLocation;
                        SpecificationsButton.IsEnabled = NewDrawing || UpdateExistingDrawing || UpdateTextOnDrawing || PerSampleTablet || RefTabletDrawing || PerSampleTool || RefToolDrawing || PerSuppliedPicture || RefNatoliDrawing || RefNonNatoliDrawing || !string.IsNullOrWhiteSpace(BinLocation);


                        CSR.Text = engineeringProject.CSR;
                        ReturnToCSR.ItemsSource = null;
                        ReturnToCSR.ItemsSource = ReturnToCSR.ItemsSource = IMethods.GetDWCSRs();
                        ReturnToCSR.SelectedItem = engineeringProject.ReturnToCSR;
                        ReturnToCSR.IsEnabled = false;

                        EnteredDate.Text = TimeZoneInfo.ConvertTimeFromUtc(engineeringProject.TimeSubmitted, TimeZoneInfo.Local).ToString("M/d/yy h:mm tt");
                        EnteredDate.IsEnabled = false;

                        RevisedBy.Text = engineeringProject.RevisedBy;
                        RevisedBy.IsEnabled = false;
                        if (_projectsContext.EngineeringArchivedProjects.Any(eap => eap.ProjectNumber == projectNumber && eap.RevNumber == (Convert.ToInt16(projectRevNumber) - 1).ToString()))
                        {
                            DateTime date = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == projectNumber && eap.RevNumber == (Convert.ToInt16(projectRevNumber) - 1).ToString()).TimeArchived ?? DateTime.MinValue;
                            date = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
                            RevisionDate.Text = date.ToString("M/d/yy h:mm tt");
                        }
                        RevisionDate.IsEnabled = false;

                        TabletStartedBy.Text = engineeringProject.TabletStartedBy;
                        TabletStartedBy.IsEnabled = false;
                        TabletDrawnBy.Text = engineeringProject.TabletDrawnBy;
                        TabletDrawnBy.IsEnabled = false;
                        ToolStartedBy.Text = engineeringProject.ToolStartedBy;
                        ToolStartedBy.IsEnabled = false;
                        ToolDrawnBy.Text = engineeringProject.ToolDrawnBy;
                        ToolDrawnBy.IsEnabled = false;

                        QuoteNumber.Text = engineeringProject.QuoteNumber;
                        QuoteNumber.IsEnabled = false;
                        QuoteRevNumber.Text = engineeringProject.QuoteRevNumber;
                        QuoteRevNumber.IsEnabled = false;
                        RefOrderNumber.Text = engineeringProject.RefOrderNumber;
                        RefOrderNumber.IsEnabled = false;
                        UnitOfMeasure.Text = engineeringProject.UnitOfMeasure;
                        UnitOfMeasure.IsEnabled = false;
                        ReferenceQuoteNumber.Text = engineeringProject.RefQuoteNumber;
                        ReferenceQuoteNumber.IsEnabled = false;
                        ReferenceQuoteRevNumber.Text = engineeringProject.RefQuoteRevNumber;
                        ReferenceQuoteRevNumber.IsEnabled = false;
                        ReferenceProjectNumber.Text = engineeringProject.RefProjectNumber;
                        ReferenceProjectNumber.IsEnabled = false;
                        ReferenceProjectRevNumber.Text = engineeringProject.RefProjectRevNumber;
                        ReferenceProjectRevNumber.IsEnabled = false;
                        CustomerNumber.Text = engineeringProject.CustomerNumber;
                        CustomerNumber.IsEnabled = false;
                        CustomerName.Text = engineeringProject.CustomerName;
                        CustomerName.IsEnabled = false;
                        ShipToNumber.Text = engineeringProject.ShipToNumber;
                        ShipToNumber.IsEnabled = false;
                        ShipToLocNumber.Text = engineeringProject.ShipToLocNumber;
                        ShipToLocNumber.IsEnabled = false;
                        ShipToName.Text = engineeringProject.ShipToName;
                        ShipToName.IsEnabled = false;
                        EndUserNumber.Text = engineeringProject.EndUserNumber;
                        EndUserNumber.IsEnabled = false;
                        EndUserLocNumber.Text = engineeringProject.EndUserLocNumber;
                        EndUserLocNumber.IsEnabled = false;
                        EndUserName.Text = engineeringProject.EndUserName;
                        EndUserName.IsEnabled = false;
                        Product.Text = engineeringProject.Product;
                        Product.IsEnabled = false;
                        Attention.Text = engineeringProject.Attention;
                        Attention.IsEnabled = false;
                        MachineNumber.Text = engineeringProject.MachineNumber;
                        MachineNumber.IsEnabled = false;
                        MachineDescription.IsEnabled = false;
                        if (!string.IsNullOrWhiteSpace(engineeringProject.MachineNumber) && Int16.TryParse(engineeringProject.MachineNumber, out short _machineNo))
                        {
                            if (_nat01Context.MachineList.Any(m => m.MachineNo == _machineNo))
                            {
                                string description = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Description.Trim();
                                string od = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od == null ? "" :((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od * conversion).ToString();
                                string ol = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol == null ? "" : ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol * conversion).ToString();
                                MachineDescription.Text = description;
                                DieOD.Text = od;
                                DieODPlaceholder.Visibility = Visibility.Collapsed;
                                DieOL.Text = ol;
                                DieOLPlaceholder.Visibility = Visibility.Collapsed;
                            }
                        }
                        DieOD.IsEnabled = false;
                        DieOL.IsEnabled = false;
                        MachineDescription.IsEnabled = false;
                        DueDate.IsEditable = true;
                        DueDate.Text = (engineeringProject.DueDate - DateTime.Today).TotalDays + " Day(s) | " + engineeringProject.DueDate.ToString("d");
                        DueDate.IsEnabled = false;
                        Priority.IsChecked = engineeringProject.Priority;
                        Priority.IsEnabled = false;

                        List<string> notes = new List<string>();
                        List<SolidColorBrush> solidColorBrushes = new List<SolidColorBrush>
                            {
                                Application.Current.Resources["Dark"] as SolidColorBrush,
                                Application.Current.Resources["Medium"] as SolidColorBrush,
                                Application.Current.Resources["Bright"] as SolidColorBrush
                            };
                        if (_projectsContext.EngineeringArchivedProjects.Any(eap => eap.ProjectNumber == projectNumber))
                        {

                            notes = _projectsContext.EngineeringArchivedProjects.Where(eap => eap.ProjectNumber == projectNumber).OrderBy(eap => Convert.ToInt32(eap.RevNumber)).Select(eap => eap.Notes).ToList();
                        }

                        notes.Add(_projectsContext.EngineeringProjects.First(eap => eap.ProjectNumber == projectNumber && eap.RevNumber == projectRevNumber).Notes);
                        int i = 0;
                        try
                        {
                            foreach (string note in notes)
                            {
                                string oldText = Notes.GetText();
                                oldText = oldText.TrimEnd('\r', '\n');
                                if (!string.IsNullOrEmpty(oldText) && !string.IsNullOrWhiteSpace(oldText) && note.TrimEnd('\r', '\n') != oldText && note.Contains(oldText))
                                {
                                    int x = note.IndexOf(oldText);
                                    int y = oldText.Length;
                                    var appending = note.Remove(note.IndexOf(oldText), oldText.Length);
                                    Notes.AppendText(appending, solidColorBrushes[i]);
                                }
                                else
                                {
                                    Notes.ReplaceText(note, solidColorBrushes[0]);
                                    i = 0;
                                }
                                i++;
                                i = i == 3 ? 0 : i;
                            }
                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("ProjectWindow => Note population - LoadEngineeringProject", ex.Message, user);
                        }
                        
                        //Notes.Document = engineeringProject.Notes;
                        Notes.IsReadOnly = true;
                        DieNumber.Text = engineeringProject.DieNumber;
                        DieNumber.IsEnabled = false;
                        DieShape.Text = engineeringProject.DieShape;
                        DieShape.IsEnabled = false;
                        TabletWidth.Text = engineeringProject.Width == null ? "" : (engineeringProject.Width * conversion).ToString().TrimEnd('0');
                        TabletWidth.IsEnabled = false;
                        TabletLength.Text = engineeringProject.Length == null ? "" : (engineeringProject.Length * conversion).ToString().TrimEnd('0');
                        TabletLength.IsEnabled = false;
                        DieTolerances.Text = engineeringProject.DieTolerances;
                        DieTolerances.IsEnabled = false;
                        UpperCupType.Text = engineeringProject.UpperCupType == null ? "" : engineeringProject.UpperCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.UpperCupType).Description.Trim();
                        UpperCupType.IsEnabled = false;
                        UpperHobNumber.Text = engineeringProject.UpperHobNumber.Trim();
                        UpperHobNumber.IsEnabled = false;
                        UpperCupDepth.Text = engineeringProject.UpperCupDepth == null ? "" : (engineeringProject.UpperCupDepth * conversion).ToString().TrimEnd('0');
                        UpperCupDepth.IsEnabled = false;
                        UpperLand.Text = engineeringProject.UpperLand == null ? "" : (engineeringProject.UpperLand * conversion).ToString().TrimEnd('0');
                        UpperLand.IsEnabled = false;
                        UpperHobDescription.Text = engineeringProject.UpperHobDescription;
                        if (!string.IsNullOrWhiteSpace(UpperHobDescription.Text.ToString()) && UpperHobDescription.Text.ToString().Length > 0)
                        {
                            UpperHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            UpperHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                        }
                        UpperHobDescription.IsEnabled = false;
                        UpperTolerances.Text = engineeringProject.UpperTolerances;
                        UpperTolerances.IsEnabled = false;
                        LowerCupType.Text = engineeringProject.LowerCupType == null ? "" : engineeringProject.LowerCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.LowerCupType).Description.Trim();
                        LowerCupType.IsEnabled = false;
                        LowerHobNumber.Text = engineeringProject.LowerHobNumber.Trim();
                        LowerHobNumber.IsEnabled = false;
                        LowerCupDepth.Text = engineeringProject.LowerCupDepth == null ? "" : (engineeringProject.LowerCupDepth * conversion).ToString().TrimEnd('0');
                        LowerCupDepth.IsEnabled = false;
                        LowerLand.Text = engineeringProject.LowerLand == null ? "" : (engineeringProject.LowerLand * conversion).ToString().TrimEnd('0');
                        LowerLand.IsEnabled = false;
                        LowerHobDescription.Text = engineeringProject.LowerHobDescription;
                        if (!string.IsNullOrWhiteSpace(LowerHobDescription.Text.ToString()) && LowerHobDescription.Text.ToString().Length > 0)
                        {
                            LowerHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            LowerHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                        }
                        LowerHobDescription.IsEnabled = false;
                        LowerTolerances.Text = engineeringProject.LowerTolerances;
                        LowerTolerances.IsEnabled = false;
                        ShortRejectCupType.Text = engineeringProject.ShortRejectCupType == null ? "" : engineeringProject.ShortRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.ShortRejectCupType).Description.Trim();
                        ShortRejectCupType.IsEnabled = false;
                        ShortRejectHobNumber.Text = engineeringProject.ShortRejectHobNumber.Trim();
                        ShortRejectHobNumber.IsEnabled = false;
                        ShortRejectCupDepth.Text = engineeringProject.ShortRejectCupDepth == null ? "" : (engineeringProject.ShortRejectCupDepth * conversion).ToString().TrimEnd('0');
                        ShortRejectCupDepth.IsEnabled = false;
                        ShortRejectLand.Text = engineeringProject.ShortRejectLand == null ? "" : (engineeringProject.ShortRejectLand * conversion).ToString().TrimEnd('0');
                        ShortRejectLand.IsEnabled = false;
                        ShortRejectHobDescription.Text = engineeringProject.ShortRejectHobDescription;
                        if (!string.IsNullOrWhiteSpace(ShortRejectHobDescription.Text.ToString()) && ShortRejectHobDescription.Text.ToString().Length > 0)
                        {
                            ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                        }
                        ShortRejectHobDescription.IsEnabled = false;
                        ShortRejectTolerances.Text = engineeringProject.ShortRejectTolerances;
                        ShortRejectTolerances.IsEnabled = false;
                        LongRejectCupType.Text = engineeringProject.LongRejectCupType == null ? "" : engineeringProject.LongRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.LongRejectCupType).Description.Trim();
                        LongRejectCupType.IsEnabled = false;
                        LongRejectHobNumber.Text = engineeringProject.LongRejectHobNumber.Trim();
                        LongRejectHobNumber.IsEnabled = false;
                        LongRejectCupDepth.Text = engineeringProject.LongRejectCupDepth == null ? "" : (engineeringProject.LongRejectCupDepth * conversion).ToString().TrimEnd('0');
                        LongRejectCupDepth.IsEnabled = false;
                        LongRejectLand.Text = engineeringProject.LongRejectLand == null ? "" : (engineeringProject.LongRejectLand * conversion).ToString().TrimEnd('0');
                        LongRejectLand.IsEnabled = false;
                        LongRejectHobDescription.Text = engineeringProject.LongRejectHobDescription;
                        if (!string.IsNullOrWhiteSpace(LongRejectHobDescription.Text.ToString()) && LongRejectHobDescription.Text.ToString().Length > 0)
                        {
                            LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                        }
                        LongRejectHobDescription.IsEnabled = false;
                        LongRejectTolerances.Text = engineeringProject.LongRejectTolerances;
                        LongRejectTolerances.IsEnabled = false;

                        MultiTipSketch.IsChecked = engineeringProject.MultiTipSketch;
                        MultiTipSketch.IsEnabled = false;
                        SketchID.Text = engineeringProject.MultiTipSketchID;
                        SketchID.IsEnabled = false;
                        if (SketchID.Text.Length>0)
                        {
                            using var _nat02Context = new NAT02Context();
                            MultiTipSketchInformation multiTipSketchInformation = _nat02Context.MultiTipSketchInformation.First(s => s.ID == Convert.ToInt32(SketchID.Text));
                            string ID = multiTipSketchInformation.ID.ToString();
                            string dieNumber = multiTipSketchInformation.DieNumber.ToString();
                            string width = ((decimal)multiTipSketchInformation.Width).ToString("F4", CultureInfo.InvariantCulture);
                            string type = multiTipSketchInformation.AssembledOrSolid == 'S' ? "SOLID" : multiTipSketchInformation.AssembledOrSolid == 'S' ? "ASSEMBLED" : "NEITHER_ASSEMBLED_OR_SOLID";
                            string tipQTY = multiTipSketchInformation.TotalNumberOfTips.ToString();
                            if (multiTipSketchInformation.Length != null)
                            {
                                string length = ((double)multiTipSketchInformation.Length).ToString("F4", CultureInfo.InvariantCulture);
                                string path = @"R:\tools\MULTI-TIP SKETCHES\" + width + " X " + length + "\\" + dieNumber + "\\" + type + "\\" + tipQTY + "-TIP " + type + "\\" + ID + "\\" + "MULTI TIP SKETCH " + ID + ".pdf";
                                if (System.IO.File.Exists(path))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Visible));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = new Uri(path, UriKind.Absolute)));
                                }
                            }
                            else
                            {
                                string path = @"R:\tools\MULTI-TIP SKETCHES\" + width + "\\" + dieNumber + "\\" + type + "\\" + tipQTY + "-TIP " + type + "\\" + ID + "\\" + "MULTI TIP SKETCH " + ID + ".pdf";
                                if (System.IO.File.Exists(path))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Visible));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = new Uri(path, UriKind.Absolute)));
                                }
                            }
                            _nat02Context.Dispose();
                        }
                        if (engineeringProject.MultiTipSolid == true)
                        {
                            MultiTipStyle.Text = "SOLID";
                        }
                        if (engineeringProject.MultiTipAssembled == true)
                        {
                            MultiTipStyle.Text = "ASSEMBLED";
                        }
                        MultiTipStyle.IsEnabled = false;

                        EngineeringTabletProjects tabletProject = null;
                        EngineeringToolProjects toolProject = null;
                        // For tablets?
                        if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            tabletProject = _projectsContext.EngineeringTabletProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                            TabletsRequired.IsChecked = true;
                            TabletsRequired.IsEnabled = false;
                            UpperTabletDrawing.IsChecked = tabletProject.UpperRequired;
                            UpperTabletDrawing.IsEnabled = false;
                            LowerTabletDrawing.IsChecked = tabletProject.LowerRequired;
                            LowerTabletDrawing.IsEnabled = false;
                            ShortRejectTabletDrawing.IsChecked = tabletProject.ShortRejectRequired;
                            ShortRejectTabletDrawing.IsEnabled = false;
                            LongRejectTabletDrawing.IsChecked = tabletProject.LongRejectRequired;
                            LongRejectTabletDrawing.IsEnabled = false;
                            Density.Text = tabletProject.Density.ToString();
                            Density.IsEnabled = false;
                            DensityUnits.SelectedItem = tabletProject.DensityUnits;
                            DensityUnits.IsEnabled = false;
                            Mass.Text = tabletProject.Mass.ToString();
                            Mass.IsEnabled = false;
                            MassUnits.SelectedItem = tabletProject.MassUnits;
                            MassUnits.IsEnabled = false;
                            Volume.Text = tabletProject.Volume.ToString();
                            Volume.IsEnabled = false;
                            VolumeUnits.SelectedItem = tabletProject.VolumeUnits;
                            VolumeUnits.IsEnabled = false;
                            TargetThickness.Text = tabletProject.TargetThickness.ToString();
                            TargetThickness.IsEnabled = false;
                            TargetThicknessUnits.SelectedItem = tabletProject.TargetThicknessUnits;
                            TargetThicknessUnits.IsEnabled = false;
                            FilmCoat.IsChecked = tabletProject.FilmCoated;
                            FilmCoat.IsEnabled = false;
                            FilmCoatStyle.SelectedItem = tabletProject.FilmCoatType;
                            FilmCoatStyle.IsEnabled = false;
                            PrePick.IsChecked = tabletProject.PrePick;
                            PrePick.IsEnabled = false;
                            PrePickAmount.Text = tabletProject.PrePickAmount.ToString();
                            PrePickAmount.IsEnabled = false;
                            PrePickUnits.SelectedItem = tabletProject.PrePickUnits;
                            PrePickUnits.IsEnabled = false;
                            Taper.IsChecked = tabletProject.Taper;
                            Taper.IsEnabled = false;
                            TaperAmount.Text = tabletProject.TaperAmount.ToString();
                            TaperAmount.IsEnabled = false;
                            TaperUnits.SelectedItem = tabletProject.TaperUnits;
                            TaperUnits.IsEnabled = false;
                        }
                        else
                        {
                            TabletsRequired.IsChecked = false;
                            TabletsRequired.IsEnabled = false;
                        }
                        // For Tools?
                        if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            toolProject = _projectsContext.EngineeringToolProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                            CoreRod = toolProject.LowerCoreRod;
                            CoreRodSteelID = toolProject.LowerCoreRodSteelID;
                            CoreRodKey = toolProject.LowerCoreRodKey;
                            CoreRodKeySteelID = toolProject.LowerCoreRodKeySteelID;
                            CoreRodKeyCollar = toolProject.LowerCoreRodKeyCollar;
                            CoreRodKeyCollarSteelID = toolProject.LowerCoreRodKeyCollarSteelID;
                            CoreRodPunch = toolProject.LowerCoreRodPunch;
                            CoreRodPunchSteelID = toolProject.LowerCoreRodPunchSteelID;
                            CoreRodButton.IsEnabled = toolProject.LowerCoreRod || toolProject.LowerCoreRodKey || toolProject.LowerCoreRodKeyCollar || toolProject.LowerCoreRodPunch;

                            ToolsRequired.IsChecked = true;
                            ToolsRequired.IsEnabled = false;
                            UpperPunch.IsChecked = toolProject.UpperPunch;
                            UpperPunch.IsEnabled = false;
                            UpperPunchSteelID.Text = toolProject.UpperPunchSteelID;
                            UpperPunchSteelID.IsEnabled = false;
                            UpperAssembly.IsChecked = toolProject.UpperAssembly;
                            UpperAssembly.IsEnabled = false;
                            UpperCap.IsChecked = toolProject.UpperCap;
                            UpperCap.IsEnabled = false;
                            UpperCapSteelID.Text = toolProject.UpperCapSteelID;
                            UpperCapSteelID.IsEnabled = false;
                            UpperHolder.IsChecked = toolProject.UpperHolder;
                            UpperHolder.IsEnabled = false;
                            UpperHolderSteelID.Text = toolProject.UpperHolderSteelID;
                            UpperHolderSteelID.IsEnabled = false;
                            UpperHead.IsChecked = toolProject.UpperHead;
                            UpperHead.IsEnabled = false;
                            UpperHeadSteelID.Text = toolProject.UpperHeadSteelID;
                            UpperHeadSteelID.IsEnabled = false;
                            UpperTip.IsChecked = toolProject.UpperTip;
                            UpperTip.IsEnabled = false;
                            UpperTipSteelID.Text = toolProject.UpperTipSteelID;
                            UpperTipSteelID.IsEnabled = false;
                            LowerPunch.IsChecked = toolProject.LowerPunch;
                            LowerPunch.IsEnabled = false;
                            LowerPunchSteelID.Text = toolProject.LowerPunchSteelID;
                            LowerPunchSteelID.IsEnabled = false;
                            LowerAssembly.IsChecked = toolProject.LowerAssembly;
                            LowerAssembly.IsEnabled = false;
                            LowerCap.IsChecked = toolProject.LowerCap;
                            LowerCap.IsEnabled = false;
                            LowerCapSteelID.Text = toolProject.LowerCapSteelID;
                            LowerCapSteelID.IsEnabled = false;
                            LowerHolder.IsChecked = toolProject.LowerHolder;
                            LowerHolder.IsEnabled = false;
                            LowerHolderSteelID.Text = toolProject.LowerHolderSteelID;
                            LowerHolderSteelID.IsEnabled = false;
                            LowerHead.IsChecked = toolProject.LowerHead;
                            LowerHead.IsEnabled = false;
                            LowerHeadSteelID.Text = toolProject.LowerHeadSteelID;
                            LowerHeadSteelID.IsEnabled = false;
                            LowerTip.IsChecked = toolProject.LowerTip;
                            LowerTip.IsEnabled = false;
                            LowerTipSteelID.Text = toolProject.LowerTipSteelID;
                            LowerTipSteelID.IsEnabled = false;
                            ShortRejectPunch.IsChecked = toolProject.ShortRejectPunch;
                            ShortRejectPunch.IsEnabled = false;
                            ShortRejectPunchSteelID.Text = toolProject.ShortRejectPunchSteelID;
                            ShortRejectPunchSteelID.IsEnabled = false;
                            ShortRejectAssembly.IsChecked = toolProject.ShortRejectAssembly;
                            ShortRejectAssembly.IsEnabled = false;
                            ShortRejectCap.IsChecked = toolProject.ShortRejectCap;
                            ShortRejectCap.IsEnabled = false;
                            ShortRejectCapSteelID.Text = toolProject.ShortRejectCapSteelID;
                            ShortRejectCapSteelID.IsEnabled = false;
                            ShortRejectHolder.IsChecked = toolProject.ShortRejectHolder;
                            ShortRejectHolder.IsEnabled = false;
                            ShortRejectHolderSteelID.Text = toolProject.ShortRejectHolderSteelID;
                            ShortRejectHolderSteelID.IsEnabled = false;
                            ShortRejectHead.IsChecked = toolProject.ShortRejectHead;
                            ShortRejectHead.IsEnabled = false;
                            ShortRejectHeadSteelID.Text = toolProject.ShortRejectHeadSteelID;
                            ShortRejectHeadSteelID.IsEnabled = false;
                            ShortRejectTip.IsChecked = toolProject.ShortRejectTip;
                            ShortRejectTip.IsEnabled = false;
                            ShortRejectTipSteelID.Text = toolProject.ShortRejectTipSteelID;
                            ShortRejectTipSteelID.IsEnabled = false;
                            LongRejectPunch.IsChecked = toolProject.LongRejectPunch;
                            LongRejectPunch.IsEnabled = false;
                            LongRejectPunchSteelID.Text = toolProject.LongRejectPunchSteelID;
                            LongRejectPunchSteelID.IsEnabled = false;
                            LongRejectAssembly.IsChecked = toolProject.LongRejectAssembly;
                            LongRejectAssembly.IsEnabled = false;
                            LongRejectCap.IsChecked = toolProject.LongRejectCap;
                            LongRejectCap.IsEnabled = false;
                            LongRejectCapSteelID.Text = toolProject.LongRejectCapSteelID;
                            LongRejectCapSteelID.IsEnabled = false;
                            LongRejectHolder.IsChecked = toolProject.LongRejectHolder;
                            LongRejectHolder.IsEnabled = false;
                            LongRejectHolderSteelID.Text = toolProject.LongRejectHolderSteelID;
                            LongRejectHolderSteelID.IsEnabled = false;
                            LongRejectHead.IsChecked = toolProject.LongRejectHead;
                            LongRejectHead.IsEnabled = false;
                            LongRejectHeadSteelID.Text = toolProject.LongRejectHeadSteelID;
                            LongRejectHeadSteelID.IsEnabled = false;
                            LongRejectTip.IsChecked = toolProject.LongRejectTip;
                            LongRejectTip.IsEnabled = false;
                            LongRejectTipSteelID.Text = toolProject.LongRejectTipSteelID;
                            LongRejectTipSteelID.IsEnabled = false;
                            Alignment.IsChecked = toolProject.Alignment;
                            Alignment.IsEnabled = false;
                            AlignmentSteelID.Text = toolProject.AlignmentSteelID;
                            AlignmentSteelID.IsEnabled = false;
                            Key.IsChecked = toolProject.Key;
                            Key.IsEnabled = false;
                            KeySteelID.Text = toolProject.KeySteelID;
                            KeySteelID.IsEnabled = false;
                            Misc.IsChecked = toolProject.Misc;
                            Misc.IsEnabled = false;
                            MiscSteelID.Text = toolProject.MiscSteelID;
                            MiscSteelID.IsEnabled = false;
                            NumberOfTips.Text = engineeringProject.NumberOfTips < 2 ? "" : engineeringProject.NumberOfTips.ToString();
                            NumberOfTips.IsEnabled = false;
                            Die.IsChecked = toolProject.Die;
                            Die.IsEnabled = false;
                            DieSteelID.Text = toolProject.DieSteelID;
                            DieSteelID.IsEnabled = false;
                            DieAssembly.IsChecked = toolProject.DieAssembly;
                            DieAssembly.IsEnabled = false;
                            DieComponent.IsChecked = toolProject.DieComponent;
                            DieComponent.IsEnabled = false;
                            DieComponentSteelID.Text = toolProject.DieComponentSteelID;
                            DieComponentSteelID.IsEnabled = false;
                            DieHolder.IsChecked = toolProject.DieHolder;
                            DieHolder.IsEnabled = false;
                            DieHolderSteelID.Text = toolProject.DieHolderSteelID;
                            DieHolderSteelID.IsEnabled = false;
                            DieInsert.IsChecked = toolProject.DieInsert;
                            DieInsert.IsEnabled = false;
                            DieInsertSteelID.Text = toolProject.DieInsertSteelID;
                            DieInsertSteelID.IsEnabled = false;
                            DiePlate.IsChecked = toolProject.DiePlate;
                            DiePlate.IsEnabled = false;
                            DiePlateSteelID.Text = toolProject.DiePlateSteelID;
                            DiePlateSteelID.IsEnabled = false;
                            DieSegment.IsChecked = toolProject.DieSegment;
                            DieSegment.IsEnabled = false;
                            DieSegmentSteelID.Text = toolProject.DieSegmentSteelID;
                            DieSegmentSteelID.IsEnabled = false;
                            KeyType.IsEditable = true;
                            KeyType.Text = toolProject.KeyType;
                            KeyType.IsEditable = false;
                            KeyType.IsEnabled = false;
                            KeyAngle.Text = toolProject.KeyAngle.ToString();
                            KeyAngle.IsEnabled = false;
                            KeyOrientation.Text = toolProject.KeyIsClockWise == true ? "CW" : toolProject.KeyIsClockWise == false ? "CCW" : "";
                            KeyOrientation.IsEnabled = false;
                            UpperKeyed.IsChecked = toolProject.UpperKeyed;
                            UpperKeyed.IsEnabled = false;
                            LowerKeyed.IsChecked = toolProject.LowerKeyed;
                            LowerKeyed.IsEnabled = false;
                            ShortRejectKeyed.IsChecked = toolProject.ShortRejectKeyed;
                            ShortRejectKeyed.IsEnabled = false;
                            LongRejectKeyed.IsChecked = toolProject.LongRejectKeyed;
                            LongRejectKeyed.IsEnabled = false;
                            UpperGrooveType.Text = toolProject.UpperGrooveType;
                            UpperGrooveType.IsEnabled = false;
                            LowerGrooveType.Text = toolProject.LowerGrooveType;
                            LowerGrooveType.IsEnabled = false;
                            UpperGroove.IsChecked = toolProject.UpperGroove;
                            UpperGroove.IsEnabled = false;
                            LowerGroove.IsChecked = toolProject.LowerGroove;
                            LowerGroove.IsEnabled = false;
                            ShortRejectGroove.IsChecked = toolProject.ShortRejectGroove;
                            ShortRejectGroove.IsEnabled = false;
                            LongRejectGroove.IsChecked = toolProject.LongRejectGroove;
                            LongRejectGroove.IsEnabled = false;
                            HeadType.Text = toolProject.HeadType;
                            HeadType.IsEnabled = false;
                            CarbideTips.IsChecked = toolProject.CarbideTips;
                            CarbideTips.IsEnabled = false;
                            MachineNotes.Text = toolProject.MachineNotes;
                            MachineNotes.IsEnabled = false;
                            if (!_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) || _projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber && p.TabletChecked == true))
                            {
                                ToolsTabItem.IsSelected = true;
                                CurrentProjectType.Text = "TOOLS";
                                RefreshRoutingButtons();
                            }
                        }
                        else
                        {
                            ToolsRequired.IsChecked = false;
                            ToolsRequired.IsEnabled = false;
                        }
                    }
                    else
                    {
                        EngineeringArchivedProjects engineeringProject = _projectsContext.EngineeringArchivedProjects.First(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber);

                        RefreshRoutingButtons();
                        
                        CreationBorder.Visibility = Visibility.Hidden;
                        ProjectNavigation.Visibility = Visibility.Visible;

                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ArchivedOrInactive.Text = "Archived"));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ArchivedOrInactive.Visibility = Visibility.Visible));

                        projectLinkedToQuote = engineeringProject.QuoteNumber.Length > 0;
                        if (projectLinkedToQuote)
                        {
                            LinkedToQuoteBorder.Background = Brushes.PaleGoldenrod;
                            LinkQuoteButton.Tag = Application.Current.Resources["unlinkDrawingImage"] as DrawingImage;
                            LinkQuoteButton.ToolTip = "Unlink Quote";
                            QuoteFolderButton.IsEnabled = true;
                            quote = new Quote(Convert.ToInt32(engineeringProject.QuoteNumber), Convert.ToInt16(engineeringProject.QuoteRevNumber));
                        }
                        else
                        {
                            LinkedToQuoteBorder.ClearValue(BackgroundProperty);
                            LinkQuoteButton.Tag = Application.Current.Resources["linkDrawingImage"] as DrawingImage;
                            LinkQuoteButton.ToolTip = "Link Quote";
                            QuoteFolderButton.IsEnabled = false;
                            quote = null;
                        }
                        LinkQuoteButton.IsEnabled = false;


                        NewDrawing = engineeringProject.NewDrawing;
                        UpdateExistingDrawing = engineeringProject.UpdateExistingDrawing;
                        UpdateTextOnDrawing = engineeringProject.UpdateTextOnDrawing;
                        PerSampleTablet = engineeringProject.PerSampleTablet;
                        RefTabletDrawing = engineeringProject.RefTabletDrawing;
                        PerSampleTool = engineeringProject.PerSampleTool;
                        RefToolDrawing = engineeringProject.RefToolDrawing;
                        PerSuppliedPicture = engineeringProject.PerSuppliedPicture;
                        RefNatoliDrawing = engineeringProject.RefNatoliDrawing;
                        RefNonNatoliDrawing = engineeringProject.RefNonNatoliDrawing;
                        BinLocation = engineeringProject.BinLocation;
                        SpecificationsButton.IsEnabled = NewDrawing || UpdateExistingDrawing || UpdateTextOnDrawing || PerSampleTablet || RefTabletDrawing || PerSampleTool || RefToolDrawing || PerSuppliedPicture || RefNatoliDrawing || RefNonNatoliDrawing || !string.IsNullOrWhiteSpace(BinLocation);


                        CSR.Text = engineeringProject.CSR;
                        ReturnToCSR.ItemsSource = null;
                        ReturnToCSR.ItemsSource = ReturnToCSR.ItemsSource = IMethods.GetDWCSRs();
                        ReturnToCSR.SelectedItem = engineeringProject.ReturnToCSR;
                        ReturnToCSR.IsEnabled = false;

                        RevisedBy.Text = engineeringProject.RevisedBy;
                        RevisedBy.IsEnabled = false;
                        if (_projectsContext.EngineeringArchivedProjects.Any(eap => eap.ProjectNumber == projectNumber && eap.RevNumber == (Convert.ToInt16(projectRevNumber) - 1).ToString()))
                        {
                            DateTime date = _projectsContext.EngineeringArchivedProjects.First(eap => eap.ProjectNumber == projectNumber && eap.RevNumber == (Convert.ToInt16(projectRevNumber) - 1).ToString()).TimeArchived ?? DateTime.MinValue;
                            date = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
                            RevisionDate.Text = date.ToString("M/d/yy h:mm tt");
                        }
                        RevisionDate.IsEnabled = false;

                        TabletStartedBy.Text = engineeringProject.TabletStartedBy;
                        TabletStartedBy.IsEnabled = false;
                        TabletDrawnBy.Text = engineeringProject.TabletDrawnBy;
                        TabletDrawnBy.IsEnabled = false;
                        ToolStartedBy.Text = engineeringProject.ToolStartedBy;
                        ToolStartedBy.IsEnabled = false;
                        ToolDrawnBy.Text = engineeringProject.ToolDrawnBy;
                        ToolDrawnBy.IsEnabled = false;

                        QuoteNumber.Text = engineeringProject.QuoteNumber;
                        QuoteNumber.IsEnabled = false;
                        QuoteRevNumber.Text = engineeringProject.QuoteRevNumber;
                        QuoteRevNumber.IsEnabled = false;
                        RefOrderNumber.Text = engineeringProject.RefOrderNumber;
                        RefOrderNumber.IsEnabled = false;
                        UnitOfMeasure.Text = engineeringProject.UnitOfMeasure;
                        UnitOfMeasure.IsEnabled = false;
                        ReferenceQuoteNumber.Text = engineeringProject.RefQuoteNumber;
                        ReferenceQuoteNumber.IsEnabled = false;
                        ReferenceQuoteRevNumber.Text = engineeringProject.RefQuoteRevNumber;
                        ReferenceQuoteRevNumber.IsEnabled = false;
                        ReferenceProjectNumber.Text = engineeringProject.RefProjectNumber;
                        ReferenceProjectNumber.IsEnabled = false;
                        ReferenceProjectRevNumber.Text = engineeringProject.RefProjectRevNumber;
                        ReferenceProjectRevNumber.IsEnabled = false;
                        CustomerNumber.Text = engineeringProject.CustomerNumber;
                        CustomerNumber.IsEnabled = false;
                        CustomerName.Text = engineeringProject.CustomerName;
                        CustomerName.IsEnabled = false;
                        ShipToNumber.Text = engineeringProject.ShipToNumber;
                        ShipToNumber.IsEnabled = false;
                        ShipToLocNumber.Text = engineeringProject.ShipToLocNumber;
                        ShipToLocNumber.IsEnabled = false;
                        ShipToName.Text = engineeringProject.ShipToName;
                        ShipToName.IsEnabled = false;
                        EndUserNumber.Text = engineeringProject.EndUserNumber;
                        EndUserNumber.IsEnabled = false;
                        EndUserLocNumber.Text = engineeringProject.EndUserLocNumber;
                        EndUserLocNumber.IsEnabled = false;
                        EndUserName.Text = engineeringProject.EndUserName;
                        EndUserName.IsEnabled = false;
                        Product.Text = engineeringProject.Product;
                        Product.IsEnabled = false;
                        Attention.Text = engineeringProject.Attention;
                        Attention.IsEnabled = false;
                        MachineNumber.Text = engineeringProject.MachineNumber;
                        MachineNumber.IsEnabled = false;
                        MachineDescription.IsEnabled = false;
                        if (!string.IsNullOrWhiteSpace(engineeringProject.MachineNumber) && Int16.TryParse(engineeringProject.MachineNumber, out short _machineNoArchived))
                        {
                            if (_nat01Context.MachineList.Any(m => m.MachineNo == _machineNoArchived))
                            {
                                string description = _nat01Context.MachineList.First(m => m.MachineNo == _machineNoArchived).Description.Trim();
                                string od = _nat01Context.MachineList.First(m => m.MachineNo == _machineNoArchived).Od.ToString();
                                string ol = _nat01Context.MachineList.First(m => m.MachineNo == _machineNoArchived).Ol.ToString();
                                MachineDescription.Text = description;
                                DieOD.Text = od;
                                DieODPlaceholder.Visibility = Visibility.Collapsed;
                                DieOL.Text = ol;
                                DieOLPlaceholder.Visibility = Visibility.Collapsed;
                            }
                        }
                        DieOD.IsEnabled = false;
                        DieOL.IsEnabled = false;
                        MachineDescription.IsEnabled = false;
                        DueDate.IsEditable = true;
                        DueDate.Text = (engineeringProject.DueDate - DateTime.Today).TotalDays + " Day(s) | " + engineeringProject.DueDate.ToString("d");
                        DueDate.IsEnabled = false;
                        Priority.IsChecked = engineeringProject.Priority;
                        Priority.IsEnabled = false;

                        if (_projectsContext.EngineeringArchivedProjects.Any(eap => eap.ProjectNumber == projectNumber))
                        {
                            List<SolidColorBrush> solidColorBrushes = new List<SolidColorBrush>
                            {
                                Application.Current.Resources["Dark"] as SolidColorBrush,
                                Application.Current.Resources["Medium"] as SolidColorBrush,
                                Application.Current.Resources["Bright"] as SolidColorBrush
                            };
                            //var d = _projectsContext.EngineeringArchivedProjects.Where(eap => eap.ProjectNumber == projectNumber && Convert.ToInt32(eap.RevNumber) <= Convert.ToInt32(projectRevNumber)).ToList();
                            List<string> notes = _projectsContext.EngineeringArchivedProjects.Where(eap => eap.ProjectNumber == projectNumber && Convert.ToInt32(eap.RevNumber) <= Convert.ToInt32(projectRevNumber)).OrderBy(eap => Convert.ToInt32(eap.RevNumber)).Select(eap => eap.Notes).ToList();
                            int i = 0;
                            try
                            {
                                foreach (string note in notes)
                                {
                                    string oldText = Notes.GetText();
                                    oldText = oldText.TrimEnd('\r', '\n');
                                    if (!string.IsNullOrEmpty(oldText) && !string.IsNullOrWhiteSpace(oldText) && note.TrimEnd('\r', '\n') != oldText && note.Contains(oldText))
                                    {
                                        int x = note.IndexOf(oldText);
                                        int y = oldText.Length;
                                        var appending = note.Remove(note.IndexOf(oldText), oldText.Length);
                                        Notes.AppendText(appending, solidColorBrushes[i]);
                                    }
                                    else
                                    {
                                        Notes.ReplaceText(note, solidColorBrushes[0]);
                                        i = 0;
                                    }
                                    i++;
                                    i = i == 3 ? 0 : i;
                                }
                            }
                            catch (Exception ex)
                            {
                                IMethods.WriteToErrorLog("ProjectWindow => Note population - LoadEngineeringProject", ex.Message, user);
                            }
                        }
                        //Notes.Text = engineeringProject.Notes;
                        Notes.IsReadOnly = true;
                        DieNumber.Text = engineeringProject.DieNumber;
                        DieNumber.IsEnabled = false;
                        DieShape.Text = engineeringProject.DieShape;
                        DieShape.IsEnabled = false;
                        TabletWidth.Text = engineeringProject.Width == null ? "" : (engineeringProject.Width * conversion).ToString().TrimEnd('0');
                        TabletWidth.IsEnabled = false;
                        TabletLength.Text = engineeringProject.Length == null ? "" : (engineeringProject.Length * conversion).ToString().TrimEnd('0');
                        TabletLength.IsEnabled = false;
                        DieTolerances.Text = engineeringProject.DieTolerances;
                        DieTolerances.IsEnabled = false;
                        UpperCupType.Text = engineeringProject.UpperCupType == null ? "" : engineeringProject.UpperCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.UpperCupType).Description.Trim();
                        UpperCupType.IsEnabled = false;
                        UpperHobNumber.Text = engineeringProject.UpperHobNumber.Trim();
                        UpperHobNumber.IsEnabled = false;
                        UpperCupDepth.Text = engineeringProject.UpperCupDepth == null ? "" : (engineeringProject.UpperCupDepth * conversion).ToString().TrimEnd('0');
                        UpperCupDepth.IsEnabled = false;
                        UpperLand.Text = engineeringProject.UpperLand == null ? "" : (engineeringProject.UpperLand * conversion).ToString().TrimEnd('0');
                        UpperLand.IsEnabled = false;
                        UpperHobDescription.Text = engineeringProject.UpperHobDescription;
                        UpperHobDescription.IsEnabled = false;
                        UpperTolerances.Text = engineeringProject.UpperTolerances;
                        UpperTolerances.IsEnabled = false;
                        LowerCupType.Text = engineeringProject.LowerCupType == null ? "" : engineeringProject.LowerCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.LowerCupType).Description.Trim();
                        LowerCupType.IsEnabled = false;
                        LowerHobNumber.Text = engineeringProject.LowerHobNumber.Trim();
                        LowerHobNumber.IsEnabled = false;
                        LowerCupDepth.Text = engineeringProject.LowerCupDepth == null ? "" : (engineeringProject.LowerCupDepth * conversion).ToString().TrimEnd('0');
                        LowerCupDepth.IsEnabled = false;
                        LowerLand.Text = engineeringProject.LowerLand == null ? "" : (engineeringProject.LowerLand * conversion).ToString().TrimEnd('0');
                        LowerLand.IsEnabled = false;
                        LowerHobDescription.Text = engineeringProject.LowerHobDescription;
                        LowerHobDescription.IsEnabled = false;
                        LowerTolerances.Text = engineeringProject.LowerTolerances;
                        LowerTolerances.IsEnabled = false;
                        ShortRejectCupType.Text = engineeringProject.ShortRejectCupType == null ? "" : engineeringProject.ShortRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.ShortRejectCupType).Description.Trim();
                        ShortRejectCupType.IsEnabled = false;
                        ShortRejectHobNumber.Text = engineeringProject.ShortRejectHobNumber.Trim();
                        ShortRejectHobNumber.IsEnabled = false;
                        ShortRejectCupDepth.Text = engineeringProject.ShortRejectCupDepth == null ? "" : (engineeringProject.ShortRejectCupDepth * conversion).ToString().TrimEnd('0');
                        ShortRejectCupDepth.IsEnabled = false;
                        ShortRejectLand.Text = engineeringProject.ShortRejectLand == null ? "" : (engineeringProject.ShortRejectLand * conversion).ToString().TrimEnd('0');
                        ShortRejectLand.IsEnabled = false;
                        ShortRejectHobDescription.Text = engineeringProject.ShortRejectHobDescription;
                        ShortRejectHobDescription.IsEnabled = false;
                        ShortRejectTolerances.Text = engineeringProject.ShortRejectTolerances;
                        ShortRejectTolerances.IsEnabled = false;
                        LongRejectCupType.Text = engineeringProject.LongRejectCupType == null ? "" : engineeringProject.LongRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProject.LongRejectCupType).Description.Trim();
                        LongRejectCupType.IsEnabled = false;
                        LongRejectHobNumber.Text = engineeringProject.LongRejectHobNumber.Trim();
                        LongRejectHobNumber.IsEnabled = false;
                        LongRejectCupDepth.Text = engineeringProject.LongRejectCupDepth == null ? "" : (engineeringProject.LongRejectCupDepth * conversion).ToString().TrimEnd('0');
                        LongRejectCupDepth.IsEnabled = false;
                        LongRejectLand.Text = engineeringProject.LongRejectLand == null ? "" : (engineeringProject.LongRejectLand * conversion).ToString().TrimEnd('0');
                        LongRejectLand.IsEnabled = false;
                        LongRejectHobDescription.Text = engineeringProject.LongRejectHobDescription;
                        LongRejectHobDescription.IsEnabled = false;
                        LongRejectTolerances.Text = engineeringProject.LongRejectTolerances;
                        LongRejectTolerances.IsEnabled = false;

                        MultiTipSketch.IsChecked = engineeringProject.MultiTipSketch;
                        MultiTipSketch.IsEnabled = false;
                        SketchID.Text = engineeringProject.MultiTipSketchID;
                        SketchID.IsEnabled = false;
                        if (SketchID.Text.Length > 0)
                        {
                            using var _nat02Context = new NAT02Context();
                            MultiTipSketchInformation multiTipSketchInformation = _nat02Context.MultiTipSketchInformation.First(s => s.ID == Convert.ToInt32(SketchID.Text));
                            string ID = multiTipSketchInformation.ID.ToString();
                            string dieNumber = multiTipSketchInformation.DieNumber.ToString();
                            string width = ((decimal)multiTipSketchInformation.Width).ToString("F4", CultureInfo.InvariantCulture);
                            string type = multiTipSketchInformation.AssembledOrSolid == 'S' ? "SOLID" : multiTipSketchInformation.AssembledOrSolid == 'S' ? "ASSEMBLED" : "NEITHER_ASSEMBLED_OR_SOLID";
                            string tipQTY = multiTipSketchInformation.TotalNumberOfTips.ToString();
                            if (multiTipSketchInformation.Length != null)
                            {
                                string length = ((double)multiTipSketchInformation.Length).ToString("F4", CultureInfo.InvariantCulture);
                                string path = @"R:\tools\MULTI-TIP SKETCHES\" + width + " X " + length + "\\" + dieNumber + "\\" + type + "\\" + tipQTY + "-TIP " + type + "\\" + ID + "\\" + "MULTI TIP SKETCH " + ID + ".pdf";
                                if (System.IO.File.Exists(path))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Visible));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = new Uri(path, UriKind.Absolute)));
                                }
                            }
                            else
                            {
                                string path = @"R:\tools\MULTI-TIP SKETCHES\" + width + "\\" + dieNumber + "\\" + type + "\\" + tipQTY + "-TIP " + type + "\\" + ID + "\\" + "MULTI TIP SKETCH " + ID + ".pdf";
                                if (System.IO.File.Exists(path))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Visible));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = new Uri(path, UriKind.Absolute)));
                                }
                            }
                            _nat02Context.Dispose();
                        }
                        if (engineeringProject.MultiTipSolid == true)
                        {
                            MultiTipStyle.Text = "SOLID";
                        }
                        if (engineeringProject.MultiTipAssembled == true)
                        {
                            MultiTipStyle.Text = "ASSEMBLED";
                        }
                        MultiTipStyle.IsEnabled = false;

                        EngineeringTabletProjects tabletProject = null;
                        EngineeringToolProjects toolProject = null;
                        // For tablets?
                        if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            tabletProject = _projectsContext.EngineeringTabletProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                            TabletsRequired.IsChecked = true;
                            TabletsRequired.IsEnabled = false;
                            UpperTabletDrawing.IsChecked = tabletProject.UpperRequired;
                            UpperTabletDrawing.IsEnabled = false;
                            LowerTabletDrawing.IsChecked = tabletProject.LowerRequired;
                            LowerTabletDrawing.IsEnabled = false;
                            ShortRejectTabletDrawing.IsChecked = tabletProject.ShortRejectRequired;
                            ShortRejectTabletDrawing.IsEnabled = false;
                            LongRejectTabletDrawing.IsChecked = tabletProject.LongRejectRequired;
                            LongRejectTabletDrawing.IsEnabled = false;
                            Density.Text = tabletProject.Density.ToString();
                            Density.IsEnabled = false;
                            DensityUnits.SelectedItem = tabletProject.DensityUnits;
                            DensityUnits.IsEnabled = false;
                            Mass.Text = tabletProject.Mass.ToString();
                            Mass.IsEnabled = false;
                            MassUnits.SelectedItem = tabletProject.MassUnits;
                            MassUnits.IsEnabled = false;
                            Volume.Text = tabletProject.Volume.ToString();
                            Volume.IsEnabled = false;
                            VolumeUnits.SelectedItem = tabletProject.VolumeUnits;
                            VolumeUnits.IsEnabled = false;
                            TargetThickness.Text = tabletProject.TargetThickness.ToString();
                            TargetThickness.IsEnabled = false;
                            TargetThicknessUnits.SelectedItem = tabletProject.TargetThicknessUnits;
                            TargetThicknessUnits.IsEnabled = false;
                            FilmCoat.IsChecked = tabletProject.FilmCoated;
                            FilmCoat.IsEnabled = false;
                            FilmCoatStyle.SelectedItem = tabletProject.FilmCoatType;
                            FilmCoatStyle.IsEnabled = false;
                            PrePick.IsChecked = tabletProject.PrePick;
                            PrePick.IsEnabled = false;
                            PrePickAmount.Text = tabletProject.PrePickAmount.ToString();
                            PrePickAmount.IsEnabled = false;
                            PrePickUnits.SelectedItem = tabletProject.PrePickUnits;
                            PrePickUnits.IsEnabled = false;
                            Taper.IsChecked = tabletProject.Taper;
                            Taper.IsEnabled = false;
                            TaperAmount.Text = tabletProject.TaperAmount.ToString();
                            TaperAmount.IsEnabled = false;
                            TaperUnits.SelectedItem = tabletProject.TaperUnits;
                            TaperUnits.IsEnabled = false;
                        }
                        else
                        {
                            TabletsRequired.IsChecked = false;
                            TabletsRequired.IsEnabled = false;
                        }
                        // For Tools?
                        if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            toolProject = _projectsContext.EngineeringToolProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                            CoreRod = toolProject.LowerCoreRod;
                            CoreRodSteelID = toolProject.LowerCoreRodSteelID;
                            CoreRodKey = toolProject.LowerCoreRodKey;
                            CoreRodKeySteelID = toolProject.LowerCoreRodKeySteelID;
                            CoreRodKeyCollar = toolProject.LowerCoreRodKeyCollar;
                            CoreRodKeyCollarSteelID = toolProject.LowerCoreRodKeyCollarSteelID;
                            CoreRodPunch = toolProject.LowerCoreRodPunch;
                            CoreRodPunchSteelID = toolProject.LowerCoreRodPunchSteelID;
                            CoreRodButton.IsEnabled = toolProject.LowerCoreRod || toolProject.LowerCoreRodKey || toolProject.LowerCoreRodKeyCollar || toolProject.LowerCoreRodPunch;

                            ToolsRequired.IsChecked = true;
                            ToolsRequired.IsEnabled = false;
                            UpperPunch.IsChecked = toolProject.UpperPunch;
                            UpperPunch.IsEnabled = false;
                            UpperPunchSteelID.Text = toolProject.UpperPunchSteelID;
                            UpperPunchSteelID.IsEnabled = false;
                            UpperAssembly.IsChecked = toolProject.UpperAssembly;
                            UpperAssembly.IsEnabled = false;
                            UpperCap.IsChecked = toolProject.UpperCap;
                            UpperCap.IsEnabled = false;
                            UpperCapSteelID.Text = toolProject.UpperCapSteelID;
                            UpperCapSteelID.IsEnabled = false;
                            UpperHolder.IsChecked = toolProject.UpperHolder;
                            UpperHolder.IsEnabled = false;
                            UpperHolderSteelID.Text = toolProject.UpperHolderSteelID;
                            UpperHolderSteelID.IsEnabled = false;
                            UpperHead.IsChecked = toolProject.UpperHead;
                            UpperHead.IsEnabled = false;
                            UpperHeadSteelID.Text = toolProject.UpperHeadSteelID;
                            UpperHeadSteelID.IsEnabled = false;
                            UpperTip.IsChecked = toolProject.UpperTip;
                            UpperTip.IsEnabled = false;
                            UpperTipSteelID.Text = toolProject.UpperTipSteelID;
                            UpperTipSteelID.IsEnabled = false;
                            LowerPunch.IsChecked = toolProject.LowerPunch;
                            LowerPunch.IsEnabled = false;
                            LowerPunchSteelID.Text = toolProject.LowerPunchSteelID;
                            LowerPunchSteelID.IsEnabled = false;
                            LowerAssembly.IsChecked = toolProject.LowerAssembly;
                            LowerAssembly.IsEnabled = false;
                            LowerCap.IsChecked = toolProject.LowerCap;
                            LowerCap.IsEnabled = false;
                            LowerCapSteelID.Text = toolProject.LowerCapSteelID;
                            LowerCapSteelID.IsEnabled = false;
                            LowerHolder.IsChecked = toolProject.LowerHolder;
                            LowerHolder.IsEnabled = false;
                            LowerHolderSteelID.Text = toolProject.LowerHolderSteelID;
                            LowerHolderSteelID.IsEnabled = false;
                            LowerHead.IsChecked = toolProject.LowerHead;
                            LowerHead.IsEnabled = false;
                            LowerHeadSteelID.Text = toolProject.LowerHeadSteelID;
                            LowerHeadSteelID.IsEnabled = false;
                            LowerTip.IsChecked = toolProject.LowerTip;
                            LowerTip.IsEnabled = false;
                            LowerTipSteelID.Text = toolProject.LowerTipSteelID;
                            LowerTipSteelID.IsEnabled = false;
                            ShortRejectPunch.IsChecked = toolProject.ShortRejectPunch;
                            ShortRejectPunch.IsEnabled = false;
                            ShortRejectPunchSteelID.Text = toolProject.ShortRejectPunchSteelID;
                            ShortRejectPunchSteelID.IsEnabled = false;
                            ShortRejectAssembly.IsChecked = toolProject.ShortRejectAssembly;
                            ShortRejectAssembly.IsEnabled = false;
                            ShortRejectCap.IsChecked = toolProject.ShortRejectCap;
                            ShortRejectCap.IsEnabled = false;
                            ShortRejectCapSteelID.Text = toolProject.ShortRejectCapSteelID;
                            ShortRejectCapSteelID.IsEnabled = false;
                            ShortRejectHolder.IsChecked = toolProject.ShortRejectHolder;
                            ShortRejectHolder.IsEnabled = false;
                            ShortRejectHolderSteelID.Text = toolProject.ShortRejectHolderSteelID;
                            ShortRejectHolderSteelID.IsEnabled = false;
                            ShortRejectHead.IsChecked = toolProject.ShortRejectHead;
                            ShortRejectHead.IsEnabled = false;
                            ShortRejectHeadSteelID.Text = toolProject.ShortRejectHeadSteelID;
                            ShortRejectHeadSteelID.IsEnabled = false;
                            ShortRejectTip.IsChecked = toolProject.ShortRejectTip;
                            ShortRejectTip.IsEnabled = false;
                            ShortRejectTipSteelID.Text = toolProject.ShortRejectTipSteelID;
                            ShortRejectTipSteelID.IsEnabled = false;
                            LongRejectPunch.IsChecked = toolProject.LongRejectPunch;
                            LongRejectPunch.IsEnabled = false;
                            LongRejectPunchSteelID.Text = toolProject.LongRejectPunchSteelID;
                            LongRejectPunchSteelID.IsEnabled = false;
                            LongRejectAssembly.IsChecked = toolProject.LongRejectAssembly;
                            LongRejectAssembly.IsEnabled = false;
                            LongRejectCap.IsChecked = toolProject.LongRejectCap;
                            LongRejectCap.IsEnabled = false;
                            LongRejectCapSteelID.Text = toolProject.LongRejectCapSteelID;
                            LongRejectCapSteelID.IsEnabled = false;
                            LongRejectHolder.IsChecked = toolProject.LongRejectHolder;
                            LongRejectHolder.IsEnabled = false;
                            LongRejectHolderSteelID.Text = toolProject.LongRejectHolderSteelID;
                            LongRejectHolderSteelID.IsEnabled = false;
                            LongRejectHead.IsChecked = toolProject.LongRejectHead;
                            LongRejectHead.IsEnabled = false;
                            LongRejectHeadSteelID.Text = toolProject.LongRejectHeadSteelID;
                            LongRejectHeadSteelID.IsEnabled = false;
                            LongRejectTip.IsChecked = toolProject.LongRejectTip;
                            LongRejectTip.IsEnabled = false;
                            LongRejectTipSteelID.Text = toolProject.LongRejectTipSteelID;
                            LongRejectTipSteelID.IsEnabled = false;
                            Alignment.IsChecked = toolProject.Alignment;
                            Alignment.IsEnabled = false;
                            AlignmentSteelID.Text = toolProject.AlignmentSteelID;
                            AlignmentSteelID.IsEnabled = false;
                            Key.IsChecked = toolProject.Key;
                            Key.IsEnabled = false;
                            KeySteelID.Text = toolProject.KeySteelID;
                            KeySteelID.IsEnabled = false;
                            Misc.IsChecked = toolProject.Misc;
                            Misc.IsEnabled = false;
                            MiscSteelID.Text = toolProject.MiscSteelID;
                            MiscSteelID.IsEnabled = false;
                            NumberOfTips.Text = engineeringProject.NumberOfTips < 2 ? "" : engineeringProject.NumberOfTips.ToString();
                            NumberOfTips.IsEnabled = false;
                            Die.IsChecked = toolProject.Die;
                            Die.IsEnabled = false;
                            DieSteelID.Text = toolProject.DieSteelID;
                            DieSteelID.IsEnabled = false;
                            DieAssembly.IsChecked = toolProject.DieAssembly;
                            DieAssembly.IsEnabled = false;
                            DieComponent.IsChecked = toolProject.DieComponent;
                            DieComponent.IsEnabled = false;
                            DieComponentSteelID.Text = toolProject.DieComponentSteelID;
                            DieComponentSteelID.IsEnabled = false;
                            DieHolder.IsChecked = toolProject.DieHolder;
                            DieHolder.IsEnabled = false;
                            DieHolderSteelID.Text = toolProject.DieHolderSteelID;
                            DieHolderSteelID.IsEnabled = false;
                            DieInsert.IsChecked = toolProject.DieInsert;
                            DieInsert.IsEnabled = false;
                            DieInsertSteelID.Text = toolProject.DieInsertSteelID;
                            DieInsertSteelID.IsEnabled = false;
                            DiePlate.IsChecked = toolProject.DiePlate;
                            DiePlate.IsEnabled = false;
                            DiePlateSteelID.Text = toolProject.DiePlateSteelID;
                            DiePlateSteelID.IsEnabled = false;
                            DieSegment.IsChecked = toolProject.DieSegment;
                            DieSegment.IsEnabled = false;
                            DieSegmentSteelID.Text = toolProject.DieSegmentSteelID;
                            DieSegmentSteelID.IsEnabled = false;
                            KeyType.IsEditable = true;
                            KeyType.Text = toolProject.KeyType;
                            KeyType.IsEditable = false;
                            KeyType.IsEnabled = false;
                            KeyAngle.Text = toolProject.KeyAngle.ToString();
                            KeyAngle.IsEnabled = false;
                            KeyOrientation.Text = toolProject.KeyIsClockWise == true ? "CW" : toolProject.KeyIsClockWise == false ? "CCW" : "";
                            KeyOrientation.IsEnabled = false;
                            UpperKeyed.IsChecked = toolProject.UpperKeyed;
                            UpperKeyed.IsEnabled = false;
                            LowerKeyed.IsChecked = toolProject.LowerKeyed;
                            LowerKeyed.IsEnabled = false;
                            ShortRejectKeyed.IsChecked = toolProject.ShortRejectKeyed;
                            ShortRejectKeyed.IsEnabled = false;
                            LongRejectKeyed.IsChecked = toolProject.LongRejectKeyed;
                            LongRejectKeyed.IsEnabled = false;
                            UpperGrooveType.Text = toolProject.UpperGrooveType;
                            UpperGrooveType.IsEnabled = false;
                            LowerGrooveType.Text = toolProject.LowerGrooveType;
                            LowerGrooveType.IsEnabled = false;
                            UpperGroove.IsChecked = toolProject.UpperGroove;
                            UpperGroove.IsEnabled = false;
                            LowerGroove.IsChecked = toolProject.LowerGroove;
                            LowerGroove.IsEnabled = false;
                            ShortRejectGroove.IsChecked = toolProject.ShortRejectGroove;
                            ShortRejectGroove.IsEnabled = false;
                            LongRejectGroove.IsChecked = toolProject.LongRejectGroove;
                            LongRejectGroove.IsEnabled = false;
                            HeadType.Text = toolProject.HeadType;
                            HeadType.IsEnabled = false;
                            CarbideTips.IsChecked = toolProject.CarbideTips;
                            CarbideTips.IsEnabled = false;
                            MachineNotes.Text = toolProject.MachineNotes;
                            MachineNotes.IsEnabled = false;
                        }
                        else
                        {
                            ToolsRequired.IsChecked = false;
                            ToolsRequired.IsEnabled = false;
                        }
                    }
                }
                else
                {
                    _projectsContext.Dispose();
                    _nat01Context.Dispose();
                    if (MessageBox.Show("This project does not exist in the database.\n This window will now close", "Cannot Find Project", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK) == MessageBoxResult.OK)
                    {
                        string path = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber;
                        try
                        {
                            if (projectRevNumber != "0")
                            {
                                if (System.IO.Directory.Exists(path + "_" + projectRevNumber + @"\"))
                                {
                                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path + "_" + projectRevNumber + @"\");
                                }
                                else
                                {
                                    if (!System.IO.Directory.Exists(path + @"\"))
                                        System.IO.Directory.CreateDirectory(path + @"\");
                                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path + @"\");
                                }
                            }
                            else
                            {
                                if (!System.IO.Directory.Exists(path + @"\"))
                                    System.IO.Directory.CreateDirectory(path + @"\");
                                System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", path + @"\");
                            }
                        }
                        catch (Exception ex)
                        {
                            // MessageBox.Show(ex.Message);
                            // WriteToErrorLog("ProjectSearchButton_Click - Before new window instance", ex.Message);
                        }
                        Close();
                    }
                }
                if (MultiTipSketch.IsChecked != true)
                {
                    MultiTipSketchTabItem.IsEnabled = false;
                }
                _projectsContext.Dispose();
                _nat01Context.Dispose();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => LoadEngineeringProject", ex.Message, user);
            }
        }
        private void LoadCopiedProject(string project, string projectRev)
        {
            try
            {
                using var _projectsContext = new ProjectsContext();
                using var _nat01Context = new NAT01Context();

                // Get copied project object
                EngineeringProjects engineeringProjectCopied = null;
                EngineeringArchivedProjects engineeringArchiveProjectCopied = null;
                if (_projectsContext.EngineeringProjects.Any(ep => ep.ProjectNumber == project && ep.RevNumber == projectRev))
                {
                    engineeringProjectCopied = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == project && ep.RevNumber == projectRev);
                }
                else if (_projectsContext.EngineeringArchivedProjects.Any(ep => ep.ProjectNumber == project && ep.RevNumber == projectRev))
                {
                    engineeringArchiveProjectCopied = _projectsContext.EngineeringArchivedProjects.First(ep => ep.ProjectNumber == project && ep.RevNumber == projectRev);
                }
                else
                {
                    _projectsContext.Dispose();
                    _nat01Context.Dispose();
                    return;
                }

                if (!Directory.Exists(projectsDirectory + projectNumber + "\\"))
                {
                    Directory.CreateDirectory(projectsDirectory + projectNumber + "\\");
                }
                ProjectNavigation.Visibility = Visibility.Hidden;
                CreationBorder.Visibility = Visibility.Visible;
                ArchivedOrInactive.Visibility = Visibility.Collapsed;
                EngineeringProjects engineeringProject = IMethods.GetBlankEngineeringProject(user, projectNumber, projectRevNumber);
                StartButton.IsEnabled = false;
                FinishButton.IsEnabled = false;
                SubmitButton.IsEnabled = false;
                CheckButton.IsEnabled = false;
                PutOnHoldButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                QuoteFolderButton.IsEnabled = false;
                LinkQuoteButton.IsEnabled = true;
                ReturnToCSR.ItemsSource = null;
                ReturnToCSR.ItemsSource = IMethods.GetDWCSRs();
                CSR.Text = user.GetDWPrincipalId();
                UnitOfMeasure.SelectedItem = engineeringProject.UnitOfMeasure;
                decimal conversion = (string)Resources["UnitsText"] == "in" ? 1 : 1 / (decimal)25.4;

                // Set fields from copied project
                if (!(engineeringProjectCopied is null))
                {
                    NewDrawing = engineeringProjectCopied.NewDrawing;
                    UpdateExistingDrawing = engineeringProjectCopied.UpdateExistingDrawing;
                    UpdateTextOnDrawing = engineeringProjectCopied.UpdateTextOnDrawing;
                    PerSampleTablet = engineeringProjectCopied.PerSampleTablet;
                    RefTabletDrawing = engineeringProjectCopied.RefTabletDrawing;
                    PerSampleTool = engineeringProjectCopied.PerSampleTool;
                    RefToolDrawing = engineeringProjectCopied.RefToolDrawing;
                    PerSuppliedPicture = engineeringProjectCopied.PerSuppliedPicture;
                    RefNatoliDrawing = engineeringProjectCopied.RefNatoliDrawing;
                    RefNonNatoliDrawing = engineeringProjectCopied.RefNonNatoliDrawing;
                    BinLocation = engineeringProjectCopied.BinLocation;
                    

                    RefOrderNumber.Text = engineeringProjectCopied.RefOrderNumber;
                    UnitOfMeasure.Text = engineeringProjectCopied.UnitOfMeasure;
                    CustomerNumber.Text = engineeringProjectCopied.CustomerNumber;
                    CustomerName.Text = engineeringProjectCopied.CustomerName;
                    ShipToNumber.Text = engineeringProjectCopied.ShipToNumber;
                    ShipToLocNumber.Text = engineeringProjectCopied.ShipToLocNumber;
                    ShipToName.Text = engineeringProjectCopied.ShipToName;
                    EndUserNumber.Text = engineeringProjectCopied.EndUserNumber;
                    EndUserLocNumber.Text = engineeringProjectCopied.EndUserLocNumber;
                    EndUserName.Text = engineeringProjectCopied.EndUserName;
                    // Product.Text = engineeringProjectCopied.Product;
                    Attention.Text = engineeringProjectCopied.Attention;
                    MachineNumber.Text = engineeringProjectCopied.MachineNumber;
                    if (!string.IsNullOrWhiteSpace(engineeringProjectCopied.MachineNumber) && Int16.TryParse(engineeringProjectCopied.MachineNumber, out short _machineNo))
                    {
                        if (_nat01Context.MachineList.Any(m => m.MachineNo == _machineNo))
                        {
                            string description = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Description.Trim();
                            string od = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od == null ? "" : ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od * conversion).ToString();
                            string ol = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol == null ? "" : ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol * conversion).ToString();
                            MachineDescription.Text = description;
                            DieOD.Text = od;
                            DieODPlaceholder.Visibility = Visibility.Collapsed;
                            DieOL.Text = ol;
                            DieOLPlaceholder.Visibility = Visibility.Collapsed;
                        }
                    }

                    // Notes.Text = engineeringProjectCopied.Notes;
                    // DieNumber.Text = engineeringProjectCopied.DieNumber;
                    // DieShape.Text = engineeringProjectCopied.DieShape;
                    // TabletWidth.Text = engineeringProjectCopied.Width == null ? "" : (engineeringProjectCopied.Width * conversion).ToString().TrimEnd('0');
                    // TabletLength.Text = engineeringProjectCopied.Length == null ? "" : (engineeringProjectCopied.Length * conversion).ToString().TrimEnd('0');
                    DieTolerances.Text = engineeringProjectCopied.DieTolerances;
                    // UpperCupType.Text = engineeringProjectCopied.UpperCupType == null ? "" : engineeringProjectCopied.UpperCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProjectCopied.UpperCupType).Description.Trim();
                    // UpperHobNumber.Text = engineeringProjectCopied.UpperHobNumber.Trim();
                    // UpperCupDepth.Text = engineeringProjectCopied.UpperCupDepth == null ? "" : (engineeringProjectCopied.UpperCupDepth * conversion).ToString().TrimEnd('0');
                    // UpperLand.Text = engineeringProjectCopied.UpperLand == null ? "" : (engineeringProjectCopied.UpperLand * conversion).ToString().TrimEnd('0');
                    // UpperHobDescription.Text = engineeringProjectCopied.UpperHobDescription;
                    if (!string.IsNullOrWhiteSpace(UpperHobDescription.Text.ToString()) && UpperHobDescription.Text.ToString().Length > 0)
                    {
                        UpperHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        UpperHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    UpperTolerances.Text = engineeringProjectCopied.UpperTolerances;
                    // LowerCupType.Text = engineeringProjectCopied.LowerCupType == null ? "" : engineeringProjectCopied.LowerCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProjectCopied.LowerCupType).Description.Trim();
                    // LowerHobNumber.Text = engineeringProjectCopied.LowerHobNumber.Trim();
                    // LowerCupDepth.Text = engineeringProjectCopied.LowerCupDepth == null ? "" : (engineeringProjectCopied.LowerCupDepth * conversion).ToString().TrimEnd('0');
                    // LowerLand.Text = engineeringProjectCopied.LowerLand == null ? "" : (engineeringProjectCopied.LowerLand * conversion).ToString().TrimEnd('0');
                    // LowerHobDescription.Text = engineeringProjectCopied.LowerHobDescription;
                    if (!string.IsNullOrWhiteSpace(LowerHobDescription.Text.ToString()) && LowerHobDescription.Text.ToString().Length > 0)
                    {
                        LowerHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LowerHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    LowerTolerances.Text = engineeringProjectCopied.LowerTolerances;
                    // ShortRejectCupType.Text = engineeringProjectCopied.ShortRejectCupType == null ? "" : engineeringProjectCopied.ShortRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProjectCopied.ShortRejectCupType).Description.Trim();
                    // ShortRejectHobNumber.Text = engineeringProjectCopied.ShortRejectHobNumber.Trim();
                    // ShortRejectCupDepth.Text = engineeringProjectCopied.ShortRejectCupDepth == null ? "" : (engineeringProjectCopied.ShortRejectCupDepth * conversion).ToString().TrimEnd('0');
                    // ShortRejectLand.Text = engineeringProjectCopied.ShortRejectLand == null ? "" : (engineeringProjectCopied.ShortRejectLand * conversion).ToString().TrimEnd('0');
                    // ShortRejectHobDescription.Text = engineeringProjectCopied.ShortRejectHobDescription;
                    if (!string.IsNullOrWhiteSpace(ShortRejectHobDescription.Text.ToString()) && ShortRejectHobDescription.Text.ToString().Length > 0)
                    {
                        ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    ShortRejectTolerances.Text = engineeringProjectCopied.ShortRejectTolerances;
                    // LongRejectCupType.Text = engineeringProjectCopied.LongRejectCupType == null ? "" : engineeringProjectCopied.LongRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringProjectCopied.LongRejectCupType).Description.Trim();
                    // LongRejectHobNumber.Text = engineeringProjectCopied.LongRejectHobNumber.Trim();
                    // LongRejectCupDepth.Text = engineeringProjectCopied.LongRejectCupDepth == null ? "" : (engineeringProjectCopied.LongRejectCupDepth * conversion).ToString().TrimEnd('0');
                    // LongRejectLand.Text = engineeringProjectCopied.LongRejectLand == null ? "" : (engineeringProjectCopied.LongRejectLand * conversion).ToString().TrimEnd('0');
                    // LongRejectHobDescription.Text = engineeringProjectCopied.LongRejectHobDescription;
                    if (!string.IsNullOrWhiteSpace(LongRejectHobDescription.Text.ToString()) && LongRejectHobDescription.Text.ToString().Length > 0)
                    {
                        LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    LongRejectTolerances.Text = engineeringProjectCopied.LongRejectTolerances;

                    MultiTipSketch.IsChecked = engineeringProjectCopied.MultiTipSketch;
                    // SketchID.Text = engineeringProjectCopied.MultiTipSketchID;
                    if (engineeringProjectCopied.MultiTipSolid == true)
                    {
                        MultiTipStyle.Text = "SOLID";
                    }
                    if (engineeringProjectCopied.MultiTipAssembled == true)
                    {
                        MultiTipStyle.Text = "ASSEMBLED";
                    }

                    EngineeringTabletProjects tabletProject = null;
                    EngineeringToolProjects toolProject = null;
                    // For tablets?
                    if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == project && p.RevNumber == projectRev))
                    {
                        tabletProject = _projectsContext.EngineeringTabletProjects.First(p => p.ProjectNumber == project && p.RevNumber == projectRev);
                        TabletsRequired.IsChecked = true;
                        UpperTabletDrawing.IsChecked = tabletProject.UpperRequired;
                        LowerTabletDrawing.IsChecked = tabletProject.LowerRequired;
                        ShortRejectTabletDrawing.IsChecked = tabletProject.ShortRejectRequired;
                        LongRejectTabletDrawing.IsChecked = tabletProject.LongRejectRequired;
                        Density.Text = tabletProject.Density.ToString();
                        DensityUnits.SelectedItem = tabletProject.DensityUnits;
                        Mass.Text = tabletProject.Mass.ToString();
                        MassUnits.SelectedItem = tabletProject.MassUnits;
                        // Volume.Text = tabletProject.Volume.ToString();
                        // VolumeUnits.SelectedItem = tabletProject.VolumeUnits;
                        // TargetThickness.Text = tabletProject.TargetThickness.ToString();
                        // TargetThicknessUnits.SelectedItem = tabletProject.TargetThicknessUnits;
                        FilmCoat.IsChecked = tabletProject.FilmCoated;
                        FilmCoatStyle.SelectedItem = tabletProject.FilmCoatType;
                        PrePick.IsChecked = tabletProject.PrePick;
                        PrePickAmount.Text = tabletProject.PrePickAmount.ToString();
                        PrePickUnits.SelectedItem = tabletProject.PrePickUnits;
                        Taper.IsChecked = tabletProject.Taper;
                        TaperAmount.Text = tabletProject.TaperAmount.ToString();
                        TaperUnits.SelectedItem = tabletProject.TaperUnits;
                    }
                    else
                    {
                        TabletsRequired.IsChecked = false;
                    }
                    // For Tools?
                    if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == project && p.RevNumber == projectRev))
                    {
                        toolProject = _projectsContext.EngineeringToolProjects.First(p => p.ProjectNumber == project && p.RevNumber == projectRev);
                        CoreRod = toolProject.LowerCoreRod;
                        CoreRodSteelID = toolProject.LowerCoreRodSteelID;
                        CoreRodKey = toolProject.LowerCoreRodKey;
                        CoreRodKeySteelID = toolProject.LowerCoreRodKeySteelID;
                        CoreRodKeyCollar = toolProject.LowerCoreRodKeyCollar;
                        CoreRodKeyCollarSteelID = toolProject.LowerCoreRodKeyCollarSteelID;
                        CoreRodPunch = toolProject.LowerCoreRodPunch;
                        CoreRodPunchSteelID = toolProject.LowerCoreRodPunchSteelID;
                        CoreRodButton.IsEnabled = toolProject.LowerCoreRod || toolProject.LowerCoreRodKey || toolProject.LowerCoreRodKeyCollar || toolProject.LowerCoreRodPunch;

                        ToolsRequired.IsChecked = true;
                        UpperPunch.IsChecked = toolProject.UpperPunch;
                        UpperPunchSteelID.Text = toolProject.UpperPunchSteelID;
                        UpperAssembly.IsChecked = toolProject.UpperAssembly;
                        UpperCap.IsChecked = toolProject.UpperCap;
                        UpperCapSteelID.Text = toolProject.UpperCapSteelID;
                        UpperHolder.IsChecked = toolProject.UpperHolder;
                        UpperHolderSteelID.Text = toolProject.UpperHolderSteelID;
                        UpperHead.IsChecked = toolProject.UpperHead;
                        UpperHeadSteelID.Text = toolProject.UpperHeadSteelID;
                        UpperTip.IsChecked = toolProject.UpperTip;
                        UpperTipSteelID.Text = toolProject.UpperTipSteelID;
                        LowerPunch.IsChecked = toolProject.LowerPunch;
                        LowerPunchSteelID.Text = toolProject.LowerPunchSteelID;
                        LowerAssembly.IsChecked = toolProject.LowerAssembly;
                        LowerCap.IsChecked = toolProject.LowerCap;
                        LowerCapSteelID.Text = toolProject.LowerCapSteelID;
                        LowerHolder.IsChecked = toolProject.LowerHolder;
                        LowerHolderSteelID.Text = toolProject.LowerHolderSteelID;
                        LowerHead.IsChecked = toolProject.LowerHead;
                        LowerHeadSteelID.Text = toolProject.LowerHeadSteelID;
                        LowerTip.IsChecked = toolProject.LowerTip;
                        LowerTipSteelID.Text = toolProject.LowerTipSteelID;
                        ShortRejectPunch.IsChecked = toolProject.ShortRejectPunch;
                        ShortRejectPunchSteelID.Text = toolProject.ShortRejectPunchSteelID;
                        ShortRejectAssembly.IsChecked = toolProject.ShortRejectAssembly;
                        ShortRejectCap.IsChecked = toolProject.ShortRejectCap;
                        ShortRejectCapSteelID.Text = toolProject.ShortRejectCapSteelID;
                        ShortRejectHolder.IsChecked = toolProject.ShortRejectHolder;
                        ShortRejectHolderSteelID.Text = toolProject.ShortRejectHolderSteelID;
                        ShortRejectHead.IsChecked = toolProject.ShortRejectHead;
                        ShortRejectHeadSteelID.Text = toolProject.ShortRejectHeadSteelID;
                        ShortRejectTip.IsChecked = toolProject.ShortRejectTip;
                        ShortRejectTipSteelID.Text = toolProject.ShortRejectTipSteelID;
                        LongRejectPunch.IsChecked = toolProject.LongRejectPunch;
                        LongRejectPunchSteelID.Text = toolProject.LongRejectPunchSteelID;
                        LongRejectAssembly.IsChecked = toolProject.LongRejectAssembly;
                        LongRejectCap.IsChecked = toolProject.LongRejectCap;
                        LongRejectCapSteelID.Text = toolProject.LongRejectCapSteelID;
                        LongRejectHolder.IsChecked = toolProject.LongRejectHolder;
                        LongRejectHolderSteelID.Text = toolProject.LongRejectHolderSteelID;
                        LongRejectHead.IsChecked = toolProject.LongRejectHead;
                        LongRejectHeadSteelID.Text = toolProject.LongRejectHeadSteelID;
                        LongRejectTip.IsChecked = toolProject.LongRejectTip;
                        LongRejectTipSteelID.Text = toolProject.LongRejectTipSteelID;
                        Alignment.IsChecked = toolProject.Alignment;
                        AlignmentSteelID.Text = toolProject.AlignmentSteelID;
                        Key.IsChecked = toolProject.Key;
                        KeySteelID.Text = toolProject.KeySteelID;
                        Misc.IsChecked = toolProject.Misc;
                        MiscSteelID.Text = toolProject.MiscSteelID;
                        NumberOfTips.Text = engineeringProjectCopied.NumberOfTips < 2 ? "" : engineeringProjectCopied.NumberOfTips.ToString();
                        Die.IsChecked = toolProject.Die;
                        DieSteelID.Text = toolProject.DieSteelID;
                        DieAssembly.IsChecked = toolProject.DieAssembly;
                        DieComponent.IsChecked = toolProject.DieComponent;
                        DieComponentSteelID.Text = toolProject.DieComponentSteelID;
                        DieHolder.IsChecked = toolProject.DieHolder;
                        DieHolderSteelID.Text = toolProject.DieHolderSteelID;
                        DieInsert.IsChecked = toolProject.DieInsert;
                        DieInsertSteelID.Text = toolProject.DieInsertSteelID;
                        DiePlate.IsChecked = toolProject.DiePlate;
                        DiePlateSteelID.Text = toolProject.DiePlateSteelID;
                        DieSegment.IsChecked = toolProject.DieSegment;
                        DieSegmentSteelID.Text = toolProject.DieSegmentSteelID;
                        KeyType.Text = toolProject.KeyType;
                        KeyAngle.Text = toolProject.KeyAngle.ToString();
                        KeyOrientation.Text = toolProject.KeyIsClockWise == true ? "CW" : toolProject.KeyIsClockWise == false ? "CCW" : "";
                        UpperKeyed.IsChecked = toolProject.UpperKeyed;
                        LowerKeyed.IsChecked = toolProject.LowerKeyed;
                        ShortRejectKeyed.IsChecked = toolProject.ShortRejectKeyed;
                        LongRejectKeyed.IsChecked = toolProject.LongRejectKeyed;
                        UpperGrooveType.Text = toolProject.UpperGrooveType;
                        LowerGrooveType.Text = toolProject.LowerGrooveType;
                        UpperGroove.IsChecked = toolProject.UpperGroove;
                        LowerGroove.IsChecked = toolProject.LowerGroove;
                        ShortRejectGroove.IsChecked = toolProject.ShortRejectGroove;
                        LongRejectGroove.IsChecked = toolProject.LongRejectGroove;
                        HeadType.Text = toolProject.HeadType;
                        CarbideTips.IsChecked = toolProject.CarbideTips;
                        MachineNotes.Text = toolProject.MachineNotes;
                        if (!_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) || _projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber && p.TabletChecked == true))
                        {
                            ToolsTabItem.IsSelected = true;
                            CurrentProjectType.Text = "TOOLS";
                            RefreshRoutingButtons();
                        }
                    }
                    else
                    {
                        ToolsRequired.IsChecked = false;
                    }
                }
                else if (!(engineeringArchiveProjectCopied is null))
                {
                    NewDrawing = engineeringArchiveProjectCopied.NewDrawing;
                    UpdateExistingDrawing = engineeringArchiveProjectCopied.UpdateExistingDrawing;
                    UpdateTextOnDrawing = engineeringArchiveProjectCopied.UpdateTextOnDrawing;
                    PerSampleTablet = engineeringArchiveProjectCopied.PerSampleTablet;
                    RefTabletDrawing = engineeringArchiveProjectCopied.RefTabletDrawing;
                    PerSampleTool = engineeringArchiveProjectCopied.PerSampleTool;
                    RefToolDrawing = engineeringArchiveProjectCopied.RefToolDrawing;
                    PerSuppliedPicture = engineeringArchiveProjectCopied.PerSuppliedPicture;
                    RefNatoliDrawing = engineeringArchiveProjectCopied.RefNatoliDrawing;
                    RefNonNatoliDrawing = engineeringArchiveProjectCopied.RefNonNatoliDrawing;
                    BinLocation = engineeringArchiveProjectCopied.BinLocation;


                    RefOrderNumber.Text = engineeringArchiveProjectCopied.RefOrderNumber;
                    UnitOfMeasure.Text = engineeringArchiveProjectCopied.UnitOfMeasure;
                    CustomerNumber.Text = engineeringArchiveProjectCopied.CustomerNumber;
                    CustomerName.Text = engineeringArchiveProjectCopied.CustomerName;
                    ShipToNumber.Text = engineeringArchiveProjectCopied.ShipToNumber;
                    ShipToLocNumber.Text = engineeringArchiveProjectCopied.ShipToLocNumber;
                    ShipToName.Text = engineeringArchiveProjectCopied.ShipToName;
                    EndUserNumber.Text = engineeringArchiveProjectCopied.EndUserNumber;
                    EndUserLocNumber.Text = engineeringArchiveProjectCopied.EndUserLocNumber;
                    EndUserName.Text = engineeringArchiveProjectCopied.EndUserName;
                    // Product.Text = engineeringArchiveProjectCopied.Product;
                    Attention.Text = engineeringArchiveProjectCopied.Attention;
                    MachineNumber.Text = engineeringArchiveProjectCopied.MachineNumber;
                    if (!string.IsNullOrWhiteSpace(engineeringArchiveProjectCopied.MachineNumber) && Int16.TryParse(engineeringArchiveProjectCopied.MachineNumber, out short _machineNo))
                    {
                        if (_nat01Context.MachineList.Any(m => m.MachineNo == _machineNo))
                        {
                            string description = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Description.Trim();
                            string od = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od == null ? "" : ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od * conversion).ToString();
                            string ol = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol == null ? "" : ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol * conversion).ToString();
                            MachineDescription.Text = description;
                            DieOD.Text = od;
                            DieODPlaceholder.Visibility = Visibility.Collapsed;
                            DieOL.Text = ol;
                            DieOLPlaceholder.Visibility = Visibility.Collapsed;
                        }
                    }

                    // Notes.Text = engineeringArchiveProjectCopied.Notes;
                    // DieNumber.Text = engineeringArchiveProjectCopied.DieNumber;
                    // DieShape.Text = engineeringArchiveProjectCopied.DieShape;
                    // TabletWidth.Text = engineeringArchiveProjectCopied.Width == null ? "" : (engineeringArchiveProjectCopied.Width * conversion).ToString().TrimEnd('0');
                    // TabletLength.Text = engineeringArchiveProjectCopied.Length == null ? "" : (engineeringArchiveProjectCopied.Length * conversion).ToString().TrimEnd('0');
                    DieTolerances.Text = engineeringArchiveProjectCopied.DieTolerances;
                    // UpperCupType.Text = engineeringArchiveProjectCopied.UpperCupType == null ? "" : engineeringArchiveProjectCopied.UpperCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringArchiveProjectCopied.UpperCupType).Description.Trim();
                    // UpperHobNumber.Text = engineeringArchiveProjectCopied.UpperHobNumber.Trim();
                    // UpperCupDepth.Text = engineeringArchiveProjectCopied.UpperCupDepth == null ? "" : (engineeringArchiveProjectCopied.UpperCupDepth * conversion).ToString().TrimEnd('0');
                    // UpperLand.Text = engineeringArchiveProjectCopied.UpperLand == null ? "" : (engineeringArchiveProjectCopied.UpperLand * conversion).ToString().TrimEnd('0');
                    // UpperHobDescription.Text = engineeringArchiveProjectCopied.UpperHobDescription;
                    if (!string.IsNullOrWhiteSpace(UpperHobDescription.Text.ToString()) && UpperHobDescription.Text.ToString().Length > 0)
                    {
                        UpperHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        UpperHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    UpperTolerances.Text = engineeringArchiveProjectCopied.UpperTolerances;
                    // LowerCupType.Text = engineeringArchiveProjectCopied.LowerCupType == null ? "" : engineeringArchiveProjectCopied.LowerCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringArchiveProjectCopied.LowerCupType).Description.Trim();
                    // LowerHobNumber.Text = engineeringArchiveProjectCopied.LowerHobNumber.Trim();
                    // LowerCupDepth.Text = engineeringArchiveProjectCopied.LowerCupDepth == null ? "" : (engineeringArchiveProjectCopied.LowerCupDepth * conversion).ToString().TrimEnd('0');
                    // LowerLand.Text = engineeringArchiveProjectCopied.LowerLand == null ? "" : (engineeringArchiveProjectCopied.LowerLand * conversion).ToString().TrimEnd('0');
                    // LowerHobDescription.Text = engineeringArchiveProjectCopied.LowerHobDescription;
                    if (!string.IsNullOrWhiteSpace(LowerHobDescription.Text.ToString()) && LowerHobDescription.Text.ToString().Length > 0)
                    {
                        LowerHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LowerHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    LowerTolerances.Text = engineeringArchiveProjectCopied.LowerTolerances;
                    // ShortRejectCupType.Text = engineeringArchiveProjectCopied.ShortRejectCupType == null ? "" : engineeringArchiveProjectCopied.ShortRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringArchiveProjectCopied.ShortRejectCupType).Description.Trim();
                    // ShortRejectHobNumber.Text = engineeringArchiveProjectCopied.ShortRejectHobNumber.Trim();
                    // ShortRejectCupDepth.Text = engineeringArchiveProjectCopied.ShortRejectCupDepth == null ? "" : (engineeringArchiveProjectCopied.ShortRejectCupDepth * conversion).ToString().TrimEnd('0');
                    // ShortRejectLand.Text = engineeringArchiveProjectCopied.ShortRejectLand == null ? "" : (engineeringArchiveProjectCopied.ShortRejectLand * conversion).ToString().TrimEnd('0');
                    // ShortRejectHobDescription.Text = engineeringArchiveProjectCopied.ShortRejectHobDescription;
                    if (!string.IsNullOrWhiteSpace(ShortRejectHobDescription.Text.ToString()) && ShortRejectHobDescription.Text.ToString().Length > 0)
                    {
                        ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    ShortRejectTolerances.Text = engineeringArchiveProjectCopied.ShortRejectTolerances;
                    // LongRejectCupType.Text = engineeringArchiveProjectCopied.LongRejectCupType == null ? "" : engineeringArchiveProjectCopied.LongRejectCupType + " - " + _nat01Context.CupConfig.First(c => c.CupID == engineeringArchiveProjectCopied.LongRejectCupType).Description.Trim();
                    // LongRejectHobNumber.Text = engineeringArchiveProjectCopied.LongRejectHobNumber.Trim();
                    // LongRejectCupDepth.Text = engineeringArchiveProjectCopied.LongRejectCupDepth == null ? "" : (engineeringArchiveProjectCopied.LongRejectCupDepth * conversion).ToString().TrimEnd('0');
                    // LongRejectLand.Text = engineeringArchiveProjectCopied.LongRejectLand == null ? "" : (engineeringArchiveProjectCopied.LongRejectLand * conversion).ToString().TrimEnd('0');
                    // LongRejectHobDescription.Text = engineeringArchiveProjectCopied.LongRejectHobDescription;
                    if (!string.IsNullOrWhiteSpace(LongRejectHobDescription.Text.ToString()) && LongRejectHobDescription.Text.ToString().Length > 0)
                    {
                        LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                    }
                    LongRejectTolerances.Text = engineeringArchiveProjectCopied.LongRejectTolerances;

                    MultiTipSketch.IsChecked = engineeringArchiveProjectCopied.MultiTipSketch;
                    // SketchID.Text = engineeringArchiveProjectCopied.MultiTipSketchID;
                    if (engineeringArchiveProjectCopied.MultiTipSolid == true)
                    {
                        MultiTipStyle.Text = "SOLID";
                    }
                    if (engineeringArchiveProjectCopied.MultiTipAssembled == true)
                    {
                        MultiTipStyle.Text = "ASSEMBLED";
                    }

                    EngineeringTabletProjects tabletProject = null;
                    EngineeringToolProjects toolProject = null;
                    // For tablets?
                    if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == project && p.RevNumber == projectRev))
                    {
                        tabletProject = _projectsContext.EngineeringTabletProjects.First(p => p.ProjectNumber == project && p.RevNumber == projectRev);
                        TabletsRequired.IsChecked = true;
                        UpperTabletDrawing.IsChecked = tabletProject.UpperRequired;
                        LowerTabletDrawing.IsChecked = tabletProject.LowerRequired;
                        ShortRejectTabletDrawing.IsChecked = tabletProject.ShortRejectRequired;
                        LongRejectTabletDrawing.IsChecked = tabletProject.LongRejectRequired;
                        Density.Text = tabletProject.Density.ToString();
                        DensityUnits.SelectedItem = tabletProject.DensityUnits;
                        Mass.Text = tabletProject.Mass.ToString();
                        MassUnits.SelectedItem = tabletProject.MassUnits;
                        // Volume.Text = tabletProject.Volume.ToString();
                        // VolumeUnits.SelectedItem = tabletProject.VolumeUnits;
                        // TargetThickness.Text = tabletProject.TargetThickness.ToString();
                        // TargetThicknessUnits.SelectedItem = tabletProject.TargetThicknessUnits;
                        FilmCoat.IsChecked = tabletProject.FilmCoated;
                        FilmCoatStyle.SelectedItem = tabletProject.FilmCoatType;
                        PrePick.IsChecked = tabletProject.PrePick;
                        PrePickAmount.Text = tabletProject.PrePickAmount.ToString();
                        PrePickUnits.SelectedItem = tabletProject.PrePickUnits;
                        Taper.IsChecked = tabletProject.Taper;
                        TaperAmount.Text = tabletProject.TaperAmount.ToString();
                        TaperUnits.SelectedItem = tabletProject.TaperUnits;
                    }
                    else
                    {
                        TabletsRequired.IsChecked = false;
                    }
                    // For Tools?
                    if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == project && p.RevNumber == projectRev))
                    {
                        toolProject = _projectsContext.EngineeringToolProjects.First(p => p.ProjectNumber == project && p.RevNumber == projectRev);
                        CoreRod = toolProject.LowerCoreRod;
                        CoreRodSteelID = toolProject.LowerCoreRodSteelID;
                        CoreRodKey = toolProject.LowerCoreRodKey;
                        CoreRodKeySteelID = toolProject.LowerCoreRodKeySteelID;
                        CoreRodKeyCollar = toolProject.LowerCoreRodKeyCollar;
                        CoreRodKeyCollarSteelID = toolProject.LowerCoreRodKeyCollarSteelID;
                        CoreRodPunch = toolProject.LowerCoreRodPunch;
                        CoreRodPunchSteelID = toolProject.LowerCoreRodPunchSteelID;
                        CoreRodButton.IsEnabled = toolProject.LowerCoreRod || toolProject.LowerCoreRodKey || toolProject.LowerCoreRodKeyCollar || toolProject.LowerCoreRodPunch;

                        ToolsRequired.IsChecked = true;
                        UpperPunch.IsChecked = toolProject.UpperPunch;
                        UpperPunchSteelID.Text = toolProject.UpperPunchSteelID;
                        UpperAssembly.IsChecked = toolProject.UpperAssembly;
                        UpperCap.IsChecked = toolProject.UpperCap;
                        UpperCapSteelID.Text = toolProject.UpperCapSteelID;
                        UpperHolder.IsChecked = toolProject.UpperHolder;
                        UpperHolderSteelID.Text = toolProject.UpperHolderSteelID;
                        UpperHead.IsChecked = toolProject.UpperHead;
                        UpperHeadSteelID.Text = toolProject.UpperHeadSteelID;
                        UpperTip.IsChecked = toolProject.UpperTip;
                        UpperTipSteelID.Text = toolProject.UpperTipSteelID;
                        LowerPunch.IsChecked = toolProject.LowerPunch;
                        LowerPunchSteelID.Text = toolProject.LowerPunchSteelID;
                        LowerAssembly.IsChecked = toolProject.LowerAssembly;
                        LowerCap.IsChecked = toolProject.LowerCap;
                        LowerCapSteelID.Text = toolProject.LowerCapSteelID;
                        LowerHolder.IsChecked = toolProject.LowerHolder;
                        LowerHolderSteelID.Text = toolProject.LowerHolderSteelID;
                        LowerHead.IsChecked = toolProject.LowerHead;
                        LowerHeadSteelID.Text = toolProject.LowerHeadSteelID;
                        LowerTip.IsChecked = toolProject.LowerTip;
                        LowerTipSteelID.Text = toolProject.LowerTipSteelID;
                        ShortRejectPunch.IsChecked = toolProject.ShortRejectPunch;
                        ShortRejectPunchSteelID.Text = toolProject.ShortRejectPunchSteelID;
                        ShortRejectAssembly.IsChecked = toolProject.ShortRejectAssembly;
                        ShortRejectCap.IsChecked = toolProject.ShortRejectCap;
                        ShortRejectCapSteelID.Text = toolProject.ShortRejectCapSteelID;
                        ShortRejectHolder.IsChecked = toolProject.ShortRejectHolder;
                        ShortRejectHolderSteelID.Text = toolProject.ShortRejectHolderSteelID;
                        ShortRejectHead.IsChecked = toolProject.ShortRejectHead;
                        ShortRejectHeadSteelID.Text = toolProject.ShortRejectHeadSteelID;
                        ShortRejectTip.IsChecked = toolProject.ShortRejectTip;
                        ShortRejectTipSteelID.Text = toolProject.ShortRejectTipSteelID;
                        LongRejectPunch.IsChecked = toolProject.LongRejectPunch;
                        LongRejectPunchSteelID.Text = toolProject.LongRejectPunchSteelID;
                        LongRejectAssembly.IsChecked = toolProject.LongRejectAssembly;
                        LongRejectCap.IsChecked = toolProject.LongRejectCap;
                        LongRejectCapSteelID.Text = toolProject.LongRejectCapSteelID;
                        LongRejectHolder.IsChecked = toolProject.LongRejectHolder;
                        LongRejectHolderSteelID.Text = toolProject.LongRejectHolderSteelID;
                        LongRejectHead.IsChecked = toolProject.LongRejectHead;
                        LongRejectHeadSteelID.Text = toolProject.LongRejectHeadSteelID;
                        LongRejectTip.IsChecked = toolProject.LongRejectTip;
                        LongRejectTipSteelID.Text = toolProject.LongRejectTipSteelID;
                        Alignment.IsChecked = toolProject.Alignment;
                        AlignmentSteelID.Text = toolProject.AlignmentSteelID;
                        Key.IsChecked = toolProject.Key;
                        KeySteelID.Text = toolProject.KeySteelID;
                        Misc.IsChecked = toolProject.Misc;
                        MiscSteelID.Text = toolProject.MiscSteelID;
                        NumberOfTips.Text = engineeringArchiveProjectCopied.NumberOfTips < 2 ? "" : engineeringArchiveProjectCopied.NumberOfTips.ToString();
                        Die.IsChecked = toolProject.Die;
                        DieSteelID.Text = toolProject.DieSteelID;
                        DieAssembly.IsChecked = toolProject.DieAssembly;
                        DieComponent.IsChecked = toolProject.DieComponent;
                        DieComponentSteelID.Text = toolProject.DieComponentSteelID;
                        DieHolder.IsChecked = toolProject.DieHolder;
                        DieHolderSteelID.Text = toolProject.DieHolderSteelID;
                        DieInsert.IsChecked = toolProject.DieInsert;
                        DieInsertSteelID.Text = toolProject.DieInsertSteelID;
                        DiePlate.IsChecked = toolProject.DiePlate;
                        DiePlateSteelID.Text = toolProject.DiePlateSteelID;
                        DieSegment.IsChecked = toolProject.DieSegment;
                        DieSegmentSteelID.Text = toolProject.DieSegmentSteelID;
                        KeyType.Text = toolProject.KeyType;
                        KeyAngle.Text = toolProject.KeyAngle.ToString();
                        KeyOrientation.Text = toolProject.KeyIsClockWise == true ? "CW" : toolProject.KeyIsClockWise == false ? "CCW" : "";
                        UpperKeyed.IsChecked = toolProject.UpperKeyed;
                        LowerKeyed.IsChecked = toolProject.LowerKeyed;
                        ShortRejectKeyed.IsChecked = toolProject.ShortRejectKeyed;
                        LongRejectKeyed.IsChecked = toolProject.LongRejectKeyed;
                        UpperGrooveType.Text = toolProject.UpperGrooveType;
                        LowerGrooveType.Text = toolProject.LowerGrooveType;
                        UpperGroove.IsChecked = toolProject.UpperGroove;
                        LowerGroove.IsChecked = toolProject.LowerGroove;
                        ShortRejectGroove.IsChecked = toolProject.ShortRejectGroove;
                        LongRejectGroove.IsChecked = toolProject.LongRejectGroove;
                        HeadType.Text = toolProject.HeadType;
                        CarbideTips.IsChecked = toolProject.CarbideTips;
                        MachineNotes.Text = toolProject.MachineNotes;
                        if (!_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) || _projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber && p.TabletChecked == true))
                        {
                            ToolsTabItem.IsSelected = true;
                            CurrentProjectType.Text = "TOOLS";
                            RefreshRoutingButtons();
                        }
                    }
                    else
                    {
                        ToolsRequired.IsChecked = false;
                    }
                }


                _projectsContext.Add(engineeringProject);
                _projectsContext.SaveChanges();
                _projectsContext.Dispose();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => PopulateBlankWindow", ex.Message, user);
            }
        }
        /// <summary>
        /// Fills in information from NAT01.(QuoteHeader, QuoteDetails, and QuoteDetailOptions) and NEC.Rm00101.
        /// </summary>
        /// <param name="quoteNumber"></param>
        /// <param name="quoteRevNumber"></param>
        /// <param name="isTabletProject"></param>
        /// <param name="isToolProject"></param>
        private void FillFromQuote(double quoteNumber, short quoteRevNumber, bool isTabletProject, bool isToolProject)
        {
            using var _nat01Context = new NAT01Context();
            using var _necContext = new NECContext();
            try
            {
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletsRequired.IsChecked = isTabletProject));
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ToolsRequired.IsChecked = isToolProject));
                QuoteHeader quoteHeader = _nat01Context.QuoteHeader.First(q => q.QuoteNo == quoteNumber && q.QuoteRevNo == quoteRevNumber);

                #region Customers, Product, Attention, Notes, RefOrderNumber
                if (!string.IsNullOrEmpty(quoteHeader.CustomerNo))
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerNumber.Text = string.IsNullOrEmpty(quoteHeader.CustomerNo) ? "" : quoteHeader.CustomerNo.Trim()));
                    if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == quoteHeader.CustomerNo.Trim()))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerName.Text = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == quoteHeader.CustomerNo.Trim()).Custname.Trim()));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerName.Text = ""));
                    }
                }
                else
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerNumber.Text = ""));
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerName.Text = ""));
                }
                if (!string.IsNullOrEmpty(quoteHeader.ShipToAccountNo))
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToNumber.Text = string.IsNullOrEmpty(quoteHeader.ShipToAccountNo) ? "" : quoteHeader.ShipToAccountNo.Trim()));
                    if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == quoteHeader.ShipToAccountNo.Trim()))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToName.Text = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == quoteHeader.ShipToAccountNo.Trim()).Custname.Trim()));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToName.Text = ""));
                    }
                }
                else
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToNumber.Text = ""));
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToName.Text = ""));
                }
                if (!string.IsNullOrEmpty(quoteHeader.UserAcctNo))
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserNumber.Text = string.IsNullOrEmpty(quoteHeader.UserAcctNo) ? "" : quoteHeader.UserAcctNo.Trim()));
                    if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == quoteHeader.UserAcctNo.Trim()))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserName.Text = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == quoteHeader.UserAcctNo.Trim()).Custname.Trim()));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserName.Text = ""));
                    }
                }
                else
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserName.Text = ""));
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserName.Text = ""));
                }
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Product.Text = string.IsNullOrEmpty(quoteHeader.ProductName) ? "" : quoteHeader.ProductName.Trim()));
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Attention.Text = string.IsNullOrEmpty(quoteHeader.ContactPerson) ? "" : quoteHeader.ContactPerson.Trim()));

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Notes.ReplaceText((quoteHeader.EngineeringNote1 ?? "").Trim() + "\n" + (quoteHeader.EngineeringNote2 ?? "").Trim() + "\n" + (quoteHeader.MiscNote ?? "").Trim(), Application.Current.Resources["Dark"] as SolidColorBrush)));
                if (quoteHeader.OrderNo != null && quoteHeader.OrderNo > 0)
                {
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => RefOrderNumber.Text = quoteHeader.OrderNo.ToString().Remove(6)));
                }

                #endregion

                Dictionary<QuoteDetails, List<QuoteDetailOptions>> quoteDetailOptionsDictionary = new Dictionary<QuoteDetails, List<QuoteDetailOptions>>();
                foreach (QuoteDetails _quoteDetail in _nat01Context.QuoteDetails.Where(q => q.QuoteNo == quoteNumber && q.Revision == quoteRevNumber && !string.IsNullOrEmpty(q.DetailTypeId)).OrderBy(q => q.LineNumber).ToList())
                {
                    List<QuoteDetailOptions> quoteDetailOptions = _nat01Context.QuoteDetailOptions.Where(qd => qd.QuoteNumber == quoteNumber && qd.RevisionNo == quoteRevNumber && qd.QuoteDetailLineNo == _quoteDetail.LineNumber).OrderBy(qd => qd.OptionLineNo).ToList();
                    quoteDetailOptionsDictionary.Add(_quoteDetail, quoteDetailOptions);
                }
                if (quoteDetailOptionsDictionary.Count > 0)
                {
                    #region Machine
                    if (quoteDetailOptionsDictionary.Any(q => q.Key.MachineNo != null && q.Key.MachineNo != 0))
                    {
                        short machineNumber = (short)quoteDetailOptionsDictionary.First(q => q.Key.MachineNo != null && q.Key.MachineNo != 0).Key.MachineNo;
                        string description = _nat01Context.MachineList.First(m => m.MachineNo == machineNumber).Description.Trim();
                        string od = _nat01Context.MachineList.First(m => m.MachineNo == machineNumber).Od.ToString();
                        string ol = _nat01Context.MachineList.First(m => m.MachineNo == machineNumber).Ol.ToString();
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MachineNumber.Text = machineNumber.ToString()));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MachineDescription.Text = description));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOD.Text = od));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieODPlaceholder.Visibility = Visibility.Collapsed));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOL.Text = ol));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOLPlaceholder.Visibility = Visibility.Collapsed));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MachineDescription.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOD.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieODPlaceholder.Visibility = Visibility.Visible));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOL.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOLPlaceholder.Visibility = Visibility.Visible));
                    }
                    #endregion

                    #region Die
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "D" || kvp.Key.DetailTypeId.Trim() == "DS")))
                    {
                        QuoteDetails quoteDetail = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "D" || kvp.Key.DetailTypeId.Trim() == "DS")).Key;
                        string dieNumber = quoteDetail.HobNoShapeId;
                        if(_nat01Context.DieList.Any(d => d.DieId.Trim() == dieNumber.Trim()))
                        { 
                            DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == dieNumber.Trim());
                            // Use Note2 from Die List if present
                            if (!string.IsNullOrWhiteSpace(die.Note2))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.DieList.First(d => d.DieId.Trim() == dieNumber.Trim()).Note2.Trim()));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                            }
                            // Use the shape ID description
                            else
                            {
                                short shapeID = (short)die.ShapeId;
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                            }
                            float width = (float)die.WidthMinorAxis;
                            float length = (float)die.LengthMajorAxis;
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));

                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieNumber.Text = dieNumber));
                        }
                    }
                    else if (quoteDetailOptionsDictionary.Any(kvp =>
                     !string.IsNullOrEmpty(kvp.Key.DetailTypeId) &&
                     (kvp.Key.DetailTypeId.Trim() == "U" ||
                     kvp.Key.DetailTypeId.Trim() == "UT" ||
                     kvp.Key.DetailTypeId.Trim() == "L" ||
                     kvp.Key.DetailTypeId.Trim() == "LT" ||
                     kvp.Key.DetailTypeId.Trim() == "R" ||
                     kvp.Key.DetailTypeId.Trim() == "RT")))
                    {
                        QuoteDetails quoteDetail = quoteDetailOptionsDictionary.First(kvp =>
                        !string.IsNullOrEmpty(kvp.Key.DetailTypeId) &&
                        (kvp.Key.DetailTypeId.Trim() == "U" ||
                        kvp.Key.DetailTypeId.Trim() == "UT" ||
                        kvp.Key.DetailTypeId.Trim() == "L" ||
                        kvp.Key.DetailTypeId.Trim() == "LT" ||
                        kvp.Key.DetailTypeId.Trim() == "R" ||
                        kvp.Key.DetailTypeId.Trim() == "RT")).Key;
                        string hobNumber = quoteDetail.HobNoShapeId;
                        if (_nat01Context.HobList.Any(h => h.HobNo == hobNumber && h.TipQty == (quoteDetail.TipQty ?? 1) && h.BoreCircle == (quoteDetail.BoreCircle ?? 0)))
                        {
                            string dieNumber = _nat01Context.HobList.First(h => h.HobNo == hobNumber && h.TipQty == (quoteDetail.TipQty ?? 1) && h.BoreCircle == (quoteDetail.BoreCircle ?? 0)).DieId;
                            DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == dieNumber.Trim());
                            // Use Note2 from Die List if present
                            if (!string.IsNullOrWhiteSpace(die.Note2))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.DieList.First(d => d.DieId.Trim() == dieNumber.Trim()).Note2.Trim()));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                            }
                            // Use the shape ID description
                            else
                            {
                                short shapeID = (short)die.ShapeId;
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                            }
                            float width = (float)die.WidthMinorAxis;
                            float length = (float)die.LengthMajorAxis;
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));

                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieNumber.Text = dieNumber));
                        }
                        else
                        {
                            DieNumber.Text = "";
                            DieShape.Text = "";
                            DieShapePlaceHolder.Visibility = Visibility.Collapsed;
                            TabletWidth.Text = "";
                            TabletLength.Text = "";
                        }

                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieNumber.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = ""));
                    }
                    #endregion

                    #region Upper Hob
                    // Line Item Exists
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT")))
                    {
                        QuoteDetails quoteDetails = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId)).Key;
                        // Hob is 'NEW'
                        if (quoteDetails.HobNoShapeId.Trim().ToUpper() == "NEW")
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobNumber.Text = "NEW"));
                            // Take the cup depth from other items
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW"))
                            {
                                var _key = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW").Key;
                                if (_nat01Context.HobList.Any(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0)))
                                {
                                    HobList hob = _nat01Context.HobList.First(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = ((float)hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = ((float)hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                                }
                                else
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = ""));
                                }
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = ""));
                            }
                        }
                        // Hob exists
                        if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0)))
                        {
                            HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0));
                            string hobNumber = hob.HobNo.Trim();
                            string note1 = hob.Note1.Trim();
                            string note2 = hob.Note2.Trim();
                            string note3 = hob.Note3.Trim();
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobNumber.Text = hobNumber));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = hob.CupDepth == null ? "0.0000" : Convert.ToSingle(hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = hob.Land == null ? "0.0000" : Convert.ToSingle(hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = note1 + " " + note2 + " " + note3));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = ""));
                        }
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescriptionPlaceHolder.Visibility = UpperHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobNumber.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescriptionPlaceHolder.Visibility = UpperHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                    }
                    #endregion

                    #region Lower Hob
                    // Line Item Exists
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT")))
                    {
                        QuoteDetails quoteDetails = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId)).Key;
                        // Hob is 'NEW'
                        if (quoteDetails.HobNoShapeId.Trim().ToUpper() == "NEW")
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobNumber.Text = "NEW"));
                            // Take the cup depth from other items
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW"))
                            {
                                var _key = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW").Key;
                                if (_nat01Context.HobList.Any(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0)))
                                {
                                    HobList hob = _nat01Context.HobList.First(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = ((float)hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = ((float)hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                                }
                                else
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = ""));
                                }
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = ""));
                            }
                        }
                        // Hob exists
                        if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0)))
                        {
                            HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0));
                            string hobNumber = hob.HobNo.Trim();
                            string note1 = hob.Note1.Trim();
                            string note2 = hob.Note2.Trim();
                            string note3 = hob.Note3.Trim();
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobNumber.Text = hobNumber));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = hob.CupDepth == null ? "0.0000" : Convert.ToSingle(hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = hob.Land == null ? "0.0000" : Convert.ToSingle(hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = note1 + " " + note2 + " " + note3));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobNumber.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = ""));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = ""));
                        }
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescriptionPlaceHolder.Visibility = LowerHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescriptionPlaceHolder.Visibility = LowerHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                    }
                    #endregion

                    #region Reject Hobs
                    // Line Items Exists
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RT")))
                    {
                        foreach (KeyValuePair<QuoteDetails, List<QuoteDetailOptions>> rejectDetailOptionsDictionary in quoteDetailOptionsDictionary.Where(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RT")))
                        {
                            QuoteDetails quoteDetails = rejectDetailOptionsDictionary.Key;
                            List<QuoteDetailOptions> quoteDetailOptions = rejectDetailOptionsDictionary.Value;
                            // Short Reject
                            if (quoteDetailOptions.Any(o => o.OptionCode == "350"))
                            {
                                // Hob is 'NEW'
                                if (quoteDetails.HobNoShapeId.Trim().ToUpper() == "NEW")
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobNumber.Text = "NEW"));
                                    // Take the cup depth from other items
                                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT" || kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW"))
                                    {
                                        var _key = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT" || kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW").Key;
                                        if (_nat01Context.HobList.Any(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0)))
                                        {
                                            HobList hob = _nat01Context.HobList.First(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = ((float)hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = ((float)hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                                        }
                                        else
                                        {
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = ""));
                                        }
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = ""));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = ""));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = ""));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = ""));
                                    }
                                }
                                // Hob exists
                                if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0)))
                                {
                                    HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0));
                                    string hobNumber = hob.HobNo.Trim();
                                    string note1 = hob.Note1.Trim();
                                    string note2 = hob.Note2.Trim();
                                    string note3 = hob.Note3.Trim();
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobNumber.Text = hobNumber));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = hob.CupDepth == null ? "0.0000" : Convert.ToSingle(hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = hob.Land == null ? "0.0000" : Convert.ToSingle(hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = note1 + " " + note2 + " " + note3));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                                }
                                else
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobNumber.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = ""));
                                }

                            }
                            // Long Reject
                            if (quoteDetailOptions.Any(o => o.OptionCode == "354"))
                            {
                                // Hob is 'NEW'
                                if (quoteDetails.HobNoShapeId.Trim().ToUpper() == "NEW")
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobNumber.Text = "NEW"));
                                    // Take the cup depth from other items
                                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT" || kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW"))
                                    {
                                        var _key = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UT" || kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LT") && !string.IsNullOrEmpty(kvp.Key.HobNoShapeId) && kvp.Key.HobNoShapeId.Trim().ToUpper() != "NEW").Key;
                                        if (_nat01Context.HobList.Any(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0)))
                                        {

                                            HobList hob = _nat01Context.HobList.First(h => h.HobNo == _key.HobNoShapeId && h.TipQty == (_key.TipQty ?? 1) && h.BoreCircle == (_key.BoreCircle ?? 0));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = ((float)hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = ((float)hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                                        }
                                        else
                                        {
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = ""));
                                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = ""));
                                        }
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = ""));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = ""));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = ""));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = ""));
                                    }
                                }
                                // Hob exists
                                if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0)))
                                {
                                    HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == quoteDetails.HobNoShapeId.Trim() && h.TipQty == (quoteDetails.TipQty ?? 1) && h.BoreCircle == (quoteDetails.BoreCircle ?? 0));
                                    string hobNumber = hob.HobNo.Trim();
                                    string note1 = hob.Note1.Trim();
                                    string note2 = hob.Note2.Trim();
                                    string note3 = hob.Note3.Trim();
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobNumber.Text = hobNumber));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = hob.CupDepth == null ? "0.0000" : Convert.ToSingle(hob.CupDepth).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = hob.Land == null ? "0.0000" : Convert.ToSingle(hob.Land).ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = note1 + " " + note2 + " " + note3));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = hob.CupCode == null ? "" : hob.CupCode + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim()));
                                }
                                else
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobNumber.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = ""));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = ""));
                                }
                            }
                        }
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescriptionPlaceHolder.Visibility = ShortRejectHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescriptionPlaceHolder.Visibility = LongRejectHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));

                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = ""));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescriptionPlaceHolder.Visibility = ShortRejectHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescriptionPlaceHolder.Visibility = LongRejectHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));

                    }

                    #endregion


                    #region Upper Punch
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperPunch.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperPunchSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Key.SteelId ?? ""));

                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperPunch.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperPunchSteelID.Text = ""));

                    }
                    #endregion
                    #region Upper Assembly
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UA"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperAssembly.IsChecked = true));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperAssembly.IsChecked = false));
                    }
                    #endregion
                    #region Upper Cap
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCap.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCapSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCap.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCapSteelID.Text = ""));
                    }
                    #endregion
                    #region Upper Holder
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHolder.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHolderSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHolder.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHolderSteelID.Text = ""));
                    }
                    #endregion
                    #region Upper Head
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHead.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHeadSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHead.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHeadSteelID.Text = ""));
                    }
                    #endregion
                    #region Upper Tip
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UT"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperTip.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperTipSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UT").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperTip.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperTipSteelID.Text = ""));
                    }
                    #endregion

                    #region Lower Punch
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerPunch.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerPunchSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerPunch.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerPunchSteelID.Text = ""));
                    }
                    #endregion
                    #region Lower Assembly
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LA"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerAssembly.IsChecked = true));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerAssembly.IsChecked = false));
                    }
                    #endregion
                    #region Lower Cap
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCap.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCapSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCap.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCapSteelID.Text = ""));
                    }
                    #endregion
                    #region Lower Holder
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHolder.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHolderSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHolder.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHolderSteelID.Text = ""));
                    }
                    #endregion
                    #region Lower Head
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHead.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHeadSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHead.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHeadSteelID.Text = ""));
                    }
                    #endregion
                    #region Lower Tip
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LT"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerTip.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerTipSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LT").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerTip.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerTipSteelID.Text = ""));
                    }
                    #endregion

                    #region Reject Tools
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RA")))
                    {
                        // Short Reject
                        if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RA")).Value.Any(o => o.OptionCode == "350"))
                        {
                            #region ShortReject Punch
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectPunch.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectPunchSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectPunch.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectPunchSteelID.Text = ""));
                            }
                            #endregion
                            #region ShortReject Assembly
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RA"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectAssembly.IsChecked = true));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectAssembly.IsChecked = false));
                            }
                            #endregion
                            #region ShortReject Cap
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RC"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCap.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCapSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RC").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCap.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCapSteelID.Text = ""));
                            }
                            #endregion
                            #region ShortReject Holder
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHolder.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHolderSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHolder.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHolderSteelID.Text = ""));
                            }
                            #endregion
                            #region ShortReject Head
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHead.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHeadSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHead.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHeadSteelID.Text = ""));
                            }
                            #endregion
                            #region ShortReject Tip
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RT"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectTip.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectTipSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RT").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectTip.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectTipSteelID.Text = ""));
                            }
                            #endregion
                        }
                        // Long Reject
                        else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RA")).Value.Any(o => o.OptionCode == "354"))
                        {
                            #region LongReject Punch
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectPunch.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectPunchSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectPunch.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectPunchSteelID.Text = ""));
                            }
                            #endregion
                            #region LongReject Assembly
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RA"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectAssembly.IsChecked = true));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectAssembly.IsChecked = false));
                            }
                            #endregion
                            #region LongReject Cap
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RC"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCap.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCapSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RC").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCap.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCapSteelID.Text = ""));
                            }
                            #endregion
                            #region LongReject Holder
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHolder.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHolderSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHolder.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHolderSteelID.Text = ""));
                            }
                            #endregion
                            #region LongReject Head
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHead.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHeadSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHead.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHeadSteelID.Text = ""));
                            }
                            #endregion
                            #region LongReject Tip
                            if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RT"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectTip.IsChecked = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectTipSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RT").Key.SteelId ?? ""));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectTip.IsChecked = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectTipSteelID.Text = ""));
                            }
                            #endregion
                        }
                    }
                    #endregion

                    #region Die
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "D"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Die.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "D").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Die.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieSteelID.Text = ""));
                    }
                    #endregion
                    #region Die Assembly
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DA"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieAssembly.IsChecked = true));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieAssembly.IsChecked = false));
                    }
                    #endregion
                    #region Die Component
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DC"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieComponent.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieComponentSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DC").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieComponent.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieComponentSteelID.Text = ""));
                    }
                    #endregion
                    #region Die Holder
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DH"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieHolder.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieHolderSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DH").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieHolder.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieHolderSteelID.Text = ""));
                    }
                    #endregion
                    #region Die Insert
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DI"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieInsert.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieInsertSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DI").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieInsert.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieInsertSteelID.Text = ""));
                    }
                    #endregion
                    #region Die Plate
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DP"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DiePlate.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DiePlateSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DP").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DiePlate.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DiePlateSteelID.Text = ""));
                    }
                    #endregion
                    #region Die Segment
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DS"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieSegment.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieSegmentSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "DS").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieSegment.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieSegmentSteelID.Text = ""));
                    }
                    #endregion

                    #region Alignment Tool
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "A"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Alignment.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => AlignmentSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "A").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Alignment.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => AlignmentSteelID.Text = ""));
                    }
                    #endregion

                    #region Key
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "K"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Key.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeySteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "K").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Key.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeySteelID.Text = ""));
                    }
                    #endregion

                    #region Miscellaneous
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "M"))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Misc.IsChecked = true));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MiscSteelID.Text = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "M").Key.SteelId ?? ""));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => Misc.IsChecked = false));
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MiscSteelID.Text = ""));
                    }
                    #endregion

                    #region Core Rod
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCR"))
                    {
                        CoreRod = true;
                        CoreRodSteelID = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCR").Key.SteelId ?? "";
                    }
                    else
                    {
                        CoreRod = false;
                        CoreRodSteelID = "";
                    }
                    #endregion
                    #region Core Rod Key
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRK"))
                    {
                        CoreRodKey = true;
                        CoreRodKeySteelID = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRK").Key.SteelId ?? "";
                    }
                    else
                    {
                        CoreRodKey = false;
                        CoreRodKeySteelID = "";
                    }
                    #endregion
                    #region Core Rod Key Collar
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRKC"))
                    {
                        CoreRodKeyCollar = true;
                        CoreRodKeyCollarSteelID = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRKC").Key.SteelId ?? "";
                    }
                    else
                    {
                        CoreRodKeyCollar = false;
                        CoreRodKeyCollarSteelID = "";
                    }
                    #endregion
                    #region Core Rod Punch
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP"))
                    {
                        CoreRodPunch = true;
                        CoreRodPunchSteelID = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Key.SteelId ?? "";
                    }
                    else
                    {
                        CoreRodPunch = false;
                        CoreRodPunchSteelID = "";
                    }
                    #endregion

                    #region Key Angle
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UH" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RH") && (kvp.Value.Any(o => o.OptionCode == "155") || kvp.Value.Any(o => o.OptionCode == "156"))))
                    {
                        QuoteDetails quoteDetails = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UH" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RH") && (kvp.Value.Any(o => o.OptionCode == "155") || kvp.Value.Any(o => o.OptionCode == "156"))).Key;
                        List<QuoteDetailOptions> quoteDetailOptions = quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UH" || kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RH") && (kvp.Value.Any(o => o.OptionCode == "155") || kvp.Value.Any(o => o.OptionCode == "156"))).Value;
                        if (quoteDetailOptions.Any(o => o.OptionCode == "156"))
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyAngle.Text = "0"));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyOrientation.Text = ""));
                        }
                        else
                        {
                            QuoteOptionValueGDegrees specialKeyAngleOrientToTipShape = _nat01Context.QuoteOptionValueGDegrees.First(qov => qov.QuoteNo == Convert.ToInt32(quoteDetails.QuoteNo) && qov.RevNo == quoteDetails.Revision && qov.OptionCode == "155");
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyAngle.Text = specialKeyAngleOrientToTipShape.Degrees == null ? "" : specialKeyAngleOrientToTipShape.Degrees.ToString()));
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyOrientation.Text = string.IsNullOrEmpty(specialKeyAngleOrientToTipShape.Text) == true ? "" : specialKeyAngleOrientToTipShape.Text.ToString().Trim()));
                        }
                    }
                    #endregion

                    #region Upper Keyed
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UH" || kvp.Key.DetailTypeId.Trim() == "UHD")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Holder
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Head
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UHD").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperKeyed.IsChecked = false));
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperKeyed.IsChecked = false));
                    }
                    #endregion

                    #region Lower Keyed
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LH" || kvp.Key.DetailTypeId.Trim() == "LHD" || kvp.Key.DetailTypeId.Trim() == "LCRP")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Holder
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Head
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LHD").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Core Rod Punch
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerKeyed.IsChecked = false));
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerKeyed.IsChecked = false));
                    }
                    #endregion

                    #region Short Reject Keyed
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RH" || kvp.Key.DetailTypeId.Trim() == "RHD") && kvp.Value.Any(o => o.OptionCode == "350")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Holder
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Head
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectKeyed.IsChecked = false));
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectKeyed.IsChecked = false));
                    }
                    #endregion

                    #region Long Reject Keyed
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "R" || kvp.Key.DetailTypeId.Trim() == "RH" || kvp.Key.DetailTypeId.Trim() == "RHD") && kvp.Value.Any(o => o.OptionCode == "350")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Holder
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RH").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        // Head
                        else if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "130" || o.OptionCode == "131" || o.OptionCode == "132" || o.OptionCode == "133" || o.OptionCode == "139" || o.OptionCode == "140" || o.OptionCode == "141" || o.OptionCode == "143" || o.OptionCode == "144"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectKeyed.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "130"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "131"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "132"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "133"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Woodruff Key W/ Screw 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "139"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Extra Long FPK"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "140"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 1 Slot"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "141"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Fixed Parallel Key 2 Slots"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "143"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key In Head"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "RHD").Value.Any(o => o.OptionCode == "144"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => KeyType.Text = "Key Fixed W/ 2 screws"));
                                }
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectKeyed.IsChecked = false));
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectKeyed.IsChecked = false));
                    }
                    #endregion

                    #region Upper Groove
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "U" || kvp.Key.DetailTypeId.Trim() == "UH" || kvp.Key.DetailTypeId.Trim() == "UC")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "U").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                        // Holder
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UH").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                        // Cap
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "UC").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGroove.IsChecked = false));
                    }
                    #endregion

                    #region Lower Groove
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && (kvp.Key.DetailTypeId.Trim() == "L" || kvp.Key.DetailTypeId.Trim() == "LH" || kvp.Key.DetailTypeId.Trim() == "LC" || kvp.Key.DetailTypeId.Trim() == "LCRP")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "L").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                        // Holder
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LH").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                        // Cap
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LC").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                        // Core Rod Punch
                        if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP"))
                        {
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGroove.IsChecked = true));
                                if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "110"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "111"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Dust Cup Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "113"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "\"O\" Ring Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "115"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Combo Groove"));
                                }
                                else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "LCRP").Value.Any(o => o.OptionCode == "116"))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGrooveType.Text = "Special Bellows Groove"));
                                }
                            }
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerGroove.IsChecked = false));
                    }
                    #endregion

                    #region Short Reject Groove
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R" && kvp.Value.Any(o => o.OptionCode == "350")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectGroove.IsChecked = true));
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "110"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Dust Cup Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "111"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Dust Cup Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "113"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "\"O\" Ring Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "115"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Combo Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Bellows Groove"));
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGroove.IsChecked = false));
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectGroove.IsChecked = false));
                    }
                    #endregion

                    #region Long Reject Groove
                    if (quoteDetailOptionsDictionary.Any(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R" && kvp.Value.Any(o => o.OptionCode == "354")))
                    {
                        // Punch
                        if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "110" || o.OptionCode == "111" || o.OptionCode == "113" || o.OptionCode == "115" || o.OptionCode == "116"))
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectGroove.IsChecked = true));
                            if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "110"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Dust Cup Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "111"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Dust Cup Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "113"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "\"O\" Ring Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "115"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Combo Groove"));
                            }
                            else if (quoteDetailOptionsDictionary.First(kvp => !string.IsNullOrEmpty(kvp.Key.DetailTypeId) && kvp.Key.DetailTypeId.Trim() == "R").Value.Any(o => o.OptionCode == "116"))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGrooveType.Text = "Special Bellows Groove"));
                            }
                        }
                        else
                        {
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperGroove.IsChecked = false));
                        }
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectGroove.IsChecked = false));
                    }
                    #endregion

                    #region Head Type
                    if (quoteDetailOptionsDictionary.Any(kvp => kvp.Value.Any(o =>
                    o.OptionCode == "001" ||
                    o.OptionCode == "002" ||
                    o.OptionCode == "003" ||
                    o.OptionCode == "004" ||
                    o.OptionCode == "005" ||
                    o.OptionCode == "006" ||
                    o.OptionCode == "007" ||
                    o.OptionCode == "008" ||
                    o.OptionCode == "009" ||
                    o.OptionCode == "010" ||
                    o.OptionCode == "011" ||
                    o.OptionCode == "012" ||
                    o.OptionCode == "013" ||
                    o.OptionCode == "014" ||
                    o.OptionCode == "015" ||
                    o.OptionCode == "016" ||
                    o.OptionCode == "017" ||
                    o.OptionCode == "018" ||
                    o.OptionCode == "019" ||
                    o.OptionCode == "022")))
                    {
                        Dictionary<string, string> headTypes = new Dictionary<string, string> {
                            {"001","Standard Head" },
                            {"002","TSM Domed Head" },
                            {"003","Manesty Head" },
                            {"004","European Head" },
                            {"005","Fette European Head" },
                            {"006","Korsch European Head" },
                            {"007","Fette EU1-441 Head" },
                            {"008","Speacial Head Configuration" },
                            {"009","Kilian European Head" },
                            {"010","Cam-Slot Head" },
                            {"011","Fette EU28-441 Head" },
                            {"012","Domed Head-Old Style" },
                            {"013","Fette EU1-441/28 Head" },
                            {"014","Fette EU1-441/28N Head" },
                            {"015","Domed Head-Special" },
                            {"016","Fette FS19 Head" },
                            {"017","Fette Fs12 Head" },
                            {"018","Natoli MDT-25 Head" },
                            {"019","Natoli MDT-19 Head" },
                            {"022","ISO European Head" },
                        };
                        string headType = headTypes[
                        quoteDetailOptionsDictionary.First(kvp => kvp.Value.Any(o =>
                    o.OptionCode == "001" ||
                    o.OptionCode == "002" ||
                    o.OptionCode == "003" ||
                    o.OptionCode == "004" ||
                    o.OptionCode == "005" ||
                    o.OptionCode == "006" ||
                    o.OptionCode == "007" ||
                    o.OptionCode == "008" ||
                    o.OptionCode == "009" ||
                    o.OptionCode == "010" ||
                    o.OptionCode == "011" ||
                    o.OptionCode == "012" ||
                    o.OptionCode == "013" ||
                    o.OptionCode == "014" ||
                    o.OptionCode == "015" ||
                    o.OptionCode == "016" ||
                    o.OptionCode == "017" ||
                    o.OptionCode == "018" ||
                    o.OptionCode == "019" ||
                    o.OptionCode == "022"))
                        .Value.First(o =>
                    o.OptionCode == "001" ||
                    o.OptionCode == "002" ||
                    o.OptionCode == "003" ||
                    o.OptionCode == "004" ||
                    o.OptionCode == "005" ||
                    o.OptionCode == "006" ||
                    o.OptionCode == "007" ||
                    o.OptionCode == "008" ||
                    o.OptionCode == "009" ||
                    o.OptionCode == "010" ||
                    o.OptionCode == "011" ||
                    o.OptionCode == "012" ||
                    o.OptionCode == "013" ||
                    o.OptionCode == "014" ||
                    o.OptionCode == "015" ||
                    o.OptionCode == "016" ||
                    o.OptionCode == "017" ||
                    o.OptionCode == "018" ||
                    o.OptionCode == "019" ||
                    o.OptionCode == "022")
                        .OptionCode];

                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => HeadType.Text = headType));
                    }
                    #endregion

                    #region Carbide Tipped
                    if (quoteDetailOptionsDictionary.Any(kvp => kvp.Value.Any(o => o.OptionCode == "240")))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CarbideTips.IsChecked = true));
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CarbideTips.IsChecked = false));
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _nat01Context.Dispose();
            _necContext.Dispose();
        }
        /// <summary>
        /// Creates a new EngineeringProjects from the information in the form.
        /// </summary>
        /// <param name="projectWillBeActive"></param>
        /// <returns></returns>
        private EngineeringProjects GetEngineeringProjectFromCurrentForm(bool projectWillBeActive)
        {
            if (!projectWillBeActive)
            {
                if (TabletsRequired.IsChecked != true && ToolsRequired.IsChecked != true)
                {
                    MessageBox.Show("Please mark for tablet or tools.");
                    return null;
                }
                else
                {
                    goto SaveProject;
                }
            }
            else if (FormCheck())
            {
                goto SaveProject;
            }
            else
            {
                return null;
            }
        SaveProject:
            try
            {

                using var _projectsContext = new ProjectsContext();

                EngineeringProjects oldEngineeringProject = projectWillBeActive && _projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) ? _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) : null;
                //EngineeringArchivedProjects archivedProject = projectWillBeActive && _projectsContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) ? _projectsContext.EngineeringArchivedProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber) : null;
                decimal conversion = (string)Resources["UnitsText"] == "in" ? 1 : 1 / (decimal)25.4;
                EngineeringProjects engineeringProject = new EngineeringProjects
                {
                    ProjectNumber = projectNumber,
                    RevNumber = projectRevNumber,
                    ActiveProject = projectWillBeActive,
                    QuoteNumber = !string.IsNullOrEmpty(QuoteNumber.Text) ? QuoteNumber.Text.Trim() : "",
                    QuoteRevNumber = !string.IsNullOrEmpty(QuoteRevNumber.Text) ? QuoteRevNumber.Text.Trim() : "",
                    RefProjectNumber = !string.IsNullOrEmpty(ReferenceProjectNumber.Text) ? ReferenceProjectNumber.Text.Trim() : "",
                    RefProjectRevNumber = !string.IsNullOrEmpty(ReferenceProjectRevNumber.Text) ? ReferenceProjectRevNumber.Text.Trim() : "",
                    RefQuoteNumber = !string.IsNullOrEmpty(ReferenceQuoteNumber.Text) ? ReferenceQuoteNumber.Text.Trim() : "",
                    RefQuoteRevNumber = !string.IsNullOrEmpty(ReferenceQuoteRevNumber.Text) ? ReferenceQuoteRevNumber.Text.Trim() : "",
                    RefOrderNumber = !string.IsNullOrEmpty(RefOrderNumber.Text) ? RefOrderNumber.Text.Trim() : "",
                    CSR = CSR.Text,
                    ReturnToCSR = !string.IsNullOrEmpty(ReturnToCSR.Text) ? ReturnToCSR.Text.Trim() : "",
                    CustomerNumber = !string.IsNullOrEmpty(CustomerNumber.Text) ? CustomerNumber.Text.Trim() : "",
                    CustomerName = !string.IsNullOrEmpty(CustomerName.Text) ? CustomerName.Text.Trim() : "",
                    ShipToNumber = !string.IsNullOrEmpty(ShipToNumber.Text) ? ShipToNumber.Text.Trim() : "",
                    ShipToLocNumber = !string.IsNullOrEmpty(ShipToLocNumber.Text) ? ShipToLocNumber.Text.Trim() : "",
                    ShipToName = !string.IsNullOrEmpty(ShipToName.Text) ? ShipToName.Text.Trim() : "",
                    EndUserNumber = !string.IsNullOrEmpty(EndUserNumber.Text) ? EndUserNumber.Text.Trim() : "",
                    EndUserLocNumber = !string.IsNullOrEmpty(EndUserLocNumber.Text) ? EndUserLocNumber.Text.Trim() : "",
                    EndUserName = !string.IsNullOrEmpty(EndUserName.Text) ? EndUserName.Text.Trim() : "",
                    UnitOfMeasure = !string.IsNullOrEmpty(UnitOfMeasure.Text) ? UnitOfMeasure.Text.Trim() : "",
                    Product = !string.IsNullOrEmpty(Product.Text) ? Product.Text.Trim() : "",
                    Attention = !string.IsNullOrEmpty(Attention.Text) ? Attention.Text.Trim() : "",
                    MachineNumber = !string.IsNullOrEmpty(MachineNumber.Text) ? MachineNumber.Text.Trim() : "",
                    DieNumber = string.IsNullOrEmpty(DieNumber.Text) ? "      " : DieNumber.Text.Trim().Length < 6 ? new string(' ', 6 - DieNumber.Text.Trim().Length) + DieNumber.Text.Trim() : DieNumber.Text.Trim(),
                    DieShape = !string.IsNullOrEmpty(DieShape.Text) ? DieShape.Text.Trim() : "",
                    Width = decimal.TryParse(TabletWidth.Text, out decimal width) ? (decimal?)width * conversion : null,
                    Length = decimal.TryParse(TabletLength.Text, out decimal length) ? (decimal?)length * conversion : null,
                    UpperCupType = string.IsNullOrEmpty(UpperCupType.Text) ? null : short.TryParse(UpperCupType.Text.Split('-')[0].Trim(), out short upperCupType) ? (short?)upperCupType : null,
                    UpperHobNumber = string.IsNullOrEmpty(UpperHobNumber.Text) ? "      " : UpperHobNumber.Text.Trim().ToUpper() == "NEW" ? "   NEW" : UpperHobNumber.Text.Trim().Length < 6 ? new string('0', 6 - UpperHobNumber.Text.Trim().Length) + UpperHobNumber.Text.Trim() : UpperHobNumber.Text.Trim(),
                    UpperHobDescription = !string.IsNullOrEmpty(UpperHobDescription.Text) ? UpperHobDescription.Text.Trim() : "",
                    UpperCupDepth = decimal.TryParse(UpperCupDepth.Text, out decimal upperCupDepth) ? (decimal?)upperCupDepth * conversion : null,
                    UpperLand = decimal.TryParse(UpperLand.Text, out decimal upperLand) ? (decimal?)upperLand * conversion : null,
                    LowerCupType = string.IsNullOrEmpty(LowerCupType.Text) ? null : short.TryParse(LowerCupType.Text.Split('-')[0].Trim(), out short lowerCupType) ? (short?)lowerCupType : null,
                    LowerHobNumber = string.IsNullOrEmpty(LowerHobNumber.Text) ? "      " : LowerHobNumber.Text.Trim().ToUpper() == "NEW" ? "   NEW" : LowerHobNumber.Text.Trim().Length < 6 ? new string('0', 6 - LowerHobNumber.Text.Trim().Length) + LowerHobNumber.Text.Trim() : LowerHobNumber.Text.Trim(),
                    LowerHobDescription = !string.IsNullOrEmpty(LowerHobDescription.Text) ? LowerHobDescription.Text.Trim() : "",
                    LowerCupDepth = decimal.TryParse(LowerCupDepth.Text, out decimal lowerCupDepth) ? (decimal?)lowerCupDepth * conversion : null,
                    LowerLand = decimal.TryParse(LowerLand.Text, out decimal lowerLand) ? (decimal?)lowerLand * conversion : null,
                    ShortRejectCupType = string.IsNullOrEmpty(ShortRejectCupType.Text) ? null : short.TryParse(ShortRejectCupType.Text.Split('-')[0].Trim(), out short shortRejectCupType) ? (short?)shortRejectCupType : null,
                    ShortRejectHobNumber = string.IsNullOrEmpty(ShortRejectHobNumber.Text) ? "      " : ShortRejectHobNumber.Text.Trim().ToUpper() == "NEW" ? "   NEW" : ShortRejectHobNumber.Text.Trim().Length < 6 ? new string('0', 6 - ShortRejectHobNumber.Text.Trim().Length) + ShortRejectHobNumber.Text.Trim() : ShortRejectHobNumber.Text.Trim(),
                    ShortRejectHobDescription = !string.IsNullOrEmpty(ShortRejectHobDescription.Text) ? ShortRejectHobDescription.Text.Trim() : "",
                    ShortRejectCupDepth = decimal.TryParse(ShortRejectCupDepth.Text, out decimal shortRejectCupDepth) ? (decimal?)shortRejectCupDepth * conversion : null,
                    ShortRejectLand = decimal.TryParse(ShortRejectLand.Text, out decimal shortRejectLand) ? (decimal?)shortRejectLand * conversion : null,
                    LongRejectCupType = string.IsNullOrEmpty(LongRejectCupType.Text) ? null : short.TryParse(LongRejectCupType.Text.Split('-')[0].Trim(), out short longRejectCupType) ? (short?)longRejectCupType : null,
                    LongRejectHobNumber = string.IsNullOrEmpty(LongRejectHobNumber.Text) ? "      " : LongRejectHobNumber.Text.Trim().ToUpper() == "NEW" ? "   NEW" : LongRejectHobNumber.Text.Trim().Length < 6 ? new string('0', 6 - LongRejectHobNumber.Text.Trim().Length) + LongRejectHobNumber.Text.Trim() : LongRejectHobNumber.Text.Trim(),
                    LongRejectHobDescription = !string.IsNullOrEmpty(LongRejectHobDescription.Text) ? LongRejectHobDescription.Text.Trim() : "",
                    LongRejectCupDepth = decimal.TryParse(LongRejectCupDepth.Text, out decimal longRejectCupDepth) ? (decimal?)longRejectCupDepth * conversion : null,
                    LongRejectLand = decimal.TryParse(LongRejectLand.Text, out decimal longRejectLand) ? (decimal?)longRejectLand * conversion : null,
                    UpperTolerances = !string.IsNullOrEmpty(UpperTolerances.Text) ? UpperTolerances.Text.Trim() : "",
                    LowerTolerances = !string.IsNullOrEmpty(LowerTolerances.Text) ? LowerTolerances.Text.Trim() : "",
                    ShortRejectTolerances = !string.IsNullOrEmpty(ShortRejectTolerances.Text) ? ShortRejectTolerances.Text.Trim() : "",
                    LongRejectTolerances = !string.IsNullOrEmpty(LongRejectTolerances.Text) ? LongRejectTolerances.Text.Trim() : "",
                    DieTolerances = !string.IsNullOrEmpty(DieTolerances.Text) ? DieTolerances.Text.Trim() : "",
                    Notes = Notes.GetText().TrimEnd('\r','\n'),
                    //Notes = !string.IsNullOrEmpty(Notes.Text) ? Notes.Text.Trim() : "",
                    TimeSubmitted = DateTime.UtcNow,
                    DueDate = DueDate.Text.Length > 0 ? (DateTime.TryParse(DueDate.Text.Remove(0, DueDate.Text.IndexOf('|') + 2), out DateTime dateTime) ? dateTime : DateTime.MaxValue) : DateTime.MaxValue,
                    Priority = Priority.IsChecked ?? false,
                    TabletStarted = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.TabletStarted) : false,
                    TabletStartedDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.TabletStartedDateTime) : null,
                    TabletStartedBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.TabletStartedBy) : "",
                    TabletDrawn = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.TabletDrawn) : false,
                    TabletDrawnDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.TabletDrawnDateTime) : null,
                    TabletDrawnBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.TabletDrawnBy) : "",
                    TabletSubmitted = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.TabletSubmitted) : false,
                    TabletSubmittedDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.TabletSubmittedDateTime) : null,
                    TabletSubmittedBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.TabletSubmittedBy) : "",
                    TabletChecked = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.TabletChecked) : false,
                    TabletCheckedDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.TabletCheckedDateTime) : null,
                    TabletCheckedBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.TabletCheckedBy) : "",
                    ToolStarted = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.ToolStarted) : false,
                    ToolStartedDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.ToolStartedDateTime) : null,
                    ToolStartedBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.ToolStartedBy) : "",
                    ToolDrawn = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.ToolDrawn) : false,
                    ToolDrawnDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.ToolDrawnDateTime) : null,
                    ToolDrawnBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.ToolDrawnBy) : "",
                    ToolSubmitted = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.ToolSubmitted) : false,
                    ToolSubmittedDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.ToolSubmittedDateTime) : null,
                    ToolSubmittedBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.ToolSubmittedBy) : "",
                    ToolChecked = projectWillBeActive ? (oldEngineeringProject == null ? false : oldEngineeringProject.ToolChecked) : false,
                    ToolCheckedDateTime = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.ToolCheckedDateTime) : null,
                    ToolCheckedBy = projectWillBeActive ? (oldEngineeringProject == null ? "" : oldEngineeringProject.ToolCheckedBy) : "",
                    NewDrawing = NewDrawing,
                    UpdateExistingDrawing = UpdateExistingDrawing,
                    UpdateTextOnDrawing = UpdateTextOnDrawing,
                    PerSampleTablet = PerSampleTablet,
                    RefTabletDrawing = RefTabletDrawing,
                    PerSampleTool = PerSampleTool,
                    RefToolDrawing = RefToolDrawing,
                    PerSuppliedPicture = PerSuppliedPicture,
                    RefNatoliDrawing = RefNatoliDrawing,
                    RefNonNatoliDrawing = RefNonNatoliDrawing,
                    MultiTipSketch = MultiTipSketch.IsChecked ?? false,
                    MultiTipSketchID = !string.IsNullOrEmpty(SketchID.Text) ? SketchID.Text.Trim() : "",
                    NumberOfTips = byte.TryParse(NumberOfTips.Text, out byte numberOfTips) ? numberOfTips : (byte)1,
                    BinLocation = BinLocation,
                    MultiTipSolid = MultiTipStyle.Text == "SOLID",
                    MultiTipAssembled = MultiTipStyle.Text == "ASSEMBLED",
                    OnHold = false,
                    OnHoldComment = "",
                    OnHoldDateTime = null,
                    RevisedBy = projectWillBeActive ? user.GetDWDisplayName() : null,
                    Changes = projectWillBeActive ? (oldEngineeringProject == null ? null : oldEngineeringProject.Changes) : null
                };
                _projectsContext.Dispose();
                return engineeringProject;
            }
            catch (Exception ex)
            {
                
                IMethods.WriteToErrorLog("ProjectWindow => GetEngineeringProjectFromCurrentForm", ex.Message, user);
                MessageBox.Show("There was an error processing this form.\r\n" + ex.Message);
                return null;
            }


        }
        /// <summary>
        /// Creates a new EngineeringTabletProjects from the information in the form.
        /// </summary>
        /// <returns></returns>
        private EngineeringTabletProjects GetTabletProjectFromCurrentForm()
        {
            try
            {
                using var _projectsContext = new ProjectsContext();
                EngineeringTabletProjects tabletProject = new EngineeringTabletProjects()
                {
                    ProjectNumber = projectNumber,
                    RevNumber = projectRevNumber,
                    Density = decimal.TryParse(Density.Text, out decimal density) ? (decimal?)density : null,
                    DensityUnits = !string.IsNullOrEmpty(DensityUnits.Text) ? DensityUnits.Text.Trim() : "",
                    FilmCoated = FilmCoat.IsChecked ?? false,
                    FilmCoatType = !string.IsNullOrEmpty(FilmCoatStyle.Text) ? FilmCoatStyle.Text.Trim() : "",
                    LongRejectRequired = LongRejectTabletDrawing.IsChecked ?? false,
                    LowerRequired = LowerTabletDrawing.IsChecked ?? false,
                    Mass = decimal.TryParse(Mass.Text, out decimal mass) ? (decimal?)mass : null,
                    MassUnits = !string.IsNullOrEmpty(MassUnits.Text) ? MassUnits.Text.Trim() : "",
                    PrePick = PrePick.IsChecked ?? false,
                    PrePickAmount = decimal.TryParse(PrePickAmount.Text, out decimal prePickAmount) ? (decimal?)prePickAmount : null,
                    PrePickUnits = !string.IsNullOrEmpty(PrePickUnits.Text) ? PrePickUnits.Text.Trim() : "",
                    ShortRejectRequired = ShortRejectTabletDrawing.IsChecked ?? false,
                    Taper = Taper.IsChecked ?? false,
                    TaperAmount = decimal.TryParse(TaperAmount.Text, out decimal taperAmount) ? (decimal?)taperAmount : null,
                    TaperUnits = !string.IsNullOrEmpty(TaperUnits.Text) ? TaperUnits.Text.Trim() : "",
                    TargetThickness = decimal.TryParse(TargetThickness.Text, out decimal targetThickness) ? (decimal?)targetThickness : null,
                    TargetThicknessUnits = !string.IsNullOrEmpty(TargetThicknessUnits.Text) ? TargetThicknessUnits.Text.Trim() : "",
                    UpperRequired = UpperTabletDrawing.IsChecked ?? false,
                    Volume = decimal.TryParse(Volume.Text, out decimal volume) ? (decimal?)volume : null,
                    VolumeUnits = !string.IsNullOrEmpty(VolumeUnits.Text) ? VolumeUnits.Text.Trim() : ""
                };
                _projectsContext.Dispose();
                return tabletProject;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => GetTabletProjectFromCurrentForm", ex.Message, user);
                return null;
            }
        }
        /// <summary>
        /// Creates a new EngineeringToolProjects from the information in the form.
        /// </summary>
        /// <returns></returns>
        private EngineeringToolProjects GetToolProjectFromCurrentForm()
        {
            try
            {
                using var _projectsContext = new ProjectsContext();
                EngineeringToolProjects engineeringToolProject = new EngineeringToolProjects()
                {
                    ProjectNumber = projectNumber,
                    RevNumber = projectRevNumber,
                    Alignment = Alignment.IsChecked ?? false,
                    AlignmentSteelID = !string.IsNullOrEmpty(AlignmentSteelID.Text) ? AlignmentSteelID.Text.Trim() : "",
                    CarbideTips = CarbideTips.IsChecked ?? false,
                    Die = Die.IsChecked ?? false,
                    DieSteelID = !string.IsNullOrEmpty(DieSteelID.Text) ? DieSteelID.Text.Trim() : "",
                    DieAssembly = DieAssembly.IsChecked ?? false,
                    DieComponent = DieComponent.IsChecked ?? false,
                    DieComponentSteelID = !string.IsNullOrEmpty(DieComponentSteelID.Text) ? DieComponentSteelID.Text.Trim() : "",
                    DieHolder = DieHolder.IsChecked ?? false,
                    DieHolderSteelID = !string.IsNullOrEmpty(DieHolderSteelID.Text) ? DieHolderSteelID.Text.Trim() : "",
                    DieSegment = DieSegment.IsChecked ?? false,
                    DieSegmentSteelID = !string.IsNullOrEmpty(DieSegmentSteelID.Text) ? DieSegmentSteelID.Text.Trim() : "",
                    DieInsert = DieInsert.IsChecked ?? false,
                    DieInsertSteelID = !string.IsNullOrEmpty(DieInsertSteelID.Text) ? DieInsertSteelID.Text.Trim() : "",
                    DiePlate = DiePlate.IsChecked ?? false,
                    DiePlateSteelID = !string.IsNullOrEmpty(DiePlateSteelID.Text) ? DiePlateSteelID.Text.Trim() : "",
                    HeadType = !string.IsNullOrEmpty(HeadType.Text) ? HeadType.Text.Trim() : "",
                    Key = Key.IsChecked ?? false,
                    KeySteelID = !string.IsNullOrEmpty(KeySteelID.Text) ? (KeySteelID.Text.Trim().Length > 3 ? "NA" : KeySteelID.Text.Trim()) : "",
                    KeyAngle = decimal.TryParse(KeyAngle.Text, out decimal keyAngle) ? (decimal?)keyAngle : null,
                    KeyType = !string.IsNullOrEmpty(KeyType.Text) ? KeyType.Text.Trim() : "",
                    KeyIsClockWise = string.IsNullOrEmpty(KeyOrientation.Text) ? (bool?)null : KeyOrientation.Text == "CW",
                    LongRejectHead = LongRejectHead.IsChecked ?? false,
                    LongRejectHeadSteelID = !string.IsNullOrEmpty(LongRejectHeadSteelID.Text) ? LongRejectHeadSteelID.Text.Trim() : "",
                    LongRejectAssembly = LongRejectAssembly.IsChecked ?? false,
                    LongRejectCap = LongRejectCap.IsChecked ?? false,
                    LongRejectCapSteelID = !string.IsNullOrEmpty(LongRejectCapSteelID.Text) ? LongRejectCapSteelID.Text.Trim() : "",
                    LongRejectHolder = LongRejectHolder.IsChecked ?? false,
                    LongRejectHolderSteelID = !string.IsNullOrEmpty(LongRejectHolderSteelID.Text) ? LongRejectHolderSteelID.Text.Trim() : "",
                    LongRejectPunch = LongRejectPunch.IsChecked ?? false,
                    LongRejectPunchSteelID = !string.IsNullOrEmpty(LongRejectPunchSteelID.Text) ? LongRejectPunchSteelID.Text.Trim() : "",
                    LongRejectTip = LongRejectTip.IsChecked ?? false,
                    LongRejectTipSteelID = !string.IsNullOrEmpty(LongRejectTipSteelID.Text) ? LongRejectTipSteelID.Text.Trim() : "",
                    LongRejectGroove = ShortRejectGroove.IsChecked ?? false,
                    LongRejectKeyed = ShortRejectKeyed.IsChecked ?? false,
                    ShortRejectHead = ShortRejectHead.IsChecked ?? false,
                    ShortRejectHeadSteelID = !string.IsNullOrEmpty(ShortRejectHeadSteelID.Text) ? ShortRejectHeadSteelID.Text.Trim() : "",
                    ShortRejectAssembly = ShortRejectAssembly.IsChecked ?? false,
                    ShortRejectCap = ShortRejectCap.IsChecked ?? false,
                    ShortRejectCapSteelID = !string.IsNullOrEmpty(ShortRejectCapSteelID.Text) ? ShortRejectCapSteelID.Text.Trim() : "",
                    ShortRejectHolder = ShortRejectHolder.IsChecked ?? false,
                    ShortRejectHolderSteelID = !string.IsNullOrEmpty(ShortRejectHolderSteelID.Text) ? ShortRejectHolderSteelID.Text.Trim() : "",
                    ShortRejectPunch = ShortRejectPunch.IsChecked ?? false,
                    ShortRejectPunchSteelID = !string.IsNullOrEmpty(ShortRejectPunchSteelID.Text) ? ShortRejectPunchSteelID.Text.Trim() : "",
                    ShortRejectTip = ShortRejectTip.IsChecked ?? false,
                    ShortRejectTipSteelID = !string.IsNullOrEmpty(ShortRejectTipSteelID.Text) ? ShortRejectTipSteelID.Text.Trim() : "",
                    ShortRejectGroove = ShortRejectGroove.IsChecked ?? false,
                    ShortRejectKeyed = ShortRejectKeyed.IsChecked ?? false,
                    LowerHead = LowerHead.IsChecked ?? false,
                    LowerHeadSteelID = !string.IsNullOrEmpty(LowerHeadSteelID.Text) ? LowerHeadSteelID.Text.Trim() : "",
                    LowerAssembly = LowerAssembly.IsChecked ?? false,
                    LowerCap = LowerCap.IsChecked ?? false,
                    LowerCapSteelID = !string.IsNullOrEmpty(LowerCapSteelID.Text) ? LowerCapSteelID.Text.Trim() : "",
                    LowerHolder = LowerHolder.IsChecked ?? false,
                    LowerHolderSteelID = !string.IsNullOrEmpty(LowerHolderSteelID.Text) ? LowerHolderSteelID.Text.Trim() : "",
                    LowerPunch = LowerPunch.IsChecked ?? false,
                    LowerPunchSteelID = !string.IsNullOrEmpty(LowerPunchSteelID.Text) ? LowerPunchSteelID.Text.Trim() : "",
                    LowerTip = LowerTip.IsChecked ?? false,
                    LowerTipSteelID = !string.IsNullOrEmpty(LowerTipSteelID.Text) ? LowerTipSteelID.Text.Trim() : "",
                    LowerGroove = LowerGroove.IsChecked ?? false,
                    LowerKeyed = LowerKeyed.IsChecked ?? false,
                    UpperHead = UpperHead.IsChecked ?? false,
                    UpperHeadSteelID = !string.IsNullOrEmpty(UpperHeadSteelID.Text) ? UpperHeadSteelID.Text.Trim() : "",
                    UpperAssembly = UpperAssembly.IsChecked ?? false,
                    UpperCap = UpperCap.IsChecked ?? false,
                    UpperCapSteelID = !string.IsNullOrEmpty(UpperCapSteelID.Text) ? UpperCapSteelID.Text.Trim() : "",
                    UpperHolder = UpperHolder.IsChecked ?? false,
                    UpperHolderSteelID = !string.IsNullOrEmpty(UpperHolderSteelID.Text) ? UpperHolderSteelID.Text.Trim() : "",
                    UpperPunch = UpperPunch.IsChecked ?? false,
                    UpperPunchSteelID = !string.IsNullOrEmpty(UpperPunchSteelID.Text) ? UpperPunchSteelID.Text.Trim() : "",
                    UpperTip = UpperTip.IsChecked ?? false,
                    UpperTipSteelID = !string.IsNullOrEmpty(UpperTipSteelID.Text) ? UpperTipSteelID.Text.Trim() : "",
                    UpperGroove = UpperGroove.IsChecked ?? false,
                    UpperKeyed = UpperKeyed.IsChecked ?? false,
                    Misc = Misc.IsChecked ?? false,
                    MiscSteelID = !string.IsNullOrEmpty(MiscSteelID.Text) ? (MiscSteelID.Text.Trim().Length>3 ? "NA" : MiscSteelID.Text.Trim()) : "",
                    LowerCoreRod = CoreRod,
                    LowerCoreRodSteelID = CoreRodSteelID,
                    LowerCoreRodKey = CoreRodKey,
                    LowerCoreRodKeySteelID = CoreRodKeySteelID,
                    LowerCoreRodKeyCollar = CoreRodKeyCollar,
                    LowerCoreRodKeyCollarSteelID = CoreRodKeyCollarSteelID,
                    LowerCoreRodPunch = CoreRodPunch,
                    LowerCoreRodPunchSteelID = CoreRodPunchSteelID,
                    MachineNotes = !string.IsNullOrEmpty(MachineNotes.Text) ? MachineNotes.Text.Trim() : "",
                    UpperGrooveType = !string.IsNullOrEmpty(UpperGrooveType.Text) ? UpperGrooveType.Text.Trim() : "",
                    LowerGrooveType = !string.IsNullOrEmpty(LowerGrooveType.Text) ? LowerGrooveType.Text.Trim() : ""
                };
                _projectsContext.Dispose();
                return engineeringToolProject;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => GetToolProjectFromCurrentForm", ex.Message, user);
                return null;
            }
        }
        /// <summary>
        /// Enables or disables tablet drawings button based on hob numbers and drawings available.
        /// </summary>
        private void EnableTabletDrawingsButton()
        {
            try
            {
                if (!string.IsNullOrEmpty(UpperHobNumber.Text) || !string.IsNullOrEmpty(LowerHobDescription.Text) || !string.IsNullOrEmpty(ShortRejectHobNumber.Text) || !string.IsNullOrEmpty(ShortRejectHobNumber.Text))
                {
                    if (!string.IsNullOrEmpty(UpperHobNumber.Text))
                    {
                        if (int.TryParse(UpperHobNumber.Text, out int hobNumber))
                        {
                            string folderPrefix = IMethods.GetEDrawingsFolderPrefix(hobNumber);
                            string[] files = Directory.GetFiles(@"\\nsql03\Data1\DRAW\E-DRAWINGS\" + folderPrefix + @"-E-DRAWINGS\", hobNumber.ToString() + " * ");
                            if (files.Length > 0)
                            {
                                TabletDrawingsButton.IsEnabled = true;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(LowerHobNumber.Text))
                    {
                        if (int.TryParse(LowerHobNumber.Text, out int hobNumber))
                        {
                            string folderPrefix = "00";
                            if (hobNumber < 1000)
                            {
                                folderPrefix = "00";
                            }
                            else if (hobNumber < 10000)
                            {
                                folderPrefix = "0" + hobNumber.ToString().First().ToString();
                            }
                            else if (hobNumber < 100000)
                            {
                                folderPrefix = hobNumber.ToString().Substring(0, 2);
                            }
                            else
                            {
                                folderPrefix = hobNumber.ToString().Substring(0, 3);
                            }
                            string[] files = Directory.GetFiles(@"\\nsql03\Data1\DRAW\E-DRAWINGS", hobNumber.ToString() + "*");
                            if (files.Length > 0)
                            {
                                TabletDrawingsButton.IsEnabled = true;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(ShortRejectHobNumber.Text))
                    {
                        if (int.TryParse(ShortRejectHobNumber.Text, out int hobNumber))
                        {
                            string folderPrefix = "00";
                            if (hobNumber < 1000)
                            {
                                folderPrefix = "00";
                            }
                            else if (hobNumber < 10000)
                            {
                                folderPrefix = "0" + hobNumber.ToString().First().ToString();
                            }
                            else if (hobNumber < 100000)
                            {
                                folderPrefix = hobNumber.ToString().Substring(0, 2);
                            }
                            else
                            {
                                folderPrefix = hobNumber.ToString().Substring(0, 3);
                            }
                            string[] files = Directory.GetFiles(@"\\nsql03\Data1\DRAW\E-DRAWINGS", hobNumber.ToString() + "*");
                            if (files.Length > 0)
                            {
                                TabletDrawingsButton.IsEnabled = true;
                            }
                        }
                    }
                    else if (!string.IsNullOrEmpty(LongRejectHobNumber.Text))
                    {
                        if (int.TryParse(LongRejectHobNumber.Text, out int hobNumber))
                        {
                            string folderPrefix = "00";
                            if (hobNumber < 1000)
                            {
                                folderPrefix = "00";
                            }
                            else if (hobNumber < 10000)
                            {
                                folderPrefix = "0" + hobNumber.ToString().First().ToString();
                            }
                            else if (hobNumber < 100000)
                            {
                                folderPrefix = hobNumber.ToString().Substring(0, 2);
                            }
                            else
                            {
                                folderPrefix = hobNumber.ToString().Substring(0, 3);
                            }
                            string[] files = Directory.GetFiles(@"\\nsql03\Data1\DRAW\E-DRAWINGS", hobNumber.ToString() + "*");
                            if (files.Length > 0)
                            {
                                TabletDrawingsButton.IsEnabled = true;
                            }
                        }
                    }
                    else
                    {
                        TabletDrawingsButton.IsEnabled = false;
                    }
                }
                else
                {
                    TabletDrawingsButton.IsEnabled = false;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => EnableTabletDrawingsButton", ex.Message, user);
            }
        }

        /// <summary>
        /// Checks the project state (based on the switch button) and enables/disables the correct routing buttons.
        /// </summary>
        private void RefreshRoutingButtons()
        {
            using var _projectsContext = new ProjectsContext();
            try
            {
                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                    if (engineeringProject.OnHold == true)
                    {
                        StartButton.IsEnabled = false;
                        FinishButton.IsEnabled = false;
                        SubmitButton.IsEnabled = false;
                        CheckButton.IsEnabled = false;
                        PutOnHoldButton.Content = "Take Off Hold";
                        ArchivedOrInactive.Text = "On Hold";
                        ArchivedOrInactive.Visibility = Visibility.Visible;
                    }
                    else if (user.Department != "Engineering")
                    {
                        StartButton.IsEnabled = true;
                        FinishButton.IsEnabled = user.EmployeeCode == "E4477";
                        SubmitButton.IsEnabled = false;
                        CheckButton.IsEnabled = false;
                    }
                    else
                    {
                        if (CurrentProjectType.Text == "TABLETS")
                        {
                            if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                            {
                                if (engineeringProject.TabletChecked)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = false;
                                }
                                else if (engineeringProject.TabletSubmitted)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = true;
                                }
                                else if (engineeringProject.TabletDrawn)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = true;
                                    CheckButton.IsEnabled = true;
                                }
                                else if (engineeringProject.TabletStarted)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = true;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = false;
                                }
                                else
                                {
                                    StartButton.IsEnabled = true;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = false;
                                }
                            }
                            else
                            {
                                StartButton.IsEnabled = false;
                                FinishButton.IsEnabled = false;
                                SubmitButton.IsEnabled = false;
                                CheckButton.IsEnabled = false;
                            }
                        }
                        else
                        {
                            if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                            {
                                if (engineeringProject.ToolChecked)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = false;
                                }
                                else if (engineeringProject.ToolSubmitted)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = true;
                                }
                                else if (engineeringProject.ToolDrawn)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = true;
                                    CheckButton.IsEnabled = true;
                                }
                                else if (engineeringProject.ToolStarted)
                                {
                                    StartButton.IsEnabled = false;
                                    FinishButton.IsEnabled = true;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = false;
                                }
                                else
                                {
                                    StartButton.IsEnabled = true;
                                    FinishButton.IsEnabled = false;
                                    SubmitButton.IsEnabled = false;
                                    CheckButton.IsEnabled = false;
                                }
                            }
                            else
                            {
                                StartButton.IsEnabled = false;
                                FinishButton.IsEnabled = false;
                                SubmitButton.IsEnabled = false;
                                CheckButton.IsEnabled = false;
                            }
                        }
                    }
                }
                else
                {
                    StartButton.IsEnabled = false;
                    FinishButton.IsEnabled = false;
                    SubmitButton.IsEnabled = false;
                    CheckButton.IsEnabled = false;
                    PutOnHoldButton.IsEnabled = false;
                    CancelButton.IsEnabled = false;
                }
                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == (int.Parse(projectRevNumber) + 1).ToString()) || _projectsContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == (int.Parse(projectRevNumber) + 1).ToString()) || _projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber && p.OnHold))
                {
                    ReviseButton.IsEnabled = false;
                }
                else
                {
                    ReviseButton.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            _projectsContext.Dispose();
        }
        /// <summary>
        /// Enables or Disables all controls.
        /// </summary>
        /// <param name="isEnabled"></param>
        private void AllControlsEnabledOrDisabled(bool isEnabled)
        {
            try
            {
                SaveButton.IsEnabled = isEnabled;
                ReturnToCSR.IsEnabled = isEnabled;
                QuoteNumber.IsEnabled = isEnabled;
                QuoteRevNumber.IsEnabled = isEnabled;
                LinkQuoteButton.IsEnabled = isEnabled;
                RefOrderNumber.IsEnabled = isEnabled;
                UnitOfMeasure.IsEnabled = isEnabled;
                ReferenceQuoteNumber.IsEnabled = isEnabled;
                ReferenceQuoteRevNumber.IsEnabled = isEnabled;
                ReferenceProjectNumber.IsEnabled = isEnabled;
                ReferenceProjectRevNumber.IsEnabled = isEnabled;
                CustomerNumber.IsEnabled = isEnabled;
                CustomerName.IsEnabled = isEnabled;
                ShipToNumber.IsEnabled = isEnabled;
                ShipToLocNumber.IsEnabled = isEnabled;
                ShipToName.IsEnabled = isEnabled;
                EndUserNumber.IsEnabled = isEnabled;
                EndUserLocNumber.IsEnabled = isEnabled;
                EndUserName.IsEnabled = isEnabled;
                Product.IsEnabled = isEnabled;
                Attention.IsEnabled = isEnabled;
                MachineNumber.IsEnabled = isEnabled;
                MachineDescription.IsEnabled = isEnabled;
                DieOD.IsEnabled = isEnabled;
                DieOL.IsEnabled = isEnabled;
                MachineDescription.IsEnabled = isEnabled;
                DueDate.IsEditable = false;
                DueDate.IsEnabled = isEnabled;
                Priority.IsEnabled = isEnabled;
                Notes.IsReadOnly = !isEnabled;
                DieNumber.IsEnabled = isEnabled;
                DieShape.IsEnabled = isEnabled;
                TabletWidth.IsEnabled = isEnabled;
                TabletLength.IsEnabled = isEnabled;
                DieTolerances.IsEnabled = isEnabled;
                UpperHobNumber.IsEnabled = isEnabled;
                UpperCupType.IsEnabled = isEnabled;
                UpperCupDepth.IsEnabled = isEnabled;
                UpperLand.IsEnabled = isEnabled;
                UpperHobDescription.IsEnabled = isEnabled;
                UpperTolerances.IsEnabled = isEnabled;
                LowerHobNumber.IsEnabled = isEnabled;
                LowerCupType.IsEnabled = IsEnabled;
                LowerCupDepth.IsEnabled = isEnabled;
                LowerLand.IsEnabled = isEnabled;
                LowerHobDescription.IsEnabled = isEnabled;
                LowerTolerances.IsEnabled = isEnabled;
                ShortRejectCupType.IsEnabled = IsEnabled;
                ShortRejectHobNumber.IsEnabled = isEnabled;
                ShortRejectCupDepth.IsEnabled = isEnabled;
                ShortRejectLand.IsEnabled = isEnabled;
                ShortRejectHobDescription.IsEnabled = isEnabled;
                ShortRejectTolerances.IsEnabled = isEnabled;
                LongRejectCupType.IsEnabled = IsEnabled;
                LongRejectHobNumber.IsEnabled = isEnabled;
                LongRejectCupDepth.IsEnabled = isEnabled;
                LongRejectLand.IsEnabled = isEnabled;
                LongRejectHobDescription.IsEnabled = isEnabled;
                LongRejectTolerances.IsEnabled = isEnabled;
                MultiTipSketch.IsEnabled = isEnabled;
                SketchID.IsEnabled = isEnabled;
                MultiTipStyle.IsEnabled = isEnabled;
                TabletsRequired.IsEnabled = isEnabled;
                UpperTabletDrawing.IsEnabled = isEnabled;
                LowerTabletDrawing.IsEnabled = isEnabled;
                ShortRejectTabletDrawing.IsEnabled = isEnabled;
                LongRejectTabletDrawing.IsEnabled = isEnabled;
                Density.IsEnabled = isEnabled;
                DensityUnits.IsEnabled = isEnabled;
                Mass.IsEnabled = isEnabled;
                MassUnits.IsEnabled = isEnabled;
                Volume.IsEnabled = isEnabled;
                VolumeUnits.IsEnabled = isEnabled;
                TargetThickness.IsEnabled = isEnabled;
                TargetThicknessUnits.IsEnabled = isEnabled;
                FilmCoat.IsEnabled = isEnabled;
                FilmCoatStyle.IsEnabled = isEnabled;
                PrePick.IsEnabled = isEnabled;
                PrePickAmount.IsEnabled = isEnabled;
                PrePickUnits.IsEnabled = isEnabled;
                Taper.IsEnabled = isEnabled;
                TaperAmount.IsEnabled = isEnabled;
                TaperUnits.IsEnabled = isEnabled;
                ToolsRequired.IsEnabled = isEnabled;
                UpperPunch.IsEnabled = isEnabled;
                UpperPunchSteelID.IsEnabled = isEnabled;
                UpperAssembly.IsEnabled = isEnabled;
                UpperCap.IsEnabled = isEnabled;
                UpperCapSteelID.IsEnabled = isEnabled;
                UpperHolder.IsEnabled = isEnabled;
                UpperHolderSteelID.IsEnabled = isEnabled;
                UpperHead.IsEnabled = isEnabled;
                UpperHeadSteelID.IsEnabled = isEnabled;
                UpperTip.IsEnabled = isEnabled;
                UpperTipSteelID.IsEnabled = isEnabled;
                LowerPunch.IsEnabled = isEnabled;
                LowerPunchSteelID.IsEnabled = isEnabled;
                LowerAssembly.IsEnabled = isEnabled;
                LowerCap.IsEnabled = isEnabled;
                LowerCapSteelID.IsEnabled = isEnabled;
                LowerHolder.IsEnabled = isEnabled;
                LowerHolderSteelID.IsEnabled = isEnabled;
                LowerHead.IsEnabled = isEnabled;
                LowerHeadSteelID.IsEnabled = isEnabled;
                LowerTip.IsEnabled = isEnabled;
                LowerTipSteelID.IsEnabled = isEnabled;
                ShortRejectPunch.IsEnabled = isEnabled;
                ShortRejectPunchSteelID.IsEnabled = isEnabled;
                ShortRejectAssembly.IsEnabled = isEnabled;
                ShortRejectCap.IsEnabled = isEnabled;
                ShortRejectCapSteelID.IsEnabled = isEnabled;
                ShortRejectHolder.IsEnabled = isEnabled;
                ShortRejectHolderSteelID.IsEnabled = isEnabled;
                ShortRejectHead.IsEnabled = isEnabled;
                ShortRejectHeadSteelID.IsEnabled = isEnabled;
                ShortRejectTip.IsEnabled = isEnabled;
                ShortRejectTipSteelID.IsEnabled = isEnabled;
                LongRejectPunch.IsEnabled = isEnabled;
                LongRejectPunchSteelID.IsEnabled = isEnabled;
                LongRejectAssembly.IsEnabled = isEnabled;
                LongRejectCap.IsEnabled = isEnabled;
                LongRejectCapSteelID.IsEnabled = isEnabled;
                LongRejectHolder.IsEnabled = isEnabled;
                LongRejectHolderSteelID.IsEnabled = isEnabled;
                LongRejectHead.IsEnabled = isEnabled;
                LongRejectHeadSteelID.IsEnabled = isEnabled;
                LongRejectTip.IsEnabled = isEnabled;
                LongRejectTipSteelID.IsEnabled = isEnabled;
                Alignment.IsEnabled = isEnabled;
                AlignmentSteelID.IsEnabled = isEnabled;
                Key.IsEnabled = isEnabled;
                KeySteelID.IsEnabled = isEnabled;
                Misc.IsEnabled = isEnabled;
                MiscSteelID.IsEnabled = isEnabled;
                NumberOfTips.IsEnabled = isEnabled;
                Die.IsEnabled = isEnabled;
                DieSteelID.IsEnabled = isEnabled;
                DieAssembly.IsEnabled = isEnabled;
                DieComponent.IsEnabled = isEnabled;
                DieComponentSteelID.IsEnabled = isEnabled;
                DieHolder.IsEnabled = isEnabled;
                DieHolderSteelID.IsEnabled = isEnabled;
                DieInsert.IsEnabled = isEnabled;
                DieInsertSteelID.IsEnabled = isEnabled;
                DiePlate.IsEnabled = isEnabled;
                DiePlateSteelID.IsEnabled = isEnabled;
                DieSegment.IsEnabled = isEnabled;
                DieSegmentSteelID.IsEnabled = isEnabled;
                KeyType.IsEditable = false;
                KeyType.IsEnabled = false;
                KeyType.IsEnabled = isEnabled;
                KeyAngle.IsEnabled = isEnabled;
                KeyOrientation.IsEnabled = isEnabled;
                UpperKeyed.IsEnabled = isEnabled;
                LowerKeyed.IsEnabled = isEnabled;
                ShortRejectKeyed.IsEnabled = isEnabled;
                LongRejectKeyed.IsEnabled = isEnabled;
                UpperGrooveType.IsEnabled = isEnabled;
                LowerGrooveType.IsEnabled = isEnabled;
                UpperGroove.IsEnabled = isEnabled;
                LowerGroove.IsEnabled = isEnabled;
                ShortRejectGroove.IsEnabled = isEnabled;
                LongRejectGroove.IsEnabled = isEnabled;
                HeadType.IsEnabled = isEnabled;
                CarbideTips.IsEnabled = isEnabled;
                MachineNotes.IsEnabled = isEnabled;
                SpecificationsButton.IsEnabled = isEnabled;
                UpperHobDescriptionPlaceHolder.Visibility = UpperHobDescription.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                LowerHobDescriptionPlaceHolder.Visibility = LowerHobDescription.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                ShortRejectHobDescriptionPlaceHolder.Visibility = ShortRejectHobDescription.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                LongRejectHobDescriptionPlaceHolder.Visibility = LongRejectHobDescription.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                DieShapePlaceHolder.Visibility = DieShape.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                NumberOfTipsPlaceHolder.Visibility = NumberOfTips.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                DieODPlaceholder.Visibility = DieOD.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
                DieOLPlaceholder.Visibility = DieOL.Text.ToString().Length > 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => AllControlsEnabledOrDisabled", ex.Message, user);
            }
        }
        private void QuoteLink()
        {
            using var _projectsContext = new ProjectsContext();
            using var _nat01Context = new NAT01Context();
            EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber);
            QuoteNumber.Text = quote.QuoteNumber.ToString();
            QuoteRevNumber.Text = quote.QuoteRevNo.ToString();
            LinkQuoteWindow linkQuoteWindow = new LinkQuoteWindow(this);
            // Load data from quote 
            if (linkQuoteWindow.ShowDialog() == true)
            {
                switch (linkQuoteWindow.PopulateFromQuote)
                {
                    case true:
                        engineeringProject.QuoteNumber = quote.QuoteNumber.ToString();
                        engineeringProject.QuoteRevNumber = quote.QuoteRevNo.ToString();
                        projectLinkedToQuote = !projectLinkedToQuote;
                        LinkQuoteButton.Tag = Application.Current.Resources["unlinkDrawingImage"] as DrawingImage;
                        LinkQuoteButton.ToolTip = "Unlink Quote";
                        QuoteFolderButton.IsEnabled = true;
                        FillFromQuote(Convert.ToDouble(quote.QuoteNumber), Convert.ToInt16(quote.QuoteRevNo), linkQuoteWindow.TabletProject, linkQuoteWindow.ToolProject);
                        break;
                    default:
                        break;
                }
                // Link folders
                try
                {
                    string projectDirectory = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber;
                    string quoteDirectory = @"\\nsql03\data1\Quotes\" + quote.QuoteNumber.ToString();
                    string orderDirectory = _nat01Context.QuoteHeader.Any(q => q.QuoteNo == Convert.ToDouble(quote.QuoteNumber) && q.QuoteRevNo == Convert.ToDouble(quote.QuoteRevNo) && (double)q.OrderNo > 0) ? @"\\nsql03\data1\WorkOrders\" + _nat01Context.QuoteHeader.First(q => q.QuoteNo == Convert.ToDouble(quote.QuoteNumber) && q.QuoteRevNo == Convert.ToDouble(quote.QuoteRevNo) && (double)q.OrderNo > 0).OrderNo.ToString().Remove(6) : _nat01Context.QuoteHeader.Any(q => q.QuoteNo == Convert.ToDouble(quote.QuoteNumber) && (double)q.OrderNo > 0) ? @"\\nsql03\data1\WorkOrders\" + _nat01Context.QuoteHeader.First(q => q.QuoteNo == Convert.ToDouble(quote.QuoteNumber) && (double)q.OrderNo > 0).OrderNo.ToString().Remove(6) : "";
                    (string Message, string Caption, MessageBoxButton Button, MessageBoxImage Image, MessageBoxResult Result) messageBoxOverloads = IMethods.LinkFolders(projectDirectory, quoteDirectory, orderDirectory);
                    MessageBox.Show(messageBoxOverloads.Message, messageBoxOverloads.Caption, messageBoxOverloads.Button, messageBoxOverloads.Image, messageBoxOverloads.Result);
                }
                catch
                {
                    MessageBox.Show("Could not create shortcut." + "\n" + "A problem occured while verifying folders exist and creating the shortcuts.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show("Nothing has been linked together.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            _projectsContext.SaveChanges();
            _projectsContext.Dispose();
            _nat01Context.Dispose();
        }

        #region Events
        private void ProjectWindowXAML_Activated(object sender, EventArgs e)
        {
            ProjectFiles = GetProjectFiles(projectNumber);
        }
        /// <summary>
        /// Disposes the window after close
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectWindowXAML_Closed(object sender, EventArgs e)
        {
            Dispose();
        }
        /// <summary>
        /// Prevents a screen capture using the print screen key
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ProjectWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.PrintScreen)
            {
                this.Hide();
                System.Threading.Thread.Sleep(15);
                this.Show();
            }
        }
        private void OpenQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(QuoteNumber.Text, out int quoteNumber) && short.TryParse(QuoteRevNumber.Text, out short quoteRevNumber))
            {
                try
                {
                    Quote quote = new Quote(quoteNumber, quoteRevNumber);
                    QuoteInfoWindow quoteInfoWindow = new QuoteInfoWindow(quote, mainWindow, user);
                    quoteInfoWindow.Show();
                }
                catch (Exception ex)
                {
                    IMethods.WriteToErrorLog("ProjectWindow => OpenQuoteButton_Click", ex.Message, user);
                }
            }
        }
        /// <summary>
        /// Changes the units on the project from mm to in or vice-versa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Units_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                bool isInch = (string)Resources["UnitsText"] == "mm";
                double scalar = isInch ? 1 / 25.4 : 25.4;
                Resources["UnitsText"] = isInch ? "in" : "mm";
                List<TextBox> textBoxes = new List<TextBox>() {
            TabletWidth,
            TabletLength,
            UpperCupDepth,
            UpperLand,
            LowerCupDepth,
            LowerLand,
            ShortRejectCupDepth,
            ShortRejectLand,
            LongRejectCupDepth,
            LongRejectLand
            };
                foreach (TextBox textBox in textBoxes)
                {
                    if (!string.IsNullOrEmpty(textBox.Text) && double.TryParse(textBox.Text, out double number))
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => textBox.Text = Math.Round(number * scalar, 6).ToString()));
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => Units_MouseUp", ex.Message, user);
            }
        }

        //private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        //{
        //    string url = e.Uri.ToString();
        //    try
        //    {
        //        Process.Start(url);
        //    }
        //    catch
        //    {
        //        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        //        {
        //            url = url.Replace("&", "^&");
        //            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        //        }
        //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        //        {
        //            Process.Start("xdg-open", url);
        //        }
        //        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        //        {
        //            Process.Start("open", url);
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }
        //}

        /// <summary>
        /// Links or Unlinks the project from a quote.
        /// Calls to fill from quote if linking.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LinkQuoteButton_Click(object sender, RoutedEventArgs e)
        {
            using var _projectsContext = new ProjectsContext();
            using var _nat01Context = new NAT01Context();
            if (_projectsContext.EngineeringProjects.Any(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber))
            {
                try
                {
                    EngineeringProjects engineeringProject = _projectsContext.EngineeringProjects.First(ep => ep.ProjectNumber == projectNumber && ep.RevNumber == projectRevNumber);
                    // Already has a quote attached
                    if (projectLinkedToQuote)
                    {
                        engineeringProject.QuoteNumber = "";
                        engineeringProject.QuoteRevNumber = "";
                        projectLinkedToQuote = !projectLinkedToQuote;
                        QuoteFolderButton.IsEnabled = false;
                        LinkQuoteButton.Tag = Application.Current.Resources["linkDrawingImage"] as DrawingImage;
                    }
                    // Does not have quote or order attached
                    else
                    {
                        if (string.IsNullOrEmpty(QuoteNumber.Text) || !uint.TryParse(QuoteNumber.Text.ToString().Trim(), out uint quoteNumberInt))
                        {
                            MessageBox.Show("Please check the quote number.", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else if (string.IsNullOrEmpty(QuoteRevNumber.Text) || !ushort.TryParse(QuoteRevNumber.Text.ToString().Trim(), out ushort quoteRevNumberShort))
                        {
                            MessageBox.Show("Please check the quote revision number.", "Conversion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        else
                        {
                            string quoteNumber = quoteNumberInt.ToString();
                            string quoteRevNumber = quoteRevNumberShort.ToString();
                            // Check that Quote Exists in QuoteHeader
                            if (_nat01Context.QuoteHeader.Any(q => q.QuoteNo == Convert.ToDouble(quoteNumber) && q.QuoteRevNo == Convert.ToInt16(quoteRevNumber)))
                            {
                                LinkQuoteWindow linkQuoteWindow = new LinkQuoteWindow(this);
                                // Load data from quote 
                                if (linkQuoteWindow.ShowDialog() == true)
                                {
                                    switch (linkQuoteWindow.PopulateFromQuote)
                                    {
                                        case true:
                                            engineeringProject.QuoteNumber = quoteNumber;
                                            engineeringProject.QuoteRevNumber = quoteRevNumber;
                                            projectLinkedToQuote = !projectLinkedToQuote;
                                            LinkQuoteButton.Tag = Application.Current.Resources["unlinkDrawingImage"] as DrawingImage;
                                            LinkQuoteButton.ToolTip = "Unlink Quote";
                                            QuoteFolderButton.IsEnabled = true;
                                            FillFromQuote(Convert.ToDouble(quoteNumber), Convert.ToInt16(quoteRevNumber), linkQuoteWindow.TabletProject, linkQuoteWindow.ToolProject);
                                            break;
                                        default:
                                            break;
                                    }
                                    // Link folders
                                    try
                                    {
                                        string projectDirectory = @"\\engserver\workstations\TOOLING AUTOMATION\Project Specifications\" + projectNumber;
                                        string quoteDirectory = @"\\nsql03\data1\Quotes\" + quoteNumber;
                                        string orderDirectory = _nat01Context.QuoteHeader.Any(q => q.QuoteNo == Convert.ToDouble(quoteNumber) && q.QuoteRevNo == Convert.ToDouble(quoteRevNumber) && (double)q.OrderNo > 0) ? @"\\nsql03\data1\WorkOrders\" + _nat01Context.QuoteHeader.First(q => q.QuoteNo == Convert.ToDouble(quoteNumber) && q.QuoteRevNo == Convert.ToDouble(quoteRevNumber) && (double)q.OrderNo > 0).OrderNo.ToString().Remove(6) : _nat01Context.QuoteHeader.Any(q => q.QuoteNo == Convert.ToDouble(quoteNumber) && (double)q.OrderNo > 0) ? @"\\nsql03\data1\WorkOrders\" + _nat01Context.QuoteHeader.First(q => q.QuoteNo == Convert.ToDouble(quoteNumber) && (double)q.OrderNo > 0).OrderNo.ToString().Remove(6) : "";
                                        (string Message, string Caption, MessageBoxButton Button, MessageBoxImage Image, MessageBoxResult Result) messageBoxOverloads = IMethods.LinkFolders(projectDirectory, quoteDirectory, orderDirectory);
                                        MessageBox.Show(messageBoxOverloads.Message, messageBoxOverloads.Caption, messageBoxOverloads.Button, messageBoxOverloads.Image, messageBoxOverloads.Result);
                                    }
                                    catch
                                    {
                                        MessageBox.Show("Could not create shortcut." + "\n" + "A problem occured while verifying folders exist and creating the shortcuts.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("Nothing has been linked together.", "Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                                }

                            }
                            else
                            {
                                MessageBox.Show("Quote entered does not exist.", "Quote Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                    }
                    _projectsContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                MessageBox.Show("Cannot verify this project exists.", "Project Does Not Exist", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            _projectsContext.Dispose();
            _nat01Context.Dispose();
        }
        private void DueDate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetProjectWarningText();
        }
        private void ProjectWindowXAML_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                if (!saved)
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to Cancel?" + "\n"
                        + "Your progress will not be saved.", "Are You Sure?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                    if (result != MessageBoxResult.Yes)
                    {
                        e.Cancel = true;
                        
                    }
                    else
                    {
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = null));
                        MultiTipSketchViewer.Dispose();
                        using var projectsContext = new ProjectsContext();
                        EngineeringProjects project = projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                        projectsContext.EngineeringProjects.Remove(project);
                        projectsContext.SaveChanges();
                        projectsContext.Dispose();
                        Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow.xaml.cs => ProjectWindowXAML_Closing", ex.Message, user);
            }
        }

        /// <summary>
        /// Opens the ProjectSpecificationsWindow to check or see what is checked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpecificationsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ProjectSpecificationsWindow projectSpecificationsWindow = new ProjectSpecificationsWindow(this, CreateButton.Visibility == Visibility.Visible, NewDrawing, UpdateExistingDrawing, UpdateTextOnDrawing, PerSampleTablet, RefTabletDrawing,
                    PerSampleTool, RefToolDrawing, PerSuppliedPicture, RefNatoliDrawing, RefNonNatoliDrawing, BinLocation);
                if (projectSpecificationsWindow.ShowDialog() == true)
                {
                    NewDrawing = projectSpecificationsWindow.newDrawing;
                    UpdateExistingDrawing = projectSpecificationsWindow.updateExistingDrawing;
                    UpdateTextOnDrawing = projectSpecificationsWindow.updateTextOnDrawing;
                    PerSampleTablet = projectSpecificationsWindow.perSampleTablet;
                    RefTabletDrawing = projectSpecificationsWindow.refTabletDrawing;
                    PerSampleTool = projectSpecificationsWindow.perSampleTool;
                    RefToolDrawing = projectSpecificationsWindow.refToolDrawing;
                    PerSuppliedPicture = projectSpecificationsWindow.perSuppliedPicture;
                    RefNatoliDrawing = projectSpecificationsWindow.refNatoliDrawing;
                    RefNonNatoliDrawing = projectSpecificationsWindow.refNonNatoliDrawing;
                    BinLocation = projectSpecificationsWindow.binLocation;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => SpecificationsButton_Click", ex.Message, user);
            }
        }
        /// <summary>
        /// Opens the CoreRodWindow to specify core rod details.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CoreRodButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CoreRodWindow coreRodWindow = new CoreRodWindow(this, CreateButton.Visibility == Visibility.Visible,
                    CoreRod, CoreRodSteelID,
                    CoreRodKey, CoreRodKeySteelID,
                    CoreRodKeyCollar, CoreRodKeyCollarSteelID,
                    CoreRodPunch, CoreRodPunchSteelID);
                if (coreRodWindow.ShowDialog() == true)
                {
                    CoreRod = coreRodWindow.CoreRod;
                    CoreRodSteelID = coreRodWindow.CoreRodSteelID;
                    CoreRodKey = coreRodWindow.CoreRodKey;
                    CoreRodKeySteelID = coreRodWindow.CoreRodKeySteelID;
                    CoreRodKeyCollar = coreRodWindow.CoreRodKeyCollar;
                    CoreRodKeyCollarSteelID = coreRodWindow.CoreRodKeyCollarSteelID;
                    CoreRodPunch = coreRodWindow.CoreRodPunch;
                    CoreRodPunchSteelID = coreRodWindow.CoreRodPunchSteelID;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => CoreRodButton_Click", ex.Message, user);
            }
        }
        /// <summary>
        /// Switches the routing from tablets to tools or vice-versa.
        /// Changes the text to show what is currently being routed.
        /// Calls the buttons to be refreshed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchProjectRoutingType_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (CurrentProjectType.Text == "TABLETS")
            {
                CurrentProjectType.Text = "TOOLS";
                RefreshRoutingButtons();
            }
            else
            {
                CurrentProjectType.Text = "TABLETS";
                RefreshRoutingButtons();
            }
        }
        /// <summary>
        /// Used to hide a text placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ComboBox)
                {
                    ComboBox comboBox = sender as ComboBox;
                    if (comboBox.Name.ToString() == "DieShape")
                    {
                        DieShapePlaceHolder.Visibility = Visibility.Collapsed;
                    }
                    else if (comboBox.Name.ToString() == "NumberOfTips")
                    {
                        NumberOfTipsPlaceHolder.Visibility = Visibility.Collapsed;
                    }

                }
                else if (sender is TextBox textBox)
                {
                    string name = textBox.Name.ToString();
                    switch (name)
                    {
                        case "UpperHobDescription":
                            UpperHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            break;
                        case "LowerHobDescription":
                            LowerHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            break;
                        case "ShortRejectHobDescription":
                            ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            break;
                        case "LongRejectHobDescription":
                            LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            break;
                        case "DieOD":
                            DieODPlaceholder.Visibility = Visibility.Collapsed;
                            break;
                        case "DieOL":
                            DieOLPlaceholder.Visibility = Visibility.Collapsed;
                            break;
                        case "NumberOfTips":
                            NumberOfTipsPlaceHolder.Visibility = Visibility.Collapsed;
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => TextBox_GotFocus", ex.Message, user);
            }
        }
        /// <summary>
        /// Used to hide or show a text placeholder.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is ComboBox)
                {
                    ComboBox comboBox = sender as ComboBox;
                    if (comboBox.Name.ToString() == "DieShape")
                    {
                        if (!string.IsNullOrWhiteSpace(DieShape.Text.ToString()))
                        {
                            DieShapePlaceHolder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            DieShapePlaceHolder.Visibility = Visibility.Visible;
                        }
                    }
                    else if (comboBox.Name.ToString() == "NumberOfTips")
                    {
                        if (!string.IsNullOrWhiteSpace(NumberOfTips.Text.ToString()))
                        {
                            NumberOfTipsPlaceHolder.Visibility = Visibility.Collapsed;
                        }
                        else
                        {
                            NumberOfTipsPlaceHolder.Visibility = Visibility.Visible;
                        }
                    }
                }
                else if (sender is TextBox textBox)
                {
                    string name = textBox.Name.ToString();
                    switch (name)
                    {
                        case "UpperHobDescription":
                            if (!string.IsNullOrWhiteSpace(UpperHobDescription.Text.ToString()))
                            {
                                UpperHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                UpperHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                            }
                            break;
                        case "LowerHobDescription":
                            if (!string.IsNullOrWhiteSpace(LowerHobDescription.Text.ToString()))
                            {
                                LowerHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                LowerHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                            }
                            break;
                        case "ShortRejectHobDescription":
                            if (!string.IsNullOrWhiteSpace(ShortRejectHobDescription.Text.ToString()))
                            {
                                ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                ShortRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                            }
                            break;
                        case "LongRejectHobDescription":
                            if (!string.IsNullOrWhiteSpace(LongRejectHobDescription.Text.ToString()))
                            {
                                LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                LongRejectHobDescriptionPlaceHolder.Visibility = Visibility.Visible;
                            }
                            break;
                        case "DieOD":
                            if (!string.IsNullOrWhiteSpace(DieOD.Text.ToString()))
                            {
                                DieODPlaceholder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                DieODPlaceholder.Visibility = Visibility.Visible;
                            }
                            break;
                        case "DieOL":
                            if (!string.IsNullOrWhiteSpace(DieOL.Text.ToString()))
                            {
                                DieOLPlaceholder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                DieOLPlaceholder.Visibility = Visibility.Visible;
                            }
                            break;
                        case "NumberOfTips":
                            if (!string.IsNullOrWhiteSpace(NumberOfTips.Text.ToString()))
                            {
                                NumberOfTipsPlaceHolder.Visibility = Visibility.Collapsed;
                            }
                            else
                            {
                                NumberOfTipsPlaceHolder.Visibility = Visibility.Visible;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => TextBox_LostFocus", ex.Message, user);
            }
        }
        /// <summary>
        /// Changes all hob tolerances if they are empty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PunchTolerance_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                ComboBox comboBox = sender as ComboBox;
                if (string.IsNullOrWhiteSpace(UpperTolerances.Text) && UpperTolerances.SelectedItem == null)
                {
                    UpperTolerances.SelectedItem = comboBox.SelectedItem;
                }
                if (string.IsNullOrWhiteSpace(LowerTolerances.Text) && LowerTolerances.SelectedItem == null)
                {
                    LowerTolerances.SelectedItem = comboBox.SelectedItem;
                }
                if (string.IsNullOrWhiteSpace(ShortRejectTolerances.Text) && ShortRejectTolerances.SelectedItem == null)
                {
                    ShortRejectTolerances.SelectedItem = comboBox.SelectedItem;
                }
                if (string.IsNullOrWhiteSpace(LongRejectTolerances.Text) && LongRejectTolerances.SelectedItem == null)
                {
                    LongRejectTolerances.SelectedItem = comboBox.SelectedItem;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => PunchTolerance_SelectionChanged", ex.Message, user);
            }
        }
        /// <summary>
        /// Enables MultiTipSketch Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TabletsRequired_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MultiTipSketch.IsChecked == true && (ToolsRequired.IsChecked == true || TabletsRequired.IsChecked == true))
                {
                    MultiTipSketchTabItem.IsEnabled = true;
                }
                SetProjectWarningText();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => TabletsRequired_Click", ex.Message, user);
            }
        }
        /// <summary>
        /// Enables MultiTipSketch Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ToolsRequired_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MultiTipSketch.IsChecked == true && (ToolsRequired.IsChecked == true || TabletsRequired.IsChecked == true))
                {
                    MultiTipSketchTabItem.IsEnabled = true;
                }
                SetProjectWarningText();
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => ToolsRequired_Click", ex.Message, user);
            }
        }
        /// <summary>
        /// Enables MultiTipSketch Tab
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MultiTipSketch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MultiTipSketch.IsChecked == true && (ToolsRequired.IsChecked == true || TabletsRequired.IsChecked == true))
                {
                    MultiTipSketchTabItem.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => MultiTipSketch_Click", ex.Message, user);
            }
        }
        private void TabletDrawingsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dictionary<int,string> hobNumbers = new Dictionary<int, string>();
                if (int.TryParse(UpperHobNumber.Text, out int upperHobNumber) && upperHobNumber > 0)
                {
                    hobNumbers.Add(0,upperHobNumber.ToString());
                }
                if (int.TryParse(LowerHobNumber.Text, out int lowerHobNumber) && lowerHobNumber > 0)
                {
                    hobNumbers.Add(1,lowerHobNumber.ToString());
                }
                if (int.TryParse(ShortRejectHobNumber.Text, out int shortRejectHobNumber) && shortRejectHobNumber > 0)
                {
                    hobNumbers.Add(2,shortRejectHobNumber.ToString());
                }
                if (int.TryParse(LongRejectHobNumber.Text, out int longRejectHobNumber) && longRejectHobNumber > 0)
                {
                    hobNumbers.Add(3,longRejectHobNumber.ToString());
                }
                if (hobNumbers.Count > 0)
                {
                    TabletDrawings tabletDrawings = new TabletDrawings(hobNumbers, this as Window, projectNumber, projectRevNumber);
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => TabletDrawingsButton_Click", ex.Message, user);
            }

        }
        private void EngineeringCommentButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if comment file exists
            string commentPath = projectsDirectory + projectNumber + "\\EngineeringComments.txt";
            FileStream comment = new FileStream(commentPath, FileMode.OpenOrCreate);

            System.Diagnostics.Process.Start("notepad.exe", commentPath);

            // Dispose of object
            comment.Dispose();
        }
        private void OpenHobAndDieInfo_Click(object sender, RoutedEventArgs e)
        {
            HobInformation hi = new HobInformation();
            hi.Topmost = true;
            hi.Show();
        }

        #region TextBox Changes And Timer
        /// <summary>
        /// Intelligently loads data into control based on provided input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EditedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                string _editedText = editedText.ToString().Trim();
                if (!string.IsNullOrWhiteSpace(_editedText))
                {
                    decimal conversion = (string)Resources["UnitsText"] == "in" ? 1 : (decimal)25.4;
                    string _editedTextBoxName = editedTextBoxName.ToString();
                    string _upperCupType = upperCupType.Trim();
                    string _upperCupDepth = upperCupDepth.Trim();
                    string _lowerCupType = lowerCupType.Trim();
                    string _lowerCupDepth = lowerCupDepth.Trim();
                    string _shortRejectCupType = shortRejectCupType.Trim();
                    string _shortRejectCupDepth = shortRejectCupDepth.Trim();
                    string _longRejectCupType = longRejectCupType.Trim();
                    string _longRejectCupDepth = longRejectCupDepth.Trim();
                    string _upperLand = upperLand.Trim();
                    string _lowerLand = lowerLand.Trim();
                    string _shortRejectLand = shortRejectLand.Trim();
                    string _longRejectLand = longRejectLand.Trim();
                    using var _necContext = new NECContext();
                    using var _nat01Context = new NAT01Context();
                    using var _nat02Context = new NAT02Context();
                    switch (_editedTextBoxName)
                    {
                        case "CustomerNumber":
                            if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == _editedText.Trim()))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerName.Text = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == _editedText).Custname.Trim()));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => CustomerName.Text = ""));
                            }
                            break;
                        case "ShipToNumber":
                            if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == _editedText.Trim()))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToName.Text = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == _editedText).Custname.Trim()));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShipToName.Text = ""));
                            }
                            break;
                        case "EndUserNumber":
                            if (_necContext.Rm00101.Any(c => c.Custnmbr.Trim() == _editedText.Trim()))
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserName.Text = _necContext.Rm00101.First(c => c.Custnmbr.Trim() == _editedText).Custname.Trim()));
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EndUserName.Text = ""));
                            }
                            break;
                        case "DieNumber":
                            if (_nat01Context.DieList.Any(d => !string.IsNullOrWhiteSpace(d.DieId) && d.DieId.Trim() == _editedText))
                            {
                                DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == _editedText);
                                LightUpBorder(DieNumber);
                                // Use Note2 from Die List if present
                                if (!string.IsNullOrWhiteSpace(die.Note2))
                                {
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => die.Note2.Trim()));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                }
                                // Use the shape ID description
                                else
                                {
                                    short shapeID = (short)die.ShapeId;
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                }
                                float width = (float)die.WidthMinorAxis * (float)conversion;
                                float length = (float)die.LengthMajorAxis * (float)conversion;
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));
                            }
                            else
                            {
                                //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = ""));
                                //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = ""));
                                //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = ""));
                            }
                            break;
                        case "UpperHobNumber":
                            // It is a hob number that exists
                            if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0')))
                            {
                                HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0'));
                                LightUpBorder(DieNumber);
                                string note1 = hob.Note1.Trim();
                                string note2 = hob.Note2.Trim();
                                string note3 = hob.Note3.Trim();
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                                {
                                    UpperCupType.Text = hob.CupCode == null ? "" : hob.CupCode.ToString().Trim() + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim();
                                    UpperCupDepth.Text = hob.CupDepth == null ? "0.0000" : (Convert.ToSingle(hob.CupDepth) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    UpperLand.Text = hob.Land == null ? "0.0000" : (Convert.ToSingle(hob.Land) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    UpperHobDescription.Text = note1 + " " + note2 + " " + note3;
                                    DieNumber.Text = hob.DieId.Trim();
                                }));
                                if (_nat01Context.DieList.Any(d => d.DieId.Trim() == hob.DieId.Trim()))
                                {
                                    DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());
                                    if (!string.IsNullOrWhiteSpace(die.Note2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = die.Note2.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    // Use the shape ID description
                                    else
                                    {
                                        short shapeID = (short)die.ShapeId;
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    float width = (float)die.WidthMinorAxis * (float)conversion;
                                    float length = (float)die.LengthMajorAxis * (float)conversion;
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));
                                }
                            }
                            else
                            {
                                // It is a new hob
                                if (_editedText.ToUpper() == "NEW")
                                {
                                    // Take the cup type from other items
                                    if (!string.IsNullOrEmpty(_lowerCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _lowerCupType));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _shortRejectCupType));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _longRejectCupType));
                                    }

                                    // Take the cup depth from other items
                                    if (!string.IsNullOrEmpty(_lowerCupDepth) && double.TryParse(_lowerCupDepth.Trim(), out double d))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = d.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectCupDepth) && double.TryParse(_shortRejectCupDepth.Trim(), out double d1))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = d1.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectCupDepth) && double.TryParse(_longRejectCupDepth.Trim(), out double d2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = d2.ToString("F4", CultureInfo.InvariantCulture)));
                                    }

                                    // Take the land from other items
                                    if (!string.IsNullOrEmpty(_lowerLand) && double.TryParse(_lowerLand.Trim(), out double d3))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = d3.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectLand) && double.TryParse(_shortRejectLand.Trim(), out double d4))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = d4.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectLand) && double.TryParse(_longRejectLand.Trim(), out double d5))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = d5.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                }
                                // Does not match anything, drive to nothing
                                else
                                {
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupDepth.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperLand.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescription.Text = ""));
                                }
                            }
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperHobDescriptionPlaceHolder.Visibility = UpperHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                            break;
                        case "LowerHobNumber":
                            if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0')))
                            {
                                HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0'));
                                LightUpBorder(DieNumber);
                                string note1 = hob.Note1.Trim();
                                string note2 = hob.Note2.Trim();
                                string note3 = hob.Note3.Trim();
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                                {
                                    LowerCupType.Text = hob.CupCode == null ? "" : hob.CupCode.ToString().Trim() + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim();
                                    LowerCupDepth.Text = hob.CupDepth == null ? "0.0000" : (Convert.ToSingle(hob.CupDepth) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    LowerLand.Text = hob.Land == null ? "0.0000" : (Convert.ToSingle(hob.Land) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    LowerHobDescription.Text = note1 + " " + note2 + " " + note3;
                                    DieNumber.Text = hob.DieId.Trim();
                                }));
                                if (_nat01Context.DieList.Any(d => d.DieId.Trim() == hob.DieId.Trim()))
                                {
                                    DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());
                                    if (!string.IsNullOrWhiteSpace(die.Note2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => die.Note2.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    // Use the shape ID description
                                    else
                                    {
                                        short shapeID = (short)die.ShapeId;
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    float width = (float)die.WidthMinorAxis * (float)conversion;
                                    float length = (float)die.LengthMajorAxis * (float)conversion;
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));
                                }
                            }
                            else
                            {
                                if (_editedText.ToUpper() == "NEW")
                                {
                                    if (!string.IsNullOrEmpty(_upperCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _upperCupType));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _shortRejectCupType));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _longRejectCupType));
                                    }

                                    if (!string.IsNullOrEmpty(_upperCupDepth) && double.TryParse(_upperCupDepth.Trim(), out double d))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = d.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectCupDepth) && double.TryParse(_shortRejectCupDepth.Trim(), out double d1))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = d1.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectCupDepth) && double.TryParse(_longRejectCupDepth.Trim(), out double d2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = d2.ToString("F4", CultureInfo.InvariantCulture)));
                                    }

                                    if (!string.IsNullOrEmpty(_upperLand) && double.TryParse(_upperLand.Trim(), out double d3))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = d3.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectLand) && double.TryParse(_shortRejectLand.Trim(), out double d4))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = d4.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectLand) && double.TryParse(_longRejectLand.Trim(), out double d5))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = d5.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                }
                                else
                                {
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupType.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerCupDepth.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerLand.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescription.Text = ""));
                                }
                            }
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LowerHobDescriptionPlaceHolder.Visibility = LowerHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                            break;
                        case "ShortRejectHobNumber":
                            if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0')))
                            {
                                HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0'));
                                LightUpBorder(DieNumber);
                                string note1 = hob.Note1.Trim();
                                string note2 = hob.Note2.Trim();
                                string note3 = hob.Note3.Trim();
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                                {
                                    ShortRejectCupType.Text = hob.CupCode == null ? "" : hob.CupCode.ToString().Trim() + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim();
                                    ShortRejectCupDepth.Text = hob.CupDepth == null ? "0.0000" : (Convert.ToSingle(hob.CupDepth) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    ShortRejectLand.Text = hob.Land == null ? "0.0000" : (Convert.ToSingle(hob.Land) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    ShortRejectHobDescription.Text = note1 + " " + note2 + " " + note3;
                                    DieNumber.Text = hob.DieId.Trim();
                                }));
                                if (_nat01Context.DieList.Any(d => d.DieId.Trim() == hob.DieId.Trim()))
                                {
                                    DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());
                                    if (!string.IsNullOrWhiteSpace(die.Note2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => die.Note2.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    // Use the shape ID description
                                    else
                                    {
                                        short shapeID = (short)die.ShapeId;
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    float width = (float)die.WidthMinorAxis * (float)conversion;
                                    float length = (float)die.LengthMajorAxis * (float)conversion;
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));
                                }
                            }
                            else
                            {
                                if (_editedText.ToUpper() == "NEW")
                                {
                                    if (!string.IsNullOrEmpty(_upperCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _upperCupType));
                                    }
                                    else if (!string.IsNullOrEmpty(_lowerCupDepth))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _lowerCupDepth));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _longRejectCupType));
                                    }

                                    if (!string.IsNullOrEmpty(_upperCupDepth) && double.TryParse(_upperCupDepth.Trim(), out double d))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = d.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_lowerCupDepth) && double.TryParse(_lowerCupDepth.Trim(), out double d1))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = d1.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectCupDepth) && double.TryParse(_longRejectCupDepth.Trim(), out double d2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = d2.ToString("F4", CultureInfo.InvariantCulture)));
                                    }

                                    if (!string.IsNullOrEmpty(_upperLand) && double.TryParse(_upperLand.Trim(), out double d3))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = d3.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_lowerLand) && double.TryParse(_lowerLand.Trim(), out double d4))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = d4.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_longRejectLand) && double.TryParse(_longRejectLand.Trim(), out double d5))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = d5.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                }
                                else
                                {
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupType.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectCupDepth.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectLand.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescription.Text = ""));
                                }
                            }
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => ShortRejectHobDescriptionPlaceHolder.Visibility = ShortRejectHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                            break;
                        case "LongRejectHobNumber":
                            if (_nat01Context.HobList.Any(h => !string.IsNullOrWhiteSpace(h.HobNo) && (!string.IsNullOrWhiteSpace(h.Note1) || !string.IsNullOrWhiteSpace(h.Note2) || !string.IsNullOrWhiteSpace(h.Note3)) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0')))
                            {
                                HobList hob = _nat01Context.HobList.First(h => !string.IsNullOrWhiteSpace(h.HobNo) && h.HobNo.Trim() == _editedText.Trim().PadLeft(6, '0'));
                                LightUpBorder(DieNumber);
                                string note1 = hob.Note1.Trim();
                                string note2 = hob.Note2.Trim();
                                string note3 = hob.Note3.Trim();
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                                {
                                    LongRejectCupType.Text = hob.CupCode == null ? "" : hob.CupCode.ToString().Trim() + " - " + _nat01Context.CupConfig.First(c => c.CupID == hob.CupCode).Description.Trim();
                                    LongRejectCupDepth.Text = hob.CupDepth == null ? "0.0000" : (Convert.ToSingle(hob.CupDepth) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    LongRejectLand.Text = hob.Land == null ? "0.0000" : (Convert.ToSingle(hob.Land) * (float)conversion).ToString("F4", CultureInfo.InvariantCulture);
                                    LongRejectHobDescription.Text = note1 + " " + note2 + " " + note3;
                                    DieNumber.Text = hob.DieId.Trim();
                                }));
                                if (_nat01Context.DieList.Any(d => d.DieId.Trim() == hob.DieId.Trim()))
                                {
                                    DieList die = _nat01Context.DieList.First(d => d.DieId.Trim() == hob.DieId.Trim());
                                    if (!string.IsNullOrWhiteSpace(die.Note2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => die.Note2.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    // Use the shape ID description
                                    else
                                    {
                                        short shapeID = (short)die.ShapeId;
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShape.Text = _nat01Context.ShapeFields.First(s => s.ShapeID == shapeID).ShapeDescription.Trim()));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieShapePlaceHolder.Visibility = Visibility.Collapsed));
                                    }
                                    float width = (float)die.WidthMinorAxis * (float)conversion;
                                    float length = (float)die.LengthMajorAxis * (float)conversion;
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletWidth.Text = width.ToString("F4", CultureInfo.InvariantCulture)));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => TabletLength.Text = length == 0 ? "" : length.ToString("F4", CultureInfo.InvariantCulture)));
                                }
                            }
                            else
                            {
                                if (_editedText.ToUpper() == "NEW")
                                {
                                    if (!string.IsNullOrEmpty(_upperCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _upperCupType));
                                    }
                                    else if (!string.IsNullOrEmpty(_lowerCupDepth))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _lowerCupDepth));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectCupType))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => UpperCupType.Text = _shortRejectCupType));
                                    }

                                    if (!string.IsNullOrEmpty(_upperCupDepth) && double.TryParse(_upperCupDepth.Trim(), out double d))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = d.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_lowerCupDepth) && double.TryParse(_lowerCupDepth.Trim(), out double d1))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = d1.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectCupDepth) && double.TryParse(_shortRejectCupDepth.Trim(), out double d2))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = d2.ToString("F4", CultureInfo.InvariantCulture)));
                                    }

                                    if (!string.IsNullOrEmpty(_upperLand) && double.TryParse(_upperLand.Trim(), out double d3))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = d3.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_lowerLand) && double.TryParse(_lowerLand.Trim(), out double d4))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = d4.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                    else if (!string.IsNullOrEmpty(_shortRejectLand) && double.TryParse(_shortRejectLand.Trim(), out double d5))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = d5.ToString("F4", CultureInfo.InvariantCulture)));
                                    }
                                }
                                else
                                {
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupType.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectCupDepth.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectLand.Text = ""));
                                    //Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescription.Text = ""));
                                }
                            }
                            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => LongRejectHobDescriptionPlaceHolder.Visibility = LongRejectHobDescription.Text.Length == 0 ? Visibility.Visible : Visibility.Hidden));
                            break;
                        case "MachineNumber":
                            if (!string.IsNullOrWhiteSpace(_editedText) && short.TryParse(_editedText, out short _machineNo))
                            {
                                if (_nat01Context.MachineList.Any(m => m.MachineNo == _machineNo))
                                {
                                    string description = _nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Description.Trim();
                                    string od = ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Od * conversion).ToString();
                                    string ol = ((decimal)_nat01Context.MachineList.First(m => m.MachineNo == _machineNo).Ol * conversion).ToString();
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MachineDescription.Text = description));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOD.Text = od));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieODPlaceholder.Visibility = Visibility.Collapsed));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOL.Text = ol));
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOLPlaceholder.Visibility = Visibility.Collapsed));
                                }
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MachineDescription.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOD.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieODPlaceholder.Visibility = Visibility.Visible));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOL.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => DieOLPlaceholder.Visibility = Visibility.Visible));
                            }
                            break;
                        case "SketchID":
                            if (!string.IsNullOrEmpty(_editedText) && int.TryParse(_editedText, out int _sketchID) && _nat02Context.MultiTipSketchInformation.Any(s => s.ID == _sketchID && s.Width != null))
                            {

                                MultiTipSketchInformation multiTipSketchInformation = _nat02Context.MultiTipSketchInformation.First(s => s.ID == _sketchID);
                                string ID = multiTipSketchInformation.ID.ToString();
                                string dieNumber = multiTipSketchInformation.DieNumber.ToString();
                                string width = ((decimal)multiTipSketchInformation.Width).ToString("F4", CultureInfo.InvariantCulture);
                                string type = multiTipSketchInformation.AssembledOrSolid == 'S' ? "SOLID" : multiTipSketchInformation.AssembledOrSolid == 'S' ? "ASSEMBLED" : "NEITHER_ASSEMBLED_OR_SOLID";
                                string tipQTY = multiTipSketchInformation.TotalNumberOfTips.ToString();
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTips.IsEnabled = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTips.Text = tipQTY));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTips.IsEnabled = false));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTipsPlaceHolder.Visibility = Visibility.Hidden));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipStyle.IsEnabled = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipStyle.Text = type));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipStyle.IsEnabled = false));
                                if (multiTipSketchInformation.Length != null)
                                {
                                    string length = ((double)multiTipSketchInformation.Length).ToString("F4", CultureInfo.InvariantCulture);
                                    string path = @"R:\tools\MULTI-TIP SKETCHES\" + width + " X " + length + "\\" + dieNumber + "\\" + type + "\\" + tipQTY + "-TIP " + type + "\\" + ID + "\\" + "MULTI TIP SKETCH " + ID + ".pdf";
                                    if (System.IO.File.Exists(path))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Visible));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = new Uri(path, UriKind.Absolute)));
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Collapsed));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = null));
                                    }
                                }
                                else
                                {
                                    string path = @"R:\tools\MULTI-TIP SKETCHES\" + width + "\\" + dieNumber + "\\" + type + "\\" + tipQTY + "-TIP " + type + "\\" + ID + "\\" + "MULTI TIP SKETCH " + ID + ".pdf";
                                    if (System.IO.File.Exists(path))
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Visible));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = new Uri(path, UriKind.Absolute)));
                                    }
                                    else
                                    {
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Collapsed));
                                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = null));
                                    }
                                }
                            }
                            else
                            {
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTips.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTips.IsEnabled = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => NumberOfTipsPlaceHolder.Visibility = Visibility.Visible));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipStyle.Text = ""));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipStyle.IsEnabled = true));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewerBorder.Visibility = Visibility.Collapsed));
                                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => MultiTipSketchViewer.Source = null));
                            }
                            break;
                        default:
                            break;
                    }
                    _necContext.Dispose();
                    _nat01Context.Dispose();
                    _nat02Context.Dispose();
                    EditedTimer.Stop();
                }
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() => EnableTabletDrawingsButton()));
            }
            catch(Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => EditedTimer_Elapsed", ex.Message, user);
            }
        }
        /// <summary>
        /// Resets the timer and changes the "editedText" and "editedTextBoxName" to be used by EditedTimer_Elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomerNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (timersEnabled)
                {
                    if (!projectLinkedToQuote)
                    {
                        if (sender is TextBox textBox)
                        {
                            EditedTimer.Stop();
                            editedText = textBox.Text.ToString();
                            editedTextBoxName = textBox.Name.ToString();
                            EditedTimer.Start();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => CustomerNumber_TextChanged", ex.Message, user);
            }
        }
        /// <summary>
        /// Resets the timer and changes the "editedText" and "editedTextBoxName" to be used by EditedTimer_Elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MachineNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (timersEnabled)
            {
                if (!projectLinkedToQuote)
                {
                    if (sender is TextBox textBox)
                    {
                        EditedTimer.Stop();
                        editedText = textBox.Text.ToString();
                        editedTextBoxName = textBox.Name.ToString();
                        EditedTimer.Start();
                    }
                }
            }
        }
        /// <summary>
        /// Resets the timer and changes the "editedText" and "editedTextBoxName" to be used by EditedTimer_Elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HobOrDieNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (timersEnabled)
            {
                if (sender is TextBox textBox)
                {
                    EditedTimer.Stop();
                    editedText = textBox.Text.ToString();
                    editedTextBoxName = textBox.Name.ToString();
                    upperCupType = UpperCupType.Text.ToString();
                    upperCupDepth = UpperCupDepth.Text.ToString();
                    lowerCupType = LowerCupType.Text.ToString();
                    lowerCupDepth = LowerCupDepth.Text.ToString();
                    shortRejectCupType = ShortRejectCupType.Text.ToString();
                    shortRejectCupDepth = ShortRejectCupDepth.Text.ToString();
                    longRejectCupType = LongRejectCupType.Text.ToString();
                    longRejectCupDepth = LongRejectCupDepth.Text.ToString();
                    upperLand = UpperLand.Text.ToString();
                    lowerLand = LowerLand.Text.ToString();
                    shortRejectLand = ShortRejectLand.Text.ToString();
                    longRejectLand = LongRejectLand.Text.ToString();
                    EditedTimer.Start();
                }
            }
        }
        /// <summary>
        /// Resets the timer and changes the "editedText" and "editedTextBoxName" to be used by EditedTimer_Elapsed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SketchID_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (timersEnabled)
            {
                if (sender is TextBox textBox)
                {
                    EditedTimer.Stop();
                    editedText = textBox.Text.ToString();
                    editedTextBoxName = textBox.Name.ToString();
                    EditedTimer.Start();
                }
            }
        }
        private void LightUpBorder(TextBox textBox)
        {
            try
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    ColorAnimation colorAnimation = new ColorAnimation()
                    {
                        BeginTime = new TimeSpan(0, 0, 0),
                        Duration = new Duration(new TimeSpan(0, 0, 3)),
                        To = (textBox.BorderBrush as SolidColorBrush).Color
                    };

                    textBox.BorderBrush = new SolidColorBrush(Color.FromRgb(67, 235, 52));
                    textBox.BorderBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
                }));
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region Project Routing
        /// <summary>
        /// Changes TabletStarted or ToolStarted in Projects.EngineeringProjects to be true.
        /// Changes TabletStartedBy or ToolStarted in Projects.EngineeringProjects to the users DriveWorksPrincipalId.
        /// Changes TabletStartedDateTime or ToolStarted in Projects.EngineeringProjects to DateTime.Now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;

            IMethods.StartProject(projectNumber, projectRevNumber, CurrentProjectType.Text, user);

            RefreshRoutingButtons();
            Cursor = Cursors.Arrow;
            mainWindow.BoolValue = true;
        }
        /// <summary>
        /// Changes TabletDrawn or ToolDrawn in Projects.EngineeringProjects to be true.
        /// Changes TabletDrawnBy or ToolDrawn in Projects.EngineeringProjects to the users DriveWorksPrincipalId.
        /// Changes TabletDrawnDateTime or ToolDrawnDateTime in Projects.EngineeringProjects to DateTime.Now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FinishButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;

            IMethods.DrawProject(projectNumber, projectRevNumber, CurrentProjectType.Text, user);

            RefreshRoutingButtons();
            Cursor = Cursors.Arrow;
            mainWindow.BoolValue = true;
        }
        /// <summary>
        /// Changes TabletSubmitted or ToolSubmitted in Projects.EngineeringProjects to be true.
        /// Changes TabletSubmittedBy or ToolSubmittedBy in Projects.EngineeringProjects to the users DriveWorksPrincipalId.
        /// Changes TabletSubmittedDateTime or ToolSubmittedDateTime in Projects.EngineeringProjects to DateTime.Now.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;

            IMethods.SubmitProject(projectNumber, projectRevNumber, CurrentProjectType.Text, user);

            RefreshRoutingButtons();
            Cursor = Cursors.Arrow;
            mainWindow.BoolValue = true;
        }
        /// <summary>
        /// Checks Tablet or Tool Project in EngineeringProjects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;

            IMethods.CheckProject(projectNumber, projectRevNumber, CurrentProjectType.Text, user);

            RefreshRoutingButtons();
            Cursor = Cursors.Arrow;
            mainWindow.BoolValue = true;
            Close();
        }
        /// <summary>
        /// Puts on Hold or Takes off hold in EngineeringProjects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PutOnHoldButton_Click(object sender, RoutedEventArgs e)
        {
            if (PutOnHoldButton.Content.ToString() != "Put On Hold")
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Are you sure you want to take project " + projectNumber + " off hold?", "Take Project Off Hold?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    IMethods.TakeProjectOffHold(projectNumber, projectRevNumber);
                }
                
                if(ArchivedOrInactive.Text == "On Hold")
                {
                    ArchivedOrInactive.Text = "";
                    ArchivedOrInactive.Visibility = Visibility.Collapsed;
                }
                PutOnHoldButton.Content = "Put On Hold";
            }
            else
            {
                OnHoldCommentWindow onHoldCommentWindow = new OnHoldCommentWindow(CurrentProjectType.Text, Convert.ToInt32(projectNumber), Convert.ToInt32(projectRevNumber), mainWindow, user, true, this);
                onHoldCommentWindow.Owner = this;
                onHoldCommentWindow.ShowDialog();

            }
            RefreshRoutingButtons();
            mainWindow.BoolValue = true;
        }
        /// <summary>
        /// Cancels project in EngineeringProjects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            IMethods.CancelProject(projectNumber, projectRevNumber, user);
            mainWindow.BoolValue = true;
            Close();
        }
        #endregion

        #region Project Creation and Revision
        /// <summary>
        /// Creates new project in EngineeringProjects 
        /// or commits the revision process by
        /// updating the current Engineering Project, deleting it, adding the revised project to EngineeringProjects, then updating the EngineeringArchivedProject that was placed there by the trigger.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (FormCheck())
                {
                    Button button = sender as Button;
                    if (button.Content.ToString() == "Create")
                    {
                        try
                        {
                            saved = true;
                            using var _projectsContext = new ProjectsContext();
                            EngineeringProjects engineeringProject = GetEngineeringProjectFromCurrentForm(true);
                            _projectsContext.Update(engineeringProject);
                            if (TabletsRequired.IsChecked ?? false)
                            {
                                EngineeringTabletProjects TabletProject = GetTabletProjectFromCurrentForm();
                                if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                                {
                                    _projectsContext.Update(TabletProject);
                                }
                                else
                                {
                                    _projectsContext.Add(TabletProject);
                                }
                            }
                            if (ToolsRequired.IsChecked ?? false)
                            {
                                EngineeringToolProjects ToolProject = GetToolProjectFromCurrentForm();
                                if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                                {
                                    _projectsContext.Update(ToolProject);
                                }
                                else
                                {
                                    _projectsContext.Add(ToolProject);
                                }
                            }
                            _projectsContext.SaveChanges();
                            _projectsContext.Dispose();
                            Directory.CreateDirectory(projectsDirectory + "\\" + projectNumber + "\\" + "FILES_FOR_CUSTOMER");

                        }
                        catch (Exception ex)
                        {
                            IMethods.WriteToErrorLog("ProjectWindow => CreateButton_Click() - Create", ex.Message, user);
                            MessageBox.Show("There was an error processing this form.\r\n" + ex.Message);
                            return;
                        }
                        if (user.VisiblePanels.Contains("AllTabletProjects")) { (Application.Current.MainWindow as MainWindow).MainRefresh("AllTabletProjects"); } 
                        if (user.VisiblePanels.Contains("AllToolProjects")) { (Application.Current.MainWindow as MainWindow).MainRefresh("AllToolProjects"); }
                        Dispose();
                        Close();
                    }
                    else if (button.Content.ToString() == "Revise")
                    {
                        saved = true;
                        using var _projectsContext = new ProjectsContext();
                        EngineeringProjects oldEngineeringProject = null;
                        EngineeringArchivedProjects archivedProject = null;
                        if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            oldEngineeringProject = _projectsContext.EngineeringProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                        }
                        else if (_projectsContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            archivedProject = _projectsContext.EngineeringArchivedProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                        }
                        EngineeringProjects engineeringProject = GetEngineeringProjectFromCurrentForm(true);
                        engineeringProject.RevNumber = short.TryParse((oldEngineeringProject == null ? archivedProject.RevNumber : oldEngineeringProject.RevNumber), out short revNumber) ? (revNumber + 1).ToString() : "100";
                        engineeringProject.TimeSubmitted = (oldEngineeringProject == null ? archivedProject.TimeSubmitted : oldEngineeringProject.TimeSubmitted);
                        if (archivedProject != null)
                        {
                            engineeringProject.CSR = user.GetDWPrincipalId();
                        }
                        List<string> changed = new List<string>();
                        string body = "To Whom It May Concern,<br><br>";


                        if (oldEngineeringProject != null)
                        {
                            body += "Project: <u>" + projectNumber + "-" + projectRevNumber + "</u> Has Been Revised To <u>" + projectNumber + "-" + (Convert.ToInt16(projectRevNumber) + 1) + "</u> By <u>" + user.GetUserName() + "</u>." + "<br>" +
                            "Here are the changes made:<br><b>";
                            changed = IMethods.GetChangedProperties(oldEngineeringProject, engineeringProject);
                            if (!DateTime.Equals(oldEngineeringProject.DueDate, engineeringProject.DueDate))
                            {
                                changed.Add("DueDate" + ": " + oldEngineeringProject.DueDate.ToString("M/d/yy") + " => " + engineeringProject.DueDate.ToString("M/d/yy"));
                            }
                            foreach (string change in changed)
                            {
                                if (!change.StartsWith("RevNumber") && !change.StartsWith("Changes"))
                                {
                                    engineeringProject.Changes += change + "|";
                                    body += change + "<br>";
                                }
                            }
                        }
                        else
                        {
                            body += "Project: <u>" + projectNumber + "-" + projectRevNumber + "</u> has been revised out of archive to <u>" + projectNumber + "-" + (Convert.ToInt16(projectRevNumber) + 1) + "</u> by <u>" + user.GetUserName() + "</u>." + "<br>";
                        }

                        if (TabletsRequired.IsChecked ?? false)
                        {
                            EngineeringTabletProjects TabletProject = GetTabletProjectFromCurrentForm();
                            TabletProject.RevNumber = engineeringProject.RevNumber;
                            if (oldEngineeringProject != null && _projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == engineeringProject.ProjectNumber && p.RevNumber == (Convert.ToInt32(engineeringProject.RevNumber) - 1).ToString()))
                            {
                                EngineeringTabletProjects oldTabletProject = _projectsContext.EngineeringTabletProjects.First(p => p.ProjectNumber == engineeringProject.ProjectNumber && p.RevNumber == (Convert.ToInt32(engineeringProject.RevNumber) - 1).ToString());
                                var changedTablet = IMethods.GetChangedProperties(oldTabletProject, TabletProject);
                                foreach (string change in changedTablet)
                                {
                                    if (!change.StartsWith("RevNumber") && !change.StartsWith("Changes"))
                                    {
                                        engineeringProject.Changes += change + "|";
                                        body += change + "<br>";
                                    }
                                }
                            }
                            _projectsContext.Add(TabletProject);
                        }
                        if (ToolsRequired.IsChecked ?? false)
                        {
                            EngineeringToolProjects ToolProject = GetToolProjectFromCurrentForm();
                            ToolProject.RevNumber = engineeringProject.RevNumber;
                            if (oldEngineeringProject != null && _projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == engineeringProject.ProjectNumber && p.RevNumber == (Convert.ToInt32(engineeringProject.RevNumber) - 1).ToString()))
                            {
                                EngineeringToolProjects oldToolProject = _projectsContext.EngineeringToolProjects.First(p => p.ProjectNumber == engineeringProject.ProjectNumber && p.RevNumber == (Convert.ToInt32(engineeringProject.RevNumber) - 1).ToString());
                                var changedTool = IMethods.GetChangedProperties(oldToolProject, ToolProject);
                                foreach (string change in changedTool)
                                {
                                    if (!change.StartsWith("RevNumber") && !change.StartsWith("Changes"))
                                    {
                                        engineeringProject.Changes += change + "|";
                                        body += change + "<br>";
                                    }
                                }
                            }
                            _projectsContext.Add(ToolProject);
                        }

                        engineeringProject.RevisedBy = user.GetDWPrincipalId();
                        List<string> to = new List<string>();
                        to.Add(engineeringProject.CSR);
                        to.Add(engineeringProject.ReturnToCSR);
                        to.Add(engineeringProject.TabletStartedBy);
                        to.Add(engineeringProject.TabletDrawnBy);
                        to.Add(engineeringProject.ToolDrawnBy);
                        to.Add(engineeringProject.ToolDrawnBy);
                        to.Add(user.GetDWPrincipalId());
                        to.Distinct();
                        to.RemoveAll(s => string.IsNullOrEmpty(s));
                        string subject = "Project: " + projectNumber + "-" + projectRevNumber + " Has Been Revised";



                        if (!string.IsNullOrEmpty(engineeringProject.Changes))
                        {
                            engineeringProject.Changes = engineeringProject.Changes.TrimEnd('|');
                        }
                        body += "</b><br><br>Thanks,<br>" +
                            "Engineering Order Interface<br><br><br>" +
                            "(This E-mail is not monitored by any person(s).)";
                        _projectsContext.Add(engineeringProject);
                        if (oldEngineeringProject != null)
                        {
                            _projectsContext.Update(oldEngineeringProject);
                            _projectsContext.Remove(oldEngineeringProject);
                        }
                        _projectsContext.SaveChanges();

                        if (archivedProject == null && _projectsContext.EngineeringArchivedProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                        {
                            EngineeringArchivedProjects newEngineeringArchivedProject = _projectsContext.EngineeringArchivedProjects.First(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber);
                            newEngineeringArchivedProject.ArchivedBy = user.GetDWPrincipalId();
                            newEngineeringArchivedProject.ArchivedFromRevision = true;
                            _projectsContext.Update(newEngineeringArchivedProject);
                            _projectsContext.SaveChanges();
                        }

                        IMethods.SendEmail(to, null, new List<string> { "Tyler" }, subject, body, null, System.Net.Mail.MailPriority.High);
                        _projectsContext.Dispose();
                        if (MessageBox.Show("Project Revised!\nWould you like to close now?", "Revise Successful", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                        {
                            if (user.VisiblePanels.Contains("AllTabletProjects")) { (Application.Current.MainWindow as MainWindow).MainRefresh("AllTabletProjects"); }
                            if (user.VisiblePanels.Contains("AllToolProjects")) { (Application.Current.MainWindow as MainWindow).MainRefresh("AllToolProjects"); }
                            Dispose();
                            Close();
                        }
                        else
                        {
                            CreationBorder.Visibility = Visibility.Hidden;
                            ProjectNavigation.Visibility = Visibility.Visible;
                            CreateButton.Content = "Create";
                            RevisionDate.Text = DateTime.Now.ToString("M/d/yy h:mm tt");
                            RevisedBy.Text = user.GetDWPrincipalId();
                            projectRevNumber = (Convert.ToInt16(projectRevNumber) + 1).ToString();
                            Title = "Project# " + projectNumber + "-" + projectRevNumber;
                            AllControlsEnabledOrDisabled(false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => CreateButton_Click", ex.Message, user);
                MessageBox.Show("There was an error processing this form.\r\n" + ex.Message);
            }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var _projectsContext = new ProjectsContext();
                EngineeringProjects engineeringProject = GetEngineeringProjectFromCurrentForm(false);
                if (engineeringProject == null)
                {
                    return;
                }
                if (_projectsContext.EngineeringProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                {
                    _projectsContext.Update(engineeringProject);
                }
                else
                {
                    _projectsContext.Add(engineeringProject);
                }
                if (TabletsRequired.IsChecked ?? false)
                {
                    EngineeringTabletProjects engineeringTabletProject = GetTabletProjectFromCurrentForm();
                    if (_projectsContext.EngineeringTabletProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                    {
                        _projectsContext.EngineeringTabletProjects.Update(engineeringTabletProject);
                    }
                    else
                    {
                        _projectsContext.EngineeringTabletProjects.Add(engineeringTabletProject);
                    }
                }
                if (ToolsRequired.IsChecked ?? false)
                {
                    EngineeringToolProjects engineeringToolProject = GetToolProjectFromCurrentForm();
                    if (_projectsContext.EngineeringToolProjects.Any(p => p.ProjectNumber == projectNumber && p.RevNumber == projectRevNumber))
                    {
                        _projectsContext.EngineeringToolProjects.Update(engineeringToolProject);
                    }
                    else
                    {
                        _projectsContext.EngineeringToolProjects.Add(engineeringToolProject);
                    }
                }
                _projectsContext.SaveChanges();
                _projectsContext.Dispose();
                if (MessageBox.Show("Project Saved!\nWould you like to close now?", "Save Successful", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
                {
                    saved = true;
                    if (user.VisiblePanels.Contains("AllTabletProjects")) { (Application.Current.MainWindow as MainWindow).MainRefresh("AllTabletProjects"); }
                    if (user.VisiblePanels.Contains("AllToolProjects")) { (Application.Current.MainWindow as MainWindow).MainRefresh("AllToolProjects"); }
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error processing this form.\r\n" + ex.Message);
                IMethods.WriteToErrorLog("ProjectWindow => SaveButton_Click", ex.Message, user);
            }
        }
        /// <summary>
        /// Enables all controls to prepare for revision.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReviseButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                saved = true;
                // See CreateButton_Click for Revision Actions
                ProjectNavigation.Visibility = Visibility.Hidden;
                CreationBorder.Visibility = Visibility.Visible;
                CreateButton.Content = "Revise";
                AllControlsEnabledOrDisabled(true);
                SaveButton.IsEnabled = false;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => ReviseButton_Click", ex.Message, user);
            }
        }
        private void CancelCreateButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Navigate to Folders
        private void ProjectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            try
            {
                string path = projectsDirectory + projectNumber + @"\";
                if (!System.IO.Directory.Exists(path))
                    System.IO.Directory.CreateDirectory(path);
                System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", @"""" + path + @"""");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Cursor = Cursors.Arrow;
        }

        private void QuoteFolderButton_Click(object sender, RoutedEventArgs e)
        {
            Cursor = Cursors.AppStarting;
            try
            {
                if(quote != null)
                {
                    if(int.TryParse(QuoteNumber.Text,out int quoteNumber) && short.TryParse(QuoteRevNumber.Text, out short quoteRevNumber))
                    {
                        quote = new Quote(quoteNumber, quoteRevNumber);
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
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            Cursor = Cursors.Arrow;
        }
        #endregion

        #region Attach Files
        protected static bool GetFilename(out string filename, DragEventArgs e)
        {
            bool ret = false;
            filename = String.Empty;
            try
            {
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
                                        Microsoft.Office.Interop.Outlook.Application OL = new Microsoft.Office.Interop.Outlook.Application();
                                        for (int i = 1; i <= OL.ActiveExplorer().Selection.Count; i++)
                                        {
                                            Object temp = OL.ActiveExplorer().Selection[i];
                                            if (temp is Microsoft.Office.Interop.Outlook.MailItem)
                                            {
                                                Microsoft.Office.Interop.Outlook.MailItem mailitem = (temp as Microsoft.Office.Interop.Outlook.MailItem);
                                                Microsoft.Office.Interop.Outlook.ItemProperties props = mailitem.ItemProperties;
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
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow => GetFilename", ex.Message, App.user);
            }
            return ret;
        }
        private void AttachFilesBorder_DragEnter(object sender, DragEventArgs e)
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

        private void AttachFilesBorder_DragLeave(object sender, DragEventArgs e)
        {
            //Mouse.OverrideCursor = null;
        }

        private void AttachFilesBorder_DragOver(object sender, DragEventArgs e)
        {
        }

        private void AttachFilesBorder_Drop(object sender, DragEventArgs e)
        {
            try
            {
                string filename;
                string nameOfFile = "someFile";
                validData = GetFilename(out filename, e);
                string path = projectsDirectory + "\\" + projectNumber + "\\";
                if (filename == ".msg")
                {
                    Microsoft.Office.Interop.Outlook.Application OL = new Microsoft.Office.Interop.Outlook.Application();
                    for (int i = 1; i <= OL.ActiveExplorer().Selection.Count; i++)
                    {
                        Object temp = OL.ActiveExplorer().Selection[i];
                        if (temp is Microsoft.Office.Interop.Outlook.MailItem)
                        {
                            Microsoft.Office.Interop.Outlook.MailItem mailitem = (temp as Microsoft.Office.Interop.Outlook.MailItem);
                            string[] subjectArray = mailitem.Headers("Subject");
                            if (subjectArray.Length == 1)
                            {
                                nameOfFile = subjectArray[0];
                            }
                            else
                            {
                                while (System.IO.File.Exists(path + nameOfFile + ".msg"))
                                {
                                    InputBox dialog = new InputBox("Enter name of file:", "File Name", this);
                                    dialog.ShowDialog();
                                    nameOfFile = dialog.ReturnString.Length > 0 ? dialog.ReturnString : nameOfFile;
                                }
                            }
                            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + "'";
                            foreach (char c in invalid)
                            {
                                nameOfFile = nameOfFile.Replace(c.ToString(), "");
                            }
                            string newFileName = nameOfFile;
                            foreach (char c in nameOfFile)
                            {
                                if (!char.IsLetterOrDigit(c) || c == '-')
                                {
                                    newFileName = nameOfFile.Replace(c.ToString(), "_");
                                }
                            }
                            mailitem.SaveAs(path + newFileName + ".msg");
                            //MessageBox.Show(this, "Successful drop.");
                        }
                    }
                }
                else
                {
                    string fp = path + nameOfFile + (System.IO.Path.GetExtension(filename).ToLower().StartsWith(".xl") ? ".xlsx" : System.IO.Path.GetExtension(filename));
                    string newFileName = Path.GetFileNameWithoutExtension(filename);

                    string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars()) + "'";
                    foreach (char c in invalid)
                    {
                        newFileName = newFileName.Replace(c.ToString(), "");
                    }
                    string _newFileName = newFileName;
                    foreach (char c in newFileName)
                    {
                        if (!char.IsLetterOrDigit(c) || c == '-')
                        {
                            _newFileName = newFileName.Replace(c.ToString(), "_");
                        }
                    }
                    fp = path + Path.GetFileName(_newFileName) + Path.GetExtension(filename).ToLower();
                    System.IO.File.Copy(filename, fp);
                    if (filename.Contains(@"\Local\Temp")) { File.Delete(filename); }
                    //MessageBox.Show(this, "Successful drop.");
                }
                ProjectFiles = GetProjectFiles(projectNumber);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("ProjectWindow.xaml.cs => AttachFilesBorder_Drop()", ex.Message, user);
            }
        }

        private void file_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                ListBox listBox = sender as ListBox;
                if (listBox.IsMouseCaptured)
                {
                    Tuple<string, string, string> file = projectFiles[listBox.SelectedIndex];
                    string fullFilePath = "\"" + file.Item2 + "\\" + file.Item1 + file.Item3 + "\"";
                    System.Diagnostics.Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", fullFilePath);
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open the file. See error below." + System.Environment.NewLine + System.Environment.NewLine + ex.Message);
            }
        }
        private void file_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == System.Windows.Input.Key.Delete)
                {
                    ListBox listBox = sender as ListBox;
                    Tuple<string, string, string> file = projectFiles[listBox.SelectedIndex];
                    string fullFilePath = file.Item2 + "\\" + file.Item1 + file.Item3;
                    File.Delete(fullFilePath);
                }
                ProjectFiles = GetProjectFiles(projectNumber);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete the file. See error below." + System.Environment.NewLine + System.Environment.NewLine + ex.Message);
            }
        }
        #endregion

        #endregion

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    EditedTimer.Dispose();
                    // mainWindow.Dispose(); This disposes the refresh timers I think.
                    // TODO: dispose managed state (managed objects).
                }
                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProjectWindow()
        // {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            GC.SuppressFinalize(this);
        }


        #endregion

        private void SetProjectWarningText()
        {
            #region ProjectsWarning
            if (DueDate.SelectedValue.ToString().Length > 0 && (TabletsRequired.IsChecked == true || ToolsRequired.IsChecked == true))
            {
                string message = "";
                DateTime dueDate = Convert.ToDateTime(DueDate.SelectedValue.ToString().Split("|")[1].Trim());
                dueDate = dueDate.AddHours(15.5);
                TimeSpan timeSpan = dueDate - DateTime.Now;
                double totalDays = timeSpan.TotalDays;
                int weeks = (int)(totalDays / 7);
                int weekendDays = weeks * 2;
                if (6 - (int)DateTime.Today.DayOfWeek < totalDays - weeks * 7)
                {
                    weekendDays += 2;
                }
                double workDays = totalDays - weekendDays;
                using var _nat02Context = new NAT02Context();
                if (TabletsRequired.IsChecked == true)
                {
                    int projectsDueInTimeSpan = _nat02Context.EoiAllTabletProjectsView.Count(p => p.Complete < 4 && p.DueDate <= dueDate);
                    int projectsDueOnThatDay = _nat02Context.EoiAllTabletProjectsView.Count(p => p.Complete < 4 && p.DueDate == dueDate);
                    double tabletProjectsPerDay = Math.Round(projectsDueInTimeSpan / workDays, 1);
                    message += tabletProjectsPerDay + " tablet projects due per day until due date. " + projectsDueOnThatDay + " tablet projects due on that day.";
                    if(tabletProjectsPerDay>20)
                    {
                        DueDateWarning.Foreground = Brushes.Red;
                    }
                }
                if (ToolsRequired.IsChecked == true)
                {
                    int projectsDueInTimeSpan = _nat02Context.EoiAllToolProjectsView.Count(p => p.Complete < 5 && p.DueDate <= dueDate);
                    int projectsDueOnThatDay = _nat02Context.EoiAllToolProjectsView.Count(p => p.Complete < 5 && p.DueDate == dueDate);
                    double toolprojectsPerDay = Math.Round(projectsDueInTimeSpan / workDays, 1);
                    string messageAddition = toolprojectsPerDay + " tool projects due per day until due date. " + projectsDueOnThatDay + " tool projects due on that day.";
                    if (message == "")
                    {
                        message += messageAddition;
                    }
                    else
                    {
                        message += System.Environment.NewLine + messageAddition;

                    }
                    if (toolprojectsPerDay > 10)
                    {
                        DueDateWarning.Foreground = Brushes.Red;
                    }
                }
                DueDateWarning.Text = message;
                DueDateWarning.Visibility = Visibility.Visible;
            }
            else
            {
                DueDateWarning.Text = "";
                DueDateWarning.Visibility = Visibility.Hidden;
                DueDateWarning.Foreground = Application.Current.Resources["Dark"] as SolidColorBrush;
            }
            #endregion
        }
    }
}
