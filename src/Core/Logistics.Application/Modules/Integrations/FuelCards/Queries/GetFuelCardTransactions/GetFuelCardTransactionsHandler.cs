using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

internal sealed class GetFuelCardTransactionsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetFuelCardTransactionsQuery, PagedResult<FuelCardTransactionDto>>
{
    public async Task<PagedResult<FuelCardTransactionDto>> Handle(
        GetFuelCardTransactionsQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<FuelCardTransaction>().Query();

        if (req.Status.HasValue)
        {
            query = query.Where(t => t.Status == req.Status.Value);
        }

        if (req.ProviderType.HasValue)
        {
            query = query.Where(t => t.ProviderType == req.ProviderType.Value);
        }

        if (req.TruckId.HasValue)
        {
            query = query.Where(t => t.TruckId == req.TruckId.Value);
        }

        if (req.FromDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate >= req.FromDate.Value);
        }

        if (req.ToDate.HasValue)
        {
            query = query.Where(t => t.TransactionDate <= req.ToDate.Value);
        }

        var totalItems = await query.CountAsync(ct);

        var transactions = await query
            .OrderBy(req.OrderBy ?? "-TransactionDate")
            .ApplyPaging(req.Page, req.PageSize)
            .ToListAsync(ct);

        var truckNumbers = await GetTruckNumbersAsync(transactions.Select(t => t.TruckId), ct);
        var dtos = transactions
            .Select(t => t.ToDto(t.TruckId is { } id ? truckNumbers.GetValueOrDefault(id) : null))
            .ToArray();

        return PagedResult<FuelCardTransactionDto>.Ok(dtos, totalItems, req.PageSize);
    }

    /// <summary>
    /// One query for the whole page — reading Truck.Number off each row would lazy-load per row.
    /// </summary>
    private async Task<Dictionary<Guid, string>> GetTruckNumbersAsync(
        IEnumerable<Guid?> truckIds,
        CancellationToken ct)
    {
        var ids = truckIds.OfType<Guid>().Distinct().ToArray();
        if (ids.Length == 0)
        {
            return [];
        }

        return await tenantUow.Repository<Truck>().Query()
            .Where(t => ids.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);
    }
}
