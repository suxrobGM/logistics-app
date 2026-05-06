using Logistics.Domain.Entities;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Invoice;

/// <summary>
/// Top section of the load invoice: company letterhead, invoice number, bill-to,
/// dates/status, and a load reference line.
/// </summary>
internal static class InvoiceHeaderSection
{
    public static void Render(IContainer container, LoadInvoice invoice, Tenant tenant)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => CompanyInfoColumn.Render(c, tenant, includeTaxIds: true));
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().AlignRight().Text("INVOICE")
                        .FontSize(28).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().AlignRight().Text($"#{invoice.Number}")
                        .FontSize(14).Bold();
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILL TO").Bold().FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(5).Text(invoice.Customer?.Name ?? "N/A")
                        .FontSize(12).Bold();
                    if (!string.IsNullOrEmpty(invoice.Customer?.TaxId))
                    {
                        col.Item().Text($"Tax ID: {invoice.Customer.TaxId}").FontSize(9);
                    }
                });

                row.ConstantItem(180).Column(col =>
                {
                    KeyValueRow(col, "Issue Date:", invoice.CreatedAt.ToString("MMM dd, yyyy"));
                    if (invoice.DueDate.HasValue)
                    {
                        KeyValueRow(col, "Due Date:", invoice.DueDate.Value.ToString("MMM dd, yyyy"));
                    }
                    KeyValueRow(col, "Status:", PdfFormatting.Display(invoice.Status));
                });
            });

            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Load #: {invoice.Load?.Number}")
                        .FontSize(11).Bold();

                    if (invoice.Load is { OriginAddress: var origin, DestinationAddress: var dest })
                    {
                        col.Item().Text($"Route: {origin?.City}, {origin?.State} → {dest?.City}, {dest?.State}")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });
            });
        });
    }

    private static void KeyValueRow(ColumnDescriptor col, string label, string value) =>
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label).Bold();
            r.ConstantItem(100).AlignRight().Text(value);
        });
}
