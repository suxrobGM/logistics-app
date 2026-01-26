using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers;

/// <summary>
///     Demo load board provider implementation for testing without real load board APIs.
///     Returns simulated load listings for development and demonstration purposes.
/// </summary>
internal class DemoLoadBoardService(ILogger<DemoLoadBoardService> logger) : ILoadBoardProviderService
{
    private static readonly Random random = new();

    private static readonly string[] cities =
    [
        "Los Angeles, CA", "Dallas, TX", "Chicago, IL", "Atlanta, GA",
        "Denver, CO", "Phoenix, AZ", "Seattle, WA", "Miami, FL",
        "New York, NY", "Houston, TX", "Kansas City, MO", "Memphis, TN"
    ];

    private static readonly string[] equipmentTypes =
    [
        "Dry Van", "Flatbed", "Reefer", "Step Deck", "Lowboy", "Car Hauler"
    ];

    private static readonly string[] commodities =
    [
        "General Freight", "Machinery", "Electronics", "Food Products",
        "Auto Parts", "Building Materials", "Consumer Goods"
    ];

    private static readonly string[] brokerNames =
    [
        "FastFreight Logistics", "TransAmerica Brokers", "Prime Load Services",
        "Continental Freight", "Midwest Shipping Co", "Pacific Carriers"
    ];

    public LoadBoardProviderType ProviderType => LoadBoardProviderType.Demo;

    public void Initialize(LoadBoardConfiguration configuration)
    {
        logger.LogInformation("Initialized Demo Load Board provider");
    }

