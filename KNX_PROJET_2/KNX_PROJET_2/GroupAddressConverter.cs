using Knx.Falcon;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace KNX_PROJET_2.Converter
{
    /// <summary>
    /// Converts a <see cref="GroupAddress"/> to its string representation and vice versa
    /// </summary>
    public class GroupAddressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //string stringValue = value as string;
            //return new GroupAddress(System.Convert.ToString(stringValue));
           return GroupAddress.Parse((string)value);
        }
    }
}

