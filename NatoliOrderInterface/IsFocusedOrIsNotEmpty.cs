using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Linq;

namespace NatoliOrderInterface
{
    public class IsFocusedOrIsNotEmpty : IMultiValueConverter
    {
        enum Types
        {
            /// <summary>
            /// True to Visible, False to Collapsed
            /// </summary>
            t2v_f2c,
            /// <summary>
            /// True to Visible, False to Hidden
            /// </summary>
            t2v_f2h,
            /// <summary>
            /// True to Collapsed, False to Visible
            /// </summary>
            t2c_f2v,
            /// <summary>
            /// True to Hidden, False to Visible
            /// </summary>
            t2h_f2v,
        }
        /// <summary>
        /// values must be (boolean,string)
        /// </summary>
        /// <param name="values"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool b = false;
            foreach (object value in values)
            {
                if (value.GetType() == Type.GetType("System.Boolean") && (bool)value == true)
                {
                    b = true;
                }
                if (value.GetType() == Type.GetType("System.String") && value.ToString().Length>0)
                {
                    b = true;
                }
            }
            string p = (string)parameter;
            var type = (Types)Enum.Parse(typeof(Types), (string)parameter);
            switch (type)
            {
                case Types.t2v_f2c:
                    return b ? Visibility.Visible : Visibility.Collapsed;
                case Types.t2v_f2h:
                    return b ? Visibility.Visible : Visibility.Hidden;
                case Types.t2c_f2v:
                    return b ? Visibility.Collapsed : Visibility.Visible;
                case Types.t2h_f2v:
                    return b ? Visibility.Hidden : Visibility.Visible;
                default:
                    throw new NotImplementedException();
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
            throw new NotImplementedException();
        }
    }
}
