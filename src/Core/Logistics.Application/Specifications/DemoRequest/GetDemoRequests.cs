using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;
using System.Linq.Expressions;

namespace Logistics.Application.Specifications;

public sealed class GetDemoRequests : BaseSpecification<DemoRequest>
{
    public GetDemoRequests(string? orderBy, int page, int pageSize, string? search = null)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = x =>
                x.Email.Contains(search) ||
                x.FirstName.Contains(search) ||
                x.LastName.Contains(search) ||
                x.Company.Contains(search);
        }

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
