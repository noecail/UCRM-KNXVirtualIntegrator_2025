using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace KNX_Virtual_Integrator.View.Utils
{
    /// <summary>
    /// Converter class used to convert between BigInteger type and String type
    /// BigIntegers are needed in the software as they are the type that KNX GroupValue uses
    /// </summary>
    public class BigIntegerToStringConverter : IValueConverter
    {
        /// <summary>
        /// Convert from BigInteger to String
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? "";
        }

        /// <summary>
        /// Convert from String to BigInteger
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (BigInteger.TryParse(value?.ToString(), out var result))
                return result;

            return BigInteger.Zero; // ou return Binding.DoNothing si tu préfères ne pas modifier
        }
    }
}

