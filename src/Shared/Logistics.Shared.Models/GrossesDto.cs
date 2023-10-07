namespace Logistics.Shared.Models;

public class GrossesDto<T> where T : IGrossChartData
{
    private IEnumerable<T> _data = new List<T>();

    public IEnumerable<T> Data
    {
        get => _data;
        set
        {
            _data = value;
            foreach (var data in _data)
            {
                TotalGross += data.Gross;
                TotalDistance += data.Distance;
            }
        }
    }

    public decimal TotalGross { get; private set; }
    public double TotalDistance { get; private set; }
}
