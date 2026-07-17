using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.LoadBoard;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

internal sealed class SearchLoadBoardHandler(
    ITenantUnitOfWork tenantUow,
    ILoadBoardProviderFactory providerFactory,
    ILogger<SearchLoadBoardHandler> logger)
    : IAppRequestHandler<SearchLoadBoardCommand, Result<LoadBoardSearchResultDto>>
{
    public async Task<Result<LoadBoardSearchResultDto>> Handle(SearchLoadBoardCommand req, CancellationToken ct)
    {
        // Get active provider configurations
        var configs = await tenantUow.Repository<LoadBoardConfiguration>()
            .GetListAsync(c => c.IsActive, ct);

        if (!configs.Any())
        {
            return Result<LoadBoardSearchResultDto>.Fail("No load board providers configured. Please add a provider first.");
        }

        // Filter by requested providers if specified
        if (req.Providers is { Length: > 0 })
        {
            configs = configs.Where(c => req.Providers.Contains(c.ProviderType)).ToList();
        }

        var allListings = new List<LoadBoardListingDto>();
        var countByProvider = new Dictionary<LoadBoardProviderType, int>();
        var errors = new Dictionary<LoadBoardProviderType, string?>();

        var criteria = new LoadBoardSearchCriteria
        {
            OriginAddress = req.OriginAddress,
            OriginRadius = req.OriginRadius,
            DestinationAddress = req.DestinationAddress,
            DestinationRadius = req.DestinationRadius,
            PickupDateStart = req.PickupDateStart,
            PickupDateEnd = req.PickupDateEnd,
            EquipmentTypes = req.EquipmentTypes,
            MinRatePerMile = req.MinRatePerMile,
            MinTotalRate = req.MinTotalRate,
            MinWeight = req.MinWeight,
            MaxWeight = req.MaxWeight,
            MaxLength = req.MaxLength,
            MaxResults = req.MaxResults
        };

        // Search each provider
        foreach (var config in configs)
        {
            try
            {
                var provider = providerFactory.GetProvider(config);
                var listings = await provider.SearchLoadsAsync(criteria);
                var listingsList = listings.ToList();

                allListings.AddRange(listingsList);
                countByProvider[config.ProviderType] = listingsList.Count;

                logger.LogDebug("Found {Count} listings from {Provider}", listingsList.Count, config.ProviderType);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error searching load board provider {Provider}", config.ProviderType);
                errors[config.ProviderType] = ex.Message;
                countByProvider[config.ProviderType] = 0;
            }
        }

        // Sort by rate per mile descending (best rates first)
        var sortedListings = allListings
            .OrderByDescending(l => l.RatePerMile ?? 0)
            .Take(req.MaxResults)
            .ToList();

        await PersistListingsAsync(sortedListings, ct);

        var result = new LoadBoardSearchResultDto
        {
            Listings = sortedListings,
            TotalCount = sortedListings.Count,
            CountByProvider = countByProvider,
            Errors = errors.Count > 0 ? errors : null
        };

        return Result<LoadBoardSearchResultDto>.Ok(result);
    }

    /// <summary>
    /// Upserts search results as LoadBoardListing entities so they can be booked later —
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

        foreach (var dto in listings)
        {
            if (existingByKey.TryGetValue((dto.ProviderType, dto.ExternalListingId), out var entity))
            {
                entity.RatePerMile = dto.RatePerMile;
                entity.TotalRate = ToMoney(dto.TotalRate, dto.Currency);
                entity.ExpiresAt = dto.ExpiresAt;
                entity.BrokerCreditScore = dto.BrokerCreditScore ?? entity.BrokerCreditScore;
                entity.BrokerDaysToPay = dto.BrokerDaysToPay ?? entity.BrokerDaysToPay;

                // Reflect already-booked state back onto the DTO
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
                    ExpiresAt = dto.ExpiresAt
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
