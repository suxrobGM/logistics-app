namespace Logistics.Application.Contracts.Models;

public class DailyGrossesDto
{
    private IEnumerable<DailyGrossDto> _dates = new List<DailyGrossDto>();

    public IEnumerable<DailyGrossDto> Dates
    {
        get => _dates;
        set
        {
            _dates = value;

            foreach (var day in _dates)
            {
                TotalIncome += day.Income;
                TotalDistance += day.Distance;
            }
        }
    }
    
    public double TotalIncome { get; private set; }
    public double TotalDistance { get; private set; }
}