namespace Logistics.Application.Tenant.Models;

public class MonthlyGrossesDto
{
    private IEnumerable<MonthlyGrossDto> _months = new List<MonthlyGrossDto>();

    public IEnumerable<MonthlyGrossDto> Months
    {
        get => _months;
        set
        {
            _months = value;

            foreach (var month in _months)
            {
                TotalIncome += month.Income;
                TotalDistance += month.Distance;
            }
        }
    }
    
    public double TotalIncome { get; private set; }
    public double TotalDistance { get; private set; }
}