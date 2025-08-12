using System.Globalization;

namespace Logistics.Shared.Models;

public class IntervalQuery
{
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; } = DateTime.UtcNow;

    public virtual IDictionary<string, string> ToDictionary()
    {
        return new Dictionary<string, string>
        {
            { "startDate", StartDate.ToString(CultureInfo.InvariantCulture) },
            { "endDate", EndDate.ToString(CultureInfo.InvariantCulture) }
        };
    }
}
