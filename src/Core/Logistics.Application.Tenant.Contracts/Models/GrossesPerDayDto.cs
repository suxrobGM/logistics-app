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
            TotalGross = _days.Sum(i => i.Gross);
        }
    }
    
    public decimal TotalGross { get; private set; }
}

public record DailyGross(DateTime Day, decimal Gross);