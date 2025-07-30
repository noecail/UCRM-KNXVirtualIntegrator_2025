using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace KNX_Virtual_Integrator.View.Utils
{
    public class BigIntegerToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (BigInteger.TryParse(value?.ToString(), out var result))
                return result;

            return BigInteger.Zero; // ou return Binding.DoNothing si tu préfères ne pas modifier
        }
    }
}

