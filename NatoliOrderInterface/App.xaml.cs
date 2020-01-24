using System.Windows;
using System.Linq;
using System.Collections.Generic;

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
        public static List<string> StandardKeys = new List<string> { "N-6600-32M", "N-6600-01M", "N-6600-02M", "N-6600-03M", "N-7080-02M", "N-6010", "N-6441", "N-6441M", "N-6653", "N-6652", "N-6445", "N-6444" };
        
        
        public static void GetConnectionString()
        {
            var configFile = NatoliOrderInterface.Properties.Resources.NatoliOrderInterface;
            string[] text = configFile.Split("\n");

            foreach (string line in text)
            {
                if (line.First() != '#')
                {
                    string key = line.Split(':')[0];
                    switch (key)
                    {
                        case "Server":
                            Server = line.Split(':')[1].Trim();
                            break;
                        case "Persist Security Info":
                            PersistSecurityInfo = line.Split(':')[1].Trim();
                            break;
                        case "User ID":
                            UserID = line.Split(':')[1].Trim();
                            break;
                        case "Password":
                            Password = line.Split(':')[1].Trim();
                            break;
                    }
                }
            }
        }
    }
}
