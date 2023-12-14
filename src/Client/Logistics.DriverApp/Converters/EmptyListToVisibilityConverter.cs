using System.Collections;
using System.Globalization;

namespace Logistics.DriverApp.Converters;

public class EmptyListToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is IEnumerable collection && !collection.Cast<object>().Any();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
