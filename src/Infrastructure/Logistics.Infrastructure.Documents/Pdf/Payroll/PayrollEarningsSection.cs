using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Services.Pdf.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Logistics.Infrastructure.Services.Pdf.Payroll;

/// <summary>
/// Body of the pay stub: earnings table → gross row → optional deductions table →
/// total deductions row → NET PAY callout → optional approval block.
/// </summary>
internal static class PayrollEarningsSection
{
    public static void Render(IContainer container, PayrollInvoice payroll)
    {
        var currency = payroll.Total.Currency;
        var lineItems = payroll.LineItems.OrderBy(li => li.Order).ToList();
        var earnings = lineItems.Where(li => li.Type != InvoiceLineItemType.Deduction).ToList();
        var deductions = lineItems.Where(li => li.Type == InvoiceLineItemType.Deduction).ToList();

        var grossEarnings = earnings.Count > 0 ? earnings.Sum(e => e.Total) : payroll.Total.Amount;
        var totalDeductions = deductions.Sum(d => d.Total);
        var netPay = grossEarnings - totalDeductions;

        container.Column(column =>
        {
            column.Spacing(15);

            column.Item().Element(c => RenderEarningsTable(c, payroll, earnings, currency));
            column.Item().Element(c => SummaryRow(c, "Gross Earnings:",
                PdfFormatting.Money(grossEarnings, currency), Colors.Green.Darken2));

            if (deductions.Count > 0)
            {
                column.Item().PaddingTop(10).Element(c => RenderDeductionsTable(c, deductions, currency));
                column.Item().Element(c => SummaryRow(c, "Total Deductions:",
                    $"-{PdfFormatting.Money(totalDeductions, currency)}", Colors.Red.Darken2));
            }

            column.Item().PaddingTop(15).Element(c => RenderNetPayCallout(c, netPay, currency));

            column.Item().PaddingTop(20).Element(c => PaymentHistoryTable.Render(
                c, payroll.Payments, currency, "PAYMENT HISTORY"));

            if (payroll.ApprovedAt.HasValue && payroll.ApprovedBy is not null)
            {
                column.Item().PaddingTop(15).Element(c => RenderApprovalBlock(c, payroll));
            }
        });
    }

    private static void RenderEarningsTable(
        IContainer container, PayrollInvoice payroll, List<InvoiceLineItem> earnings, string currency)
    {
        container.Column(col =>
        {
            col.Item().Text("EARNINGS").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.ConstantColumn(80);
                    c.ConstantColumn(60);
                    c.ConstantColumn(90);
                    c.ConstantColumn(90);
                });

                table.Header(h =>
                {
                    h.Cell().Element(PdfStyles.EarningsHeaderCell).Text("Description");
                    h.Cell().Element(PdfStyles.EarningsHeaderCell).Text("Type");
                    h.Cell().Element(PdfStyles.EarningsHeaderCell).AlignCenter().Text("Qty");
                    h.Cell().Element(PdfStyles.EarningsHeaderCell).AlignRight().Text("Rate");
                    h.Cell().Element(PdfStyles.EarningsHeaderCell).AlignRight().Text("Amount");
                });

                if (earnings.Count == 0)
                {
                    var salaryLabel = payroll.Employee?.SalaryType is { } st ? PdfFormatting.Display(st) : "—";
                    table.Cell().Element(PdfStyles.Cell).Text("Base Pay");
                    table.Cell().Element(PdfStyles.Cell).Text(salaryLabel);
                    table.Cell().Element(PdfStyles.Cell).AlignCenter().Text("1");
                    table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(payroll.Total.Amount, currency));
                    table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(payroll.Total.Amount, currency));
                    return;
                }

                foreach (var item in earnings)
                {
                    table.Cell().Element(PdfStyles.Cell).Text(item.Description);
                    table.Cell().Element(PdfStyles.Cell).Text(PdfFormatting.Display(item.Type));
                    table.Cell().Element(PdfStyles.Cell).AlignCenter().Text(item.Quantity.ToString());
                    table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(item.Amount.Amount, currency));
                    table.Cell().Element(PdfStyles.Cell).AlignRight().Text(PdfFormatting.Money(item.Total, currency));
                }
            });
        });
    }

    private static void RenderDeductionsTable(
        IContainer container, List<InvoiceLineItem> deductions, string currency)
    {
        container.Column(col =>
        {
            col.Item().Text("DEDUCTIONS").FontSize(12).Bold().FontColor(Colors.Red.Darken2);
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(c =>
                {
                    c.RelativeColumn(4);
                    c.ConstantColumn(90);
                });

                table.Header(h =>
                {
                    h.Cell().Element(PdfStyles.DeductionsHeaderCell).Text("Description");
                    h.Cell().Element(PdfStyles.DeductionsHeaderCell).AlignRight().Text("Amount");
                });

                foreach (var item in deductions)
                {
                    table.Cell().Element(PdfStyles.Cell).Text(item.Description);
                    table.Cell().Element(PdfStyles.Cell).AlignRight()
                        .Text($"-{PdfFormatting.Money(item.Total, currency)}")
                        .FontColor(Colors.Red.Darken2);
                }
            });
        });
    }

    private static void SummaryRow(IContainer container, string label, string value, string fontColor)
    {
        container.AlignRight().Width(220).Row(row =>
        {
            row.RelativeItem().Text(label).Bold();
            row.ConstantItem(100).AlignRight().Text(value).Bold().FontColor(fontColor);
        });
    }

    private static void RenderNetPayCallout(IContainer container, decimal netPay, string currency)
    {
        container.Background(Colors.Blue.Lighten5)
                 .Padding(15)
                 .Row(row =>
                 {
                     row.RelativeItem().Text("NET PAY").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                     row.ConstantItem(160).AlignRight()
                         .Text(PdfFormatting.Money(netPay, currency))
                         .FontSize(20).Bold().FontColor(Colors.Blue.Darken3);
                 });
    }

    private static void RenderApprovalBlock(IContainer container, PayrollInvoice payroll)
    {
        container.Column(col =>
        {
            col.Item().Text("APPROVAL").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
            col.Item().PaddingTop(5).Column(inner =>
            {
                inner.Item().Text(
                    $"Approved by: {payroll.ApprovedBy!.FirstName} {payroll.ApprovedBy.LastName}")
                    .FontSize(9);
                inner.Item().Text($"Date: {payroll.ApprovedAt!.Value:MMM dd, yyyy 'at' h:mm tt}")
                    .FontSize(9);
                if (!string.IsNullOrEmpty(payroll.ApprovalNotes))
                {
                    inner.Item().Text($"Notes: {payroll.ApprovalNotes}")
                        .FontSize(9).FontColor(Colors.Grey.Darken1);
                }
            });
        });
    }
}
