using System.Linq.Expressions;
using Logistics.Application.Modules.Financial.Invoices.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;
using Logistics.Application.Tests.Factoring;
using static Logistics.Application.Tests.Factoring.FactoringTestData;

namespace Logistics.Application.Tests.Invoices;

public class GetInvoiceDashboardHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Invoice, Guid> _invoiceRepo = Substitute.For<ITenantRepository<Invoice, Guid>>();
    private readonly GetInvoiceDashboardHandler _sut;

    public GetInvoiceDashboardHandlerTests()
    {
        var fundedRepo = Substitute.For<ITenantRepository<FactoringBatchItem, Guid>>();
        fundedRepo.GetListAsync(Arg.Any<Expression<Func<FactoringBatchItem, bool>>>(), Arg.Any<CancellationToken>())
            .Returns([]);
        _tenantUow.Repository<Invoice>().Returns(_invoiceRepo);
        _tenantUow.Repository<FactoringBatchItem>().Returns(fundedRepo);
        _sut = new GetInvoiceDashboardHandler(_tenantUow);
    }

    private void GivenInvoices(params Invoice[] invoices) =>
        _invoiceRepo.GetListAsync(Arg.Any<Expression<Func<Invoice, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(invoices.ToList());

    private static LoadInvoice PastDue(InvoiceStatus status, decimal total = 1000m)
    {
        var invoice = Invoice(Customer(null), total: total, status: status);
        invoice.DueDate = DateTime.UtcNow.AddDays(-5);
        return invoice;
    }

    [Theory]
    [InlineData(InvoiceStatus.Issued)]
    [InlineData(InvoiceStatus.Sent)]
    [InlineData(InvoiceStatus.Overdue)]
    public async Task Handle_UnpaidPastDue_CountsAsOverdue(InvoiceStatus status)
    {
        GivenInvoices(PastDue(status));

        var result = await _sut.Handle(new GetInvoiceDashboardQuery(), CancellationToken.None);

        Assert.Equal(1, result.Value!.OverdueCount);
        Assert.Equal(1000m, result.Value.OverdueAmount);
        Assert.Equal(0, result.Value.PendingCount);
    }

    [Fact]
    public async Task Handle_SentNotDue_CountsAsPending()
    {
        var invoice = Invoice(Customer(null), status: InvoiceStatus.Sent);
        invoice.DueDate = DateTime.UtcNow.AddDays(10);
        GivenInvoices(invoice);

        var result = await _sut.Handle(new GetInvoiceDashboardQuery(), CancellationToken.None);

        Assert.Equal(1, result.Value!.PendingCount);
        Assert.Equal(0, result.Value.OverdueCount);
    }

    [Fact]
    public async Task Handle_PartiallyPaidPastDue_CountsOnlyTheUnpaidBalanceOnce()
    {
        var invoice = PastDue(InvoiceStatus.PartiallyPaid);
        invoice.Payments.Add(new Payment
        {
            Amount = new Money { Amount = 400m, Currency = "USD" },
            Status = PaymentStatus.Paid,
            BillingAddress = FactoringTestData.Address
        });
        GivenInvoices(invoice);

        var result = await _sut.Handle(new GetInvoiceDashboardQuery(), CancellationToken.None);

        Assert.Equal(600m, result.Value!.OverdueAmount);
        Assert.Equal(600m, result.Value.TotalOutstanding);
    }
}
