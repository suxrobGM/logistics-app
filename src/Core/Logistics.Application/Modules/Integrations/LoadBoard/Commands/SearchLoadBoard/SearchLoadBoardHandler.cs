using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class SearchLoadBoardHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderGateway gateway,
    ILogger<SearchLoadBoardHandler> logger)
    : IRequestHandler<SearchLoadBoardCommand, Result<LoadBoardSearchResultDto>>
{
    /// <summary>
    /// How long a listing the board gave no expiry for stays bookable. Board postings are
    /// short-lived; one not seen again by a search within this window is treated as gone.
    /// </summary>
    private static readonly TimeSpan UnknownExpiryShelfLife = TimeSpan.FromHours(24);

    /// <summary>
    /// Caps one RateView lookup; a full page of results rarely spans more distinct lanes.
    /// </summary>
    private const int MaxMarketRateLanes = 50;

    public async Task<Result<LoadBoardSearchResultDto>> Handle(SearchLoadBoardCommand req, CancellationToken ct)
    {
        var configs = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetListAsync(c => c.IsActive, ct);

        if (configs.Count == 0)
        {
            return Result<LoadBoardSearchResultDto>.Fail("No load board providers configured. Please add a provider first.");
        }

        if (req.Providers is { Length: > 0 })
        {
            configs = configs.Where(c => req.Providers.Contains(c.ProviderType)).ToList();
        }

        var allListings = new List<LoadBoardListingDto>();
        var countByProvider = new Dictionary<LoadBoardProviderType, int>();
        var errors = new Dictionary<LoadBoardProviderType, string>();

        var criteria = new LoadBoardSearchCriteria
        {
            Origin = req.Origin,
            OriginRadius = req.OriginRadius,
            Destination = req.Destination,
            DestinationRadius = req.DestinationRadius,
            PickupDateStart = req.PickupDateStart,
            PickupDateEnd = req.PickupDateEnd,
            EquipmentTypes = req.EquipmentTypes,
            MinRatePerMile = req.MinRatePerMile,
            MinTotalRate = req.MinTotalRate,
            MaxWeight = req.MaxWeight,
            MaxLength = req.MaxLength,
            MaxResults = req.MaxResults
        };

        // Sequential: a token renewal saves the shared scope.
        foreach (var config in configs)
        {
            var result = await gateway.ExecuteAsync(config, p => p.SearchLoadsAsync(criteria, ct), ct);
            if (!result.IsSuccess || result.Value is null)
            {
                errors[config.ProviderType] = result.Error ?? "Search failed";
                countByProvider[config.ProviderType] = 0;
                continue;
            }

            allListings.AddRange(result.Value);
            countByProvider[config.ProviderType] = result.Value.Count;
            logger.LogDebug("Found {Count} listings from {Provider}", result.Value.Count, config.ProviderType);
        }

        var sortedListings = allListings
            .Where(l => MeetsRateFloor(l, req))
            .OrderByDescending(l => l.RatePerMile ?? 0)
            .Take(req.MaxResults)
            .ToList();

        // Priced after the floor and the cap, so lookups and the lane cap go to listings the user will see.
        foreach (var config in configs.Where(c => c.MarketRatesEnabled && !errors.ContainsKey(c.ProviderType)))
        {
            var listings = sortedListings.Where(l => l.ProviderType == config.ProviderType).ToList();
            var ratesError = await AttachMarketRatesAsync(config, listings, ct);
            if (ratesError is not null)
            {
                errors[config.ProviderType] = $"Market rates unavailable. {ratesError}";
            }
        }

        await PersistListingsAsync(sortedListings, ct);

        return Result<LoadBoardSearchResultDto>.Ok(new LoadBoardSearchResultDto
        {
            Listings = sortedListings,
            TotalCount = sortedListings.Count,
            CountByProvider = countByProvider,
            Errors = errors.Count > 0 ? errors : null
        });
    }

    /// <summary>
    /// Prices each distinct city-to-city lane once, in one batched lookup, and stamps the rate on
    /// every listing on that lane. Returns the provider's error when the lookup failed.
    /// </summary>
    private async Task<string?> AttachMarketRatesAsync(
        LoadBoardConfiguration config, IReadOnlyList<LoadBoardListingDto> listings, CancellationToken ct)
    {
        var byLane = listings
            .Where(l => l.OriginAddress.City.Length > 0 && l.DestinationAddress.City.Length > 0)
            .GroupBy(l => new MarketRateLane(
                l.OriginAddress.City, l.OriginAddress.State,
                l.DestinationAddress.City, l.DestinationAddress.State,
                l.EquipmentType))
            .Take(MaxMarketRateLanes)
            .ToList();

        if (byLane.Count == 0)
        {
            return null;
        }

        var lanes = byLane.Select(g => g.Key).ToList();
        var result = await gateway.ExecuteAsync(config, p => p.GetMarketRatesAsync(lanes, ct), ct);
        if (!result.IsSuccess || result.Value is null)
        {
            return result.Error;
        }

        for (var i = 0; i < byLane.Count && i < result.Value.Count; i++)
        {
            if (result.Value[i] is not { } rate)
            {
                continue;
            }

            foreach (var listing in byLane[i])
            {
                listing.MarketRatePerMile = rate.RatePerMile;
                listing.MarketRateLowPerMile = rate.LowPerMile;
                listing.MarketRateHighPerMile = rate.HighPerMile;
            }
        }

        return null;
    }

    /// <summary>
    /// Boards filter by lane and equipment, not by rate, so the rate floors apply here. A listing
    /// with no posted rate passes: most board freight is "call for rate".
    /// </summary>
    private static bool MeetsRateFloor(LoadBoardListingDto listing, SearchLoadBoardCommand req)
    {
        var perMileOk = req.MinRatePerMile is not { } minPerMile ||
                        listing.RatePerMile is not { } perMile ||
                        perMile >= minPerMile;
        var totalOk = req.MinTotalRate is not { } minTotal ||
                      listing.TotalRate is not { } total ||
                      total >= minTotal;
        return perMileOk && totalOk;
    }

    /// <summary>
    /// Upserts search results as LoadBoardListing entities so they can be booked later -
    /// booking resolves the listing by entity Id. Stamps each DTO with the entity Id and
    /// reflects any already-booked state back onto the DTO.
    /// </summary>
    private async Task PersistListingsAsync(List<LoadBoardListingDto> listings, CancellationToken ct)
    {
        if (listings.Count == 0)
        {
            return;
        }

        var externalIds = listings.Select(l => l.ExternalListingId).ToList();
        var listingRepository = tenantUow.Repository<LoadBoardListing>();
        var existing = await listingRepository.GetListAsync(l => externalIds.Contains(l.ExternalListingId), ct);
        var existingByKey = existing
            .GroupBy(l => (l.ProviderType, l.ExternalListingId))
            .ToDictionary(g => g.Key, g => g.First());

        var defaultExpiry = DateTime.UtcNow + UnknownExpiryShelfLife;

        foreach (var dto in listings)
        {
            var expiresAt = dto.ExpiresAt ?? defaultExpiry;
            dto.ExpiresAt = expiresAt;

            if (existingByKey.TryGetValue((dto.ProviderType, dto.ExternalListingId), out var entity))
            {
                entity.RatePerMile = dto.RatePerMile;
                entity.TotalRate = ToMoney(dto.TotalRate, dto.Currency);
                entity.ExpiresAt = expiresAt;
                entity.BrokerCreditScore = dto.BrokerCreditScore ?? entity.BrokerCreditScore;
                entity.BrokerDaysToPay = dto.BrokerDaysToPay ?? entity.BrokerDaysToPay;

                dto.Status = entity.Status;
                dto.BookedAt = entity.BookedAt;
                dto.LoadId = entity.LoadId;
            }
            else
            {
                entity = new LoadBoardListing
                {
                    ExternalListingId = dto.ExternalListingId,
                    ProviderType = dto.ProviderType,
                    OriginAddress = dto.OriginAddress,
                    OriginLocation = dto.OriginLocation,
                    DestinationAddress = dto.DestinationAddress,
                    DestinationLocation = dto.DestinationLocation,
                    RatePerMile = dto.RatePerMile,
                    TotalRate = ToMoney(dto.TotalRate, dto.Currency),
                    Distance = dto.Distance,
                    Weight = dto.Weight,
                    Length = dto.Length,
                    PickupDateStart = dto.PickupDateStart,
                    PickupDateEnd = dto.PickupDateEnd,
                    DeliveryDateStart = dto.DeliveryDateStart,
                    DeliveryDateEnd = dto.DeliveryDateEnd,
                    EquipmentType = dto.EquipmentType,
                    Commodity = dto.Commodity,
                    BrokerName = dto.BrokerName,
                    BrokerPhone = dto.BrokerPhone,
                    BrokerEmail = dto.BrokerEmail,
                    BrokerMcNumber = dto.BrokerMcNumber,
                    BrokerCreditScore = dto.BrokerCreditScore,
                    BrokerDaysToPay = dto.BrokerDaysToPay,
                    Status = LoadBoardListingStatus.Available,
                    ExpiresAt = expiresAt
                };
                await listingRepository.AddAsync(entity, ct);
            }

            dto.Id = entity.Id;
        }

        await tenantUow.SaveChangesAsync(ct);
    }

    private static Money? ToMoney(decimal? amount, string? currency)
    {
        return amount.HasValue
            ? new Money { Amount = amount.Value, Currency = currency ?? "USD" }
            : null;
    }
}
