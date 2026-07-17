using Logistics.Application.Abstractions.FuelCards;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Modules.Integrations.FuelCards.Services;

internal sealed class FuelCardSyncService(
    ITenantUnitOfWork tenantUow,
    IFuelCardProviderFactory providerFactory,
    ILogger<FuelCardSyncService> logger) : IFuelCardSyncService
{
    /// <summary>Subtracted from LastSyncedAt so late-posting transactions are never missed.</summary>
    private static readonly TimeSpan SyncOverlap = TimeSpan.FromDays(3);

    /// <summary>How far back the first sync of a new provider configuration reaches.</summary>
    private static readonly TimeSpan InitialSyncWindow = TimeSpan.FromDays(90);

    public async Task<FuelCardSyncResultDto> SyncCurrentTenantAsync(
        FuelCardProviderType? providerType = null,
        CancellationToken ct = default)
    {
        var configs = await tenantUow.Repository<FuelCardProviderConfiguration>()
            .GetListAsync(c => c.IsActive && (providerType == null || c.ProviderType == providerType), ct);

        var result = new FuelCardSyncResultDto();
        var errors = new Dictionary<FuelCardProviderType, string?>();

        foreach (var config in configs)
        {
            try
            {
                await SyncProviderAsync(config, result, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fuel card sync failed for provider {Provider}", config.ProviderType);
                errors[config.ProviderType] = ex.Message;
            }
        }

        result.Errors = errors.Count > 0 ? errors : null;
        return result;
    }

    private async Task SyncProviderAsync(
        FuelCardProviderConfiguration config,
        FuelCardSyncResultDto result,
        CancellationToken ct)
    {
        var since = (config.LastSyncedAt ?? DateTime.UtcNow - InitialSyncWindow) - SyncOverlap;
        var provider = providerFactory.GetProvider(config);
        var transactions = await provider.GetTransactionsAsync(since, ct);

        if (transactions.Count > 0)
        {
            var transactionRepo = tenantUow.Repository<FuelCardTransaction>();

            var externalIds = transactions.Select(t => t.ExternalTransactionId).ToList();
            var knownIds = (await transactionRepo.Query()
                    .Where(t => t.ProviderType == config.ProviderType && externalIds.Contains(t.ExternalTransactionId))
                    .Select(t => t.ExternalTransactionId)
                    .ToListAsync(ct))
                .ToHashSet();

            var trucks = await tenantUow.Repository<Truck>().GetListAsync(ct: ct);
            var trucksByNumber = trucks
                .GroupBy(t => t.Number.Trim().ToUpperInvariant())
                .ToDictionary(g => g.Key, g => g.First());
            var trucksById = trucks.ToDictionary(t => t.Id);

            var cardsByExternalId = (await tenantUow.Repository<FuelCard>()
                    .GetListAsync(c => c.ProviderType == config.ProviderType, ct))
                .GroupBy(c => c.ExternalCardId)
                .ToDictionary(g => g.Key, g => g.First());

            // DistinctBy guards against providers repeating a transaction within one batch —
            // a duplicate would violate the unique (provider, external ID) index on save.
            foreach (var data in transactions
                         .DistinctBy(t => t.ExternalTransactionId)
                         .Where(t => !knownIds.Contains(t.ExternalTransactionId)))
            {
                var entity = CreateTransactionEntity(config.ProviderType, data);
                var truck = await ResolveTruckAsync(
                    config.ProviderType, data, trucksByNumber, trucksById, cardsByExternalId, ct);

                if (truck is not null)
                {
                    var expense = FuelCardExpenseFactory.CreateExpense(entity, truck.Id);
                    await tenantUow.Repository<Expense>().AddAsync(expense, ct);
                    entity.TruckId = truck.Id;
                    entity.ExpenseId = expense.Id;
                    entity.Status = FuelCardTransactionStatus.Matched;
                    result.Matched++;
                }
                else
                {
                    result.Pending++;
                }

                await transactionRepo.AddAsync(entity, ct);
                result.Imported++;
            }
        }

        config.LastSyncedAt = DateTime.UtcNow;
        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Fuel card sync for {Provider}: {Imported} imported ({Matched} matched, {Pending} pending)",
            config.ProviderType, result.Imported, result.Matched, result.Pending);
    }

    /// <summary>
    /// Matching order: explicit FuelCard mapping → exact unit-number match against a truck
    /// number (auto-creates the mapping) → unmatched (review queue).
    /// </summary>
    private async Task<Truck?> ResolveTruckAsync(
        FuelCardProviderType providerType,
        FuelCardTransactionData data,
        Dictionary<string, Truck> trucksByNumber,
        Dictionary<Guid, Truck> trucksById,
        Dictionary<string, FuelCard> cardsByExternalId,
        CancellationToken ct)
    {
        // Resolve through the already-loaded truck list rather than card.Truck: the navigation
        // property would lazy-load one SELECT per mapped transaction.
        if (data.ExternalCardId is not null &&
            cardsByExternalId.TryGetValue(data.ExternalCardId, out var card) &&
            card is { IsActive: true, TruckId: not null } &&
            trucksById.TryGetValue(card.TruckId.Value, out var mappedTruck))
        {
            return mappedTruck;
        }

        var unitNumber = data.UnitNumber?.Trim().ToUpperInvariant();
        if (unitNumber is null || !trucksByNumber.TryGetValue(unitNumber, out var truck))
        {
            return null;
        }

        // Remember the card → truck association so future transactions match even if the
        // provider stops sending the unit number.
        if (data.ExternalCardId is not null && !cardsByExternalId.ContainsKey(data.ExternalCardId))
        {
            var newCard = new FuelCard
            {
                ProviderType = providerType,
                ExternalCardId = data.ExternalCardId,
                UnitNumber = data.UnitNumber,
                TruckId = truck.Id,
                Truck = truck
            };
            await tenantUow.Repository<FuelCard>().AddAsync(newCard, ct);
            cardsByExternalId[data.ExternalCardId] = newCard;
        }

        return truck;
    }

    private static FuelCardTransaction CreateTransactionEntity(
        FuelCardProviderType providerType,
        FuelCardTransactionData data)
    {
        return new FuelCardTransaction
        {
            ProviderType = providerType,
            ExternalTransactionId = data.ExternalTransactionId,
            TransactionDate = data.TransactionDate,
            Amount = new() { Amount = data.Amount, Currency = data.Currency },
            Quantity = data.Quantity,
            QuantityUnit = data.QuantityUnit,
            PricePerUnit = data.PricePerUnit,
            ProductCategory = data.ProductCategory,
            MerchantName = data.MerchantName,
            MerchantCity = data.MerchantCity,
            PurchaseJurisdiction = data.PurchaseJurisdiction,
            CardNumberMasked = data.CardNumberMasked,
            ExternalCardId = data.ExternalCardId,
            UnitNumber = data.UnitNumber,
            DriverName = data.DriverName,
            RawPayloadJson = data.RawJson,
            Status = FuelCardTransactionStatus.Pending
        };
    }
}
