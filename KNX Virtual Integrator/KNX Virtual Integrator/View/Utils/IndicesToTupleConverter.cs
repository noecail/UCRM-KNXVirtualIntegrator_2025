using System.Globalization;
using System.Windows.Data;

namespace KNX_Virtual_Integrator.View.Utils;

public class IndicesToTupleConverter : IMultiValueConverter
{
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

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}