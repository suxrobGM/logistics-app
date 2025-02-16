namespace Logistics.DbMigrator.Core;

public static class DateRangeGenerator
{
    public static List<(DateTime StartDate, DateTime EndDate)> GenerateMonthlyRanges(DateTime startDate, DateTime endDate)
    {
        var ranges = new List<(DateTime StartDate, DateTime EndDate)>();
        var currentStartDate = new DateTime(startDate.Year, startDate.Month, 1);
        currentStartDate = DateTime.SpecifyKind(currentStartDate, DateTimeKind.Utc);
        
        while (currentStartDate <= endDate)
        {
            var currentEndDate = currentStartDate.AddMonths(1).AddDays(-1);
            currentEndDate = DateTime.SpecifyKind(currentEndDate, DateTimeKind.Utc);
            
            if (currentEndDate > endDate)
                currentEndDate = endDate;

            ranges.Add((currentStartDate, currentEndDate));
            currentStartDate = currentStartDate.AddMonths(1);
        }

        return ranges;
    }

    public static List<(DateTime StartDate, DateTime EndDate)> GenerateWeeklyRanges(DateTime startDate, DateTime endDate)
    {
        var ranges = new List<(DateTime StartDate, DateTime EndDate)>();
        var currentStartDate = startDate.AddDays(-(int)startDate.DayOfWeek); // Week starting from Sunday
        currentStartDate = DateTime.SpecifyKind(currentStartDate, DateTimeKind.Utc);
        
        while (currentStartDate < endDate)
        {
            var currentEndDate = currentStartDate.AddDays(6);
            currentEndDate = DateTime.SpecifyKind(currentEndDate, DateTimeKind.Utc);
            
            if (currentEndDate > endDate)
                currentEndDate = endDate;

            ranges.Add((currentStartDate, currentEndDate));
            currentStartDate = currentEndDate.AddDays(1);
        }

        return ranges;
    }
}
