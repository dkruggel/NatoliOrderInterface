using NatoliOrderInterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>")]
    /// <summary>
    /// Interaction logic for EditLayoutWindow.xaml
    /// </summary>
    public partial class EditLayoutWindow : Window
    {
        private readonly MainWindow parent;
        private readonly User user;
        private readonly List<string> possiblePanels = new List<string>();
        private readonly List<string> visiblePanels = new List<string>();

        public EditLayoutWindow(User usr, MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            user = usr;
            visiblePanels = user.VisiblePanels.ToList();
            if (user.EmployeeCode == "E4408" || user.EmployeeCode == "E4754" || user.EmployeeCode == "E4509" || user.EmployeeCode == "E3072")
            {
                possiblePanels.Add("QuotesNotConverted");
                possiblePanels.Add("QuotesToConvert");
                possiblePanels.Add("BeingEntered");
                possiblePanels.Add("InTheOffice");
                possiblePanels.Add("EnteredUnscanned");
                possiblePanels.Add("InEngineering");
                possiblePanels.Add("ReadyToPrint");
                possiblePanels.Add("PrintedInEngineering");
                possiblePanels.Add("AllTabletProjects");
                possiblePanels.Add("TabletProjectsNotStarted");
                possiblePanels.Add("TabletProjectsStarted");
                possiblePanels.Add("TabletProjectsDrawn");
                possiblePanels.Add("TabletProjectsSubmitted");
                possiblePanels.Add("TabletProjectsOnHold");
                possiblePanels.Add("AllToolProjects");
                possiblePanels.Add("ToolProjectsNotStarted");
                possiblePanels.Add("ToolProjectsStarted");
                possiblePanels.Add("ToolProjectsDrawn");
                possiblePanels.Add("ToolProjectsOnHold");
                possiblePanels.Add("DriveWorksQueue");
                possiblePanels.Add("NatoliOrderList");
                possiblePanels.Add("");
            }
            else if (user.Department == "Engineering")
            {
                possiblePanels.Add("QuotesNotConverted");
                possiblePanels.Add("BeingEntered");
                possiblePanels.Add("InTheOffice");
                possiblePanels.Add("EnteredUnscanned");
                possiblePanels.Add("InEngineering");
                possiblePanels.Add("ReadyToPrint");
                possiblePanels.Add("PrintedInEngineering");
                possiblePanels.Add("AllTabletProjects");
                possiblePanels.Add("TabletProjectsNotStarted");
                possiblePanels.Add("TabletProjectsStarted");
                possiblePanels.Add("TabletProjectsDrawn");
                possiblePanels.Add("TabletProjectsSubmitted");
                possiblePanels.Add("TabletProjectsOnHold");
                possiblePanels.Add("AllToolProjects");
                possiblePanels.Add("ToolProjectsNotStarted");
                possiblePanels.Add("ToolProjectsStarted");
                possiblePanels.Add("ToolProjectsDrawn");
                possiblePanels.Add("ToolProjectsOnHold");
                possiblePanels.Add("DriveWorksQueue");
                possiblePanels.Add("");
            }
            else if (user.Department == "Customer Service")
            {
                possiblePanels.Add("QuotesNotConverted");
                possiblePanels.Add("QuotesToConvert");
                possiblePanels.Add("BeingEntered");
                possiblePanels.Add("InTheOffice");
                possiblePanels.Add("EnteredUnscanned");
                possiblePanels.Add("InEngineering");
                possiblePanels.Add("ReadyToPrint");
                possiblePanels.Add("PrintedInEngineering");
                possiblePanels.Add("AllTabletProjects");
                possiblePanels.Add("AllToolProjects");
                possiblePanels.Add("NatoliOrderList");
                possiblePanels.Add("");
            }
            else if (user.Department == "Order Entry")
            {
                possiblePanels.Add("QuotesToConvert");
                possiblePanels.Add("BeingEntered");
                possiblePanels.Add("InTheOffice");
                possiblePanels.Add("EnteredUnscanned");
                possiblePanels.Add("InEngineering");
                possiblePanels.Add("ReadyToPrint");
                possiblePanels.Add("PrintedInEngineering");
                possiblePanels.Add("");
            }
            FillComboBoxes();
            CreateLayoutGrid();
        }

        private void FillComboBoxes(bool changed = false, int panels = 0)
        {
            Panel0ComboBox.Visibility = Visibility.Hidden;
            Panel1ComboBox.Visibility = Visibility.Hidden;
            Panel2ComboBox.Visibility = Visibility.Hidden;
            Panel3ComboBox.Visibility = Visibility.Hidden;
            Panel4ComboBox.Visibility = Visibility.Hidden;
            Panel5ComboBox.Visibility = Visibility.Hidden;
            Panel0TextBlock.Visibility = Visibility.Hidden;
            Panel1TextBlock.Visibility = Visibility.Hidden;
            Panel2TextBlock.Visibility = Visibility.Hidden;
            Panel3TextBlock.Visibility = Visibility.Hidden;
            Panel4TextBlock.Visibility = Visibility.Hidden;
            Panel5TextBlock.Visibility = Visibility.Hidden;
            int count = changed ? panels : visiblePanels.Count;
            for (int i = 0; i <= count; i++)
            {
                switch (i)
                {
                    case 1:
                        Panel0ComboBox.ItemsSource = possiblePanels;
                        if (visiblePanels.Count >= 1)
                        {
                            Panel0ComboBox.SelectedItem = visiblePanels[0];
                        }
                        Panel0ComboBox.Visibility = Visibility.Visible;
                        Panel0TextBlock.Visibility = Visibility.Visible;
                        break;
                    case 2:
                        Panel1ComboBox.ItemsSource = possiblePanels;
                        if (visiblePanels.Count >= 2)
                        {
                            Panel1ComboBox.SelectedItem = visiblePanels[1];
                        }
                        Panel1ComboBox.Visibility = Visibility.Visible;
                        Panel1TextBlock.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        Panel2ComboBox.ItemsSource = possiblePanels;
                        if (visiblePanels.Count >= 3)
                        {
                            Panel2ComboBox.SelectedItem = visiblePanels[2];
                        }
                        Panel2ComboBox.Visibility = Visibility.Visible;
                        Panel2TextBlock.Visibility = Visibility.Visible;
                        break;
                    case 4:
                        Panel3ComboBox.ItemsSource = possiblePanels;
                        if (visiblePanels.Count >= 4)
                        {
                            Panel3ComboBox.SelectedItem = visiblePanels[3];
                        }
                        Panel3ComboBox.Visibility = Visibility.Visible;
                        Panel3TextBlock.Visibility = Visibility.Visible;
                        break;
                    case 5:
                        Panel4ComboBox.ItemsSource = possiblePanels;
                        if (visiblePanels.Count >= 5)
                        {
                            Panel4ComboBox.SelectedItem = visiblePanels[4];
                        }
                        Panel4ComboBox.Visibility = Visibility.Visible;
                        Panel4TextBlock.Visibility = Visibility.Visible;
                        break;
                    case 6:
                        Panel5ComboBox.ItemsSource = possiblePanels;
                        if (visiblePanels.Count >= 6)
                        {
                            Panel5ComboBox.SelectedItem = visiblePanels[5];
                        }
                        Panel5ComboBox.Visibility = Visibility.Visible;
                        Panel5TextBlock.Visibility = Visibility.Visible;
                        break;
                    default:
                        break;
                }
            }
            
        }
        private void CreateLayoutGrid(bool changed = false, int panels = 0)
        {
            GridPanelsMaster.Children.Clear();
            GridPanelsMaster.ColumnDefinitions.Clear();
            GridPanelsMaster.RowDefinitions.Clear();
            int count = changed ? panels : visiblePanels.Count;
            switch (count)
            {
                case 6:
                    ColumnDefinition column1_6 = new ColumnDefinition();
                    ColumnDefinition column2_6 = new ColumnDefinition();
                    ColumnDefinition column3_6 = new ColumnDefinition();
                    RowDefinition row1_6 = new RowDefinition();
                    RowDefinition row2_6 = new RowDefinition();
                    Grid gridPanel1_6 = new Grid { Name = "GridPanel1",};
                    gridPanel1_6.SetValue(Grid.ColumnProperty, 0);
                    gridPanel1_6.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel2_6 = new Grid { Name = "GridPanel2", };
                    gridPanel2_6.SetValue(Grid.ColumnProperty, 0);
                    gridPanel2_6.SetValue(Grid.RowProperty, 1);
                    Grid gridPanel3_6 = new Grid { Name = "GridPanel3", };
                    gridPanel3_6.SetValue(Grid.ColumnProperty, 1);
                    gridPanel3_6.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel4_6 = new Grid { Name = "GridPanel4", };
                    gridPanel4_6.SetValue(Grid.ColumnProperty, 1);
                    gridPanel4_6.SetValue(Grid.RowProperty, 1);
                    Grid gridPanel5_6 = new Grid { Name = "GridPanel5", };
                    gridPanel5_6.SetValue(Grid.ColumnProperty, 2);
                    gridPanel5_6.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel6_6 = new Grid { Name = "GridPanel6", };
                    gridPanel6_6.SetValue(Grid.ColumnProperty, 2);
                    gridPanel6_6.SetValue(Grid.RowProperty, 1);
                    TextBlock textBlock1_6 = new TextBlock { Name = "TextBlock1", Text = "1", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock2_6 = new TextBlock { Name = "TextBlock2", Text = "2", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock3_6 = new TextBlock { Name = "TextBlock3", Text = "3", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock4_6 = new TextBlock { Name = "TextBlock4", Text = "4", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock5_6 = new TextBlock { Name = "TextBlock5", Text = "5", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock6_6 = new TextBlock { Name = "TextBlock6", Text = "6", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    gridPanel1_6.Children.Add(textBlock1_6);
                    gridPanel2_6.Children.Add(textBlock2_6);
                    gridPanel3_6.Children.Add(textBlock3_6);
                    gridPanel4_6.Children.Add(textBlock4_6);
                    gridPanel5_6.Children.Add(textBlock5_6);
                    gridPanel6_6.Children.Add(textBlock6_6);
                    GridPanelsMaster.ColumnDefinitions.Add(column1_6);
                    GridPanelsMaster.ColumnDefinitions.Add(column2_6);
                    GridPanelsMaster.ColumnDefinitions.Add(column3_6);
                    GridPanelsMaster.RowDefinitions.Add(row1_6);
                    GridPanelsMaster.RowDefinitions.Add(row2_6);
                    GridPanelsMaster.Children.Add(gridPanel1_6);
                    GridPanelsMaster.Children.Add(gridPanel2_6);
                    GridPanelsMaster.Children.Add(gridPanel3_6);
                    GridPanelsMaster.Children.Add(gridPanel4_6);
                    GridPanelsMaster.Children.Add(gridPanel5_6);
                    GridPanelsMaster.Children.Add(gridPanel6_6);
                    break;
                case 5:
                    ColumnDefinition column1_5 = new ColumnDefinition();
                    ColumnDefinition column2_5 = new ColumnDefinition();
                    ColumnDefinition column3_5 = new ColumnDefinition();
                    RowDefinition row1_5 = new RowDefinition();
                    RowDefinition row2_5 = new RowDefinition();
                    Grid gridPanel1_5 = new Grid { Name = "GridPanel1", };
                    gridPanel1_5.SetValue(Grid.ColumnProperty, 0);
                    gridPanel1_5.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel2_5 = new Grid { Name = "GridPanel2", };
                    gridPanel2_5.SetValue(Grid.ColumnProperty, 0);
                    gridPanel2_5.SetValue(Grid.RowProperty, 1);
                    Grid gridPanel3_5 = new Grid { Name = "GridPanel3", };
                    gridPanel3_5.SetValue(Grid.ColumnProperty, 1);
                    gridPanel3_5.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel4_5 = new Grid { Name = "GridPanel4", };
                    gridPanel4_5.SetValue(Grid.ColumnProperty, 1);
                    gridPanel4_5.SetValue(Grid.RowProperty, 1);
                    Grid gridPanel5_5 = new Grid { Name = "GridPanel5", };
                    gridPanel5_5.SetValue(Grid.ColumnProperty, 2);
                    gridPanel5_5.SetValue(Grid.RowProperty, 0);
                    gridPanel5_5.SetValue(Grid.RowSpanProperty, 2);
                    TextBlock textBlock1_5 = new TextBlock { Name = "TextBlock1", Text = "1", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock2_5 = new TextBlock { Name = "TextBlock2", Text = "2", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock3_5 = new TextBlock { Name = "TextBlock3", Text = "3", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock4_5 = new TextBlock { Name = "TextBlock4", Text = "4", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock5_5 = new TextBlock { Name = "TextBlock5", Text = "5", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    gridPanel1_5.Children.Add(textBlock1_5);
                    gridPanel2_5.Children.Add(textBlock2_5);
                    gridPanel3_5.Children.Add(textBlock3_5);
                    gridPanel4_5.Children.Add(textBlock4_5);
                    gridPanel5_5.Children.Add(textBlock5_5);
                    GridPanelsMaster.ColumnDefinitions.Add(column1_5);
                    GridPanelsMaster.ColumnDefinitions.Add(column2_5);
                    GridPanelsMaster.ColumnDefinitions.Add(column3_5);
                    GridPanelsMaster.RowDefinitions.Add(row1_5);
                    GridPanelsMaster.RowDefinitions.Add(row2_5);
                    GridPanelsMaster.Children.Add(gridPanel1_5);
                    GridPanelsMaster.Children.Add(gridPanel2_5);
                    GridPanelsMaster.Children.Add(gridPanel3_5);
                    GridPanelsMaster.Children.Add(gridPanel4_5);
                    GridPanelsMaster.Children.Add(gridPanel5_5);
                    break;
                case 4:
                    ColumnDefinition column1_4 = new ColumnDefinition();
                    ColumnDefinition column2_4 = new ColumnDefinition();
                    RowDefinition row1_4 = new RowDefinition();
                    RowDefinition row2_4 = new RowDefinition();
                    Grid gridPanel1_4 = new Grid { Name = "GridPanel1", };
                    gridPanel1_4.SetValue(Grid.ColumnProperty, 0);
                    gridPanel1_4.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel2_4 = new Grid { Name = "GridPanel2", };
                    gridPanel2_4.SetValue(Grid.ColumnProperty, 0);
                    gridPanel2_4.SetValue(Grid.RowProperty, 1);
                    Grid gridPanel3_4 = new Grid { Name = "GridPanel3", };
                    gridPanel3_4.SetValue(Grid.ColumnProperty, 1);
                    gridPanel3_4.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel4_4 = new Grid { Name = "GridPanel4", };
                    gridPanel4_4.SetValue(Grid.ColumnProperty, 1);
                    gridPanel4_4.SetValue(Grid.RowProperty, 1);
                    TextBlock textBlock1_4 = new TextBlock { Name = "TextBlock1", Text = "1", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock2_4 = new TextBlock { Name = "TextBlock2", Text = "2", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock3_4 = new TextBlock { Name = "TextBlock3", Text = "3", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock4_4 = new TextBlock { Name = "TextBlock4", Text = "4", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    gridPanel1_4.Children.Add(textBlock1_4);
                    gridPanel2_4.Children.Add(textBlock2_4);
                    gridPanel3_4.Children.Add(textBlock3_4);
                    gridPanel4_4.Children.Add(textBlock4_4);
                    GridPanelsMaster.ColumnDefinitions.Add(column1_4);
                    GridPanelsMaster.ColumnDefinitions.Add(column2_4);
                    GridPanelsMaster.RowDefinitions.Add(row1_4);
                    GridPanelsMaster.RowDefinitions.Add(row2_4);
                    GridPanelsMaster.Children.Add(gridPanel1_4);
                    GridPanelsMaster.Children.Add(gridPanel2_4);
                    GridPanelsMaster.Children.Add(gridPanel3_4);
                    GridPanelsMaster.Children.Add(gridPanel4_4);
                    break;
                case 3:
                    ColumnDefinition column1_3 = new ColumnDefinition();
                    ColumnDefinition column2_3 = new ColumnDefinition();
                    ColumnDefinition column3_3 = new ColumnDefinition();
                    RowDefinition row1_3 = new RowDefinition();
                    Grid gridPanel1_3 = new Grid { Name = "GridPanel1", };
                    gridPanel1_3.SetValue(Grid.ColumnProperty, 0);
                    gridPanel1_3.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel2_3 = new Grid { Name = "GridPanel2", };
                    gridPanel2_3.SetValue(Grid.ColumnProperty, 1);
                    gridPanel2_3.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel3_3 = new Grid { Name = "GridPanel3", };
                    gridPanel3_3.SetValue(Grid.ColumnProperty, 2);
                    gridPanel3_3.SetValue(Grid.RowProperty, 0);
                    TextBlock textBlock1_3 = new TextBlock { Name = "TextBlock1", Text = "1", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock2_3 = new TextBlock { Name = "TextBlock2", Text = "2", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock3_3 = new TextBlock { Name = "TextBlock3", Text = "3", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    gridPanel1_3.Children.Add(textBlock1_3);
                    gridPanel2_3.Children.Add(textBlock2_3);
                    gridPanel3_3.Children.Add(textBlock3_3);
                    GridPanelsMaster.ColumnDefinitions.Add(column1_3);
                    GridPanelsMaster.ColumnDefinitions.Add(column2_3);
                    GridPanelsMaster.ColumnDefinitions.Add(column3_3);
                    GridPanelsMaster.RowDefinitions.Add(row1_3);
                    GridPanelsMaster.Children.Add(gridPanel1_3);
                    GridPanelsMaster.Children.Add(gridPanel2_3);
                    GridPanelsMaster.Children.Add(gridPanel3_3);
                    break;
                case 2:
                    ColumnDefinition column1_2 = new ColumnDefinition();
                    ColumnDefinition column2_2 = new ColumnDefinition();
                    RowDefinition row1_2 = new RowDefinition();
                    Grid gridPanel1_2 = new Grid { Name = "GridPanel1", };
                    gridPanel1_2.SetValue(Grid.ColumnProperty, 0);
                    gridPanel1_2.SetValue(Grid.RowProperty, 0);
                    Grid gridPanel2_2 = new Grid { Name = "GridPanel2", };
                    gridPanel2_2.SetValue(Grid.ColumnProperty, 1);
                    gridPanel2_2.SetValue(Grid.RowProperty, 0);
                    TextBlock textBlock1_2 = new TextBlock { Name = "TextBlock1", Text = "1", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    TextBlock textBlock2_2 = new TextBlock { Name = "TextBlock2", Text = "2", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    gridPanel1_2.Children.Add(textBlock1_2);
                    gridPanel2_2.Children.Add(textBlock2_2);
                    GridPanelsMaster.ColumnDefinitions.Add(column1_2);
                    GridPanelsMaster.ColumnDefinitions.Add(column2_2);
                    GridPanelsMaster.RowDefinitions.Add(row1_2);
                    GridPanelsMaster.Children.Add(gridPanel1_2);
                    GridPanelsMaster.Children.Add(gridPanel2_2);
                    break;
                case 1:
                    Grid gridPanel1_1 = new Grid { Name = "GridPanel1", };
                    TextBlock textBlock1_1 = new TextBlock { Name = "TextBlock1", Text = "1", FontSize = 50, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };
                    gridPanel1_1.Children.Add(textBlock1_1);
                    GridPanelsMaster.Children.Add(gridPanel1_1);
                    break;
                default:
                    break;
            }
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            switch (comboBox.Name.Substring(0, 6))
            {
                case "Panel0":
                    if (visiblePanels.Count >= 1)
                    {
                        visiblePanels[0] = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    
                    break;
                case "Panel1":
                    if (visiblePanels.Count >= 2)
                    {
                        visiblePanels[1] = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        for (int i = visiblePanels.Count+1; i <= 2; i++)
                        {
                            visiblePanels.Add("");
                        }
                        visiblePanels[1] = comboBox.SelectedItem.ToString();
                    }
                    break;
                case "Panel2":
                    if (visiblePanels.Count >= 3)
                    {
                        visiblePanels[2] = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        for (int i = visiblePanels.Count+1; i <= 3; i++)
                        {
                            visiblePanels.Add("");
                        }
                        visiblePanels[2] = comboBox.SelectedItem.ToString();
                    }
                    break;
                case "Panel3":
                    if (visiblePanels.Count >= 4)
                    {
                        visiblePanels[3] = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        for (int i = visiblePanels.Count+1; i <= 4; i++)
                        {
                            visiblePanels.Add("");
                        }
                        visiblePanels[3] = comboBox.SelectedItem.ToString();
                    }
                    break;
                case "Panel4":
                    if (visiblePanels.Count >= 5)
                    {
                        visiblePanels[4] = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        for (int i = visiblePanels.Count+1; i <= 5; i++)
                        {
                            visiblePanels.Add("");
                        }
                        visiblePanels[4] = comboBox.SelectedItem.ToString();
                    }
                    break;
                case "Panel5":
                    if (visiblePanels.Count >= 6)
                    {
                        visiblePanels[5] = comboBox.SelectedItem.ToString();
                    }
                    else
                    {
                        for (int i = visiblePanels.Count+1; i <= 6; i++)
                        {
                            visiblePanels.Add("");
                        }
                        visiblePanels[5] = comboBox.SelectedItem.ToString();
                    }
                    break;
                default:
                    break;
            }
        }
        private void OnePanelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if(visiblePanels.Count!=1)
            {
                visiblePanels.Clear();
                foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>())
                {
                    if (!(comboBox.SelectedItem is null))
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    else
                    {
                        visiblePanels.Add("");
                    }
                }
                visiblePanels.RemoveRange(1, visiblePanels.Count - 1);
                FillComboBoxes(true, 1);
                CreateLayoutGrid(true , 1);
            }
        }
        private void TwoPanelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (visiblePanels.Count != 2)
            {
                visiblePanels.Clear();
                foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>())
                {
                    if (!(comboBox.SelectedItem is null))
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    else
                    {
                        visiblePanels.Add("");
                    }
                }
                visiblePanels.RemoveRange(2, visiblePanels.Count - 2);
                FillComboBoxes(true, 2);
                CreateLayoutGrid(true, 2);
            }
        }
        private void ThreePanelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (visiblePanels.Count != 3)
            {
                visiblePanels.Clear();
                foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>())
                {
                    if (!(comboBox.SelectedItem is null))
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    else
                    {
                        visiblePanels.Add("");
                    }
                }
                visiblePanels.RemoveRange(3, visiblePanels.Count - 3);
                FillComboBoxes(true, 3);
                CreateLayoutGrid(true, 3);
            }
        }
        private void FourPanelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (visiblePanels.Count != 4)
            {
                foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>())
                {
                    if (!(comboBox.SelectedItem is null))
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    else
                    {
                        visiblePanels.Add("");
                    }
                }
                visiblePanels.RemoveRange(4, visiblePanels.Count - 4);
                FillComboBoxes(true, 4);
                CreateLayoutGrid(true, 4);
            }
        }
        private void FivePanelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (visiblePanels.Count != 5)
            {
                visiblePanels.Clear();
                foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>())
                {
                    if (!(comboBox.SelectedItem is null))
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    else
                    {
                        visiblePanels.Add("");
                    }
                }
                visiblePanels.RemoveRange(5, visiblePanels.Count - 5);
                FillComboBoxes(true, 5);
                CreateLayoutGrid(true, 5);
            }
        }
        private void SixPanelsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (visiblePanels.Count != 6)
            {
                visiblePanels.Clear();
                foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>())
                {
                    if (!(comboBox.SelectedItem is null))
                    {
                        visiblePanels.Add(comboBox.SelectedItem.ToString());
                    }
                    else
                    {
                        visiblePanels.Add("");
                    }
                }
                visiblePanels.RemoveRange(6, visiblePanels.Count - 6);
                FillComboBoxes(true, 6);
                CreateLayoutGrid(true, 6);
            }
        }
        private void SaveSettings()
        {
            NAT02Context _nat02context = new NAT02Context();

            EoiSettings eoiSettings = _nat02context.EoiSettings.Where(s => s.EmployeeId == user.EmployeeCode).First();
            string newPanels = "";
            foreach (string p in visiblePanels)
            {
                newPanels += p + ",";
            }
            newPanels = newPanels.Remove(newPanels.Length - 1);
            eoiSettings.Panels = newPanels;
            _nat02context.EoiSettings.Update(eoiSettings);
            _nat02context.SaveChanges();
            _nat02context.Dispose();

            user.VisiblePanels = visiblePanels;
            parent.BoolValue = true;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            visiblePanels.Clear();
            foreach (ComboBox comboBox in PanelsStackPanel.Children.OfType<ComboBox>().Where(cb => cb.IsVisible == true))
            {
                visiblePanels.Add(comboBox.SelectedItem.ToString());
            }
            if (visiblePanels.Contains(""))
            {
                var result = MessageBox.Show("You have chosen to show blank panel(s). Select 'Cancel' to continue making changes.", "Information", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
                else
                {
                    SaveSettings();
                }
            }
            else
            {
                SaveSettings();
            }
            
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {

        }
    }
}
