using Logistics.Shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Documents.Pdf.Ifta;

internal static class IftaJurisdictionTableSection
{
    public static void Render(IContainer container, IftaReportDto report)
    {
        container.Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(70);  // Jurisdiction
                columns.RelativeColumn();    // Miles
                columns.RelativeColumn();    // Taxable gal
                columns.RelativeColumn();    // Purchased gal
                columns.RelativeColumn();    // Net taxable gal
                columns.RelativeColumn();    // Rate
                columns.RelativeColumn();    // Tax due
            });

            table.Header(header =>
            {
                HeaderCell(header.Cell(), "Jurisdiction");
                HeaderCell(header.Cell(), "Miles");
                HeaderCell(header.Cell(), "Taxable Gal");
                HeaderCell(header.Cell(), "Purchased Gal");
                HeaderCell(header.Cell(), "Net Taxable Gal");
                HeaderCell(header.Cell(), "Rate $/Gal");
                HeaderCell(header.Cell(), "Tax Due $");
            });

            foreach (var row in report.Jurisdictions)
            {
                BodyCell(table.Cell(), $"{row.CountryCode}-{row.Region}", bold: true);
                BodyCell(table.Cell(), row.Miles.ToString("N1"));
                BodyCell(table.Cell(), row.TaxableGallons.ToString("N1"));
                BodyCell(table.Cell(), row.PurchasedGallons.ToString("N1"));
                BodyCell(table.Cell(), row.NetTaxableGallons.ToString("N1"));
                BodyCell(table.Cell(), FormatRate(row));
                BodyCell(table.Cell(), row.TaxDue?.ToString("N2") ?? "—");
            }
        });
    }

    private static string FormatRate(IftaJurisdictionRowDto row)
    {
        if (row.RateMissing)
        {
            return "missing";
        }

        return row.SurchargeRatePerGallon is > 0
            ? $"{row.RatePerGallon:N4} +{row.SurchargeRatePerGallon:N4}"
            : $"{row.RatePerGallon:N4}";
    }

    private static void HeaderCell(IContainer cell, string text)
    {
        cell.BorderBottom(1).BorderColor(Colors.Grey.Darken1).PaddingVertical(4).PaddingHorizontal(3)
            .Text(text).FontSize(8).Bold();
    }

    private static void BodyCell(IContainer cell, string text, bool bold = false)
    {
        var t = cell.BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).PaddingVertical(3).PaddingHorizontal(3)
            .Text(text).FontSize(8);
        if (bold)
        {
            t.Bold();
        }
    }
}
