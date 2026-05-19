using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Tax;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

internal sealed class PreviewInvoiceTaxHandler(
    ITenantUnitOfWork tenantUow,
    ITaxCalculator calculator)
    : IAppRequestHandler<PreviewInvoiceTaxQuery, Result<PreviewInvoiceTaxResponse>>
{
    public async Task<Result<PreviewInvoiceTaxResponse>> Handle(PreviewInvoiceTaxQuery req, CancellationToken ct)
    {
        var customer = await tenantUow.Repository<Customer>().GetByIdAsync(req.Request.CustomerId, ct);
        if (customer is null)
        {
            return Result<PreviewInvoiceTaxResponse>.Fail(
                $"Could not find a customer with ID '{req.Request.CustomerId}'");
        }

        if (customer.Address is null)
        {
            return Result<PreviewInvoiceTaxResponse>.Fail(
                "Customer has no billing address; cannot determine tax jurisdiction.");
        }

        var tenant = tenantUow.GetCurrentTenant();
        var currency = req.Request.Currency;

        var calcRequest = new TaxCalculationRequest
        {
            Currency = currency,
            TenantId = tenant.Id,
            TenantRegion = tenant.Settings?.Region ?? Region.US,
            TenantAddress = tenant.CompanyAddress,
            TenantTaxId = tenant.VatNumber,
            TenantTaxResidencyCountry = tenant.TaxResidencyCountry,
            CustomerAddress = customer.Address,
            CustomerTaxId = customer.TaxId,
            IsCustomerVatExempt = customer.IsVatExempt,
            LineItems = req.Request.LineItems.ToCalculationLines()
        };

        var result = await calculator.CalculateAsync(calcRequest, ct);

        var subtotal = decimal.Round(req.Request.LineItems.Sum(l => l.Amount * l.Quantity), 2);
        var taxTotal = decimal.Round(result.Lines.Sum(l => l.TaxAmount), 2);
        var total = result.TaxBehavior == TaxBehavior.ReverseCharge ? subtotal : subtotal + taxTotal;

        return Result<PreviewInvoiceTaxResponse>.Ok(
            result.ToPreviewResponse(currency, subtotal, taxTotal, total));
    }
}
