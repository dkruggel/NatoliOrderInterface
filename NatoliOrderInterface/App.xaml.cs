using System.Windows;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.FileExtensions;
using System.IO;
using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using NatoliOrderInterface.Models;
using System.Windows.Input;
using NatoliOrderInterface.Models.NAT01;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Server;
        public static string PersistSecurityInfo;
        public static string UserID;
        public static string Password;
        public static string SmtpServer;
        public static int? SmtpPort;
        public static List<string> StandardKeys = new List<string> { "N-6600-32M", "N-6600-01M", "N-6600-02M", "N-6600-03M", "N-7080-02M", "N-6010", "N-6441", "N-6441M", "N-6653", "N-6652", "N-6445", "N-6444" };
        public static User user { get; set; }
        
        public static void GetConnectionString()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(@"\\nshare\VB_Apps\NatoliOrderInterface\Resources")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                var emailConfiguration = configuration.GetSection("ConnectionStrings");
                Server = emailConfiguration.GetSection("Server").Value;
                PersistSecurityInfo = emailConfiguration.GetSection("PersistSecurityInfo").Value;
                UserID = emailConfiguration.GetSection("UserID").Value;
                Password = emailConfiguration.GetSection("Password").Value;
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("App.Xaml.cs => GetConnectionString()", ex.Message, null);
            }
        }
        public static void GetEmailSettings()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(@"\\nshare\VB_Apps\NatoliOrderInterface\Resources")
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                IConfigurationRoot configuration = builder.Build();
                var emailConfiguration = configuration.GetSection("EmailConfiguration");
                SmtpServer = emailConfiguration.GetSection("SmtpServer").Value;
                SmtpPort = Int32.Parse(emailConfiguration.GetSection("SmtpPort").Value);
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("App.Xaml.cs => GetEmailSettings()", ex.Message, null);
            }
        }

        private void OpenNotesButtonButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Grid childGrid = (Grid)button.Parent;
            Grid grid = (Grid)childGrid.Parent;

            int id = Convert.ToInt32(((TextBlock)grid.Children.OfType<TextBlock>().First(tb => tb.Tag.ToString() == "ID")).Text);
            CustomerNoteWindow customerNoteWindow = new CustomerNoteWindow(id, new User(Environment.UserName));
            customerNoteWindow.Show();
        }

        private void SearchTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            
            if (e.Key == System.Windows.Input.Key.Escape && textBox.IsFocused)
            {
                Grid grid = textBox.Parent as Grid;
                Image xImage = grid.Children.OfType<Image>().First(i => i.Name.ToString() == "xImage") as Image;
                textBox.Text = "";
                xImage.Visibility = Visibility.Collapsed;
            }
        }

        private void xImage_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image xImage = sender as Image;
            Grid grid = xImage.Parent as Grid;
            TextBox searchTextBox = grid.Children.OfType<TextBox>().First(i => i.Name.ToString() == "SearchTextBox") as TextBox;
            xImage.Visibility = Visibility.Collapsed;
            searchTextBox.Text = "";
        }

        private void MagImage_PreviewMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Image magImage = sender as Image;
            Grid grid = magImage.Parent as Grid;
            Border border = grid.Parent as Border;
            if (border.ActualWidth < 140)
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(150, TimeSpan.FromSeconds(.5));
                border.BeginAnimation(Border.WidthProperty, doubleAnimation);
            }
            else
            {
                DoubleAnimation doubleAnimation = new DoubleAnimation(24, TimeSpan.FromSeconds(.5));
                border.BeginAnimation(Border.WidthProperty, doubleAnimation);
            }
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox.Text.Length > 0)
            {
                Grid grid = textBox.Parent as Grid;
                Image xImage = grid.Children.OfType<Image>().First(i => i.Name.ToString() == "xImage") as Image;
                xImage.Visibility = Visibility.Visible;
            }
            else
            {
                Grid grid = textBox.Parent as Grid;
                Image xImage = grid.Children.OfType<Image>().First(i => i.Name.ToString() == "xImage") as Image;
                xImage.Visibility = Visibility.Collapsed;
            }
        }

        private void OpenOrderButton_Click(object sender, RoutedEventArgs e)
        {
            using var _context = new NAT02Context();
            using var _nat01context = new NAT01Context();
            Image image = sender as Image;
            Grid grid = ((image.Parent as StackPanel).Parent as Grid).Parent as Grid;
            Window parent = Window.GetWindow(grid);
            MainWindow mainWindow = null;
            WorkOrder workOrder = null;

            try
            {
                TextBlock textBlock = grid.Children.OfType<TextBlock>().ToList()[0];
                string orderNumber = textBlock.Text;
                workOrder = new WorkOrder(int.Parse(orderNumber), parent);
                WindowCollection collection = App.Current.Windows;
                foreach (Window w in collection)
                {
                    if (w.Title.Contains(workOrder.OrderNumber.ToString()))
                    {
                        _context.Dispose();
                        _nat01context.Dispose();
                        w.WindowState = WindowState.Normal;
                        w.Show();
                        goto AlreadyOpen2;
                    }
                    else if (w.GetType().FullName.Contains("MainWindow"))
                    {
                        mainWindow = w as MainWindow;
                    }
                }
                if (_context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User != user.GetUserName()))
                {
                    MessageBox.Show("BEWARE!!\n" + _context.EoiOrdersBeingChecked.Where(o => o.OrderNo == workOrder.OrderNumber && o.User != user.GetUserName()).FirstOrDefault().User + " is in this order at the moment.");
                }
                else if (_context.EoiOrdersBeingChecked.Any(o => o.OrderNo == workOrder.OrderNumber && o.User == user.GetUserName()))
                {
                    MessageBox.Show("You already have this order open.");
                    _context.Dispose();
                    _nat01context.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                OrderInfoWindow orderInfoWindow = new OrderInfoWindow(workOrder, mainWindow, null, user)
                {
                    //Owner = parent,
                    Left = MainWindow.Left,
                    Top = MainWindow.Top
                };
                orderInfoWindow.Show();
                orderInfoWindow.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        AlreadyOpen2:
            _context.Dispose();
            _nat01context.Dispose();
        }

        private void ExpandModule_Click(object sender, RoutedEventArgs e)
        {
            Button expandButton = sender as Button;
            Button collapseButton = (expandButton.Parent as StackPanel).Children.OfType<Button>().Single(b => b.Name == "CollapseButton");
            int count = (((expandButton.Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Items.Count;

            expandButton.Visibility = Visibility.Collapsed;

            collapseButton.Visibility = Visibility.Visible;

            if (count == 0)
            {
                (expandButton.TemplatedParent as Label).MinHeight = 100;
                (expandButton.TemplatedParent as Label).Height = 205;
            }
            else
            {
                if (count < 10)
                {
                    (collapseButton.TemplatedParent as Label).Height = 35 * (count + 1) + 100;
                }
                else
                {
                    (collapseButton.TemplatedParent as Label).Height = 500;
                }
                (expandButton.TemplatedParent as Label).MinHeight = 200;
            }
        }

        private void CollapseModule_Click(object sender, RoutedEventArgs e)
        {
            Button collapseButton = sender as Button;
            Button expandButton = (collapseButton.Parent as StackPanel).Children.OfType<Button>().Single(b => b.Name == "ExpandButton");
            int count = (((collapseButton.Parent as StackPanel).Parent as DockPanel).Parent as Grid).Children.OfType<ListBox>().First().Items.Count;

            collapseButton.Visibility = Visibility.Collapsed;

            expandButton.Visibility = Visibility.Visible;

            if (count == 0)
            {
                (collapseButton.TemplatedParent as Label).MinHeight = 100;
                (collapseButton.TemplatedParent as Label).Height = 100;
            }
            else
            {
                if (count < 3)
                {
                    (collapseButton.TemplatedParent as Label).Height = 35 * count + 100;
                }
                else
                {
                    (collapseButton.TemplatedParent as Label).Height = 205;
                }
                (expandButton.TemplatedParent as Label).MinHeight = 200;
            }
        }
    }
}
