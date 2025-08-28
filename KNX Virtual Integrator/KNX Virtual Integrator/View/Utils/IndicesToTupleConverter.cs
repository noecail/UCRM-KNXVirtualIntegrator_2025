using System.Globalization;
using System.Windows.Data;

namespace KNX_Virtual_Integrator.View.Utils;

/// <summary>
/// Converter class used to convert from two Int values to a Tuple
/// Specifically used in the Structure Edit Window, delete DPT button, to pass both the element structure's index and the to-be-deleted DPT's index
/// </summary>
public class IndicesToTupleConverter : IMultiValueConverter
{
    /// <summary>
    /// Convert from the two Int values to the Tuple
    /// </summary>
    /// <param name="values"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 2 
            && values[0] is int parentIndex 
            && values[1] is int childIndex)
        {
            return Tuple.Create(parentIndex, childIndex);
        }
        return Binding.DoNothing;
    }

    /// <summary>
    /// Convert from the Tuple to the two Int values
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetTypes"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}