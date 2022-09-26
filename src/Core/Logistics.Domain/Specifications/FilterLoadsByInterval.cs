namespace Logistics.Domain.Specifications;

public class FilterLoadsByInterval : BaseSpecification<Load>
{
    public FilterLoadsByInterval(string? truckId, DateTime startPeriod, DateTime endPeriod)
    {
        if (!string.IsNullOrEmpty(truckId))
        {
            Criteria = i =>
                i.AssignedTruckId == truckId &&
                i.DeliveryDate.HasValue && i.DeliveryDate >= startPeriod && i.DeliveryDate <= endPeriod;
        }
        else
        {
            Criteria = i =>
                i.DeliveryDate.HasValue && i.DeliveryDate >= startPeriod && i.DeliveryDate <= endPeriod;
        }

        OrderBy = i => i.DeliveryDate!;
    }
}