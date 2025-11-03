using System.Globalization;

namespace Logistics.DriverApp.Converters;

public class ShortAddressConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string address)
            throw new ArgumentException("Expected value to be a string type");

        return string.IsNullOrEmpty(address) ? string.Empty : address[..address.LastIndexOf(',')];
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
