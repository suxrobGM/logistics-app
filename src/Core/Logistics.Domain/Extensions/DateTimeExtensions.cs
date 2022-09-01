namespace Logistics.Domain.Extensions;

public static class DateTimeExtensions
{
    public static DateOnly? ToDateOnly(this DateTime? dateTime)
    {
        if (dateTime.HasValue)
            return DateOnly.FromDateTime(dateTime.Value);

        return null;
    }

    public static DateOnly ToDateOnly(this DateTime dateTime)
    {
        return DateOnly.FromDateTime(dateTime);
    }
}