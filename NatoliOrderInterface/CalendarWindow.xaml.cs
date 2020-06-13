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
using System.Linq;
using Jarloo;

namespace NatoliOrderInterface
{
    
    /// <summary>
    /// Interaction logic for CalendarWindow.xaml
    /// </summary>
    public partial class CalendarWindow : Window
    {
        User user = null;
        public CalendarWindow(User user)
        {
            InitializeComponent();
            this.user = user;
            List<string> months = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            cboMonth.ItemsSource = months;

            for (int i = -50; i < 50; i++)
            {
                cboYear.Items.Add(DateTime.Today.AddYears(i).Year);
            }

            cboMonth.SelectedItem = months.FirstOrDefault(w => w == DateTime.Today.ToString("MMMM"));
            cboYear.SelectedItem = DateTime.Today.Year;

            cboMonth.SelectionChanged += (o, e) => RefreshCalendar();
            cboYear.SelectionChanged += (o, e) => RefreshCalendar();
            this.Top = Application.Current.MainWindow.Top;
            this.Left = Application.Current.MainWindow.Left;
        }

        private void RefreshCalendar()
        {
            if (cboYear.SelectedItem == null) return;
            if (cboMonth.SelectedItem == null) return;

            int year = (int)cboYear.SelectedItem;

            int month = cboMonth.SelectedIndex + 1;

            DateTime targetDate = new DateTime(year, month, 1);

            Calendar.BuildCalendar(targetDate);
        }

        private void Calendar_DayChanged(object sender, Jarloo.Calendar.DayChangedEventArgs e)
        {
            //save the text edits to persistant storage
        }

        private void PreviousMonth_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cboMonth.SelectedIndex > 0)
            {
                cboMonth.SelectedIndex--;
            }

        }

        private void NextMonth_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (cboMonth.SelectedIndex < 12)
            {
                cboMonth.SelectedIndex++;
            }
        }
    }
}
