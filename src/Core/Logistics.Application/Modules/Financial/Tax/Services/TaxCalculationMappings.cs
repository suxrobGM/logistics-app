using Logistics.Application.Abstractions.Payments;
using Logistics.Application.Modules.Financial.Invoices;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

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
                    NetAmount = li.Amount * li.Quantity,
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
            decimal subtotal,
            decimal taxTotal,
            decimal total) => new()
            {
                TaxBehavior = result.TaxBehavior,
                Subtotal = subtotal,
                TaxTotal = taxTotal,
                Total = total,
                Breakdown = result.Breakdown.ToDto(),
                Lines = result.Lines.ToDto(),
                Warning = result.Warning
            };
    }
}
