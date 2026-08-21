using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Negotiation.Queries;

internal sealed class GetNegotiationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetNegotiationsQuery, PagedResult<RateNegotiationDto>>
{
    public async Task<PagedResult<RateNegotiationDto>> Handle(GetNegotiationsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<RateNegotiation>().Query();

        if (req.Status.HasValue)
        {
            query = query.Where(n => n.Status == req.Status.Value);
        }
        else if (req.ActiveOnly)
        {
            query = query.Where(n =>
                n.Status == RateNegotiationStatus.AwaitingBroker ||
                n.Status == RateNegotiationStatus.BrokerReplied);
        }

        var totalItems = await query.CountAsync(ct);

        var negotiations = await query
            .OrderBy(req.OrderBy ?? "-CreatedAt")
            .ApplyPaging(req.Page, req.PageSize)
            .ToListAsync(ct);

        var listings = await GetListingsAsync(negotiations.Select(n => n.LoadBoardListingId), ct);
        var dtos = negotiations
            .Select(n => n.ToDto(listings.GetValueOrDefault(n.LoadBoardListingId)))
            .ToArray();

        return PagedResult<RateNegotiationDto>.Ok(dtos, totalItems, req.PageSize);
    }

    /// <summary>One query for the whole page - the listing navigation would lazy-load per row.</summary>
    private async Task<Dictionary<Guid, LoadBoardListing>> GetListingsAsync(
        IEnumerable<Guid> listingIds, CancellationToken ct)
    {
        var ids = listingIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        return await tenantUow.Repository<LoadBoardListing>().Query()
            .Where(l => ids.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);
    }
}
