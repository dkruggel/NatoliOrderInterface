using NatoliOrderInterface.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;

namespace NatoliOrderInterface
{
    class TextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(FontStyle))
            {
                if (value is EoiAllTabletProjectsView)
                {
                    EoiAllTabletProjectsView project = value as EoiAllTabletProjectsView;
                    if ((bool)project.Tools)
                    {
                        return FontStyles.Oblique;
                    }
                    else
                    {
                        return FontStyles.Normal;
                    }
                }
                else
                {
                    return FontStyles.Normal;
                }
            }
            else if(targetType == typeof(FontWeight))
            {
                if(value is bool?)
                {
                    bool? isChecked = value as bool?;
                    if (isChecked == true)
                    {
                        return FontWeights.SemiBold;
                    }
                    else
                    {
                        return FontWeights.Normal;
                    }
                }
                else
                { 
                    return FontWeights.Normal; 
                }
            }
            else
            {
                return null;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
