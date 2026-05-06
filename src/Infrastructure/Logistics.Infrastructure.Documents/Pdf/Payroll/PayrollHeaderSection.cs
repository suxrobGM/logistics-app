using Logistics.Domain.Entities;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Payroll;

/// <summary>
/// Top section of the pay stub: company letterhead, "PAY STUB" title, employee block,
/// pay period / status, and the period totals (hours, distance) when present.
/// </summary>
internal static class PayrollHeaderSection
{
    public static void Render(IContainer container, PayrollInvoice payroll, Tenant tenant)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            column.Item().Row(row =>
            {
                row.RelativeItem().Element(c => CompanyInfoColumn.Render(c, tenant, includeTaxIds: false));
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().AlignRight().Text("PAY STUB")
                        .FontSize(28).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().AlignRight().Text($"#{payroll.Number}")
                        .FontSize(14).Bold();
                });
            });

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            column.Item().Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("EMPLOYEE").Bold().FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(5).Text(payroll.Employee?.GetFullName() ?? "N/A")
                        .FontSize(12).Bold();

                    if (!string.IsNullOrEmpty(payroll.Employee?.Email))
                    {
                        col.Item().Text(payroll.Employee.Email)
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                    if (payroll.Employee?.Role is not null)
                    {
                        col.Item().PaddingTop(3).Text(payroll.Employee.Role.Name)
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                    }
                });

                row.ConstantItem(200).Column(col =>
                {
                    KeyValueRow(col, "Pay Period:",
                        $"{payroll.PeriodStart:MMM dd} - {payroll.PeriodEnd:MMM dd, yyyy}");
                    KeyValueRow(col, "Issue Date:", payroll.CreatedAt.ToString("MMM dd, yyyy"));
                    KeyValueRow(col, "Status:", PdfFormatting.Display(payroll.Status));
                    if (payroll.Employee?.SalaryType is { } salary)
                    {
                        KeyValueRow(col, "Pay Type:", PdfFormatting.Display(salary));
                    }
                });
            });

            if (payroll.TotalHoursWorked > 0 || payroll.TotalDistanceDriven > 0)
            {
                column.Item().PaddingTop(10).Column(col =>
                {
                    if (payroll.TotalHoursWorked > 0)
                    {
                        col.Item().Text($"Total Hours Worked: {payroll.TotalHoursWorked:F2}")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                    }
                    if (payroll.TotalDistanceDriven > 0)
                    {
                        col.Item().Text($"Total Distance Driven: {payroll.TotalDistanceDriven:F1} km")
                            .FontSize(10).FontColor(Colors.Grey.Darken2);
                    }
                });
            }
        });
    }

    private static void KeyValueRow(ColumnDescriptor col, string label, string value) =>
        col.Item().Row(r =>
        {
            r.RelativeItem().Text(label).Bold();
            r.ConstantItem(120).AlignRight().Text(value);
        });
}
