using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Integrations.FuelCards.Providers;

/// <summary>
/// Demo fuel card provider for development and demos. Generates deterministic transactions
/// keyed by (truck, day) — external IDs are stable so re-syncs are no-ops, ~80% of
/// transactions carry a real tenant truck number (auto-match), the rest an unknown unit
/// number so the review queue demos non-empty.
/// </summary>
internal class DemoFuelCardService(
    ITenantUnitOfWork tenantUow,
    ILogger<DemoFuelCardService> logger) : IFuelCardProviderService
{
    private static readonly (string City, string State)[] Stops =
    [
        ("Dallas", "TX"), ("Oklahoma City", "OK"), ("Little Rock", "AR"), ("Memphis", "TN"),
        ("Nashville", "TN"), ("Atlanta", "GA"), ("Indianapolis", "IN"), ("Chicago", "IL"),
        ("Kansas City", "MO"), ("Denver", "CO"), ("Albuquerque", "NM"), ("Phoenix", "AZ")
    ];

    private static readonly string[] Merchants =
    [
        "Pilot Flying J", "Love's Travel Stops", "TA Petro", "Speedway", "Casey's"
    ];

    public FuelCardProviderType ProviderType => FuelCardProviderType.Demo;

    public void Initialize(FuelCardProviderConfiguration configuration)
    {
        logger.LogInformation("Initialized Demo fuel card provider");
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
                transactions.Add(CreateTransaction($"DEMO-{truck.Number}-{date:yyyyMMdd}", unitNumber, date, seed));
            }
        }

        logger.LogDebug("Demo fuel card provider generated {Count} transactions", transactions.Count);
        return transactions;
    }

    private static FuelCardTransactionData CreateTransaction(string externalId, string unitNumber, DateOnly date, uint seed)
    {
        var (city, state) = Stops[seed % (uint)Stops.Length];
        var gallons = 60m + seed % 120;
        var pricePerGallon = 3.20m + (seed % 140) / 100m;
        var timestamp = date.ToDateTime(new TimeOnly((int)(seed % 24), (int)(seed % 60)), DateTimeKind.Utc);

        return new FuelCardTransactionData
        {
            ExternalTransactionId = externalId,
            TransactionDate = timestamp,
            Amount = Math.Round(gallons * pricePerGallon, 2),
            Currency = "USD",
            Quantity = gallons,
            QuantityUnit = VolumeUnit.Gallons,
            PricePerUnit = pricePerGallon,
            ProductCategory = "Diesel",
            MerchantName = Merchants[seed % (uint)Merchants.Length],
            MerchantCity = city,
            PurchaseJurisdiction = new TaxJurisdiction { CountryCode = "US", Region = state },
            CardNumberMasked = $"****{seed % 10000:D4}",
            ExternalCardId = $"DEMOCARD-{unitNumber}",
            UnitNumber = unitNumber,
            DriverName = null,
            RawJson = JsonSerializer.Serialize(new { demo = true, unitNumber, date })
        };
    }

    private static uint Seed(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return BitConverter.ToUInt32(hash, 0);
    }
}
