namespace Logistics.Domain.Extensions;

public static class DateTimeExtensions
{
    public static IEnumerable<(int Year, int Month)> MonthsBetween(
        this DateTime startDate,
        DateTime endDate)
    {
        DateTime iterator;
        DateTime limit;

        if (endDate > startDate)
        {
            iterator = new DateTime(startDate.Year, startDate.Month, 1);
            limit = endDate;
        }
        else
        {
            iterator = new DateTime(endDate.Year, endDate.Month, 1);
            limit = startDate;
        }

        while (iterator <= limit)
        {
            yield return (iterator.Year, iterator.Month);
            iterator = iterator.AddMonths(1);
        }
    }

    public static IEnumerable<(int Year, int Month, int Day)> DaysBetween(
        this DateTime startDate,
        DateTime endDate)
    {
        var iterator = (endDate > startDate ? startDate : endDate).Date;
        var limit = (endDate > startDate ? endDate : startDate).Date;

        while (iterator <= limit)
        {
            yield return (iterator.Year, iterator.Month, iterator.Day);
            iterator = iterator.AddDays(1);
        }
    }
}
