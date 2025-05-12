using System.Linq.Expressions;
using Logistics.Domain.Entities;

namespace Logistics.Domain.Specifications;

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
        
        ApplyOrderBy("DeliveryDate");
    }

    protected override Expression<Func<Load, object?>> CreateOrderByExpression(string propertyName)
    {
        return i => i.DeliveryDate;
    }
}
