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
                (e.VendorName ?? "").ToLower().Contains(search) ||
                (e.Notes ?? "").ToLower().Contains(search));
        }

        var totalItems = await query.CountAsync(ct);

        // Apply sorting
        query = ApplySorting(query, req.OrderBy);

        // Apply pagination
        var expenses = await query
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToListAsync(ct);

        var dtos = expenses.Select(e => e.ToDto()).ToArray();

        return PagedResult<ExpenseDto>.Succeed(dtos, totalItems, req.PageSize);
    }

    private static IQueryable<Expense> ApplySorting(IQueryable<Expense> query, string? orderBy)
    {
        if (string.IsNullOrEmpty(orderBy))
        {
            return query.OrderByDescending(e => e.ExpenseDate);
        }

        var descending = orderBy.StartsWith('-');
        var field = descending ? orderBy[1..] : orderBy;

        return field.ToLowerInvariant() switch
        {
            "expensedate" => descending
                ? query.OrderByDescending(e => e.ExpenseDate)
                : query.OrderBy(e => e.ExpenseDate),
            "amount" => descending
                ? query.OrderByDescending(e => e.Amount.Amount)
                : query.OrderBy(e => e.Amount.Amount),
            "number" => descending ? query.OrderByDescending(e => e.Number) : query.OrderBy(e => e.Number),
            "status" => descending ? query.OrderByDescending(e => e.Status) : query.OrderBy(e => e.Status),
            "type" => descending ? query.OrderByDescending(e => e.Type) : query.OrderBy(e => e.Type),
            "createdat" => descending ? query.OrderByDescending(e => e.CreatedAt) : query.OrderBy(e => e.CreatedAt),
            _ => query.OrderByDescending(e => e.ExpenseDate)
        };
    }
}
