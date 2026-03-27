namespace Logistics.Domain.Primitives;

public static class DateTimeHelpers
{
    /// <summary>
    /// Returns the start of the current ISO 8601 week (Monday 00:00 UTC).
    /// </summary>
    public static DateTime GetCurrentIsoWeekStart()
    {
        var today = DateTime.UtcNow.Date;
        var dayOfWeek = (int)today.DayOfWeek;
        // ISO weeks start on Monday (DayOfWeek.Sunday = 0, Monday = 1)
        var daysToSubtract = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return today.AddDays(-daysToSubtract);
    }
}
