using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Shared;

/// <summary>
/// Renders the "Payment History" panel — a table of paid <see cref="Payment"/>s with date,
/// reference, and amount. Returns silently when there are no paid payments.
/// </summary>
internal static class PaymentHistoryTable
{
    public static void Render(
        IContainer container,
        IEnumerable<Payment> payments,
        string currency,
        string title = "Payment History")
    {
        var paid = payments.Where(p => p.Status == PaymentStatus.Paid).OrderBy(p => p.RecordedAt).ToList();
        if (paid.Count == 0) return;

        container.Column(col =>
        {
            col.Item().Text(title).FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(5).Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.RelativeColumn(2);
                    c.ConstantColumn(120);
                });

                table.Header(h =>
                {
                    h.Cell().Element(PdfStyles.SmallHeaderCell).Text("Date");
                    h.Cell().Element(PdfStyles.SmallHeaderCell).Text("Reference");
                    h.Cell().Element(PdfStyles.SmallHeaderCell).Text("Method");
                    h.Cell().Element(PdfStyles.SmallHeaderCell).AlignRight().Text("Amount");
                });

                foreach (var p in paid)
                {
                    table.Cell().Element(PdfStyles.SmallCell).Text(p.RecordedAt?.ToString("MMM dd, yyyy") ?? "-");
                    table.Cell().Element(PdfStyles.SmallCell).Text(p.ReferenceNumber ?? "-");
                    table.Cell().Element(PdfStyles.SmallCell).Text("Payment");
                    table.Cell().Element(PdfStyles.SmallCell).AlignRight()
                        .Text(PdfFormatting.Money(p.Amount.Amount, currency));
                }
            });
        });
    }
}
