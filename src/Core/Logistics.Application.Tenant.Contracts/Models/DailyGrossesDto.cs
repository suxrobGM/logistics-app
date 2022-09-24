namespace Logistics.Application.Contracts.Models;

public class DailyGrossesDto
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
                TotalIncome += day.Income;
                TotalDistance += day.Distance;
            }
        }
    }
    
    public double TotalIncome { get; private set; }
    public double TotalDistance { get; private set; }
}