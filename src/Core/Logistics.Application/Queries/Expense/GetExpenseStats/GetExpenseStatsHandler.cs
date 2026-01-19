using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetExpenseStatsHandler : IAppRequestHandler<GetExpenseStatsQuery, Result<ExpenseStatsDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetExpenseStatsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<Result<ExpenseStatsDto>> Handle(GetExpenseStatsQuery req, CancellationToken ct)
    {
        var query = _tenantUow.Repository<Expense>().Query();

        // Apply date filters
        if (req.FromDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate >= req.FromDate.Value);
        }

        if (req.ToDate.HasValue)
        {
            query = query.Where(e => e.ExpenseDate <= req.ToDate.Value);
        }

        var expenses = await query.ToListAsync(ct);

        // Calculate totals
        var totalAmount = expenses.Sum(e => e.Amount.Amount);
        var totalCount = expenses.Count;
        var pendingExpenses = expenses.Where(e => e.Status == ExpenseStatus.Pending).ToList();
        var approvedExpenses = expenses.Where(e => e.Status == ExpenseStatus.Approved).ToList();
        var paidExpenses = expenses.Where(e => e.Status == ExpenseStatus.Paid).ToList();

        // Group by type
        var byType = expenses
            .GroupBy(e => e.Type)
            .Select(g => new ExpenseTypeStatDto
            {
                Type = g.Key.ToString(),
                Amount = g.Sum(e => e.Amount.Amount),
                Count = g.Count()
            })
            .OrderByDescending(t => t.Amount)
            .ToList();

        // Group by company category
        var byCompanyCategory = expenses
            .OfType<CompanyExpense>()
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategoryStatDto
            {
                Category = g.Key.ToString(),
                Amount = g.Sum(e => e.Amount.Amount),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Group by truck category
        var byTruckCategory = expenses
            .OfType<TruckExpense>()
            .GroupBy(e => e.Category)
            .Select(g => new ExpenseCategoryStatDto
            {
                Category = g.Key.ToString(),
                Amount = g.Sum(e => e.Amount.Amount),
                Count = g.Count()
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        // Monthly trend (last 12 months)
        var monthlyTrend = expenses
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
            .ToList();

        // Top trucks by expense
        var truckExpenses = expenses
            .Where(e => e is TruckExpense or BodyShopExpense)
            .ToList();

        var topTrucks = new List<TruckExpenseStatDto>();

        var truckIds = truckExpenses
            .Select(e => e switch
            {
                TruckExpense te => te.TruckId,
                BodyShopExpense bse => bse.TruckId,
                _ => Guid.Empty
            })
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        foreach (var truckId in truckIds)
        {
            var truck = await _tenantUow.Repository<Truck>().GetByIdAsync(truckId);
            if (truck != null)
            {
                var truckAmount = truckExpenses
                    .Where(e => (e is TruckExpense te && te.TruckId == truckId) ||
                                (e is BodyShopExpense bse && bse.TruckId == truckId))
                    .Sum(e => e.Amount.Amount);

                var truckCount = truckExpenses
                    .Count(e => (e is TruckExpense te && te.TruckId == truckId) ||
                                (e is BodyShopExpense bse && bse.TruckId == truckId));

                topTrucks.Add(new TruckExpenseStatDto
                {
                    TruckId = truckId,
                    TruckNumber = truck.Number,
                    TotalAmount = truckAmount,
                    ExpenseCount = truckCount
                });
            }
        }

        topTrucks = topTrucks.OrderByDescending(t => t.TotalAmount).Take(10).ToList();

        var stats = new ExpenseStatsDto
        {
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            PendingAmount = pendingExpenses.Sum(e => e.Amount.Amount),
            PendingCount = pendingExpenses.Count,
            ApprovedAmount = approvedExpenses.Sum(e => e.Amount.Amount),
            ApprovedCount = approvedExpenses.Count,
            PaidAmount = paidExpenses.Sum(e => e.Amount.Amount),
            PaidCount = paidExpenses.Count,
            ByType = byType,
            ByCompanyCategory = byCompanyCategory,
            ByTruckCategory = byTruckCategory,
            MonthlyTrend = monthlyTrend,
            TopTrucks = topTrucks
        };

        return Result<ExpenseStatsDto>.Ok(stats);
    }
}
