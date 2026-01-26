using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class FinancialsReportHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<FinancialsReportQuery, Result<FinancialsReportDto>>
{
    public async Task<Result<FinancialsReportDto>> Handle(FinancialsReportQuery req, CancellationToken ct)
    {
        var invoicesQuery = tenantUow.Repository<LoadInvoice>().Query();
        var expenseQuery = tenantUow.Repository<Expense>().Query();

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

        var invoices = invoicesQuery.ToList();

        var totalCount = invoices.Count;
        var totalInvoiced = invoices.Sum(i => i.Total.Amount);
        var totalPaid = invoices.Sum(i => i.Payments.Sum(p => p.Amount.Amount));
        var totalDue = totalInvoiced - totalPaid;

        var fullyPaidInvoices = invoices.Count(i => i.Payments.Sum(p => p.Amount.Amount) >= i.Total.Amount);
        var partiallyPaidInvoices = invoices.Count(i =>
            i.Payments.Sum(p => p.Amount.Amount) > 0 &&
            i.Payments.Sum(p => p.Amount.Amount) < i.Total.Amount);
        var unpaidInvoices = invoices.Count(i => i.Payments.Sum(p => p.Amount.Amount) == 0);

        var averageInvoiceValue = totalCount > 0 ? totalInvoiced / totalCount : 0;
        var collectionRate = totalInvoiced > 0 ? (totalPaid / totalInvoiced) * 100 : 0;

        var paymentTrends = CalculatePaymentTrends(
            invoices,
            req.StartDate == default ? invoices.Min(i => i.CreatedAt) : req.StartDate,
            req.EndDate == default ? invoices.Max(i => i.CreatedAt) : req.EndDate
        );

        var topCustomers = GetTopCustomersByValue(invoices);
        var statusDistribution = GetStatusDistribution(invoices, fullyPaidInvoices, partiallyPaidInvoices, unpaidInvoices);
        var revenueTrends = CalculateRevenueTrends(invoices, expenseQuery, req.StartDate, req.EndDate);
        var financialMetrics = CalculateFinancialMetrics(invoices, totalInvoiced, totalPaid, totalDue);

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
            OverdueAmount = GetOverdueAmount(invoices),
            OverdueInvoices = GetOverdueInvoicesCount(invoices),
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
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var monthInvoices = invoices
                .Where(i => i.CreatedAt >= monthStart && i.CreatedAt <= monthEnd)
                .ToList();

            var monthTotal = monthInvoices.Sum(i => i.Total.Amount);
            var monthPaid = monthInvoices.Sum(i => i.Payments.Sum(p => p.Amount.Amount));

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
                TotalInvoiced = g.Sum(i => i.Total.Amount),
                TotalPaid = g.Sum(i => i.Payments.Sum(p => p.Amount.Amount)),
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

    private static List<RevenueTrendDto> CalculateRevenueTrends(
        List<LoadInvoice> invoices,
        IQueryable<Expense> expenseQuery,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<RevenueTrendDto>();
        var currentDate = startDate == default ? DateTime.UtcNow.AddMonths(-6) : DateTime.SpecifyKind(startDate, DateTimeKind.Utc);
        var end = endDate == default ? DateTime.UtcNow : DateTime.SpecifyKind(endDate, DateTimeKind.Utc);

        while (currentDate <= end)
        {
            var monthStart = DateTime.SpecifyKind(new DateTime(currentDate.Year, currentDate.Month, 1), DateTimeKind.Utc);
            var monthEnd = DateTime.SpecifyKind(monthStart.AddMonths(1).AddDays(-1), DateTimeKind.Utc);

            var monthInvoices = invoices
                .Where(i => i.CreatedAt >= monthStart && i.CreatedAt <= monthEnd)
                .ToList();

            var revenue = monthInvoices.Sum(i => i.Total.Amount);
            var paid = monthInvoices.Sum(i => i.Payments.Sum(p => p.Amount.Amount));

            // Query actual expenses for this month
            var expenses = expenseQuery
                .Where(e => e.ExpenseDate >= monthStart && e.ExpenseDate <= monthEnd)
                .Sum(e => e.Amount.Amount);

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

            currentDate = currentDate.AddMonths(1);
        }

        return trends;
    }

    private static List<FinancialMetricDto> CalculateFinancialMetrics(List<LoadInvoice> invoices, decimal totalInvoiced, decimal totalPaid, decimal totalDue)
    {
        var avgInvoiceValue = invoices.Count != 0 ? totalInvoiced / invoices.Count : 0;
        var collectionRate = totalInvoiced > 0 ? (double)(totalPaid / totalInvoiced) * 100 : 0;
        var overdueAmount = GetOverdueAmount(invoices);
        var overdueCount = GetOverdueInvoicesCount(invoices);

        return
        [
            new() { Metric = "Average Invoice Value", Value = avgInvoiceValue, Unit = "$", Category = "Revenue" },
            new() { Metric = "Collection Rate", Value = (decimal)collectionRate, Unit = "%", Category = "Performance" },
            new() { Metric = "Outstanding Amount", Value = totalDue, Unit = "$", Category = "Risk" },
            new() { Metric = "Overdue Amount", Value = overdueAmount, Unit = "$", Category = "Risk" },
            new() { Metric = "Overdue Invoices", Value = overdueCount, Unit = "count", Category = "Risk" }
        ];
    }

    private static decimal GetOverdueAmount(List<LoadInvoice> invoices)
    {
        var today = DateTime.Today;
        return invoices
            .Where(i => i.DueDate.HasValue && i.DueDate < today && i.Payments.Sum(p => p.Amount.Amount) < i.Total.Amount)
            .Sum(i => i.Total.Amount - i.Payments.Sum(p => p.Amount.Amount));
    }

    private static int GetOverdueInvoicesCount(List<LoadInvoice> invoices)
    {
        var today = DateTime.Today;
        return invoices
            .Count(i => i.DueDate.HasValue && i.DueDate < today && i.Payments.Sum(p => p.Amount.Amount) < i.Total.Amount);
    }
}
