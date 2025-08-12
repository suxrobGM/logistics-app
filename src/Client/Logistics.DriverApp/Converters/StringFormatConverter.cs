using System.Globalization;

namespace Logistics.DriverApp.Converters;

public class StringFormatConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var formatString = TryGetFormatValue(parameter);
        if (!string.IsNullOrEmpty(formatString))
        {
            return string.Format(culture, formatString, value);
        }
        return value?.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    public string? TryGetFormatValue(object? parameter)
    {
        if (parameter is string strValue)
        {
            return strValue;
        }

        if (parameter is Binding bindingProperty && bindingProperty.Source is StatsPage statsPage)
        {
            return statsPage.ViewModel.DateFormat;
        }

        return default;
    }
}
