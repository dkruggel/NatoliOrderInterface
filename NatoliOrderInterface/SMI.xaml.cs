using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using NatoliOrderInterface.Models.NAT01;
using System.Linq;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for SMI.xaml
    /// </summary>
    public partial class SMI : Window
    {
        
        private List<CustomerInstructionTable> SMIs = new List<CustomerInstructionTable>();
        private string customerName;
        private string customerNumber;
        public SMI(string custName, string custNumber)
        {
            InitializeComponent();
            customerName = custName;
            customerNumber = custNumber;
            SMIUserSpecificHeader.Text = customerName + " SMI's";
            NAT01Context nat01Context = new NAT01Context();
            SMIs = nat01Context.CustomerInstructionTable.Where(i => i.CustomerId == customerNumber).OrderBy(i => i.Inactive).ThenBy(i => i.Sequence).ToList();
            nat01Context.Dispose();
            FillInfo();
        }
        public void FillInfo()
        {
            SMIUserSpecificGrid.Children.Clear();
            SMIUserSpecificGrid.RowDefinitions.Clear();
            CreateHeaderGrid();
            CreateSMIGrids();
        }
        public void CreateHeaderGrid()
        {
            RowDefinition rowDef0 = new RowDefinition { Height = new GridLength(20) };
            #region GridHeaderBorders
            Border border0 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(1, 1, 1, 1) };
            border0.SetValue(Grid.ColumnProperty, 0);
            Border border1 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 1, 1) };
            border1.SetValue(Grid.ColumnProperty, 1);
            Border border2 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 1, 1) };
            border2.SetValue(Grid.ColumnProperty, 2);
            Border border3 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 1, 1) };
            border3.SetValue(Grid.ColumnProperty, 3);
            Border border4 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 1, 1) };
            border4.SetValue(Grid.ColumnProperty, 4);
            Border border5 = new Border { BorderBrush = Brushes.Black, BorderThickness = new Thickness(0, 1, 1, 1) };
            border5.SetValue(Grid.ColumnProperty, 5);
            #endregion

            #region GridHeaderGrids
            Grid grid0 = new Grid();
            TextBlock text0 = new TextBlock
            {
                Text = "Inactive",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            grid0.Children.Add(text0);

            Grid grid1 = new Grid();
            TextBlock text1 = new TextBlock
            {
                Text = "Seq.",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            grid1.Children.Add(text1);

            Grid grid2 = new Grid();
            TextBlock text2 = new TextBlock
            {
                Text = "SMI Description",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            grid2.Children.Add(text2);

            Grid grid3 = new Grid();
            TextBlock text3 = new TextBlock
            {
                Text = "Category",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            grid3.Children.Add(text3);

            Grid grid4 = new Grid();
            TextBlock text4 = new TextBlock
            {
                Text = "User Stamp",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            grid4.Children.Add(text4);

            Grid grid5 = new Grid();
            TextBlock text5 = new TextBlock
            {
                Text = "Date Stamp",
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(4, 0, 0, 0)
            };
            grid5.Children.Add(text5);
            #endregion

            #region AddGridHeader
            border0.Child = grid0;
            border1.Child = grid1;
            border2.Child = grid2;
            border3.Child = grid3;
            border4.Child = grid4;
            border5.Child = grid5;
            SMIUserSpecificGrid.RowDefinitions.Add(rowDef0);
            SMIUserSpecificGrid.Children.Add(border0);
            SMIUserSpecificGrid.Children.Add(border1);
            SMIUserSpecificGrid.Children.Add(border2);
            SMIUserSpecificGrid.Children.Add(border3);
            SMIUserSpecificGrid.Children.Add(border4);
            SMIUserSpecificGrid.Children.Add(border5);
            #endregion
        }
        public void CreateSMIGrids()
        {
            int i = 1;
            foreach (CustomerInstructionTable SMI in SMIs)
            {
                RowDefinition rowDef = new RowDefinition { Height = new GridLength(20) };
                Border border0 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(1, 0, 1, 1) };
                border0.SetValue(Grid.ColumnProperty, 0);
                border0.SetValue(Grid.RowProperty, i);
                Border border1 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1) };
                border1.SetValue(Grid.ColumnProperty, 1);
                border1.SetValue(Grid.RowProperty, i);
                Border border2 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1) };
                border2.SetValue(Grid.ColumnProperty, 2);
                border2.SetValue(Grid.RowProperty, i);
                Border border3 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1) };
                border3.SetValue(Grid.ColumnProperty, 3);
                border3.SetValue(Grid.RowProperty, i);
                Border border4 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1) };
                border4.SetValue(Grid.ColumnProperty, 4);
                border4.SetValue(Grid.RowProperty, i);
                Border border5 = new Border { BorderBrush = Brushes.Gray, BorderThickness = new Thickness(0, 0, 1, 1) };
                border5.SetValue(Grid.ColumnProperty, 5);
                border5.SetValue(Grid.RowProperty, i);

                Grid grid0 = new Grid();
                Grid grid1 = new Grid();
                Grid grid2 = new Grid();
                Grid grid3 = new Grid();
                Grid grid4 = new Grid();
                Grid grid5 = new Grid();

                CheckBox checkBox0 = new CheckBox
                {
                    IsEnabled = false,
                    IsChecked = SMI.Inactive,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                TextBlock text1 = new TextBlock
                {
                    Text = SMI.Sequence.ToString(),
                    Foreground = Brushes.LightGray,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 2, 0)
                };
                TextBlock text2 = new TextBlock
                {
                    Text = SMI.Instruction.TrimEnd(),
                    Foreground = SMI.Inactive ? Brushes.Gray : Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 0, 0)
                };
                TextBlock text3 = new TextBlock
                {
                    Text = SMI.Category,
                    Foreground = SMI.Inactive ? Brushes.Gray : Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 0, 0)
                };
                TextBlock text4 = new TextBlock
                {
                    Text = SMI.UserStamp,
                    Foreground = SMI.Inactive ? Brushes.Gray : Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 0, 0)
                };
                TextBlock text5 = new TextBlock
                {
                    Text = SMI.DateStamp.ToShortDateString(),
                    Foreground = SMI.Inactive ? Brushes.Gray : Brushes.Black,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(2, 0, 0, 0)
                };

                grid0.Children.Add(checkBox0);
                grid1.Children.Add(text1);
                grid2.Children.Add(text2);
                grid3.Children.Add(text3);
                grid4.Children.Add(text4);
                grid5.Children.Add(text5);
                border0.Child = grid0;
                border1.Child = grid1;
                border2.Child = grid2;
                border3.Child = grid3;
                border4.Child = grid4;
                border5.Child = grid5;
                SMIUserSpecificGrid.RowDefinitions.Add(rowDef);
                SMIUserSpecificGrid.Children.Add(border0);
                SMIUserSpecificGrid.Children.Add(border1);
                SMIUserSpecificGrid.Children.Add(border2);
                SMIUserSpecificGrid.Children.Add(border3);
                SMIUserSpecificGrid.Children.Add(border4);
                SMIUserSpecificGrid.Children.Add(border5);
                i++;
            }
        }
    }
}
