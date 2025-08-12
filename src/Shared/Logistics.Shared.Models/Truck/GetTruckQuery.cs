namespace Logistics.Shared.Models;

public record GetTruckQuery
{
    public Guid? TruckOrDriverId { get; set; }
    public bool IncludeLoads { get; set; }
    public bool OnlyActiveLoads { get; set; }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>()
        {
            { "includeLoads", IncludeLoads.ToString() },
            { "onlyActiveLoads", OnlyActiveLoads.ToString() }
        };

        if (TruckOrDriverId.HasValue)
        {
            dict.Add("truckOrDriverId", TruckOrDriverId.Value.ToString());
        }

        return dict;
    }
}
