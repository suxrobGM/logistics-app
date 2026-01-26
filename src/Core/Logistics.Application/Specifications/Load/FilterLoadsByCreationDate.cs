using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications
{
    public class FilterLoadsByCreationDate : BaseSpecification<Load>
    {
        public FilterLoadsByCreationDate(DateTime? startDate, DateTime? endDate)
        {
            var hasStart = startDate.HasValue && startDate != default;
            var hasEnd = endDate.HasValue && endDate != default;
            var from = hasStart ? DateTime.SpecifyKind(startDate!.Value, DateTimeKind.Utc) : (DateTime?)null;
            var to = hasEnd ? DateTime.SpecifyKind(endDate!.Value, DateTimeKind.Utc) : (DateTime?)null;

            Criteria = (hasStart, hasEnd) switch
            {
                (true, true) => l => l.CreatedAt >= from && l.CreatedAt <= to,
                (true, false) => l => l.CreatedAt >= from,
                (false, true) => l => l.CreatedAt <= to,
                _ => _ => true
            };
        }
    }
}
