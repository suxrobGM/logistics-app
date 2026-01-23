using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public sealed class GetContactSubmissions : BaseSpecification<ContactSubmission>
{
    public GetContactSubmissions(string? orderBy, int page, int pageSize, string? search = null)
    {
        if (!string.IsNullOrEmpty(search))
        {
            Criteria = x =>
                x.Email.Contains(search) ||
                x.FirstName.Contains(search) ||
                x.LastName.Contains(search) ||
                x.Message.Contains(search);
        }

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
