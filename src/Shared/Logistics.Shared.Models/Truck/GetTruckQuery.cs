namespace Logistics.Shared.Models;

public class GetTruckQuery
{
    public string? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>()
        {
            { "includeLoads", IncludeLoads.ToString() },
            { "onlyActiveLoads", OnlyActiveLoads.ToString() }
        };

        if (!string.IsNullOrEmpty(TruckOrDriverId))
        {
            dict.Add("truckOrDriverId", TruckOrDriverId);
        }

        return dict;
    }
}
