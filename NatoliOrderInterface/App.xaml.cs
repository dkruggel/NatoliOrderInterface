using System.Windows;
using System.Linq;
using System.IO;
using System.Windows.Controls;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Server = "";
        public static string PersistSecurityInfo = "";
        public static string UserID = "";
        public static string Password = "";
        public static void GetConnectionString()
        {
            StreamReader reader = File.OpenText("NatoliOrderInterface.config");
            string line;
            while ((line = reader.ReadLine()) != null)
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
            reader.Dispose();
        }
    }
}
