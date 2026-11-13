using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Integrations.LoadBoard.Credit;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.LoadBoard.Providers;

internal class DemoLoadBoardService(ILogger<DemoLoadBoardService> logger) : ILoadBoardProviderService
{
    private static readonly Random random = new();

    private static readonly string[] cities =
    [
        "Los Angeles, CA", "Dallas, TX", "Chicago, IL", "Atlanta, GA",
        "Denver, CO", "Phoenix, AZ", "Seattle, WA", "Miami, FL",
        "New York, NY", "Houston, TX", "Kansas City, MO", "Memphis, TN"
    ];

    private static readonly LoadBoardEquipmentType[] equipmentTypes =
    [
        LoadBoardEquipmentType.Van, LoadBoardEquipmentType.Flatbed, LoadBoardEquipmentType.Reefer,
        LoadBoardEquipmentType.StepDeck, LoadBoardEquipmentType.Lowboy, LoadBoardEquipmentType.AutoCarrier
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
        logger.LogDebug("Initialized Demo Load Board provider");
    }

    public Task<OAuthTokenResultDto> AcquireTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default)
    {
        return Task.FromResult(new OAuthTokenResultDto
        {
            AccessToken = "demo-token",
            ExpiresAt = DateTime.UtcNow.AddYears(10)
        });
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(LoadBoardConfiguration configuration, CancellationToken ct = default)
    {
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public Task<IReadOnlyList<LoadBoardListingDto>> SearchLoadsAsync(LoadBoardSearchCriteria criteria, CancellationToken ct = default)
    {
        var count = Math.Min(criteria.MaxResults, random.Next(5, 25));
        var listings = Enumerable.Range(0, count).Select(_ => GenerateListingDto()).ToList();

        logger.LogDebug("Demo provider returned {Count} load listings", listings.Count);
        return Task.FromResult<IReadOnlyList<LoadBoardListingDto>>(listings);
    }

    public Task<PostTruckResultDto> PostTruckAsync(PostTruckRequest request, CancellationToken ct = default)
    {
        logger.LogInformation("Demo: Posting truck {TruckId} to load board", request.TruckId);

        return Task.FromResult(new PostTruckResultDto
        {
            ExternalPostId = $"DEMO-POST-{Guid.NewGuid():N}"[..20].ToUpper(),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
    }

    public Task RemoveTruckPostAsync(string externalPostId, CancellationToken ct = default)
    {
        logger.LogInformation("Demo: Removing truck post {PostId}", externalPostId);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ProviderTruckPost>?> GetPostedTrucksAsync(CancellationToken ct = default)
    {
        return Task.FromResult<IReadOnlyList<ProviderTruckPost>?>(null);
    }

    public Task<IReadOnlyList<MarketRate?>> GetMarketRatesAsync(IReadOnlyList<MarketRateLane> lanes, CancellationToken ct = default)
    {
        IReadOnlyList<MarketRate?> rates = lanes
            .Select(_ =>
            {
                var perMile = Math.Round(2.0m + (decimal)(random.NextDouble() * 1.2), 2);
                return (MarketRate?)new MarketRate(
                    perMile / ImperialUnits.KilometersPerMile,
                    null,
                    (perMile - 0.35m) / ImperialUnits.KilometersPerMile,
                    (perMile + 0.40m) / ImperialUnits.KilometersPerMile);
            })
            .ToList();
        return Task.FromResult(rates);
    }

    #region Data Generation

    private LoadBoardListingDto GenerateListingDto()
    {
        var originIndex = random.Next(cities.Length);
        var destinationIndex = (originIndex + random.Next(1, cities.Length)) % cities.Length;

        var origin = cities[originIndex];
        var destination = cities[destinationIndex];

        // US demo freight is generated in miles, pounds and feet as a real board posts it, then stored in metric units
        var miles = random.Next(200, 2000);
        var ratePerMile = 2.0m + (decimal)(random.NextDouble() * 1.5);
        var totalRate = Math.Round(ratePerMile * miles, 2);

        var pickupStart = DateTime.UtcNow.AddDays(random.Next(1, 5));
        var brokerMcNumber = $"MC{random.Next(100000, 999999)}";
        var (creditScore, daysToPay, _) = DemoBrokerCredit.Compute(brokerMcNumber);

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
            RatePerKm = ratePerMile / ImperialUnits.KilometersPerMile,
            TotalRate = totalRate,
            Currency = "USD",
            Distance = miles * ImperialUnits.MetersPerMile,
            WeightKg = Math.Round(random.Next(10000, 45000) * ImperialUnits.KilogramsPerPound),
            LengthMeters = Math.Round(random.Next(20, 53) * ImperialUnits.MetersPerFoot, 2),
            PickupDateStart = pickupStart,
            PickupDateEnd = pickupStart.AddDays(1),
            DeliveryDateStart = pickupStart.AddDays(random.Next(1, 3)),
            DeliveryDateEnd = pickupStart.AddDays(random.Next(3, 5)),
            EquipmentType = equipmentTypes[random.Next(equipmentTypes.Length)],
            Commodity = commodities[random.Next(commodities.Length)],
            BrokerName = brokerNames[random.Next(brokerNames.Length)],
            BrokerPhone = $"555-{random.Next(100, 999)}-{random.Next(1000, 9999)}",
            BrokerEmail = $"dispatch{random.Next(1, 100)}@broker.example.com",
            BrokerMcNumber = brokerMcNumber,
            BrokerCreditScore = creditScore,
            BrokerDaysToPay = daysToPay,
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
