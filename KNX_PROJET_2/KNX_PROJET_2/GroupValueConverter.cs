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
    /// Converts a <see cref="GroupValue"/> to its string representation and vice versa
    /// </summary>
    public class GroupValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string stringValue = value as string;

            if (stringValue == null)
            {
                throw new InvalidOperationException("Value must be a string.");
            }

            if (stringValue.Length == 1 && (stringValue == "0" || stringValue == "1"))
            {
                return new GroupValue(System.Convert.ToByte(stringValue, 16), 6);
            }
            else if (stringValue.Length == 2)
            {
                return new GroupValue(System.Convert.ToByte(stringValue, 16), 8);
            }
            else if (stringValue.Length == 3)
            {
                return new GroupValue(System.Convert.ToByte(stringValue), 8);
            }

            throw new InvalidOperationException("Value must be a string of length 1, 2, or 3.");

            //return GroupValue.Parse((string)value);
        }
    }
}
