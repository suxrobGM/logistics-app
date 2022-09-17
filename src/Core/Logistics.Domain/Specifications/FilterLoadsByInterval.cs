namespace Logistics.Domain.Specifications;

public class FilterLoadsByInterval : BaseSpecification<Load>
{
    public FilterLoadsByInterval(DateOnly startPeriod, DateOnly endPeriod)
    {
        Criteria = i =>
            i.DeliveryDate.HasValue && i.DeliveryDate >= startPeriod && i.DeliveryDate <= endPeriod;

        OrderBy = i => i.DeliveryDate!;
    }
}