    public Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        // Demo provider always validates successfully with any non-empty key
        return Task.FromResult(!string.IsNullOrEmpty(apiKey));
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public Task<IEnumerable<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria)
    {
        var listings = new List<LoadBoardListingDto>();
        var count = Math.Min(criteria.MaxResults, random.Next(5, 25));

        for (var i = 0; i < count; i++)
        {
            listings.Add(GenerateListingDto());
        }

        logger.LogDebug("Demo provider returned {Count} load listings", listings.Count);
        return Task.FromResult<IEnumerable<LoadBoardListingDto>>(listings);
    }

    public Task<LoadBoardListingDto?> GetLoadDetailsAsync(string externalListingId)
    {
        var listing = GenerateListingDto();
        return Task.FromResult<LoadBoardListingDto?>(listing with { ExternalListingId = externalListingId });
    }

    public Task<LoadBoardBookingResultDto> BookLoadAsync(string externalListingId, LoadBoardBookingRequest request)
    {
        logger.LogInformation("Demo: Booking load {ListingId} for truck {TruckId}",
            externalListingId, request.TruckId);

        return Task.FromResult(new LoadBoardBookingResultDto
        {
            Success = true, ExternalConfirmationId = $"DEMO-CONF-{Guid.NewGuid():N}"[..20].ToUpper()
        });
    }

    public Task<bool> CancelBookingAsync(string externalListingId, string? reason)
    {
        logger.LogInformation("Demo: Cancelling booking for {ListingId}. Reason: {Reason}",
            externalListingId, reason ?? "Not specified");
        return Task.FromResult(true);
    }

    public Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request)
    {
        logger.LogInformation("Demo: Posting truck {TruckId} to load board", request.TruckId);

        return Task.FromResult(new PostTruckResultDto
        {
            Success = true,
            ExternalPostId = $"DEMO-POST-{Guid.NewGuid():N}"[..20].ToUpper(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    public Task<bool> UpdateTruckPostAsync(string externalPostId, PostTruckRequest request)
    {
        logger.LogInformation("Demo: Updating truck post {PostId}", externalPostId);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveTruckPostAsync(string externalPostId)
    {
        logger.LogInformation("Demo: Removing truck post {PostId}", externalPostId);
        return Task.FromResult(true);
    }

    public Task<IEnumerable<PostedTruckDto>> GetPostedTrucksAsync()
    {
        // Return empty list - posted trucks are tracked internally
        return Task.FromResult<IEnumerable<PostedTruckDto>>([]);
    }

    public Task<LoadBoardWebhookResultDto> ProcessWebhookAsync(string payload, string? signature, string? webhookSecret)
    {
        // Demo provider doesn't process real webhooks
        return Task.FromResult(new LoadBoardWebhookResultDto
        {
            EventType = LoadBoardWebhookEventType.Unknown,
            IsValid = false,
            ErrorMessage = "Demo provider does not support webhooks"
        });
    }

    #region Data Generation

    private LoadBoardListingDto GenerateListingDto()
    {
        var originIndex = random.Next(cities.Length);
        var destinationIndex = (originIndex + random.Next(1, cities.Length)) % cities.Length;

        var origin = cities[originIndex];
        var destination = cities[destinationIndex];

        var distance = random.Next(200, 2000);
        var ratePerMile = 2.0m + (decimal)(random.NextDouble() * 1.5);
        var totalRate = Math.Round(ratePerMile * distance, 2);

        var pickupStart = DateTime.UtcNow.AddDays(random.Next(1, 5));

        return new LoadBoardListingDto
        {
            ExternalListingId = $"DEMO-{Guid.NewGuid():N}"[..16].ToUpper(),
            ProviderType = LoadBoardProviderType.Demo,
            ProviderName = "Demo",
            OriginAddress =
                new Address
                {
                    Line1 = $"{random.Next(100, 9999)} Demo Street",
                    City = origin.Split(',')[0].Trim(),
                    State = origin.Split(',')[1].Trim(),
                    Country = "US",
                    ZipCode = $"{random.Next(10000, 99999)}"
                },
            OriginLocation = GenerateGeoPoint(originIndex),
            DestinationAddress =
                new Address
                {
                    Line1 = $"{random.Next(100, 9999)} Demo Avenue",
                    City = destination.Split(',')[0].Trim(),
                    State = destination.Split(',')[1].Trim(),
                    Country = "US",
                    ZipCode = $"{random.Next(10000, 99999)}"
                },
            DestinationLocation = GenerateGeoPoint(destinationIndex),
            RatePerMile = ratePerMile,
            TotalRate = totalRate,
            Currency = "USD",
            Distance = distance,
            Weight = random.Next(10000, 45000),
            Length = random.Next(20, 53),
            PickupDateStart = pickupStart,
            PickupDateEnd = pickupStart.AddDays(1),
            DeliveryDateStart = pickupStart.AddDays(random.Next(1, 3)),
            DeliveryDateEnd = pickupStart.AddDays(random.Next(3, 5)),
            EquipmentType = equipmentTypes[random.Next(equipmentTypes.Length)],
            Commodity = commodities[random.Next(commodities.Length)],
            BrokerName = brokerNames[random.Next(brokerNames.Length)],
            BrokerPhone = $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
            BrokerEmail = $"dispatch{random.Next(1, 100)}@broker.example.com",
            BrokerMcNumber = $"MC{random.Next(100000, 999999)}",
            Status = LoadBoardListingStatus.Available,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
    }

    private static GeoPoint GenerateGeoPoint(int cityIndex)
    {
        // Approximate coordinates for demo cities
        var coordinates = new[]
        {
            (34.0522, -118.2437), // LA
            (32.7767, -96.7970), // Dallas
            (41.8781, -87.6298), // Chicago
            (33.7490, -84.3880), // Atlanta
            (39.7392, -104.9903), // Denver
            (33.4484, -112.0740), // Phoenix
            (47.6062, -122.3321), // Seattle
            (25.7617, -80.1918), // Miami
            (40.7128, -74.0060), // New York
            (29.7604, -95.3698), // Houston
            (39.0997, -94.5786), // Kansas City
            (35.1495, -90.0490) // Memphis
        };

        var (lat, lng) = coordinates[cityIndex];
        var latitude = lat + ((random.NextDouble() - 0.5) * 0.1);
        var longitude = lng + ((random.NextDouble() - 0.5) * 0.1);
        return new GeoPoint(longitude, latitude);
    }

    #endregion
}
