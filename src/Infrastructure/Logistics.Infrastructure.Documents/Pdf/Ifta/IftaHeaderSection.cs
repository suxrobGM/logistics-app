using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Documents.Pdf.Ifta;

internal static class IftaHeaderSection
{
    public static void Render(IContainer container, IftaReportDto report, Tenant tenant)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("IFTA Quarterly Fuel Tax Report").FontSize(16).Bold();
                col.Item().Text($"Q{report.Quarter} {report.Year}").FontSize(12).FontColor(Colors.Grey.Darken2);
                if (report.IsClosed && report.ClosedAt.HasValue)
                {
                    col.Item().Text($"Quarter closed {report.ClosedAt:yyyy-MM-dd}").FontSize(8).FontColor(Colors.Grey.Darken1);
                }
            });

            row.ConstantItem(220).Column(col =>
            {
                col.Item().AlignRight().Text(tenant.CompanyName ?? tenant.Name).FontSize(11).Bold();
                if (!string.IsNullOrEmpty(tenant.DotNumber))
                {
                    col.Item().AlignRight().Text($"DOT {tenant.DotNumber}").FontSize(8);
                }
                if (!string.IsNullOrEmpty(tenant.McNumber))
                {
                    col.Item().AlignRight().Text($"MC {tenant.McNumber}").FontSize(8);
                }
            });
        });
    }
}
