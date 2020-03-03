using NatoliOrderInterface.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;
using System.Windows.Media;
using System.Linq;

namespace NatoliOrderInterface
{
    class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double orderNumber = double.Parse(value.ToString());
            SolidColorBrush solidColorBrush;
            using var _nat02Context = new NAT02Context();
            if (_nat02Context.EoiAllOrdersView.Any(o => o.OrderNumber == orderNumber))
            {
                EoiAllOrdersView order = _nat02Context.EoiAllOrdersView.Single(o => o.OrderNumber == orderNumber);

                if (order.BeingEntered == 1) { return new SolidColorBrush(Colors.Transparent); }

                if (order.InTheOffice == 1)
                {
                    if (order.DoNotProcess == 1)
                    {
                        solidColorBrush = new SolidColorBrush(Colors.Pink);
                        solidColorBrush.Opacity = 0.65;
                        return solidColorBrush;
                    }
                }

                if (order.EnteredUnscanned == 1)
                {
                    if (order.DoNotProcess == 1)
                    {
                        solidColorBrush = new SolidColorBrush(Colors.Pink);
                        solidColorBrush.Opacity = 0.65;
                        return solidColorBrush;
                    }
                    if ((order.ProcessState == "Failed" && order.ProcessState != "Complete") || order.TransitionName == "NeedInfo") {
                        solidColorBrush = new SolidColorBrush(Colors.DarkGray);
                        solidColorBrush.Opacity = 0.65;
                        return solidColorBrush;
                    }
                }

                if (order.InEngineering == 1)
                {
                    if (order.DoNotProcess == 1)
                    {
                        solidColorBrush = new SolidColorBrush(Colors.Pink);
                        solidColorBrush.Opacity = 0.65;
                        return solidColorBrush;
                    }
                    if (order.BeingChecked == 1) {
                        solidColorBrush = new SolidColorBrush(Colors.DodgerBlue);
                        solidColorBrush.Opacity = 0.65;
                        return solidColorBrush;
                    }
                    if (order.MarkedForChecking == 1) {
                        solidColorBrush = new SolidColorBrush(Colors.GreenYellow);
                        solidColorBrush.Opacity = 0.65;
                        return solidColorBrush;
                    }
                }
            }
            _nat02Context.Dispose();

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;

            throw new NotImplementedException();
        }
    }
}
