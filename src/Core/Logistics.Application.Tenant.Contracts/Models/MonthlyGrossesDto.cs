namespace Logistics.Application.Contracts.Models;

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
                TotalGross += month.Gross;
                TotalDistance += month.Distance;
            }
        }
    }
    
    public double TotalGross { get; private set; }
    public double TotalDistance { get; private set; }
}