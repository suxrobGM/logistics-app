using Logistics.Domain.Entities;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Application.Abstractions.Tax;

namespace Logistics.Application.Modules.Financial.Tax.Services;

public static class TaxCalculationMappings
{
    extension(IEnumerable<InvoiceLineItem> lineItems)
    {
        /// <summary>Taxes each line on its net total, Amount * Quantity.</summary>
        public IReadOnlyList<TaxCalculationLineItem> ToCalculationLines() =>
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
        public IReadOnlyList<TaxCalculationLineItem> ToCalculationLines() =>
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
