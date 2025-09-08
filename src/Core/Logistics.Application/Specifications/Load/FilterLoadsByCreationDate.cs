using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications
{
    public class FilterLoadsByCreationDate : BaseSpecification<Load>
    {
        public FilterLoadsByCreationDate(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && startDate != default)
            {
                var from = DateTime.SpecifyKind(startDate.Value, DateTimeKind.Utc);
                Criteria = l =>
                    l.CreatedAt >= from;
            }
            if (endDate.HasValue && endDate != default)
            {
                var to = DateTime.SpecifyKind(endDate.Value, DateTimeKind.Utc);
                Criteria = l =>
                    l.CreatedAt <= to;
            }
        }
    }
}
