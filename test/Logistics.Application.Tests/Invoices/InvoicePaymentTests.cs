using System.Linq.Expressions;
using Logistics.Application.Modules.Financial.Payments.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;
using Logistics.Application.Tests.Factoring;
using static Logistics.Application.Tests.Factoring.FactoringTestData;

namespace Logistics.Application.Tests.Invoices;

public class InvoicePaymentTests
{
    private static readonly DateTime Now = new(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);

    #region RemovePayment

    [Fact]
    public void RemovePayment_SomeStillPaid_PartiallyPaid()
    {
        var invoice = Invoice(Customer(null));
        invoice.ApplyPayment(Payment(400m));
        var second = Payment(600m);
        invoice.ApplyPayment(second);

        invoice.RemovePayment(second, Now);

        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
        Assert.Single(invoice.Payments);
    }

    [Fact]
    public void RemovePayment_NothingPaidAndPastDue_Overdue()
    {
        var invoice = Invoice(Customer(null));
        invoice.DueDate = Now.AddDays(-1);
        var payment = Payment(1000m);
        invoice.ApplyPayment(payment);

        invoice.RemovePayment(payment, Now);

        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Theory]
    [InlineData(true, InvoiceStatus.Sent)]
    [InlineData(false, InvoiceStatus.Issued)]
    public void RemovePayment_NothingPaidNotDue_SentOrIssued(bool sent, InvoiceStatus expected)
    {
        var invoice = Invoice(Customer(null));
        invoice.DueDate = Now.AddDays(10);
        invoice.SentAt = sent ? Now.AddDays(-2) : null;
        var payment = Payment(1000m);
        invoice.ApplyPayment(payment);

        invoice.RemovePayment(payment, Now);

        Assert.Equal(expected, invoice.Status);
        Assert.Empty(invoice.Payments);
    }

    [Fact]
    public void RemovePayment_LastPaymentOnPayroll_BackToApproved()
    {
        var total = new Money { Amount = 1000m, Currency = "USD" };
        var payroll = new PayrollInvoice
        {
            EmployeeId = Guid.NewGuid(),
            Subtotal = total,
            TaxTotal = Money.Zero("USD"),
            Total = total,
            Status = InvoiceStatus.Approved,
            DueDate = Now.AddDays(-1)
        };
        var payment = Payment(1000m);
        payroll.ApplyPayment(payment);

        payroll.RemovePayment(payment, Now);

        Assert.Equal(InvoiceStatus.Approved, payroll.Status);
    }

    #endregion

    #region Update payment

    [Fact]
    public async Task UpdatePayment_AmountLowered_InvoiceNoLongerPaid()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var paymentRepo = Substitute.For<ITenantRepository<Payment, Guid>>();
        var invoiceRepo = Substitute.For<ITenantRepository<Invoice, Guid>>();
        tenantUow.Repository<Payment>().Returns(paymentRepo);
        tenantUow.Repository<Invoice>().Returns(invoiceRepo);

        var invoice = Invoice(Customer(null));
        var payment = Payment(1000m);
        invoice.ApplyPayment(payment);
        paymentRepo.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        invoiceRepo.GetAsync(Arg.Any<Expression<Func<Invoice, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(invoice);
        var sut = new UpdatePaymentHandler(tenantUow);

        var result = await sut.Handle(new UpdatePaymentCommand { Id = payment.Id, Amount = 400m }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(400m, payment.Amount.Amount);
        Assert.Equal(InvoiceStatus.PartiallyPaid, invoice.Status);
    }

    #endregion

    #region Delete payment

    [Fact]
    public async Task DeletePayment_OnAPaidInvoice_ReopensTheInvoice()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        var paymentRepo = Substitute.For<ITenantRepository<Payment, Guid>>();
        var invoiceRepo = Substitute.For<ITenantRepository<Invoice, Guid>>();
        tenantUow.Repository<Payment>().Returns(paymentRepo);
        tenantUow.Repository<Invoice>().Returns(invoiceRepo);

        var invoice = Invoice(Customer(null));
        var payment = Payment(1000m);
        invoice.ApplyPayment(payment);
        paymentRepo.GetByIdAsync(payment.Id, Arg.Any<CancellationToken>()).Returns(payment);
        invoiceRepo.GetAsync(Arg.Any<Expression<Func<Invoice, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(invoice);
        var sut = new DeletePaymentHandler(tenantUow);

        var result = await sut.Handle(new DeletePaymentCommand { Id = payment.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(InvoiceStatus.Issued, invoice.Status);
        Assert.Empty(invoice.Payments);
        paymentRepo.Received(1).Delete(payment);
    }

    #endregion

    private static Payment Payment(decimal amount) => new()
    {
        Amount = new Money { Amount = amount, Currency = "USD" },
        Status = PaymentStatus.Paid,
        TenantId = Guid.NewGuid(),
        BillingAddress = FactoringTestData.Address
    };
}
