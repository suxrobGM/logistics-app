using Logistics.Application.Abstractions.Documents;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Documents.Pdf.Shared;
using Logistics.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Documents.Pdf.Ifta;

public class IftaReportPdfService : IIftaReportPdfService
{
    public byte[] GenerateIftaReportPdf(IftaReportDto report, Tenant tenant)
    {
        var doc = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().Element(c => IftaHeaderSection.Render(c, report, tenant));

                page.Content().PaddingVertical(15).Column(col =>
                {
                    col.Spacing(12);
                    col.Item().Element(c => IftaSummarySection.Render(c, report));
                    col.Item().Element(c => IftaJurisdictionTableSection.Render(c, report));

                    if (report.HasMissingMileageOrFuel)
                    {
                        Warning(col.Item(), "This quarter has fuel purchases but no recorded miles, or miles but no fuel, so fleet MPG and tax due cannot be computed. Record the missing data before filing.");
                    }

                    if (report.HasMissingRates)
                    {
                        Warning(col.Item(), "Some jurisdictions have no published tax rate for this quarter - their tax due is omitted. Add the missing rates before filing.");
                    }
                });

                page.Footer().Element(c => PdfFooter.Render(c, tenant,
                    "Generated for IFTA quarterly filing - verify against your base jurisdiction's return before submitting."));
            });
        });

        return doc.GeneratePdf();
    }

    private static void Warning(IContainer container, string message) =>
        container
            .Background(Colors.Orange.Lighten4)
            .Padding(8)
            .Text(message)
            .FontSize(8).FontColor(Colors.Orange.Darken3);
}
