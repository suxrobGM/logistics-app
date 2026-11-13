using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Modules.Operations.Trucks.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

internal sealed class GetExpensesHandler(ITenantUnitOfWork tenantUow, ITruckDtoMapper truckDtoMapper)
    : IRequestHandler<GetExpensesQuery, PagedResult<ExpenseDto>>
{
    public async Task<PagedResult<ExpenseDto>> Handle(GetExpensesQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Expense>().Query();

        // Apply type filter
        if (req.Type.HasValue)
        {
            query = query.Where(e => e.Type == req.Type.Value);
        }

        // Apply status filter
        if (req.Status.HasValue)
        {
            query = query.Where(e => e.Status == req.Status.Value);
        }

        // Apply truck filter (works for TruckExpense and BodyShopExpense)
        if (req.TruckId.HasValue)
        {
            query = query.Where(e =>
                (e is TruckExpense && ((TruckExpense)e).TruckId == req.TruckId.Value) ||
                (e is BodyShopExpense && ((BodyShopExpense)e).TruckId == req.TruckId.Value));
        }

        // Apply date range filter
        if (req.FromDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= req.FromDate.Value);
        }

        if (req.ToDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= req.ToDate.Value);
        }

        // Apply search (case-insensitive)
        if (!string.IsNullOrEmpty(req.Search))
        {
            var search = req.Search.ToLower();
            query = query.Where(e =>
                (e.VendorName ?? "").ToLower().Contains(search) ||
                (e.Notes ?? "").ToLower().Contains(search));
        }

        return await query.ToPagedResultAsync(req, MapPageAsync, ct, defaultOrderBy: "-ExpenseDate");
    }

    private async Task<IEnumerable<ExpenseDto>> MapPageAsync(IReadOnlyList<Expense> expenses, CancellationToken ct)
    {
        var truckIds = expenses.OfType<TruckLinkedExpense>().Select(e => e.TruckId).Distinct().ToArray();
        var trucks = truckIds.Length == 0
            ? []
            : await tenantUow.Repository<Truck>().Query()
                .Where(t => truckIds.Contains(t.Id))
                .ToListAsync(ct);
        var truckDtos = await truckDtoMapper.MapAsync(trucks, ct);

        return expenses.Select(e => e.ToDto(
            e is TruckLinkedExpense linked ? truckDtos.GetValueOrDefault(linked.TruckId) : null,
            receiptDownloadUrl: null));
    }
}
