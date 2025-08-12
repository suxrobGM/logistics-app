namespace Logistics.DriverApp.Models;

public class DateRange
{
    public DateRange(string displayName, DateTime startDate, DateTime endDate)
    {
        DisplayName = displayName;
        StartDate = startDate;
        EndDate = endDate;
    }

    public string DisplayName { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public override string ToString()
    {
        return DisplayName;
    }
}
