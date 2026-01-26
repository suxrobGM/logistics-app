using Logistics.Application.Services.Pdf;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Services.Pdf;

public class PayrollPayStubService : IPayrollPayStubService
{
    public byte[] GeneratePayStubPdf(PayrollInvoice payroll, Tenant tenant)
    {
        Settings.License = LicenseType.Community;

        var document = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, payroll, tenant));
                page.Content().Element(c => ComposeContent(c, payroll));
                page.Footer().Element(c => ComposeFooter(c, tenant));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, PayrollInvoice payroll, Tenant tenant)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            // Company info and Pay Stub title row
            column.Item().Row(row =>
            {
                // Company info (left side)
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text(tenant.CompanyName ?? tenant.Name)
                        .FontSize(18)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    col.Item().Text(tenant.CompanyAddress.Line1);
                    if (!string.IsNullOrEmpty(tenant.CompanyAddress.Line2))
                    {
                        col.Item().Text(tenant.CompanyAddress.Line2);
                    }

                    col.Item().Text(
                        $"{tenant.CompanyAddress.City}, {tenant.CompanyAddress.State} {tenant.CompanyAddress.ZipCode}");

                    if (!string.IsNullOrEmpty(tenant.PhoneNumber))
                    {
                        col.Item().Text($"Phone: {tenant.PhoneNumber}");
                    }
                });

                // Pay Stub title (right side)
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().AlignRight().Text("PAY STUB")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    col.Item().AlignRight().Text($"#{payroll.Number}")
                        .FontSize(14)
                        .Bold();
                });
            });

            // Divider
            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Employee info and Pay Period row
            column.Item().Row(row =>
            {
                // Employee info (left side)
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("EMPLOYEE").Bold().FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(5).Text(payroll.Employee?.GetFullName() ?? "N/A")
                        .FontSize(12)
                        .Bold();

                    if (!string.IsNullOrEmpty(payroll.Employee?.Email))
                    {
                        col.Item().Text(payroll.Employee.Email)
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    }

                    if (payroll.Employee?.Role != null)
                    {
                        col.Item().PaddingTop(3).Text(payroll.Employee.Role.Name)
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });

                // Pay period details (right side)
                row.ConstantItem(200).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Pay Period:").Bold();
                        r.ConstantItem(120).AlignRight().Text(
                            $"{payroll.PeriodStart:MMM dd} - {payroll.PeriodEnd:MMM dd, yyyy}");
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Issue Date:").Bold();
                        r.ConstantItem(120).AlignRight().Text(payroll.CreatedAt.ToString("MMM dd, yyyy"));
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Status:").Bold();
                        r.ConstantItem(120).AlignRight().Text(GetStatusText(payroll.Status));
                    });

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Pay Type:").Bold();
                        r.ConstantItem(120).AlignRight().Text(GetSalaryTypeText(payroll.Employee?.SalaryType));
                    });
                });
            });

            // Additional info for specific salary types
            if (payroll.TotalHoursWorked > 0 || payroll.TotalDistanceDriven > 0)
            {
                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        if (payroll.TotalHoursWorked > 0)
                        {
                            col.Item().Text($"Total Hours Worked: {payroll.TotalHoursWorked:F2}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                        }

                        if (payroll.TotalDistanceDriven > 0)
                        {
                            col.Item().Text($"Total Distance Driven: {payroll.TotalDistanceDriven:F1} km")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken2);
                        }
                    });
                });
            }
        });
    }

    private static void ComposeContent(IContainer container, PayrollInvoice payroll)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Separate earnings and deductions
            var lineItems = payroll.LineItems?.OrderBy(li => li.Order).ToList() ?? [];
            var earnings = lineItems.Where(li => li.Type != InvoiceLineItemType.Deduction).ToList();
            var deductions = lineItems.Where(li => li.Type == InvoiceLineItemType.Deduction).ToList();

            // Earnings table
            column.Item().Text("EARNINGS").FontSize(12).Bold().FontColor(Colors.Blue.Darken3);
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Description
                    columns.ConstantColumn(80); // Type
                    columns.ConstantColumn(60); // Qty/Hours
                    columns.ConstantColumn(90); // Rate
                    columns.ConstantColumn(90); // Amount
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Description");
                    header.Cell().Element(HeaderCellStyle).Text("Type");
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Qty");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Rate");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");
                });

                // Earnings items
                if (earnings.Count > 0)
                {
                    foreach (var item in earnings)
                    {
                        table.Cell().Element(CellStyle).Text(item.Description);
                        table.Cell().Element(CellStyle).Text(GetLineItemTypeText(item.Type));
                        table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                        table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(item.Amount.Amount));
                        table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(item.Total));
                    }
                }
                else
                {
                    // Show base salary if no line items
                    table.Cell().Element(CellStyle).Text("Base Pay");
                    table.Cell().Element(CellStyle).Text(GetSalaryTypeText(payroll.Employee?.SalaryType));
                    table.Cell().Element(CellStyle).AlignCenter().Text("1");
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(payroll.Total.Amount));
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(payroll.Total.Amount));
                }
            });

            // Gross earnings
            var grossEarnings = earnings.Count > 0 ? earnings.Sum(e => e.Total) : payroll.Total.Amount;
            column.Item().AlignRight().Width(200).Row(row =>
            {
                row.RelativeItem().Text("Gross Earnings:").Bold();
                row.ConstantItem(80).AlignRight().Text(FormatCurrency(grossEarnings))
                    .Bold()
                    .FontColor(Colors.Green.Darken2);
            });

            // Deductions table (if any)
            if (deductions.Count > 0)
            {
                column.Item().PaddingTop(10).Text("DEDUCTIONS").FontSize(12).Bold().FontColor(Colors.Red.Darken2);
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(4); // Description
                        columns.ConstantColumn(90); // Amount
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Element(DeductionHeaderCellStyle).Text("Description");
                        header.Cell().Element(DeductionHeaderCellStyle).AlignRight().Text("Amount");
                    });

                    foreach (var item in deductions)
                    {
                        table.Cell().Element(CellStyle).Text(item.Description);
                        table.Cell().Element(CellStyle).AlignRight().Text($"-{FormatCurrency(item.Total)}")
                            .FontColor(Colors.Red.Darken2);
                    }
                });

                var totalDeductions = deductions.Sum(d => d.Total);
                column.Item().AlignRight().Width(200).Row(row =>
                {
                    row.RelativeItem().Text("Total Deductions:").Bold();
                    row.ConstantItem(80).AlignRight().Text($"-{FormatCurrency(totalDeductions)}")
                        .Bold()
                        .FontColor(Colors.Red.Darken2);
                });
            }

            // Net Pay section
            var totalDeductionsAmount = deductions.Sum(d => d.Total);
            var netPay = grossEarnings - totalDeductionsAmount;

            column.Item().PaddingTop(15).Element(c =>
            {
                c.Background(Colors.Blue.Lighten5)
                    .Padding(15)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("NET PAY").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                        row.ConstantItem(120).AlignRight().Text(FormatCurrency(netPay))
                            .FontSize(20)
                            .Bold()
                            .FontColor(Colors.Blue.Darken3);
                    });
            });

            // Payment history
            var paidPayments = payroll.Payments?.Where(p => p.Status == PaymentStatus.Paid).ToList() ?? [];
            if (paidPayments.Count > 0)
            {
                column.Item().PaddingTop(20).Column(paymentCol =>
                {
                    paymentCol.Item().Text("PAYMENT HISTORY").FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
                    paymentCol.Item().PaddingTop(5).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(2); // Date
                            columns.RelativeColumn(2); // Reference
                            columns.RelativeColumn(2); // Method
                            columns.ConstantColumn(90); // Amount
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(SmallHeaderCellStyle).Text("Date");
                            header.Cell().Element(SmallHeaderCellStyle).Text("Reference");
                            header.Cell().Element(SmallHeaderCellStyle).Text("Method");
                            header.Cell().Element(SmallHeaderCellStyle).AlignRight().Text("Amount");
                        });

                        foreach (var payment in paidPayments.OrderBy(p => p.RecordedAt))
                        {
                            table.Cell().Element(SmallCellStyle)
                                .Text(payment.RecordedAt?.ToString("MMM dd, yyyy") ?? "-");
                            table.Cell().Element(SmallCellStyle).Text(payment.ReferenceNumber ?? "-");
                            table.Cell().Element(SmallCellStyle).Text("Payment");
                            table.Cell().Element(SmallCellStyle).AlignRight()
                                .Text(FormatCurrency(payment.Amount.Amount))
                                .FontColor(Colors.Green.Darken2);
                        }
                    });
                });
            }

            // Approval info
            if (payroll.ApprovedAt.HasValue && payroll.ApprovedBy != null)
            {
                column.Item().PaddingTop(15).Column(approvalCol =>
                {
                    approvalCol.Item().Text("APPROVAL").FontSize(10).Bold().FontColor(Colors.Grey.Darken2);
                    approvalCol.Item().PaddingTop(5).Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text($"Approved by: {payroll.ApprovedBy.FirstName} {payroll.ApprovedBy.LastName}")
                                .FontSize(9);
                            col.Item().Text($"Date: {payroll.ApprovedAt.Value:MMM dd, yyyy 'at' h:mm tt}")
                                .FontSize(9);
                            if (!string.IsNullOrEmpty(payroll.ApprovalNotes))
                            {
                                col.Item().Text($"Notes: {payroll.ApprovalNotes}")
                                    .FontSize(9)
                                    .FontColor(Colors.Grey.Darken1);
                            }
                        });
                    });
                });
            }
        });
    }

    private static void ComposeFooter(IContainer container, Tenant tenant)
    {
        container.Column(column =>
        {
            column.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10);

            column.Item().AlignCenter().Text(text =>
            {
                text.Span("This is a pay stub for your records.")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });

            column.Item().PaddingTop(5).AlignCenter().Text(text =>
            {
                text.Span(tenant.CompanyName ?? tenant.Name)
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
                text.Span(" | ")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
                text.Span(tenant.BillingEmail)
                    .FontSize(9)
                    .FontColor(Colors.Grey.Medium);
            });
        });
    }

    private static IContainer HeaderCellStyle(IContainer container) =>
        container
            .Background(Colors.Green.Darken2)
            .Padding(8)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));

    private static IContainer DeductionHeaderCellStyle(IContainer container) =>
        container
            .Background(Colors.Red.Darken2)
            .Padding(8)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));

    private static IContainer CellStyle(IContainer container) =>
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(8)
            .DefaultTextStyle(x => x.FontSize(10));

    private static IContainer SmallHeaderCellStyle(IContainer container) =>
        container
            .Background(Colors.Grey.Lighten3)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(9));

    private static IContainer SmallCellStyle(IContainer container) =>
        container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));

    private static string FormatCurrency(decimal amount) => $"${amount:N2}";

    private static string GetStatusText(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Draft => "Draft",
        InvoiceStatus.PendingApproval => "Pending Approval",
        InvoiceStatus.Approved => "Approved",
        InvoiceStatus.Rejected => "Rejected",
        InvoiceStatus.Issued => "Issued",
        InvoiceStatus.Sent => "Sent",
        InvoiceStatus.Paid => "Paid",
        InvoiceStatus.PartiallyPaid => "Partially Paid",
        InvoiceStatus.Overdue => "Overdue",
        InvoiceStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    private static string GetSalaryTypeText(SalaryType? salaryType) => salaryType switch
    {
        SalaryType.Monthly => "Monthly",
        SalaryType.Weekly => "Weekly",
        SalaryType.Hourly => "Hourly",
        SalaryType.ShareOfGross => "Share of Gross",
        SalaryType.RatePerDistance => "Per Distance",
        SalaryType.None => "N/A",
        null => "N/A",
        _ => salaryType.ToString() ?? "N/A"
    };

    private static string GetLineItemTypeText(InvoiceLineItemType type) => type switch
    {
        InvoiceLineItemType.BasePay => "Base Pay",
        InvoiceLineItemType.Bonus => "Bonus",
        InvoiceLineItemType.Reimbursement => "Reimbursement",
        InvoiceLineItemType.Adjustment => "Adjustment",
        InvoiceLineItemType.Deduction => "Deduction",
        InvoiceLineItemType.Other => "Other",
        _ => type.ToString()
    };
}
