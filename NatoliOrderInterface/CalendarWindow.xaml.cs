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
using NatoliOrderInterface.Models;

namespace NatoliOrderInterface
{


    /// <summary>
    /// Interaction logic for CalendarWindow.xaml
    /// </summary>
    public partial class CalendarWindow : Window
    {
        User user = null;
        List<string> months = new List<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        DateTime? timeEnteredCalendar = null;
        public CalendarWindow(User user)
        {
            InitializeComponent();
            this.user = user;
            this.timeEnteredCalendar = DateTime.Now;
            cboMonth.ItemsSource = months;

            for (int i = -50; i < 50; i++)
            {
                cboYear.Items.Add(DateTime.Today.AddYears(i).Year);
            }

            MoveCalendarToToday();

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
            timeEnteredCalendar = DateTime.Now;
        }

        private void MoveCalendarToToday()
        {
            cboMonth.SelectedItem = months.FirstOrDefault(w => w == DateTime.Today.ToString("MMMM"));
            cboYear.SelectedItem = DateTime.Today.Year;
        }
        private void Calendar_DayChanged(object sender, Jarloo.Calendar.DayChangedEventArgs e)
        {
            DateTime d = e.Day.Date;

            using var _nat02Context = new NAT02Context();
            // Delete from table
            if (string.IsNullOrEmpty(e.Day.Notes) || string.IsNullOrWhiteSpace(e.Day.Notes))
            {
                // Exists
                if (_nat02Context.EoiCalendar.Any(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day))
                {
                    if(_nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day).ActionDateTime < timeEnteredCalendar || _nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day).DomainName == user.DomainName)
                    {
                        // Remove
                        EoiCalendar eoiCalendar = _nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day);
                        _nat02Context.EoiCalendar.Remove(eoiCalendar);
                        _nat02Context.SaveChanges();
                        _nat02Context.Dispose();
                    }
                    else
                    {
                        MessageBox.Show("There appears to have been a change made by '" + _nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day).DomainName + "' to this date before you deleted the note. Please re-open the calendar to load the information.", "Prevoiusly changed", MessageBoxButton.OK, MessageBoxImage.Information);
                        _nat02Context.Dispose();
                    }
                    
                }
            }
            // Insert or Update
            else
            {
                // Exists
                if (_nat02Context.EoiCalendar.Any(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day))
                {
                    if (_nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day).ActionDateTime < timeEnteredCalendar || _nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day).DomainName == user.DomainName)
                    {
                        EoiCalendar eoiCalendar = _nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day);
                        eoiCalendar.Notes = e.Day.Notes;
                        eoiCalendar.DomainName = user.DomainName;
                        _nat02Context.SaveChanges();
                        _nat02Context.Dispose();
                    }
                    else
                    {
                        MessageBox.Show("There appears to have been a change made by '" + _nat02Context.EoiCalendar.First(c => c.Year == (short)d.Year && c.Month == (byte)d.Month && c.Day == (byte)d.Day).DomainName + "' to this date before you altered the note. Please re-open the calendar to load the information.", "Prevoiusly changed", MessageBoxButton.OK, MessageBoxImage.Information);
                        _nat02Context.Dispose();
                    }
                }
                // New
                else
                {
                    EoiCalendar eoiCalendar = new EoiCalendar
                    {
                        Day = (byte)d.Day,
                        Month = (byte)d.Month,
                        Year = (Int16)d.Year,
                        DomainName = user.DomainName,
                        Notes = e.Day.Notes
                    };
                    _nat02Context.EoiCalendar.Add(eoiCalendar);
                    _nat02Context.SaveChanges();
                    _nat02Context.Dispose();
                }
            }

        }

        private void NextMonth()
        {
            if (cboMonth.SelectedIndex < 12)
            {
                cboMonth.SelectedIndex++;
            }
            else
            {
                cboMonth.SelectedIndex = 0;
                cboYear.SelectedIndex++;
            }
        }
        private void PreviousMonth()
        {
            if (cboMonth.SelectedIndex > 0)
            {
                cboMonth.SelectedIndex--;
            }
            else
            {
                cboMonth.SelectedIndex = 12;
                cboYear.SelectedIndex--;
            }
        }
        private void PreviousMonth_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            PreviousMonth();
        }

        private void NextMonth_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextMonth();
        }

        private void TodayButton_Click(object sender, RoutedEventArgs e)
        {
            MoveCalendarToToday();
        }

        private void RecurringEventButton_Click(object sender, RoutedEventArgs e)
        {
            RecurringEventWindow recurringEventWindow = new RecurringEventWindow(user)
            {
                Owner = this as Window
            };
            recurringEventWindow.ShowDialog();
            RefreshCalendar();
        }

        private void Window_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            int change = e.Delta / 120;
            if (e.Delta > 1)
            {
                for (int i = 0; i < change; i++)
                {
                    PreviousMonth();
                }
            }
            else
            {
                for(int i = 0; i < -change; i++)
                {
                    NextMonth();
                }
            }
        }
    }
}
