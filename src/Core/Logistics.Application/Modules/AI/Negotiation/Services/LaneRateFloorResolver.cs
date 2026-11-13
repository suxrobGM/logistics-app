using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Negotiation.Services;

/// <summary>Resolves the rate floor for a listing: the most specific matching <see cref="LaneRateFloor"/>, else the tenant's default floor.</summary>
public interface ILaneRateFloorResolver : IApplicationService
{
    Task<EffectiveRateFloorDto> ResolveAsync(LoadBoardListing listing, CancellationToken ct = default);
}

internal sealed class LaneRateFloorResolver(ITenantUnitOfWork tenantUow) : ILaneRateFloorResolver
{
    /// <summary>Lane floors loaded once per request scope, since a dispatch turn resolves a floor for every candidate listing.</summary>
    private IReadOnlyList<LaneRateFloor>? _cachedFloors;

    public async Task<EffectiveRateFloorDto> ResolveAsync(LoadBoardListing listing, CancellationToken ct = default)
    {
        var tenant = tenantUow.GetCurrentTenant();
        if (!ListingCurrency.IsTenantCurrency(listing, tenant))
        {
            return new EffectiveRateFloorDto { HasFloor = false, Source = RateFloorSource.None };
        }

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

            return Build(matched.MinRatePerKm, matched.MinTotalRate, source, listing);
        }

        var defaultRatePerKm = tenant.Settings.DefaultRateFloorPerKm;
        if (defaultRatePerKm.HasValue)
        {
            return Build(defaultRatePerKm.Value, null, RateFloorSource.TenantDefault, listing);
        }

        return new EffectiveRateFloorDto { HasFloor = false, Source = RateFloorSource.None };
    }

    private async Task<IReadOnlyList<LaneRateFloor>> GetFloorsAsync(CancellationToken ct) =>
        _cachedFloors ??= await tenantUow.Repository<LaneRateFloor>().GetListAsync(ct);

    private static EffectiveRateFloorDto Build(
        decimal minRatePerKm,
        decimal? minTotalRate,
        RateFloorSource source,
        LoadBoardListing listing)
    {
        var (floorTotal, belowFloor, gapPerKm) = Evaluate(minRatePerKm, minTotalRate, listing);

        return new EffectiveRateFloorDto
        {
            HasFloor = true,
            MinRatePerKm = minRatePerKm,
            MinTotalRate = minTotalRate,
            Source = source,
            EffectiveFloorTotal = floorTotal,
            ListingBelowFloor = belowFloor,
            GapPerKm = gapPerKm
        };
    }

    /// <summary>Checks a listing's rate against the floor: as total amounts when the listing's distance is known, otherwise as rates per km.</summary>
    private static (decimal? floorTotal, bool belowFloor, decimal? gapPerKm) Evaluate(
        decimal minRatePerKm, decimal? minTotalRate, LoadBoardListing listing)
    {
        decimal? distanceKm = listing.Distance is > 0
            ? DistanceUnit.Kilometers.FromMeters(listing.Distance.Value).ToDecimal()
            : null;
        decimal? floorTotal = distanceKm is { } km
            ? Math.Max(minRatePerKm * km, minTotalRate ?? 0M)
            : minTotalRate;

        var listingTotal = listing.TotalRate?.Amount
            ?? (listing.RatePerKm.HasValue && distanceKm.HasValue
                ? listing.RatePerKm.Value * distanceKm.Value
                : null);

        if (floorTotal.HasValue && listingTotal.HasValue)
        {
            var gapTotal = floorTotal.Value - listingTotal.Value;
            var gapPerKm = distanceKm.HasValue ? gapTotal / distanceKm.Value : (decimal?)null;
            return (floorTotal, gapTotal > 0, gapPerKm);
        }

        if (listing.RatePerKm.HasValue)
        {
            var gap = minRatePerKm - listing.RatePerKm.Value;
            return (floorTotal, gap > 0, gap);
        }

        return (floorTotal, false, null);
    }
}
