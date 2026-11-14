using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Ifta.TaxRates.Queries;

internal sealed class GetIftaTaxRatesHandler(
    IMasterUnitOfWork masterUow) : IRequestHandler<GetIftaTaxRatesQuery, PagedResult<IftaTaxRateDto>>
{
    private static readonly SortFields<IftaTaxRate> Sorts = new SortFields<IftaTaxRate>("jurisdiction")
        .Add("jurisdiction", x => x.Jurisdiction.CountryCode)
        .Add("year", x => x.Year)
        .Add("quarter", x => x.Quarter)
        .Add("ratePerGallon", x => x.RatePerGallon);

    public Task<PagedResult<IftaTaxRateDto>> Handle(GetIftaTaxRatesQuery req, CancellationToken ct)
    {
        var query = masterUow.Repository<IftaTaxRate>().Query();

        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(x =>
                x.Jurisdiction.CountryCode.ToLower().Contains(search) ||
                (x.Jurisdiction.Region != null && x.Jurisdiction.Region.ToLower().Contains(search)));
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

        var items = Sorts.Apply(query, req.OrderBy, x => x.Id)
            .ApplyPaging(req.Page, req.PageSize)
            .Select(i => i.ToDto())
            .ToArray();

        return Task.FromResult(PagedResult<IftaTaxRateDto>.Ok(items, totalItems, req.PageSize));
    }
}
