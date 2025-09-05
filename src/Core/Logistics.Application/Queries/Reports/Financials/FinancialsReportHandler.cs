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
            invoicesQuery = invoicesQuery.Where(i => i.CreatedAt >= req.StartDate);
        }

        if (req.EndDate != default)
        {
            invoicesQuery = invoicesQuery.Where(i => i.CreatedAt <= req.EndDate);
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
            PaymentTrends = paymentTrends,
            TopCustomers = topCustomers,
            StatusDistribution = statusDistribution
        };

        return Result<FinancialsReportDto>.Ok(dto);
    }

    private List<PaymentTrendDto> CalculatePaymentTrends(
        List<LoadInvoice> invoices,
        DateTime startDate,
        DateTime endDate)
    {
        var trends = new List<PaymentTrendDto>();
        var currentDate = new DateTime(startDate.Year, startDate.Month, 1);

        while (currentDate <= endDate)
        {
            var monthStart = new DateTime(currentDate.Year, currentDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

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
}
