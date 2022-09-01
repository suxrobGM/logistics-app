namespace Logistics.Domain.Extensions;

public static class DateOnlyExtensions
{
    public static DateTime? ToDateTime(this DateOnly? date)
    {
        return date?.ToDateTime(TimeOnly.Parse("12:00 AM"));
    }
    
    public static DateTime ToDateTime(this DateOnly date)
    {
        return date.ToDateTime(TimeOnly.Parse("12:00 AM"));
    }
}