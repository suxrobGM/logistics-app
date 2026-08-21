using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

internal sealed class LaneRateFloorResolver(ITenantUnitOfWork tenantUow) : ILaneRateFloorResolver
{
    public async Task<EffectiveRateFloorDto> ResolveAsync(LoadBoardListing listing, CancellationToken ct = default)
    {
        var originCountry = NormalizeCountry(listing.OriginAddress.Country);
        var originState = NormalizeState(listing.OriginAddress.State);
        var destinationCountry = NormalizeCountry(listing.DestinationAddress.Country);
        var destinationState = NormalizeState(listing.DestinationAddress.State);

        var candidates = await tenantUow.Repository<LaneRateFloor>().GetListAsync(
            f => f.OriginCountry == originCountry && f.DestinationCountry == destinationCountry, ct);

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

            return Build(matched.MinRatePerMile, matched.MinTotalRate, source, matched.Id, listing);
        }

        var defaultRatePerMile = tenantUow.GetCurrentTenant().Settings.DefaultRateFloorPerMile;
        if (defaultRatePerMile.HasValue)
        {
            return Build(defaultRatePerMile.Value, null, RateFloorSource.TenantDefault, null, listing);
        }

        return new EffectiveRateFloorDto { HasFloor = false, Source = RateFloorSource.None };
    }

    private static EffectiveRateFloorDto Build(
        decimal minRatePerMile,
        Money? minTotalRate,
        RateFloorSource source,
        Guid? matchedLaneId,
        LoadBoardListing listing)
    {
        var (floorTotal, belowFloor, gapPerMile) = Evaluate(minRatePerMile, minTotalRate, listing);

        return new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerMile = minRatePerMile,
            MinTotalRate = minTotalRate,
            Source = source,
            MatchedLaneId = matchedLaneId,
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

    private static string NormalizeCountry(string country) => country.Trim().ToUpperInvariant();

    private static string? NormalizeState(string? state) =>
        string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
}
