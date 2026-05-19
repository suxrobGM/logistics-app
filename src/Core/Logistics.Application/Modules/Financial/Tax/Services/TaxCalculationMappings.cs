using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Application.Modules.Financial.Tax.Services;

/// <summary>
/// Mapping helpers between calculator inputs/outputs (which live in
/// <c>Logistics.Application.Services.Tax</c>) and either domain entities or shared DTOs.
/// Lives in Application because <c>Logistics.Mappings</c> doesn't reference Application,
/// so calculator types are out of its scope.
/// </summary>
public static class TaxCalculationMappings
{
    /// <summary>
    /// Builds calculator-line inputs from invoice line items, using each line's net total
    /// (Amount * Quantity) as the taxable base.
    /// </summary>
    public static IReadOnlyList<TaxCalculationLineItem> ToCalculationLines(
        this IEnumerable<InvoiceLineItem> lineItems) =>
        lineItems
            .Select(li => new TaxCalculationLineItem
            {
                LineItemId = li.Id,
                NetAmount = li.Amount.Amount * li.Quantity,
                TaxCode = li.TaxCode,
                Description = li.Description
            })
            .ToList();

    /// <summary>
    /// Builds calculator-line inputs from the preview-tax request DTOs.
    /// </summary>
    public static IReadOnlyList<TaxCalculationLineItem> ToCalculationLines(
        this IEnumerable<PreviewInvoiceTaxLineItem> lineItems) =>
        lineItems
            .Select(li => new TaxCalculationLineItem
            {
                LineItemId = li.LineItemId,
                NetAmount = li.Amount * li.Quantity,
                TaxCode = li.TaxCode,
                Description = li.Description
            })
            .ToList();

    public static PreviewLineItemTax ToDto(this TaxCalculationLineResult line) => new()
    {
        LineItemId = line.LineItemId,
        RatePercent = line.RatePercent,
        TaxAmount = line.TaxAmount,
        TaxCode = line.TaxCode
    };

    public static IEnumerable<PreviewLineItemTax> ToDto(this IEnumerable<TaxCalculationLineResult> lines) =>
        lines.Select(ToDto);

    /// <summary>
    /// Translate a <see cref="TaxCalculationResult"/> into the API's preview response, given the
    /// invoice currency and pre-tax totals.
    /// </summary>
    public static PreviewInvoiceTaxResponse ToPreviewResponse(
        this TaxCalculationResult result,
        string currency,
        decimal subtotal,
        decimal taxTotal,
        decimal total) => new()
        {
            TaxBehavior = result.TaxBehavior,
            Subtotal = new() { Amount = subtotal, Currency = currency },
            TaxTotal = new() { Amount = taxTotal, Currency = currency },
            Total = new() { Amount = total, Currency = currency },
            Breakdown = result.Breakdown.ToDto(),
            Lines = result.Lines.ToDto(),
            Warning = result.Warning
        };
}
