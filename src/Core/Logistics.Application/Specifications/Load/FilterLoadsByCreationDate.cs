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
                Criteria = l =>
                    l.CreatedAt >= startDate;
            }
            if (endDate.HasValue && endDate != default)
            {
                Criteria = l =>
                    l.CreatedAt <= endDate;
            }
        }
    }
}
