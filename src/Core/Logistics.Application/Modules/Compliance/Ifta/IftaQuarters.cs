namespace Logistics.Application.Modules.Compliance.Ifta;

/// <summary>
/// IFTA calendar-quarter arithmetic shared by the report service, quarter-close job, and
/// filing-reminder job.
/// </summary>
public static class IftaQuarters
{
    /// <summary>Inclusive UTC start of the quarter.</summary>
    public static DateTime StartOf(int year, int quarter) =>
        new(year, (quarter - 1) * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>Exclusive UTC end of the quarter.</summary>
    public static DateTime EndOf(int year, int quarter) =>
        StartOf(year, quarter).AddMonths(3);

    public static (int Year, int Quarter) Current(DateTime utcNow) =>
        (utcNow.Year, (utcNow.Month - 1) / 3 + 1);

    public static (int Year, int Quarter) Previous(DateTime utcNow)
    {
        var (year, quarter) = Current(utcNow);
        return quarter == 1 ? (year - 1, 4) : (year, quarter - 1);
    }

    /// <summary>
    /// IFTA filing deadline for the quarter: the last day of the month following quarter end
    /// (Apr 30, Jul 31, Oct 31, Jan 31).
    /// </summary>
    public static DateTime FilingDeadline(int year, int quarter)
    {
        var monthAfterEnd = EndOf(year, quarter);
        return monthAfterEnd.AddMonths(1).AddDays(-1);
    }
}
