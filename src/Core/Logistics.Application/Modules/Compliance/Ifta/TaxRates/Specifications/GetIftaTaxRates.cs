using Logistics.Domain.Entities;
using Logistics.Domain.Specifications;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Specifications;

public sealed class GetIftaTaxRates : BaseSpecification<IftaTaxRate>
{
    public GetIftaTaxRates(
        string? orderBy,
        int page,
        int pageSize,
        string? search = null,
        int? year = null,
        int? quarter = null,
        string? countryCode = null)
    {
        Criteria = x =>
            (string.IsNullOrEmpty(search) ||
             x.Jurisdiction.CountryCode.Contains(search) ||
             (x.Jurisdiction.Region != null && x.Jurisdiction.Region.Contains(search))) &&
            (!year.HasValue || x.Year == year.Value) &&
            (!quarter.HasValue || x.Quarter == quarter.Value) &&
            (string.IsNullOrEmpty(countryCode) || x.Jurisdiction.CountryCode == countryCode);

        OrderBy(orderBy);
        ApplyPaging(page, pageSize);
    }
}
