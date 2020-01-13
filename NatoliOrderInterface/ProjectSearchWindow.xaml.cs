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
        private readonly Timer timer = new Timer(300);

        public ProjectSearchWindow()
        {
            InitializeComponent();
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(() => GetProjectInfo(false)).ContinueWith(t => Dispatcher.Invoke(() => BindProjectInfo()), TaskScheduler.Current);
        }

        private void GetProjectInfo(bool filter, string searchString = "")
        {
            try
            {
                List<EngineeringArchivedProjects> engineeringArchivedProjects;

                using var _context = new ProjectsContext();
                if (filter)
                {
                    if (searchString.Contains(":"))
                    {
                        var column = searchString.Split(':');
                        string colSearch = column[1].Trim().Split(" ")[0];
                        string remainder = column[1].Trim().Split(" ")[1] ?? "";
                        switch (column[0])
                        {
                            case "csr":
                                engineeringArchivedProjects = _context.EngineeringArchivedProjects.Where(o => o.CSR.ToLower().Contains(colSearch))
                                                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                                                   .Take(150)
                                                                                                   .ToList();
                                break;
                            case "customer name":
                                engineeringArchivedProjects = _context.EngineeringArchivedProjects.Where(o => o.CustomerName.ToLower().Contains(colSearch) ||
                                                                                                              o.EndUserName.ToLower().Contains(colSearch))
                                                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                                                   .Take(150)
                                                                                                   .ToList();
                                break;
                            case "product":
                                engineeringArchivedProjects = _context.EngineeringArchivedProjects.Where(o => o.Product.ToLower().Contains(colSearch))
                                                                                                   .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                                                   .Take(150)
                                                                                                   .ToList();
                                break;
                            default:
                                engineeringArchivedProjects = _context.EngineeringArchivedProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                                                  .Take(150)
                                                                                                  .ToList();
                                break;
                        }
                        engineeringArchivedProjects = engineeringArchivedProjects.Where(o => o.ArchivedBy.ToLower().Contains(remainder) ||
                                                                                             o.Attention.ToLower().Contains(remainder) ||
                                                                                             o.CSR.ToLower().Contains(remainder) ||
                                                                                             o.CustomerName.ToLower().Contains(remainder) ||
                                                                                             //o.CustomerNumber.ToString().Contains(remainder) ||
                                                                                             //o.DieNumber.ToString().Contains(remainder) ||
                                                                                             o.DieShape.Contains(remainder) ||
                                                                                             //o.DueDate.ToString().Contains(remainder) ||
                                                                                             o.EndUserName.Contains(remainder) ||
                                                                                             o.UpperHobDescription.Contains(remainder) ||
                                                                                             o.LowerHobDescription.Contains(remainder) ||
                                                                                             o.Notes.Contains(remainder) ||
                                                                                             o.Product.Contains(remainder))
                                                                                 .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                                 .Take(150)
                                                                                 .ToList();
                    }
                    else
                    {
                        engineeringArchivedProjects = _context.EngineeringArchivedProjects.Where(o => o.ArchivedBy.ToLower().Contains(searchString) ||
                                                                                                      o.Attention.ToLower().Contains(searchString) ||
                                                                                                      o.CSR.ToLower().Contains(searchString) ||
                                                                                                      o.CustomerName.ToLower().Contains(searchString) ||
                                                                                                      //o.CustomerNumber.ToString().Contains(searchString) ||
                                                                                                      //o.DieNumber.ToString().Contains(searchString) ||
                                                                                                      o.DieShape.Contains(searchString) ||
                                                                                                      //o.DueDate.ToString().Contains(searchString) ||
                                                                                                      o.EndUserName.Contains(searchString) ||
                                                                                                      o.UpperHobDescription.Contains(searchString) ||
                                                                                                      o.LowerHobDescription.Contains(searchString) ||
                                                                                                      o.Notes.Contains(searchString) ||
                                                                                                      o.Product.Contains(searchString))
                                                                                           .OrderByDescending(p => Convert.ToInt32(p.ProjectNumber.Trim()))
                                                                                           .Take(150)
                                                                                           .ToList();
                    }
                }
                else
                {
                    engineeringArchivedProjects = _context.EngineeringArchivedProjects.OrderByDescending(p => Convert.ToInt32(p.ProjectNumber))
                                                                                      .Take(150)
                                                                                      .ToList();
                }
                
                _context.Dispose();

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
        private void BindProjectInfo()
        {
            ProjectExpanders(archivedProjectsDict);
        }

        private void ProjectExpanders(Dictionary<EngineeringArchivedProjects, (string background, string foreground, string fontWeight)> dict)
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
            //if (dict.Keys.Count > 16)
            //{
            //    if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 6)
            //    {
            //        Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
            //        AddColumn(headerGrid, CreateColumnDefinition(new GridLength(22)));
            //    }
            //}
            //else
            //{
            //    if ((dockPanel.Children.OfType<Border>().First().Child as Grid).ColumnDefinitions.Count == 7)
            //    {
            //        Grid headerGrid = dockPanel.Children.OfType<Border>().First().Child as Grid;
            //        headerGrid.ColumnDefinitions.RemoveAt(headerGrid.ColumnDefinitions.Count - 1);
            //    }
            //}

            //dockPanel.Children.OfType<Grid>().First().Children.OfType<Label>().First().Content = headers.Where(kvp => kvp.Key == "BeingEntered").First().Value;
            //dockPanel.Children.OfType<Label>().First(l => l.Name == "TotalLabel").Content = "Total: " + dict.Count;
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
            timer.Start();
            TextBox textBox = sender as TextBox;
            string searchString = textBox.Text.ToLower();

            Task.Run(() => GetProjectInfo(true, searchString)).ContinueWith(t => Dispatcher.Invoke(() => BindProjectInfo()), TaskScheduler.Current);

            // Filter databased on text entry
            //var _filtered =
            //    archivedProjectsDict.Where(o => o.Key.ArchivedBy.ToLower().Contains(searchString) ||
            //                                    o.Key.Attention.ToLower().Contains(searchString) ||
            //                                    o.Key.CSR.ToLower().Contains(searchString) ||
            //                                    o.Key.CustomerName.ToLower().Contains(searchString) ||
            //                                    o.Key.CustomerNumber.ToString().Contains(searchString) ||
            //                                    o.Key.DieNumber.ToString().Contains(searchString) ||
            //                                    o.Key.DieShape.Contains(searchString) ||
            //                                    o.Key.DueDate.ToString().Contains(searchString) ||
            //                                    o.Key.EndUserName.Contains(searchString))
            //                        .OrderByDescending(p => Convert.ToInt32(p.Key.ProjectNumber))
            //                        .ToDictionary(x => x.Key, x => x.Value);

            // Remove/Add expanders based on filtering
            //ProjectExpanders(_filtered);
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
    }
}
