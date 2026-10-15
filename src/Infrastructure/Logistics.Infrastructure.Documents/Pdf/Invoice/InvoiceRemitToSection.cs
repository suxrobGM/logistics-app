using Logistics.Domain.Entities;
using Logistics.Infrastructure.Documents.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Documents.Pdf.Invoice;

/// <summary>
/// Notice of assignment for an invoice sold to a factoring company: the customer must pay the factor,
/// and paying the carrier does not settle the debt.
/// </summary>
internal static class InvoiceRemitToSection
{
    public const string Heading = "NOTICE OF ASSIGNMENT";

    public static void Render(IContainer container, FactoringCompany factor)
    {
        container.Border(1).BorderColor(Colors.Grey.Darken3).Padding(10).Column(col =>
        {
            col.Spacing(3);
            col.Item().Text(Heading).Bold().FontSize(11);
            col.Item().Text(
                    $"This account has been assigned to and must be paid only to {factor.Name}. " +
                    "Payment to anyone else does not discharge the debt.")
                .FontSize(9);

            col.Item().PaddingTop(5).Text($"Remit payment to: {factor.Name}").Bold();
            if (factor.RemitToAddress is { } address)
            {
                col.Item().Text(address.Line1);
                if (!string.IsNullOrEmpty(address.Line2))
                {
                    col.Item().Text(address.Line2);
                }
                col.Item().Text(PdfFormatting.CityLine(address));
            }

            if (!string.IsNullOrEmpty(factor.Phone))
            {
                col.Item().Text($"Phone: {factor.Phone}").FontSize(9);
            }
        });
    }
}
