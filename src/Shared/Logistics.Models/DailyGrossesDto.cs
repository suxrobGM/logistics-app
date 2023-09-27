namespace Logistics.Models;

public class DailyGrossesDto
{
    private IEnumerable<DailyGrossDto> _data = new List<DailyGrossDto>();

    public IEnumerable<DailyGrossDto> Data
    {
        get => _data;
        set
        {
            _data = value;

            foreach (var dailyGross in _data)
            {
                TotalIncome += dailyGross.Income;
                TotalDistance += dailyGross.Distance;
            }
        }
    }
    
    public double TotalIncome { get; private set; }
    public double TotalDistance { get; private set; }
}
