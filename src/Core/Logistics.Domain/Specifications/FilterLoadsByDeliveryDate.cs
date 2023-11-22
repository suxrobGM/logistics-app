using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

public class FilterLoadsByDeliveryDate : BaseSpecification<Load>
{
    public FilterLoadsByDeliveryDate(
        string? truckId,
        DateTime startPeriod,
        DateTime endPeriod)
    {
        if (!string.IsNullOrEmpty(truckId))
        {
            Criteria = i =>
                i.AssignedTruckId == truckId &&
                i.DeliveryDate.HasValue && 
                i.DeliveryDate >= startPeriod && 
                i.DeliveryDate <= endPeriod;
        }
        else
        {
            Criteria = i =>
                i.DeliveryDate.HasValue && 
                i.DeliveryDate >= startPeriod && 
                i.DeliveryDate <= endPeriod;
        }
        
        ApplyOrderBy(i => i.DeliveryDate);
    }
}
