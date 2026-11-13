using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Expenses.Queries;

internal sealed class GetExpenseStatsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetExpenseStatsQuery, Result<ExpenseStatsDto>>
{
    private const int TopTruckCount = 10;

    public async Task<Result<ExpenseStatsDto>> Handle(GetExpenseStatsQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<Expense>().Query();

        if (req.FromDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= req.FromDate.Value);
        }

        if (req.ToDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= req.ToDate.Value);
        }

        var expenses = await query.ToListAsync(ct);
        var pending = expenses.Where(e => e.Status == ExpenseStatus.Pending).ToList();
        var approved = expenses.Where(e => e.Status == ExpenseStatus.Approved).ToList();
        var paid = expenses.Where(e => e.Status == ExpenseStatus.Paid).ToList();

        var stats = new ExpenseStatsDto
        {
            TotalAmount = expenses.Sum(e => e.Amount.Amount),
            TotalCount = expenses.Count,
            PendingAmount = pending.Sum(e => e.Amount.Amount),
            PendingCount = pending.Count,
            ApprovedAmount = approved.Sum(e => e.Amount.Amount),
            ApprovedCount = approved.Count,
            PaidAmount = paid.Sum(e => e.Amount.Amount),
            PaidCount = paid.Count,
            ByType = expenses
                .GroupBy(e => e.Type)
                .Select(g => new ExpenseTypeStatDto
                {
                    Type = g.Key.ToString(),
                    Amount = g.Sum(e => e.Amount.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(t => t.Amount)
                .ToList(),
            ByCompanyCategory = SumByCategory(expenses.OfType<CompanyExpense>(), e => e.Category.ToString()),
            ByTruckCategory = SumByCategory(expenses.OfType<TruckExpense>(), e => e.Category.ToString()),
            MonthlyTrend = expenses
                .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
                .Select(g => new ExpenseMonthlyStatDto
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(e => e.Amount.Amount),
                    Count = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToList(),
            TopTrucks = await GetTopTrucksAsync(expenses.OfType<TruckLinkedExpense>(), ct)
        };

        return Result<ExpenseStatsDto>.Ok(stats);
    }

    private static List<ExpenseCategoryStatDto> SumByCategory<T>(IEnumerable<T> expenses, Func<T, string> category)
        where T : Expense
    {
        return expenses
            .GroupBy(category)
            .Select(g => new ExpenseCategoryStatDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount.Amount),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();
    }

    private async Task<List<TruckExpenseStatDto>> GetTopTrucksAsync(
        IEnumerable<TruckLinkedExpense> expenses,
        CancellationToken ct)
    {
        var totals = expenses
            .GroupBy(e => e.TruckId)
            .Select(g => new { TruckId = g.Key, Amount = g.Sum(e => e.Amount.Amount), Count = g.Count() })
            .OrderByDescending(t => t.Amount)
            .Take(TopTruckCount)
            .ToList();

        // One query for the top trucks' numbers; reading Truck off each expense would lazy-load per row.
        var truckIds = totals.Select(t => t.TruckId).ToArray();
        var truckNumbers = await tenantUow.Repository<Truck>().Query()
            .Where(t => truckIds.Contains(t.Id))
            .Select(t => new { t.Id, t.Number })
            .ToDictionaryAsync(t => t.Id, t => t.Number, ct);

        return totals
            .Select(t => new TruckExpenseStatDto
            {
                TruckId = t.TruckId,
                TruckNumber = truckNumbers[t.TruckId],
                TotalAmount = t.Amount,
                ExpenseCount = t.Count
            })
            .ToList();
    }
}
