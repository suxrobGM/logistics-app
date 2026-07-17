using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;

internal sealed class GetIftaTaxRatesHandler(
    IMasterUnitOfWork masterUow) : IAppRequestHandler<GetIftaTaxRatesQuery, PagedResult<IftaTaxRateDto>>
{
    public Task<PagedResult<IftaTaxRateDto>> Handle(GetIftaTaxRatesQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<IftaTaxRate>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            query = query.Where(x =>
                x.Jurisdiction.CountryCode.Contains(req.Search) ||
                (x.Jurisdiction.Region != null && x.Jurisdiction.Region.Contains(req.Search)));
        }

        if (req.Year.HasValue)
        {
            query = query.Where(x => x.Year == req.Year.Value);
        }

        if (req.Quarter.HasValue)
        {
            query = query.Where(x => x.Quarter == req.Quarter.Value);
        }

        if (!string.IsNullOrEmpty(req.CountryCode))
        {
            query = query.Where(x => x.Jurisdiction.CountryCode == req.CountryCode);
        }

        var totalItems = query.Count();

        // Default sort is a nested owned-type path (Jurisdiction.CountryCode), which the string
        // OrderBy overload can't resolve, so apply it with a typed selector.
        var ordered = string.IsNullOrEmpty(req.OrderBy)
            ? query.OrderBy(x => x.Jurisdiction.CountryCode)
            : query.OrderBy(req.OrderBy);

        var items = ordered
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<IftaTaxRateDto>.Ok(items, totalItems, req.PageSize));
    }
}
