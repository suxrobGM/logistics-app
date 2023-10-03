namespace Logistics.DriverApp.Models;

public static class PredefinedDateRanges
{
    public static readonly DateRange ThisWeek = GetThisWeek();
    public static readonly DateRange LastWeek = GetLastWeek();
    public static readonly DateRange ThisMonth = GetThisMonth();
    public static readonly DateRange LastMonth = GetLastMonth();
    public static readonly DateRange Past90Days = GetPast90Days();
    public static readonly DateRange ThisYear = GetThisYear();
    public static readonly DateRange LastYear = GetLastYear();
    
    private static DateRange GetThisWeek()
    {
        var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
        var end = start.AddDays(6);
        return new DateRange("This Week", start, end);
    }

    private static DateRange GetLastWeek()
    {
        var start = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday - 7);
        var end = start.AddDays(6);
        return new DateRange("Last Week", start, end);
    }

    private static DateRange GetThisMonth()
    {
        var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        var end = DateTime.Today;
        return new DateRange("This Month", start, end);
    }

    private static DateRange GetLastMonth()
    {
        var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRange("Last Month", start, end);
    }

    private static DateRange GetPast90Days()
    {
        var end = DateTime.Today;
        var start = end.AddDays(-90);
        return new DateRange("Past 90 Days", start, end);
    }

    private static DateRange GetThisYear()
    {
        var start = new DateTime(DateTime.Now.Year, 1, 1);
        var end = DateTime.Today;
        return new DateRange("This Year", start, end);
    }

    private static DateRange GetLastYear()
    {
        var start = new DateTime(DateTime.Now.Year - 1, 1, 1);
        var end = new DateTime(DateTime.Now.Year - 1, 12, 31);
        return new DateRange("Last Year", start, end);
    }
}
