using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Invoice;

/// <summary>
/// Right-aligned totals box: Subtotal / Tax / Total / Paid / Balance Due.
/// </summary>
internal static class InvoiceTotalsSection
{
    public static void Render(IContainer container, LoadInvoice invoice, string taxLabel, bool showTax)
    {
        var currency = invoice.Total.Currency;

        container.AlignRight().Width(220).Column(totals =>
        {
            totals.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10);

            Row(totals, "Subtotal:", PdfFormatting.Money(invoice.Subtotal.Amount, currency));

            if (showTax)
            {
                var taxValue = invoice.TaxBehavior == TaxBehavior.ReverseCharge
                    ? "0.00 (reverse charge)"
                    : PdfFormatting.Money(invoice.TaxTotal.Amount, currency);
                Row(totals, $"{taxLabel}:", taxValue, paddingTop: 3);
            }

            totals.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text("Total:").FontSize(14).Bold();
                row.ConstantItem(100).AlignRight()
                    .Text(PdfFormatting.Money(invoice.Total.Amount, currency))
                    .FontSize(14).Bold().FontColor(Colors.Blue.Darken3);
            });

            var paid = invoice.Payments.Where(p => p.Status == PaymentStatus.Paid).ToList();
            if (paid.Count == 0) return;

            var totalPaid = paid.Sum(p => p.Amount.Amount);
            totals.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Text("Paid:").FontColor(Colors.Green.Darken2);
                row.ConstantItem(100).AlignRight()
                    .Text($"-{PdfFormatting.Money(totalPaid, currency)}")
                    .FontColor(Colors.Green.Darken2);
            });

            var balance = invoice.Total.Amount - totalPaid;
            totals.Item().Row(row =>
            {
                row.RelativeItem().Text("Balance Due:").FontSize(12).Bold();
                row.ConstantItem(100).AlignRight()
                    .Text(PdfFormatting.Money(balance, currency))
                    .FontSize(12).Bold()
                    .FontColor(balance > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
            });
        });
    }

    private static void Row(ColumnDescriptor col, string label, string value, int paddingTop = 0)
    {
        var item = paddingTop == 0 ? col.Item() : col.Item().PaddingTop(paddingTop);
        item.Row(row =>
        {
            row.RelativeItem().Text(label).Bold();
            row.ConstantItem(100).AlignRight().Text(value);
        });
    }
}
