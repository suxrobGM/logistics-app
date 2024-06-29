using System.Globalization;

namespace Logistics.HttpClient.Models;

public abstract class IntervalQuery
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public virtual IDictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { "startDate", StartDate.ToString(CultureInfo.InvariantCulture) },
            { "endDate", EndDate.ToString(CultureInfo.InvariantCulture) }
        };
    }
}
