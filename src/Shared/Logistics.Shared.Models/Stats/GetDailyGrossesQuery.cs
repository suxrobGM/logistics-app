namespace Logistics.Shared.Models;

public class GetDailyGrossesQuery : IntervalQuery
{
    public Guid? TruckId { get; set; }
    public Guid? UserId { get; set; }

    public override IDictionary<string, string> ToDictionary()
    {
        var dict = base.ToDictionary();

        if (TruckId.HasValue)
        {
            dict.Add("truckId", TruckId.Value.ToString());
        }
        if (UserId.HasValue)
        {
            dict.Add("userId", UserId.Value.ToString());
        }

        return dict;
    }
}
