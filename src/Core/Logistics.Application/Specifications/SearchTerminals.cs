using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Specifications;

public class SearchTerminals : BaseSpecification<Terminal>
{
    public SearchTerminals(
        string? search,
        string? orderBy,
        int page,
        int pageSize,
        TerminalType? type = null,
        string? countryCode = null)
    {
        Criteria = i =>
            (string.IsNullOrEmpty(search) || i.Name.Contains(search) || i.Code.Contains(search)) &&
            (!type.HasValue || i.Type == type.Value) &&
            (string.IsNullOrEmpty(countryCode) || i.CountryCode == countryCode);

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
