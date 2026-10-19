using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Documents.Pdf;
using UglyToad.PdfPig;
using Xunit;

namespace Logistics.Infrastructure.Documents.Tests.Pdf;

public class InvoicePdfServiceTests
{
    private static readonly Address Address = new()
    {
        Line1 = "500 Remit Ave",
        City = "Dallas",
        State = "TX",
        ZipCode = "75201",
        Country = "US"
    };

    private readonly InvoicePdfService _sut;

    public InvoicePdfServiceTests()
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        _sut = new InvoicePdfService();
    }

    private static readonly Tenant Tenant = new()
    {
        Name = "Carrier",
        DatabaseName = "test",
        BillingEmail = "billing@carrier.test",
        CompanyAddress = Address
    };

    private static LoadInvoice Invoice(FactoringCompany? factor)
    {
        var money = new Money { Amount = 1500m, Currency = "USD" };
        var customer = new Customer { Name = "Big Broker", FactoringCompanyId = factor?.Id, FactoringCompany = factor };
        var invoice = new LoadInvoice
        {
            Number = 1001,
            Status = InvoiceStatus.Paid,
            Subtotal = money,
            TaxTotal = Money.Zero("USD"),
            Total = money,
            LoadId = Guid.NewGuid(),
            Load = null!,
            CustomerId = customer.Id,
            Customer = customer
        };
        invoice.LineItems.Add(new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = "Linehaul",
            Type = InvoiceLineItemType.BaseRate,
            Amount = money
        });
        invoice.Payments.Add(new Payment
        {
            Amount = money,
            Status = PaymentStatus.Paid,
            BillingAddress = Address,
            ReferenceNumber = "SOA-7"
        });
        return invoice;
    }

    private static string Text(byte[] pdf)
    {
        using var document = PdfDocument.Open(pdf);
        var text = string.Concat(document.GetPages().Select(p => p.Text));
        return new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
    }

    [Fact]
    public void GenerateLoadInvoicePdf_FactoredCustomer_PrintsNoticeOfAssignmentWithRemitTo()
    {
        var factor = new FactoringCompany
        {
            Name = "Acme Capital",
            SubmissionEmail = "schedules@acme.test",
            RemitToAddress = Address
        };

        var text = Text(_sut.GenerateLoadInvoicePdf(Invoice(factor), Tenant));

        Assert.Contains("NOTICEOFASSIGNMENT", text);
        Assert.Contains("Remitpaymentto:AcmeCapital", text);
        Assert.Contains("500RemitAve", text);
        Assert.DoesNotContain("BalanceDue", text);
        Assert.DoesNotContain("SOA-7", text);
    }

    [Fact]
    public void GenerateLoadInvoicePdf_CustomerNotFactored_HasNoNoticeAndShowsPayments()
    {
        var text = Text(_sut.GenerateLoadInvoicePdf(Invoice(null), Tenant));

        Assert.DoesNotContain("NOTICEOFASSIGNMENT", text);
        Assert.Contains("BalanceDue", text);
    }
}
