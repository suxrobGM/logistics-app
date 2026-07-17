using Logistics.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Documents.Pdf.Ifta;

internal static class IftaSummarySection
{
    public static void Render(IContainer container, IftaReportDto report)
    {
        container.Row(row =>
        {
            row.Spacing(10);
            Stat(row, "Total Miles", report.TotalMiles.ToString("N1"));
            Stat(row, "Total Gallons", report.TotalGallons.ToString("N1"));
            Stat(row, "Fleet Avg MPG", report.AverageMpg.ToString("N2"));
            Stat(row, "Net Tax Due (USD)", report.TotalTaxDue.ToString("N2"));
        });
    }

    private static void Stat(RowDescriptor row, string label, string value)
    {
        row.RelativeItem()
            .Background(Colors.Grey.Lighten4)
            .Padding(10)
            .Column(col =>
            {
                col.Item().Text(label).FontSize(7).FontColor(Colors.Grey.Darken2);
                col.Item().Text(value).FontSize(13).Bold();
            });
    }
}
