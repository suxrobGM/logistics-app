using System.Globalization;

namespace Logistics.DriverApp.Converters;

public class DistanceConverter : IValueConverter
{
    private const double MetersToMiles = 0.000621371;
    private const double MetersToKilometers = 0.001;
    private const double MetersToYards = 1.09361;

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double meters)
            throw new ArgumentException("Expected value to be a double representing meters");

        var conversion = parameter as string;

        return conversion?.ToLower() switch
        {
            "mi" => Math.Round(meters * MetersToMiles, 2),
            "km" => Math.Round(meters * MetersToKilometers, 2),
            "yd" => Math.Round(meters * MetersToYards, 2),
            _ => Math.Round(meters, 2),
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double targetValue)
            throw new ArgumentException("Expected value to be a double");

        var conversion = parameter as string;

        return conversion?.ToLower() switch
        {
            "mi" => Math.Round(targetValue / MetersToMiles, 2),
            "km" => Math.Round(targetValue / MetersToKilometers, 2),
            "yd" => Math.Round(targetValue / MetersToYards, 2),
            _ => Math.Round(targetValue, 2)
        };
    }
}
