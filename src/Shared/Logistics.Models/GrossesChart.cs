namespace Logistics.Models;

public class GrossesChart<T> where T : IGrossChart
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
                TotalDistance = data.Distance;
            }
        }
    }

    public double TotalGross { get; private set; }
    public double TotalDistance { get; private set; }
}
