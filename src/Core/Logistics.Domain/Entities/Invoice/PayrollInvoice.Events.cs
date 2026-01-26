using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Domain event methods for PayrollInvoice entity.
/// </summary>
public partial class PayrollInvoice
{
    /// <summary>
    /// Submits the payroll invoice for approval and raises PayrollSubmittedEvent.
    /// </summary>
    public void SubmitForApproval()
    {
        Status = InvoiceStatus.PendingApproval;
        RaiseSubmittedEvent();
    }

    /// <summary>
    /// Approves the payroll invoice and raises PayrollApprovedEvent.
    /// </summary>
    public void Approve(Guid approvedById, string? notes = null)
    {
        Status = InvoiceStatus.Approved;
        ApprovedById = approvedById;
        ApprovedAt = DateTime.UtcNow;
        ApprovalNotes = notes;
        RaiseApprovedEvent(approvedById);
    }

    /// <summary>
    /// Rejects the payroll invoice and raises PayrollRejectedEvent.
    /// </summary>
    public void Reject(Guid rejectedById, string reason)
    {
        Status = InvoiceStatus.Rejected;
        RejectionReason = reason;
        RaiseRejectedEvent(rejectedById, reason);
    }

    /// <summary>
    /// Applies a payment and raises PayrollPaidEvent.
    /// </summary>
    public void ApplyPaymentWithEvent(Payment payment)
    {
        Payments.Add(payment);
        var paid = Payments.Sum(p => p.Amount);
        var isFullyPaid = paid >= Total.Amount;
        Status = isFullyPaid ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;

        RaisePaidEvent(payment.Amount.Amount, isFullyPaid);
    }

    private void RaiseSubmittedEvent()
    {
        DomainEvents.Add(new PayrollSubmittedEvent(
            Id,
            Number,
            EmployeeId,
            Employee?.GetFullName() ?? "Unknown",
            Employee?.Email ?? string.Empty,
            Total.Amount,
            Total.Currency,
            PeriodStart,
            PeriodEnd));
    }

    private void RaiseApprovedEvent(Guid approvedById)
    {
        DomainEvents.Add(new PayrollApprovedEvent(
            Id,
            Number,
            EmployeeId,
            Employee?.GetFullName() ?? "Unknown",
            Employee?.Email ?? string.Empty,
            Employee?.DeviceToken,
            Total.Amount,
            Total.Currency,
            PeriodStart,
            PeriodEnd,
            approvedById));
    }

    private void RaiseRejectedEvent(Guid rejectedById, string reason)
    {
        DomainEvents.Add(new PayrollRejectedEvent(
            Id,
            Number,
            EmployeeId,
            Employee?.GetFullName() ?? "Unknown",
            Employee?.Email ?? string.Empty,
            Employee?.DeviceToken,
            reason,
            PeriodStart,
            PeriodEnd,
            rejectedById));
    }

    private void RaisePaidEvent(decimal paymentAmount, bool isFullyPaid)
    {
        var totalPaid = Payments.Sum(p => p.Amount);
        var outstanding = Total.Amount - totalPaid;

        DomainEvents.Add(new PayrollPaidEvent(
            Id,
            Number,
            EmployeeId,
            Employee?.GetFullName() ?? "Unknown",
            Employee?.Email ?? string.Empty,
            Employee?.DeviceToken,
            paymentAmount,
            Total.Amount,
            outstanding,
            Total.Currency,
            isFullyPaid,
            PeriodStart,
            PeriodEnd));
    }
}
