using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Common.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

internal sealed class GetFuelCardTransactionsHandler(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
    : IRequestHandler<GetFuelCardTransactionsQuery, PagedResult<FuelCardTransactionDto>>
{
    private static readonly SortFields<FuelCardTransaction> Sorts = new SortFields<FuelCardTransaction>("-transactionDate")
        .Add("transactionDate", x => x.TransactionDate);

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

        return await query.ToPagedResultAsync(req, Sorts, MapPageAsync, ct);
    }

    private async Task<IEnumerable<FuelCardTransactionDto>> MapPageAsync(
        IReadOnlyList<FuelCardTransaction> transactions,
        CancellationToken ct)
    {
        var truckNumbers = await names.GetTruckNumbersAsync(transactions.Select(t => t.TruckId), ct);
        return transactions.Select(t => t.ToDto(truckNumbers.Find(t.TruckId)));
    }
}
