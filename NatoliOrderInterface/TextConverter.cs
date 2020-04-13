using NatoliOrderInterface.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace NatoliOrderInterface
{
    class TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            EoiAllTabletProjectsView project = value as EoiAllTabletProjectsView;
            if ((bool)project.Tools) { return FontStyles.Oblique; }
            return FontStyles.Normal;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
