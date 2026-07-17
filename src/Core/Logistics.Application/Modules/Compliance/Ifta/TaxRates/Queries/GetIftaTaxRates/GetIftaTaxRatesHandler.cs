using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Compliance.Ifta.TaxRates.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;

internal sealed class GetIftaTaxRatesHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetIftaTaxRatesQuery, PagedResult<IftaTaxRateDto>>
{
    public async Task<PagedResult<IftaTaxRateDto>> Handle(GetIftaTaxRatesQuery req, CancellationToken ct)
    {
        var spec = new GetIftaTaxRates(
            req.OrderBy ?? "Jurisdiction.CountryCode",
            req.Page,
            req.PageSize,
            req.Search,
            req.Year,
            req.Quarter,
            req.CountryCode);

        var totalItems = await masterUow.Repository<IftaTaxRate>().CountAsync(spec.Criteria, ct);
        var items = masterUow.Repository<IftaTaxRate>()
            .ApplySpecification(spec)
            .Select(i => i.ToDto())
            .ToArray();

        return PagedResult<IftaTaxRateDto>.Ok(items, totalItems, req.PageSize);
    }
}
