using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Tax;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Application.Modules.Financial.Tax.Services;

internal sealed class InvoiceTaxApplier(
    ITaxCalculator calculator,
    ITenantUnitOfWork tenantUow,
    ILogger<InvoiceTaxApplier> logger) : IInvoiceTaxApplier
{
    public async Task ApplyAsync(Invoice invoice, CancellationToken ct = default)
    {
        // Subscription + Payroll invoices have no external customer to charge VAT to;
        // skip calculator and just keep totals consistent.
        if (invoice is not LoadInvoice loadInvoice || loadInvoice.Customer?.Address is null)
        {
            invoice.RecalculateTotals();
            return;
        }

        if (invoice.LineItems.Count == 0)
        {
            invoice.SetTaxBreakdown([]);
            invoice.RecalculateTotals();
            return;
        }

        var tenant = tenantUow.GetCurrentTenant();
        var request = new TaxCalculationRequest
        {
            Currency = invoice.Total.Currency,
            TenantId = tenant.Id,
            TenantRegion = tenant.Settings?.Region ?? Region.US,
            TenantAddress = tenant.CompanyAddress,
            TenantTaxId = tenant.VatNumber,
            TenantTaxResidencyCountry = tenant.TaxResidencyCountry,
            CustomerAddress = loadInvoice.Customer.Address,
            CustomerTaxId = loadInvoice.Customer.TaxId,
            IsCustomerVatExempt = loadInvoice.Customer.IsVatExempt,
            LineItems = invoice.LineItems.ToCalculationLines()
        };

        var result = await calculator.CalculateAsync(request, ct);
        if (!string.IsNullOrEmpty(result.Warning))
        {
            logger.LogWarning("Tax calculation warning for invoice {InvoiceId}: {Warning}",
                invoice.Id, result.Warning);
        }

        var perLine = result.Lines.ToDictionary(l => l.LineItemId);
        foreach (var li in invoice.LineItems)
        {
            if (perLine.TryGetValue(li.Id, out var line))
            {
                li.TaxRatePercent = line.RatePercent;
                li.TaxAmount = line.TaxAmount;
                li.TaxCode = line.TaxCode ?? li.TaxCode;
            }
            else
            {
                li.TaxAmount = 0m;
                li.TaxRatePercent = 0m;
            }
        }

        invoice.TaxBehavior = result.TaxBehavior;
        invoice.SetTaxBreakdown(result.Breakdown);
        invoice.RecalculateTotals();
    }
}
