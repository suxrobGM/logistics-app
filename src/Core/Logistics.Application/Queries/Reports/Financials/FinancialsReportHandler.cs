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
        var revenueTrends = CalculateRevenueTrends(invoices, req.StartDate, req.EndDate);
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

    private List<PaymentTrendDto> CalculatePaymentTrends(
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

    private StatusDistributionDto GetStatusDistribution(
        List<LoadInvoice> invoices,
        int fullyPaidInvoices,
        int partiallyPaidInvoices,
        int unpaidInvoices)
    {
        return new StatusDistributionDto
        {
            PaidPercentage = invoices.Any() ? (fullyPaidInvoices * 100.0) / invoices.Count : 0,
            PartialPercentage = invoices.Any() ? (partiallyPaidInvoices * 100.0) / invoices.Count : 0,
            UnpaidPercentage = invoices.Any() ? (unpaidInvoices * 100.0) / invoices.Count : 0
        };
    }

    private List<RevenueTrendDto> CalculateRevenueTrends(List<LoadInvoice> invoices, DateTime startDate, DateTime endDate)
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
            var expenses = revenue * 0.3m; // Assuming 30% expenses
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

    private List<FinancialMetricDto> CalculateFinancialMetrics(List<LoadInvoice> invoices, decimal totalInvoiced, decimal totalPaid, decimal totalDue)
    {
        var avgInvoiceValue = invoices.Any() ? totalInvoiced / invoices.Count : 0;
        var collectionRate = totalInvoiced > 0 ? (double)(totalPaid / totalInvoiced) * 100 : 0;
        var overdueAmount = GetOverdueAmount(invoices);
        var overdueCount = GetOverdueInvoicesCount(invoices);

        return new List<FinancialMetricDto>
        {
            new() { Metric = "Average Invoice Value", Value = avgInvoiceValue, Unit = "$", Trend = 5.2, Category = "Revenue" },
            new() { Metric = "Collection Rate", Value = (decimal)collectionRate, Unit = "%", Trend = 3.1, Category = "Performance" },
            new() { Metric = "Outstanding Amount", Value = totalDue, Unit = "$", Trend = -2.5, Category = "Risk" },
            new() { Metric = "Overdue Amount", Value = overdueAmount, Unit = "$", Trend = -1.8, Category = "Risk" },
            new() { Metric = "Overdue Invoices", Value = overdueCount, Unit = "count", Trend = -0.9, Category = "Risk" }
        };
    }

    private decimal GetOverdueAmount(List<LoadInvoice> invoices)
    {
        var today = DateTime.Today;
        return invoices
            .Where(i => i.DueDate.HasValue && i.DueDate < today && i.Payments.Sum(p => p.Amount.Amount) < i.Total.Amount)
            .Sum(i => i.Total.Amount - i.Payments.Sum(p => p.Amount.Amount));
    }

    private int GetOverdueInvoicesCount(List<LoadInvoice> invoices)
    {
        var today = DateTime.Today;
        return invoices
            .Count(i => i.DueDate.HasValue && i.DueDate < today && i.Payments.Sum(p => p.Amount.Amount) < i.Total.Amount);
    }
}
