using System.Globalization;

namespace Logistics.Shared.Models;

public class GetLoadsQuery : SearchableQuery
{
    public string? TruckId { get; set; }
    public bool? FilterActiveLoads { get; set; }
    public bool? LoadAllPages { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public override IDictionary<string, string> ToDictionary()
    {
        var queryDict = base.ToDictionary();

        if (!string.IsNullOrEmpty(TruckId))
        {
            queryDict.Add("truckId", TruckId);
        }
        if (StartDate.HasValue && EndDate.HasValue)
        {
            queryDict.Add("startDate", StartDate.Value.ToString(CultureInfo.InvariantCulture));
            queryDict.Add("endDate", EndDate.Value.ToString(CultureInfo.InvariantCulture));
        }
        if (LoadAllPages.HasValue)
        {
            queryDict.Add("loadAllPages", LoadAllPages.Value.ToString());
        }
        if (FilterActiveLoads.HasValue)
        {
            queryDict.Add("filterActiveLoads", FilterActiveLoads.Value.ToString());
        }

        return queryDict;
    }
}
