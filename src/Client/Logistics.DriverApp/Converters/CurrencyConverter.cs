using System.Globalization;

namespace Logistics.DriverApp.Converters;

public class CurrencyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return string.Format(culture, "{0:C}", decimalValue);
        }
        if (value is double doubleValue)
        {
            return string.Format(culture, "{0:C}", doubleValue);
        }
        if (value is int intValue)
        {
            return string.Format(culture, "{0:C}", intValue);
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
