using Logistics.Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Shared;

internal static class PdfFooter
{
    public static void Render(IContainer container, Tenant tenant, string headlineMessage)
    {
        container.Column(column =>
        {
            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10);

            column.Item().AlignCenter().Text(text =>
            {
                text.Span(headlineMessage)
                    .FontSize(10)
                    .FontColor(Colors.Grey.Darken1);
            });

            column.Item().PaddingTop(5).AlignCenter().Text(text =>
            {
                text.Span(tenant.CompanyName ?? tenant.Name).FontSize(9).FontColor(Colors.Grey.Medium);
                text.Span(" | ").FontSize(9).FontColor(Colors.Grey.Medium);
                text.Span(tenant.BillingEmail).FontSize(9).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
