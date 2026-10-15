using Logistics.Application.Abstractions.Documents;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Documents.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Documents.Pdf.Factoring;

internal sealed class FactoringSchedulePdfService : IFactoringSchedulePdfService
{
    public byte[] GenerateSchedulePdf(FactoringBatch batch, Tenant tenant)
    {
        var currency = batch.Total.Currency;
        var companyName = tenant.CompanyName ?? tenant.Name;
        var items = batch.Items.OrderBy(i => i.Invoice.Number).ToList();

        var doc = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Column(column =>
                {
                    column.Spacing(10);
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Element(c => CompanyInfoColumn.Render(c, tenant, includeTaxIds: true));
                        row.ConstantItem(220).Column(col =>
                        {
                            col.Item().AlignRight().Text("SCHEDULE OF ACCOUNTS").FontSize(16).Bold();
                            col.Item().AlignRight().Text($"#{batch.Number}").FontSize(12).Bold();
                            col.Item().AlignRight().Text(batch.SubmittedAt.ToString("MMM dd, yyyy"));
                        });
                    });
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    column.Item().Text(text =>
                    {
                        text.Span("Factor: ").Bold();
                        text.Span(batch.FactoringCompany.Name);
                    });
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Spacing(15);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.ConstantColumn(70);
                            c.ConstantColumn(75);
                            c.ConstantColumn(60);
                            c.RelativeColumn();
                            c.ConstantColumn(100);
                        });

                        table.Header(h =>
                        {
                            h.Cell().Element(PdfStyles.HeaderCell).Text("Invoice #");
                            h.Cell().Element(PdfStyles.HeaderCell).Text("Date");
                            h.Cell().Element(PdfStyles.HeaderCell).Text("Load #");
                            h.Cell().Element(PdfStyles.HeaderCell).Text("Debtor");
                            h.Cell().Element(PdfStyles.HeaderCell).AlignRight().Text("Amount");
                        });

                        foreach (var item in items)
                        {
                            var invoice = item.Invoice;
                            table.Cell().Element(PdfStyles.Cell).Text(invoice.Number.ToString());
                            table.Cell().Element(PdfStyles.Cell).Text(invoice.CreatedAt.ToString("MMM dd, yyyy"));
                            table.Cell().Element(PdfStyles.Cell).Text(invoice.Load.Number.ToString());
                            table.Cell().Element(PdfStyles.Cell).Text(invoice.Customer.Name);
                            table.Cell().Element(PdfStyles.Cell).AlignRight()
                                .Text(PdfFormatting.Money(item.Amount.Amount, currency));
                        }
                    });

                    col.Item().AlignRight().Text(text =>
                    {
                        text.Span($"{items.Count} invoice(s), total ").Bold();
                        text.Span(PdfFormatting.Money(batch.Total.Amount, currency)).Bold().FontSize(12);
                    });

                    col.Item().Text(
                            $"{companyName} sells and assigns the accounts listed above to {batch.FactoringCompany.Name} " +
                            "under the factoring agreement between them, and warrants that the services billed were " +
                            "performed and the amounts are due without offset. The invoices and delivery paperwork " +
                            "(proof of delivery, bill of lading, rate confirmation) are attached.")
                        .FontSize(9);

                    if (!string.IsNullOrEmpty(batch.Notes))
                    {
                        col.Item().Text($"Notes: {batch.Notes}").FontSize(9);
                    }
                });

                page.Footer().Element(c => PdfFooter.Render(c, tenant, $"Schedule of accounts #{batch.Number}"));
            });
        });

        return doc.GeneratePdf();
    }
}
