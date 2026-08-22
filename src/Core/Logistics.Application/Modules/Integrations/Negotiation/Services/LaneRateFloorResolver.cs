using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

internal sealed class LaneRateFloorResolver(ITenantUnitOfWork tenantUow) : ILaneRateFloorResolver
{
    /// <summary>
    /// A dispatch turn resolves a floor per candidate listing within one scope, and the table is
    /// small enough to filter in memory - so read it once instead of once per listing.
    /// </summary>
    private IReadOnlyList<LaneRateFloor>? cachedFloors;

    public async Task<EffectiveRateFloorDto> ResolveAsync(LoadBoardListing listing, CancellationToken ct = default)
    {
        var originCountry = LaneKey.Country(listing.OriginAddress.Country);
        var originState = LaneKey.State(listing.OriginAddress.State);
        var destinationCountry = LaneKey.Country(listing.DestinationAddress.Country);
        var destinationState = LaneKey.State(listing.DestinationAddress.State);

        var candidates = (await GetFloorsAsync(ct))
            .Where(f => f.OriginCountry == originCountry && f.DestinationCountry == destinationCountry)
            .ToArray();

        var matched = candidates.FirstOrDefault(f =>
                f.OriginState == originState && f.DestinationState == destinationState)
            ?? candidates.FirstOrDefault(f => f.OriginState == originState && f.DestinationState is null)
            ?? candidates.FirstOrDefault(f => f.OriginState is null && f.DestinationState == destinationState);

        if (matched is not null)
        {
            var source = matched.OriginState == originState && matched.DestinationState == destinationState
                ? RateFloorSource.LaneExact
                : matched.OriginState == originState
                    ? RateFloorSource.LaneOriginAny
                    : RateFloorSource.LaneDestinationAny;

            return Build(matched.MinRatePerMile, matched.MinTotalRate, source, listing);
        }

        var defaultRatePerMile = tenantUow.GetCurrentTenant().Settings.DefaultRateFloorPerMile;
        if (defaultRatePerMile.HasValue)
        {
            return Build(defaultRatePerMile.Value, null, RateFloorSource.TenantDefault, listing);
        }

        return new EffectiveRateFloorDto { HasFloor = false, Source = RateFloorSource.None };
    }

    private async Task<IReadOnlyList<LaneRateFloor>> GetFloorsAsync(CancellationToken ct) =>
        cachedFloors ??= await tenantUow.Repository<LaneRateFloor>().GetListAsync(specification: null, ct);

    private static EffectiveRateFloorDto Build(
        decimal minRatePerMile,
        Money? minTotalRate,
        RateFloorSource source,
        LoadBoardListing listing)
    {
        var (floorTotal, belowFloor, gapPerMile) = Evaluate(minRatePerMile, minTotalRate, listing);

        return new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = minRatePerMile,
            MinTotalRate = minTotalRate,
            Source = source,
            EffectiveFloorTotal = floorTotal,
            ListingBelowFloor = belowFloor,
            GapPerMile = gapPerMile
        };
    }

    /// <summary>
    /// Combines the per-mile and flat-total floor into one comparison against the listing's own
    /// rate. When the distance is known, both sides are compared as totals - the per-mile floor is
    /// converted with distance and the higher of the two floor components wins - and the per-mile
    /// gap is the total gap spread back over the distance. Without a distance, a flat-total floor
    /// stands alone (it needs no conversion) and a per-mile floor falls back to a direct per-mile
    /// comparison against the listing's rate per mile.
    /// </summary>
    private static (decimal? floorTotal, bool belowFloor, decimal? gapPerMile) Evaluate(
        decimal minRatePerMile, Money? minTotalRate, LoadBoardListing listing)
    {
        var distanceMiles = listing.Distance;
        decimal? floorTotal = distanceMiles is > 0
            ? Math.Max(minRatePerMile * (decimal)distanceMiles.Value, minTotalRate?.Amount ?? 0M)
            : minTotalRate?.Amount;

        var listingTotal = listing.TotalRate?.Amount
            ?? (listing.RatePerMile.HasValue && distanceMiles is > 0
                ? listing.RatePerMile.Value * (decimal)distanceMiles.Value
                : null);

        if (floorTotal.HasValue && listingTotal.HasValue)
        {
            var gapTotal = floorTotal.Value - listingTotal.Value;
            var gapPerMile = distanceMiles is > 0 ? gapTotal / (decimal)distanceMiles.Value : (decimal?)null;
            return (floorTotal, gapTotal > 0, gapPerMile);
        }

        if (listing.RatePerMile.HasValue)
        {
            var gap = minRatePerMile - listing.RatePerMile.Value;
            return (floorTotal, gap > 0, gap);
        }

        return (floorTotal, false, null);
    }
}
