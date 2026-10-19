using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

internal sealed class NegotiationDtoMapper(ITenantUnitOfWork tenantUow) : INegotiationDtoMapper
{
    public async Task<RateNegotiationDto[]> MapAsync(
        IReadOnlyCollection<RateNegotiation> negotiations, CancellationToken ct)
    {
        var listings = await GetListingsAsync(negotiations, ct);

        return [.. negotiations.Select(n => n.ToDto(listings.GetValueOrDefault(n.LoadBoardListingId)))];
    }

    private async Task<Dictionary<Guid, LoadBoardListing>> GetListingsAsync(
        IReadOnlyCollection<RateNegotiation> negotiations, CancellationToken ct)
    {
        var ids = negotiations.Select(n => n.LoadBoardListingId).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        return await tenantUow.Repository<LoadBoardListing>().Query()
            .Where(l => ids.Contains(l.Id))
            .ToDictionaryAsync(l => l.Id, ct);
    }
}
