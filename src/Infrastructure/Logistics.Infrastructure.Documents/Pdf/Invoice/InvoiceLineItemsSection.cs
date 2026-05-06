using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Invoice;

/// <summary>
/// Line-items table + (when applicable) reverse-charge notice and per-jurisdiction
/// tax breakdown panel. The Net | Rate | Tax | Gross columns only render when the
/// invoice actually has tax.
/// </summary>
internal static class InvoiceLineItemsSection
{
    private const string ReverseChargeNotice =
        "Reverse charge — VAT to be accounted for by the recipient " +
        "(Article 196, Council Directive 2006/112/EC).";

    public static void Render(IContainer container, LoadInvoice invoice, string taxLabel)
    {
        var currency = invoice.Total.Currency;
        var showTax = ShouldShowTaxColumns(invoice);

        container.Column(column =>
        {
            column.Spacing(15);

            column.Item().Element(c => RenderLineItemsTable(c, invoice, currency, taxLabel, showTax));

            if (invoice.TaxBehavior == TaxBehavior.ReverseCharge)
            {
                column.Item().Element(RenderReverseChargeNotice);
            }

            var breakdown = invoice.GetTaxBreakdown();
            if (showTax && breakdown.Count > 1)
            {
                column.Item().Element(c => RenderTaxBreakdownTable(c, breakdown, currency, taxLabel));
            }
        });
    }

    private static bool ShouldShowTaxColumns(LoadInvoice invoice) =>
        invoice.TaxBehavior != TaxBehavior.Exclusive
        || invoice.TaxTotal.Amount > 0m
        || invoice.LineItems.Any(li => li.TaxAmount > 0m || li.TaxRatePercent > 0m);

    private static void RenderLineItemsTable(
        IContainer container, LoadInvoice invoice, string currency, string taxLabel, bool showTax)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(c =>
            {
                c.RelativeColumn(showTax ? 4 : 5);  // Description
                c.ConstantColumn(50);               // Qty
                c.ConstantColumn(80);               // Unit
                c.ConstantColumn(80);               // Net
                if (showTax)
                {
                    c.ConstantColumn(45);           // Rate
                    c.ConstantColumn(70);           // Tax
                    c.ConstantColumn(80);           // Gross
                }
            });

            table.Header(h =>
            {
                h.Cell().Element(PdfStyles.HeaderCell).Text("Description");
                h.Cell().Element(PdfStyles.HeaderCell).AlignCenter().Text("Qty");
                h.Cell().Element(PdfStyles.HeaderCell).AlignRight().Text("Unit");
                h.Cell().Element(PdfStyles.HeaderCell).AlignRight().Text("Net");
                if (showTax)
                {
                    h.Cell().Element(PdfStyles.HeaderCell).AlignRight().Text("Rate");
                    h.Cell().Element(PdfStyles.HeaderCell).AlignRight().Text(taxLabel);
                    h.Cell().Element(PdfStyles.HeaderCell).AlignRight().Text("Gross");
                }
            });

            var lines = invoice.LineItems.OrderBy(li => li.Order).ToList();
            if (lines.Count == 0 && invoice.Load is not null)
            {
                RenderFallbackRow(table, invoice.Load.DeliveryCost.Amount, currency, showTax);
                return;
            }

            foreach (var item in lines)
            {
                table.Cell().Element(PdfStyles.Cell).Text(item.Description);
                table.Cell().Element(PdfStyles.Cell).AlignCenter().Text(item.Quantity.ToString());
                table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(item.Amount.Amount, currency));
                table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(item.Total, currency));
                if (showTax)
                {
                    table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Rate(item.TaxRatePercent));
                    table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(item.TaxAmount, currency));
                    table.Cell().Element(PdfStyles.Cell).AlignRight()
                        .Text(PdfFormatting.Money(item.Total + item.TaxAmount, currency));
                }
            }
        });
    }

    private static void RenderFallbackRow(TableDescriptor table, decimal amount, string currency, bool showTax)
    {
        table.Cell().Element(PdfStyles.Cell).Text("Delivery Service");
        table.Cell().Element(PdfStyles.Cell).AlignCenter().Text("1");
        table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(amount, currency));
        table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(amount, currency));
        if (showTax)
        {
            table.Cell().Element(PdfStyles.Cell).AlignRight().Text("—");
            table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(0m, currency));
            table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(amount, currency));
        }
    }

    private static void RenderReverseChargeNotice(IContainer container) =>
        container.Background(Colors.Yellow.Lighten4)
                 .Border(1).BorderColor(Colors.Yellow.Darken2)
                 .Padding(10)
                 .Text(ReverseChargeNotice).Bold().FontSize(10);

    private static void RenderTaxBreakdownTable(
        IContainer container,
        IReadOnlyList<InvoiceTaxLine> breakdown,
        string currency,
        string taxLabel)
    {
        container.Column(b =>
        {
            b.Item().Text($"{taxLabel} breakdown")
                .FontSize(11).Bold().FontColor(Colors.Grey.Darken2);

            b.Item().PaddingTop(5).Table(t =>
            {
                t.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(3);
                    c.ConstantColumn(60);
                    c.ConstantColumn(90);
                    c.ConstantColumn(90);
                });
                t.Header(h =>
                {
                    h.Cell().Element(PdfStyles.SmallHeaderCell).Text("Jurisdiction");
                    h.Cell().Element(PdfStyles.SmallHeaderCell).AlignRight().Text("Rate");
                    h.Cell().Element(PdfStyles.SmallHeaderCell).AlignRight().Text("Base");
                    h.Cell().Element(PdfStyles.SmallHeaderCell).AlignRight().Text("Amount");
                });

                foreach (var line in breakdown)
                {
                    t.Cell().Element(PdfStyles.SmallCell).Text(line.Description ?? line.Jurisdiction.ToString());
                    t.Cell().Element(PdfStyles.SmallCell).AlignRight().Text(PdfFormatting.Rate(line.RatePercent));
                    t.Cell().Element(PdfStyles.SmallCell).AlignRight().Text(PdfFormatting.Money(line.BaseAmount, currency));
                    t.Cell().Element(PdfStyles.SmallCell).AlignRight().Text(PdfFormatting.Money(line.TaxAmount, currency));
                }
            });
        });
    }
}
