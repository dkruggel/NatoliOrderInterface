using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace NatoliOrderInterface
{
    class ModuleHeightConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //int index = (value as MainWindow).User.VisiblePanels.IndexOf(parameter.ToString());
            //var x = (value as MainWindow).MainWrapPanel.Children.OfType<Grid>().ToList()[index].Children[0] as Label;
            //x.ApplyTemplate();
            //ListBox listBox = (VisualTreeHelper.GetChild(x, 0) as Grid).Children.OfType<Grid>().First().Children.OfType<ListBox>().First();
            //int count = listBox.Items.Count;
            //if (count == 0)
            //{
            //    return 100;
            //}
            //else
            //{
            //    if (count < 11)
            //    {
            //        return 35 * count + 100;
            //    }
            //    else
            //    {
            //        return 500;
            //    }
            //}
            return 500;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
