using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.EntityFrameworkCore;
using NatoliOrderInterface.Models.Projects;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for ProjectSearchWindow.xaml
    /// </summary>
    public partial class ProjectSearchWindow : Window, IDisposable
    {
        #region All Projects
        private ListBox AllProjectsListBox = new ListBox();
        private List<AllProjectsView> allProjects = new List<AllProjectsView>();
        private List<AllProjectsView> _allProjects = new List<AllProjectsView>();
        public List<AllProjectsView> AllProjects
        {
            get
            {
                return allProjects;
            }
            set
            {
                if (!allProjects.SequenceEqual(value))
                {
                    allProjects = value;
                    AllProjectsListBox.ItemsSource = null;
                    AllProjectsListBox.ItemsSource = allProjects;
                }
            }
        }
        #endregion
        private Dictionary<AllProjectsView, (string background, string foreground, string fontWeight)> allProjectsDict;
        private readonly Timer timer = new Timer(400);
        private string searchString;
        private string prevSearchString;

        public ProjectSearchWindow()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
            this.WindowState = WindowState.Maximized;
        }
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Task.Run(() => GetProjectInfo(false)).ContinueWith(t => Dispatcher.Invoke(() => BindProjectInfo()), TaskScheduler.Current);
            AllProjectsListBox = (VisualTreeHelper.GetChild(MainLabel as DependencyObject, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();

            Task.Run(() => GetAllProjects()).ContinueWith(t => Dispatcher.BeginInvoke((Action)(() => BindAllProjects()), System.Windows.Threading.DispatcherPriority.ApplicationIdle));

            AllProjectsListBox.ItemsSource = null;
            AllProjectsListBox.ItemsSource = allProjects;
        }
        private void GetAllProjects()
        {
            try
            {
                using var _context = new ProjectsContext();
                List<AllProjectsView> allProjectsView = new List<AllProjectsView>();

                _allProjects = _context.AllProjectsView.FromSqlRaw("SELECT TOP 100 PERCENT * FROM " +
                                    "(SELECT CAST(ProjectNumber AS INT) AS ProjectNumber, CAST(RevisionNumber AS VARCHAR(10)) AS RevNumber, '' AS QuoteNumber, CSR, ReturnToCSR, CustomerName, EndUser AS EndUserName, Product, " +
                                    "'' AS DieNumber, Shape AS DieShape, UpperHobNumber, LowerHobNumber, '' AS ShortRejectHobNumber, '' AS LongRejectHobNumber, DueDate " +
                                    "FROM Projects.dbo.ProjectSpecSheet PSS UNION " +
                                    "SELECT CAST(ProjectNumber AS INT), RevNumber, QuoteNumber, CSR, ReturnToCSR, CustomerName, EndUserName, Product, " +
                                    "DieNumber, DieShape, UpperHobNumber, LowerHobNumber, ShortRejectHobNumber, LongRejectHobNumber, DueDate " +
                                    "FROM Projects.dbo.EngineeringArchivedProjects EAP " +
                                    "WHERE DueDate <> '9999-12-31' UNION " +
                                    "SELECT CAST(ProjectNumber AS INT), RevNumber, QuoteNumber, CSR, ReturnToCSR, CustomerName, EndUserName, Product, " +
                                    "DieNumber, DieShape, UpperHobNumber, LowerHobNumber, ShortRejectHobNumber, LongRejectHobNumber, DueDate " +
                                    "FROM Projects.dbo.EngineeringProjects EP " +
                                    "WHERE DueDate <> '9999-12-31') AS AllProjectsView ORDER BY ProjectNumber DESC").ToList();

                _context.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void BindAllProjects()
        {
            string searchString = SearchBox.Text.ToLower();

            string column;
            var _filtered = _allProjects;
            _filtered =
                    _allProjects.Where(p => p.ProjectNumber.ToString().ToLower().Contains(searchString) ||
                                            p.RevNumber.ToString().ToLower().Contains(searchString) ||
                                            (!string.IsNullOrEmpty(p.QuoteNumber) && p.QuoteNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.CSR) && p.CSR.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.ReturnToCSR) && p.ReturnToCSR.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.CustomerName) && p.CustomerName.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.EndUserName) && p.EndUserName.ToLower().Contains(searchString)) ||
                                            (p.DueDate.ToShortDateString().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.QuoteNumber) && p.QuoteNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.DieNumber) && p.DieNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.DieShape) && p.DieShape.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.UpperHobNumber) && p.UpperHobNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.LowerHobNumber) && p.LowerHobNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.ShortRejectHobNumber) && p.ShortRejectHobNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.LongRejectHobNumber) && p.LongRejectHobNumber.ToLower().Contains(searchString)) ||
                                            (!string.IsNullOrEmpty(p.Product) && p.Product.ToLower().Contains(searchString)))
                                      .OrderByDescending(kvp => kvp.ProjectNumber)
                                      .ToList();

            Grid headerGrid = GetHeaderGridFromListBox(AllProjectsListBox);
            foreach (ToggleButton tb in headerGrid.Children.OfType<ToggleButton>().Where(tb => !(tb is CheckBox) && tb.IsChecked != null))
            {
                if (tb.IsChecked == false)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Proj #":
                            _filtered = _filtered
                                .OrderBy(o => o.ProjectNumber)
                                .ToList();
                            break;
                        case "CSR":
                            _filtered = _filtered
                                .OrderBy(o => o.CSR)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderBy(o => o.CustomerName)
                                .ToList();
                            break;
                        case "End User Name":
                            _filtered = _filtered
                                .OrderBy(o => o.EndUserName)
                                .ToList();
                            break;
                        case "Due Date":
                            _filtered = _filtered
                                .OrderBy(o => o.DueDate)
                                .ToList();
                            break;
                        case "Quote #":
                            _filtered = _filtered
                                .OrderBy(o => o.QuoteNumber)
                                .ToList();
                            break;
                        case "Product":
                            _filtered = _filtered
                                .OrderBy(o => o.Product)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
                else if (tb.IsChecked == true)
                {
                    switch (tb.Tag.ToString())
                    {
                        case "Proj #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.ProjectNumber)
                                .ToList();
                            break;
                        case "CSR":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CSR)
                                .ToList();
                            break;
                        case "Customer Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.CustomerName)
                                .ToList();
                            break;
                        case "End User Name":
                            _filtered = _filtered
                                .OrderByDescending(o => o.EndUserName)
                                .ToList();
                            break;
                        case "Due Date":
                            _filtered = _filtered
                                .OrderByDescending(o => o.DueDate)
                                .ToList();
                            break;
                        case "Quote #":
                            _filtered = _filtered
                                .OrderByDescending(o => o.QuoteNumber)
                                .ToList();
                            break;
                        case "Product":
                            _filtered = _filtered
                                .OrderByDescending(o => o.Product)
                                .ToList();
                            break;
                        default:
                            break;
                    }
                }
            }
            AllProjects = _filtered;
        }
        private Grid GetHeaderGridFromListBox(ListBox listBox)
        {
            Grid displayGrid = listBox.Parent as Grid;
            return displayGrid.Children.OfType<Grid>().First(g => g.Name == "HeaderGrid");
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            timer.Stop();

            TextBox textBox = sender as TextBox; // (sender as TextBox).Template.FindName("SearchBox", sender as TextBox) as TextBox;
            //TextBlock textBlock = (sender as TextBox).Template.FindName("SearchTextBlock", sender as TextBox) as TextBlock;
            //Image image = (sender as TextBox).Template.FindName("MagImage", (sender as TextBox)) as Image;
            searchString = textBox.Text.ToLower();

            //if (textBox.Text.Length > 0)
            //{
            //    image.Source = ((Image)App.Current.Resources["closeDrawingImage"]).Source;
            //    image.MouseLeftButtonUp += Image_MouseLeftButtonUp;
            //    textBlock.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    image.Source = ((Image)App.Current.Resources["searchDrawingImage"]).Source;
            //    textBlock.Visibility = Visibility.Visible;
            //}

            timer.Start();
        }
        private void SearchBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                TextBox textBox = (sender as TextBox).Template.FindName("SearchTextBox", sender as TextBox) as TextBox;
                textBox.Text = "";
            }
        }
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                BindAllProjects();
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);

            //if (searchString != prevSearchString)
            //{
            //    Task.Run(() => GetProjectInfo(true, searchString)).ContinueWith(t => Dispatcher.Invoke(() => BindProjectInfo()), TaskScheduler.Current);
            //    prevSearchString = searchString;
            //}
        }
        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ProjectSearchWindow()
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
            // GC.SuppressFinalize(this);
        }
        #endregion
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
