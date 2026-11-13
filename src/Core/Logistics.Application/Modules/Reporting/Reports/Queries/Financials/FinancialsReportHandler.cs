using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Reporting.Reports.Queries;

internal sealed class FinancialsReportHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<FinancialsReportQuery, Result<FinancialsReportDto>>
{
    public async Task<Result<FinancialsReportDto>> Handle(FinancialsReportQuery req, CancellationToken ct)
    {
        var invoicesQuery = tenantUow.Repository<LoadInvoice>().Query()
            .Where(i => InvoiceStatuses.Receivable.Contains(i.Status));

        if (req.StartDate != default)
        {
            var from = DateTime.SpecifyKind(req.StartDate, DateTimeKind.Utc);
            invoicesQuery = invoicesQuery.Where(i => i.CreatedAt >= from);
        }

        if (req.EndDate != default)
        {
            var to = DateTime.SpecifyKind(req.EndDate, DateTimeKind.Utc);
            invoicesQuery = invoicesQuery.Where(i => i.CreatedAt <= to);
        }

        var invoices = await invoicesQuery.ToListAsync(ct);

        var totalCount = invoices.Count;
        var totalInvoiced = invoices.Sum(i => i.Total);
        var totalPaid = invoices.Sum(i => i.AmountPaid);
        var totalDue = totalInvoiced - totalPaid;

        var fullyPaidInvoices = invoices.Count(i => i.AmountPaid >= i.Total);
        var partiallyPaidInvoices = invoices.Count(i =>
            i.AmountPaid > 0 &&
            i.AmountPaid < i.Total);
        var unpaidInvoices = invoices.Count(i => i.AmountPaid == 0);

        var averageInvoiceValue = totalCount > 0 ? totalInvoiced / totalCount : 0;
        var collectionRate = totalInvoiced > 0 ? (totalPaid / totalInvoiced) * 100 : 0;
        var now = DateTime.UtcNow;
        var overdue = invoices.Where(i => InvoiceStatuses.Open.Contains(i.Status) && i.DueDate < now).ToList();
        var overdueAmount = overdue.Sum(i => i.AmountDue);
        var overdueInvoices = overdue.Count;

        var createdDates = invoices.Select(i => i.CreatedAt).DefaultIfEmpty(now).ToList();
        var paymentTrends = CalculatePaymentTrends(
            invoices,
            req.StartDate != default ? req.StartDate : createdDates.Min(),
            req.EndDate != default ? req.EndDate : createdDates.Max()
        );

        var topCustomers = GetTopCustomersByValue(invoices);
        var statusDistribution = GetStatusDistribution(invoices, fullyPaidInvoices, partiallyPaidInvoices, unpaidInvoices);
        var revenueTrends = await CalculateRevenueTrendsAsync(invoices, req.StartDate, req.EndDate, ct);
        List<FinancialMetricDto> financialMetrics =
        [
            new() { Metric = "Average Invoice Value", Value = averageInvoiceValue, Unit = "$", Category = "Revenue" },
            new() { Metric = "Collection Rate", Value = collectionRate, Unit = "%", Category = "Performance" },
            new() { Metric = "Outstanding Amount", Value = totalDue, Unit = "$", Category = "Risk" },
            new() { Metric = "Overdue Amount", Value = overdueAmount, Unit = "$", Category = "Risk" },
            new() { Metric = "Overdue Invoices", Value = overdueInvoices, Unit = "count", Category = "Risk" }
        ];

        var dto = new FinancialsReportDto
        {
            TotalInvoiced = totalInvoiced,
            TotalPaid = totalPaid,
            TotalDue = totalDue,
            FullyPaidInvoices = fullyPaidInvoices,
            PartiallyPaidInvoices = partiallyPaidInvoices,
            UnpaidInvoices = unpaidInvoices,
            AverageInvoiceValue = averageInvoiceValue,
            CollectionRate = collectionRate,
            OutstandingAmount = totalDue,
            OverdueAmount = overdueAmount,
            OverdueInvoices = overdueInvoices,
            PaymentTrends = paymentTrends,
            TopCustomers = topCustomers,
            StatusDistribution = statusDistribution,
            RevenueTrends = revenueTrends,
            FinancialMetrics = financialMetrics
        };

        return Result<FinancialsReportDto>.Ok(dto);
    }

    private static List<PaymentTrendDto> CalculatePaymentTrends(
        List<LoadInvoice> invoices,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<PaymentTrendDto>();
        var currentDate = DateTime.SpecifyKind(new DateTime(startDate.Year, startDate.Month, 1), DateTimeKind.Utc);

        while (currentDate <= endDate)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(currentDate.Year, currentDate.Month, 1), DateTimeKind.Utc);
            var nextMonthStart = monthStart.AddMonths(1);

            var monthInvoices = invoices
                .Where(i => i.CreatedAt >= monthStart && i.CreatedAt < nextMonthStart)
                .ToList();

            var monthTotal = monthInvoices.Sum(i => i.Total);
            var monthPaid = monthInvoices.Sum(i => i.AmountPaid);

