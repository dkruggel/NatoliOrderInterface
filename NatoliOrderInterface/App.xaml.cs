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
using NatoliOrderInterface.Models;
using System.Windows.Input;

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



            //try
            //{
            //    var configFile = NatoliOrderInterface.Properties.Resources.NatoliOrderInterface;
            //    string[] text = configFile.Split("\n");

            //    foreach (string line in text)
            //    {
            //        if (line.First() != '#')
            //        {
            //            string key = line.Split(':')[0];
            //            switch (key)
            //            {
            //                case "Server":
            //                    Server = line.Split(':')[1].Trim();
            //                    break;
            //                case "Persist Security Info":
            //                    PersistSecurityInfo = line.Split(':')[1].Trim();
            //                    break;
            //                case "User ID":
            //                    UserID = line.Split(':')[1].Trim();
            //                    break;
            //                case "Password":
            //                    Password = line.Split(':')[1].Trim();
            //                    break;
            //            }
            //        }
            //    }
            //}
            //catch (Exception ex)
            //{
            //    IMethods.WriteToErrorLog("App.Xaml.cs => GetConnectionString()", ex.Message, null);
            //}
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
    }
}
