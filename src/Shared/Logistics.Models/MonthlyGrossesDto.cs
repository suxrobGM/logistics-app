namespace Logistics.Models;

public class MonthlyGrossesDto
{
    private IEnumerable<MonthlyGrossDto> _data = new List<MonthlyGrossDto>();

    public IEnumerable<MonthlyGrossDto> Data
    {
        get => _data;
        set
        {
            _data = value;

            foreach (var monthlyGross in _data)
            {
                TotalIncome += monthlyGross.Income;
                TotalDistance += monthlyGross.Distance;
            }
        }
    }
    
    public double TotalIncome { get; private set; }
    public double TotalDistance { get; private set; }
}
