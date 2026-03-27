using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetExpensesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetExpensesQuery, PagedResult<ExpenseDto>>
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
                (e.VendorName ?? "").Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                (e.Notes ?? "").Contains(search, StringComparison.CurrentCultureIgnoreCase));
        }

        var totalItems = await query.CountAsync(ct);

        var expenses = await query
            .OrderBy(req.OrderBy ?? "-ExpenseDate")
            .ApplyPaging(req.Page, req.PageSize)
            .ToListAsync(ct);

        var dtos = expenses.Select(e => e.ToDto()).ToArray();

        return PagedResult<ExpenseDto>.Ok(dtos, totalItems, req.PageSize);
    }
}
