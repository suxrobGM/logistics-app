using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

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

        var result = new LoadBoardSearchResultDto
        {
            Listings = sortedListings,
            TotalCount = sortedListings.Count,
            CountByProvider = countByProvider,
            Errors = errors.Count > 0 ? errors : null
        };

        return Result<LoadBoardSearchResultDto>.Ok(result);
    }
}
