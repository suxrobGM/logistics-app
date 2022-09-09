namespace Logistics.Domain.Specifications;

public class FilterLoadsForPeriod : BaseSpecification<Load>
{
    public FilterLoadsForPeriod(DateOnly startPeriod, DateOnly endPeriod)
    {
        Criteria = i =>
            i.DeliveryDate.HasValue && i.DeliveryDate >= startPeriod && i.DeliveryDate <= endPeriod;

        OrderBy = i => i.DeliveryDate!;
    }
}