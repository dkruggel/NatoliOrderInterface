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
using NatoliOrderInterface.Models;

namespace NatoliOrderInterface
{
    /// <summary>
    /// Interaction logic for RecurringEventWindow.xaml
    /// </summary>
    public partial class RecurringEventWindow : Window
    {
        public string Notes = "";
        public byte? Period = null;
        public DateTime? startDate = null;
        public DateTime? endDate = null;
        private User user;

        public enum PeriodUnit 
        { 
            Day,
            Week,
            Month,
            Year
        };
        public PeriodUnit Unit = PeriodUnit.Week;
        public RecurringEventWindow(User user)
        {
            InitializeComponent();
            this.user = user;
        }

        private void AddNoteToDate(DateTime dateTime, string note)
        {
            using var _nat02Context = new NAT02Context();
            try
            {
                // Exists
                if (_nat02Context.EoiCalendar.Any(c => c.Year == dateTime.Year && c.Month == dateTime.Month && c.Day == dateTime.Day))
                {
                    EoiCalendar eoiCalendar = _nat02Context.EoiCalendar.First(c => c.Year == dateTime.Year && c.Month == dateTime.Month && c.Day == dateTime.Day);
                    eoiCalendar.Notes += System.Environment.NewLine + note;
                    eoiCalendar.DomainName = user.DomainName;
                    _nat02Context.SaveChanges();
                }
                else
                {
                    EoiCalendar eoiCalendar = new EoiCalendar
                    {
                        Year = (Int16)dateTime.Year,
                        Month = (byte)dateTime.Month,
                        Day = (byte)dateTime.Day,
                        DomainName = user.DomainName,
                        Notes = note
                    };
                    _nat02Context.EoiCalendar.Add(eoiCalendar);
                    _nat02Context.SaveChanges();

                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error processing your request. It may have been parially processed" + System.Environment.NewLine + ex.Message,"Error",MessageBoxButton.OK,MessageBoxImage.Error);
                IMethods.WriteToErrorLog("RecurringEventWindow => AddNoteToDate => Date: " + dateTime.ToLongDateString() + " => Note: " + note, ex.Message, user);
            }
            _nat02Context.Dispose();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (startDate != null && endDate != null && !string.IsNullOrEmpty(Notes) && Period != null)
                {
                    switch (Unit)
                    {
                        case PeriodUnit.Day:
                            {
                                DateTime day = (DateTime)startDate;
                                while (day <= endDate)
                                {
                                    AddNoteToDate(day, Notes);
                                    day = day.AddDays(Convert.ToDouble(Period));
                                }
                                break;
                            }
                        case PeriodUnit.Week:
                            {
                                DateTime day = (DateTime)startDate;
                                while (day <= endDate)
                                {
                                    AddNoteToDate(day, Notes);
                                    day = day.AddDays(Convert.ToDouble(Period * 7));
                                }
                                break;
                            }
                        case PeriodUnit.Month:
                            {
                                DateTime day = (DateTime)startDate;
                                while (day <= endDate)
                                {
                                    AddNoteToDate(day, Notes);
                                    day = day.AddMonths(Convert.ToInt32(Period));
                                }
                                break;
                            }
                        case PeriodUnit.Year:
                            {
                                DateTime day = (DateTime)startDate;
                                while (day <= endDate)
                                {
                                    AddNoteToDate(day, Notes);
                                    day = day.AddYears(Convert.ToInt32(Period));
                                }
                                break;
                            }
                    }
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Please ensure that all required values are entered.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error processing your request. It may have been parially processed" + System.Environment.NewLine + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                IMethods.WriteToErrorLog("RecurringEventWindow => AddButton_Click" , ex.Message, user);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PeriodUnits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)e.AddedItems[0];
            switch (comboBoxItem.Content.ToString())
            {
                case "Week(s)":
                    {
                        Unit = PeriodUnit.Week;
                        break;
                    }
                case "Month(s)":
                    {
                        Unit = PeriodUnit.Month;
                        break;
                    }
                case "Year(s)":
                    {
                        Unit = PeriodUnit.Year;
                        break;
                    }
                case "Day(s)":
                    {
                        Unit = PeriodUnit.Day;
                        break;
                    }
            }
        }

        private void Period_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBoxItem = (ComboBoxItem)e.AddedItems[0];
            
            if (byte.TryParse(comboBoxItem.Content.ToString(), out byte result))
            {
                Period = result;
            }
        }

        private void Notes_TextChanged(object sender, TextChangedEventArgs e)
        {
            Notes = NotesTextBox.Text;
        }

        private void StartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            startDate = StartDateDatePicker.SelectedDate.Value;
        }

        private void EndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            endDate = EndDateDatePicker.SelectedDate.Value;
        }
    }
}
