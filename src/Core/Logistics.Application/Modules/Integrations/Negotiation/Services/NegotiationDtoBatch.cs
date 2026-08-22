using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Maps a batch of negotiations to DTOs.
/// </summary>
public static class NegotiationDtoBatch
{
    /// <summary>
    /// One query for the whole batch - the listing navigation would lazy-load per row, and the
    /// mapper needs the listing for the lane and the listing rate.
    /// </summary>
    public static async Task<RateNegotiationDto[]> MapAsync(
        ITenantUnitOfWork tenantUow,
        IReadOnlyCollection<RateNegotiation> negotiations,
        CancellationToken ct)
    {
        var listings = await GetListingsAsync(tenantUow, negotiations, ct);

        return [.. negotiations.Select(n => n.ToDto(listings.GetValueOrDefault(n.LoadBoardListingId)))];
    }

    private static async Task<Dictionary<Guid, LoadBoardListing>> GetListingsAsync(
        ITenantUnitOfWork tenantUow,
        IReadOnlyCollection<RateNegotiation> negotiations,
        CancellationToken ct)
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
