using System.Globalization;

namespace Logistics.DriverApp.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            decimal decimalValue => string.Format(culture, "{0:C}", decimalValue),
            double doubleValue => string.Format(culture, "{0:C}", doubleValue),
            int intValue => string.Format(culture, "{0:C}", intValue),
            _ => value
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
