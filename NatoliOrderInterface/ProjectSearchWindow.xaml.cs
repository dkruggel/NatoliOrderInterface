using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
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
        private Dictionary<EngineeringArchivedProjects, (string background, string foreground, string fontWeight)> archivedProjectsDict;
        private readonly Timer timer = new Timer(400);
        private string searchString;
        private string prevSearchString;

        public ProjectSearchWindow()
        {
            InitializeComponent();
            timer.Elapsed += Timer_Elapsed;
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => GetProjectInfo(false)).ContinueWith(t => Dispatcher.Invoke(() => BindProjectInfo()), TaskScheduler.Current);
        }

        private void GetProjectInfo(bool filter, string search = "")
        {
            try
            {
                List<EngineeringArchivedProjects> engineeringArchivedProjects = new List<EngineeringArchivedProjects>();

                string[] searchStringsAnd = search.Split('&');
                string[] searchStringsOr = search.Split('|');

                if (searchStringsAnd.Length > 1)
                {
                    bool x = false;
                    foreach (string searchPart in searchStringsAnd)
                    {
                        if (!x)
                        {
                            engineeringArchivedProjects = ProjectSearch(searchPart, 500);
                            x = !x;
                        }
                        else
                        {
                            var temp = ProjectSearch(searchPart).Select(p => (p.ProjectNumber, p.RevNumber));
                            var exist = engineeringArchivedProjects.Select(p => (p.ProjectNumber, p.RevNumber)).Intersect(temp);
                            engineeringArchivedProjects.RemoveAll(p => !exist.Contains((p.ProjectNumber, p.RevNumber)));
                        }
                    }
                }
                else if (searchStringsOr.Length > 1)
                {
                    bool x = false;
                    foreach (string searchPart in searchStringsOr)
                    {
                        if (!x)
                        {
                            engineeringArchivedProjects = ProjectSearch(searchPart, 1500);
                            x = !x;
                        }
                        else
                        {
                            // engineeringArchivedProjects = engineeringArchivedProjects.Union(ProjectSearch(searchPart)).ToList();
                            var temp = ProjectSearch(searchPart);
                            engineeringArchivedProjects.AddRange(temp);
                        }
                    }
                }
                else
                {
                    if (filter)
                    {
                        engineeringArchivedProjects = ProjectSearch(search);
                    }
                    else
                    {
                        using var _context = new ProjectsContext();
                        engineeringArchivedProjects = _context.EngineeringArchivedProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber))
                                                                                          .Take(150)
                                                                                          .ToList();
                        _context.Dispose();
                    }
                }

                archivedProjectsDict = new Dictionary<EngineeringArchivedProjects, (string background, string foreground, string fontWeight)>();

                foreach (EngineeringArchivedProjects project in engineeringArchivedProjects)
                {
                    SolidColorBrush back;
                    SolidColorBrush fore;
                    FontWeight weight;
                    if (project.Priority)
                    {
                        fore = new SolidColorBrush(Colors.DarkRed);
                        weight = FontWeights.ExtraBold;
                    }
                    else
                    {
                        fore = new SolidColorBrush(Colors.Black);
                        weight = FontWeights.Normal;
                    }

                    back = (SolidColorBrush)new BrushConverter().ConvertFrom("#FFFFFFFF");

                    archivedProjectsDict.Add(project, (back.Color.ToString(), fore.Color.ToString(), weight.ToString()));
                }

                engineeringArchivedProjects.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private static List<EngineeringArchivedProjects> ProjectSearch(string searchString, int length = 150)
        {
            List<EngineeringArchivedProjects> res = new List<EngineeringArchivedProjects>();
            using var _context = new ProjectsContext();
            
            if (searchString.Contains(":"))
            {
                var column = searchString.Split(':');
                string colSearch = column[1];

                switch (column[0])
                {
                    case "attention":
                        res = _context.EngineeringArchivedProjects.Where(o => o.Attention.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "csr":
                        res = _context.EngineeringArchivedProjects.Where(o => o.CSR.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "customer name":
                        res = _context.EngineeringArchivedProjects.Where(o => o.CustomerName.ToLower().Contains(colSearch) ||
                                                                               o.EndUserName.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "end user name":
                        goto case "customer name";
                    case "product":
                        res = _context.EngineeringArchivedProjects.Where(o => o.Product.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "project #":
                        res = _context.EngineeringArchivedProjects.Where(o => o.ProjectNumber.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "quote #":
                        res = _context.EngineeringArchivedProjects.Where(o => o.QuoteNumber.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "machine":
                        res = _context.EngineeringArchivedProjects.Where(o => o.MachineNumber.ToLower().Contains(colSearch))
                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                    case "due date":
                        if (!colSearch.Contains("/"))
                        {
                            res = _context.EngineeringArchivedProjects.Where(o => o.DueDate.Day.ToString().Contains(colSearch) ||
                                                                                   o.DueDate.Month.ToString().Contains(colSearch) ||
                                                                                   o.DueDate.Year.ToString().Contains(colSearch))
                                                                       .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                       .Take(length)
                                                                       .ToList();
                        }
                        else if (colSearch.Count(x => x == '/') == 1)
                        {
                            string[] dateParts = colSearch.Split('/');
                            res = _context.EngineeringArchivedProjects.Where(o => ((o.DueDate.Month.ToString() == dateParts[0]) && (o.DueDate.Day.ToString() == dateParts[1])) ||
                                                                                   ((o.DueDate.Day.ToString() == dateParts[0]) && (o.DueDate.Year.ToString() == dateParts[1])))
                                                                       .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                       .Take(length)
                                                                       .ToList();
                        }
                        else
                        {
                            res = _context.EngineeringArchivedProjects.Where(o => o.DueDate.ToShortDateString().Contains(colSearch))
                                                                       .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                       .Take(length)
                                                                       .ToList();
                        }
                        break;
                    default:
                        res = _context.EngineeringArchivedProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                   .Take(length)
                                                                   .ToList();
                        break;
                }
            }
            else
            {
                res = _context.EngineeringArchivedProjects.Where(o => o.ArchivedBy.ToLower().Contains(searchString) ||
                                                                       o.Attention.ToLower().Contains(searchString) ||
                                                                       o.CSR.ToLower().Contains(searchString) ||
                                                                       o.CustomerName.ToLower().Contains(searchString) ||
                                                                       //o.CustomerNumber.ToString().Contains(searchString) ||
                                                                       //o.DieNumber.ToString().Contains(searchString) ||
                                                                       o.DieShape.Contains(searchString) ||
                                                                       o.EndUserName.Contains(searchString) ||
                                                                       o.UpperHobDescription.Contains(searchString) ||
                                                                       o.LowerHobDescription.Contains(searchString) ||
                                                                       o.Notes.Contains(searchString) ||
                                                                       o.Product.Contains(searchString))
                                                           .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                           .Take(length)
                                                           .ToList();
            }

            _context.Dispose();

            return res;
        }
        private void BindProjectInfo()
        {
            ProjectExpanders(archivedProjectsDict);
        }

        private void ProjectExpanders(Dictionary<EngineeringArchivedProjects, (string background, string foreground, string fontWeight)> dict)
        {
            try
            {
                ProjectsStackPanel.Children.Clear();
                IEnumerable<(int, int)> projects = ProjectsStackPanel.Children.OfType<Expander>().Select(e => (int.Parse((e.Header as Grid).Children[0].GetValue(ContentProperty).ToString()),
                                                                                                               int.Parse((e.Header as Grid).Children[1].GetValue(ContentProperty).ToString())));

                IEnumerable<(int, int)> newProjects = dict.Select(kvp => (Convert.ToInt32(kvp.Key.ProjectNumber), Convert.ToInt32(kvp.Key.RevNumber)))
                                                          .Except(projects.Select(p => (p.Item1, p.Item2)));

                foreach ((int, int) project in newProjects)
                {
                    int index = dict.Keys.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber)).ToList().IndexOf(dict.First(kvp => Convert.ToInt32(kvp.Key.ProjectNumber) == project.Item1).Key);
                    Expander expander = CreateProjectExpander(dict.First(x => Convert.ToInt32(x.Key.ProjectNumber) == project.Item1));
                    Dispatcher.Invoke(() => ProjectsStackPanel.Children.Insert(index, expander));
                }

                List<Expander> removeThese = new List<Expander>();
                foreach (Expander expander in ProjectsStackPanel.Children.OfType<Expander>())
                {
                    string _project = ((Grid)expander.Header).Children[0].GetValue(ContentProperty) as string;
                    if (!dict.Any(kvp => kvp.Key.ProjectNumber == _project))
                    {
                        removeThese.Add(expander);
                        continue;
                    }
                    Dispatcher.Invoke(() =>
                    expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(dict.First(kvp => kvp.Key.ProjectNumber == _project).Value.background)));
                }
                foreach (Expander expander1 in removeThese) { ProjectsStackPanel.Children.Remove(expander1); }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private Expander CreateProjectExpander(KeyValuePair<EngineeringArchivedProjects, (string background, string foreground, string fontWeight)> kvp)
        {
            Grid grid = new Grid
            {
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            SolidColorBrush foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.foreground));
            FontWeight fontWeight = (FontWeight)new FontWeightConverter().ConvertFromString(kvp.Value.fontWeight);
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.ProjectNumber, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(45)), CreateLabel(kvp.Key.RevNumber, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(100)), CreateLabel(kvp.Key.CSR, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(300)), CreateLabel(kvp.Key.CustomerName, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(300)), CreateLabel(kvp.Key.EndUserName, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.DueDate.ToShortDateString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.QuoteNumber, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.QuoteRevNumber, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(150)), CreateLabel(kvp.Key.Product, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(150)), CreateLabel(kvp.Key.Attention, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(60)), CreateLabel(kvp.Key.MachineNumber, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.UpperHobNumber.ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(250)), CreateLabel(kvp.Key.UpperHobDescription.ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.LowerHobNumber.ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(250)), CreateLabel(kvp.Key.LowerHobDescription.ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.DieNumber.ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(kvp.Key.DieShape.ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(Decimal.Round((decimal)kvp.Key.Width, 4).ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(75)), CreateLabel(Decimal.Round((decimal)kvp.Key.Length, 4).ToString(), 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));
            AddColumn(grid, CreateColumnDefinition(new GridLength(500)), CreateLabel(kvp.Key.Notes, 0, grid.ColumnDefinitions.Count, fontWeight, foreground, null, 14, true));

            Expander expander = new Expander()
            {
                IsExpanded = false,
                Header = grid,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                BorderBrush = new SolidColorBrush(Colors.Black)
            };

            expander.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(kvp.Value.background));
            //expander.MouseDoubleClick += OrderDataGrid_MouseDoubleClick;
            //expander.PreviewKeyDown += OrderDataGrid_PreviewKeyDown;
            //expander.PreviewMouseDown += OrderDataGrid_PreviewMouseDown;
            //expander.MouseRightButtonUp += OrderDataGrid_MouseRightButtonUp;
            return expander;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="columnDefinition"></param>
        /// <param name="label"></param>
        private static void AddColumn(Grid grid, ColumnDefinition columnDefinition = null, UIElement element = null)
        {
            try
            {
                grid.ColumnDefinitions.Add(columnDefinition);
                if (!(element is null)) { grid.Children.Add(element); };
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="width"></param>
        /// <returns></returns>
        private ColumnDefinition CreateColumnDefinition(GridLength width)
        {
            ColumnDefinition colDef = new ColumnDefinition();
            try
            {
                colDef.Width = width;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return colDef;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private Label CreateLabel(string content, int row, int column, FontWeight weight, SolidColorBrush textColor = null, FontStyle? fontStyle = null, double fontSize = 12, bool addPadding = false)
        {
            Label label = new Label();
            try
            {
                label.Content = content;
                label.HorizontalAlignment = HorizontalAlignment.Stretch;
                label.HorizontalContentAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Top;
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.FontSize = fontSize;
                if (addPadding) { label.Padding = new Thickness(0, 0, 0, 0); }
                if (!(textColor is null))
                {
                    label.Foreground = textColor;
                }
                label.FontWeight = weight;
                label.FontStyle = !(fontStyle is null) ? (FontStyle)fontStyle : FontStyles.Normal;
                Grid.SetRow(label, row);
                Grid.SetColumn(label, column);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return label;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            timer.Stop();
            TextBox textBox = sender as TextBox;
            searchString = textBox.Text.ToLower();
            timer.Start();
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (searchString != prevSearchString)
            {
                Task.Run(() => GetProjectInfo(true, searchString)).ContinueWith(t => Dispatcher.Invoke(() => BindProjectInfo()), TaskScheduler.Current);
                prevSearchString = searchString;
            }
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

        private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string text = SearchBox.Text.Contains(':') ? SearchBox.Text.Split(':')[1] : SearchBox.Text ;
            SearchBox.Text = (sender as Label).Content.ToString() + ':' + text;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            timer.Dispose();
        }
    }
}
