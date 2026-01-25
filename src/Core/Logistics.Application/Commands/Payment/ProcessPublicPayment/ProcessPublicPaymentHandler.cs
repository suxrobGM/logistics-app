using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Commands;

internal sealed class ProcessPublicPaymentHandler(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    IStripeConnectService stripeConnectService,
    ILogger<ProcessPublicPaymentHandler> logger)
    : IAppRequestHandler<ProcessPublicPaymentCommand, Result<ProcessPublicPaymentResult>>
{
    public async Task<Result<ProcessPublicPaymentResult>> Handle(ProcessPublicPaymentCommand req, CancellationToken ct)
    {
        // Get the tenant
        var tenant = await masterUow.Repository<Tenant>().GetByIdAsync(req.TenantId, ct);
        if (tenant is null)
        {
            return Result<ProcessPublicPaymentResult>.Fail("Invalid payment link.");
        }

        // Check if tenant has Stripe Connect enabled
        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            return Result<ProcessPublicPaymentResult>.Fail("This company is not set up to receive payments.");
        }

        if (!tenant.ChargesEnabled)
        {
            return Result<ProcessPublicPaymentResult>.Fail("This company cannot currently accept payments. Please contact them directly.");
        }

        // Switch to tenant database
        tenantUow.SetCurrentTenant(tenant);

        // Find the payment link by token
        var paymentLink = await tenantUow.Repository<PaymentLink>()
            .GetAsync(p => p.Token == req.Token, ct);

        if (paymentLink is null || !paymentLink.IsValid)
        {
            return Result<ProcessPublicPaymentResult>.Fail("This payment link has expired or been revoked.");
        }

        // Get the invoice
        var invoice = await tenantUow.Repository<Invoice>().GetByIdAsync(paymentLink.InvoiceId, ct);
        if (invoice is null)
        {
            return Result<ProcessPublicPaymentResult>.Fail("Invoice not found.");
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            return Result<ProcessPublicPaymentResult>.Fail("This invoice has already been paid.");
        }

        if (invoice.Status == InvoiceStatus.Cancelled)
        {
            return Result<ProcessPublicPaymentResult>.Fail("This invoice has been cancelled.");
        }

        // Calculate amount due
        var totalPaid = invoice.Payments.Sum(p => p.Amount.Amount);
        var amountDue = invoice.Total.Amount - totalPaid;

        // Determine payment amount
        var paymentAmount = req.Amount ?? amountDue;
        if (paymentAmount <= 0)
        {
            return Result<ProcessPublicPaymentResult>.Fail("Payment amount must be greater than zero.");
        }

        if (paymentAmount > amountDue)
        {
            return Result<ProcessPublicPaymentResult>.Fail($"Payment amount cannot exceed amount due ({amountDue:C}).");
        }

        try
        {
            // Create a Payment entity
            var payment = new Payment
            {
                Amount = new Money { Amount = paymentAmount, Currency = invoice.Total.Currency },
                Status = PaymentStatus.Pending,
                MethodId = Guid.Empty, // Public payment doesn't have a stored method
                TenantId = tenant.Id,
                Description = $"Payment for Invoice #{invoice.Number}",
                BillingAddress = tenant.CompanyAddress // Use tenant's address as placeholder
            };

            // Create PaymentIntent with destination charges
            var paymentIntent = await stripeConnectService.CreateConnectedPaymentIntentAsync(
                payment,
                tenant.StripeConnectedAccountId,
                applicationFeePercent: 0); // Configurable platform fee (currently 0%)

            // Update payment with Stripe details
            payment.StripePaymentIntentId = paymentIntent.Id;
            payment.Status = paymentIntent.Status switch
            {
                "succeeded" => PaymentStatus.Paid,
                "processing" => PaymentStatus.Pending,
                "requires_action" or "requires_confirmation" or "requires_payment_method" => PaymentStatus.Pending,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Pending
            };

            // Save the payment
            await tenantUow.Repository<Payment>().AddAsync(payment, ct);

            // If payment succeeded, apply to invoice
            if (payment.Status == PaymentStatus.Paid)
            {
                invoice.ApplyPayment(payment);
                tenantUow.Repository<Invoice>().Update(invoice);
            }

            await tenantUow.SaveChangesAsync(ct);

            logger.LogInformation(
                "Processed public payment for invoice {InvoiceId}, PaymentIntent {PaymentIntentId}, Status {Status}",
                invoice.Id, paymentIntent.Id, paymentIntent.Status);

            return Result<ProcessPublicPaymentResult>.Ok(new ProcessPublicPaymentResult
            {
                PaymentIntentId = paymentIntent.Id,
                ClientSecret = paymentIntent.ClientSecret,
                Status = paymentIntent.Status
            });
        }
        catch (Stripe.StripeException ex)
        {
            logger.LogError(ex, "Failed to process public payment for invoice {InvoiceId}", invoice.Id);
            return Result<ProcessPublicPaymentResult>.Fail($"Payment processing failed: {ex.Message}");
        }
    }
}
