namespace Logistics.Domain.Primitives;

public static class DateTimeExtensions
{
    extension(DateTime date)
    {
        /// <summary>Monday 00:00 of the ISO 8601 week <paramref name="date" /> falls in.</summary>
        public DateTime GetIsoWeekStart()
        {
            var day = date.Date;
            var daysSinceMonday = day.DayOfWeek == DayOfWeek.Sunday ? 6 : (int)day.DayOfWeek - 1;
            return day.AddDays(-daysSinceMonday);
        }
    }

    extension(DateTime startDate)
    {
        public IEnumerable<(int Year, int Month)> MonthsBetween(
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

        public IEnumerable<(int Year, int Month, int Day)> DaysBetween(
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

}
