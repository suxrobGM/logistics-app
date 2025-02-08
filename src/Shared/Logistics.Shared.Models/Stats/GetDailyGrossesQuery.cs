namespace Logistics.Shared.Models;

public class GetDailyGrossesQuery : IntervalQuery
{
    public string? TruckId { get; set; }
    public string? UserId { get; set; }
    
    public override IDictionary<string, string> ToDictionary()
    {
        var dict = base.ToDictionary();

        if (!string.IsNullOrEmpty(TruckId))
        {
            dict.Add("truckId", TruckId);
        }
        if (!string.IsNullOrEmpty(UserId))
        {
            dict.Add("userId", UserId);
        }

        return dict;
    }
}
