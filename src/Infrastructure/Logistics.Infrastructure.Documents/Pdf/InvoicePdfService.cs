using System.Globalization;
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
    private const string ReverseChargeNotice =
        "Reverse charge — VAT to be accounted for by the recipient " +
        "(Article 196, Council Directive 2006/112/EC).";

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
                page.Content().Element(c => ComposeContent(c, invoice, tenant));
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

                    col.Item().Text(FormatCityLine(tenant.CompanyAddress));

                    if (!string.IsNullOrEmpty(tenant.PhoneNumber))
                    {
                        col.Item().Text($"Phone: {tenant.PhoneNumber}");
                    }

                    col.Item().Text($"Email: {tenant.BillingEmail}");

                    if (!string.IsNullOrEmpty(tenant.DotNumber))
                    {
                        col.Item().Text($"DOT#: {tenant.DotNumber}");
                    }

                    if (!string.IsNullOrEmpty(tenant.McNumber))
                    {
                        col.Item().Text($"MC#: {tenant.McNumber}");
                    }

                    if (!string.IsNullOrEmpty(tenant.VatNumber))
                    {
                        col.Item().Text($"VAT: {tenant.VatNumber}");
                    }

                    if (!string.IsNullOrEmpty(tenant.EoriNumber))
                    {
                        col.Item().Text($"EORI: {tenant.EoriNumber}");
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

                    if (!string.IsNullOrEmpty(invoice.Customer?.TaxId))
                    {
                        col.Item().Text($"Tax ID: {invoice.Customer.TaxId}").FontSize(9);
                    }
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

    private static void ComposeContent(IContainer container, LoadInvoice invoice, Tenant tenant)
    {
        var currency = invoice.Total.Currency;
        var taxLabel = GetTaxLabel(tenant.Settings?.Region);
        var showTaxColumns = ShouldShowTaxColumns(invoice);

        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(15);

            // Line items table
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(showTaxColumns ? 4 : 5); // Description
                    columns.ConstantColumn(50); // Qty
                    columns.ConstantColumn(80); // Unit
                    columns.ConstantColumn(80); // Net
                    if (showTaxColumns)
                    {
                        columns.ConstantColumn(45); // Rate
                        columns.ConstantColumn(70); // Tax
                        columns.ConstantColumn(80); // Gross
                    }
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Element(HeaderCellStyle).Text("Description");
                    header.Cell().Element(HeaderCellStyle).AlignCenter().Text("Qty");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Unit");
                    header.Cell().Element(HeaderCellStyle).AlignRight().Text("Net");
                    if (showTaxColumns)
                    {
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Rate");
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text(taxLabel);
                        header.Cell().Element(HeaderCellStyle).AlignRight().Text("Gross");
                    }
                });

                // Line items
                var lineItems = invoice.LineItems.OrderBy(li => li.Order).ToList();
                foreach (var item in lineItems)
                {
                    table.Cell().Element(CellStyle).Text(item.Description);
                    table.Cell().Element(CellStyle).AlignCenter().Text(item.Quantity.ToString());
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(item.Amount.Amount, currency));
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(item.Total, currency));
                    if (showTaxColumns)
                    {
                        table.Cell().Element(CellStyle).AlignRight().Text(FormatRate(item.TaxRatePercent));
                        table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(item.TaxAmount, currency));
                        table.Cell().Element(CellStyle).AlignRight()
                            .Text(FormatMoney(item.Total + item.TaxAmount, currency));
                    }
                }

                // If no line items, show delivery cost as default
                if (lineItems.Count == 0 && invoice.Load is not null)
                {
                    var fallback = invoice.Load.DeliveryCost.Amount;
                    table.Cell().Element(CellStyle).Text("Delivery Service");
                    table.Cell().Element(CellStyle).AlignCenter().Text("1");
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(fallback, currency));
                    table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(fallback, currency));
                    if (showTaxColumns)
                    {
                        table.Cell().Element(CellStyle).AlignRight().Text("—");
                        table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(0m, currency));
                        table.Cell().Element(CellStyle).AlignRight().Text(FormatMoney(fallback, currency));
                    }
                }
            });

            // Reverse-charge notice (legally required for EU cross-border B2B)
            if (invoice.TaxBehavior == TaxBehavior.ReverseCharge)
            {
                column.Item().PaddingTop(10)
                    .Background(Colors.Yellow.Lighten4)
                    .Border(1).BorderColor(Colors.Yellow.Darken2)
                    .Padding(10)
                    .Text(ReverseChargeNotice)
                    .Bold()
                    .FontSize(10);
            }

            // Tax breakdown table (when there is more than one tax line, e.g. layered US sales tax)
            var breakdown = invoice.GetTaxBreakdown();
            if (showTaxColumns && breakdown.Count > 1)
            {
                column.Item().PaddingTop(10).Column(b =>
                {
                    b.Item().Text($"{taxLabel} breakdown")
                        .FontSize(11).Bold().FontColor(Colors.Grey.Darken2);
                    b.Item().PaddingTop(5).Table(t =>
                    {
                        t.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);  // Jurisdiction
                            c.ConstantColumn(60); // Rate
                            c.ConstantColumn(90); // Base
                            c.ConstantColumn(90); // Amount
                        });
                        t.Header(h =>
                        {
                            h.Cell().Element(SmallHeaderCellStyle).Text("Jurisdiction");
                            h.Cell().Element(SmallHeaderCellStyle).AlignRight().Text("Rate");
                            h.Cell().Element(SmallHeaderCellStyle).AlignRight().Text("Base");
                            h.Cell().Element(SmallHeaderCellStyle).AlignRight().Text("Amount");
                        });
                        foreach (var line in breakdown)
                        {
                            t.Cell().Element(SmallCellStyle).Text(line.Description ?? line.Jurisdiction.ToString());
                            t.Cell().Element(SmallCellStyle).AlignRight().Text(FormatRate(line.RatePercent));
                            t.Cell().Element(SmallCellStyle).AlignRight().Text(FormatMoney(line.BaseAmount, currency));
                            t.Cell().Element(SmallCellStyle).AlignRight().Text(FormatMoney(line.TaxAmount, currency));
                        }
                    });
                });
            }

            // Totals section
            column.Item().AlignRight().Width(220).Column(totals =>
            {
                totals.Item().BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(10);

                totals.Item().Row(row =>
                {
                    row.RelativeItem().Text("Subtotal:").Bold();
                    row.ConstantItem(100).AlignRight().Text(FormatMoney(invoice.Subtotal.Amount, currency));
                });

                if (showTaxColumns)
                {
                    totals.Item().PaddingTop(3).Row(row =>
                    {
                        row.RelativeItem().Text($"{taxLabel}:").Bold();
                        row.ConstantItem(100).AlignRight()
                            .Text(invoice.TaxBehavior == TaxBehavior.ReverseCharge
                                ? "0.00 (reverse charge)"
                                : FormatMoney(invoice.TaxTotal.Amount, currency));
                    });
                }

                totals.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text("Total:").FontSize(14).Bold();
                    row.ConstantItem(100).AlignRight().Text(FormatMoney(invoice.Total.Amount, currency))
                        .FontSize(14)
                        .Bold()
                        .FontColor(Colors.Blue.Darken3);
                });

                // Payments
                var payments = invoice.Payments.Where(p => p.Status == PaymentStatus.Paid).ToList();
                if (payments.Count > 0)
                {
                    var totalPaid = payments.Sum(p => p.Amount.Amount);
                    totals.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text("Paid:").FontColor(Colors.Green.Darken2);
                        row.ConstantItem(100).AlignRight().Text($"-{FormatMoney(totalPaid, currency)}")
                            .FontColor(Colors.Green.Darken2);
                    });

                    var balance = invoice.Total.Amount - totalPaid;
                    totals.Item().Row(row =>
                    {
                        row.RelativeItem().Text("Balance Due:").FontSize(12).Bold();
                        row.ConstantItem(100).AlignRight().Text(FormatMoney(balance, currency))
                            .FontSize(12)
                            .Bold()
                            .FontColor(balance > 0 ? Colors.Red.Darken2 : Colors.Green.Darken2);
                    });
                }
            });

            // Payment history
            var paidPayments = invoice.Payments.Where(p => p.Status == PaymentStatus.Paid).ToList();
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
                                .Text(FormatMoney(payment.Amount.Amount, currency));
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

    private static IContainer HeaderCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Blue.Darken3)
            .Padding(8)
            .DefaultTextStyle(x => x.FontColor(Colors.White).Bold().FontSize(10));
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .Padding(8)
            .DefaultTextStyle(x => x.FontSize(10));
    }

    private static IContainer SmallHeaderCellStyle(IContainer container)
    {
        return container
            .Background(Colors.Grey.Lighten3)
            .Padding(5)
            .DefaultTextStyle(x => x.Bold().FontSize(9));
    }

    private static IContainer SmallCellStyle(IContainer container)
    {
        return container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten3)
            .Padding(5)
            .DefaultTextStyle(x => x.FontSize(9));
    }

    private static string GetTaxLabel(Region? region) => region switch
    {
        Region.Eu => "VAT",
        Region.Us => "Sales tax",
        _ => "Tax"
    };

    private static bool ShouldShowTaxColumns(LoadInvoice invoice) =>
        invoice.TaxBehavior != TaxBehavior.Exclusive
        || invoice.TaxTotal.Amount > 0m
        || invoice.LineItems.Any(li => li.TaxAmount > 0m || li.TaxRatePercent > 0m);

    private static string FormatMoney(decimal amount, string currency)
    {
        try
        {
            var info = NumberFormatInfo.InvariantInfo;
            return string.Create(CultureInfo.InvariantCulture, $"{amount.ToString("N2", info)} {currency}");
        }
        catch
        {
            return amount.ToString("N2", CultureInfo.InvariantCulture);
        }
    }

    private static string FormatRate(decimal percent) =>
        percent <= 0m ? "—" : percent.ToString("0.##", CultureInfo.InvariantCulture) + "%";

    private static string FormatCityLine(Logistics.Domain.Primitives.ValueObjects.Address a)
    {
        var state = string.IsNullOrEmpty(a.State) ? "" : $", {a.State}";
        return $"{a.City}{state} {a.ZipCode}, {a.Country}";
    }

    private static string GetStatusText(InvoiceStatus status)
    {
        return status switch
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
    }

    private static string GetPaymentMethodText(Payment payment)
    {
        // If we have a method record, use its type, otherwise default
        return "Payment";
    }
}
