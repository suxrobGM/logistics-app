using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Logistics.Application.Abstractions.Integrations;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Geo;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers;

/// <summary>Fake fuel card provider for demos. Transactions are deterministic per truck and day so re-syncs create no duplicates, and some use unknown unit numbers so the unmatched review queue has entries.</summary>
internal class DemoFuelCardService(
    ITenantUnitOfWork tenantUow,
    ILogger<DemoFuelCardService> logger) : IFuelCardProviderService
{
    private static readonly FuelStop[] UsStops =
    [
        new("Dallas", "US", "TX"), new("Oklahoma City", "US", "OK"), new("Little Rock", "US", "AR"),
        new("Memphis", "US", "TN"), new("Nashville", "US", "TN"), new("Atlanta", "US", "GA"),
        new("Indianapolis", "US", "IN"), new("Chicago", "US", "IL"), new("Kansas City", "US", "MO"),
        new("Denver", "US", "CO"), new("Albuquerque", "US", "NM"), new("Phoenix", "US", "AZ")
    ];

    private static readonly FuelStop[] EuStops =
    [
        new("Hamburg", "DE", null), new("Munich", "DE", null), new("Rotterdam", "NL", null),
        new("Antwerp", "BE", null), new("Lyon", "FR", null), new("Milan", "IT", null),
        new("Barcelona", "ES", null), new("Poznan", "PL", null)
    ];

    private static readonly string[] UsMerchants =
    [
        "Pilot Flying J", "Love's Travel Stops", "TA Petro", "Speedway", "Casey's"
    ];

    private static readonly string[] EuMerchants =
    [
        "Shell", "Aral", "TotalEnergies", "Esso", "Orlen"
    ];

    public FuelCardProviderType ProviderType => FuelCardProviderType.Demo;

    public void Initialize(FuelCardProviderConfiguration configuration)
    {
        logger.LogDebug("Initialized Demo fuel card provider");
    }

    public Task<bool> ValidateCredentialsAsync(string apiKey, string? apiSecret)
    {
        return Task.FromResult(!string.IsNullOrEmpty(apiKey));
    }

    public Task<OAuthTokenResultDto?> RefreshTokenAsync(string refreshToken)
    {
        return Task.FromResult<OAuthTokenResultDto?>(null);
    }

    public async Task<IReadOnlyList<FuelCardTransactionData>> GetTransactionsAsync(
        DateTime sinceUtc, CancellationToken ct = default)
    {
        var trucks = await tenantUow.Repository<Truck>().GetListAsync(ct: ct);
        if (trucks.Count == 0)
        {
            return [];
        }

        var tenant = tenantUow.GetCurrentTenant();
        var transactions = new List<FuelCardTransactionData>();
        var startDate = DateOnly.FromDateTime(sinceUtc.Date);
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-1));

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            foreach (var truck in trucks)
            {
                var seed = Seed($"{truck.Number}|{date:yyyyMMdd}");

                // Roughly every other truck-day has a fuel stop
                if (seed % 100 < 55)
                {
                    continue;
                }

                // ~20% of transactions carry an unknown unit number → review queue
                var isUnmatched = seed % 100 >= 92;
                var unitNumber = isUnmatched ? $"POOL-{seed % 9 + 1}" : truck.Number;
                // ID is keyed by the source truck (not the unit number) so two trucks hashing
                // to the same POOL unit on the same day can't collide on the unique index.
                transactions.Add(CreateTransaction($"DEMO-{truck.Number}-{date:yyyyMMdd}", unitNumber, date, seed, tenant));
            }
        }

        logger.LogDebug("Demo fuel card provider generated {Count} transactions", transactions.Count);
        return transactions;
    }

    /// <summary>Builds one fake transaction: EU-region tenants buy liters at EU stations, US tenants buy gallons at US stations.</summary>
    private static FuelCardTransactionData CreateTransaction(
        string externalId, string unitNumber, DateOnly date, uint seed, Tenant tenant)
    {
        var isEu = tenant.Settings.Region == Region.EU;
        var stops = isEu ? EuStops : UsStops;
        var merchants = isEu ? EuMerchants : UsMerchants;
        var stop = stops[seed % (uint)stops.Length];
        var quantity = isEu ? 250m + seed % 400 : 60m + seed % 120;
        var pricePerUnit = isEu ? 1.55m + (seed % 40) / 100m : 3.20m + (seed % 140) / 100m;
        var timestamp = date.ToDateTime(new TimeOnly((int)(seed % 24), (int)(seed % 60)), DateTimeKind.Utc);

        return new FuelCardTransactionData
        {
            ExternalTransactionId = externalId,
            TransactionDate = timestamp,
            Amount = Math.Round(quantity * pricePerUnit, 2),
            Currency = tenant.Currency,
            Quantity = quantity,
            QuantityUnit = isEu ? VolumeUnit.Liters : VolumeUnit.Gallons,
            PricePerUnit = pricePerUnit,
            ProductCategory = "Diesel",
            MerchantName = merchants[seed % (uint)merchants.Length],
            MerchantCity = stop.City,
            PurchaseJurisdiction = new TaxJurisdiction { CountryCode = stop.CountryCode, Region = stop.Region },
            CardNumberMasked = $"****{seed % 10000:D4}",
            ExternalCardId = $"DEMOCARD-{unitNumber}",
            UnitNumber = unitNumber,
            DriverName = null,
            RawJson = JsonSerializer.Serialize(new { demo = true, unitNumber, date })
        };
    }

    private sealed record FuelStop(string City, string CountryCode, string? Region);

    private static uint Seed(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToUInt32(hash, 0);
    }
}
