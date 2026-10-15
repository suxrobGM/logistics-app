using Logistics.Domain.Core;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Domain.Entities;

/// <summary>
/// One invoice on a schedule of accounts and what the factor did with it. Rejected and charged-back
/// invoices keep their row, so a schedule's history survives a resubmission.
/// </summary>
public class FactoringBatchItem : AuditableEntity, ITenantEntity
{
    public required Guid BatchId { get; set; }
    public virtual FactoringBatch Batch { get; set; } = null!;

    public required Guid InvoiceId { get; set; }
    public virtual LoadInvoice Invoice { get; set; } = null!;

    /// <summary>
    /// Invoice total when it was submitted.
    /// </summary>
    public required Money Amount { get; set; }

    public FactoringStatus Status { get; set; } = FactoringStatus.Submitted;

    /// <summary>
    /// Cash the factor paid the carrier at funding.
    /// </summary>
    public decimal? AdvanceAmount { get; set; }

    public decimal? FeeAmount { get; set; }

    /// <summary>
    /// Held back by the factor until the customer pays it.
    /// </summary>
    public decimal? ReserveAmount { get; set; }

    public decimal ReserveReleasedAmount { get; set; }

    public DateTime? FundedAt { get; set; }
    public DateTime? ReserveReleasedAt { get; set; }
    public DateTime? ChargedBackAt { get; set; }

    public string? RejectionReason { get; set; }

    /// <summary>
    /// The payment that settled the invoice in the carrier's books when the factor bought it.
    /// </summary>
    public Guid? PaymentId { get; set; }

    /// <summary>
    /// Reserve the factor still owes the carrier on a funded invoice.
    /// </summary>
    public decimal ReserveOutstanding =>
        Status == FactoringStatus.Funded ? (ReserveAmount ?? 0m) - ReserveReleasedAmount : 0m;

    /// <summary>
    /// Record the factor buying the invoice. The sale settles the customer's balance with the carrier, so
    /// the invoice is paid in full by a payment for its total; the factor's advance, fee and reserve are
    /// kept here. Add the returned payment with the repository.
    /// </summary>
    public Payment Fund(decimal advance, decimal fee, DateTime fundedAt, Guid tenantId, Address billingAddress)
    {
        EnsureStatus(FactoringStatus.Submitted, "funded");
        var amounts = FactoringAmounts.FromAdvanceAndFee(Amount.Amount, advance, fee);

        var payment = new Payment
        {
            Amount = Amount,
            Status = PaymentStatus.Paid,
            TenantId = tenantId,
            BillingAddress = billingAddress,
            Description = $"Invoice #{Invoice.Number} sold to factoring company (schedule #{Batch.Number})",
            ReferenceNumber = $"SOA-{Batch.Number}",
            RecordedAt = fundedAt
        };
        Invoice.ApplyPayment(payment);
        Invoice.FactoringStatus = FactoringStatus.Funded;

        Status = FactoringStatus.Funded;
        AdvanceAmount = amounts.Advance;
        FeeAmount = amounts.Fee;
        ReserveAmount = amounts.Reserve;
        FundedAt = fundedAt;
        PaymentId = payment.Id;
        return payment;
    }

    public void Reject(string? reason)
    {
        EnsureStatus(FactoringStatus.Submitted, "rejected");
        Status = FactoringStatus.Rejected;
        RejectionReason = reason;
        Invoice.FactoringStatus = FactoringStatus.Rejected;
    }

    /// <summary>
    /// Record reserve the factor paid out once the customer paid it. Partial releases add up.
    /// </summary>
    public void ReleaseReserve(decimal amount, DateTime releasedAt)
    {
        EnsureStatus(FactoringStatus.Funded, "have its reserve released");
        if (amount <= 0m || amount > ReserveOutstanding)
        {
            throw new InvalidOperationException(
                $"The released amount must be more than 0 and at most the outstanding reserve ({ReserveOutstanding:0.00}).");
        }

        ReserveReleasedAmount += amount;
        ReserveReleasedAt = releasedAt;
    }

    /// <summary>
    /// The factor sold the invoice back because the customer did not pay it (recourse). The sale payment
    /// is reversed, so the customer owes the carrier again. Returns that payment for the caller to delete.
    /// </summary>
    public Payment? ChargeBack(DateTime chargedBackAt)
    {
        EnsureStatus(FactoringStatus.Funded, "charged back");

        var payment = Invoice.Payments.FirstOrDefault(p => p.Id == PaymentId);
        if (payment is not null)
        {
            Invoice.Payments.Remove(payment);
        }

        var paid = Invoice.Payments.Sum(p => p.Amount.Amount);
        if (paid > 0m)
        {
            Invoice.Status = InvoiceStatus.PartiallyPaid;
        }
        else if (Invoice.DueDate < chargedBackAt)
        {
            Invoice.Status = InvoiceStatus.Overdue;
        }
        else
        {
            Invoice.Status = Invoice.SentAt is not null ? InvoiceStatus.Sent : InvoiceStatus.Issued;
        }

        Invoice.FactoringStatus = FactoringStatus.ChargedBack;
        Status = FactoringStatus.ChargedBack;
        ChargedBackAt = chargedBackAt;
        PaymentId = null;
        return payment;
    }

    private void EnsureStatus(FactoringStatus expected, string action)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Invoice #{Invoice.Number} is {Status.GetDescription().ToLowerInvariant()} and cannot be {action}.");
        }
    }
}
