using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class FilterLoadsByDeliveryDate : BaseSpecification<Load>
{
    public FilterLoadsByDeliveryDate(
        Guid? truckId,
        DateTime startPeriod,
        DateTime endPeriod)
    {
        if (truckId.HasValue)
        {
            Criteria = i =>
                i.AssignedTruckId == truckId &&
                i.DeliveredAt.HasValue &&
                i.DeliveredAt >= startPeriod &&
                i.DeliveredAt <= endPeriod;
        }
        else
        {
            Criteria = i =>
                i.DeliveredAt.HasValue &&
                i.DeliveredAt >= startPeriod &&
                i.DeliveredAt <= endPeriod;
        }
    }
}
