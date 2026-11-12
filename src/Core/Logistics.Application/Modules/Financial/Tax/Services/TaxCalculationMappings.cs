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
    extension(IEnumerable<InvoiceLineItem> lineItems)
    {
        /// <summary>
        /// Builds calculator-line inputs from invoice line items, using each line's net total
        /// (Amount * Quantity) as the taxable base.
        /// </summary>
        public IReadOnlyList<TaxCalculationLineItem> ToCalculationLines(
            ) =>
            lineItems
                .Select(li => new TaxCalculationLineItem
                {
                    LineItemId = li.Id,
                    NetAmount = li.Amount.Amount * li.Quantity,
                    TaxCode = li.TaxCode,
                    Description = li.Description
                })
                .ToList();
    }

    extension(IEnumerable<PreviewInvoiceTaxLineItem> lineItems)
    {
        /// <summary>
        /// Builds calculator-line inputs from the preview-tax request DTOs.
        /// </summary>
        public IReadOnlyList<TaxCalculationLineItem> ToCalculationLines(
            ) =>
            lineItems
                .Select(li => new TaxCalculationLineItem
                {
                    LineItemId = li.LineItemId,
                    NetAmount = li.Amount * li.Quantity,
                    TaxCode = li.TaxCode,
                    Description = li.Description
                })
                .ToList();
    }

    extension(TaxCalculationLineResult line)
    {
        public PreviewLineItemTax ToDto() => new()
        {
            LineItemId = line.LineItemId,
            RatePercent = line.RatePercent,
            TaxAmount = line.TaxAmount,
            TaxCode = line.TaxCode
        };
    }

    extension(IEnumerable<TaxCalculationLineResult> lines)
    {
        public IEnumerable<PreviewLineItemTax> ToDto() =>
            lines.Select(ToDto);
    }

    extension(TaxCalculationResult result)
    {
        /// <summary>
        /// Translate a <see cref="TaxCalculationResult"/> into the API's preview response, given the
        /// invoice currency and pre-tax totals.
        /// </summary>
        public PreviewInvoiceTaxResponse ToPreviewResponse(
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
}
