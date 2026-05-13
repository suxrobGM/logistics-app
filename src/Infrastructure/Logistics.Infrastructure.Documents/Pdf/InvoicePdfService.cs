using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Services.Pdf.Invoice;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Logistics.Application.Abstractions.Documents;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Services.Pdf;

public class InvoicePdfService : IInvoicePdfService
{
    public byte[] GenerateLoadInvoicePdf(LoadInvoice invoice, Tenant tenant)
    {
        Settings.License = LicenseType.Community;

        var taxLabel = PdfFormatting.TaxLabel(tenant.Settings?.Region);
        var showTax = HasTax(invoice);

        var doc = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => InvoiceHeaderSection.Render(c, invoice, tenant));

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(15);
                    col.Item().Element(c => InvoiceLineItemsSection.Render(c, invoice, taxLabel));
                    col.Item().Element(c => InvoiceTotalsSection.Render(c, invoice, taxLabel, showTax));
                    col.Item().Element(c => PaymentHistoryTable.Render(c, invoice.Payments, invoice.Total.Currency));
                    if (!string.IsNullOrEmpty(invoice.Notes))
                    {
                        col.Item().Element(c => RenderNotes(c, invoice.Notes));
                    }
                });

                page.Footer().Element(c => PdfFooter.Render(c, tenant, "Thank you for your business!"));
            });
        });

        return doc.GeneratePdf();
    }

    private static bool HasTax(LoadInvoice invoice) =>
        invoice.TaxBehavior != TaxBehavior.Exclusive
        || invoice.TaxTotal.Amount > 0m
        || invoice.LineItems.Any(li => li.TaxAmount > 0m || li.TaxRatePercent > 0m);

    private static void RenderNotes(IContainer container, string notes)
    {
        container.Column(col =>
        {
            col.Item().Text("Notes").FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(5)
                .Background(Colors.Grey.Lighten4)
                .Padding(10)
                .Text(notes).FontSize(9);
        });
    }
}
