namespace Logistics.Application.Contracts.Models;

public class GrossesPerDayDto
{
    private IEnumerable<DailyGross> _days = new List<DailyGross>();

    public IEnumerable<DailyGross> Days
    {
        get => _days;
        set
        {
            _days = value;

            foreach (var day in _days)
            {
                TotalGross += day.Gross;
                TotalDistance += day.Distance;
            }
        }
    }
    
    public decimal TotalGross { get; private set; }
    public double TotalDistance { get; private set; }
}

public record DailyGross(DateTime Date)
{
    public decimal Gross { get; set; }
    public double Distance { get; set; }
}