            trends.Add(new PaymentTrendDto
            {
                Period = monthStart.ToString("MMM yyyy"),
                Invoiced = monthTotal,
                Paid = monthPaid,
                CollectionRate = (double)(monthTotal > 0 ? (monthPaid / monthTotal) * 100 : 0)
            });

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    private List<TopCustomerDto> GetTopCustomersByValue(List<LoadInvoice> invoices)
    {
        return invoices
            .GroupBy(i => i.Customer)
            .Select(g => new TopCustomerDto
            {
                CustomerName = g.Key.Name,
                TotalInvoiced = g.Sum(i => i.Total),
                TotalPaid = g.Sum(i => i.AmountPaid),
                InvoiceCount = g.Count()
            })
            .OrderByDescending(c => c.TotalInvoiced)
            .Take(5)
            .ToList();
    }

    private static StatusDistributionDto GetStatusDistribution(
        List<LoadInvoice> invoices,
        int fullyPaidInvoices,
        int partiallyPaidInvoices,
        int unpaidInvoices)
    {
        return new StatusDistributionDto
        {
            PaidPercentage = invoices.Count != 0 ? (fullyPaidInvoices * 100.0) / invoices.Count : 0,
            PartialPercentage = invoices.Count != 0 ? (partiallyPaidInvoices * 100.0) / invoices.Count : 0,
            UnpaidPercentage = invoices.Count != 0 ? (unpaidInvoices * 100.0) / invoices.Count : 0
        };
    }

    /// <summary>Cash-basis net profit per month: paid invoices minus approved expenses, payroll and factoring fees.</summary>
    private async Task<List<RevenueTrendDto>> CalculateRevenueTrendsAsync(
        List<LoadInvoice> invoices,
        DateTime startDate,
        DateTime endDate,
        CancellationToken ct)
    {
        var start = startDate == default ? DateTime.UtcNow.AddMonths(-6) : startDate;
        var end = endDate == default ? DateTime.UtcNow : endDate;
        var firstMonth = DateTime.SpecifyKind(new DateTime(start.Year, start.Month, 1), DateTimeKind.Utc);
        var afterLastMonth = DateTime.SpecifyKind(new DateTime(end.Year, end.Month, 1), DateTimeKind.Utc).AddMonths(1);

        var expensesByMonth = await SumExpensesByMonthAsync(firstMonth, afterLastMonth, ct);
        var payrollByMonth = await SumPayrollByMonthAsync(firstMonth, afterLastMonth, ct);
        var factoringByMonth = await SumFactoringFeesByMonthAsync(firstMonth, afterLastMonth, ct);

        var trends = new List<RevenueTrendDto>();
        for (var monthStart = firstMonth; monthStart < afterLastMonth; monthStart = monthStart.AddMonths(1))
        {
            var nextMonthStart = monthStart.AddMonths(1);
            var monthInvoices = invoices
                .Where(i => i.CreatedAt >= monthStart && i.CreatedAt < nextMonthStart)
                .ToList();

            var revenue = monthInvoices.Sum(i => i.Total);
            var paid = monthInvoices.Sum(i => i.AmountPaid);
            var month = (monthStart.Year, monthStart.Month);
            var expenses = expensesByMonth.GetValueOrDefault(month)
                + payrollByMonth.GetValueOrDefault(month)
                + factoringByMonth.GetValueOrDefault(month);

            var profit = paid - expenses;
            var profitMargin = revenue > 0 ? (double)(profit / revenue) * 100 : 0;

            trends.Add(new RevenueTrendDto
            {
                Period = monthStart.ToString("MMM yyyy"),
                Revenue = revenue,
                Profit = profit,
                Expenses = expenses,
                ProfitMargin = profitMargin
            });
        }

        return trends;
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> SumExpensesByMonthAsync(
        DateTime from, DateTime to, CancellationToken ct)
    {
        var rows = await tenantUow.Repository<Expense>().Query()
            .Where(e => ExpenseStatuses.Counted.Contains(e.Status))
            .Where(e => e.ExpenseDate >= from && e.ExpenseDate < to)
            .GroupBy(e => new { e.ExpenseDate.Year, e.ExpenseDate.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(e => e.Amount) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => (r.Year, r.Month), r => r.Total);
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> SumPayrollByMonthAsync(
        DateTime from, DateTime to, CancellationToken ct)
    {
        var fromDay = DateOnly.FromDateTime(from);
        var toDay = DateOnly.FromDateTime(to);
        var rows = await tenantUow.Repository<PayrollInvoice>().Query()
            .Where(p => p.Status == InvoiceStatus.Approved || InvoiceStatuses.Receivable.Contains(p.Status))
            .Where(p => p.PeriodEnd >= fromDay && p.PeriodEnd < toDay)
            .GroupBy(p => new { p.PeriodEnd.Year, p.PeriodEnd.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(p => p.Total) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => (r.Year, r.Month), r => r.Total);
    }

    private async Task<Dictionary<(int Year, int Month), decimal>> SumFactoringFeesByMonthAsync(
        DateTime from, DateTime to, CancellationToken ct)
    {
        var rows = await tenantUow.Repository<FactoringBatchItem>().Query()
            .Where(f => f.FeeAmount != null && f.FundedAt >= from && f.FundedAt < to)
            .GroupBy(f => new { f.FundedAt!.Value.Year, f.FundedAt!.Value.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Total = g.Sum(f => f.FeeAmount!.Value) })
            .ToListAsync(ct);
        return rows.ToDictionary(r => (r.Year, r.Month), r => r.Total);
    }
}
