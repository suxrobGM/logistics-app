using Logistics.Domain.Primitives.Enums;

namespace Logistics.Shared.Models;

public record FinancialsReportDto
{
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal TotalDue { get; set; }

    public int FullyPaidInvoices { get; set; }
    public int PartiallyPaidInvoices { get; set; }
    public int UnpaidInvoices { get; set; }
    public decimal AverageInvoiceValue { get; set; }
    public decimal CollectionRate { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal OverdueAmount { get; set; }
    public int OverdueInvoices { get; set; }

    public List<PaymentTrendDto>? PaymentTrends { get; set; }
    public List<TopCustomerDto>? TopCustomers { get; set; }
    public StatusDistributionDto? StatusDistribution { get; set; }
    public List<RevenueTrendDto>? RevenueTrends { get; set; }
    public List<FinancialMetricDto>? FinancialMetrics { get; set; }
}

public class PaymentTrendDto
{
    public string? Period { get; set; }
    public decimal Invoiced { get; set; }
    public decimal Paid { get; set; }
    public double CollectionRate { get; set; }
}

public class TopCustomerDto
{
    public string? CustomerName { get; set; }
    public decimal TotalInvoiced { get; set; }
    public decimal TotalPaid { get; set; }
    public int InvoiceCount { get; set; }
}

public class StatusDistributionDto
{
    public double PaidPercentage { get; set; }
    public double PartialPercentage { get; set; }
    public double UnpaidPercentage { get; set; }
}

public class RevenueTrendDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public decimal Profit { get; set; }
    public decimal Expenses { get; set; }
    public double ProfitMargin { get; set; }
}

public class FinancialMetricDto
{
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public double Trend { get; set; }
    public string Category { get; set; } = string.Empty;
}
