using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Interaction logic for LegendWindow.xaml
    /// </summary>
    public partial class LegendWindow : Window
    {
        User User = null;
        public LegendWindow(User user)
        {
            InitializeComponent();
            this.User = user;



            if (User.EmployeeCode == "E4408" || User.EmployeeCode == "E4754" || User.EmployeeCode == "E4509")
            {

            }
            else if (User.Department == "Engineering")
            {
                
            }
            else if (User.Department == "Order Entry")
            {
                
            }
            else if (User.Department == "Barb")
            {
                
            }
            else
            {
                if (User.Department != "Hob Programming")
                {
                    
                }
            }
        }
    }
}
