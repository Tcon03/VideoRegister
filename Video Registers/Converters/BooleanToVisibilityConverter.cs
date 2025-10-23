using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Video_Registers.Converters
{
    public class BooleanToVisibilityConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                //true thì hiển thị ra 
                return Visibility.Visible;
            }
            return Visibility.Collapsed;

        }
    }
}
