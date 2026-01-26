using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class RecordManualPaymentHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService,
    ILogger<RecordManualPaymentHandler> logger)
    : IAppRequestHandler<RecordManualPaymentCommand, Result>
{
    public async Task<Result> Handle(RecordManualPaymentCommand req, CancellationToken ct)
    {
        var currentUserId = currentUserService.GetUserId();
        if (currentUserId is null)
        {
            return Result.Fail("User not authenticated.");
        }

        // Validate payment type is Cash or Check
        if (req.Type != PaymentMethodType.Cash && req.Type != PaymentMethodType.Check)
        {
            return Result.Fail("Manual payments can only be Cash or Check.");
        }

        // Get the invoice
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(req.InvoiceId, ct);
        if (invoice is null)
        {
            return Result.Fail("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result.Fail("This invoice has already been paid.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result.Fail("This invoice has been cancelled.");
        }

        // Validate amount
        if (req.Amount <= 0)
        {
            return Result.Fail("Payment amount must be greater than zero.");
        }

        // Calculate amount due
        var totalPaid = invoice.Payments.Sum(p => p.Amount.Amount);
        var amountDue = invoice.Total.Amount - totalPaid;

        if (req.Amount > amountDue)
        {
            return Result.Fail($"Payment amount cannot exceed amount due ({amountDue:C}).");
        }

        var tenant = tenantUow.GetCurrentTenant();

        // Create the payment entity
        var payment = new Payment
        {
            Amount = new Money { Amount = req.Amount, Currency = invoice.Total.Currency },
            Status = PaymentStatus.Paid, // Manual payments are immediately considered paid
            MethodId = Guid.Empty, // No stored method for manual payments
            TenantId = tenant.Id,
            Description = req.Notes ?? $"Manual {req.Type} payment for Invoice #{invoice.Number}",
            BillingAddress = tenant.CompanyAddress ?? new Address
            {
                Line1 = "Unknown",
                City = "Unknown",
                State = "Unknown",
                ZipCode = "00000",
                Country = "US"
            },
            ReferenceNumber = req.ReferenceNumber,
            RecordedByUserId = currentUserId.Value,
            RecordedAt = req.ReceivedDate ?? DateTime.UtcNow
        };

        // Save the payment
        await tenantUow.Repository<Payment>().AddAsync(payment, ct);

        // Apply to invoice (this updates status automatically)
        // Use event-raising method for PayrollInvoice to send notifications
        if (invoice is PayrollInvoice payrollInvoice)
        {
            payrollInvoice.ApplyPaymentWithEvent(payment);
        }
        else
        {
            invoice.ApplyPayment(payment);
        }

        tenantUow.Repository<Invoice>().Update(invoice);

        await tenantUow.SaveChangesAsync(ct);

        logger.LogInformation(
            "Recorded manual {PaymentType} payment of {Amount} for invoice {InvoiceId} by user {UserId}",
            req.Type, req.Amount, invoice.Id, currentUserId.Value);

        return Result.Ok();
    }
}
