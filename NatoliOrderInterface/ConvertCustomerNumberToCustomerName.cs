using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using NatoliOrderInterface.Models.NEC;

namespace NatoliOrderInterface
{
    class ConvertCustomerNumberToCustomerName : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string customerNumber = value.ToString().Trim();
            if (customerNumber.Length > 2)
            {
                using var _necContext = new NECContext();
                if (_necContext.Rm00101.Any(c => c.Custnmbr != null && c.Custnmbr.Trim() == customerNumber))
                {
                    string customerName = _necContext.Rm00101.First(c => c.Custnmbr != null && c.Custnmbr.Trim() == customerNumber).Custname.Trim();
                    _necContext.Dispose();
                    return customerName;
                }
                else
                {
                    _necContext.Dispose();
                    return "";
                }
            }
            else
            {
                return "";
            }

            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
