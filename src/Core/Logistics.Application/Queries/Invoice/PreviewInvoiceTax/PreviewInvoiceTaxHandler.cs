using Logistics.Application.Abstractions;
using Logistics.Application.Services.Tax;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

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
            TenantRegion = tenant.Settings?.Region ?? Region.Us,
            TenantAddress = tenant.CompanyAddress,
            TenantTaxId = tenant.VatNumber,
            TenantTaxResidencyCountry = tenant.TaxResidencyCountry,
            CustomerAddress = customer.Address,
            CustomerTaxId = customer.TaxId,
            IsCustomerVatExempt = customer.IsVatExempt,
            LineItems = req.Request.LineItems
                .Select(li => new TaxCalculationLineItem
                {
                    LineItemId = li.LineItemId,
                    NetAmount = li.Amount * li.Quantity,
                    TaxCode = li.TaxCode,
                    Description = li.Description
                })
                .ToList()
        };

        var result = await calculator.CalculateAsync(calcRequest, ct);

        var subtotal = decimal.Round(req.Request.LineItems.Sum(l => l.Amount * l.Quantity), 2);
        var taxTotal = decimal.Round(result.Lines.Sum(l => l.TaxAmount), 2);
        var total = result.TaxBehavior == TaxBehavior.ReverseCharge
            ? subtotal
            : subtotal + taxTotal;

        var response = new PreviewInvoiceTaxResponse
        {
            TaxBehavior = result.TaxBehavior,
            Subtotal = new Money { Amount = subtotal, Currency = currency },
            TaxTotal = new Money { Amount = taxTotal, Currency = currency },
            Total = new Money { Amount = total, Currency = currency },
            Breakdown = result.Breakdown.Select(MapTaxLine),
            Lines = result.Lines.Select(l => new PreviewLineItemTax
            {
                LineItemId = l.LineItemId,
                RatePercent = l.RatePercent,
                TaxAmount = l.TaxAmount,
                TaxCode = l.TaxCode
            }),
            Warning = result.Warning
        };

        return Result<PreviewInvoiceTaxResponse>.Ok(response);
    }

    private static InvoiceTaxLineDto MapTaxLine(InvoiceTaxLine line) => new()
    {
        RatePercent = line.RatePercent,
        BaseAmount = line.BaseAmount,
        TaxAmount = line.TaxAmount,
        TaxCode = line.TaxCode,
        Description = line.Description,
        Jurisdiction = new TaxJurisdictionDto
        {
            CountryCode = line.Jurisdiction.CountryCode,
            Region = line.Jurisdiction.Region
        }
    };
}
