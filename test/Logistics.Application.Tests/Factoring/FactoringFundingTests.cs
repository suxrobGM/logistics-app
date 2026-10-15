using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Xunit;
using static Logistics.Application.Tests.Factoring.FactoringTestData;

namespace Logistics.Application.Tests.Factoring;

public class FactoringFundingTests
{
    private static readonly Guid TenantId = Guid.NewGuid();

    #region Rates

    [Fact]
    public void EstimateAmounts_AdvanceAndFeeRates_ReserveIsTheRemainder()
    {
        var amounts = Company(advance: 90m, fee: 3m).EstimateAmounts(1234.56m);

        Assert.Equal(1111.10m, amounts.Advance);
        Assert.Equal(37.04m, amounts.Fee);
        Assert.Equal(86.42m, amounts.Reserve);
        Assert.Equal(1234.56m, amounts.Advance + amounts.Fee + amounts.Reserve);
    }

    [Fact]
    public void EstimateAmounts_NoReserve_ReserveIsZero()
    {
        var amounts = Company(advance: 97m, fee: 3m).EstimateAmounts(2000m);

        Assert.Equal(1940m, amounts.Advance);
        Assert.Equal(60m, amounts.Fee);
        Assert.Equal(0m, amounts.Reserve);
    }

    [Fact]
    public void ReservePercent_IsWhatAdvanceAndFeeLeave()
    {
        Assert.Equal(7m, Company(advance: 90m, fee: 3m).ReservePercent);
    }

    [Fact]
    public void FromAdvanceAndFee_MoreThanTheInvoice_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => FactoringAmounts.FromAdvanceAndFee(1000m, 980m, 30m));
    }

    #endregion

    #region Funding

    [Fact]
    public void Fund_RecordsAmountsAndSettlesTheInvoiceInFull()
    {
        var (batch, item, invoice) = SubmittedBatch(Company());

        var payment = item.Fund(900m, 30m, DateTime.UtcNow, TenantId, Address);
        batch.RefreshStatus();

        Assert.Equal(FactoringStatus.Funded, item.Status);
        Assert.Equal(900m, item.AdvanceAmount);
        Assert.Equal(30m, item.FeeAmount);
        Assert.Equal(70m, item.ReserveAmount);
        Assert.Equal(70m, item.ReserveOutstanding);
        Assert.Equal(1000m, payment.Amount.Amount);
        Assert.Equal(payment.Id, item.PaymentId);
        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
        Assert.Equal(FactoringStatus.Funded, invoice.FactoringStatus);
        Assert.Equal(FactoringBatchStatus.Funded, batch.Status);
    }

    [Fact]
    public void Fund_AlreadyFunded_Throws()
    {
        var (_, item, _) = SubmittedBatch(Company());
        item.Fund(900m, 30m, DateTime.UtcNow, TenantId, Address);

        Assert.Throws<InvalidOperationException>(() => item.Fund(900m, 30m, DateTime.UtcNow, TenantId, Address));
    }

    [Fact]
    public void ReleaseReserve_PartialThenRest_LeavesNothingOutstanding()
    {
        var (_, item, _) = SubmittedBatch(Company());
        item.Fund(900m, 30m, DateTime.UtcNow, TenantId, Address);

        item.ReleaseReserve(50m, DateTime.UtcNow);
        item.ReleaseReserve(20m, DateTime.UtcNow);

        Assert.Equal(70m, item.ReserveReleasedAmount);
        Assert.Equal(0m, item.ReserveOutstanding);
    }

    [Fact]
    public void ReleaseReserve_MoreThanHeld_Throws()
    {
        var (_, item, _) = SubmittedBatch(Company());
        item.Fund(900m, 30m, DateTime.UtcNow, TenantId, Address);

        Assert.Throws<InvalidOperationException>(() => item.ReleaseReserve(71m, DateTime.UtcNow));
    }

    [Fact]
    public void RefreshStatus_OneFundedOneRejected_PartiallyFunded()
    {
        var factor = Company();
        var customer = Customer(factor);
        var batch = FactoringBatch.Submit(
            factor, [Invoice(customer, number: 1), Invoice(customer, number: 2)], DateTime.UtcNow, null);

        batch.Items[0].Fund(900m, 30m, DateTime.UtcNow, TenantId, Address);
        batch.RefreshStatus();
        Assert.Equal(FactoringBatchStatus.PartiallyFunded, batch.Status);

        batch.Items[1].Reject("Debtor over credit limit");
        batch.RefreshStatus();
        Assert.Equal(FactoringBatchStatus.PartiallyFunded, batch.Status);
        Assert.Equal(FactoringStatus.Rejected, batch.Items[1].Invoice.FactoringStatus);
    }

    [Fact]
    public void RefreshStatus_AllRejected_Rejected()
    {
        var (batch, item, _) = SubmittedBatch(Company());

        item.Reject(null);
        batch.RefreshStatus();

        Assert.Equal(FactoringBatchStatus.Rejected, batch.Status);
    }

    #endregion

    #region Chargeback

    [Fact]
    public void ChargeBack_PutsTheInvoiceBackInTheCarriersReceivables()
    {
        var (batch, item, invoice) = SubmittedBatch(Company());
        invoice.SentAt = DateTime.UtcNow.AddDays(-5);
        invoice.DueDate = DateTime.UtcNow.AddDays(10);
        var sale = item.Fund(900m, 30m, DateTime.UtcNow, TenantId, Address);

        var removed = item.ChargeBack(DateTime.UtcNow);
        batch.RefreshStatus();

        Assert.Same(sale, removed);
        Assert.Empty(invoice.Payments);
        Assert.Equal(InvoiceStatus.Sent, invoice.Status);
        Assert.Equal(FactoringStatus.ChargedBack, invoice.FactoringStatus);
        Assert.Equal(FactoringStatus.ChargedBack, item.Status);
        Assert.Equal(0m, item.ReserveOutstanding);
        Assert.Null(item.PaymentId);
        Assert.Equal(FactoringBatchStatus.Funded, batch.Status);
    }

    [Fact]
    public void ChargeBack_PastDue_InvoiceIsOverdue()
    {
        var (_, item, invoice) = SubmittedBatch(Company());
        invoice.DueDate = DateTime.UtcNow.AddDays(-1);
        item.Fund(900m, 30m, DateTime.UtcNow.AddDays(-20), TenantId, Address);

        item.ChargeBack(DateTime.UtcNow);

        Assert.Equal(InvoiceStatus.Overdue, invoice.Status);
    }

    [Fact]
    public void ChargeBack_NotFunded_Throws()
    {
        var (_, item, _) = SubmittedBatch(Company());

        Assert.Throws<InvalidOperationException>(() => item.ChargeBack(DateTime.UtcNow));
    }

    #endregion
}
