using Logistics.Application.Services.Pdf;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestDocument = QuestPDF.Fluent.Document;

namespace Logistics.Infrastructure.Services.Pdf;

public class InvoicePdfService : IInvoicePdfService
{
    public byte[] GenerateLoadInvoicePdf(LoadInvoice invoice, Tenant tenant)
    {
        Settings.License = LicenseType.Community;

        var document = QuestDocument.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c => ComposeHeader(c, invoice, tenant));
                page.Content().Element(c => ComposeContent(c, invoice));
                page.Footer().Element(c => ComposeFooter(c, tenant));
            });
        });

        return document.GeneratePdf();
    }

    private static void ComposeHeader(IContainer container, LoadInvoice invoice, Tenant tenant)
    {
        container.Column(column =>
        {
            column.Spacing(10);

            // Company info and Invoice title row
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

                    col.Item().Text($"Email: {tenant.BillingEmail}");

                    if (!string.IsNullOrEmpty(tenant.DotNumber))
                    {
                        col.Item().Text($"DOT#: {tenant.DotNumber}");
                    }
                });

                // Invoice title (right side)
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().AlignRight().Text("INVOICE")
                        .FontSize(28)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);

                    col.Item().AlignRight().Text($"#{invoice.Number}")
                        .FontSize(14)
                        .Bold();
                });
            });

            // Divider
            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

            // Invoice details and Bill To row
            column.Item().Row(row =>
            {
                // Bill To (left side)
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("BILL TO").Bold().FontColor(Colors.Grey.Darken2);
                    col.Item().PaddingTop(5).Text(invoice.Customer?.Name ?? "N/A")
                        .FontSize(12)
                        .Bold();
                });

                // Invoice details (right side)
                row.ConstantItem(180).Column(col =>
                {
                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Issue Date:").Bold();
                        r.ConstantItem(100).AlignRight().Text(invoice.CreatedAt.ToString("MMM dd, yyyy"));
                    });

                    if (invoice.DueDate.HasValue)
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Due Date:").Bold();
                            r.ConstantItem(100).AlignRight().Text(invoice.DueDate.Value.ToString("MMM dd, yyyy"));
                        });
                    }

                    col.Item().Row(r =>
                    {
                        r.RelativeItem().Text("Status:").Bold();
                        r.ConstantItem(100).AlignRight().Text(GetStatusText(invoice.Status));
                    });
                });
            });

            // Load reference
            column.Item().PaddingTop(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text($"Load #: {invoice.Load?.Number}")
                        .FontSize(11)
                        .Bold();

                    if (invoice.Load is not null)
                    {
                        var origin = invoice.Load.OriginAddress;
                        var dest = invoice.Load.DestinationAddress;
                        col.Item().Text($"Route: {origin?.City}, {origin?.State} → {dest?.City}, {dest?.State}")
                            .FontSize(9)
                            .FontColor(Colors.Grey.Darken1);
                    }
                });
            });
        });
    }

    private static void ComposeContent(IContainer container, LoadInvoice invoice)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Line items table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Description
                    columns.ConstantColumn(60); // Qty
                    columns.ConstantColumn(90); // Unit Price
                    columns.ConstantColumn(90); // Amount
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Description");
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Qty");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit Price");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Amount");
                });

                // Line items
                var lineItems = invoice.LineItems?.OrderBy(li => li.Order).ToList() ?? [];
                foreach (var item in lineItems)
                {
                    table.Cell().Element(CellStyle).Text(item.Description);
                    table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(item.Amount.Amount));
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(item.Total));
                }

                // If no line items, show delivery cost as default
                if (lineItems.Count == 0 && invoice.Load != null)
                {
                    table.Cell().Element(CellStyle).Text("Delivery Service");
                    table.Cell().Element(CellStyle).AlignCenter().Text("1");
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(invoice.Load.DeliveryCost.Amount));
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatCurrency(invoice.Load.DeliveryCost.Amount));
                }
            });

            // Totals section
            column.Item().AlignRight().Width(200).Column(totals =>
            {
                totals.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10);

                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").Bold();
                    row.ConstantItem(80).AlignRight().Text(FormatCurrency(invoice.Total.Amount));
                });

                totals.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Total:").FontSize(14).Bold();
                    row.ConstantItem(80).AlignRight().Text(FormatCurrency(invoice.Total.Amount))
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);
                });

                // Payments
                var payments = invoice.Payments?.Where(p => p.Status == PaymentStatus.Paid).ToList() ?? [];
                if (payments.Count > 0)
                {
                    var totalPaid = payments.Sum(p => p.Amount.Amount);
                    totals.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("Paid:").FontColor(Colors.Green.Darken2);
                        row.ConstantItem(80).AlignRight().Text($"-{FormatCurrency(totalPaid)}")
                            .FontColor(Colors.Green.Darken2);
                    });

                    var balance = invoice.Total.Amount - totalPaid;
                    totals.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Balance Due:").FontSize(12).Bold();
                        row.ConstantItem(80).AlignRight().Text(FormatCurrency(balance))
                            .FontSize(12)
                            .Bold()
                            .FontColor(balance > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                    });
                }
            });

            // Payment history
            var paidPayments = invoice.Payments?.Where(p => p.Status == PaymentStatus.Paid).ToList() ?? [];
            if (paidPayments.Count > 0)
            {
                column.Item().PaddingTop(20).Column(paymentCol =>
                {
                    paymentCol.Item().Text("Payment History").FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
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
                            table.Cell().Element(SmallCellStyle).Text(GetPaymentMethodText(payment));
                            table.Cell().Element(SmallCellStyle).AlignRight()
                                .Text(FormatCurrency(payment.Amount.Amount));
                        }
                    });
                });
            }

            // Notes
            if (!string.IsNullOrEmpty(invoice.Notes))
            {
                column.Item().PaddingTop(20).Column(notesCol =>
                {
                    notesCol.Item().Text("Notes").FontSize(12).Bold().FontColor(Colors.Grey.Darken2);
                    notesCol.Item().PaddingTop(5)
                        .Background(Colors.Grey.Lighten4)
                        .Padding(10)
                        .Text(invoice.Notes)
                        .FontSize(9);
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
                text.Span("Thank you for your business!")
                    .FontSize(10)
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
            .Background(Colors.Blue.Darken3)
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
        InvoiceStatus.Issued => "Issued",
        InvoiceStatus.Sent => "Sent",
        InvoiceStatus.Paid => "Paid",
        InvoiceStatus.PartiallyPaid => "Partially Paid",
        InvoiceStatus.Overdue => "Overdue",
        InvoiceStatus.Cancelled => "Cancelled",
        _ => status.ToString()
    };

    private static string GetPaymentMethodText(Payment payment)
    {
        // If we have a method record, use its type, otherwise default
        return "Payment";
    }
}
