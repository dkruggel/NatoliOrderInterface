using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows;
using System.Linq;

namespace NatoliOrderInterface
{
    class CheckBoxConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                double orderNumber = double.Parse(values[0].ToString());

                bool headerIsChecked = (bool)values[1];

                if (headerIsChecked)
                {
                    return true;
                }

                if (parameter.ToString() == "Quote")
                {
                    return ((App)Application.Current).selectedQuotes.Any(o => o.Item1 == orderNumber.ToString());
                }
                else if (parameter.ToString() == "Project")
                {
                    return ((App)Application.Current).selectedProjects.Any(o => o.Item1 == orderNumber.ToString());
                }
                else //if (parameter.ToString() == "Order")
                {
                    //MessageBox.Show(orderNumber.ToString());
                    return ((App)Application.Current).selectedOrders.Any(o => o.Item1 == orderNumber.ToString());
                }
            }
            catch (Exception ex)
            {
                IMethods.WriteToErrorLog("CheckBoxConverter", ex.Message, null);
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[] { Binding.DoNothing, Binding.DoNothing };
            throw new NotImplementedException();
        }
    }
}
