namespace Logistics.Shared.Models;

public class GrossesDto<T> where T : IGrossChartData
{
    public IEnumerable<T> Data
    {
        get;
        set
        {
            field = value;
            foreach (var data in field)
            {
                TotalGross += data.Gross;
                TotalDistance += data.Distance;
            }
        }
    } = [];

    public decimal TotalGross { get; private set; }
    public double TotalDistance { get; private set; }
}
