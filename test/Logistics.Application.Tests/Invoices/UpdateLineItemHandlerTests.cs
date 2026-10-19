using Logistics.Application.Modules.Financial.Invoices.Commands;
using Logistics.Application.Modules.Financial.Tax.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;
using static Logistics.Application.Tests.Factoring.FactoringTestData;

namespace Logistics.Application.Tests.Invoices;

public class UpdateLineItemHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Invoice, Guid> _invoiceRepo = Substitute.For<ITenantRepository<Invoice, Guid>>();
    private readonly IInvoiceTaxApplier _taxApplier = Substitute.For<IInvoiceTaxApplier>();
    private readonly UpdateLineItemHandler _sut;

    public UpdateLineItemHandlerTests()
    {
        _tenantUow.Repository<Invoice>().Returns(_invoiceRepo);
        _tenantUow.Repository<InvoiceLineItem>().Returns(Substitute.For<ITenantRepository<InvoiceLineItem, Guid>>());

        // A flat 10% tax on every line.
        _taxApplier.When(t => t.ApplyAsync(Arg.Any<Invoice>(), Arg.Any<CancellationToken>()))
            .Do(call =>
            {
                var invoice = call.Arg<Invoice>();
                foreach (var line in invoice.LineItems)
                {
                    line.TaxRatePercent = 10m;
                    line.TaxAmount = line.Total * 0.1m;
                }

                invoice.RecalculateTotals();
            });

        _sut = new UpdateLineItemHandler(_tenantUow, _taxApplier);
    }

    [Fact]
    public async Task Handle_AmountChanged_RecalculatesTaxAndTotals()
    {
        var (invoice, line) = InvoiceWithLine(1000m);

        var result = await _sut.Handle(Update(invoice, line, 2000m), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(200m, line.TaxAmount);
        Assert.Equal(2000m, invoice.Subtotal.Amount);
        Assert.Equal(200m, invoice.TaxTotal.Amount);
        Assert.Equal(2200m, invoice.Total.Amount);
    }

    [Fact]
    public async Task Handle_TotalLoweredToWhatWasPaid_InvoiceBecomesPaid()
    {
        var (invoice, line) = InvoiceWithLine(1000m);
        invoice.ApplyPayment(new Payment
        {
            Amount = new Money { Amount = 550m, Currency = "USD" },
            Status = PaymentStatus.Paid,
            BillingAddress = Factoring.FactoringTestData.Address
        });
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);

        var result = await _sut.Handle(Update(invoice, line, 500m), default);

        Assert.True(result.IsSuccess);
        Assert.Equal(550m, invoice.Total.Amount);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public async Task Handle_DraftWithoutPayments_StaysDraft()
    {
        var (invoice, line) = InvoiceWithLine(1000m, InvoiceStatus.Draft);

        await _sut.Handle(Update(invoice, line, 500m), default);

        Assert.Equal(InvoiceStatus.Draft, invoice.Status);
    }

    private (LoadInvoice Invoice, InvoiceLineItem Line) InvoiceWithLine(
        decimal amount, InvoiceStatus status = InvoiceStatus.Issued)
    {
        var invoice = Invoice(Customer(null), total: amount, status: status);
        var line = new InvoiceLineItem
        {
            InvoiceId = invoice.Id,
            Description = "Linehaul",
            Type = InvoiceLineItemType.BaseRate,
            Amount = new Money { Amount = amount, Currency = "USD" }
        };
        invoice.LineItems.Add(line);
        _invoiceRepo.GetByIdAsync(invoice.Id, Arg.Any<CancellationToken>()).Returns(invoice);
        return (invoice, line);
    }

    private static UpdateLineItemCommand Update(Invoice invoice, InvoiceLineItem line, decimal amount) => new()
    {
        InvoiceId = invoice.Id,
        LineItemId = line.Id,
        Amount = amount
    };
}
