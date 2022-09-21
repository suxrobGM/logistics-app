namespace Logistics.Application.Contracts.Models;

public class GrossesForIntervalDto
{
    private IEnumerable<DailyGrossDto> _days = new List<DailyGrossDto>();

    public IEnumerable<DailyGrossDto> Days
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
    
    public double TotalGross { get; private set; }
    public double TotalDistance { get; private set; }
}