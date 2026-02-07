using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Stripe;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;

namespace Logistics.Application.Commands;

internal sealed class ProcessPaymentHandler(
    ITenantUnitOfWork tenantUow,
    IStripePaymentService stripePaymentService,
    ILogger<ProcessPaymentHandler> logger)
    : IAppRequestHandler<ProcessPaymentCommand, Result>
{
    public async Task<Result> Handle(
        ProcessPaymentCommand req, CancellationToken ct)
    {
        var payment = await tenantUow.Repository<Payment>().GetByIdAsync(req.PaymentId, ct);

        if (payment is null)
        {
            return Result.Fail($"Could not find a payment with ID '{req.PaymentId}'");
        }

        var paymentMethod = await tenantUow.Repository<PaymentMethod>().GetByIdAsync(payment.MethodId, ct);

        if (paymentMethod is null)
        {
            return Result.Fail($"Could not find payment method with ID '{payment.MethodId}'");
        }

        if (string.IsNullOrEmpty(paymentMethod.StripePaymentMethodId))
        {
            return Result.Fail("Payment method is not synced with Stripe");
        }

        var tenant = tenantUow.GetCurrentTenant();

        try
        {
            var paymentIntent = await stripePaymentService.CreatePaymentIntentAsync(payment, paymentMethod, tenant);

            payment.StripePaymentIntentId = paymentIntent.Id;

            payment.Status = paymentIntent.Status switch
            {
                "succeeded" => PaymentStatus.Paid,
                "processing" => PaymentStatus.Pending,
                "requires_action" or "requires_confirmation" => PaymentStatus.Pending,
                "canceled" => PaymentStatus.Cancelled,
                _ => PaymentStatus.Failed
            };

            tenantUow.Repository<Payment>().Update(payment);
            await tenantUow.SaveChangesAsync(ct);

            if (payment.Status == PaymentStatus.Failed)
            {
                return Result.Fail($"Payment failed: {paymentIntent.LastPaymentError?.Message ?? "Unknown error"}");
            }

            logger.LogInformation(
                "Processed payment {PaymentId} with Stripe PaymentIntent {PaymentIntentId}, status: {Status}",
                payment.Id, paymentIntent.Id, payment.Status);

            return Result.Ok();
        }
        catch (StripeException ex)
        {
            payment.Status = PaymentStatus.Failed;
            tenantUow.Repository<Payment>().Update(payment);
            await tenantUow.SaveChangesAsync(ct);

            logger.LogError(ex, "Stripe error processing payment {PaymentId}: {Message}",
                payment.Id, ex.Message);
            return Result.Fail($"Payment processing failed: {ex.Message}");
        }
    }
}
