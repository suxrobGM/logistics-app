using Logistics.Application.Abstractions.Documents;
using Logistics.Domain.Entities;
using Logistics.Infrastructure.Services.Pdf.Shared;
using Logistics.Shared.Models;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Services.Pdf.Ifta;

public class IftaReportPdfService : IIftaReportPdfService
{
    public byte[] GenerateIftaReportPdf(IftaReportDto report, Tenant tenant)
    {
        Settings.License = LicenseType.Community;

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

                    if (report.HasMissingRates)
                    {
                        col.Item()
                            .Background(Colors.Orange.Lighten4)
                            .Padding(8)
                            .Text("Some jurisdictions have no published tax rate for this quarter — their tax due is omitted. Add the missing rates before filing.")
                            .FontSize(8).FontColor(Colors.Orange.Darken3);
                    }
                });

                page.Footer().Element(c => PdfFooter.Render(c, tenant,
                    "Generated for IFTA quarterly filing — verify against your base jurisdiction's return before submitting."));
            });
        });

        return doc.GeneratePdf();
    }
}
